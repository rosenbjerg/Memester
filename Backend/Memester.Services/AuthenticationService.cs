using System;
using System.Threading.Tasks;
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
            var token = await _databaseContext.LoginTokens.AsNoTracking().Include(u => u.User).FirstOrDefaultAsync(u => u.Token == key);
            if (token != null)
            {
                var session = new Session
                {
                    UserAgent = userAgent,
                    UserId = token.UserId,
                    Created = DateTime.UtcNow
                };
                await _sessionService.AddSession(token.Token, session, _sessionLength);
                _operationContext.Session = session;
                return (token.Token, DateTime.UtcNow.Add(_sessionLength));
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
                Token = TokenGenerator.Generate(),
                Expiration = DateTime.Now.AddDays(3),
                User = user,
                UserAgent = userAgent
            };
            _databaseContext.Add(loginToken);
            await _databaseContext.SaveChangesAsync();

            await _emailService.Send(email, $"Login til hvaderderikantinen", $"<p>Open or click the link below to log in to hvaderderikantinen.dk</p><p><a target=\"_blank\" href=\"https://hvaderderikantinen.dk/api/authentication/login/{loginToken.Token}\">hvaderderikantinen.dk/api/authentication/login/{loginToken.Token}</a></p>");
        }
    }
}