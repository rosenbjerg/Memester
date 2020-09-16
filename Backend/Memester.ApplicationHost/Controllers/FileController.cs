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
        private readonly string _imageFolder;
        private readonly string _snapshotFolder;

        public FileController(IConfiguration configuration)
        {
            var foldersSection = configuration.GetSection("Folders");
            _imageFolder = Path.GetFullPath(foldersSection["Videos"]);
            _snapshotFolder = Path.GetFullPath(foldersSection["Snapshots"]);
        }
        
        [HttpGet("{threadId}/{fileId}/webm")]
        public IActionResult GetWebm([FromRoute, Required]long threadId, [FromRoute, Required]long fileId)
        {
            var threadDirectory = Path.Combine(_imageFolder, $"thread{threadId}", $"{fileId}.webm");
            return PhysicalFile(threadDirectory, "video/webm", true);
        }

        [HttpGet("{threadId}/{fileId}/snapshot")]
        public IActionResult GetSnapshot([FromRoute, Required]long threadId, [FromRoute, Required]long fileId)
        {
            var threadDirectory = Path.Combine(_snapshotFolder, $"thread{threadId}", $"{fileId}.jpeg");
            return PhysicalFile(threadDirectory, "image/jpeg");
        }
    }
}