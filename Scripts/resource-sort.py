#!/usr/bin/env python2

import sys

def main(filename):
    f = open(filename, "rb")
    data = f.read()
    f.close()

    f = open(filename, "wb")
    lines = data.split("\r\n")

    for l in lines:
        if not l.strip():
            continue

        columns = l.split("|")
        items = columns[2].split("\n")

        def sort_key(x):
            sp = x.split(": ")
            if len(sp) == 1:
                return 0
            else return int(sp[-1])

        items = sorted(items, key=sort_key), reverse=True)

        s_items = "\n".join(items)
        columns[2] = s_items
        s_columns = "|".join(columns)

        f.write(s_columns + "\r\n")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Fatal: No Input")
        exit(-1)
    main(sys.argv[1])
