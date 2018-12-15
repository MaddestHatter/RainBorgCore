using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task InfoAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.tipBalance = RainBorg.GetBalance();

                decimal i = RainBorg.tipMin + RainBorg.tipFee - RainBorg.tipBalance;
                if (i < 0) i = 0;

                string m = "```Current tip balance: " + RainBorg.Format(RainBorg.tipBalance) + " " + RainBorg.currencyName + "\n" +
                    "Amount needed for next tip: " + RainBorg.Format(i) + " " + RainBorg.currencyName + "\n" +
                    "Next tip at: " + RainBorg.waitNext + "\n" +
                    "Tip minimum: " + RainBorg.Format(RainBorg.tipMin) + " " + RainBorg.currencyName + "\n" +
                    "Tip maximum: " + RainBorg.Format(RainBorg.tipMax) + " " + RainBorg.currencyName + "\n" +
                    "Megatip amount: " + RainBorg.Format(RainBorg.megaTipAmount) + " " + RainBorg.currencyName + "\n" +
                    "Megatip chance: " + RainBorg.Format(RainBorg.megaTipChance) + "%\n" +
                    "Minimum users: " + RainBorg.userMin + "\n" +
                    "Maximum users: " + RainBorg.userMax + "\n" +
                    "Minimum wait time: " + String.Format("{0:n0}", RainBorg.waitMin) + "s (" + TimeSpan.FromSeconds(RainBorg.waitMin).ToString() + ")\n" +
                    "Maximum wait time: " + String.Format("{0:n0}", RainBorg.waitMax) + "s (" + TimeSpan.FromSeconds(RainBorg.waitMax).ToString() + ")\n" +
                    "Message timeout: " + String.Format("{0:n0}", RainBorg.timeoutPeriod) + "s (" + TimeSpan.FromSeconds(RainBorg.timeoutPeriod).ToString() + ")\n" +
                    "Minimum account age: " + TimeSpan.FromHours(RainBorg.accountAge).ToString() + "\n" +
                    "Flush pools on tip: " + RainBorg.flushPools + "\n" +
                    "Operators: " + Operators.Count + "\n" +
                    "Blacklisted: " + Blacklist.Count + "\n" +
                    "Greylisted: " + RainBorg.Greylist.Count + "\n" +
                    "Channels: " + RainBorg.UserPools.Keys.Count + "\n" +
                    "Paused: " + RainBorg.Paused.ToString() + "\n" +
                    "TipBotOnline ?: " + RainBorg.IsTipBotOnline() +
		    "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("operators")]
        public async Task OperatorsAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                string m = "```Operators:\n";
                foreach (ulong i in Operators.ToList())
                    try
                    {
                        m += Context.Client.GetUser(i).Username + "\n";
                    }
                    catch { }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("blacklist")]
        public async Task BlacklistAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                string m = "```Blacklisted Users:\n";
                foreach (KeyValuePair<ulong, string> i in Blacklist.ToList())
                {
                    try
                    {
                        if ((m + Context.Client.GetUser(i.Key).Username + " (" + i.Key + ") - " + i.Value + "\n```").Length < 2000)
                        {
                            m += Context.Client.GetUser(i.Key).Username + " (" + i.Key + ")";
                            if (i.Value != "")
                                m += " - " + i.Value;
                            m += "\n";
                        }
                        else
                        {
                            m += "```";
                            await Context.Message.Author.SendMessageAsync(m);
                            m = "```" + Context.Client.GetUser(i.Key).Username + " (" + i.Key + ")";
                            if (i.Value != "") m += " - " + i.Value;
                            m += "\n";
                        }
                    }
                    catch
                    {
                        if (("User Not Found (" + i.Key + ") - " + i.Value + "\n```").Length < 2000)
                        {
                            m += "User Not Found (" + i.Key + ")";
                            if (i.Value != "") m += " - " + i.Value;
                            m += "\n";
                        }
                        else
                        {
                            m += "```";
                            await Context.Message.Author.SendMessageAsync(m);
                            m = "```User Not Found (" + i.Key + ")";
                            if (i.Value != "") m += " - " + i.Value;
                            m += "\n";
                        }
                    }
                }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);

                m = "```Greylisted Users:\n";
                foreach (ulong i in RainBorg.Greylist)
                {
                    try
                    {
                        if ((m + Context.Client.GetUser(i).Username + " (" + i + ")\n```").Length < 2000)
                            m += Context.Client.GetUser(i).Username + " (" + i + ")\n";
                        else
                        {
                            m += "```";
                            await Context.Message.Author.SendMessageAsync(m);
                            m = "```" + Context.Client.GetUser(i).Username + " (" + i + ")\n";
                        }
                    }
                    catch
                    {
                        if (("User Not Found (" + i + ")\n```").Length < 2000)
                            m += "User Not Found (" + i + ")\n";
                        else
                        {
                            m += "```";
                            await Context.Message.Author.SendMessageAsync(m);
                            m = "```User Not Found (" + i + ")\n";
                        }
                    }
                }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("userpools")]
        public async Task UserPoolsAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                string m = "```Current User Pools:\n";
                foreach (KeyValuePair<ulong, RainBorg.LimitedList<ulong>> entry in RainBorg.UserPools)
                    try
                    {
                        m += "#" + Context.Client.GetChannel(entry.Key) + " (" + entry.Key + ") :\n";

                        List<ulong> v = entry.Value;
                        foreach (ulong s in v) m += Context.Client.GetUser(s).Username + " (" + s + ")\n";
                        m += "\n\n";
                    }
                    catch { }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("channels")]
        public async Task ChannelsAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                string m = "```Tippable Channels:\n";
                foreach (KeyValuePair<ulong, RainBorg.LimitedList<ulong>> entry in RainBorg.UserPools)
                    try
                    {
                        m += "#" + Context.Client.GetChannel(entry.Key) + ", weight of ";

                        var x = RainBorg.ChannelWeight.GroupBy(i => i);
                        foreach (var channel in x)
                            if (channel.Key == entry.Key) m += channel.Count();

                        m += "\n";
                    }
                    catch { }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("stats")]
        public async Task StatsAsync([Remainder]ulong Id = 0)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                string m = "```";
                StatTracker Stat = null;
                Console.WriteLine(Id);  // Removed in Developerversion .... 

                // Channel stats
                if ((Stat = Stats.GetChannelStats(Id)) != null)
                {
                    m += "#" + Context.Client.GetChannel(Id) + " Channel Stats:\n";
                    m += "Total " + RainBorg.currencyName + " Sent: " + RainBorg.Format(Stat.TotalAmount) + " " + RainBorg.currencyName + "\n";
                    m += "Total Tips Sent: " + Stat.TotalTips + "\n";
                    m += "Average Tip: " + RainBorg.Format(Stat.TotalAmount / Stat.TotalTips) + " " + RainBorg.currencyName + "";
                }

                // User stats
                else if ((Stat = Stats.GetUserStats(Id)) != null)
                {
                    m += "@" + Context.Client.GetUser(Id).Username + " User Stats:\n";
                    m += "Total " + RainBorg.currencyName + " Sent: " + RainBorg.Format(Stat.TotalAmount) + " " + RainBorg.currencyName + "\n";
                    m += "Total Tips Sent: " + Stat.TotalTips + "\n";
                    m += "Average Tip: " + RainBorg.Format(Stat.TotalAmount / Stat.TotalTips) + " " + RainBorg.currencyName + "";
                }

                // Global stats
                else
                {
                    Stat = Stats.GetGlobalStats();
                    m += "Global Stats:\n";
                    m += "Total " + RainBorg.currencyName + " Sent: " + RainBorg.Format(Stat.TotalAmount) + " " + RainBorg.currencyName + "\n";
                    m += "Total Tips Sent: " + Stat.TotalTips + "\n";
                    m += "Average Tip: " + RainBorg.Format(Stat.TotalAmount / Stat.TotalTips) + " " + RainBorg.currencyName + "";
                }

                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }
    }
}
