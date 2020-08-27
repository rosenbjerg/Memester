using System;
using System.Collections.Generic;

namespace Memester.Models
{
    public class Meme
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long FileId { get; set; }
        public long ThreadId { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public ICollection<FavoritedMeme> Favorites { get; set; }
        public Thread Thread { get; set; }
        public DateTime Created { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }

        public override string ToString() => Name;
    }
}