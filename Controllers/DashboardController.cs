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
            var currentEmpCode = HttpContext.Session.GetString("EmpCode");
            if (string.IsNullOrWhiteSpace(currentEmpCode))
                return View(new List<EmployeeNode>());

            var employees = _repo.GetEmployeeHierarchy(currentEmpCode);
            if (employees == null || employees.Count == 0)
                return View(new List<EmployeeNode>());

            BuildTree(employees);

            var selfNode = employees.FirstOrDefault(e =>
                string.Equals(e.EmpCode?.Trim(), currentEmpCode.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            EmployeeNode root;

            if (selfNode == null)
            {
                var minLevel = employees.Min(x => x.HierarchyLevel);
                root = employees.First(x => x.HierarchyLevel == minLevel);
            }
            else if (selfNode.CustRole == "Admin")
            {
                root = selfNode;
            }
            else
            {
                var minLevel = employees.Min(x => x.HierarchyLevel);
                root = employees.FirstOrDefault(x => x.HierarchyLevel == minLevel)
                       ?? selfNode;
            }

            return View(new List<EmployeeNode> { root });
        }

        private static void BuildTree(List<EmployeeNode> employees)
        {
            foreach (var emp in employees)
                emp.Children = new List<EmployeeNode>();

            var lookup = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.EmpCode))
                .GroupBy(
                    e => e.EmpCode.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var emp in employees)
            {
                if (string.IsNullOrWhiteSpace(emp.ParentCode))
                    continue;

                if (string.Equals(emp.EmpCode?.Trim(), emp.ParentCode?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                if (lookup.TryGetValue(emp.ParentCode.Trim(), out var parent))
                    parent.Children.Add(emp);
            }
        }
    }
}