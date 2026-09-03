using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;


namespace WEBLINK_CRM.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
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
            string EmpCode = HttpContext.Session.GetString("EmpCode");
            var personLists = await _companymaster.GetHirechyEmployees(EmpCode);

            ViewBag.SalesManagers = personLists;

            return View(companyList);
        }

        [HttpPost]
        public async Task<IActionResult> GetFilteredCompanies([FromBody] JsonElement filter)
        {
            try
            {
                string empCode = HttpContext.Session.GetString("EmpCode");

                var companymaster = new Companymaster
                {
                    SessionName = filter.TryGetProperty("SessionName", out var sm)? sm.GetString() : null,
                    typess = filter.TryGetProperty("typess", out var ts) ? ts.GetString() : null,
                    CName = filter.TryGetProperty("CName", out var cn) ? cn.GetString() : null,
                    empcode = empCode
                };

                var companyList = await _companymaster.GetFilteredcompanyList(companymaster);

                var data = companyList.Select(c => new {
                    id = c.Id,
                    cCode = c.CCode,
                    cName = c.CName,
                    email = c.Email,
                    mobile = c.Mobile,
                    gstNo = c.GSTNo,
                    typess = c.typess
                });

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message + " | " + ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CloseFollowUpClick(int id)
        {
            try
            {
                var data = await _companymaster.GetCommentHistoryById(id);
                var res = await _companymaster.GetActiveEmployeeList();

                if (data == null)
                {
                    return Json(new { success = false, message = "Company details not found" });
                }

                return Json(new { success = true, commentId = data, userList = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCommmentHistory(string CompanyCode)
        {
            try
            {
                var comments = await _companymaster.GetCommentHistoryList(CompanyCode);

                if (comments == null) return Json(new { success = false, message = "Comment details not found" });

                return Json(new { success = true, comments = comments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompanyUser(string newName, string CompCode)
        {
            try
            {
                if (string.IsNullOrEmpty(newName) || newName == "NoName")
                {
                    return Json(new { success = false, message = "Invalid name" });
                }
                string SessionName = HttpContext.Session.GetString("EmpCode")?.ToString() ?? "NA";
                var result = await _companymaster.UpdateCompanyCreatedByName(newName, CompCode, SessionName);

                return Json(new { success = true, message = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCompanyUpdate([FromBody] CallandMeeting model)
        {
            model.CreatedBy = HttpContext.Session.GetString("EmpCode")?.ToString();
            model.FromMail = HttpContext.Session.GetString("EmailId")?.ToString();
            if (model == null)
                return Json(new { success = false, message = "Invalid data" });

            if (!string.IsNullOrEmpty(model.MeetingTimeView))
            {
                string[] formats = { "HH:mm", "h:mm tt" };
                if (TimeOnly.TryParseExact(model.MeetingTimeView, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    model.MeetingTime = time;
                }
                else
                {
                    model.MeetingTime = null;
                }
            }
            int UpdateData = await _companymaster.UpdateOldCommentHistory(model.Id);
            if (UpdateData == 1)
            {
                model.Id = 0;
                var result = await _companymaster.FromListSubmitCommentHistory(model);
                return Json(new { success = true, message = "Call and Meeting Updated successfully.." });
            }
            else
            {
                return Json(new { success = false, message = "Something went worng..." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? leadCode,string? mobile,string? email,string? ownerName)
        {
            var model = new Companymaster();

            model.CCode = GenerateCompanyCode();

            // Get BDE list
            var result = await _companymaster.GetBDE("GetBDETME");

            // Inquiry values
            model.LeadCode = leadCode;
            model.Mobile = mobile;
            model.Email = email;
            model.OName = ownerName;

            // Find requested person in BDE list
            var bde = result.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.name) &&
                !string.IsNullOrWhiteSpace(ownerName) &&
                x.name.Trim().Equals(
                    ownerName.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            // Automatically select matching BDE
            model.BDE = bde?.name;

            // Send BDE list to View
            model.SalesPersons = result;

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

                string userName = HttpContext.Session.GetString("UserName");

                model.CreatedBy = userName;

                var result = await _companymaster.SubmitDetails(model, "Insert");


                if (result == -1)
                {
                    TempData["ToastMessage"] = "Company already exists.";
                    TempData["ToastType"] = "warning";

                    return RedirectToAction("Index", "Companymaster");
                }


                if (result > 0)
                {
                    TempData["ToastMessage"] = "Company Created successfully.";
                    TempData["ToastType"] = "success";

                    return RedirectToAction("Index", "Companymaster");
                }


                return View(model);
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Something went wrong.";
                TempData["ToastType"] = "error";
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string userName = HttpContext.Session.GetString("UserName");

                var result = await _companymaster.DeleteReord(
                    id.ToString(),
                    userName
                );

                if (result > 0)
                {
                    TempData["ToastMessage"] = "Company deleted successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {

                    TempData["ToastMessage"] = "Unable to delete company.";
                    TempData["ToastType"] = "error";
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Something went wrong..";
                TempData["ToastType"] = "error";
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string ID)
        {
            try
            {
                var companymaster = await _companymaster.GetcompanybyId(ID);

                if (companymaster == null)
                {
                    return View("Error");
                }

                var bdeList = await _companymaster.GetBDE("GetBDETME");

                ViewBag.BDEList = bdeList;

                return View(companymaster);
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

                string userName = HttpContext.Session.GetString("UserName");

                model.CreatedBy = userName;


                var result = await _companymaster.SubmitDetails(model, "UpdateCompany");


                if (result == -1)
                {
                    TempData["ToastMessage"] = "Company already exists.";
                    TempData["ToastType"] = "warning";
           
                    return RedirectToAction("Index", "Companymaster");

                }


                if (result > 0)
                {
                    TempData["ToastMessage"] = "Company Created successfully.";
                    TempData["ToastType"] = "success";          

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
