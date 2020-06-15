using System;

namespace Memester.Models
{
    public class FavoritedMeme
    {
        public DateTime Favorited { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long MemeId { get; set; }
        public Meme Meme { get; set; }
    }
}