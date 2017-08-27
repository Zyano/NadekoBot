using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NadekoBot.Common.Attributes;
using NadekoBot.Extensions;
using NadekoBot.Services;

namespace NadekoBot.Modules.Administration
{
    public partial class Administration
    {
        public class ServerAnnoucmentCommands : NadekoSubmodule<AnnouncementService>
        {
            private readonly DbService _db;

            public ServerAnnoucmentCommands(DbService db)
            {
                _db = db;
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GetChannels(int id, int page = 1, int perPage = 5)
            {
                var channels = await Context.Guild.GetTextChannelsAsync();
                IEnumerable<string> enumChannels = channels.Select(x => x.Name);
                await Context.Message.Channel.SendTableAsync(enumChannels,s => s);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task SetInfoChannel(string channelName)
            {
                var channel =
                    (await Context.Guild.GetTextChannelsAsync()).FirstOrDefault(
                        x => String.Compare(x.Name, channelName, StringComparison.OrdinalIgnoreCase) == 0);

                await _service.UpdateInfoChannel(Context.Guild.Id, channel.Id);
                await Context.Message.Channel.SendConfirmAsync("Music channel has been updated to " + channel.Name);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task SetMusicChannel(string channelName)
            {
                var channel =
                    (await Context.Guild.GetTextChannelsAsync()).FirstOrDefault(
                        x => String.Compare(x.Name, channelName, StringComparison.OrdinalIgnoreCase) == 0);

                await _service.UpdateMusicChannel(Context.Guild.Id, channel.Id);
                await Context.Message.Channel.SendConfirmAsync("Music channel has been updated to " + channel.Name);
            }
        }
    }
}