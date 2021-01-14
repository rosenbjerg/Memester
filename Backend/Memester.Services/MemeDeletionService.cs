using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memester.Database;
using Microsoft.EntityFrameworkCore;

namespace Memester.Services
{
    public class MemeDeletionService
    {
        private readonly DatabaseContext _databaseContext;

        public MemeDeletionService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public async Task<List<long>> DetermineMemesToDelete(long maxCapacity)
        {
            var now = DateTime.Now;
            var currentTotalSize = await _databaseContext.Memes.SumAsync(m => m.FileSize);
            var overhead = currentTotalSize - maxCapacity;

            if (overhead < 0) 
                return new List<long>();
            
            var memeMeta = (await _databaseContext.Memes
                    .Select(m => new
                    {
                        m.Id,
                        m.FileSize,
                        m.Created,
                        Total = m.Votes.Count,
                        Positive = m.Votes.Count(v => v.Positive)
                    }).ToListAsync())
                .Select(m => new
                {
                    m.Id,
                    m.FileSize,
                    Score = ((m.Total * 1.5) + (m.Positive * 2.5)) / now.Subtract(m.Created).Days
                })
                .OrderBy(m => m.Score)
                .ToList();

            return memeMeta.TakeWhile(m =>
            {
                var done = overhead < 0;
                overhead -= m.FileSize;
                return !done;
            }).Select(m => m.Id).ToList();
        }
    }
}