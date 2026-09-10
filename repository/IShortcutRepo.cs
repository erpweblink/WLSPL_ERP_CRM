using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public interface IShortcutRepo
    {
        Task<List<ShortcutItem>> GetShortcutsAsync();
        Task<List<ShortcutItem>> SearchEmployees(string r, string userRole, string userCode);
        Task<List<ShortcutItem>> SearchCompanies(string q, string userRole, string userCode);
    }
}
