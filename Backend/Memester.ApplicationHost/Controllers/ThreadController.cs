using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/threads")]
    public class ThreadController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public ThreadController(DatabaseContext databaseContext)
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
    }
}