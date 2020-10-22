using System.ComponentModel.DataAnnotations;

namespace Memester.Core.Options
{
    public class SessionOptions
    {
        [Required]
        public int SessionLengthDays { get; set; }
    }
}