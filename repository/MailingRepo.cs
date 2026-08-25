
using System.Net;
using System.Net.Mail;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class MailingRepo : IMailingRepo
    {
        private readonly IConfiguration _configuration;

        public MailingRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendAsync(MailRequest request)
        {
            using MailMessage mail = new MailMessage();

            var fromMail = request.From ?? "";

            if (string.IsNullOrWhiteSpace(fromMail))
                throw new Exception("Mail username is not configured.");

            mail.From = new MailAddress(fromMail);

            // To
            if (!string.IsNullOrWhiteSpace(request.To))
            {
                foreach (var email in request.To
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(email.Trim());
                }
            }

            // CC
            foreach (var email in request.Cc)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    mail.CC.Add(email.Trim());
            }

            // BCC
            foreach (var email in request.Bcc)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    mail.Bcc.Add(email.Trim());
            }

            // Subject
            mail.Subject = request.Subject ?? "";

            // Body
            string body = request.Body ?? "";

            mail.Body = body;
            mail.IsBodyHtml = request.IsBodyHtml;

            // Attachments
            var attachmentStreams = new List<MemoryStream>();

            try
            {
                foreach (var attachment in request.Attachments)
                {
                    if (attachment.Content == null ||
                        attachment.Content.Length == 0)
                    {
                        continue;
                    }

                    var stream = new MemoryStream(attachment.Content);

                    attachmentStreams.Add(stream);

                    mail.Attachments.Add(
                        new Attachment(
                            stream,
                            attachment.FileName,
                            attachment.ContentType
                        )
                    );
                }

                // SMTP
                using SmtpClient smtp = new SmtpClient();

                smtp.Host = _configuration["MailSettings:Host"]?? throw new Exception("SMTP Host not configured.");

                smtp.Port = int.Parse(_configuration["MailSettings:Port"] ?? "25");

                smtp.EnableSsl = bool.Parse(_configuration["MailSettings:EnableSsl"] ?? "false");

                smtp.UseDefaultCredentials = false;

                smtp.Credentials = new NetworkCredential(_configuration["MailSettings:MailUserName"],_configuration["MailSettings:MailUserPass"]);

                // IMPORTANT: Send before disposing streams
                await smtp.SendMailAsync(mail);

                return true;
            }
            finally
            {
                foreach (var stream in attachmentStreams)
                {
                    await stream.DisposeAsync();
                }
            }
        }
    }
}
