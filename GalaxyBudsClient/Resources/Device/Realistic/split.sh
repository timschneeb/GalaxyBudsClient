#!/bin/sh

# This script splits an image of the earbuds from the official app into two parts: left and right.
convert -crop 50%x100% +repage $1 $1
