using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Identity.CommonService
{
    public class EmailSetting
    {
        public string EmailFrom { get; set; } = null!;
        public string EmailTo { get; set; } = null!;
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = null!;
        public string SmtpPass { get; set; } = null!;
    }
    public interface IEmailService
    {
        void Send(string to, string subject, string html, string from = null);
    }
    public class EmailService : IEmailService
    {
        private readonly EmailSetting _appSettings;

        public EmailService(IOptions<EmailSetting> options)
        {
            _appSettings = options.Value;
        }
        public void Send(string to, string subject, string html, string from = null)
        {
            //Create Message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_appSettings.EmailFrom));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            //Send Mail

            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
