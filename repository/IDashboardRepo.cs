using WLSPL_ERP_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public interface IDashboardRepo
    {
        List<EmployeeNode> GetAllEmployees();
    }
}
