using System.IO;
using Memester.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/stream")]
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
        [HttpGet("{threadId}/{memeId}/video")]
        public IActionResult StreamVideo(long threadId, long memeId)
        {
            var path = Path.Combine(_videoFolder, $"thread{threadId}", $"{memeId}.webm");
            return PhysicalFile(path,"video/webm",enableRangeProcessing:true);
        }
    }
}