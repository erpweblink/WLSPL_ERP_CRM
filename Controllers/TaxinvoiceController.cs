using Microsoft.AspNetCore.Mvc;
using WLSPL_ERP_CRM.Models;
using WLSPL_ERP_CRM.repository;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

namespace WLSPL_ERP_CRM.Controllers
{
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

                // ============================================
                // DEFAULT MONTH
                // Current month when no month is selected
                //
                // month = 0 => All Months
                // month = 1-12 => Selected Month
                // ============================================

                if (!month.HasValue)
                {
                    month = today.Month;
                }

                // ============================================
                // GET INVOICE DATA
                // ============================================

                var data = await _TaxinvoiceRepo.GetInfo(
                    financialYear,
                    month
                );

                // ============================================
                // GET COMPLETE FINANCIAL YEAR SUMMARY
                // ============================================

                var financialYearSummary =
                    await _TaxinvoiceRepo.GetFinancialYearSummary(
                        financialYear
                    );

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

        public async Task<IActionResult> GetPdf(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid invoice ID.");

            var invoice = await _TaxinvoiceRepo.GetInvoiceForPdfAsync(id);

            if (invoice == null)
                return NotFound("Invoice not found.");

            return View(invoice);


        }

        public async Task<IActionResult> Create()
        {
            var invoiceMain = await _TaxinvoiceRepo.Getinvoicenoss();

            var companies = await _TaxinvoiceRepo.Getcompany();

            var model = new TaxInvoiceCreateVM
            {
                main = invoiceMain ?? new TaxInvoiceCreate(),
                details = new List<Taxinvoice.InvoiceDetails>(),
                companies = companies ?? new List<TaxInvoiceCreate>()
            };

            if (model.main.invoicedate == null)
            {
                model.main.invoicedate = DateTime.Today;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveInvoice([FromBody] TaxInvoiceCreateVM model)
        {
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

              var result = _TaxinvoiceRepo.UpdateSave(model, Action : "insert");

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
        public async Task<IActionResult> Deleteinvoice(int id)
        {
            var result = await _TaxinvoiceRepo.Deletereords(id);

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
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _TaxinvoiceRepo.Getinvoicebyid(id);

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
        public IActionResult UpdateInvoice([FromBody] TaxInvoiceCreateVM model)
        {
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var result = _TaxinvoiceRepo.UpdateSave(model, Action: "updateOldData");

            return Json(new { success = true, invoiceNo = model.main.invoiceno });
        }

    }

}
