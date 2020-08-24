using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

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
                Name = m.Name,
                FileId = m.FileId,
                ThreadId = m.ThreadId,
                ThreadName = m.Thread.Name
            }).SingleAsync();
        }
        
        private static readonly HttpClient Client = new HttpClient { DefaultRequestHeaders = {{"User-Agent", "Memester"}}};
        [HttpGet("{threadId}/{memeId}/video")]
        public async Task GetMemesWebm([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            long offset = 0; 
            long count = int.MaxValue;
            var rangeHeader = HttpContext.Request.GetTypedHeaders().Range;
            if (rangeHeader != null && rangeHeader.Ranges.Any())
            {
                var range = rangeHeader.Ranges.First();
                offset = range.From ?? 0;
                count = range.To + 1 - offset ?? int.MaxValue;
            }

            var memeFileId = await _databaseContext.Memes.Where(m => m.ThreadId == threadId && m.Id == memeId).Select(m => m.FileId).SingleAsync();
            using var response = await Client.GetAsync($"https://is2.4chan.org/wsg/{memeFileId}.webm", HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Unexpected error on proxying request for blob: {errorText}", errorText);
                HttpContext.Response.StatusCode = (int)response.StatusCode;
                await HttpContext.Response.CompleteAsync();
            }

            Response.Headers["Content-Disposition"] = response.Content.Headers.ContentDisposition.ToString();
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Accept-Ranges"] = "bytes";
            Response.ContentLength = response.Content.Headers.ContentLength;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(HttpContext.Response.Body);
            await HttpContext.Response.CompleteAsync();
        }
    }
}