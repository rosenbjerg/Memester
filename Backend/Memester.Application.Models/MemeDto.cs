using System;

namespace Memester.Application.Model
{
    public class MemeDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ThreadId { get; set; }
        public string ThreadName { get; set; }
    }
}