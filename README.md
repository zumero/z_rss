z_rss
=====

This code is published largely for the purpose of being read.  It accompanies
a series of blog articles, the first of which is:

    http://www.ericsink.com/entries/rss_cat_1.html

If you want to
actually compile/run this code, here are some tips:

This code was built on a Mac.  If you want to run it somewhere else, all
references to dylib and clang probably need to be tweaked.

The C# code was built with Mono 3.0.6.  I have no idea if it'll work with
other versions of Mono/Xamarin stuff.

The build script expects the sqlite-net repo to be a sibling directory.
In other words, you probably want to do something like this:

    mkdir work
    cd work
    git clone https://github.com/zumero/z_rss.git
    git clone https://github.com/praeclarum/sqlite-net.git

You need to get the Zumero Client SDK and copy zumero.dylib into this directory.

    http://zumero.com/dev-center/

zinst393e9343b87.s.zumero.net is my Zumero server for this project.
You'll need to get your own so that you these scripts can have admin access.

