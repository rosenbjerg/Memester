using System;
using System.Collections.Generic;

namespace Memester.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public bool Admin { get; set; }
        public List<FavoritedMeme> Favorited { get; set; }
        public DateTime Created { get; set; }
        public ICollection<Vote> Votes { get; set; }
    }
}