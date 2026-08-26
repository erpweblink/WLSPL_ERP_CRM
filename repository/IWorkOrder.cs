using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_WorkOrder;

namespace WEBLINK_CRM.repository
{
    public interface IWorkOrder
    {
        Task<List<VM_WorkOrder>> GetWorkOrderList(string size);
        Task<List<WorkOrderDetailVM>> GetWorkOrderDEtailsByID(string wono);
        Task<List<object>> GetCompanyList(string Status);
        Task<List<object>> GetDepartmentlist(string Status);

        Task<List<object>> GetCompanyDataByCode(string Code);
        Task<List<object>> GetServiceByID(string ID);
        Task<VM_WorkOrder> GetWorkOrderDataById(string ID);
        Task<List<object>> GetServices(string Dept);

        Task<int> SaveWorkOrder(VM_WorkOrder model);

        Task<List<WorkOrderDetailVM>> GetWorkDetailsById(string ID);

        Task<List<BankDetailVM>> GetBankDetailsById(string ID);
        Task<bool> DeleteWorkOrder(int id);

        Task<bool> AdminApproveWorkOrder(int ID);
      
    }
}
