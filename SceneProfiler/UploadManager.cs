#define ENABLE_PROFILER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Threading;

using UnityEngine;

using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace SceneProfiler {

class UploadManager {
    private float upload_progress_ = -1;
    private float upload_progress_step_;
    public float uploadProgress {
        get {
            if(upload_thread == null || !upload_thread.IsAlive)
                upload_progress_ = -1;
            return upload_progress_;
        }
    }

    public delegate void progressDelegate(float progress);

    public string remote_base_directory_;
    public string remoteBaseDirectory {
        get { return remote_base_directory_; }
        set {
            assertIdle();

            if(!Regex.IsMatch(value, URI_REGEX))
                throw new InvalidOperationException("Bad FTP address.");
            if(value.Length >= 1 && value.Last() == '/')
                value = value.Substring(0, value.Length - 1);
            remote_base_directory_ = value;
        }
    }

    public string local_base_directory_;
    public string localBaseDirectory {
        get { return local_base_directory_; }
        set { assertIdle(); local_base_directory_ = value; }
    }

    public string target_name_;
    public string targetName {
        get { return target_name_; }
        set { assertIdle(); target_name_ = value; }
    }

    public bool compress_ = true;
    public bool compress {
        get { return compress_; }
        set { assertIdle(); compress_ = value; }
    }

    Thread upload_thread;

    void assertIdle() {
        if(upload_thread != null && upload_thread.IsAlive)
            throw new InvalidOperationException("Previous upload still running");
    }

    public void startToUpload() {
        assertIdle();

        upload_progress_ = 0;

        upload_thread = new Thread(() => {
            try {
                if(compress)
                    uploadCompressDirectory();
                else
                    uploadDirectory();
            } catch(Exception e) {
                Debug.Log(e);
            }
        });
        upload_thread.Start();
    }

    public void uploadDirectory() {
        // in this function, rXX means remote properties
        string dn = Path.Combine(localBaseDirectory, targetName);
        string rdn = remoteBaseDirectory + "/" + targetName;

        upload(rdn, dn);

        string[] fs = Directory.GetFiles(dn);
        upload_progress_step_ = 100 / (fs.Length + 1);

        // reward for directory creation
        upload_progress_ = upload_progress_step_;

        for (int i = 0; i < fs.Length; i++) {
            string f = fs[i];
            string basename = Path.GetFileName(f);
            string rf = rdn + "/" + basename;

            upload(rf, f);
        }

        Directory.Delete(dn, true);

        notify(targetName);
    }

    public void uploadCompressDirectory() {
        string dn = Path.Combine(localBaseDirectory, targetName);
        string fn = Path.Combine(localBaseDirectory, targetName + ".zip");

        /*
        System.Diagnostics.ProcessStartInfo ci = new System.Diagnostics.ProcessStartInfo("tar",
            string.Format("-czf {0}.tgz {0}", targetName));
        ci.WorkingDirectory = localBaseDirectory;
        var process = System.Diagnostics.Process.Start(ci);
        process.WaitForExit();
        */
        using(var archive_stm = File.Create(fn))
        using(var archive = ZipArchive.Create()) {
            string[] files = Directory.GetFiles(dn);
            foreach(string arc_fn in files) {
                var f = File.OpenRead(arc_fn);
                archive.AddEntry(Path.GetFileName(arc_fn), f, true);
            }

            archive.SaveTo(archive_stm);
        }

        upload_progress_ = 1;
        upload_progress_step_ = 99;

        string rfn = remoteBaseDirectory + "/" + targetName + ".zip";
        upload(rfn, fn);

        File.Delete(fn);
        Directory.Delete(dn, true);

        notify(targetName + ".zip");
    }

    void notify(string name) {
        var mc = Regex.Match(remoteBaseDirectory, URI_REGEX);
        UdpSignal s = new UdpSignal(UdpSignal.BuiltInPorts.PivotPort, false);
        s.signal(mc.Groups[2].Value, "Uploaded:" + name);
    }

    static int BUFFER_SIZE = 512;
    static string URI_REGEX = @"^ftp://([^:]+:[^:@]+@)?([^/]+)(/.*)?$";

    /// <summary>
    /// When autoUpload is set true, this function will be invoked implicitly
    /// after profiling stops. On uploading finished.
    /// </summary>
    public void upload(string destination, string source) {
        //////////////////////////////////////////////////////////////////////
        // Parse the address
        var mc = Regex.Match(destination, URI_REGEX);

        string auth = mc.Groups[1].Value;
        string dest = "ftp://" + mc.Groups[2].Value + mc.Groups[3].Value;
        NetworkCredential nc = null;

        if(auth != "") {
            string[] up = auth.Split(':');
            nc = new NetworkCredential(up[0], up[1].Substring(0, up[1].Length - 1));
        }

        //////////////////////////////////////////////////////////////////////
        // Create connection
        Debug.Log("Uploading: " + source + " -> " + dest);
        float begin_prog = uploadProgress;

        if(Directory.Exists(source)) {
            FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(dest);
            if(nc != null) ftpRequest.Credentials = nc;
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            ftpRequest.GetResponse().Close();
        } else if(File.Exists(source)) {
            FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(dest);
            if(nc != null) ftpRequest.Credentials = nc;
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                //////////////////////////////////////////////////////////////////////
                // Upload starts
                Debug.Log("1`");
            Stream ftpStream = ftpRequest.GetRequestStream();
                Debug.Log("4");
            FileStream localFileStream = new FileStream(source, FileMode.Open);
                Debug.Log("2");
            byte[] byteBuffer = new byte[BUFFER_SIZE];
            int bytesSent = localFileStream.Read(byteBuffer, 0, BUFFER_SIZE);

            while (bytesSent != 0) {
                ftpStream.Write(byteBuffer, 0, bytesSent);
                bytesSent = localFileStream.Read(byteBuffer, 0, BUFFER_SIZE);

                upload_progress_ = begin_prog + upload_progress_step_ * localFileStream.Position / localFileStream.Length;
            }

            localFileStream.Close();
            ftpStream.Close();
        }
    }
}

}
