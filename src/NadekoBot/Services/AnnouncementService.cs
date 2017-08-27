using Discord;
using Discord.WebSocket;
using NadekoBot.Extensions;
using NadekoBot.Services.Database.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NadekoBot.Common;
using NadekoBot.Common.Replacements;

namespace NadekoBot.Services
{
    public class AnnouncementService : INService
    {
        public readonly ConcurrentDictionary<ulong, AnnouncementSettings> GuildConfigsCache;
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;
        private readonly Logger _log;

        public AnnouncementService(DiscordSocketClient client, IEnumerable<GuildConfig> guildConfigs, DbService db)
        {
            _db = db;
            _client = client;
            _log = LogManager.GetCurrentClassLogger();

            GuildConfigsCache = new ConcurrentDictionary<ulong, AnnouncementSettings>(guildConfigs.ToDictionary(g => g.GuildId, AnnouncementSettings.Create));

            _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        }

        private async Task ClientOnUserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState oldChannel, SocketVoiceState newChannel)
        {
            var settings = GetOrAddSettingsForGuild(((SocketGuildUser)socketUser).Guild.Id);
            if (settings.InfoChannelId == null)
                return;

            // Joins voice channel
            if (oldChannel.VoiceChannel != null && newChannel.VoiceChannel != null)
            {
                await oldChannel.VoiceChannel.Guild.TextChannels.FirstOrDefault(x => x.Id == settings.InfoChannelId).EmbedAsync(new EmbedBuilder()
                    .WithTitle(socketUser.Username + " joined - " + newChannel.VoiceChannel.Name)
                ).ConfigureAwait(false);
            }
            else if (oldChannel.VoiceChannel != null && newChannel.VoiceChannel == null) // leaves voice channel
            {
                
                await oldChannel.VoiceChannel.Guild.TextChannels.FirstOrDefault(x => x.Id == settings.InfoChannelId).EmbedAsync(new EmbedBuilder()
                    .WithTitle(socketUser.Username + " left - " + oldChannel.VoiceChannel.Name)
                ).ConfigureAwait(false);
            } else if (oldChannel.VoiceChannel == null && newChannel.VoiceChannel != null)
            {
                await newChannel.VoiceChannel.Guild.TextChannels.FirstOrDefault(x => x.Id == settings.InfoChannelId).EmbedAsync(new EmbedBuilder()
                    .WithTitle(socketUser.Username + " joined - " + newChannel.VoiceChannel.Name)
                ).ConfigureAwait(false);
            }
        }

        public AnnouncementSettings GetOrAddSettingsForGuild(ulong guildId)
        {
            AnnouncementSettings settings;
            GuildConfigsCache.TryGetValue(guildId, out settings);

            if (settings != null)
                return settings;

            using (var uow = _db.UnitOfWork)
            {
                var gc = uow.GuildConfigs.For(guildId, set => set);
                settings = AnnouncementSettings.Create(gc);
            }

            GuildConfigsCache.TryAdd(guildId, settings);
            return settings;
        }

        public async Task<AnnouncementSettings> UpdateInfoChannel(ulong guildId, ulong messageChannelId)
        {
            var current = GetOrAddSettingsForGuild(guildId);
            current.InfoChannelId = messageChannelId;

            var newVal = await UpdateAnnoucementSettings(guildId, current);
            return newVal;
        }

        public async Task<AnnouncementSettings> UpdateMusicChannel(ulong guildId, ulong messageChannelId)
        {
            var current = GetOrAddSettingsForGuild(guildId);
            current.MusicChannelId = messageChannelId;

            var newVal = await UpdateAnnoucementSettings(guildId, current);
            return newVal;
        }

        public async Task<AnnouncementSettings> UpdateAnnoucementSettings(ulong guildId, AnnouncementSettings settings)
        {
            var oldValue = GetOrAddSettingsForGuild(guildId);
            using (var uow = _db.UnitOfWork)
            {
                var gc = uow.GuildConfigs.For(guildId);
                gc.InfoChannelId = settings.InfoChannelId;
                gc.MusicChannelId = settings.MusicChannelId;
                await uow.CompleteAsync();
            }

            GuildConfigsCache.TryUpdate(guildId, settings, oldValue);

            return settings;
        }

    }

    public class AnnouncementSettings
    {
        public ulong? InfoChannelId { get; set; }
        public ulong? MusicChannelId { get; set; }

        public static AnnouncementSettings Create(GuildConfig g) => new AnnouncementSettings
        {
            InfoChannelId = g.InfoChannelId,
            MusicChannelId = g.MusicChannelId
        };
    }
}