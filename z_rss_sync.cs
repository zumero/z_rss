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

[assembly: AssemblyTitle("z_rss_sync")]
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

namespace z_rss_sync
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

        private static void do_feeds(SQLiteConnection db_all_feeds, List<feed_row> rows, string server_url, string password)
        {
            foreach (feed_row q in rows)
            {
                string dbfile_name_for_this_feed = string.Format("feed_{0}", q.feedid);
                Console.WriteLine("Synchronizing {0}: {1}", dbfile_name_for_this_feed, q.url);

                SQLiteConnection db_this_feed = open_and_load_zumero(dbfile_name_for_this_feed);

                string sync_result = db_this_feed.ExecuteScalar<string>("SELECT zumero_sync('main', ?, ?, zumero_internal_auth_scheme('zumero_users_admin'), 'admin', ?);", server_url, dbfile_name_for_this_feed, password);
                Console.WriteLine("    {0}", sync_result);

                db_this_feed.Close();
            }
        }

		public static void Main (string[] args)
		{
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: z_rss_sync.exe server_url admin_password");
                return;
            }

            Console.WriteLine("server_url: {0}", args[0]);
            Console.WriteLine("server_url: {0}", args[1]);

            SQLiteConnection db_all_feeds = open_and_load_zumero("all_feeds");

            List<feed_row> rows = null;

            rows = db_all_feeds.Query<feed_row> ("SELECT f.feedid AS feedid, f.url AS url FROM feeds AS f INNER JOIN about AS a ON (f.feedid=a.feedid);");

            do_feeds(db_all_feeds, rows, args[0], args[1]);

            db_all_feeds.Close();
		}
	}
}

