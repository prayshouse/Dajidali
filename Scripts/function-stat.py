#!/usr/bin/env python2

import sys
import os
import numpy

def extract_cpu_stack(f_cs):
    f_cs = open(f_cs, "r")

    cs_data = {}
    for l in f_cs:
        items = l.strip().split("|")
        frame_count = int(items[0])

        if frame_count not in cs_data:
            cs_data[frame_count] = {}

        cs_data[frame_count][items[1]] = items[2:]

    return cs_data

def extract_extra(f_extra):
    f_extra = open(f_extra, "r")

    extra_data = {}
    for l in f_extra:
        items = l.strip().split("|")
        frame_count = int(items[0])

        extra_dict = {}
        for i in items[1:]:
            k, v = tuple(i.split(":")[:2])
            extra_dict[k] = v
        extra_dict["FrameCount"] = frame_count

        extra_data[frame_count] = extra_dict

    return extra_data

def count_by(cs_data, extra_data, key, funcs):
    count_table = {}

    for frame_count, extra_dict in extra_data.items():
        if key not in extra_dict:
            continue
        value = int(extra_dict[key])

        if value not in count_table:
            count_table[value] = {}

        for f in funcs:
            if frame_count not in cs_data:
                continue
            if f not in cs_data[frame_count]:
                continue
            if f not in count_table[value]:
                count_table[value][f] = []
            count_table[value][f] += [float(cs_data[frame_count][f][2])]

    return count_table

def visualize_table(count_table, columns):
    keys = count_table.keys()
    keys = sorted(keys)

    table = []

    for k in keys:
        row = [k]
        for c in columns:
            if c not in count_table[k]:
                row += [ "N/A" ] * 4
            else:
                row += [
                    numpy.mean(count_table[k][c]),
                    numpy.max(count_table[k][c]),
                    numpy.min(count_table[k][c]),
                    numpy.var(count_table[k][c]),
                ]
        table += [row]

    columns = map(lambda x: x + " mean | max | min | var", columns)

    print(" | ".join(["\\#"] + columns))
    print(" | ".join(["--"] * (1 + len(columns) * 4)))

    for row in table:
        print(" | ".join(map(lambda x: str(x), row)))

def main(d, key, funcs):
    f_cs = os.path.join(d, "cpu_stack.log")
    f_extra = os.path.join(d, "extra.log")

    if not os.path.isfile(f_cs) and not os.path.isfile(f_extra):
        raise IOError("%s and %s not found" % (f_cs, f_extra))

    cs_data = extract_cpu_stack(f_cs)
    extra_data = extract_extra(f_extra)

    count_table = count_by(cs_data, extra_data, key, funcs)
    visualize_table(count_table, funcs)

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("USAGE: function-stat.py <Directory> <Key> [<Functions>]")
        exit(-1)
    main(sys.argv[1], sys.argv[2], sys.argv[3:])


