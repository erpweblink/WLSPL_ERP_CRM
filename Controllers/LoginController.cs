using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employee = _employeeRepository.Login(model.UserName, model.Password);

            if (employee == null)
            {
                ViewBag.ErrorMessage = "Incorrect username or password.";
                return View(model);
            }

            // ============================
            // SESSION VALUES
            // ============================

            HttpContext.Session.SetInt32("EmployeeId", employee.id);
            HttpContext.Session.SetString("EmployeeName", employee.name ?? "");
            HttpContext.Session.SetString("EmpCode", employee.empcode ?? "");
            HttpContext.Session.SetString("EmailId", employee.email ?? "");
            HttpContext.Session.SetString("UserName", employee.UserName ?? "");
            HttpContext.Session.SetString("Role", employee.role ?? "");


            // ============================
            // COOKIE AUTHENTICATION
            // ============================

            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            employee.id.ToString()
        ),

        new Claim(
            ClaimTypes.Name,
            employee.UserName ?? ""
        ),

        new Claim(
            ClaimTypes.Email,
            employee.email ?? ""
        ),

        new Claim(
            ClaimTypes.Role,
            employee.role ?? ""
        ),

        new Claim(
            "EmployeeName",
            employee.name ?? ""
        ),

        new Claim(
            "EmpCode",
            employee.empcode ?? ""
        )
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );


            // ============================
            // SUCCESS MESSAGE
            // ============================

            ViewBag.LoginSuccess =
                "Login successful! Welcome, " + employee.name + ".";

            ViewBag.RedirectUrl =
                Url.Action("Index", "Dashboard");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            HttpContext.Session.Clear();

            TempData["LogoutMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction(
                "Index",
                "Login"
            );
        }
    }
}