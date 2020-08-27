using Hangfire;
using Memester.Database;

namespace Memester.Services
{
    public class IndexingService
    {
        public void UpdateIndex()
        {
            BackgroundJob.Enqueue<ScrapingService>(service => service.IndexBoard("wsg"));
        }
    }
}