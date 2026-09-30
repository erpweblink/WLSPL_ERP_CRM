using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WEBLINK_CRM.Helpers;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;
using WLSPL_ERP_CRM.Models;
using WLSPL_ERP_CRM.repository;
using static WLSPL_ERP_CRM.Models.ProformaInvoice;


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


                var data = await objProforma.GetInfo(financialYear, month, salesManager, SessionName, SessionRole);

                var financialYearSummary = await objProforma.GetFinancialYearSummary(financialYear, salesManager, SessionName, SessionRole);


                var personLists = await objProforma.GetSalesPersonList(SessionName, SessionRole);

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


        public async Task<IActionResult> Create(string id = null)
        {
            var companies = await objProforma.Getcompany();

            // Edit
            if (!string.IsNullOrEmpty(id))
            {
                var invoiceId = Convert.ToInt32(EncryptionHelper.Decrypt(id));

                var result = await objProforma.Getinvoicebyid(invoiceId);

                if (result == null || result.main == null)
                {
                    return NotFound();
                }

                var vm = new ProformaInvoice.ProformaInvoiceCreateVM
                {
                    main = result.main,
                    details = result.details ?? new List<ProformaInvoice.InvoiceDetails>(),
                    companies = companies ?? new List<ProformaInvoice.ProformaInvoiceCreate>(),
                    BankDetails = result.BankDetails ?? new List<ProformaInvoice.InvoiceBankDetail>()
                };

                return View("Create", vm);
            }

            // Create
            var invoiceMain = await objProforma.GetBlankModelWithinvoiceno();

            var model = new ProformaInvoiceCreateVM
            {
                main = invoiceMain ?? new ProformaInvoiceCreate(),
                details = new List<ProformaInvoice.InvoiceDetails>(),
                companies = companies ?? new List<ProformaInvoiceCreate>(),
                BankDetails = new List<ProformaInvoice.InvoiceBankDetail>()
            };

            if (model.main.invoicedate == null)
            {
                model.main.invoicedate = DateTime.Today;
            }

            return View("Create", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SaveInvoice([FromBody] ProformaInvoiceCreateVM model)
        {
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var result = objProforma.UpdateSave(model, Action: "insert");

            return Json(new { success = true, invoiceNo = model.main.invoiceno });
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotationsByCompany(string companyName, string type)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return BadRequest("Company name is required.");

            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Type is required.");

            var result = await objProforma.GetQuotationsByCompany(companyName, type);

            if (result.Count == 0)
                return NotFound("No records found.");

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotationProformaDetails(int id, string type)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(type))
                return BadRequest("Invalid request.");

            var result = await objProforma.GetQuotationProformaDetails(id, type);

            if (result == null)
                return NotFound("No details found.");

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Getcomapnybycname(string cname)
        {
            if (string.IsNullOrWhiteSpace(cname))
            {
                return BadRequest("Company name is required.");
            }

            var result = await objProforma.Getcompanybycname(cname);

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

                var result = await objProforma.SearchServices(q);

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }


        [HttpPost]
        public IActionResult UpdateInvoice([FromBody] ProformaInvoiceCreateVM model)
        {
            if (model == null || model.main == null)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Error = x.Value.Errors[0].ErrorMessage });
                return BadRequest(new { success = false, message = "Model binding failed", errors });
            }
            model.main.sessionname = HttpContext.Session.GetString("EmpCode")?.ToString();

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var result = objProforma.UpdateSave(model, Action: "updateOldData");

            return Json(new { success = true, invoiceNo = model.main.invoiceno });
        }

        [HttpPost]
        public async Task<IActionResult> Deleteinvoice(int ID)
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
                var name= HttpContext.Session.GetString("EmpCode")?.ToString();
                var result = await objProforma.DeleteInvoiceDetails(ID, name);

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

        [HttpPost]
        public IActionResult ViewTaxInvoicePDF(string id)
        {
            string encId = EncryptionHelper.Encrypt(id.ToString());

            var url = Url.Action(
                 "TaxInvoicePDF",
                 "TaxInvoice") + "?id=" + Uri.EscapeDataString(encId);


            return Json(new
            {
                success = true,
                url = url
            });
        }

    }
}
