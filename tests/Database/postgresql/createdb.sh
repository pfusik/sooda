#!/bin/bash
createdb SoodaUnitTests
psql -d SoodaUnitTests -f createdb.sql

