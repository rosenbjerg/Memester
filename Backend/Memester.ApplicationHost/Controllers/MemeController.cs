using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/meme")]
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
        public async Task<MemeDto> GetRandomMeme([FromQuery] long? threadId)
        {
            var threadIds = threadId != null
                ? new[] { threadId.Value }
                : await _databaseContext.Threads.Select(t => t.Id).ToArrayAsync();

            var randomThreadIndex = _random.Next(0, threadIds.Length - 1);
            var randomThreadId = threadIds[randomThreadIndex];
            
            var memeIds = await _databaseContext.Memes.Where(m => m.ThreadId == randomThreadId).Select(m => m.Id).ToArrayAsync();
            var randomMemeIndex = _random.Next(0, memeIds.Length - 1);
            var randomMemeId = memeIds[randomMemeIndex];
            
            return await _databaseContext.Memes.Where(m => m.ThreadId == randomThreadId && m.Id == randomMemeId).Select(t => new MemeDto
            {
                Id = t.Id,
                ThreadId = t.Id,
                ThreadName = t.Thread.Name,
                Name = t.Name
            }).FirstOrDefaultAsync();
        }
        
        [HttpGet("{threadId}/{memeId}")]
        public Task<MemeDto> GetMemes([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            return _databaseContext.Memes.Where(m => m.ThreadId == threadId && m.Id == memeId).Select(t => new MemeDto
            {
                Id = t.Id,
                ThreadId = t.Id,
                Name = t.Name
            }).FirstOrDefaultAsync();
        }
        
        private static readonly HttpClient Client = new HttpClient { DefaultRequestHeaders = {{"User-Agent", "Memester"}}};
        [HttpGet("{threadId}/{memeId}/video")]
        public async Task<ActionResult<Stream>> GetMemesWebm([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var memeFileId = _databaseContext.Memes.Where(m => m.ThreadId == threadId && m.Id == memeId).Select(m => m.FileId).FirstOrDefault();
            using var response = await Client.GetAsync($"https://is2.4chan.org/wsg/{memeFileId}.webm", HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return NotFound();
            await using var memeStream = await response.Content.ReadAsStreamAsync();
            return memeStream;
        }
    }
}