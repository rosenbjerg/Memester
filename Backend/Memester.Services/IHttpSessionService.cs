using System;
using Microsoft.AspNetCore.Http;

namespace Memester.Services
{
    public interface IHttpSessionService
    {
        string? ExtractSessionToken(HttpRequest request);
        public void AttachSessionToken(HttpResponse response, string? sessionToken = null, DateTime? sessionExpiration = null);
    }
}