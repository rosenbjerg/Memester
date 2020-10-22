using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Memester.Core.Options
{
    public class EmailOptions
    {
        [Required]
        public string SmtpHost { get; set; } = null!;
        [Required]
        public int SmtpPort { get; set; }
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string ImapHost { get; set; } = null!;
        [Required]
        public int ImapPort { get; set; }
        [Required]
        public List<string> WhitelistedEmails { get; set; }
    }
}
