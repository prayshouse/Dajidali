#!/usr/bin/env python2

import os
import sys
import signal
import socket
import re
import Queue
import thread
import zipfile
import requests
import json
import PIL.Image as Image

UPLOAD_DIRECTORY = r"E:\download\sProfiler"
PIVOT_PORT = 51984
PARSER_HOST = "127.0.0.1"
PARSER_PORT = 51985
SPROFILER_SERVER = "http://127.0.0.1:8000"

def uploaded(f, signal):
    print("Decompressing %s" % f)

    bn = os.path.splitext(f)[0]
    dn = os.path.join(UPLOAD_DIRECTORY, bn)
    fn = os.path.join(UPLOAD_DIRECTORY, f)

    with zipfile.ZipFile(fn, "r") as zf:
        if not os.path.isdir(dn):
            os.mkdir(dn)
        zf.extractall(dn)

    print("Sending for parsing: %s" % dn)
    signal.sendto(dn, (PARSER_HOST, PARSER_PORT))

def parsed(dn, signal):
    print("Converting images in %s" % dn)

    files = os.listdir(dn)
    for fn in files:
        full_fn = os.path.join(dn, fn)
        splitted_full_fn = os.path.splitext(full_fn)

        if os.path.isfile(full_fn) and splitted_full_fn[-1] == ".ppm":
            i = Image.open(full_fn)
            i = i.transpose(Image.FLIP_TOP_BOTTOM)
            i.save(splitted_full_fn[0] + ".png", "PNG")

    pngs = map(lambda x: re.match(r'^ScreenshotAtFrame(\d+).png$', x), os.listdir(dn))
    pngs = filter(lambda x: x, pngs)
    pngs = sorted(pngs, key=lambda x: int(x.group(1)))
    pngs = list(pngs)

    line_c = 0
    png_c = 0

    with open(os.path.join(dn, "fps.log"), "r") as fps_log:
        for l in fps_log:
            if not pngs: break
            if not l: continue

            line_c += 1

            frame_count = int(l.split('|')[0])
            if frame_count > int(pngs[png_c].group(1)):
                pngs[png_c] = (pngs[png_c], line_c)
                png_c += 1
                if png_c >= len(pngs): break

    for name_idx_ in pngs:
        if not isinstance(name_idx_, tuple) or len(name_idx_) != 2:
            continue
        name, idx = name_idx_
        screenshotImgName = os.path.join(dn, name.group(0))
        overdrawImgName = screenshotImgName.replace("Screenshot", "Overdraw")
        os.rename(screenshotImgName, os.path.join(dn, "ScreenshotAtFrame" + str(idx) + ".png"))
        os.rename(overdrawImgName, os.path.join(dn, "OverdrawAtFrame" + str(idx) + ".png"))

    print("Packing up data in %s" % dn)

    files = os.listdir(dn)
    zfn = dn + "-parsed.zip"
    zf = zipfile.ZipFile(zfn, "w")
    for fn in files:
        if not fn.endswith(".png") and not fn.endswith(".log"):
            continue
        if fn.startswith("raw-data"):
            continue

        full_fn = os.path.join(dn, fn)
        zf.write(full_fn, fn, zipfile.ZIP_DEFLATED)
    zf.close()

    base_dn = os.path.basename(dn)

    print("Transfering to sProfilerLog node %s" % base_dn)

    resp1 = requests.post(SPROFILER_SERVER + "/addIndex", data={
            "pid": 0, "name": base_dn, "isLeaf": "true"
        })
    resp1 = resp1.json()
    if "id" not in resp1:
        print("Unable to get leaf ID. Aborted.")
        return

    print("ID is " + resp1["id"])

    resp2 = requests.post(SPROFILER_SERVER + "/uploadFiles",
        data = {
            "project": "PM02",
            "leafId": resp1["id"],
            "scene": "Unknown",
            "remarks": ""
        }, files = {
            "logFiles": open(zfn, "rb"),
        })

    print("Uploading result is %s" % repr(resp2.text))

def worker(q):
    signal = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    while True:
        try:
            addr, data = q.get()

            print("[%s:%d] %s" % (addr[0], addr[1], data))

            match = re.match('Uploaded:\s*(.*)', data)
            if match:
                uploaded(match.group(1), signal)

            match = re.match('Parsed:\s*(.*)', data)
            if match:
                parsed(match.group(1), signal)
        except Exception, e:
            import traceback
            print(traceback.format_exc())

def main():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.settimeout(1)
    s.bind(('0.0.0.0', 51984))
    q = Queue.Queue()
    worker_t = thread.start_new_thread(worker, (q,))

    while True:
        try:
            data, addr = s.recvfrom(4096)
            q.put((addr, data))
        except socket.timeout, e:
            pass


if __name__ == "__main__":
    main()


