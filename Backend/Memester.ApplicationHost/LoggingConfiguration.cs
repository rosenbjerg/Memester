using System;
using System.IO;
using Memester.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace Memester
{
    public static class LoggingConfiguration
    {
        public static IHostBuilder UseNxplxSerilog(this IHostBuilder hostBuilder, string serviceName)
        {
            return hostBuilder
                .ConfigureServices((context, collection) => collection.AddLogging(builder => builder.ClearProviders()))
                .UseSerilog(ConfigureSerilog(serviceName));
        }

        private static Action<HostBuilderContext, LoggerConfiguration> ConfigureSerilog(string serviceName)
        {
            return (hostingContext, loggerConfiguration) =>
            {
                var logSettings = hostingContext.Configuration.GetSection("Logging").Get<LoggingOptions>();
                loggerConfiguration
                    .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                    .Filter.ByExcluding(Matching.FromSource("Hangfire"))
                    .Enrich.FromLogContext().WriteTo.Async(x =>
                    {
#if DEBUG
                        x.Console();
#else
                        x.Console(
                            formatter: new JsonFormatter(),
                            restrictedToMinimumLevel: logSettings.LogLevel,
                            standardErrorFromLevel: LogEventLevel.Error);
#endif
                    });
            };
        }
    }
}