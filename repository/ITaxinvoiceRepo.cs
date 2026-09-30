using WEBLINK_CRM.Models;
using WLSPL_ERP_CRM.Models;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

namespace WLSPL_ERP_CRM.repository
{
    public interface ITaxinvoiceRepo
    {
        Task<List<Taxinvoice.TaxInvoiceCreate>> GetInfo(string financialYear, int? month, string? salesManager, string empCode, string empRole);

        Task<List<Taxinvoice.TaxInvoiceCreate>> GetApprovelList();

        Task<List<Taxinvoice.TaxInvoiceCreate>> GetFinancialYearSummary(string financialYear, string? salesManager, string empCode, string empRole);

        Task<dynamic> GetSalesPersonList(string empCode, string empRole);

        Task<Taxinvoice.TaxInvoiceCreate?> Getinvoiceno();

        Task<Taxinvoice.TaxInvoiceCreateVM?> GetProformaDetails(string ProformaId);

        Task<List<TaxInvoiceCreate>> Getcompany();


        Task<TaxInvoiceCreate> Getcompanybycname(string cname);

        Task<List<InvoiceDetails>> SearchServices(string cname);

        Task<bool> UpdateSave(TaxInvoiceCreateVM model, string Action);

        Task SaveInvoiceChangeHistory(string sessionName, string invoiceNo, string message);
        public string? BuildChangeComment(Taxinvoice.TaxInvoiceCreate oldMain, List<Taxinvoice.InvoiceDetails> oldDetails,
     Taxinvoice.TaxInvoiceCreate newMain, List<Taxinvoice.InvoiceDetails> newDetails);

        Task<bool> Deletereords(int ID);

        Task<dynamic> Getinvoicebyid(int ID);

        Task<bool> Approve(int id, string user);

        Task<bool> Reject(int id, string user);


        TaxInvoicePdfResult GenerateInvoicePdf(int invoiceId);
    }
}
