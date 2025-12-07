using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace InformacinesSistemos.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendComplaintEmailAsync(string fromEmail, string fromName, string subject, string body)
        {
            var host = _config["Email:SmtpHost"];
            var portString = _config["Email:SmtpPort"];
            var enableSslStr = _config["Email:EnableSsl"];
            var userName = _config["Email:UserName"];
            var password = _config["Email:Password"];
            var moderatorMail = _config["Email:ModeratorEmail"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(moderatorMail))
            {
                throw new InvalidOperationException("Neteisingai sukonfigūruota Email sekcija appsettings.json.");
            }

            var port = int.TryParse(portString, out var p) ? p : 587;
            var enableSsl = bool.TryParse(enableSslStr, out var ssl) ? ssl : true;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(userName, password)
            };

            // Laiške “From” užrašome būtent PRISIJUNGUSIO VARTOTOJO el. paštą
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            // Siunčiame moderatoriui
            message.To.Add(new MailAddress(moderatorMail));

            // Kad atsakymas eitų tam vartotojui, pridedam ir Reply-To
            if (!string.IsNullOrWhiteSpace(fromEmail))
            {
                message.ReplyToList.Add(new MailAddress(fromEmail, fromName));
            }

            await client.SendMailAsync(message);
        }
    }
}
