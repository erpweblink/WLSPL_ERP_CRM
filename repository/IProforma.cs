using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_Proforma;

namespace WEBLINK_CRM.repository
{
    public interface IProforma
    {
        Task<List<VM_Proforma>> GetProformaList(string size);
        Task<List<ProformaDetailVM>> GetDetailsById(string wono);
        Task<List<object>> GetCompanyList(string Status);
        Task<List<object>> GetStateList(string Status);
        Task<List<object>> GetCompanyByCode(string Code);
 
        Task<VM_Proforma> GetProformaById(string ID);   

        Task<int> Save(VM_Proforma model);

        Task<bool> Delete(int id);
        byte[] ProformaPdf(int id);


    }
}
