﻿using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Memester.Core
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T?> GetObjectAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken cancellationToken = default)
            where T : class
        {
            var json = await distributedCache.GetAsync(key, cancellationToken);
            return json != null && json.Length > 0 ? JsonSerializer.Deserialize<T>(json) : null;
        }
        public static async Task SetObjectAsync(this IDistributedCache distributedCache, string key, object value, DistributedCacheEntryOptions? distributedCacheEntryOptions = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(value);
            await distributedCache.SetAsync(key, json, distributedCacheEntryOptions ?? new DistributedCacheEntryOptions(), cancellationToken);
        }
    }
}