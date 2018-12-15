using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("addchannel")]
        public async Task AddChannelAsync(ulong Id, int Weight, [Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.UserPools.ContainsKey(Id) && Weight > 0)
                    {
                        RainBorg.UserPools.Add(Id, new RainBorg.LimitedList<ulong>());
                        for (int i = 0; i < Weight; i++) RainBorg.ChannelWeight.Add(Id);
                        await Config.Save();
                        if (RainBorg.entranceMessage != "")
                            await (Context.Client.GetChannel(Id) as SocketTextChannel).SendMessageAsync(RainBorg.entranceMessage);
                        try
                        {
                            RainBorg.Log(2, "Command", "{0} added to channel list with a weight of {1} by {2}", Id, Weight, Context.User.Username);

                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch
                        {
                            await Context.Message.AddReactionAsync(new Emoji("👌"));
                        }
                    }
                }
                catch { }
            }
        }

        [Command("addchannel")]
        public async Task AddChannelMentionAsync(SocketChannel Channel, int Weight, [Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.UserPools.ContainsKey(Channel.Id) && Weight > 0)
                    {
                        RainBorg.UserPools.Add(Channel.Id, new RainBorg.LimitedList<ulong>());
                        for (int i = 0; i < Weight; i++) RainBorg.ChannelWeight.Add(Channel.Id);
                        await Config.Save();
                        if (RainBorg.entranceMessage != "")
                            await (Context.Client.GetChannel(Channel.Id) as SocketTextChannel).SendMessageAsync(RainBorg.entranceMessage);
                        try
                        {
                            RainBorg.Log(2, "Command", "{0} added to channel list with a weight of {1} by {2}", Channel.Id, Weight, Context.User.Username);

                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch
                        {
                            await Context.Message.AddReactionAsync(new Emoji("👌"));
                        }
                    }
                }
                catch { }
            }
        }

        [Command("removechannel")]
        public async Task RemoveChannelAsync(params ulong[] Ids)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong Id in Ids)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Id))
                        {
                            RainBorg.UserPools.Remove(Id);
                            RainBorg.ChannelWeight.RemoveAll(ChannelId => ChannelId == Id);
                        }
                    }
                    catch { }
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Channels removed by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("removechannel")]
        public async Task RemoveChannelMentionAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketChannel Channel in Context.Message.MentionedChannels)
                    if (RainBorg.UserPools.ContainsKey(Channel.Id))
                    {
                        RainBorg.UserPools.Remove(Channel.Id);
                        RainBorg.ChannelWeight.RemoveAll(ChannelId => ChannelId == Channel.Id);
                    }
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Channels removed by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("addstatuschannel")]
        public async Task AddStatusChannelMentionAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketChannel Channel in Context.Message.MentionedChannels)
                    if (!RainBorg.StatusChannel.Contains(Channel.Id))
                        RainBorg.StatusChannel.Add(Channel.Id);
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Status channel added by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("addstatuschannel")]
        public async Task AddStatusChannelAsync(params ulong[] Channels)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong Channel in Channels)
                    try
                    {
                        if (!RainBorg.StatusChannel.Contains(Channel))
                            RainBorg.StatusChannel.Add(Channel);
                    }
                    catch { }
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Status channel added by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("removestatuschannel")]
        public async Task RemoveStatusChannelAsync(params ulong[] Channels)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong Channel in Channels)
                    try
                    {
                        if (RainBorg.StatusChannel.Contains(Channel))
                            RainBorg.StatusChannel.Remove(Channel);
                    }
                    catch { }
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Status channel removed by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("removestatuschannel")]
        public async Task RemoveStatusChannelMentionAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketChannel Channel in Context.Message.MentionedChannels)
                    if (RainBorg.StatusChannel.Contains(Channel.Id))
                        RainBorg.StatusChannel.Remove(Channel.Id);
                await Config.Save();
                try
                {
                    RainBorg.Log(2, "Command", "Status channel removed by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("addoperator")]
        public async Task AddOperatorAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    if (!Operators.ContainsKey(user.Id))
                    {
                        Operators.Add(user.Id);
                        RainBorg.Log(2, "Command", "{0} added as an operator by {1}", user.Id, Context.User.Username);
                    }
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
            }
        }

        [Command("addoperator")]
        public async Task AddOperatorAsync(params ulong[] users)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (!Operators.ContainsKey(Context.Client.GetUser(user).Id))
                        {
                            Operators.Add(Context.Client.GetUser(user).Id);
                            RainBorg.Log(2, "Command", "{0} added as an operator by {1}", user, Context.User.Username);
                        }
                    }
                    catch { }
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
            }
        }

        [Command("removeoperator")]
        public async Task RemoveOperatorAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    if (Operators.ContainsKey(user.Id))
                    {
                        Operators.Remove(user.Id);
                        RainBorg.Log(2, "Command", "{0} removed as an operator by {1}", user.Id, Context.User.Username);
                    }
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
            }
        }

        [Command("removeoperator")]
        public async Task RemoveOperatorAsync(params ulong[] users)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (Operators.ContainsKey(Context.Client.GetUser(user).Id))
                        {
                            Operators.Remove(Context.Client.GetUser(user).Id);
                            RainBorg.Log(2, "Command", "{0} removed as an operator by {1}", user, Context.User.Username);
                        }
                    }
                    catch { }
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
            }
        }
    }
}
