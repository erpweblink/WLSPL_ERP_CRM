using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WEBLINK_CRM.repository;
using WLSPL_ERP_CRM.Models;
using WLSPL_ERP_CRM.repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var invoiceMain = await _TaxinvoiceRepo.Getinvoiceno();

            var companies = await _TaxinvoiceRepo.Getcompany();

            // DEBUG
            Console.WriteLine("Company Count = " + companies.Count);

            foreach (var company in companies)
            {
                Console.WriteLine("Company Name = " + company.cname);
            }

            var model = new Taxinvoice.TaxInvoiceCreateViewModel
            {
                Main = invoiceMain ?? new Taxinvoice.InvoiceMain(),
                Details = new List<Taxinvoice.InvoiceDetails>(),
                Companies = companies ?? new List<Taxinvoice.InvoiceMain>()
            };

            if (model.Main.invoicedate == null)
            {
                model.Main.invoicedate = DateTime.Today;
            }

            return View(model);
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





    }






}
