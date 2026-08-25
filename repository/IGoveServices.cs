using Newtonsoft.Json.Linq;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface IGoveServices
    {
        Task<AuthKeyResponse> GetOrGenerateAuthKeyAsync();
        Task<AuthKeyResponse> GenerateAuthKeyAsync(string userName, string password, string sellerGstNo);
        Task<JObject> GetGSTDetailsAsync(string gstNo);
    }
}
