
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WLSPL_ERP_CRM.repository
{
    public class RepoReports : IReports
    {
        private readonly IConfiguration _configuration;

        public RepoReports(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<object>> GetTopInvoiceList()
        {
            using (var connection = new SqlConnection(
                 _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetTopInvoiceList");               

                var result = await connection.QueryAsync<object>(
                    "SP_Reports",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }
    }
}
