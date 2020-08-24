using System;

namespace Memester.Application.Model
{
    public class MemeDto : MemeIdentification
    {
        public string Name { get; set; }
        public string ThreadName { get; set; }
        public long FileId { get; set; }
    }

    public class MemeIdentification
    {
        public long Id { get; set; }
        public long ThreadId { get; set; }
    }
}