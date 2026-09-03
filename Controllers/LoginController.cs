using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Security.Claims;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.Repositories;

namespace WEBLINK_CRM.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IConfiguration _config;

        public LoginController(IEmployeeRepository employeeRepository, IConfiguration config)
        {
            _employeeRepository = employeeRepository;
            _config = config;
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

            HttpContext.Session.SetInt32("EmployeeId", employee.id);
            HttpContext.Session.SetString("EmployeeName", employee.name ?? "");
            HttpContext.Session.SetString("EmpCode", employee.empcode ?? "");
            HttpContext.Session.SetString("EmailId", employee.email ?? "");
            HttpContext.Session.SetString("UserName", employee.UserName ?? "");
            HttpContext.Session.SetString("Role", employee.role ?? "");
            HttpContext.Session.SetString("Profile", employee.ProfileImagePath ?? "");

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

            TempData["ToastMessage"] = "Login successful! Welcome, " + employee.name + ".";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            HttpContext.Session.Clear();

            Response.Cookies.Delete(".AspNetCore.Cookies");

            TempData["LogoutMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction(
                "Index",
                "Login"
            );
        }

        private const string SessKey_Otp = "FP_OTP";
        private const string SessKey_Email = "FP_EMAIL";
        private const string SessKey_EmpId = "FP_EMPID";
        private const string SessKey_Expiry = "FP_EXPIRY";
        private const string SessKey_Verified = "FP_VERIFIED";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CheckEmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { exists = false });

            var employee = _employeeRepository.GetByEmail(email.Trim());
            return Json(new { exists = employee != null });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendOtp(string email)
        {
            var employee = _employeeRepository.GetByEmail(email?.Trim());
            if (employee == null)
                return Json(new { success = false, message = "No account found with this email address." });

            string otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString(SessKey_Otp, otp);
            HttpContext.Session.SetString(SessKey_Email, email.Trim());
            HttpContext.Session.SetInt32(SessKey_EmpId, employee.id);
            HttpContext.Session.SetString(SessKey_Expiry, DateTime.Now.AddMinutes(10).ToString("o"));
            HttpContext.Session.SetString(SessKey_Verified, "false");

            try
            {
                SendOtpEmail(email.Trim(), otp);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Could not send OTP email: " + ex.Message });
            }

            return Json(new { success = true, message = "OTP sent to your email." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyOtp(string email, string otp)
        {
            var sessionEmail = HttpContext.Session.GetString(SessKey_Email);
            var sessionOtp = HttpContext.Session.GetString(SessKey_Otp);
            var expiryStr = HttpContext.Session.GetString(SessKey_Expiry);

            if (sessionEmail == null || sessionOtp == null || expiryStr == null || sessionEmail != email?.Trim())
                return Json(new { success = false, message = "Please request a new OTP." });

            if (DateTime.Now > DateTime.Parse(expiryStr))
                return Json(new { success = false, message = "This OTP has expired. Please request a new one." });

            if (sessionOtp != otp)
                return Json(new { success = false, message = "The OTP you entered is incorrect." });

            HttpContext.Session.SetString(SessKey_Verified, "true");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            var sessionEmail = HttpContext.Session.GetString(SessKey_Email);
            var verified = HttpContext.Session.GetString(SessKey_Verified);
            var empId = HttpContext.Session.GetInt32(SessKey_EmpId);

            if (sessionEmail == null || sessionEmail != email?.Trim() || verified != "true" || empId == null)
                return Json(new { success = false, message = "OTP verification is required before resetting the password." });

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
                return Json(new { success = false, message = "Password must be at least 4 characters." });

            if (newPassword != confirmPassword)
                return Json(new { success = false, message = "Password and Confirm Password do not match." });

            // empId came from Session (set during CheckEmailExists/SendOtp), never from client input
            _employeeRepository.UpdatePassword(empId.Value, newPassword);

            HttpContext.Session.Remove(SessKey_Otp);
            HttpContext.Session.Remove(SessKey_Email);
            HttpContext.Session.Remove(SessKey_EmpId);
            HttpContext.Session.Remove(SessKey_Expiry);
            HttpContext.Session.Remove(SessKey_Verified);

            return Json(new { success = true, message = "Your password has been updated. Please sign in." });
        }

        private void SendOtpEmail(string toEmail, string otp)
        {
            var smtpHost = _config["MailSettings:Host"];
            var smtpPort = int.Parse(_config["MailSettings:Port"]);
            var smtpUser = _config["MailSettings:MailUserName"];
            var smtpPass = _config["MailSettings:MailUserPass"];
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpUser, smtpUser));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Your Password Reset OTP";
            message.Body = new TextPart("plain")
            {
                Text = "Your OTP for resetting your WEB LINK CRM password is: " + otp +
                       "\n\nThis code expires in 10 minutes. If you did not request this, please ignore this email."
            };

            using var client = new MailKit.Net.Smtp.SmtpClient(); // fully qualified — avoids clash with System.Net.Mail.SmtpClient
            client.Connect(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(smtpUser, smtpPass);
            client.Send(message);
            client.Disconnect(true);
        }
    }

}