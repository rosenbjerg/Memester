using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Memester.Application.Model;
using Memester.Core;
using MimeKit;
using MimeKit.Text;

namespace Memester.Services
{
    public class MailkitEmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        public MailkitEmailService(EmailOptions emailOptions)
        {
            _emailOptions = emailOptions;
        }

        public async Task Send(string receiver, string subject, string message)
        {
            var mimeMessage = new MimeMessage
            {
                To = { MailboxAddress.Parse(receiver) },
                Sender = new MailboxAddress(Encoding.Default, "Hvad er der i kantinen?", _emailOptions.Username),
                Subject = subject,
                Body = new TextPart(TextFormat.Html) { Text = message }
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailOptions.SmtpHost, _emailOptions.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);
            await smtp.SendAsync(mimeMessage);
            await smtp.DisconnectAsync(true);
        }
    }
}