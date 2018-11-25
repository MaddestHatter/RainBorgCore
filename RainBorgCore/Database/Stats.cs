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

                // Update global stats
                StatTracker GlobalStats = new StatTracker();
                SqliteCommand Command = new SqliteCommand("SELECT totaltips, totalamount FROM global", Connection);
                //Command.Parameters.AddWithValue("id", Id);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) GlobalStats = new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    };
                GlobalStats.TotalTips++;
                GlobalStats.TotalAmount += Amount;
                Command = new SqliteCommand(@"UPDATE global SET totaltips = @totaltips, totalamount = @totalamount", Connection);
                //Command.Parameters.AddWithValue("id", Channel);
                Command.Parameters.AddWithValue("totaltips", GlobalStats.TotalTips);
                Command.Parameters.AddWithValue("totalamount", GlobalStats.TotalAmount);
                Command.ExecuteNonQuery();

                // Update channel stats
                StatTracker ChannelStats = new StatTracker();
                Command = new SqliteCommand("SELECT totaltips, totalamount FROM channels WHERE id = @id", Connection);
                Command.Parameters.AddWithValue("id", Channel);
                using (SqliteDataReader Reader = Command.ExecuteReader())
                    if (Reader.Read()) ChannelStats = new StatTracker
                    {
                        TotalTips = Reader.GetInt32(0),
                        TotalAmount = Reader.GetDecimal(1)
                    };
                ChannelStats.TotalTips++;
                ChannelStats.TotalAmount += Amount;
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
                SqliteCommand GlobalStatsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS global (
			totaltips INTEGER DEFAULT 0,
                        totalamount BIGINT DEFAULT 0
                    )
                ", Connection);
                GlobalStatsTable.ExecuteNonQuery();
                SqliteCommand ChannelStatsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS channels (
                        id INTEGER UNIQUE,
                        totaltips INTEGER DEFAULT 0,
                        totalamount BIGINT DEFAULT 0
                    )
                ", Connection);
                ChannelStatsTable.ExecuteNonQuery(); 
		// does not WORK !! needs to be update users , changed to userstats?
                SqliteCommand UserStatsTable = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS userstats (
                        id INTEGER UNIQUE,
                        totaltips INTEGER DEFAULT 0,
                        totalamount BIGINT DEFAULT 0
                    )
                ", Connection);
                UserStatsTable.ExecuteNonQuery();
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
                SqliteCommand Command = new SqliteCommand("SELECT totaltips, totalamount FROM global", Connection);
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
