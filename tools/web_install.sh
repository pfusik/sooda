#!/bin/bash

# this file will be uploaded to sourceforge.net every time 
# new website is deployed

HTML_DIR=$1

umask 002
echo Installing web.zip...
# rm -rf $HTML_DIR/en
# rm -rf $HTML_DIR/pl
# rm -rf $HTML_DIR/samples
# rm -rf $HTML_DIR/screenshots
# all files but no dirs
# rm -f $HTML_DIR/* 
unzip -o -d $HTML_DIR web.zip
cp $HTML_DIR/redirector.html $HTML_DIR/en/blog.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/components.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/development.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/documentation.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/download.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/examples.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/faq.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/index.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/links.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/mailinglist.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/screenshots.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/status.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/technical-cdil.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/technical-config.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/technical.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/technical-logging.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/tools.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/tutorial.html
cp $HTML_DIR/redirector.html $HTML_DIR/en/users.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/blog.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/components.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/development.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/documentation.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/download.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/examples.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/faq.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/index.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/links.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/mailinglist.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/screenshots.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/status.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/technical-cdil.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/technical-config.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/technical.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/technical-logging.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/tools.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/tutorial.html
cp $HTML_DIR/redirector.html $HTML_DIR/pl/users.html
cp $HTML_DIR/introduction.html $HTML_DIR/index.html
cp $HTML_DIR/redirector.php $HTML_DIR/en/index.php
cp $HTML_DIR/redirector.php $HTML_DIR/pl/index.php
rm -f $HTML_DIR/en/index.html
rm -f $HTML_DIR/pl/index.html
rm -f $HTML_DIR/redirector.php
rm -f $HTML_DIR/redirector.htm
rm -f web.zip
rm -f web_install.sh
exit 0
