using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WEBLINK_CRM.Models;
using WLSPL_ERP_CRM.Models;
using static WLSPL_ERP_CRM.Models.ProformaInvoice;

namespace WEBLINK_CRM.repository
{
    public interface IProforma
    {
        Task<List<ProformaInvoice.ProformaInvoiceCreate>> GetInfo(string financialYear, int? month, string? salesManager, string empCode, string empRole);

        Task<List<ProformaInvoice.ProformaInvoiceCreate>> GetFinancialYearSummary(string financialYear, string? salesManager, string empCode, string empRole);

        Task<dynamic> GetSalesPersonList(string empCode, string empRole);

        Task<ProformaInvoice.ProformaInvoiceCreate?> GetBlankModelWithinvoiceno();

        Task<List<ProformaInvoiceCreate>> Getcompany();
        Task<List<dynamic>> GetQuotationsByCompany(string cname, string type);
        Task<object> GetQuotationProformaDetails(int id, string type);
        Task<ProformaInvoiceCreate> Getcompanybycname(string cname);

        Task<List<InvoiceDetails>> SearchServices(string cname);

        Task<bool> UpdateSave(ProformaInvoiceCreateVM model, string Action);

        Task<dynamic> Getinvoicebyid(int ID);

        Task<bool> DeleteInvoiceDetails(int id, string name);

        byte[] ProformaPdf(int id);
    }
}
