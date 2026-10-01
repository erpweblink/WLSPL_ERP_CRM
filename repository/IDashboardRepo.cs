using WLSPL_ERP_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public interface IDashboardRepo
    {
        List<EmployeeNode> GetEmployeeHierarchy(string employeeCode);

        Task<List<EmployeeNodeInfo>> GetEmployeePerformance(string currentEmpCode,string selectedEmpCode,DateTime fromDate,DateTime toDate);

        List<InvoiceRenewalModel> GetInvoiceRenewals(string employeeCode, bool isAdmin);
    }
}
