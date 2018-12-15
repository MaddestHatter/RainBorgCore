using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RainBorg
{
    partial class RainBorg
    {
        // Checks a message for spam
        private Task CheckForSpamAsync(SocketUserMessage message, out bool result)
        {
            // Set default to not spam
            result = false;

            // Check if bot
            if (message.Author.IsBot)
                result = true;

            // Check blacklist and greylist and optedout
            if (Blacklist.ContainsKey(message.Author.Id) ||
                Greylist.Contains(message.Author.Id) ||
                OptedOut.ContainsKey(message.Author.Id))
                result = true;

            // Check if command
            if (message.Content.StartsWith("!") ||
                message.Content.StartsWith("$") ||
                message.Content.StartsWith(".") ||
                message.Content.StartsWith("^"))
            {
                Log(3, "Filter", "{0} Command ignored", message.Author);
                result = true;
            }

            // Check user created time
            try
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - message.Author.CreatedAt.ToUnixTimeMilliseconds() < accountAge * 60 * 60 * 1000)
                {
                    Log(3, "Filter", "Account age filter triggered: {0} - {1} = {2} < {3}",
                        DateTimeOffset.Now.ToUnixTimeMilliseconds(), message.Author.CreatedAt.ToUnixTimeMilliseconds(),
                        DateTimeOffset.Now.ToUnixTimeMilliseconds() - message.Author.CreatedAt.ToUnixTimeMilliseconds(),
                        accountAge * 60 * 60 * 1000);
                    result = true;
                }
            }
            catch { }

            // Check minimum number of spaces
            if (message.Content.Count(char.IsWhiteSpace) < 3)
            {
                Log(3, "Filter", "{0} Less than 3 spaces", message.Author);
                result = true;
            }

            // Check if message doesn't contain any alphanumeric
            if (new Regex("[^a-zA-Z0-9]").Replace(message.Content, "").Length < 14)
            {
                Log(3, "Filter", "{0} Not enough alphanumeric", message.Author);
                result = true;
            }

            // Check that message contains at least 1 lowercase letter
            if (!message.Content.Any(char.IsLower))
            {
                Log(3, "Filter", "{0} No lower case letters", message.Author);
                result = true;
            }

            // Check ignored word list
            foreach (string ignore in wordFilter)
                if (message.Content.ToLower().Contains(ignore))
                {
                    Log(3, "Filter", "{0} Ignored word found", message.Author);
                    result = true;
                    break;
                }

            // Check that last message was different
            //if (!UserMessages.ContainsKey(message.Author.Id)) UserMessages[message.Author.Id] = new UserMessage(message);
            //else if (UserMessages[message.Author.Id].Content == message.Content)
            if (UserMessages.ContainsKey(message.Author.Id) && UserMessages[message.Author.Id].Content == message.Content)
	    {
                Log(3, "Filter", "{0} Last message same as current one", message.Author);
                result = true;
            }

            // Check exiled nickname list
            try
            {
                foreach (string ignore in ignoredNicknames)
                    if ((message.Author as SocketGuildUser).Nickname.ToLower().Contains(ignore))
                    {
                        Log(3, "Filter", "{0} Nickname contains blacklisted term", message.Author);
                        result = true;
                        break;
                    }
            }
            catch { }

            // Check that user has at least one required role (if applicable)
            if (requiredRoles.Count > 0)
            {
                var user = message.Author as SocketGuildUser;
                bool HasRole = false;
                foreach (string Role in requiredRoles)
                {
                    var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == Role);
                    if (user.Roles.Contains(role))
                    {
                        HasRole = true;
                        break;
                    }
                }
                if (!HasRole)
                {
                    Log(3, "Filter", "{0} No required role", message.Author);
                    result = true;
                }
            }

            // Completed
            UserMessages[message.Author.Id] = new UserMessage(message);
            return Task.CompletedTask;
        }
    }
}
