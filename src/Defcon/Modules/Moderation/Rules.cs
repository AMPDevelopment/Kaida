﻿using System.Text;
using System.Threading.Tasks;
using Defcon.Data.Guilds;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Moderation
{
    [Group("Rules")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [RequireGuild]
    public class Rules : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Rules(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [Command("Agreement")]
        public async Task RulesAgreement(CommandContext context, [Description("The 'I agreed' role.")] DiscordRole role)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(context.Guild.Id));

            if (guild.RulesAgreement.MessageId == ulong.MinValue)
            {
                var acceptedEmoji = DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Accepted);
                var deniedEmoji = DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Denied);

                var description = new StringBuilder().AppendLine($"React to this message with {acceptedEmoji} if you have read the rules and want to unlock the server!")
                                                     .AppendLine($"If you don't agree with the above written rules, please react with {deniedEmoji} to leave now the server.")
                                                     .ToString();

                var response = await context.RespondAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Rules Agreement",
                    Description = description
                });

                await response.CreateReactionAsync(acceptedEmoji);
                await response.CreateReactionAsync(deniedEmoji);

                guild.RulesAgreement.MessageId = response.Id;
                guild.RulesAgreement.RoleId = role.Id;

                await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(context.Guild.Id), guild);
            }
        }

    }
}