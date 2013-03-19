
.load zumero.dylib

.echo ON

SELECT zumero_sync(
    'main',
    'https://zinst393e9343b87.s.zumero.net',
    'all_feeds'
    );

SELECT * FROM feeds;

