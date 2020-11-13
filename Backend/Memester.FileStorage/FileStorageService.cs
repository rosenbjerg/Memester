using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Memester.Core;
using Memester.Core.Options;

namespace Memester.FileStorage
{
    public class FileStorageService : IAsyncInitialized
    {
        private readonly AmazonS3Client _s3client;
        private readonly string _bucket;

        public FileStorageService(FileStorageOptions fileStorageOptions)
        {
            _bucket = fileStorageOptions.Bucket;
            _s3client = new AmazonS3Client(new BasicAWSCredentials(fileStorageOptions.AccessKey, fileStorageOptions.SecretKey), new AmazonS3Config
            {
                ServiceURL = fileStorageOptions.Endpoint,
                MaxConnectionsPerServer = 100
            });
        }

        public async Task<(Stream? ResponseStream, long ContentLength, string? ContentRange)> Read(string id, long from = 0, long? to = null)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = $"{GeneratePrefix(id)}/{id}"
            };

            var response = await _s3client.GetObjectAsync(request);
            if (!IsSuccessStatus(response.HttpStatusCode))
            {
                return (null, default, default);
            }
            
            return (response.ResponseStream, response.ContentLength, response.ContentRange);
        }

        public async Task Write(string id, Stream stream)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = $"{GeneratePrefix(id)}/{id}",
                InputStream = stream
            };

            await _s3client.PutObjectAsync(request);
        }

        public async Task Delete(string id)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = $"{GeneratePrefix(id)}/{id}"
            };

            await _s3client.DeleteObjectAsync(request);
        }

        public async Task Initialize()
        {
            if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3client, _bucket))
            {
                await _s3client.PutBucketAsync(_bucket);
            }
        }

        private bool IsSuccessStatus(HttpStatusCode httpStatusCode)
        {
            var httpCodeValue = (int)httpStatusCode;
            return httpCodeValue >= 200 && httpCodeValue <= 299;
        }
        
        private static string GeneratePrefix(string id) => (GetStableHashCode(id) % 999).ToString();

        private static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i+1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
                }

                return hash1 + (hash2*1566083941);
            }
        }
    }
}