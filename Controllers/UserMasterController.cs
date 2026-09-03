using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WEBLINK_CRM.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
    public class UserMasterController : Controller
    {
        private readonly IUserRepository _repository;
        public UserMasterController(IUserRepository repository)
        {
            _repository = repository;
        }

        private void BindSalesTLList(RegisterUserr model)
        {
            model.SalesTLList = _repository.GetSalesTLManagers();
        }

        public IActionResult Index()
        {
            var users = _repository.GetAllUsers();

            var managers = users
                .Where(u => u.Sales_TL_Manager == true && u.status == true)
                .Select(u => new { UserCode = u.empcode, FullName = u.name })
                .ToList();

            ViewBag.SalesManagers = managers;
            return View(users);
        }

        [HttpPost]
        public IActionResult GetFilteredUsers([FromBody] UserFilterDto filter)
        {
            try
            {
                var data = _repository.GetFilteredUsers(
                    filter.ManagerEmpCode, filter.Status, filter.Search);

                var result = data.Select(u => new {
                    u.id,
                    u.empcode,
                    u.name,
                    u.email,
                    u.mobile,
                    u.role,
                    u.status,
                    u.UserName,
                    u.TL_Manager,
                    u.Sales_TL_Manager,
                    u.Level,
                    u.Path
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {

            RegisterUserr model = new RegisterUserr();

            BindSalesTLList(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RegisterUserr model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(
                            item.Key + " : " + error.ErrorMessage
                        );
                    }
                }

                model.SalesTLList = _repository.GetSalesTLManagers();

                return View(model);
            }

            bool result = _repository.CreateUser(model);

            if (result)
            {
                TempData["ToastMessage"] = "User saved successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index");
            }


            model.SalesTLList = _repository.GetSalesTLManagers();

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _repository.GetUserById(id);

            if (user == null)
                return NotFound();

            user.SalesTLList = _repository.GetSalesTLManagers();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RegisterUserr model)
        {
            if (ModelState.IsValid)
            {
                bool result = _repository.UpdateUser(model);

                if (result)
                {
                    TempData["ToastMessage"] = "User updated successfully.";
                    TempData["ToastType"] = "success";

                    return RedirectToAction("Index");
                }
            }

            // Reload dropdown if validation fails
            model.SalesTLList = _repository.GetSalesTLManagers();

            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            bool result = _repository.DeleteUser(id);

            if (result)
            {
                TempData["ToastMessage"] = "User deleted successfully.";
                TempData["ToastType"] = "success";

            }
            else
            {
                TempData["ToastMessage"] = "Unable to delete user.";
                TempData["ToastType"] = "error";

            }

            return RedirectToAction(nameof(Index));

        }


        [HttpGet]
        public IActionResult UserProfile()
        {
            int userId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            var user = _repository.GetUserById(userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult UserProfile(RegisterUserr model)
        {
            var user = _repository.UpdateUserProfile(model);

            if (user)
            {
                TempData["ToastMessage"] = "Profile updated successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction("UserProfile");
            }

            TempData["ToastMessage"] = "Unable to updated Profile.";
            TempData["ToastType"] = "error";

            return RedirectToAction("UserProfile");
        }

        [HttpPost]
        public IActionResult UpdateAvatar(string avatarUrl, IFormFile avatarFile)
        {
            // alter employee table to add profile image path 
            //Alter table employees ADD ProfileImagePath Nvarchar(MAX) null

            string savedPath = null;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/users/uploads");
                Directory.CreateDirectory(uploads);

                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    avatarFile.CopyTo(stream);

                savedPath = $"/assets/images/users/uploads/{fileName}";
            }
            else if (!string.IsNullOrEmpty(avatarUrl))
            {
                savedPath = avatarUrl;
            }

            if (savedPath != null)
            {
                int Id = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
                var user = _repository.UpdateUserAvatar(Id, savedPath);
                if (user)
                {
                    HttpContext.Session.SetString("Profile", savedPath);
                }
            }

            TempData["ToastMessage"] = "Profile picture updated successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction("UserProfile");
        }
    }
}
