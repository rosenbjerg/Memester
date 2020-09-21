using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Memester.Services.ChanModels
{
    public class ChanThreadRoot
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        
        [JsonPropertyName("threads")]
        public IList<ChanThread> Threads { get; set; }
    }
}