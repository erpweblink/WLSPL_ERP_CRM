using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_Proforma;

namespace WEBLINK_CRM.repository
{
    public class RepoProforma : IProforma
    {
        private readonly IConfiguration _configuration;

        public RepoProforma(IConfiguration configuration)
        {
            _configuration = configuration;
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

        public async Task<List<object>> GetCompanyByCode(string Code)
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

        public async Task<int> Save(VM_Proforma model)
        {
            try
            {
                using (SqlConnection con =
                       new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await con.OpenAsync();

                    var parameters = new DynamicParameters();

                    parameters.Add("@ID", model.ID);                
                    parameters.Add("@ProformaDate", model.ProformaDate);
                    parameters.Add("@ReverseCharge", model.ReverseCharge);
                    parameters.Add("@State", model.State);
                    parameters.Add("@CompanyName", model.CompanyName);
                    parameters.Add("@CompanyCode", model.CompanyCode);
                    parameters.Add("@Address", model.Address);
                    parameters.Add("@GSTNO", model.GSTNO);
                    parameters.Add("@BillState", model.BillState);
                    parameters.Add("@TotalAmtBeforeTax", model.TotalAmtBeforeTax);
                    parameters.Add("@TotalAmtAfterTax", model.TotalAmtAfterTax);
                    parameters.Add("@@CreatedBy", model.CreatedBy);

                    DataTable dtDetails = new DataTable();
                    dtDetails.Columns.Add("ProductDescription", typeof(string));
                    dtDetails.Columns.Add("SACCode", typeof(string));
                    dtDetails.Columns.Add("Qty", typeof(decimal));
                    dtDetails.Columns.Add("Rate", typeof(decimal));
                    dtDetails.Columns.Add("Amount", typeof(decimal));
                    dtDetails.Columns.Add("TaxableValue", typeof(decimal));
                    dtDetails.Columns.Add("CGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("CGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("SGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("SGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("IGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("IGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("Total", typeof(decimal));

                    if (model.objtblProformaDtl != null)
                    {
                        foreach (var item in model.objtblProformaDtl)
                        {
                            dtDetails.Rows.Add(
        item.ProductDescription,
        item.SACCode,
        item.Qty,
        item.Rate,
        item.Amount,
        item.TaxableValue,
        item.CGSTRate,
        item.CGSTAmt,
        item.SGSTRate,
        item.SGSTAmt,
        item.IGSTRate,
        item.IGSTAmt,
        item.Total
    );
                        }
                    }

                    parameters.Add(
                        "@ProformaDetails",
                        dtDetails.AsTableValuedParameter("WLSPL.ProformaDetailType")
                    );

                    var result = await con.QuerySingleAsync<int>(
                        "[WLSPL].[SP_SaveProforma]",
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

        public async Task<List<VM_Proforma>> GetProformaList(string size)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetProformaList");
                parameters.Add("@PageSize", size);

                var result = await connection.QueryAsync<VM_Proforma>(
     "SP_Proforma",
     parameters,
     commandType: CommandType.StoredProcedure
 );

                return result.Cast<VM_Proforma>().ToList();
            }
        }

        public async Task<VM_Proforma> GetProformaById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetProformaDataById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryFirstOrDefaultAsync<VM_Proforma>(
               "SP_Proforma",
               parameters,
               commandType: CommandType.StoredProcedure
           );

                return result;
            }
        }
        public async Task<List<ProformaDetailVM>> GetDetailsById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetDetailsById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<ProformaDetailVM>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<ProformaDetailVM>().ToList();
            }
        }

        public async Task<bool> Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Proforma", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "Delete");
                    cmd.Parameters.AddWithValue("@ID", id);

                    await con.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    return result > 0;
                }
            }
        }

        public async Task<List<object>> GetStateList(string Status)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetStateList");
                parameters.Add("@Status", Status);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }
    }
}
