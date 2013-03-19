z_rss
=====

This code is published largely for the purpose of being read.  If you want to
actually compile/run this code, here are some tips:

This code was built on a Mac.  If you want to run it somewhere else, all
references to dylib and clang probably need to be tweaked.

This C# code was built with Mono 3.0.6.  I have no idea if it'll work with
other versions of Mono/Xamarin stuff.

The build script expects the sqlite-net repo to be a sibling directory.

    https://github.com/praeclarum/sqlite-net

You need to get the Zumero Client SDK and copy zumero.dylib into this directory.

https://zinst393e9343b87.s.zumero.net is my Zumero server for this project.
You'll need to get your own so that you these scripts can have admin access.

