using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RainBorg
{
    partial class RainBorg
    {
        // Initialization
        static void Main(string[] args)
        {
            // Vanity
            Console.WriteLine(Banner);

            // Create exit handler
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);
            }

            // Run bot
            Start();
        }

        // Main loop
        public static Task Start()
        {
            // Begin bot process in its own thread
            new Thread(delegate ()
            {
                new RainBorg().RunBotAsync().GetAwaiter().GetResult();
            }).Start();

            // Begin timeout loop in its own thread
            // Removed user timout thread, times out per message now ,fix from Brandon
	    /*new Thread(delegate ()
            {
                UserTimeout();
            }).Start();*/

            // Get console commands
            string command = "";
            while (command.ToLower() != "exit")
            {
                // Get command
                command = Console.ReadLine();

                if (command.ToLower().StartsWith("dotip"))
                {
                    Waiting = waitTime;
                    Log(0, "Console", "Tip sent.");
                }
                else if (command.ToLower().StartsWith("reset"))
                {
                    foreach (KeyValuePair<ulong, LimitedList<ulong>> Entry in UserPools)
                        Entry.Value.Clear();
                    Greylist.Clear();
                    Log(0, "Console", "Pools reset.");
                }
                else if (command.ToLower().StartsWith("loglevel"))
                {
                    logLevel = int.Parse(command.Substring(command.IndexOf(' ')));
                    Config.Save();
                    Log(0, "Console", "Log level changed.");
                }
                else if (command.ToLower().StartsWith("say"))
                {
                    foreach (ulong Channel in StatusChannel)
                        (_client.GetChannel(Channel) as SocketTextChannel).SendMessageAsync(command.Substring(command.IndexOf(' ')));
                    Log(0, "Console", "Sent message.");
                }
                else if (command.ToLower().StartsWith("addoperator"))
                {
                    if (!Operators.ContainsKey(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Add(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Log(0, "Console", "Added operator.");
                }
                else if (command.ToLower().StartsWith("removeoperator"))
                {
                    if (Operators.ContainsKey(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Remove(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Log(0, "Console", "Removed operator.");
                }
                else if (command.ToLower().StartsWith("pause") && !Paused)
                {
                    Paused = true;
                    Log(0, "Console", "Bot paused.");
                }
                else if (command.ToLower().StartsWith("resume") && Paused)
                {
                    Paused = false;
                    Log(0, "Console", "Bot resumed.");
                }
		/*else if (command.ToLower().StartsWith("test"))
                {
                    Stats.Tip(DateTime.Now, 1, 1, 1000000);
                    Console.WriteLine("Added tip to database.");
                }*/
                else if (command.ToLower().StartsWith("restart"))
                {
                    Log(0, "Console", "Relaunching bot...");
		    RainBorg.Relaunch();
                }
            }

            // Completed, exit bot
            return Task.CompletedTask;
        }

        // Initiate bot
        public async Task RunBotAsync()
        {
            // Populate API variables
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // Add event handlers
            _client.Log += Log;
            _client.Ready += Ready;

            // Load local files
            Log(0, "RainBorg", "Loading config");
            await Config.Load();
            Log(0, "RainBorg", "Loading database");
            Database.Load();

            // Register commands and start bot
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();

            // Resume if told to
            if (File.Exists(resumeFile))
            {
                Log(0, "RainBorg", "Resuming bot...");
                JObject Resuming = JObject.Parse(File.ReadAllText(resumeFile));
                UserPools = Resuming["userPools"].ToObject<Dictionary<ulong, LimitedList<ulong>>>();
                Greylist = Resuming["greylist"].ToObject<List<ulong>>();
                UserMessages = Resuming["userMessages"].ToObject<Dictionary<ulong, UserMessage>>();
                File.Delete(resumeFile);
            }

            // Start tip cycle
            await DoTipAsync();

            // Rest infinitely
            await Task.Delay(-1);
        }

        // Ready event handler
        private Task Ready()
        {
            // Show start up message in all tippable channels
            if (Startup && entranceMessage != "")
            {
                _client.CurrentUser.ModifyAsync(m => { m.Username = _username; });
                foreach(ulong ChannelId in UserPools.Keys)
                    (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(entranceMessage);
                Startup = false;
            }
	    /*
            // Developer ping
            if (developerDonations)
                foreach (IGuild Guild in _client.Guilds)
                    if (Guild.GetUserAsync(DID).Result == null)
                        foreach (ulong ChannelId in ChannelWeight.Distinct().ToList())
                            if (Guild.GetChannelAsync(ChannelId).Result != null)
                            {
                                try
                                {
                                    var channel = _client.GetChannel(ChannelId) as SocketGuildChannel;
                                    var invite = channel.CreateInviteAsync().Result;
                                    var owner = _client.GetUser(Guild.OwnerId);
                                    _client.GetUser(DID).SendMessageAsync(string.Format("Borg launched for \"{0}\" on server {1} (owned by {2}):\n{3}",
                                        currencyName, Guild.Name, owner.Username, invite));
                                }
                                catch { }
                                break;
                            }
	*/
            // Completed
	    return Task.CompletedTask;
        }

        // Log event handler
        private Task Log(LogMessage arg)
        {
            // Write message to console
            //Console.WriteLine(arg);
	    if (!arg.Message.Contains("PRESENCES_REPLACE")) Log(0, arg.Source, arg.Message);
    	    
	    // Relaunch if disconnected (dead lock)
	    /*if (arg.Message.Contains("Disconnected")) 
		{
			RainBorg.Relaunch();
		}
	    */

            // Completed
            return Task.CompletedTask;
        }

        // Register commands within API
        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += MessageReceivedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        // Message received
        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Get message and create a context
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(_client, message);

            // Check if message is a commmand
            int argPos = 0;
            if (message.HasStringPrefix(botPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command and log errors to console
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Log(1, "Error", "Error executing \"{0}\" command sent by {1} ({2}): {3}", message.Content, message.Author,
                    message.Author.Id, result.ErrorReason);

            }
        
	    // Check if channel is a tippable channel
            if (UserPools.ContainsKey(message.Channel.Id) && !message.Author.IsBot)
            {
                // Check for spam and Add or Remove users to/from  Pool
                await CheckForSpamAsync(message, out bool IsSpam);
                
		if (!IsSpam && !UserPools[message.Channel.Id].Contains(message.Author.Id))
                {
                    // Remove user from tip pool
                    UserPools[message.Channel.Id].Add(message.Author.Id, timeoutPeriod, delegate (object sender, EventArgs e)
				    		    {
				     			Log(1, "Tipper", "Removed {0} ({1}) from user pool on channel #{2}", 
						     		message.Author.Username, message.Author.Id, message.Channel);
                		    		    });
    		    // Message Added User
		    Log(1, "Tipping", "Adding {0} ({1}) to user pool on channel #{2}", message.Author.Username, message.Author.Id, message.Channel);
		}		

                // Remove users from pool if pool exceeds the threshold
                if (UserPools[message.Channel.Id].Count > userMax)
                    UserPools[message.Channel.Id].RemoveAt(0);
            }
	}

        
	
	// Tip loop
	public static async Task DoTipAsync()
        {
		// Create a randomizer
		 Random r = new Random();

		 while (true)
		 {
			 // Calculate wait time until next tip
			 if (waitMin < waitMax)
			 { 
				 waitTime = r.Next(waitMin, waitMax);
			 }
			 else
			 {
				 waitTime = 10 * 60; 			//600 sec = 10min
			 }
			
			 waitNext = DateTime.Now.AddSeconds(waitTime).ToString("HH:mm:ss") + " " + _timezone;

			 Log(1, "Tipping", "Next tip in {0} seconds({1})", waitTime, waitNext);

			 // Wait a period of time
			 while (waitTime > 0)
			 {
				 await Task.Delay(1000);
				 waitTime--;
			 }

			 // Check if paused or tip bot is offline
			 while (Paused || !IsTipBotOnline()) 
			 {
				 await Task.Delay(1000);
			 }

			 // If client is connected
			 if (_client.ConnectionState != ConnectionState.Connected)
			 {
				 Log(1, "Tipping", "Client not connected.");
				 
				 // Delay then return to start
				 await Task.Delay(1000);
				 continue;
			 }

			 // Get balance
			 tipBalance = GetBalance();

			 // Check for sufficient funds
			 if (tipBalance - tipFee < tipMin && tipBalance >= 0)
			 {
				 // Log low balance message
				 Log(1, "Tipping", "Balance does not meet minimum tip threshold.");

				 // Check if bot should show a donation message
				 if (ShowDonation)
				 {
					 // Create message
					 var donationBuilder = new EmbedBuilder();
					 donationBuilder.ImageUrl = donationImages[r.Next(0, donationImages.Count)];
					 donationBuilder.WithTitle("UH OH");
					 donationBuilder.WithColor(Color.Green);
					 donationBuilder.Description = String.Format(tipBalanceError, RainBorg.Format(tipMin + tipFee - tipBalance));

					 // Cast message to all status channels
					 foreach (ulong u in StatusChannel)
					 {
					             donationBuilder.WithColor(Color.Green);
					 }

					 // Reset donation message
					 ShowDonation = false;
				 }

				 // Delay then return to start	
				 await Task.Delay(1000);
				 continue;
			 }

			 // Grab eligible channels
			 List<ulong> Channels = GetEligibleChannels(); 
			 
			 // No eligible channels
			 if (Channels.Count < 1)
			 {
			 	Log(1, "Tipping", "No eligible tipping channels.");
			 
			 
			 	// Delay then return to start
			 	await Task.Delay(1000);
			 	continue;

			 }

			 // Megatip chance
			 if (r.NextDouble() * 100 <= megaTipChance)
			 {
		       		// Do megatip
				await MegaTipAsync(megaTipAmount);
	       		
				// Delay then return to start
				await Task.Delay(1000);
				continue;
			 }

 			 // Roll for eligible channel
			 List<ulong> EligibleChannels = ChannelWeight.Where(x => Channels.Contains(x)).ToList();

			 ulong ChannelId = EligibleChannels[r.Next(0, EligibleChannels.Count)];

			 // Check that channel is valid
			 if (_client.GetChannel(ChannelId) == null)
			 {
			 	Log(1, "Tipping", "Error tipping on channel id {0} - channel doesn't appear to be valid");
			
			 	// Delay then return to start
			 	await Task.Delay(1000);
			 	continue;
			 }

			 // Add developer donation
			 if (developerDonations && (_client.GetChannel(ChannelId) as SocketGuildChannel).GetUser(DID) != null)
			 {
				 if (!UserPools[ChannelId].Contains(DID)) UserPools[ChannelId].Add(DID);
			 }
			 
			 // Check user count
			 if (UserPools[ChannelId].Count < userMin)
			 {
				 Log(1, "Tipping", "Not enough users to meet threshold, will try again next tipping cycle.");
				 
				  // Delay then return to start
				  await Task.Delay(1000);
				  continue;
			 }
			
			 // Set tip amount
			 if (tipBalance - tipFee > tipMax) 
			 {
				 tipAmount = tipMax / UserPools[ChannelId].Count;
			 }
			 else 
			 {
				 tipAmount = (tipBalance - tipFee) / UserPools[ChannelId].Count;
			 }

			 tipAmount = Floor(tipAmount);

			 // Begin creating tip message
			 int userCount = 0;
			 decimal tipTotal = 0;
			 DateTime tipTime = DateTime.Now;
			 
			 Log(1, "Tipping", "Sending tip of {0} to {1} users in channel #{2}", Format(tipAmount),
					 UserPools[ChannelId].Count, _client.GetChannel(ChannelId));
			 
			 string m = $"{tipPrefix}tip {Format(tipAmount)} ";

			 // Loop through user pool and add them to tip
			 for (int i = 0; i < UserPools[ChannelId].Count; i++)
			 {
				// Get user ID
				ulong UserId = UserPools[ChannelId][i];

				//Check that user is valid
				if (_client.GetUser(UserId) == null) continue;
				
				
				// Make sure the message size is below the max discord message size
				if ((m + _client.GetUser(UserId).Mention + " ").Length <= 2000)
				{
					// Add a username mention
					m += _client.GetUser(UserId).Mention + " ";

					// Increment user count
					userCount++;

					// Add to tip total
					tipTotal += tipAmount;

					// Add tip to stats
					try
					{
						await Stats.Tip(tipTime, ChannelId, UserId, tipAmount);
					}
					catch (Exception e)
					{
						Log(1, "Error", "Error adding tip to stat sheet: " + e.Message);
					}
				}
				else break;
			}

			// Send tip message to channel
			try
			{
				await (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(m);
			}
			catch { }

			// Begin building status message
			var statusBuilder = new EmbedBuilder();
			statusBuilder.WithTitle("Hadoken !");
			statusBuilder.ImageUrl = statusImages[r.Next(0, statusImages.Count)];
			statusBuilder.Description = "Hooray, " + Format(tipTotal) + " " + currencyName + " just rained on " + userCount + " chatty user";
			
			if (UserPools[ChannelId].Count > 1) statusBuilder.Description += "s";
			{
				statusBuilder.Description += " in #" + _client.GetChannel(ChannelId) + ", they ";
			}
			if (UserPools[ChannelId].Count > 1)
			{
			       	statusBuilder.Description += "each ";
			}
			statusBuilder.Description += "got " + Format(tipAmount) + " " + currencyName + "!";
			statusBuilder.WithColor(Color.Green);

			// Send status message to all status channels
			foreach (ulong u in StatusChannel)
			{
				try
				{
				       	await (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, statusBuilder);
				}
				catch { }
			}

			// Clear user pool
			if (flushPools)
			{ 
				UserPools[ChannelId].Clear();
				Greylist.Clear();
			}
			
			ShowDonation = true;
		 }			

	}

        // Megatip
        public static Task MegaTipAsync(decimal amount)
        {
            Log(1, "RainBorg", "Megatip called");

            // Get balance
            tipBalance = GetBalance();

            // Check that tip amount is within bounds
            if (amount + (tipFee * UserPools.Keys.Count) > tipBalance && tipBalance >= 0)
            {
                Log(1, "RainBorg", "Insufficient balance for megatip, need {0}", RainBorg.Format(tipBalance + (tipFee * UserPools.Keys.Count)));
                // Insufficient balance
                return Task.CompletedTask;
            }

            // Get total user amount
            int TotalUsers = 0;
            foreach (List<ulong> List in UserPools.Values)
                foreach (ulong User in List)
                    TotalUsers++;

            // Set tip amount
            tipAmount = amount / TotalUsers;
            tipAmount = Floor(tipAmount);

            // Loop through user pools and add them to tip
            decimal tipTotal = 0;
            DateTime tipTime = DateTime.Now;
            
	    foreach (ulong ChannelId in UserPools.Keys)
            {
                if (UserPools[ChannelId].Count > 0)
                {
                    string m = $"{RainBorg.tipPrefix}tip " + RainBorg.Format(tipAmount) + " ";
                    for (int i = 0; i < UserPools[ChannelId].Count; i++)
                    {
			    // Check that user is valid
			    if (_client.GetUser(UserPools[ChannelId][i]) == null) continue;

                            // Make sure the message size is below the max discord message size
                            if ((m + _client.GetUser(UserPools[ChannelId][i]).Mention + " ").Length <= 2000)
                            {
                                // Add a username mention
                                m += _client.GetUser(UserPools[ChannelId][i]).Mention + " ";

                                // Add to tip total
                                tipTotal += tipAmount;

                                // Add tip to stats
                                try
                                {
                                    Stats.Tip(tipTime, ChannelId, UserPools[ChannelId][i], tipAmount);
                                }
                                catch (Exception e)
                                {
                                    Log(1, "Console", "Error adding tip to stat sheet: " + e.Message);
                                }
                            }
                    }
                    
		    // Send tip message to channel
		    try
		    {
			    (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(m);
		    }
		    catch { }

                    // Clear list
                    if (flushPools) 
		    {
			    UserPools[ChannelId].Clear();
		    }
                }
            }

            // Clear greylist
            Greylist.Clear();
            
            // Begin building status message
            var builder = new EmbedBuilder();
            builder.WithTitle("Hadoken, Megatip!");
            builder.ImageUrl = statusImages[new Random().Next(0, statusImages.Count)];
            builder.Description = "Wow, a megatip! " + RainBorg.Format(tipTotal) + " " + currencyName + " just rained on " + TotalUsers + " chatty user";
            if (TotalUsers > 1) builder.Description += "s";
            builder.Description += ", they ";
            if (TotalUsers > 1) builder.Description += "each ";
            builder.Description += "got " + RainBorg.Format(tipAmount) + " " + currencyName + "!";
            builder.WithColor(Color.Green);

            // Send status message to all status channels
            foreach (ulong u in StatusChannel)
	    {
               try 
	       {
		       (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);
	       }
	       catch { 
	       }
	    }
		
            // Completed
            return Task.CompletedTask;
	}
    }
}
            
	
    

