
.load zumero.dylib

.echo ON

CREATE VIRTUAL TABLE IF NOT EXISTS last_update USING zumero(
    feedid INTEGER UNIQUE NOT NULL REFERENCES feeds (feedid),
    when_unix_time INTEGER NOT NULL
    );

CREATE VIRTUAL TABLE IF NOT EXISTS about USING zumero(
    feedid INTEGER UNIQUE NOT NULL REFERENCES feeds (feedid),
    title TEXT NOT NULL
    );

