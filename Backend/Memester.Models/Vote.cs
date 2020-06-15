using System;

namespace Memester.Models
{
    public class Vote
    {
        public DateTime Cast { get; set; }
        public bool Positive { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long MemeId { get; set; }
        public Meme Meme { get; set; }
    }
}