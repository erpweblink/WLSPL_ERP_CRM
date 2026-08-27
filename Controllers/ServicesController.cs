using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;

namespace WEBLINK_CRM.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly IServicesRepo _services;

        public ServicesController(IServicesRepo services)
        {
            _services = services;
        }


        public async Task<IActionResult> Index()
        {
            var servicesList = await _services.GetServices(
                new Services(),
                "GetServicesList"
            );

            return View(servicesList);
        }
        [HttpGet]
        public async Task<IActionResult> GetDepratments()
        {
            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Department model = new Department();

            var departments = await _services.Getdepartments(
                model,
                "Get"
            );

            ViewBag.Departments = departments;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Services model)
        {
            try
            {

                string userName = HttpContext.Session.GetString("userName");

                model.CreatedBy = userName;


                var result = await _services.SubmitServices(model, "Insert");


                if (result == -1)
                {
                    TempData["ToastMessage"] = "Service already exists.";
                    TempData["ToastType"] = "warning";
          

                    return RedirectToAction("Index", "Services");
                }


                if (result > 0)
                {
                    TempData["ToastMessage"] = "Service created successfully.";
                    TempData["ToastType"] = "success";
              
                    return RedirectToAction("Index", "Services");
                }

                TempData["ToastMessage"] = "Unable to create service.";
                TempData["ToastType"] = "error";
             

                return View("Create", model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Something went wrong while creating the service.";
                TempData["ToastType"] = "error";            

                return View("Create", model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string ID)
        {
            try
            {
                Department model = new Department();

                var departments = await _services.Getdepartments(
                    model,
                    "Get"
                );

                ViewBag.Departments = departments;
                var services = await _services.GetServicesById(ID);
                if (services == null)
                {
                    return View("Error", new { message = "service not found" });
                }

                return View(services);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string userName = HttpContext.Session.GetString("userName");

                var result = await _services.DeleteServices(
                    id.ToString(),
                    userName
                );

                if (result > 0)
                {
                    TempData["ToastMessage"] = "Service deleted successfully.";
                    TempData["ToastType"] = "success";
                 
                }
                else
                {
                    TempData["ToastMessage"] = "Unable to delete service.";
                    TempData["ToastType"] = "error";
       
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Something went wrong while deleting the service.";
                TempData["ToastType"] = "error";            

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Services model)
        {
            try
            {

                string userName = HttpContext.Session.GetString("userName");

                model.CreatedBy = userName;


                var result = await _services.SubmitServices(model, "Update");


                if (result == -1)
                {
                    TempData["ToastMessage"] = "Service Updated exists.";
                    TempData["ToastType"] = "warning";
             
                    return RedirectToAction("Index", "Services");
                }


                if (result > 0)
                {
                    TempData["ToastMessage"] = "Service Updated successfully.";
                    TempData["ToastType"] = "success";
                   

                    return RedirectToAction("Index", "Services");
                }

                TempData["ToastMessage"] = "Unable to Update service.";
                TempData["ToastType"] = "error";
         

                return View("Create", model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Something went wrong while Updating the service.";
                TempData["ToastType"] = "error";           

                return View("Create", model);
            }
        }


    }
}
