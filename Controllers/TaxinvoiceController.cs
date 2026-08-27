using Microsoft.AspNetCore.Mvc;
using WEBLINK_CRM.repository;
using WLSPL_ERP_CRM.repository;

namespace WLSPL_ERP_CRM.Controllers
{
    public class TaxinvoiceController : Controller
    {
        private readonly ITaxinvoiceRepo _TaxinvoiceRepo;

        public TaxinvoiceController(ITaxinvoiceRepo taxinvoiceRepo)
        {
            _TaxinvoiceRepo = taxinvoiceRepo;
        }
        //public async Task<IActionResult> Index(string? financialYear, int? month)
        //{
        //    try
        //    {
        //        var today = DateTime.Now;

        //        // Default Financial Year
        //        if (string.IsNullOrEmpty(financialYear))
        //        {
        //            financialYear = today.Month >= 4
        //                ? $"{today.Year}-{(today.Year + 1).ToString().Substring(2)}"
        //                : $"{today.Year - 1}-{today.Year.ToString().Substring(2)}";
        //        }

        //        // Default current month
        //        if (!month.HasValue)
        //        {
        //            month = today.Month;
        //        }

        //        var data = await _TaxinvoiceRepo.GetInfo(
        //            financialYear,
        //            month
        //        );

        //        ViewBag.SelectedFinancialYear = financialYear;
        //        ViewBag.SelectedMonth = month;

        //        return View(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorMessage = ex.Message;
        //        return View("Error");
        //    }
        //}


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

    }






}
