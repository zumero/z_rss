using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;

using SQLite; // https://github.com/praeclarum/sqlite-net

// ---- BEGIN stuff the usually goes in AssemblyInfo.cs

using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("z_rss_create")]
[assembly: AssemblyDescription("An example of using Zumero from C#")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("1.0.*")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

// ---- END stuff the usually goes in AssemblyInfo.cs

namespace z_rss_create
{
	class Program
	{
        private static SQLiteConnection open_and_load_zumero(string s)
        {
            // open the local SQLite db
            SQLiteConnection db = new SQLiteConnection(s);

            // tell SQLite to allow load_extension()
            db.EnableLoadExtension(1);

            // load the Zumero extension
            // this is the equivalent of ".load zumero.dylib" in the sqlite3 shell
            db.ExecuteScalar<string>("SELECT load_extension('zumero.dylib');");

            return db;
        }

        // define a little class to represent rows of the feeds table

        public class feed_row
        {
            public int feedid { get; set; }
            public string url { get; set; }
        };

        private static void do_feeds(SQLiteConnection db, List<feed_row> rows)
        {
            foreach (feed_row q in rows)
            {
                string dbfile_name_for_this_feed = string.Format("feed_{0}", q.feedid);
                Console.WriteLine("Creating {0}: {1}", dbfile_name_for_this_feed, q.url);

                SyndicationFeed f = null;

                try
                {
                    XmlReader xr = new XmlTextReader(q.url);
                    f = SyndicationFeed.Load(xr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}", e);
                    // TODO failed trying to retrieve or parse this feed.
                    // TODO log the failure?
                    // TODO delete the feed row?
                    // TODO launch nethack?
                }

                if (f != null)
                {
                    db.Execute("ATTACH ? AS cur;", dbfile_name_for_this_feed);

                    db.Execute("BEGIN TRANSACTION;");

                    db.Execute(
                            @"CREATE VIRTUAL TABLE 
                            cur.items 
                            USING zumero
                            (
                              id TEXT PRIMARY KEY NOT NULL, 
                              title TEXT NOT NULL,
                              summary TEXT NOT NULL,
                              pubdate_unix_time INTEGER NOT NULL
                            );"
                            );

                    // each feed is allowed to be pulled by anyone, but only the admin user
                    // can make changes

                    db.ExecuteScalar<string>(
                            @"SELECT zumero_define_acl_table('cur');"
                            );

                    db.Execute(
                            @"INSERT INTO cur.z_acl
                            (scheme,who,tbl,op,result)
                            VALUES ( 
                                '',
                                zumero_named_constant('acl_who_anyone'),
                                '',
                                '*',
                                zumero_named_constant('acl_result_deny')
                            );"
                            );

                    db.Execute(
                            @"INSERT INTO cur.z_acl
                            (scheme,who,tbl,op,result)
                            VALUES ( 
                                zumero_internal_auth_scheme('zumero_users_admin'),
                                zumero_named_constant('acl_who_any_authenticated_user'),
                                '',
                                '*',
                                zumero_named_constant('acl_result_allow')
                            );"
                            );

                    db.Execute(
                            @"INSERT INTO cur.z_acl
                            (scheme,who,tbl,op,result)
                            VALUES ( 
                                '',
                                zumero_named_constant('acl_who_anyone'),
                                '',
                                zumero_named_constant('acl_op_pull'),
                                zumero_named_constant('acl_result_allow')
                            );"
                            );

                    // set the feed title

                    db.Execute("INSERT INTO main.about (feedid, title) VALUES (?,?)",
                            q.feedid,
                            f.Title.Text
                            );

                    db.Execute("COMMIT TRANSACTION;");

                    db.Execute("DETACH cur;");
                }
            }
        }

		public static void Main (string[] args)
		{
            SQLiteConnection db_all_feeds = open_and_load_zumero("all_feeds");

            List<feed_row> rows = null;

            // find ALL the feeds that have never had their title set

            rows = db_all_feeds.Query<feed_row> ("SELECT f.feedid AS feedid, f.url AS url FROM feeds AS f LEFT OUTER JOIN about AS a ON (f.feedid=a.feedid) WHERE (a.title IS NULL);");

            do_feeds(db_all_feeds, rows);

            db_all_feeds.Close();
		}
	}
}

