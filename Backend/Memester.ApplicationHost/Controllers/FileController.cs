using System.ComponentModel.DataAnnotations;
using AspNetCore.Mvc.RangedStream;
using Memester.FileStorage;
using Microsoft.AspNetCore.Mvc;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;

        public FileController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        
        [HttpGet("{memeId}/webm")]
        public RangedStreamResult GetWebm([FromRoute, Required]long memeId)
        {
            var filename = $"meme{memeId}.webm";
            return new RangedStreamResult((from, to) => _fileStorageService.Read(filename, from, to), filename, "video/webm");
        }
        [HttpGet("{memeId}/snapshot")]
        public RangedStreamResult GetSnapshot([FromRoute, Required]long memeId)
        {
            var filename = $"meme{memeId}.jpeg";
            return new RangedStreamResult((from, to) => _fileStorageService.Read(filename, from, to), filename, "image/jpeg");
        }

    }
}