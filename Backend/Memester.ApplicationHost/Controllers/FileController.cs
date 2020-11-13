using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Memester.FileStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;
        private readonly string _videoFolder;
        private readonly string _snapshotFolder;

        public FileController(IConfiguration configuration, FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            var foldersSection = configuration.GetSection("Folders");
            _videoFolder = Path.GetFullPath(foldersSection["Videos"]);
            _snapshotFolder = Path.GetFullPath(foldersSection["Snapshots"]);
        }
        
        [HttpGet("{threadId}/{memeId}/webm")]
        public IActionResult GetWebm([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var webmFile = Path.Combine(_videoFolder, $"thread{threadId}", $"{memeId}.webm");
            var range = 
            return PhysicalFile(webmFile, "video/webm", true);
        }

        [HttpGet("{threadId}/{memeId}/snapshot")]
        public IActionResult GetSnapshot([FromRoute, Required]long threadId, [FromRoute, Required]long memeId)
        {
            var jpegFile = Path.Combine(_snapshotFolder, $"thread{threadId}", $"{memeId}.jpeg");
            return PhysicalFile(jpegFile, "image/jpeg");
        }
    }
}