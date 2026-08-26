using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;

namespace WEBLINK_CRM.Controllers
{
    public class UserMasterController : Controller
    {
        private readonly IUserRepository _repository;
        public UserMasterController(IUserRepository repository)
        {
            _repository = repository;
        }

        // Helper method for dropdown
        private void BindSalesTLList(RegisterUserr model)
        {
            model.SalesTLList = _repository.GetSalesTLManagers();
        }

        // ================= INDEX =================

        public IActionResult Index()
        {
            var users = _repository.GetAllUsers();

            return View(users);
        }

        // ================= CREATE GET =================

        [HttpGet]
        public IActionResult Create()
        {

            RegisterUserr model = new RegisterUserr();

            BindSalesTLList(model);

            return View(model);
        }

        // ================= CREATE POST =================

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

        // ================= EDIT GET =================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _repository.GetUserById(id);

            if (user == null)
                return NotFound();

            user.SalesTLList = _repository.GetSalesTLManagers();

            return View(user);
        }

        // ================= EDIT POST =================

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
        // ================= DELETE =================

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

    }
}
