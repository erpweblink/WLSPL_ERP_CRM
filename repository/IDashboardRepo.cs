using WLSPL_ERP_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public interface IDashboardRepo
    {
        List<EmployeeNode> GetEmployeeHierarchy(string employeeCode);

        Task<EmployeeNodeInfo> GetEmployeeCompanies(string sessionName);

        List<InvoiceRenewalModel> GetInvoiceRenewals(string employeeCode, bool isAdmin);
    }
}
