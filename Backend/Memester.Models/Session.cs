using System;

namespace Memester.Models
{
    public class Session
    {
        public string UserAgent { get; set; }
        public long UserId { get; set; }
        public DateTime Created { get; set; }
    }
}