using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using WLSPL_ERP_CRM.repository;

namespace WLSPL_ERP_CRM.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReports objreports;
        public ReportController(IReports reports)
        {
            objreports = reports;
        }
        public async Task<IActionResult> GetTopInvoiceList()
        {
            var list = await objreports.GetTopInvoiceList();
            return View(list);
        }
    }
}
