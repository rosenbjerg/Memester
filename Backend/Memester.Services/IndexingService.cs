using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memester.Database;
using Microsoft.EntityFrameworkCore;

namespace Memester.Services
{
    public class IndexingService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ScrapingService _scrapingService;

        public IndexingService(DatabaseContext databaseContext, ScrapingService scrapingService)
        {
            _databaseContext = databaseContext;
            _scrapingService = scrapingService;
        }

        public async Task UpdateIndex()
        {
            var started = DateTime.UtcNow;
            var threads = await _scrapingService.FetchThreads();
            var elapsedFetching = DateTime.UtcNow.Subtract(started);
            Console.WriteLine($"Spent {elapsedFetching.TotalMilliseconds}ms fetching {threads.Count} containing {threads.Sum(t => t.Memes.Count)} posts");

            started = DateTime.UtcNow;
            var threadIds = threads.Select(t => t.Id).ToList();
            var existingThreadIds = new HashSet<long>(await _databaseContext.Threads.Where(t => threadIds.Contains(t.Id)).Select(t => t.Id).ToListAsync());
            var newThreads = threads.Where(t => !existingThreadIds.Contains(t.Id)).ToList();
            _databaseContext.Threads.AddRange(newThreads);
            await _databaseContext.SaveChangesAsync();
            var elapsedSaving = DateTime.UtcNow.Subtract(started);
            Console.WriteLine($"Spent {elapsedSaving.TotalMilliseconds}ms saving {newThreads.Count} new threads containing {newThreads.Sum(t => t.Memes.Count)} posts");

            started = DateTime.UtcNow;
            var posts = threads.Where(t => existingThreadIds.Contains(t.Id)).SelectMany(t => t.Memes).ToList();
            var postIds = posts.Select(p => p.Id).ToList();
            var existingPostIds = new HashSet<long>(await _databaseContext.Memes.Where(m => postIds.Contains(m.Id)).Select(m => m.Id).ToListAsync());
            var newMemes = posts.Where(t => !existingPostIds.Contains(t.Id)).ToList();
            _databaseContext.Memes.AddRange(newMemes);
            await _databaseContext.SaveChangesAsync();
            elapsedSaving = DateTime.UtcNow.Subtract(started);
            Console.WriteLine($"Spent {elapsedSaving.TotalMilliseconds}ms adding {newMemes.Count} new memes to existing threads");
        }
    }
}