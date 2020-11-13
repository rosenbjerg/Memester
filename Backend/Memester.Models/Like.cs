namespace Memester.Models
{
    public class Like
    {
        public long MemeId { get; set; }
        public Meme Meme { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}