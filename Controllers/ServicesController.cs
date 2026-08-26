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
                    TempData["Save_Record"] = "Service already exists.";
                    TempData["icon"] = "warning";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Services");
                }


                if (result > 0)
                {
                    TempData["Save_Record"] = "Service created successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Services");
                }

                // Insert failed
                TempData["Save_Record"] = "Unable to create service.";
                TempData["icon"] = "error";
                TempData["Time"] = "2000";

                return View("Create", model);
            }
            catch (Exception ex)
            {
                // Ideally log the exception
                TempData["Save_Record"] = "Something went wrong while creating the service.";
                TempData["icon"] = "error";
                TempData["Time"] = "3000";

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
                    TempData["Save_Record"] = "Service deleted successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";
                }
                else
                {
                    TempData["Save_Record"] = "Unable to delete service.";
                    TempData["icon"] = "error";
                    TempData["Time"] = "2000";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Save_Record"] = "Something went wrong while deleting the service.";
                TempData["icon"] = "error";
                TempData["Time"] = "3000";

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
                    TempData["Save_Record"] = "Service Updated exists.";
                    TempData["icon"] = "warning";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Services");
                }


                if (result > 0)
                {
                    TempData["Save_Record"] = "Service Updated successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Services");
                }

                // Insert failed
                TempData["Save_Record"] = "Unable to Update service.";
                TempData["icon"] = "error";
                TempData["Time"] = "2000";

                return View("Create", model);
            }
            catch (Exception ex)
            {
                // Ideally log the exception
                TempData["Save_Record"] = "Something went wrong while Updating the service.";
                TempData["icon"] = "error";
                TempData["Time"] = "3000";

                return View("Create", model);
            }
        }


    }
}
