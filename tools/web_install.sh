#!/bin/bash

# this file will be uploaded to sourceforge.net every time new website is deployed

HTML_DIR=$1

umask 002
echo Installing web.zip...
rm -rf $HTML_DIR/*
unzip -o -d $HTML_DIR web.zip
rm -f web.zip
rm -f web_install.sh
exit 0
