using System.ComponentModel.DataAnnotations;

namespace Memester.Core
{
    public class SessionOptions
    {
        [Required]
        public int SessionLengthDays { get; set; }
    }
}