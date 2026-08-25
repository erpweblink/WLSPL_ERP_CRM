using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IMailingRepo
    {
        Task<bool> SendAsync(MailRequest request);
    }
}
