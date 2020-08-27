using System.Text.Json.Serialization;

namespace Memester.Services
{
    public class ChanThread
    {
        [JsonPropertyName("no")]
        public long Number { get; set; }
        
        [JsonPropertyName("last_modified")]
        public long LastModified { get; set; }
        
        [JsonPropertyName("replies")]
        public long Replies { get; set; }
    }
}