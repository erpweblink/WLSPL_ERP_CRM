using WLSPL_ERP_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public interface ITaxinvoiceRepo
    {


        Task<List<Taxinvoice.InvoiceList>> GetInfo(string financialYear,int? month);

        Task<List<Taxinvoice.InvoiceMonthSummary>>GetFinancialYearSummary(string financialYear);

        Task<Taxinvoice.TaxInvoicePdfViewModel?>GetInvoiceForPdfAsync(int id);

    }
}
