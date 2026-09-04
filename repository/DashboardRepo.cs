using Microsoft.Data.SqlClient;
using WLSPL_ERP_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly string _connectionString;

        public DashboardRepo(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Conn_Stringg")
                ?? throw new Exception("Connection string 'Conn_Stringg' not found.");
        }

        public List<EmployeeNode> GetAllEmployees()
        {
            var list = new List<EmployeeNode>();

            const string sql = @"
                SELECT
                    empcode,
                    name,
                    role,
                    status,
                    TL_Manager AS ParentCode,
                    Sales_TL_Manager AS SalesTLManager
                FROM employees
                WHERE isdeleted = 0
                  AND status = 1 AND TL_Manager IS NOT NULL
                ORDER BY id ASC";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, con);

            con.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new EmployeeNode
                {
                    EmpCode = reader["empcode"] == DBNull.Value
                        ? null
                        : reader["empcode"].ToString(),

                    Name = reader["name"] == DBNull.Value
                        ? null
                        : reader["name"].ToString(),

                    Role = reader["role"] == DBNull.Value
                        ? null
                        : reader["role"].ToString(),

                    Status = reader["status"] == DBNull.Value
                        ? "0"
                        : reader["status"].ToString(),

                    ParentCode = reader["ParentCode"] == DBNull.Value
                        ? null
                        : reader["ParentCode"].ToString(),

                    SalesTLManager = reader["SalesTLManager"] == DBNull.Value
                        ? null
                        : reader["SalesTLManager"].ToString()
                });
            }

            return list;
        }
    }
}
