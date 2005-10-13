#!/bin/bash

# this file will be uploaded to sourceforge.net every time 
# new website is deployed

HTML_DIR=/home/groups/s/so/sooda/htdocs

umask 002
echo Installing web.zip...
# rm -rf $HTML_DIR/en
# rm -rf $HTML_DIR/pl
# rm -rf $HTML_DIR/samples
# rm -rf $HTML_DIR/screenshots
# all files but no dirs
# rm -f $HTML_DIR/* 
unzip -o -d $HTML_DIR web.zip
rm -f web.zip
rm -f web_install.sh
exit 0
