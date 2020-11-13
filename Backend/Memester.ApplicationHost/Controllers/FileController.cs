using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Database;
using Memester.FileStorage;
using Microsoft.AspNetCore.Http;
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

        public FileController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        
        [HttpGet("{memeId}/webm")]
        public async Task<IActionResult> GetWebm([FromRoute, Required]long memeId)
        {
            var filename = $"meme{memeId}.webm";
            var requestedRange = Request.GetTypedHeaders().Range?.Ranges.FirstOrDefault();
            var (stream, length, servedRange) = await _fileStorageService.Read(filename, requestedRange?.From ?? 0, requestedRange?.To);
            return SendStream(stream, filename, "video/webm", servedRange, length);
        }
        [HttpGet("{memeId}/snapshot")]
        public async Task<IActionResult> GetSnapshot([FromRoute, Required]long memeId)
        {
            var filename = $"meme{memeId}.jpeg";
            var requestedRange = Request.GetTypedHeaders().Range?.Ranges.FirstOrDefault();
            var (stream, length, servedRange) = await _fileStorageService.Read(filename, requestedRange?.From ?? 0, requestedRange?.To);
            return SendStream(stream, filename, "image/jpeg", servedRange, length);
        }

        private IActionResult SendStream(Stream? stream, string filename, string mime, string? servedRange, long length)
        {
            if (stream == null)
            {
                return NotFound();
            }

            Response.StatusCode = servedRange != null ? 206 : 200;
            Response.ContentLength = length;
            Response.Headers["Accept-Ranges"] = "bytes";
            Response.Headers["Content-Range"] = servedRange;
            Response.Headers["Content-Disposition"] = $"filename=\"{WebUtility.UrlEncode(filename)}\"";

            return File(stream, mime, true);
        }

    }
}