using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_Quotation;

namespace WEBLINK_CRM.repository
{
    public interface IQuotation
    {
        Task<List<VM_Quotation>> GetList(string size);
        Task<List<QuotationDetailVM>> GetDetailsById(string wono);
        Task<List<object>> GetCompanyList(string Status);
        Task<List<object>> GetStateList(string Status);
        Task<List<object>> GetCompanyByCode(string Code);
 
        Task<VM_Quotation> GetById(string ID);   

        Task<int> Save(VM_Quotation model);

        Task<bool> Delete(int id);
        byte[] QuotationPdf(int id);


    }
}
