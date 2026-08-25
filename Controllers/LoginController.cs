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
                return View(model);

            var employee = _employeeRepository.Login(model.UserName, model.Password);

            if (employee == null)
            {
                ViewBag.ErrorMessage = "Incorrect username or password.";
                return View(model);
            }

            HttpContext.Session.SetInt32("EmployeeId", employee.id);
            HttpContext.Session.SetString("EmployeeName", employee.name ?? "");
            HttpContext.Session.SetString("EmpCode", employee.empcode ?? "");
            HttpContext.Session.SetString("EmailId", employee.email ?? "");
            HttpContext.Session.SetString("UserName", employee.UserName ?? "");
            HttpContext.Session.SetString("Role", employee.role ?? "");

            // ✅ ViewBag — safe to read multiple times in same view
            ViewBag.LoginSuccess = "Login successful! Welcome, " + employee.name + ".";
            ViewBag.RedirectUrl = Url.Action("Index", "Dashboard");

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Login");
        }
    }
}