using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;

namespace WEBLINK_CRM.Controllers
{
    [Authorize]
    public class CallAndMeetingController : Controller
    {
        private readonly ICallandMeetingRepo _callmeetmaster;
        public CallAndMeetingController(ICallandMeetingRepo callmeetmaster)
        {
            _callmeetmaster = callmeetmaster;
        }

        //Call and Meeting Creation functions
        [HttpGet]
        public IActionResult Create()
        {
            var companyData = new Company();
            if (TempData["CompanyData"] != null)
            {
                companyData = Newtonsoft.Json.JsonConvert
                     .DeserializeObject<Company>(TempData["CompanyData"].ToString());
            }

            ViewBag.DashboardCompanyDetails = companyData;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCallAndMeeting(CallandMeeting model)
        {
            try
            {
                model.CreatedBy = HttpContext.Session.GetString("EmpCode")?.ToString();
                model.FromMail = HttpContext.Session.GetString("EmailId")?.ToString();

                int data = await _callmeetmaster.SubmitDetails(model);
                if (data > 0)
                {
                    TempData["ToastMessage"] = "Call and Meeting saved successfully.";
                    TempData["ToastType"] = "success";

                    return RedirectToAction("Create");
                }

                TempData["ToastMessage"] = "Unable to save Call and Meeting.";
                TempData["ToastType"] = "error";

                return RedirectToAction("Create");
            }
            catch (Exception )
            {
                TempData["ToastMessage"] = "Something went wrong. Please try again.";
                TempData["ToastType"] = "error";

                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyName(string Name, string actionby)
        {
            var data = await _callmeetmaster.GetcompanyName(Name, actionby);

            var result = data.Select(c => new
            {
                value = c.cname,
                code = c.ccode,
                companyCreatedby = c.UserName,
                companyUserName = c.name
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyDetails(string companyCode)
        {
            var data = await _callmeetmaster.GetcompanybyId(companyCode);
            return Json(data);
        }


        //Call and Meeting Report functions
        public async Task<IActionResult> FollowUpReport()
        {
            string SessionName = HttpContext.Session.GetString("EmpCode")?.ToString()?? "NA";
            string SessionRole = HttpContext.Session.GetString("Role")?.ToString() ?? "NA";

            var result = await _callmeetmaster.List(SessionName);
            var personLists = await _callmeetmaster.GetSalesPersonList(SessionName, SessionRole);

            ViewBag.SalesManagers = personLists.SalesManagers;
            ViewBag.MeetingWithManagers = personLists.MeetingWithManagers;

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetFilteredReport([FromBody] FollowUpFilterModel filter)
        {
            try
            {
                filter.EmpCode = HttpContext.Session.GetString("EmpCode") ?? "NA";
                filter.Role = HttpContext.Session.GetString("Role") ?? "NA";

                var result = await _callmeetmaster.GetFilteredReport(filter);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load filtered report." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRemarks(string id, string remarks)
        {
            //BY Nikhil 
            //ALTER TABLE dbo.CommentHistory ADD AdminRemark Nvarchar(MAX) null
            //Run this  query to get adminremark and then add the field in [stswlspl].[VW_FollowUpRpt]
            //And also add it in [stswlspl].[SP_CallMeetingReport] 

            string role = HttpContext.Session.GetString("Role") ?? "NA";

            if (role != "Admin" && role != "Sub Admin")
            {
                return Json(new { success = false, message = "You are not authorized to edit this." });
            }

            try
            {
                bool updated = await _callmeetmaster.UpdateRemarks(id, remarks);
                if (!updated)
                {
                    return Json(new { success = false, message = "No matching record found to update." });
                }
                return Json(new { success = true, remarks = remarks });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to update remarks." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CloseFollowUpClick(int id)
        {
            try
            {
                var data = await _callmeetmaster.GetCommentHistoryById(id);
                var res =  await _callmeetmaster.GetHirechyWiseUser();

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
                var result = await _callmeetmaster.UpdateCompanyCreatedByName(newName, CompCode, SessionName);

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
            int UpdateData = await _callmeetmaster.UpdateOldCommentHistory(model.Id);
            if (UpdateData == 1)
            {
                model.Id = 0;
                var result = await _callmeetmaster.FromListSubmitCommentHistory(model);
                return Json(new { success = true, message = "Call and Meeting Updated successfully.." });
            }
            else
            {
                return Json(new { success = false, message = "Something went worng..." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCommmentHistory(string CompanyCode)
        {
            try
            {
                var comments = await _callmeetmaster.GetCommentHistoryList(CompanyCode);

                if (comments == null) return Json(new { success = false, message = "Comment details not found" });

                return Json(new { success = true, comments = comments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        public IActionResult NotUpdateIndex()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotUpdatedList(string Days)
        {
            try
            {
                var comments = await _callmeetmaster.GetNotUpdatedList(Days);
                var res = await _callmeetmaster.GetHirechyWiseUser();

                if (comments == null) return Json(new { success = false, message = "Comment details not found" });

                return Json(new { success = true, comments = comments, userList = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
    }
}


