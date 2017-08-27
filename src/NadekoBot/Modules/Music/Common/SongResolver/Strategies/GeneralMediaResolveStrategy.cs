using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Network;
using NadekoBot.Services.Database.Models;
using HttpMethod = System.Net.Http.HttpMethod;

namespace NadekoBot.Modules.Music.Common.SongResolver.Strategies
{
    public class GeneralMediaResolveStrategy : IResolveStrategy
    {
        private static readonly Regex mp3Regex = new Regex("(.*)(.mp3)", RegexOptions.Compiled);
        private static readonly Regex oggRegex = new Regex("(.*)(.ogg)", RegexOptions.Compiled);
        private static readonly Regex wavRegex = new Regex("(.*)(.wav)", RegexOptions.Compiled);
        private static readonly Regex mp4fRegex = new Regex("(.*)(.mp4)", RegexOptions.Compiled);
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<bool> IsMediaUrl(string query)
        {
            var res = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, query));
            var type = res.Content.Headers.ContentType;
            return type.MediaType.StartsWith("audio/");
        }
            
        public async Task<SongInfo> ResolveSong(string query)
        {
            if (! await IsMediaUrl(query))
                throw new ArgumentException("This url provided is not returning a content-type which can be played", nameof(query));

            return new SongInfo
            {
                Uri = () => Task.FromResult(query),
                Title = query,
                Provider = "General media",
                ProviderType = MusicType.General,
                Query = query,
                TotalTime = TimeSpan.Zero,
                Thumbnail = "https://cdn.discordapp.com/attachments/155726317222887425/261850914783100928/1482522077_music.png",
            };
        }
    }
}
