using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WEBLINK_CRM.Helpers;
using WLSPL_ERP_CRM.Models;
using WLSPL_ERP_CRM.repository;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

/* Things to do when creating invoice 
   1.Alter table InvoiceMain add columns AgainstBy Nvarchar(500) null and AgainstByValue Nvarchar(500) null
   2.Alter table invoicedetails add column ServiceName Nvarchar(500) null and ServiceId nvarchar(500) null and ValidateTill nvarchar(500) null 
   3.ADD parameters in [dbo].[SP_AddInvoice] for InvoiceMain  @AgainstBy nvarchar(MAX) = null,
        @AgainstByValue nvarchar(MAX) = null, @TotalPayable nvarchar(MAX) = null, @TdsPer nvarchar(MAX) = null,
        @TdsAmt nvarchar(MAX) = null,@companyCode nvarchar(MAX) = null, @Remarks nvarchar(MAX) = null,
        @newinvoiceno nvarchar(900) = null output
   4.Alter table InvoiceMain add TotalPayable nvarchar(max) null, TdsPer nvarchar(max) null, TdsAmt nvarchar(max) null
   5.Alter table InvoiceMain add compCode nvarchar(max) null,UploadedFilePath nvarchar(max) null
 */

namespace WLSPL_ERP_CRM.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
    public class TaxinvoiceController : Controller
    {
        private readonly ITaxinvoiceRepo _TaxinvoiceRepo;
        private readonly IWebHostEnvironment _env;

        public TaxinvoiceController(ITaxinvoiceRepo taxinvoiceRepo, IWebHostEnvironment env)
        {
            _TaxinvoiceRepo = taxinvoiceRepo;
            _env = env;
        }

        public async Task<IActionResult> Index(string? financialYear, int? month, string? salesManager)
        {
            try
            {
                string SessionName = HttpContext.Session.GetString("EmpCode")?.ToString() ?? "NA";
                string SessionRole = HttpContext.Session.GetString("Role")?.ToString() ?? "NA";

                var today = DateTime.Now;

                // ============================================
                // DEFAULT FINANCIAL YEAR
                // ============================================

                if (string.IsNullOrEmpty(financialYear))
                {
                    financialYear = today.Month >= 4
                        ? $"{today.Year}-{(today.Year + 1).ToString().Substring(2)}"
                        : $"{today.Year - 1}-{today.Year.ToString().Substring(2)}";
                }

                if (!month.HasValue)
                {
                    month = today.Month;
                }


                var data = await _TaxinvoiceRepo.GetInfo(financialYear, month, salesManager, SessionName, SessionRole);

                var financialYearSummary = await _TaxinvoiceRepo.GetFinancialYearSummary(financialYear, salesManager, SessionName, SessionRole);


                var personLists = await _TaxinvoiceRepo.GetSalesPersonList(SessionName, SessionRole);

                ViewBag.SalesManagers = personLists.SalesManagers;
                ViewBag.SelectedSalesManager = salesManager;
                ViewBag.SelectedFinancialYear = financialYear;
                ViewBag.SelectedMonth = month;
                ViewBag.FinancialYearSummary = financialYearSummary;

                return View(data);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;

                return View("Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create(string? ProformaId)
        {
            var companies = await _TaxinvoiceRepo.Getcompany();

            TaxInvoiceCreateVM model;

            if (!string.IsNullOrEmpty(ProformaId))
            {
                model = await _TaxinvoiceRepo.GetProformaDetails(ProformaId)
                        ?? new TaxInvoiceCreateVM();
            }
            else
            {
                var invoiceMain = await _TaxinvoiceRepo.Getinvoiceno()
                                  ?? new TaxInvoiceCreate();

                model = new TaxInvoiceCreateVM
                {
                    main = invoiceMain,
                    details = new List<InvoiceDetails>()
                };
            }

            // Always attach companies
            model.companies = companies ?? new List<TaxInvoiceCreate>();

            if (model.main.invoicedate == null)
                model.main.invoicedate = DateTime.Today;

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveInvoice([FromBody] TaxInvoiceCreateVM model)
        {
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var result = await _TaxinvoiceRepo.UpdateSave(model, Action : "insert");

            await _TaxinvoiceRepo.SaveInvoiceChangeHistory(
                      sessionName: model.main.sessionname ?? "System",
                      invoiceNo: model.main.compCode ?? model.main.Id,
                      message: $"Invoice {model.main.invoiceno} created on {DateTime.Now:dd-MMM-yyyy HH:mm} ."
                  );

            return Json(new { success = true, invoiceNo = model.main.invoiceno });
        }
 
        [HttpGet]
        public async Task<IActionResult> Getcomapnybycname(string cname)
        {
            if (string.IsNullOrWhiteSpace(cname))
            {
                return BadRequest("Company name is required.");
            }

            var result = await _TaxinvoiceRepo.Getcompanybycname(cname);

            if (result == null)
            {
                return NotFound("Company not found.");
            }

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchServices(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest("Search query is required.");

                var result = await _TaxinvoiceRepo.SearchServices(q);

                return Json(result); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Deleteinvoice(string id)
        {
            var result = await _TaxinvoiceRepo.Deletereords(Convert.ToInt32(EncryptionHelper.Decrypt(id)));

            if (result)
            {
                return Json(new
                {
                    success = true,
                    message = "Invoice deleted successfully."
                });
            }

            return Json(new
            {
                success = false,
                message = "Invoice not found or could not be deleted."
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _TaxinvoiceRepo.Getinvoicebyid(Convert.ToInt32(EncryptionHelper.Decrypt(id)));

            if (result == null || result.main == null)
            {
                return NotFound();
            }

            var companies = await _TaxinvoiceRepo.Getcompany();

            var vm = new Taxinvoice.TaxInvoiceCreateVM
            {
                main = result.main,
                details = result.details ?? new List<Taxinvoice.InvoiceDetails>(),
                companies = companies ?? new List<Taxinvoice.TaxInvoiceCreate>()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInvoice([FromBody] TaxInvoiceCreateVM model)
        {
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var oldVM = await _TaxinvoiceRepo.Getinvoicebyid(Convert.ToInt32(model.main.Id))
             as Taxinvoice.TaxInvoiceCreateVM;

            var result = await _TaxinvoiceRepo.UpdateSave(model, Action: "updateOldData");

            if (oldVM?.main != null)
            {
                var changes = _TaxinvoiceRepo.BuildChangeComment(oldVM.main, oldVM.details, model.main, model.details);

                if (!string.IsNullOrEmpty(changes))
                {
                    await _TaxinvoiceRepo.SaveInvoiceChangeHistory(
                        sessionName: model.main.sessionname ?? "System",
                        invoiceNo: model.main.compCode ?? model.main.Id,
                        message: changes
                    );
                }
            }

            return Json(new { success = true, invoiceNo = model.main.invoiceno });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Json(new { success = false, message = "Only admin can approve or reject invoices." });

            if (status != "approve" && status != "reject")
                return Json(new { success = false, message = "Invalid status." });

            int invoiceId = Convert.ToInt32(EncryptionHelper.Decrypt(id));   
            string empCode = HttpContext.Session.GetString("EmpCode") ?? "NA";

            bool ok = await _TaxinvoiceRepo.Approve(invoiceId, empCode);

            return Json(new
            {
                success = ok,
                message = ok ? (status == "approve" ? "Invoice approved." : "Invoice rejected.") : "Could not update the invoice."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file selected." });

            int invoiceId;
            try { invoiceId = Convert.ToInt32(EncryptionHelper.Decrypt(id)); }
            catch { return Json(new { success = false, message = "Invalid invoice." }); }

            string empCode = HttpContext.Session.GetString("EmpCode") ?? "NA";

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
                return Json(new { success = false, message = "Only JPG, PNG or PDF files are allowed." });
            if (file.Length > 5 * 1024 * 1024)
                return Json(new { success = false, message = "File is larger than 5 MB." });

            string folder = Path.Combine(_env.WebRootPath, "TaxInvoicedocumnet");
            Directory.CreateDirectory(folder);

            string savedName = $"{Guid.NewGuid()}{ext}";
            string fullPath = Path.Combine(folder, savedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            string? oldPath;
            try
            {
                oldPath = await _TaxinvoiceRepo.SaveDocument(invoiceId, $"/TaxInvoicedocumnet/{savedName}", empCode);
            }
            catch
            {
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                throw;
            }

            // remove the replaced file (only inside the document folder)
            if (!string.IsNullOrWhiteSpace(oldPath))
            {
                string oldFull = Path.Combine(_env.WebRootPath,
                    oldPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (oldFull.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(oldFull))
                    System.IO.File.Delete(oldFull);
            }

            return Json(new
            {
                success = true,
                message = string.IsNullOrWhiteSpace(oldPath) ? "Document uploaded." : "Document replaced."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments(string id)
        {
            int invoiceId;
            try { invoiceId = Convert.ToInt32(EncryptionHelper.Decrypt(id)); }
            catch { return Json(new { success = false, data = (object?)null }); }

            string? path = await _TaxinvoiceRepo.GetDocument(invoiceId);

            if (string.IsNullOrWhiteSpace(path))
                return Json(new { success = true, data = (object?)null });

            string fullPath = Path.Combine(_env.WebRootPath,
                path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(fullPath))
                return Json(new { success = true, data = (object?)null });

            return Json(new
            {
                success = true,
                data = new
                {
                    fileName = "Invoice document" + Path.GetExtension(path),
                    url = Url.Content("~" + path),            // works even if the app runs in a virtual directory
                    uploadedOn = System.IO.File.GetLastWriteTime(fullPath).ToString("dd-MMM-yyyy")
                }
            });
        }

        [HttpGet]
        public IActionResult TaxInvoicePDF(string id)
        {
            int invoiceId;
            try
            {
                if (!int.TryParse(EncryptionHelper.Decrypt(id), out invoiceId) || invoiceId <= 0)
                    return BadRequest("Invalid invoice ID.");
            }
            catch
            {
                return BadRequest("Invalid invoice ID.");
            }

            bool isAdmin = HttpContext.Session.GetString("Role") == "Admin";

            try
            {
                var result = _TaxinvoiceRepo.GenerateInvoicePdf(invoiceId);

                switch (result.Status)
                {
                    case PdfStatus.NotFound:
                        return NotFound("Invoice not found.");
                    case PdfStatus.Forbidden:
                        return Forbid();   // or: return Content("PDF is available only after approval.");
                }

                Response.Headers["Content-Disposition"] = $"inline; filename=\"{result.FileName}\"";
                return File(result.Bytes!, "application/pdf");
            }
            catch (Exception)
            {
                // log ex here (ILogger) instead of sending internals to the browser
                return StatusCode(500, "PDF generation failed.");
            }
        }
    }

}
