﻿using System.Threading.Tasks;
using Defcon.Data.Users;
using Defcon.Library.Services.Infractions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Moderation
{
    [Group("Ban")]
    public class Ban : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private readonly IInfractionService infractionService;

        public Ban(ILogger logger, IRedisDatabase redis, IInfractionService infractionService)
        {
            this.logger = logger;
            this.redis = redis;
            this.infractionService = infractionService;
        }

        [GroupCommand]
        public async Task BanSuspect(CommandContext context, [Description("The suspect.")] DiscordMember suspect, [Description("Reason for the moderation action.")] [RemainingText] string reason = "No reason given.")
        {
            await infractionService.CreateInfraction(context.Guild, context.Channel, context.Client, context.Member, suspect, reason, InfractionType.Ban);
        }
    }
}
