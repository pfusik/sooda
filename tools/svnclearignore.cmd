svn status --no-ignore %* | grep "^I" | cut -b 8- | xargs rm -vrf
