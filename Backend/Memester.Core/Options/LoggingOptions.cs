using System.ComponentModel.DataAnnotations;
using Serilog.Events;

namespace Memester.Core.Options
{
    public class LoggingOptions
    {
        [Required]
        public LogEventLevel LogLevel { get; set; }

        [Required] public string Directory { get; set; } = null!;
    }
}