//using Newtonsoft.Json.Linq;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RainBorg
{
    public partial class RainBorg
    {
        internal static decimal GetBalanceORG()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string dl = client.DownloadString(balanceUrl);
                    JObject j = JObject.Parse(dl);
                    return (decimal)j["balance"];
                }
            }
            catch
            {
                return -1;
            }
        }

 	// New Balance Method via Request	
	internal static decimal GetBalance()
        {
        //  public static JObject RPC(string Host, int Port, string Method, JObject Params = null, string Password = null)

        // Get Balance from Service
	//JObject Result = Request.RPC(daemonHost, daemonPort, "getBalance", new JObject{["address"] = botAddress }, daemonPassword);
	// if (Result.Count < 1 || Result.ContainsKey("error")) return -1;
        // Return current supply dividet by CoinUnits
        //        return (decimal)Result["availableBalance"] / 10000;
	
	// Check Balance Method local or remote 
		if (localTipBot) 
		{   
			// Get Balance from TipBotDB File	
	    		decimal balance = Wallet.GetBalance();
	    		return balance;
		}
	 	else 
		{
			return GetBalanceORG();
		}	
        }



        public static decimal Floor(decimal Input)
        {
            var r = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(Input * r) / r;
        }

        public static string Format(decimal Input)
        {
            Input = Floor(Input);
            string f = "{0:#,##0.#############}";
            return string.Format(f, Input);
        }
        
	public static string Format(double Input)
        {
            decimal I = Floor((decimal)Input);
            string f = "{0:#,##0.#############}";
            return string.Format(f, I);
        }

        // Log
        public static void Log(int LogLevel, string Source, string Message, params object[] Objects)
        {
	    // Check LogLevel
	    if (logLevel >= LogLevel)
	    {
		// Log to console
		Console.WriteLine("{0} {1}\t{2}", DateTime.Now.ToString("HH:mm:ss"), Source, string.Format(Message, Objects));	
	    
	    	// If log file is specified
            	if (!string.IsNullOrEmpty(logFile))
                 using (StreamWriter w = File.AppendText(logFile))
                    w.WriteLine(string.Format("{0} {1} {2}\t{3}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), Source, string.Format(Message, Objects)));
             }	
	}
        
	// On exit
        public static bool ConsoleEventCallback(int eventType)
        {
            // Exiting
            if (eventType == 2)
            {
                if (exitMessage != "") foreach (KeyValuePair<ulong, LimitedList<ulong>> Entry in UserPools)
                        (_client.GetChannel(Entry.Key) as SocketTextChannel).SendMessageAsync(exitMessage).GetAwaiter().GetResult();
                Config.Save().GetAwaiter().GetResult();
            }
            return false;
        }
	
	// Remove a user from all user pools
        public static Task RemoveUserAsync(SocketUser User, ulong ChannelId)
        {
            // 0 = all channels
            if (ChannelId == 0)
                foreach (KeyValuePair<ulong, LimitedList<ulong>> Entry in UserPools)
                {
                    if (Entry.Value.Contains(User.Id))
                        Entry.Value.Remove(User.Id);
                }
            // Specific channel pool
            else if (UserPools.ContainsKey(ChannelId))
            {
                if (UserPools[ChannelId].Contains(User.Id))
                    UserPools[ChannelId].Remove(User.Id);
            }
            return Task.CompletedTask;
        }
        
	// Grab eligible channels
        private static List<ulong> GetEligibleChannels()
        {
            List<ulong> Output = new List<ulong>();
            foreach (KeyValuePair<ulong, LimitedList<ulong>> Entry in UserPools)
            {
                if (Entry.Value.Count >= userMin)
                {
                    Output.Add(Entry.Key);
                }
            }
            return Output;
        }

	// Check if tip bot is online
	public static bool IsTipBotOnline()
	{
	    // Return true if no tip bot ID is supplied
	     if (RainBorg.tipBotId <= 0) return true;
	      // Check client connection state
	      if (_client.ConnectionState != Discord.ConnectionState.Connected) return false;
	       
	       // Check that tip bot uid exists
	       SocketUser TipBot = _client.GetUser(RainBorg.tipBotId);
	       if (TipBot == null)
	       {
	         Log(1, "Utility", "Supplied tip bot user ID does not seem to exist");
	         return false;
	       }
	       
	       // Check online status
	       if (TipBot.Status != Discord.UserStatus.Online) return false;
	       // Tip bot is online
	       return true;
	}
	
	// Relaunch bot
        public static void Relaunch()
        {
            Log(0, "RainBorg", "Relaunching bot...");
            Paused = true;
            JObject Resuming = new JObject
            {
                ["userPools"] = JToken.FromObject(UserPools),
                ["greylist"] = JToken.FromObject(Greylist),
                ["userMessages"] = JToken.FromObject(UserMessages)
            };
            File.WriteAllText(resumeFile, Resuming.ToString());
            Process.Start("RelaunchUtility", "RainBorg");
            ConsoleEventCallback(2);
            Environment.Exit(0);
        }
    
    	//Utility class for serialization of message log on restart
	public class UserMessage
	{
		public DateTimeOffset CreatedAt;
	        public string Content;
	        public UserMessage(SocketMessage Message)
	        {
	          CreatedAt = DateTimeOffset.Now;
	          Content = Message.Content;
	        }
	        public UserMessage() { }
	}
	 
	// Utility class for list that has a timeout on all entries
	public class LimitedList<T> : List<T>
	{
	  // Adds a list entry that expires after a period of seconds
	  public void Add(T Entry, int Expiration, EventHandler OnExpiration = null)
	  {
	 	  // Add to list
	          Add(Entry);
	          
		  // Create a timer
	          Timer Timer = new Timer();
                  Timer.Interval = Expiration * 1000;    
		  Timer.AutoReset = false;
                  // Create timer callback
	          Timer.Elapsed += delegate (object sender, ElapsedEventArgs e) 
		  {
	          	// Remove entry
	          	Remove(Entry);
	          	// Stop timer
	          	Timer.Stop();
	          	// Dispose of timer
	          	Timer.Dispose();
	          	// Trigger expiration event
		  	OnExpiration?.Invoke(null, EventArgs.Empty);
		  };
   		  // Start Timer
		  Timer.Start();
	  }
	
	}
    }
}


