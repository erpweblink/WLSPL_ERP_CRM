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

        public List<EmployeeNode> GetEmployeeHierarchy(string employeeCode)
        {
            var list = new List<EmployeeNode>();

            const string sql = @"
                WITH EmployeeHierarchy AS
                (
                    SELECT
                        e.empcode,
                        e.name,
                        e.role,
                        e.status,
                        e.TL_Manager AS ParentCode,
                        ISNULL(e.Sales_TL_Manager, 0) AS SalesTLManager,
                        0 AS HierarchyLevel,
                        CAST(
                            '/' + e.empcode + '/'
                            AS VARCHAR(MAX)
                        ) AS HierarchyPath
                    FROM employees e
                    WHERE e.isdeleted = 0
                      AND e.status = 1
                      AND e.TL_Manager = e.empcode


                    UNION ALL

                    SELECT
                        c.empcode,
                        c.name,
                        c.role,
                        c.status,
                        c.TL_Manager AS ParentCode,
                        ISNULL(c.Sales_TL_Manager, 0) AS SalesTLManager,
                        p.HierarchyLevel + 1 AS HierarchyLevel,
                        CAST(
                            p.HierarchyPath + c.empcode + '/'
                            AS VARCHAR(MAX)
                        ) AS HierarchyPath
                    FROM employees c
                    INNER JOIN EmployeeHierarchy p
                        ON c.TL_Manager = p.empcode
                    WHERE c.isdeleted = 0
                      AND c.status = 1
                      AND c.empcode <> c.TL_Manager
                ),
                RoleHierarchy AS
                (
                    SELECT
                        e.*,
                        CASE
                            WHEN e.HierarchyLevel = 0 THEN 'Admin'
                            WHEN e.role = 'SubAdmin' THEN 'Sub Admin'
                            WHEN e.HierarchyLevel = 1 AND e.role = 'Sales' THEN 'Sales Manager'
                            WHEN e.HierarchyLevel >= 3 AND e.SalesTLManager = 1 THEN 'Sales TL'
                            WHEN e.role = 'Sales'
                                 AND EXISTS
                                 (
                                     SELECT 1
                                     FROM EmployeeHierarchy c
                                     WHERE c.ParentCode = e.empcode
                                 )
                                THEN 'Assistant Sales Manager'
                            WHEN e.role = 'Sales' THEN 'Sales Executive'
                            ELSE 'Other'
                        END AS CustRole
                    FROM EmployeeHierarchy e
                ),
                FilteredHierarchy AS
                (
                    SELECT *
                    FROM RoleHierarchy e
                    WHERE
                        e.empcode = @EmployeeCode
                        OR e.HierarchyPath LIKE
                           (
                               SELECT HierarchyPath + '%'
                               FROM EmployeeHierarchy
                               WHERE empcode = @EmployeeCode
                           )
                        OR (
                            SELECT HierarchyPath
                            FROM EmployeeHierarchy
                            WHERE empcode = @EmployeeCode
                        ) LIKE
                            e.HierarchyPath + '%'
                )
                SELECT
                    empcode,
                    name,
                    role,
                    status,
                    ParentCode,
                    SalesTLManager,
                    HierarchyLevel,
                    CustRole,
                    HierarchyPath
                FROM FilteredHierarchy
                ORDER BY HierarchyPath
                OPTION (MAXRECURSION 100);
            ";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);

            con.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new EmployeeNode
                {
                    EmpCode = reader["empcode"] == DBNull.Value ? null : reader["empcode"].ToString(),
                    Name = reader["name"] == DBNull.Value ? null : reader["name"].ToString(),
                    Role = reader["role"] == DBNull.Value ? null : reader["role"].ToString(),
                    CustRole = reader["CustRole"] == DBNull.Value ? null : reader["CustRole"].ToString(),
                    OrgRole = reader["CustRole"] == DBNull.Value ? null : reader["CustRole"].ToString(),
                    Status = reader["status"] == DBNull.Value ? "0" : reader["status"].ToString(),
                    ParentCode = reader["ParentCode"] == DBNull.Value ? null : reader["ParentCode"].ToString(),
                    SalesTLManager = reader["SalesTLManager"] == DBNull.Value ? "0" : reader["SalesTLManager"].ToString(),
                    HierarchyLevel = reader["HierarchyLevel"] == DBNull.Value ? 0 : Convert.ToInt32(reader["HierarchyLevel"]),
                    HierarchyPath = reader["HierarchyPath"] == DBNull.Value ? null : reader["HierarchyPath"].ToString(),
                    Children = new List<EmployeeNode>()
                });
            }

            return list;
        }
    }
}