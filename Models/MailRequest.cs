namespace WEBLINK_CRM.Models
{
    public class MailRequest
    {
        public string? From { get; set; }

        public string? To { get; set; }

        public List<string> Cc { get; set; } = new();

        public List<string> Bcc { get; set; } = new();

        public string? Subject { get; set; }

        public string? Body { get; set; }

        public bool IsBodyHtml { get; set; } = true;

        public List<MailAttachment> Attachments { get; set; } = new();
    }
    public class MailAttachment
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream";
    }
}
