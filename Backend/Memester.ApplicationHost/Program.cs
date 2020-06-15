using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Memester.Services;

namespace Memester
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var service = new ScrapingService();

            var start = DateTime.UtcNow;
            var threads = await service.FetchThreads();
            var elapsed = DateTime.UtcNow.Subtract(start).TotalMilliseconds;

            var ss = threads.SelectMany(t => t.Memes.Select(m => m.FileSize / 1000 / 1000)).Sum();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseKestrel().UseStartup<Startup>(); });
    }
}