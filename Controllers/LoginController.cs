using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.Repositories;

namespace WEBLINK_CRM.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public LoginController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = _employeeRepository.Login(
                model.UserName,
                model.Password
            );

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Incorrect username or password.";

                return View(model);
            }

            HttpContext.Session.SetInt32(
                "EmployeeId",
                employee.id
            );

            HttpContext.Session.SetString(
                "EmployeeName",
                employee.name ?? ""
            );

            HttpContext.Session.SetString(
                "EmpCode",
                employee.empcode ?? ""
            );

            HttpContext.Session.SetString(
                "EmailId",
                employee.email ?? ""
            );

            HttpContext.Session.SetString(
                "UserName",
                employee.UserName ?? ""
            );

            HttpContext.Session.SetString(
                "Role",
                employee.role ?? ""
            );

            TempData["SuccessMessage"] =
                "Login successful! Welcome " + employee.name;

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction("Index", "Login");
        }
    }
}