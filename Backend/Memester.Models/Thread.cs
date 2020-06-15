using System;
using System.Collections.Generic;

namespace Memester.Models
{
    public class Thread
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public IList<Meme> Memes { get; set; }

        public override string ToString() => Name;
    }
}