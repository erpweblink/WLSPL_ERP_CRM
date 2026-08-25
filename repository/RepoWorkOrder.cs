using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_WorkOrder;

namespace WEBLINK_CRM.repository
{
    public class RepoWorkOrder : IWorkOrder
    {
        private readonly IConfiguration _configuration;

        public RepoWorkOrder(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<object>> GetDepartmentlist(string Status)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetDepartmentList");
                parameters.Add("@Status", Status);

                var result = await connection.QueryAsync<object>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }

        public async Task<List<object>> GetServices(string Dept)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetServices");
                parameters.Add("@Status", Dept);

                var result = await connection.QueryAsync<object>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }

        public async Task<List<object>> GetServiceByID(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetServicesById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<object>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<List<object>> GetCompanyList(string Status)
        {
            using (var connection = new SqlConnection(
               _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetCompanyList");
                parameters.Add("@Status", Status);

                var result = await connection.QueryAsync<object>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }

        public async Task<List<object>> GetCompanyDataByCode(string Code)
        {
            using (var connection = new SqlConnection(
                 _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetCompanyDataByCode");
                parameters.Add("@Code", Code);

                var result = await connection.QueryAsync<object>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<int> SaveWorkOrder(VM_WorkOrder model)
        {
            try
            {

                using (SqlConnection con =
                       new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await con.OpenAsync();

                    var parameters = new DynamicParameters();

                    parameters.Add("@ID", model.ID);

                    parameters.Add("@Type", model.Type);
                    parameters.Add("@WOStatus", model.WOStatus);
                    parameters.Add("@WONo", model.WONo);
                    parameters.Add("@CompanyName", model.CompanyName);
                    parameters.Add("@CompanyCode", model.CompanyCode);
                    parameters.Add("@OwnerName", model.OwnerName);
                    parameters.Add("@Address", model.Address);
                    parameters.Add("@GSTNO", model.GSTNO);
                    parameters.Add("@EmailID", model.EmailID);

                    parameters.Add("@TodayDt", model.TodayDt);
                    parameters.Add("@RenewalDt", model.RenewalDt);

                    parameters.Add("@PaymentMode", model.PaymentMode);

                    parameters.Add(
                        "@TotalDealBasicAmount",
                        model.TotalDealBasicAmount);

                    parameters.Add(
                        "@TotalDealGSTAmount",
                        model.TotalDealGSTAmount);

                    parameters.Add(
                        "@BasicAmountReceived",
                        model.BasicAmountReceived);

                    parameters.Add(
                        "@GSTAmountReceived",
                        model.GSTAmountReceived);

                    parameters.Add(
                        "@BalanceBasicAmount",
                        model.BalanceBasicAmount);

                    parameters.Add(
                        "@BalanceGSTAmount",
                        model.BalanceGSTAmount);

                    parameters.Add(
                        "@TotalAmountBalance",
                        model.TotalAmountBalance);

                    parameters.Add("@CreatedBy", model.CreatedBy);
                    parameters.Add("@UpdatedBy", model.UpdatedBy);


                    DataTable dtDetails = new DataTable();

                    dtDetails.Columns.Add(
                        "Department",
                        typeof(string));

                    dtDetails.Columns.Add(
                        "ServicesDescription",
                        typeof(string));

                    dtDetails.Columns.Add(
                        "Remark",
                        typeof(string));

                    dtDetails.Columns.Add(
                        "Qty",
                        typeof(decimal));

                    dtDetails.Columns.Add(
                        "NoofYr",
                        typeof(int));

                    dtDetails.Columns.Add(
                        "Rate",
                        typeof(decimal));

                    dtDetails.Columns.Add(
                        "Amount",
                        typeof(decimal));

                    dtDetails.Columns.Add(
                        "IsComplete",
                        typeof(bool));

                    dtDetails.Columns.Add(
                        "Status",
                        typeof(string));


                    foreach (var item in model.objtblWorkOrderDtl)
                    {
                        dtDetails.Rows.Add(
                            item.Department ?? "",
                            item.ServicesDescription ?? "",
                            item.Remark ?? "",
                            item.Qty,
                            item.NoofYr,
                            item.Rate,
                            item.Amount,
                            item.IsComplete,
                            item.Status ?? ""
                        );
                    }


                    DataTable dtBank = new DataTable();

                    dtBank.Columns.Add(
                        "BankName",
                        typeof(string));

                    dtBank.Columns.Add(
                        "ChequeNo",
                        typeof(string));

                    dtBank.Columns.Add(
                        "ChequeDate",
                        typeof(DateTime));

                    dtBank.Columns.Add(
                        "Amount",
                        typeof(decimal));


                    foreach (var item in model.objtblBankDetail)
                    {
                        DataRow row = dtBank.NewRow();

                        row["BankName"] =
                            item.BankName ?? "";

                        row["ChequeNo"] =
                            item.ChequeNo ?? "";

                        row["ChequeDate"] =
                            item.ChequeDate.HasValue
                            ? (object)item.ChequeDate.Value
                            : DBNull.Value;

                        row["Amount"] =
                            item.Amount;

                        dtBank.Rows.Add(row);
                    }


                    parameters.Add(
                        "@WorkOrderDetails",
                        dtDetails.AsTableValuedParameter(
                            "WLSPL.WorkOrderDetailType"
                        )
                    );

                    parameters.Add(
                        "@BankDetails",
                        dtBank.AsTableValuedParameter(
                            "WLSPL.BankDetailType"
                        )
                    );

                    var result = await con.QuerySingleAsync<int>(
    "[WLSPL].[SP_SaveWorkOrder]",
    parameters,
    commandType: CommandType.StoredProcedure
);


                    return result;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<VM_WorkOrder>> GetWorkOrderList(string size)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetWorkOrderList");
                parameters.Add("@PageSize", size);

                var result = await connection.QueryAsync<VM_WorkOrder>(
     "SP_WorkOrder",
     parameters,
     commandType: CommandType.StoredProcedure
 );

                return result.Cast<VM_WorkOrder>().ToList();
            }
        }

        public async Task<List<WorkOrderDetailVM>> GetWorkOrderDEtailsByID(string ID)
        {
         
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetWorkOrderDEtailsByID");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<WorkOrderDetailVM>(
     "SP_WorkOrder",
     parameters,
     commandType: CommandType.StoredProcedure
 );

                return result.Cast<WorkOrderDetailVM>().ToList();
            }
        }

        public async Task<VM_WorkOrder> GetWorkOrderDataById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetWorkOrderDataById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryFirstOrDefaultAsync<VM_WorkOrder>(
               "SP_WorkOrder",
               parameters,
               commandType: CommandType.StoredProcedure
           );

                return result;
            }
        }
        public async Task<List<WorkOrderDetailVM>> GetWorkDetailsById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetWorkDetailsById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<WorkOrderDetailVM>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<WorkOrderDetailVM>().ToList();
            }
        }
        public async Task<List<BankDetailVM>> GetBankDetailsById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetBankDetailsById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<BankDetailVM>(
                    "SP_WorkOrder",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<BankDetailVM>().ToList();
            }
        }

        public async Task<bool> DeleteWorkOrder(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                using (SqlCommand cmd = new SqlCommand("SP_WorkOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "DeleteWorkOrder");
                    cmd.Parameters.AddWithValue("@ID", id);

                    await con.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    return result > 0;
                }
            }
        }
    }
}
