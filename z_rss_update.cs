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

[assembly: AssemblyTitle("z_rss_update")]
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

namespace z_rss_update
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
                Console.WriteLine("Updating {0}: {1}", dbfile_name_for_this_feed, q.url);
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

                    // set last_update to the time we retrieved the feed XML

                    db.Execute(
                            @"INSERT OR REPLACE 
                            INTO main.last_update
                            (feedid, when_unix_time)
                            VALUES
                            (?, strftime('%s','now')
                            );", 
                            q.feedid);

                    foreach (SyndicationItem it in f.Items)
                    {
                        Console.WriteLine("    {0}", it.Title.Text);

                        TextSyndicationContent t = (TextSyndicationContent) it.Summary;

                        string id = it.Id;

                        if (null == id)
                        {
                            foreach(SyndicationLink link in it.Links) 
                            {
                                id = link.Uri.ToString();
                                break;
                            }
                        }

                        if (null == id)
                        {
                            Console.WriteLine("        no id");
                        }
                        else
                        {
                            db.Execute("INSERT OR IGNORE INTO cur.items (id, title, summary, pubdate_unix_time) VALUES (?,?,?,?)",
                                    id,
                                    it.Title.Text,
                                    t.Text,
                                    (it.PublishDate.UtcDateTime - new DateTime(1970,1,1)).TotalSeconds
                                    );
                        }
                    }

                    db.Execute("COMMIT TRANSACTION;");

                    db.Execute("DETACH cur;");
                }
            }
        }

		public static void Main (string[] args)
		{
            SQLiteConnection db_all_feeds = open_and_load_zumero("all_feeds");

            List<feed_row> rows = null;

            // find ALL the feeds where the dbfile has been created but its XML
            // has never been updated

            rows = db_all_feeds.Query<feed_row> ("SELECT f.feedid AS feedid, f.url AS url FROM feeds AS f INNER JOIN about AS a ON (f.feedid=a.feedid) LEFT OUTER JOIN last_update AS u ON (f.feedid=u.feedid) WHERE (u.when_unix_time IS NULL);");

            do_feeds(db_all_feeds, rows);

            // now check for feeds that have not been updated in the last hour.
            // do 5 of them.

            rows = db_all_feeds.Query<feed_row> ("SELECT f.feedid AS feedid, f.url AS url FROM feeds AS f INNER JOIN last_update AS u ON (f.feedid=u.feedid) WHERE ((strftime('%s','now') - u.when_unix_time) > (60 * 60)) ORDER BY u.when_unix_time ASC LIMIT 5;");

            do_feeds(db_all_feeds, rows);

            db_all_feeds.Close();
		}
	}
}

