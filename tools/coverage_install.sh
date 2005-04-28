#!/bin/bash

# this file will be uploaded to sourceforge.net every time 
# new website is deployed

HTML_DIR=/home/groups/s/so/sooda/htdocs/clover

umask 002
echo Installing coverage.zip...
rm -f $HTML_DIR/* 
unzip -o -d $HTML_DIR coverage.zip
rm -f coverage.zip
rm -f coverage_install.sh
exit 0
