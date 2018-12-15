using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("balance")]
        public async Task BalanceAsync([Remainder]string Remainder = null)
        {
            // Get balance
            RainBorg.tipBalance = RainBorg.GetBalance();

            decimal i = RainBorg.tipMin - RainBorg.tipBalance;
            if (i < 0) i = 0;

            string m = "Current tip balance: " + RainBorg.Format(RainBorg.tipBalance) + " " + RainBorg.currencyName;
            await ReplyAsync(m);
        }

        [Command("donate")]
        public async Task DonateAsync([Remainder]string Remainder = null)
        {
            string m = "Want to donate to keep the rain a-pouring? How generous of you! :)\n\n";
            m += "To donate, simply send some " + RainBorg.currencyName + " to the following address, REMEMBER to use the provided payment ID, or else your funds will NOT reach the tip pool.\n";
            m += "```Address:\n" + RainBorg.botAddress + "\n";
            m += "Payment ID (INCLUDE THIS):\n" + RainBorg.botPaymentId + "```";
            await Context.Message.Author.SendMessageAsync(m);
        }

        [Command("help")]
        public async Task HelpAsync([Remainder]string Remainder = null)
        {
            string m = "```List of Commands:\n";
            m += $"{RainBorg.botPrefix}balance - Check the bot's tip balance\n";
            m += $"{RainBorg.botPrefix}donate - Learn how you can donate to the tip pool\n";
            m += $"\t Donate to (T)Rainmans TipYar easy, by tipping the Bot with $tip \n";
	    m += $"{RainBorg.botPrefix}optout - Opt out of receiving tips from the bot\n";
            m += $"{RainBorg.botPrefix}optin - Opt back into receiving tips from the bot```";
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                m += "Op-only message:\nOperator-only command documentation can be found at:\n";
                m += "https://github.com/MaddestHatter/RainBorgCore/wiki/Operator-Commands\n";
            }
            //m += "Need more help? Check the wiki link below to learn how to be a part of the rain:\n" + RainBorg.wikiURL;
            await Context.Message.Author.SendMessageAsync(m);
        }

        [Command("optout")]
        public async Task OutOutAsync([Remainder]string Remainder = null)
        {
            if (!OptedOut.ContainsKey(Context.Message.Author.Id))
            {
                OptedOut.Add(Context.Message.Author.Id);
                await RainBorg.RemoveUserAsync(Context.Message.Author, 0);
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
                await Context.Message.Author.SendMessageAsync("You have opted out from receiving future tips.");
            }
            else await Context.Message.Author.SendMessageAsync("You have already opted out, use $optin to opt back into receiving tips.");
        }

        [Command("optin")]
        public async Task OptInAsync([Remainder]string Remainder = null)
        {
            if (OptedOut.ContainsKey(Context.Message.Author.Id))
            {
                OptedOut.Remove(Context.Message.Author.Id);
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
                await Context.Message.Author.SendMessageAsync("You have opted back in, and will receive tips once again.");
            }
            else await Context.Message.Author.SendMessageAsync("You have not opted out, you are already able to receive tips.");
        }
    }
}
