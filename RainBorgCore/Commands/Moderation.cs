using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("exile")]
        public async Task ExileAsync(SocketUser user, [Remainder]string Remainder = "")
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                if (!Blacklist.ContainsKey(user.Id))
                {
                    Blacklist.Add(user.Id, Remainder);
                    await RainBorg.RemoveUserAsync(user, 0);
                }
                await Config.Save();
                await ReplyAsync("Blacklisted user, they will receive no tips.");
                try
                {
                    RainBorg.Log(2, "Command", "{0} was blacklisted by {1} with reason being: {2}", user.Id, Context.User.Username, Remainder);

                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("exile")]
        public async Task ExileAsync(ulong user, [Remainder]string Remainder = "")
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                try
                {
                    if (!Blacklist.ContainsKey(Context.Client.GetUser(user).Id))
                    {
                        Blacklist.Add(Context.Client.GetUser(user).Id, Remainder);
                        await RainBorg.RemoveUserAsync(Context.Client.GetUser(user), 0);
                    }
                }
                catch { }
                await Config.Save();
                await ReplyAsync("Blacklisted users, they will receive no tips.");
                try
                {
                    RainBorg.Log(2, "Command", "{0} was blacklisted by {1} with reason being: {2}", user, Context.User.Username, Remainder);

                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("unexile")]
        public async Task UnExileAsync(SocketUser user, [Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                if (Blacklist.ContainsKey(user.Id))
                    Blacklist.Remove(user.Id);
                await Config.Save();
                await ReplyAsync("Removed users from blacklist, they may receive tips again.");
                try
                {
                    RainBorg.Log(2, "Command", "{0} was removed from the blacklist by {1}", user.Id, Context.User.Username);

                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("unexile")]
        public async Task UnExileAsync(ulong user)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                try
                {
                    if (Blacklist.ContainsKey(Context.Client.GetUser(user).Id))
                        Blacklist.Remove(Context.Client.GetUser(user).Id);
                }
                catch { }
                await Config.Save();
                await ReplyAsync("Removed users from blacklist, they may receive tips again.");
                try
                {
                    RainBorg.Log(2, "Command", "{0} was removed from the blacklist by {1}", user, Context.User.Username);

                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("warn")]
        public async Task WarnAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    if (user != null && !RainBorg.Greylist.Contains(user.Id))
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithColor(Color.Green);
                        builder.WithTitle("SPAM WARNING");
                        builder.Description = RainBorg.spamWarning;

                        RainBorg.Greylist.Add(user.Id);
                        await RainBorg.RemoveUserAsync(user, 0);

                        RainBorg.Log(2, "Command", "{0} was sent a spam warning by {1}", user.Id, Context.User.Username);

                        await user.SendMessageAsync("", false, builder);
                    }
                try
                {
                    
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("warn")]
        public async Task WarnAsync(params ulong[] users)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (Context.Client.GetUser(user) != null && !RainBorg.Greylist.Contains(user))
                        {
                            EmbedBuilder builder = new EmbedBuilder();
                            builder.WithColor(Color.Green);
                            builder.WithTitle("SPAM WARNING");
                            builder.Description = RainBorg.spamWarning;

                            RainBorg.Greylist.Add(user);
                            await RainBorg.RemoveUserAsync(Context.Client.GetUser(user), 0);

                            RainBorg.Log(2, "Command", "{0} was sent a spam warning by {1}", user, Context.User.Username);

                            await Context.Client.GetUser(user).SendMessageAsync("", false, builder);
                        }
                    }
                    catch { }
                try
                {
                    
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }
    }
}
