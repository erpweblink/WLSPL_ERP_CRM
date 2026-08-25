using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;


namespace WEBLINK_CRM.Controllers
{
    public class CompanymasterController : Controller
    {
        private readonly IcomapnymasterRepo _companymaster;
        private readonly IGoveServices _govServices;
        public CompanymasterController(IcomapnymasterRepo CompanymasterRepo, IGoveServices govServices)
        {
            _companymaster = CompanymasterRepo;
            _govServices = govServices;
        }
        // Company List
        public async Task<IActionResult> Index(Companymaster companymaster)
        {
            var companyList = await _companymaster.GetcompanyList(
                companymaster,
                "GetCompanyMasterList"
            );

            return View(companyList);
        }



        // Create Page
        [HttpGet]
        public IActionResult Create()
        {
            Companymaster model = new Companymaster();

            // Generate Company Code
            model.CCode = GenerateCompanyCode();

            return View(model);
        }



        private string GenerateCompanyCode()
        {
            string prefix = "WLS";

            int number = 1;

            // Example:
            // WLS00001

            return prefix + number.ToString("00000");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Companymaster model)
        {
            try
            {

                string userName = HttpContext.Session.GetString("userName");

                model.CreatedBy = userName;

                var result = await _companymaster.SubmitDetails(model, "Insert");


                if (result == -1)
                {
                    TempData["Save_Record"] = "Company already exists.";
                    TempData["icon"] = "warning";
                    TempData["Time"] = "2000";

                    //return RedirectToAction("IndexLogin", "Alert");

                    return RedirectToAction("Index", "Companymaster");
                }


                if (result > 0)
                {
                    TempData["Save_Record"] = "Company Created successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";

                    //return RedirectToAction("IndexLogin", "Alert");
                    return RedirectToAction("Index", "Companymaster");
                }


                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> GetGSTDetails(string gstNo)
        {
            if (string.IsNullOrWhiteSpace(gstNo) || gstNo.Length != 15)
                return Json(new { success = false, message = "Invalid GST number" });

            try
            {
                var jo = await _govServices.GetGSTDetailsAsync(gstNo);

                if (jo["status_cd"]?.ToString() == "1")
                {
                    var data = jo["data"];
                    return Json(new
                    {
                        success = true,
                        tradeName = data["TradeName"]?.ToString(),
                        address = $"{data["AddrBnm"]} {data["AddrBno"]} {data["AddrFlno"]} {data["AddrSt"]} {data["AddrLoc"]}",
                        location = data["AddrLoc"]?.ToString(),
                        pincode = data["AddrPncd"]?.ToString(),
                        stateCode = data["StateCode"]?.ToString()

                    });
                }

                return Json(new { success = false, message = jo["status_desc"]?.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        string userName = HttpContext.Session.GetString("userName");

        //        var result = await _companymaster.DeleteReord(
        //            id.ToString(),
        //            userName
        //        );

        //        if (result > 0)
        //        {
        //            TempData["Save_Record"] = "Company deleted successfully.";
        //            TempData["icon"] = "success";
        //            TempData["Time"] = "2000";
        //        }
        //        else
        //        {
        //            TempData["Save_Record"] = "Unable to delete company.";
        //            TempData["icon"] = "error";
        //            TempData["Time"] = "2000";
        //        }

        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Save_Record"] = ex.Message;
        //        TempData["icon"] = "error";
        //        TempData["Time"] = "2000";

        //        return RedirectToAction("Index");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string userName = HttpContext.Session.GetString("userName");

                var result = await _companymaster.DeleteReord(
                    id.ToString(),
                    userName
                );

                if (result > 0)
                {
                    TempData["Save_Record"] = "Company deleted successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";
                }
                else
                {
                    TempData["Save_Record"] = "Unable to delete company.";
                    TempData["icon"] = "error";
                    TempData["Time"] = "2000";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Save_Record"] = ex.Message;
                TempData["icon"] = "error";
                TempData["Time"] = "2000";

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string ID)
        {
            try
            {
                var Companymaster = await _companymaster.GetcompanybyId(ID);
                if (Companymaster == null)
                {
                    return View("Error", new { message = "company not found" });
                }

                return View(Companymaster);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Companymaster model)
        {
            try
            {

                string userName = HttpContext.Session.GetString("userName");

                model.CreatedBy = userName;


                var result = await _companymaster.SubmitDetails(model, "UpdateCompany");


                if (result == -1)
                {
                    TempData["Save_Record"] = "Company already exists.";
                    TempData["icon"] = "warning";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Companymaster");

                }


                if (result > 0)
                {
                    TempData["Save_Record"] = "Company Created successfully.";
                    TempData["icon"] = "success";
                    TempData["Time"] = "2000";

                    return RedirectToAction("Index", "Companymaster");

                }


                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
