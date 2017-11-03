using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using UnityEngine;

namespace SceneProfiler {

public class UdpSignal {
    public enum BuiltInPorts {
        PivotPort = 51984,
        ParserPort = 51985,
    }

    int port_;

    UdpClient uc_;
    IAsyncResult rec_;

    public delegate void signalHandler(string sender, string signal);
    public event signalHandler onSignal = delegate { };

    public bool receiving {
        get { return rec_ != null; }
    }

    public UdpSignal(BuiltInPorts port, bool receive = true) :
        this((int) port, receive) { }

    public UdpSignal(int port, bool receive = true) {
        port_ = port;
        uc_ = new UdpClient();

        try {
            if(receive) {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, port_);
                uc_.Client.ExclusiveAddressUse = false;
                uc_.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                uc_.Client.Bind(ep);
                rec_ = uc_.BeginReceive(new AsyncCallback(receiveCallback), null);
            }
        } catch(Exception e) {
            Debug.Log(e);
        }
    }

    public void signal(string receiver, string s) {
        try {
            var addr = receiver.Split(':');
            int port = addr.Length > 1 ? int.Parse(addr[1]) : port_;

            byte[] bytes = Encoding.UTF8.GetBytes(s);
            uc_.Send(bytes, bytes.Length, addr[0], port);
            Debug.Log("Signaled at " + addr[0] + ":" + port);
        } catch(Exception e) {
            Debug.Log(e);
        }
    }

    void receiveCallback(IAsyncResult ar) {
        try {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port_);
            byte[] b = uc_.EndReceive(rec_, ref ep);
            Debug.Log("Got Signaled by " + ep.Address.ToString() + ":" + ep.Port);

            if(b.Length > 0)
                onSignal(ep.Address.ToString(), Encoding.UTF8.GetString(b, 0, b.Length));

            rec_ = uc_.BeginReceive(new AsyncCallback(receiveCallback), null);
        } catch(Exception e) {
            Debug.Log(e);
        }
    }
}

}
