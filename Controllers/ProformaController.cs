using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;


namespace WEBLINK_CRM.Controllers
{
    [Authorize]
    public class ProformaController : Controller
    {
        private readonly IProforma objProforma;
        public ProformaController(IProforma proforma)
        {
            objProforma = proforma;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string pageSize = "10";
            var list = await objProforma.GetProformaList(pageSize);
            return View(list);

        }

        [HttpPost]
        public async Task<IActionResult> GetDetailsById(string ID)
        {
            try
            {
                var list = await objProforma.GetDetailsById(ID);

                return Json(new { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int ID)
        {
            try
            {
                if (ID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Proforma ID."
                    });
                }

                var result = await objProforma.Delete(ID);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Proforma deleted successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Proforma could not be deleted."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] VM_Proforma DataList)
        {
            try
            {

                var loginId = HttpContext.Session.GetString("EmployeeId");

                if (DataList == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }
                DataList.CreatedBy = loginId;
                int ID = await objProforma.Save(DataList);

                return Json(new
                {
                    success = true,
                    ID = ID,
                    Message = "Proforma saved successfully."
                });


            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> GetCompany(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetCompanyList(Status);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetCompanyByCode(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetCompanyByCode(ID);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetProformaDataById(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var loginId = HttpContext.Session.GetString("EmployeeId");

                if (string.IsNullOrEmpty(loginId))
                {
                    return RedirectToAction("Login", "Login");
                }

                // IMPORTANT: await async methods
                var list = await objProforma.GetProformaById(ID);
                var Dtlslist = await objProforma.GetDetailsById(ID);

                if (list == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Work Order not found"
                    });
                }
                var result = new
                {
                    proformaHdr = list,
                    proformaDtls = Dtlslist
                };

                return Json(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    Success = false,
                    Message = e.Message
                });
            }
        }

        public async Task<IActionResult> GetState(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetStateList(Status);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewPDF(string ID)
        {
            var decryptedId = int.Parse(ID);
            byte[] pdfBytes = objProforma.ProformaPdf(decryptedId);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return Json(new { success = false, message = "No data found for this Proforma." });
            }

            string base64Pdf = Convert.ToBase64String(pdfBytes);

            return Json(new
            {
                success = true,
                fileName = $"Proforma_{decryptedId}.pdf",
                fileData = base64Pdf
            });
        }

    }
}
