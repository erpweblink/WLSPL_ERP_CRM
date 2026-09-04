using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Repositories;
using WLSPL_ERP_CRM.Models;

namespace WEBLINK_CRM.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepo _repo;

        public DashboardController(IDashboardRepo repo)
        {
            _repo = repo;
        }

        public ActionResult Index()
        {
            var allEmployees = _repo.GetAllEmployees();

            var tree = BuildManagerSalesTree(allEmployees);

            return View(tree);
        }

        private static List<EmployeeNode> BuildManagerSalesTree(List<EmployeeNode> employees)
        {
            var result = new List<EmployeeNode>();

            if (employees == null || employees.Count == 0)
                return result;

            // Clear children
            foreach (var employee in employees)
            {
                employee.Children = new List<EmployeeNode>();
            }

            // Create lookup by employee code
            var employeeLookup = employees
                .Where(x => !string.IsNullOrWhiteSpace(x.EmpCode))
                .ToDictionary(
                    x => x.EmpCode.Trim(),
                    x => x,
                    StringComparer.OrdinalIgnoreCase);

            // Build ParentCode hierarchy
            foreach (var employee in employees)
            {
                if (string.IsNullOrWhiteSpace(employee.ParentCode))
                    continue;

                var parentCode = employee.ParentCode.Trim();

                if (employeeLookup.TryGetValue(parentCode, out var parent))
                {
                    // Don't add itself as child
                    if (!string.Equals(
                        employee.EmpCode,
                        parent.EmpCode,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        parent.Children.Add(employee);
                    }
                }
            }

            // Root employees
            // Your WLSPL/01 Admin is the root.
            var roots = employees
                .Where(x =>
                    string.IsNullOrWhiteSpace(x.ParentCode) ||
                    string.Equals(
                        x.EmpCode,
                        x.ParentCode,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            result.AddRange(roots);

            return result;
        }

    }
}
