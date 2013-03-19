
-- this is the setup script for the dbfile called 'all_feeds', which is the
-- master list of all feeds we've got.

.load zumero.dylib

.echo ON

BEGIN TRANSACTION;

CREATE VIRTUAL TABLE feeds USING zumero(
    feedid INTEGER PRIMARY KEY,
    url TEXT NOT NULL UNIQUE
    );

-- configure the permissions on this dbfile to allow 'anyone' to 
-- (1) pull the dbfile, and 
-- (2) add rows to the feeds table  
-- and nothing else.

SELECT zumero_define_acl_table('main');

INSERT INTO z_acl (scheme,who,tbl,op,result) VALUES (
    '',
    zumero_named_constant('acl_who_anyone'),
    '',
    '*',
    zumero_named_constant('acl_result_deny')
    );

INSERT INTO z_acl (scheme,who,tbl,op,result) VALUES (
    zumero_internal_auth_scheme('zumero_users_admin'), 
    zumero_named_constant('acl_who_any_authenticated_user'), 
    '', 
    '*',
    zumero_named_constant('acl_result_allow')
    );

INSERT INTO z_acl (scheme,who,tbl,op,result) VALUES (
    '',
    zumero_named_constant('acl_who_anyone'),
    '',
    zumero_named_constant('acl_op_pull'),
    zumero_named_constant('acl_result_allow')
    );

INSERT INTO z_acl (scheme,who,tbl,op,result) VALUES (
    '',
    zumero_named_constant('acl_who_anyone'),
    'feeds',
    zumero_named_constant('acl_op_tbl_add_row'),
    zumero_named_constant('acl_result_allow')
    );

INSERT INTO feeds (url) VALUES ('http://feeds.hanselman.com/ScottHanselman');

COMMIT TRANSACTION;

-- zinst393e9343b87.s.zumero.net is my Zumero server for this project.
-- if you want to actually run this code, you'll need to get your own

SELECT zumero_sync(
    'main',
    'https://zinst393e9343b87.s.zumero.net',
    'all_feeds',
    zumero_internal_auth_scheme('zumero_users_admin'), 
    'admin',
    'PASSWORD'
    );


