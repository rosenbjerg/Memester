using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/memes")]
    public class MemeController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly Random _random;

        public MemeController(DatabaseContext databaseContext, Random random)
        {
            _databaseContext = databaseContext;
            _random = random;
        }
        
        [HttpGet("")]
        public async Task<MemeIdentification> GetRandomMeme([FromQuery] long? threadId)
        {
            var threadIds = threadId != null
                ? new[] { threadId.Value }
                : await _databaseContext.Threads.Select(t => t.Id).ToArrayAsync();

            var randomThreadIndex = _random.Next(0, threadIds.Length - 1);
            var randomThreadId = threadIds[randomThreadIndex];
            
            var memeIds = await _databaseContext.Memes.Where(m => m.ThreadId == randomThreadId).Select(m => m.Id).ToArrayAsync();
            var randomMemeIndex = _random.Next(0, memeIds.Length - 1);
            var randomMemeId = memeIds[randomMemeIndex];
            
            return await _databaseContext.Memes.Where(m => m.ThreadId == randomThreadId && m.Id == randomMemeId).Select(m => new MemeIdentification
            {
                Id = m.Id,
                ThreadId = m.ThreadId
            }).SingleAsync();
        }
        
        [HttpGet("{threadId}/{memeId}")]
        public Task<MemeDto> GetMemes([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            return _databaseContext.Memes.Where(m => m.ThreadId == threadId && m.Id == memeId).Select(m => new MemeDto
            {
                Id = m.Id,
                Name = m.FileName,
                FileId = m.FileId,
                ThreadId = m.ThreadId,
                ThreadName = m.Thread.Name
            }).SingleAsync();
        }
    }
}