using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memester.Database;
using Memester.Models;
using Memester.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Memester.UnitTests
{
    public class Tests
    {
        [Test]
        public async Task TestCleanup()
        {
            const long maxCapacity = 100_000;
            var benchBuilder = UnitTestBench.Create();
            benchBuilder.AddScoped<MemeDeletionService>();
            var bench = benchBuilder.BuildServiceProvider();
            
            await using var databaseContext = bench.GetRequiredService<DatabaseContext>();
            databaseContext.AddRange(GenerateMemes(1000));
            await databaseContext.SaveChangesAsync();

            var usedBeforeClean = await databaseContext.Memes.SumAsync(m => m.FileSize);
            Assert.True(usedBeforeClean > maxCapacity);
            
            var scrapingService = bench.GetRequiredService<MemeDeletionService>();
            var ids = await scrapingService.DetermineMemesToDelete(maxCapacity);

            var memes = await databaseContext.Memes.Where(m => ids.Contains(m.Id)).ToListAsync();
            databaseContext.RemoveRange(memes);
            await databaseContext.SaveChangesAsync();

            var usedAfterClean = await databaseContext.Memes.SumAsync(m => m.FileSize);
            Assert.True(usedAfterClean < maxCapacity);
        }

        private IEnumerable<Meme> GenerateMemes(int count)
        {
            var now = DateTime.UtcNow;
            var random = new Random();
            var thread = new Thread { Name = "thread" };
            for (var i = 0; i < count; i++)
            {
                yield return new Meme
                {
                    Name = "meme",
                    Thread = thread,
                    FileSize = random.Next(2_000, 5_000),
                    FileName = "file.webm"
                };
            }
        }
    }
}