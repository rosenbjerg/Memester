using System.Collections.Generic;

namespace Memester.Application.Model
{
    public class FullThreadDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<MemeDto> Memes { get; set; }
    }
}