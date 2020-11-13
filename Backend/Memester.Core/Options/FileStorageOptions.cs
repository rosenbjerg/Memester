using System.ComponentModel.DataAnnotations;

namespace Memester.Core.Options
{
    public class FileStorageOptions
    {
        [Required]
        public string AccessKey { get; set; }
        [Required]
        public string SecretKey { get; set; }
        [Required]
        public string Endpoint { get; set; }
        [Required]
        public string Bucket { get; set; }
    }
}