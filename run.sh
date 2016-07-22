#!/bin/sh

FORTH=${FORTH:-forth}

$FORTH \
  util.fs \
  term.fs \
  random.fs \
  core.fs \
  mem.fs \
  header.fs \
  strings.fs \
  objects.fs \
  cpu/util.fs \
  quetzal.fs \
  cpu/0op.fs \
  cpu/1op.fs \
  cpu/2op.fs \
  cpu/var.fs \
  cpu/ext.fs \
  cpu.fs \
  main.fs
