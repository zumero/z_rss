#!/bin/sh

set -x
curl -O http://www.sqlite.org/sqlite-amalgamation-3071502.zip
unzip sqlite-amalgamation-3071502.zip
cd sqlite-amalgamation-3071502
clang -DSQLITE_DEFAULT_FOREIGN_KEYS=1 -DSQLITE_ENABLE_FTS3_PARENTHESIS -DSQLITE_ENABLE_FTS4 -dynamiclib -arch i386 -arch x86_64 -o libsqlite03071502.dylib sqlite3.c
mv libsqlite03071502.dylib ..
clang -o sqlite3 -arch i386 -arch x86_64 sqlite3.c shell.c
mv sqlite3 ..

