using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFMpegCore;
using Hangfire;
using Instances;
using Memester.Application.Model;
using Memester.Database;
using Memester.Models;
using Memester.Services.ChanModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Memester.Services
{
    public class ScrapingService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly string _videoFolder;
        private readonly string _snapshotFolder;
        private readonly long _maxCapacityBytes;
        private static readonly HttpClient Client = new HttpClient { DefaultRequestHeaders = {{"User-Agent", "Memester"}}};
        private static readonly Regex HtmlTrimmer = new Regex("<.*?>", RegexOptions.Compiled);

        private const string ThreadsUrl = "https://a.4cdn.org/{BOARD}/threads.json";
        private const string ThreadUrl = "https://a.4cdn.org/{BOARD}/thread/{THREAD}.json";

        public ScrapingService(DatabaseContext databaseContext, IConfiguration configuration)
        {    
            _databaseContext = databaseContext;
            var foldersSection = configuration.GetSection("Folders");
            _videoFolder = Path.GetFullPath(foldersSection["Videos"]);
            _snapshotFolder = Path.GetFullPath(foldersSection["Snapshots"]);
            _maxCapacityBytes = long.Parse(configuration["MaxCapacityBytes"]);
        }
        
        [Queue(JobQueues.BoardIndexing)]
        public async Task IndexBoard(string board = "wsg")
        {
            var response = await Client.GetAsync(ThreadsUrl.Replace("{BOARD}", board));
            var jsonStream = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<List<ChanThreadRoot>>(jsonStream);
#if DEBUG
            var ids = root.SelectMany(p => p.Threads.Select(t => t.Number)).Skip(1).Take(10).ToArray();
#else
            var ids = root.SelectMany(p => p.Threads.Select(t => t.Number)).Skip(1).ToArray();
#endif
            foreach (var threadId in ids)
                BackgroundJob.Enqueue<ScrapingService>(service => service.IndexThread(board, threadId));
            BackgroundJob.Enqueue<ScrapingService>(service => service.EnforceMaxCapacity());
        }
        
        [Queue(JobQueues.DiskCleanup)]
        public async Task EnforceMaxCapacity()
        {
            var memeSizePairs = await _databaseContext.Memes.OrderBy(m => m.Created)
                .Select(m => new { Id = m.Id, ThreadId = m.ThreadId, Size = m.FileSize }).ToListAsync();
            var sum = memeSizePairs.Sum(m => (long)m.Size);
            var toDelete = new List<long>();
            foreach (var memeSizePair in memeSizePairs)
            {
                if (sum < _maxCapacityBytes) break;
                var webmFile = Path.Combine(_videoFolder, $"thread{memeSizePair.ThreadId}", $"{memeSizePair.Id}.webm");
                try
                {
                    File.Delete(webmFile);
                    sum -= memeSizePair.Size;
                    toDelete.Add(memeSizePair.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not delete {webmFile}: {e}");
                    toDelete.Remove(memeSizePair.Id);
                }
            }

            var memesToDelete = await _databaseContext.Memes.Where(m => toDelete.Contains(m.Id)).ToListAsync();
            _databaseContext.RemoveRange(memesToDelete);
            await _databaseContext.SaveChangesAsync();
        }
        
        [Queue(JobQueues.ThreadIndexing)]
        public async Task IndexThread(string board, long threadId)
        {
            var rootPost = await DownloadThreadPosts(board, threadId);
            if (rootPost == null) return;
            
            var firstPost = rootPost.posts.First();

            var thread = await _databaseContext.Threads.FirstOrDefaultAsync(t => t.Id == threadId);
            if (thread == null)
            {
                thread = new Thread
                {
                    Id = threadId,
                    Name = WebUtility.HtmlDecode(HtmlTrimmer.Replace(firstPost.Commentary ?? firstPost.Filename ?? string.Empty, string.Empty)),
                    Created = DateTime.UtcNow,
                };
                _databaseContext.Add(thread);
            }

            thread.Memes ??= new List<Meme>();

            var threadDirectory = Path.Combine(_videoFolder, $"thread{threadId}");
            var snapshotDirectory = Path.Combine(_snapshotFolder, $"thread{threadId}");
            Directory.CreateDirectory(threadDirectory);
            Directory.CreateDirectory(snapshotDirectory);
            var existingMemes = await _databaseContext.Memes.Where(m => m.ThreadId == threadId).Select(m => m.Id).ToListAsync();
#if DEBUG
            var posts = rootPost.posts.Where(p => !existingMemes.Contains(p.Number) && p.FileId != 0 && p.Extension == ".webm").Take(5).ToList();
#else
            var posts = rootPost.posts.Where(p => !existingMemes.Contains(p.Number) && p.FileId != 0 && p.Extension == ".webm").ToList();
#endif
            var downloadedMemes = new List<Meme>();
            foreach (var post in posts)
            {
                var meme = new Meme
                {
                    Id = post.Number,
                    Created = DateTime.UtcNow,
                    Name = WebUtility.HtmlDecode(HtmlTrimmer.Replace(post.Commentary ?? post.Filename ?? string.Empty, string.Empty)),
                    ThreadId = threadId,
                    Thread = thread,
                    FileId = post.FileId,
                    FileName = post.Filename,
                    FileSize = post.FileSize
                };
                if (await TryDownloadWebm(threadDirectory, snapshotDirectory, meme.FileId, meme.Id))
                    downloadedMemes.Add(meme);
            }

            if (!downloadedMemes.Any())
                return;
            
            _databaseContext.AddRange(downloadedMemes);
            thread.Memes.AddRange(downloadedMemes);
            await _databaseContext.SaveChangesAsync();
            
        }

        private async Task<bool> TryDownloadWebm(string videoFolder, string snapshotFolder, long fileId, long memeId)
        {
            var urlsToTry = new[]
            {
                $"https://is2.4chan.org/wsg/{fileId}.webm",
                $"https://i.4cdn.org/wsg/{fileId}.webm"
            };

            foreach (var url in urlsToTry)
            {
                using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) continue;
                
                var filePath = await DownloadStream(videoFolder, fileId, memeId, response);
                try
                {
                    var tempFilePath = await CreateTemporarySnapshotFile(filePath);
                    var snapshotFilePath = Path.Combine(snapshotFolder, $"{memeId}.jpeg");
                    await ResizeImage(tempFilePath, 200, 200, snapshotFilePath);
                    File.Delete(tempFilePath);
                }
                catch (Exception)
                {
                    File.Delete(filePath);
                    return false;
                }
                
                return true;
            }

            return false;
        }

        private static async Task<string> CreateTemporarySnapshotFile(string filePath)
        {
            var analysis = await FFProbe.AnalyseAsync(filePath);
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.png");
            await FFMpeg.SnapshotAsync(analysis, tempFilePath);
            return tempFilePath;
        }

        private static async Task ResizeImage(string imagePath, int width, int height, string outputPath)
        {
            var imageExtension = Path.GetExtension(outputPath);
            var arg = $"\"{imagePath}\" -resize {height}x{width} -strip -trim -quality 70";
            if (imageExtension == ".png")
                arg += "-format PNG24";
            else if (imageExtension == ".jpg" || imageExtension == ".jpeg") 
                arg += " -interlace plane -format JPG";

            arg += $" \"{outputPath}\"";
            await Instance.FinishAsync("magick", arg);
        }

        private static async Task<string> DownloadStream(string downloadFolder, long fileId, long memeId,
            HttpResponseMessage response)
        {
            var filePath = Path.Combine(downloadFolder, $"{memeId}.webm");
            await using var inputStream = await response.Content.ReadAsStreamAsync();
            await using var outputStream = File.Create(filePath);
            await inputStream.CopyToAsync(outputStream);
            return filePath;
        }

        private async Task<ChanPostRoot?> DownloadThreadPosts(string board, long thread)
        {
            var response = await Client.GetAsync(ThreadUrl.Replace("{BOARD}", board).Replace("{THREAD}", thread.ToString()));
            var jsonStream = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChanPostRoot>(jsonStream);
        }
    }
}