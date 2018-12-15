using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace RainBorg
{
    // Utility class to hold tip information
    class Tip
    {
        public DateTime Date { get; }
        public decimal Amount { get; }
        public ulong Channel { get; }
        public Tip (DateTime date, ulong channel, decimal amount)
        {
            Date = date;
            Amount = amount;
            Channel = channel;
        }
    }

    // Utility class to hold channel information
    class StatTracker
    {
        public int TotalTips;
        public decimal TotalAmount;
        public StatTracker()
        {
            TotalTips = 0;
            TotalAmount = 0;
        }
    }

    class Stats
    {
        // Update user tips
        public static Task Tip(DateTime Date, ulong Channel, ulong Id, decimal Amount)
        {
            // Create a database connection
            using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFile))
            {
                // Open connection to database
                Connection.Open();
	 	// Init empty SqliteCommand Object first	
		SqliteCommand Command = new SqliteCommand();
                
		// Update channel stats
                StatTracker ChannelStats = new StatTracker(); // init new StatTracker Obj.
		Command = new SqliteCommand("SELECT totaltips, totalamount FROM channels WHERE id = @id", Connection);
                Command.Parameters.AddWithValue("id", Channel);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) ChannelStats = new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0), 		// field 0 
                        TotalAmount = Reader.GetDecimal(1)		// field 1
                    };
                ChannelStats.TotalTips++;				// increment TotalTips
                ChannelStats.TotalAmount += Amount;			// add Amount
                Command = new SqliteCommand(@"INSERT OR REPLACE INTO channels (id, totaltips, totalamount) values (@id, @totaltips, @totalamount)", 
                    Connection);
                Command.Parameters.AddWithValue("id", Channel);
                Command.Parameters.AddWithValue("totaltips", ChannelStats.TotalTips);
                Command.Parameters.AddWithValue("totalamount", ChannelStats.TotalAmount);
                Command.ExecuteNonQuery();

                // Update user stats
                StatTracker UserStats = new StatTracker();
                Command = new SqliteCommand("SELECT totaltips, totalamount FROM userstats WHERE id = @id", Connection);
                Command.Parameters.AddWithValue("id", Id);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) UserStats = new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    };
                UserStats.TotalTips++;
                UserStats.TotalAmount += Amount;
                Command = new SqliteCommand(@"INSERT OR REPLACE INTO userstats (id, totaltips, totalamount) values (@id, @totaltips, @totalamount)",
                    Connection);
                Command.Parameters.AddWithValue("id", Id);
                Command.Parameters.AddWithValue("totaltips", UserStats.TotalTips);
                Command.Parameters.AddWithValue("totalamount", UserStats.TotalAmount);
                Command.ExecuteNonQuery();

                // Add tip to database
                Command = new SqliteCommand(@"INSERT INTO tipstats (userid, channelid, date, amount) values (@user, @channel, @date, @amount)", Connection);
                Command.Parameters.AddWithValue("user", Id);
                Command.Parameters.AddWithValue("channel", Channel);
                Command.Parameters.AddWithValue("date", Date);
                Command.Parameters.AddWithValue("amount", Amount);
                Command.ExecuteNonQuery();
            }

            // Completed
            return Task.CompletedTask;
        }

        // Loads stats from stat sheet
        public static Task Load()
        {
            // Create stat sheet if it doesn't exist
            using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFile))
            {
                Connection.Open();
                
		// Creates Channels Stats Table	
		SqliteCommand ChannelStatsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS channels (
                        id INTEGER UNIQUE,
                        totaltips INTEGER DEFAULT 0,
                        totalamount BIGINT DEFAULT 0
                    )
                ", Connection);
                ChannelStatsTable.ExecuteNonQuery(); 
		
                // Create User Stats Table  
		SqliteCommand UserStatsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS userstats (
                        id INTEGER UNIQUE,
                        totaltips INTEGER DEFAULT 0,
                        totalamount BIGINT DEFAULT 0
                    )
                ", Connection);
                UserStatsTable.ExecuteNonQuery();
                
		// Create Tip Stats Table
		SqliteCommand TipsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS tipstats (
                        userid INTEGER,
                        channelid INTEGER,
                        date TIMESTAMP,
                        amount INTEGER DEFAULT 0
                    )
                ", Connection);
                TipsTable.ExecuteNonQuery();
            }

            // Completed
            return Task.CompletedTask;
        }

        internal static StatTracker GetChannelStats(ulong Id)
        {
            using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFile))
            {
                Connection.Open();
                SqliteCommand Command = new SqliteCommand("SELECT totaltips, totalamount FROM channels WHERE id = @id", Connection);
                Command.Parameters.AddWithValue("id", Id);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) return new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    };
                    else return null;
            }
        }
        internal static StatTracker GetUserStats(ulong Id)
        {
            using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFile))
            {
                Connection.Open();
                SqliteCommand Command = new SqliteCommand("SELECT totaltips, totalamount FROM userstats WHERE id = @id", Connection);
                Command.Parameters.AddWithValue("id", Id);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) return new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    };
                    else return null;
            }
        }
        internal static StatTracker GetGlobalStats()
        {
            using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFile))
            {
                Connection.Open();
                // select sum of values from DB table userstats to get gloabalstats
		SqliteCommand Command = new SqliteCommand("SELECT sum(totaltips),sum(totalamount) FROM userstats", Connection);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) return new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    }; 
                    else return null;
            }
        }
    }
}
