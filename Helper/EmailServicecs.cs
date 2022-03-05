using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LimLink_API.Helper
{
    public class EmailServicecs
    {
        private SMTPEmailConfigModel _emailConfiguration = new SMTPEmailConfigModel();

        public EmailServicecs(IConfiguration configuration)
        {
            configuration.Bind("Email", _emailConfiguration);
        }
        public async Task SendEmail(string ToEmail, string Body, string Subject)
        {
            MailMessage mail = new MailMessage
            {
                Subject = Subject,
                Body = Body,
                From = new MailAddress(_emailConfiguration.SenderAddress, _emailConfiguration.SenderDisplayName),
                IsBodyHtml = true,
            };
            mail.To.Add(ToEmail);

            NetworkCredential networkCredential = new NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password);

            SmtpClient smtp = new SmtpClient
            {
                Host = _emailConfiguration.Host,
                Port = _emailConfiguration.Port,
                EnableSsl = _emailConfiguration.EnableSsl,
                UseDefaultCredentials = _emailConfiguration.UseDefaultCredentials,
                Credentials = networkCredential
            };

            mail.BodyEncoding = Encoding.Default;

            await smtp.SendMailAsync(mail);
        }
    }

    public class SMTPEmailConfigModel
    {
        public string SenderAddress { get; set; }
        public string SenderDisplayName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
