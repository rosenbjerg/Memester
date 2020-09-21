﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memester.Application.Model;
using Memester.Core;
using Memester.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Memester.Services
{
    public class SessionService
    {
        private const string SessionPrefix = "session";
        private readonly IDistributedCache _distributedCache;

        public SessionService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        
        public Task<Session?> FindSession(string token, CancellationToken cancellationToken = default) => _distributedCache.GetObjectAsync<Session>($"{SessionPrefix}:{token}", cancellationToken);

        public async Task<SessionDto[]> FindSessions(long userId, CancellationToken cancellationToken = default)
        {
            var sessions = await _distributedCache.GetObjectAsync<List<string>>($"{SessionPrefix}:{userId}", cancellationToken);
            if (sessions == null)
                return new SessionDto[0];

            var foundSessions = await Task.WhenAll(sessions.Select(async token => (token, session: await FindSession(token, cancellationToken))));
            return foundSessions.Where(s => s.session != null).Select(s => new SessionDto
            {
                Token = s.token,
                UserAgent = s.session!.UserAgent
            }).ToArray()!;
        }

        public async Task AddSession(string token, Session session, TimeSpan validity, CancellationToken cancellationToken = default)
        {
            var sessions = await _distributedCache.GetObjectAsync<List<string>>($"{SessionPrefix}:{session.UserId}", cancellationToken);
            sessions ??= new List<string>();
            await _distributedCache.SetObjectAsync($"{SessionPrefix}:{token}", session, new DistributedCacheEntryOptions
            {
                SlidingExpiration = validity
            }, cancellationToken);
            
            sessions.Add(token);
            await _distributedCache.SetObjectAsync($"{SessionPrefix}:{session.UserId}", sessions, cancellationToken: cancellationToken);
        }
        
        public async Task RemoveSession(long userId, string token, CancellationToken cancellationToken = default)
        {
            var sessions = await _distributedCache.GetObjectAsync<List<string>>($"{SessionPrefix}:{userId}", cancellationToken);
            await _distributedCache.RemoveAsync(token, cancellationToken);
            
            if (sessions != null && sessions.Remove(token))
                await _distributedCache.SetObjectAsync($"{SessionPrefix}:{userId}", sessions, cancellationToken: cancellationToken);
        }
    }
}