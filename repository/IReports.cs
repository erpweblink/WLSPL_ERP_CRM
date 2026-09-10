using WEBLINK_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public interface IReports
    {
        Task<List<VM_Reports>> GetTopInvoiceList();
    }
}
