using System;
using System.Threading.Tasks;
using Hangfire;
using Memester.Application.Model;
using Memester.Core;
using Memester.Database;
using Memester.Models;
using Microsoft.EntityFrameworkCore;

namespace Memester.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly OperationContext _operationContext;
        private readonly SessionService _sessionService;
        private readonly IEmailService _emailService;
        private readonly TimeSpan _sessionLength;

        public AuthenticationService(DatabaseContext databaseContext, OperationContext operationContext, SessionOptions sessionOptions, SessionService sessionService, IEmailService emailService)
        {
            _databaseContext = databaseContext;
            _operationContext = operationContext;
            _sessionService = sessionService;
            _emailService = emailService;
            _sessionLength = TimeSpan.FromDays(sessionOptions.SessionLengthDays);
        }
        public async Task<(string Token, DateTime Expiry)> Login(string key, string userAgent)
        {
            var token = await _databaseContext.LoginTokens.AsNoTracking().Include(u => u.User).FirstOrDefaultAsync(u => u.Key == key);
            if (token != null)
            {
                var session = new Session
                {
                    UserAgent = userAgent,
                    UserId = token.UserId,
                    Created = DateTime.UtcNow
                };
                await _sessionService.AddSession(token.Key, session, _sessionLength);
                _operationContext.Session = session;
                return (token.Key, DateTime.UtcNow.Add(_sessionLength));
            }

            return default;
        }

        public async Task Logout()
        {
            await _sessionService.RemoveSession(_operationContext.Session.UserId, _operationContext.SessionId);
        }

        public async Task SendLoginMail(string email, string userAgent)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                _databaseContext.Add(user = new User { Email = email });
            
            var loginToken = new LoginToken
            {
                Key = TokenGenerator.Generate(),
                Expiration = DateTime.Now.AddDays(3),
                User = user,
                UserAgent = userAgent
            };
            _databaseContext.Add(loginToken);
            await _databaseContext.SaveChangesAsync();
            BackgroundJob.Enqueue<AuthenticationService>(service => service.SendLoginToken(loginToken.Key));
        }

        public async Task SendLoginToken(string token)
        {
            var loginToken = await _databaseContext.LoginTokens.AsNoTracking().Include(t => t.User).SingleAsync(t => t.Key == token);
            await _emailService.Send(loginToken.User.Email, $"Login til memester", $"<p>Open or click the link below to log in to memester.club</p><p><a target=\"_blank\" href=\"https://memester.club/api/authentication/login/{loginToken.Key}\">memester.club/api/authentication/login/{loginToken.Key}</a></p>");

        }
    }
}