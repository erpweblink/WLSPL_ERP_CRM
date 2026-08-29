using WEBLINK_CRM.Models;
using WLSPL_ERP_CRM.Models;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

namespace WLSPL_ERP_CRM.repository
{
    public interface ITaxinvoiceRepo
    {


        Task<List<Taxinvoice.InvoiceList>> GetInfo(string financialYear,int? month);

        Task<List<Taxinvoice.InvoiceMonthSummary>>GetFinancialYearSummary(string financialYear);

        Task<Taxinvoice.TaxInvoicePdfViewModel?>GetInvoiceForPdfAsync(int id);

        Task<Taxinvoice.InvoiceMain?>Getinvoiceno();
        Task<Taxinvoice.TaxInvoiceCreate?> Getinvoicenoss();

        Task<List<TaxInvoiceCreate>>Getcompany();
        Task<TaxInvoiceCreate> Getcompanybycname(string cname);

        Task<bool> UpdateSave(TaxInvoiceCreateVM model);

    }
}
