using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Memester.Models;

namespace Memester.Services
{
    public class ScrapingService
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly Regex HtmlTrimmer = new Regex("<.*?>", RegexOptions.Compiled);

        private const string ThreadsUrl = "https://a.4cdn.org/{BOARD}/threads.json";
        private const string ThreadUrl = "https://a.4cdn.org/{BOARD}/thread/{THREAD}.json";
        
        public async Task<List<Thread>> FetchThreads(string board = "wsg")
        {
            var response = await Client.GetAsync(ThreadsUrl.Replace("{BOARD}", board));
            var jsonStream = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<List<ChanThreadRoot>>(jsonStream);
            var ids = root.SelectMany(p => p.Threads.Select(t => t.Number)).ToArray();
            var threads = await Task.WhenAll(ids.Select(id => FindMemes("wsg", id)));
            return threads.Where(thread => thread != null).ToList()!;
        }
        private async Task<Thread?> FindMemes(string board, long thread)
        {
            var response = await Client.GetAsync(ThreadUrl.Replace("{BOARD}", board).Replace("{THREAD}", thread.ToString()));
            var jsonStream = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<ChanPostRoot>(jsonStream);
            var memes = root.posts.Where(p => p.FileId != 0 && p.Extension == ".webm").ToList();

            if (!memes.Any()) return null;
            
            var firstPost = memes.First();
            return new Thread
            {
                Id = thread,
                Name = WebUtility.HtmlDecode(HtmlTrimmer.Replace(firstPost.Commentary ?? firstPost.Filename, string.Empty)),
                Created = DateTime.UtcNow,
                Memes = memes.Skip(1).Select(m => new Meme
                {
                    Id = m.Number,
                    Created = DateTime.UtcNow,
                    Name = WebUtility.HtmlDecode(HtmlTrimmer.Replace(m.Commentary ?? m.Filename, string.Empty)),
                    ThreadId = thread,
                    FileId = m.FileId,
                    FileSize = m.FileSize
                }).ToList()
            };
        }
    }
    
    public class ChanThread
    {
        [JsonPropertyName("no")]
        public long Number { get; set; }
        
        [JsonPropertyName("last_modified")]
        public long LastModified { get; set; }
        
        [JsonPropertyName("replies")]
        public long Replies { get; set; }
    }

    public class ChanThreadRoot
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        
        [JsonPropertyName("threads")]
        public IList<ChanThread> Threads { get; set; }
    }
    public class ChanPost
    {
        [JsonPropertyName("no")]
        public int Number { get; set; }
        
        [JsonPropertyName("name")]
        public string Uploader { get; set; }
        
        [JsonPropertyName("com")]
        public string Commentary { get; set; }
        
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        
        [JsonPropertyName("ext")]
        public string Extension { get; set; }
        
        [JsonPropertyName("tim")]
        public long FileId { get; set; }
        
        [JsonPropertyName("time")]
        public long Timestamp { get; set; }
        
        [JsonPropertyName("fsize")]
        public int FileSize { get; set; }
    }

    public class ChanPostRoot
    {
        public IList<ChanPost> posts { get; set; }
    }
}