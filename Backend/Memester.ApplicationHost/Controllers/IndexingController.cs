using Memester.Services;
using Microsoft.AspNetCore.Mvc;

namespace Memester.Controllers
{
    [ApiController]
    [Route("api/indexing")]
    public class IndexingController : ControllerBase
    {
        private readonly IndexingService _indexingService;

        public IndexingController(IndexingService indexingService)
        {
            _indexingService = indexingService;
        }
        [HttpGet("run")]
        public IActionResult Index()
        {
            _indexingService.UpdateIndex();
            return Ok();
        }
    }
}