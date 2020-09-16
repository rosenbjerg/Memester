using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly string _videoFolder;
        private readonly string _snapshotFolder;

        public FileController(IConfiguration configuration)
        {
            var foldersSection = configuration.GetSection("Folders");
            _videoFolder = Path.GetFullPath(foldersSection["Videos"]);
            _snapshotFolder = Path.GetFullPath(foldersSection["Snapshots"]);
        }
        
        [HttpGet("webm/{threadId}/{memeId}")]
        public IActionResult GetWebm([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var threadDirectory = Path.Combine(_videoFolder, $"thread{threadId}", $"{memeId}.webm");
            return PhysicalFile(threadDirectory, "video/webm", true);
        }

        [HttpGet("snapshot/{threadId}/{memeId}")]
        public IActionResult GetSnapshot([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var threadDirectory = Path.Combine(_snapshotFolder, $"thread{threadId}", $"{memeId}.jpeg");
            return PhysicalFile(threadDirectory, "image/jpeg");
        }
    }
}