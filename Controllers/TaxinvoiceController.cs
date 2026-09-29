using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using WEBLINK_CRM.Helpers;
using WLSPL_ERP_CRM.Models;
using WLSPL_ERP_CRM.repository;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

/* Things to do when creating invoice 
   1.Alter table InvoiceMain add columns AgainstBy Nvarchar(500) null and AgainstByValue Nvarchar(500) null
   2.Alter table invoicedetails add column ServiceName Nvarchar(500) null and ServiceId nvarchar(500) null and ValidateTill nvarchar(500) null 
   3.ADD parameters in [dbo].[SP_AddInvoice] for InvoiceMain  @AgainstBy nvarchar(MAX) = null,
        @AgainstByValue nvarchar(MAX) = null, @TotalPayable nvarchar(MAX) = null, @TdsPer nvarchar(MAX) = null,
        @TdsAmt nvarchar(MAX) = null,@companyCode nvarchar(MAX) = null, @Remarks nvarchar(MAX) = null
   4.Alter table InvoiceMain add TotalPayable nvarchar(max) null, TdsPer nvarchar(max) null, TdsAmt nvarchar(max) null
   5.Alter table InvoiceMain add compCode nvarchar(max) null
 */

namespace WLSPL_ERP_CRM.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize]
    public class TaxinvoiceController : Controller
    {
        private readonly ITaxinvoiceRepo _TaxinvoiceRepo;

        public TaxinvoiceController(ITaxinvoiceRepo taxinvoiceRepo)
        {
            _TaxinvoiceRepo = taxinvoiceRepo;
        }

        public async Task<IActionResult> Index(string? financialYear, int? month)
        {
            try
            {
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


                var data = await _TaxinvoiceRepo.GetInfo(financialYear,month);

                var financialYearSummary = await _TaxinvoiceRepo.GetFinancialYearSummary(financialYear);

                // ============================================
                // SEND DATA TO VIEW
                // ============================================

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

        public async Task<IActionResult> GetPdf(string id)
        {
            if (Convert.ToInt32(EncryptionHelper.Decrypt(id)) <= 0)
                return BadRequest("Invalid invoice ID.");

            var invoice = await _TaxinvoiceRepo.GetInvoiceForPdfAsync(Convert.ToInt32(EncryptionHelper.Decrypt(id)));

            if (invoice == null)
                return NotFound("Invoice not found.");

            return View(invoice);


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
                var invoiceMain = await _TaxinvoiceRepo.Getinvoicenoss()
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

        public async Task<IActionResult> ApprovalList()
        {
            var list = await _TaxinvoiceRepo.GetApprovelList();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int ID)
        {
            try
            {
                if (ID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid  ID."
                    });
                }
                var EmpCode = HttpContext.Session.GetString("EmpCode")?.ToString();
                var result = await _TaxinvoiceRepo.Approve(ID, EmpCode);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Approved successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Invoice could not be Approve."
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

        [HttpPost]
        public async Task<IActionResult> Reject(int ID)
        {
            try
            {
                if (ID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Tax Invoice."
                    });
                }
                var EmpCode = HttpContext.Session.GetString("EmpCode")?.ToString();
                var result = await _TaxinvoiceRepo.Reject(ID, EmpCode);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = " Rejected successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Tax-Invoice could not be Reject."
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
    }

}
