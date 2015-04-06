#!/bin/sh

GFORTH=${GFORTH:-gforth-itc}

$GFORTH \
  util.fs \
  random.fs \
  core.fs \
  mem.fs \
  header.fs \
  strings.fs \
  objects.fs \
  cpu/util.fs \
  cpu/0op.fs \
  cpu/1op.fs \
  cpu/2op.fs \
  cpu/var.fs \
  cpu/ext.fs \
  cpu.fs \
  main.fs
