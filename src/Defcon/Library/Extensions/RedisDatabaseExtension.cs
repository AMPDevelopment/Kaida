﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Defcon.Data.Guilds;
using Defcon.Data.Roles;
using Defcon.Data.Users;
using Defcon.Library.Redis;
using DSharpPlus.Entities;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Library.Extensions
{
    public static class RedisDatabaseExtension
    {
        public static async Task<User> InitUser(this IRedisDatabase redis, ulong userId)
        {
            var user = await redis.GetAsync<User>(RedisKeyNaming.User(userId));

            if (user != null) return user;

            user = new User
            {
                Id = userId,
                Description = null,
                Birthdate = null,
                SteamId = null,
                Nicknames = new List<Nickname>(),
                Infractions = new List<Infraction>(),
                InfractionId = 0
            };
                    
            await redis.AddAsync(RedisKeyNaming.User(userId), user);

            return user;
        }

        public static async Task<bool> IsModerator(this IRedisDatabase redis, ulong guildId, DiscordMember member)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));

            var isModerator = false;

            foreach (var role in member.Roles)
            {
                if (isModerator)
                {
                    break;
                }

                isModerator = guild.ModeratorRoleIds.Any(x => x == role.Id);
            }

            return isModerator;
        }

        public static async Task InitGuild(this IRedisDatabase redis, ulong guildId)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));

            if (guild == null)
            {
                await redis.AddAsync<Guild>(RedisKeyNaming.Guild(guildId), new Guild()
                {
                    Id = guildId,
                    Prefix = ApplicationInformation.DefaultPrefix,
                    ModeratorRoleIds = new List<ulong>(),
                    AllowWarnModerators = false,
                    AllowMuteModerators = false,
                    RulesAgreement = new RulesAgreement(),
                    Logs = new List<Log>(),
                    Settings = new List<Setting>(),
                    ReactionCategories = new List<ReactionCategory>(),
                    ReactionMessages = new List<ReactionMessage>()
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(guild.Prefix))
                {
                    guild.Prefix = ApplicationInformation.DefaultPrefix;
                }

                guild.ModeratorRoleIds ??= new List<ulong>();

                if (guild.AllowWarnModerators != true && guild.AllowWarnModerators != false)
                {
                    guild.AllowWarnModerators = false;
                }

                if (guild.AllowMuteModerators != true && guild.AllowMuteModerators != false)
                {
                    guild.AllowMuteModerators = false;
                }

                guild.RulesAgreement ??= new RulesAgreement()
                {
                    MessageId = ulong.MinValue,
                    RoleId = ulong.MinValue
                };
                guild.Logs ??= new List<Log>();
                guild.Settings ??= new List<Setting>();
                guild.ReactionCategories ??= new List<ReactionCategory>();
                guild.ReactionMessages ??= new List<ReactionMessage>();

                await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guildId), guild);
            }
        }
    }
}