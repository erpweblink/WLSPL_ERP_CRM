
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public class RepoReports : IReports
    {
        private readonly IConfiguration _configuration;

        public RepoReports(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<VM_Reports>> GetTopInvoiceList()
        {
            using (var connection = new SqlConnection(
                 _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetTopInvoiceList");               

                var result = await connection.QueryAsync<VM_Reports>(
                    "SP_Reports",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<VM_Reports>().ToList();
            }
        }
    }
}
