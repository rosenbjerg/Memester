using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Route("api/threads")]
    public class MemeController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public MemeController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("")]
        public Task<List<ThreadDto>> GetThreads()
        {
            return _databaseContext.Threads.Select(t => new ThreadDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToListAsync();
        }
        
        [HttpGet("{threadId}")]
        public async Task<FullThreadDto> GetMemes([FromRoute, Required]long threadId)
        {
            var t =  await _databaseContext.Threads.Where(t => t.Id == threadId).Select(t => new FullThreadDto
            {
                Id = t.Id,
                Name = t.Name,
                Memes = t.Memes.Select(m => new MemeDto
                {
                    Name = m.Name,
                    Id = m.Id
                })
            }).FirstOrDefaultAsync();

            return t;
        }
        [HttpGet("{threadId}/{memeId}")]
        public Task<MemeDto> GetMemes([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            return _databaseContext.Memes.Where(m => m.Id == memeId).Select(t => new MemeDto
            {
                Id = t.Id,
                Name = t.Name
            }).FirstOrDefaultAsync();
        }
        
        private static HttpClient _client = new HttpClient();
        [HttpGet("{threadId}/{memeId}/video")]
        public async Task<IActionResult> GetMemes([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var memeFileId = _databaseContext.Memes.Where(m => m.Id == memeId).Select(m => m.FileId).FirstOrDefault();
            using var response = await _client.GetAsync($"https://is2.4chan.org/wsg/{memeFileId}.webm");
            if (!response.IsSuccessStatusCode)
                return 
        }
    }
}