using System.Text.Json.Serialization;

namespace Memester.Services
{
    public class ChanPost
    {
        [JsonPropertyName("no")]
        public int Number { get; set; }
        
        [JsonPropertyName("name")]
        public string Uploader { get; set; }
        
        [JsonPropertyName("com")]
        public string? Commentary { get; set; }
        
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
        
        [JsonPropertyName("ext")]
        public string Extension { get; set; }
        
        [JsonPropertyName("tim")]
        public long FileId { get; set; }
        
        [JsonPropertyName("time")]
        public long Timestamp { get; set; }
        
        [JsonPropertyName("fsize")]
        public int FileSize { get; set; }
    }
}