using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Memester.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/thread")]
    public class MemeController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public MemeController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("all")]
        public Task<List<ThreadDto>> GetThreads()
        {
            return _databaseContext.Threads.Select(t => new ThreadDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToListAsync();
        }
        [HttpGet("{threadId}")]
        public Task<List<MemeDto>> GetMemes([FromRoute, Required]long threadId)
        {
            return _databaseContext.Memes.Where(m => m.ThreadId == threadId).Select(t => new MemeDto
            {
                FileId = t.FileId,
                Name = t.Name
            }).ToListAsync();
        }
    }

    public class ThreadDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class MemeDto
    {
        public long FileId { get; set; }
        public string Name { get; set; }
    }
}