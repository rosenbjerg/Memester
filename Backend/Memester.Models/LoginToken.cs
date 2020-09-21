using System;

namespace Memester.Models
{
    public class LoginToken
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        public string Key { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public Guid CreatedCorrelationId { get; set; }
        public Guid UpdatedCorrelationId { get; set; }
        public string UserAgent { get; set; } = null!;
    }
}