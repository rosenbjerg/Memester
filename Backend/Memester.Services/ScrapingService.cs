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
using Memester.FileStorage;
using Memester.Models;
using Memester.Services.ChanModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Memester.Services
{
    public class ScrapingService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly FileStorageService _fileStorageService;
        private readonly ILogger<ScrapingService> _logger;
        private readonly long _maxCapacityBytes;
        private static readonly HttpClient Client = new HttpClient { DefaultRequestHeaders = {{"User-Agent", "Memester"}}};
        private static readonly Regex HtmlTrimmer = new Regex("<.*?>", RegexOptions.Compiled);

        private const string ThreadsUrl = "https://a.4cdn.org/{BOARD}/threads.json";
        private const string ThreadUrl = "https://a.4cdn.org/{BOARD}/thread/{THREAD}.json";

        public ScrapingService(DatabaseContext databaseContext, FileStorageService fileStorageService , IConfiguration configuration, ILogger<ScrapingService> logger)
        {    
            _databaseContext = databaseContext;
            _fileStorageService = fileStorageService;
            _logger = logger;
            var foldersSection = configuration.GetSection("Folders");
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
                try
                {
                    await _fileStorageService.Delete($"meme{memeSizePair.Id}.webm");
                    await _fileStorageService.Delete($"meme{memeSizePair.Id}.jpeg");
                    sum -= memeSizePair.Size;
                    toDelete.Add(memeSizePair.Id);
                }
                catch (Exception)
                {
                    _logger.LogError("Could not delete meme {MemeId}", memeSizePair.Id);
                    toDelete.Remove(memeSizePair.Id);
                }
            }

            var memesToDelete = await _databaseContext.Memes.Where(m => toDelete.Contains(m.Id)).ToListAsync();
            _databaseContext.RemoveRange(memesToDelete);
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation("Deleted {DeletedMemes} memes", memesToDelete.Count);
            
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

            var existingMemes = await _databaseContext.Memes.Where(m => m.ThreadId == threadId).Select(m => m.Id).ToListAsync();
#if DEBUG
            var posts = rootPost.posts.Where(p => !existingMemes.Contains(p.Number) && p.FileId != 0 && p.Extension == ".webm").Take(5).ToList();
#else
            var posts = rootPost.posts.Where(p => !existingMemes.Contains(p.Number) && p.FileId != 0 && p.Extension == ".webm").Take(20).ToList();
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
                if (await TryDownloadWebm(meme.FileId, meme.Id))
                    downloadedMemes.Add(meme);
            }

            if (!downloadedMemes.Any())
                return;
            
            _databaseContext.AddRange(downloadedMemes);
            await _databaseContext.SaveChangesAsync();
            
        }

        private async Task<bool> TryDownloadWebm(long fileId, long memeId)
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
                
                var tempMemePath = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.webm");
                var snapshotPath = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.png");
                var thumbnailPath = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.jpeg");
                
                try
                {
                    await using (var webmInputStream = await response.Content.ReadAsStreamAsync())
                    await using (var tempFileOutput = File.Create(tempMemePath))
                    {
                        await webmInputStream.CopyToAsync(tempFileOutput);
                    }
                    
                    await CreateTemporarySnapshotFile(tempMemePath, snapshotPath);
                    await ResizeImage(snapshotPath, 200, 200, thumbnailPath);
                    await using (var file = File.OpenRead(tempMemePath))
                        await _fileStorageService.Write($"meme{memeId}.webm", file);
                    await using (var file = File.OpenRead(thumbnailPath))
                        await _fileStorageService.Write($"meme{memeId}.jpeg", file);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    if (File.Exists(tempMemePath)) File.Delete(tempMemePath);
                    if (File.Exists(snapshotPath)) File.Delete(snapshotPath);
                    if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
                }
                
                return true;
            }

            return false;
        }

        private static async Task CreateTemporarySnapshotFile(string filePath, string snapshotPath)
        {
            var analysis = await FFProbe.AnalyseAsync(filePath);
            await FFMpeg.SnapshotAsync(analysis, snapshotPath, new Size(200, 200), analysis.Duration * 0.2);
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

        private static async Task<Stream> DownloadStream(string downloadFolder, long fileId, long memeId,
            HttpResponseMessage response)
        {
            var filePath = Path.Combine(downloadFolder, $"{memeId}.webm");
            return await response.Content.ReadAsStreamAsync();
        }

        private async Task<ChanPostRoot?> DownloadThreadPosts(string board, long thread)
        {
            var response = await Client.GetAsync(ThreadUrl.Replace("{BOARD}", board).Replace("{THREAD}", thread.ToString()));
            var jsonStream = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChanPostRoot>(jsonStream);
        }
    }
}