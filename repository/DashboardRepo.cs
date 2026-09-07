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
                           -- WHEN e.HierarchyLevel >= 3 AND e.SalesTLManager = 1 THEN 'Sales TL'
                            WHEN e.HierarchyLevel >= 3 AND (
                                     e.SalesTLManager = 1
                                     OR EXISTS (
                                         SELECT 1
                                         FROM EmployeeHierarchy c
                                         WHERE c.ParentCode = e.empcode
                                     )
                                 )
                            THEN 'Sales TL'
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

        public Task<EmployeeNodeInfo> GetEmployeeCompanies(string sessionName)
        {
            var info = new EmployeeNodeInfo();
            const string sql = @"
                --- Company Details
                SELECT e.empcode AS EmployeeCode , e.name AS EmployeeName, COUNT(c.sessionname) AS TotalCompanies,
                SUM(CASE WHEN LOWER(c.type) = 'paid' THEN 1 ELSE 0 END) AS PaidCompanies,
                SUM(CASE WHEN LOWER(c.type) = 'unpaid' THEN 1 ELSE 0 END) AS UnPaidCompanies
                FROM employees e LEFT JOIN Company c 
                ON c.sessionname = e.empcode AND c.status = 1
                WHERE e.empcode = @SessionName AND e.isdeleted = 0 AND e.status = 1
                GROUP BY e.empcode, e.name, e.role;
 
                --- Meeting details
                SELECT name, DATENAME(month, GETDATE()) AS CurrentMonth, 
                SUM(CASE WHEN Type = 'Fresh' THEN MeetingNo ELSE 0 END) AS FreshMeetings,
                SUM(CASE WHEN Type = 'Follow-up' THEN MeetingNo ELSE 0 END) AS FollowupMeetings,
                SUM(CASE WHEN Type = 'Services' THEN MeetingNo ELSE 0 END) AS ServicesMeetings,
                SUM(MeetingNo) AS AllMeetings FROM (SELECT COUNT(cname) AS MeetingNo, name, Type 
                FROM stswlspl.VW_FollowUpRpt WHERE sessionname = @SessionName
                AND commentdatetime >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0) 
                AND commentdatetime < DATEADD(month, DATEDIFF(month, 0, GETDATE()) + 1, 0) 
                AND Updatefor = 'Meeting' 
                GROUP BY name, Type) AS CombinedResults 
                GROUP BY name;";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SessionName", sessionName);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                info.EmployeeCode = reader["EmployeeCode"] == DBNull.Value ? "0" : reader["EmployeeCode"].ToString();
                info.EmployeeName = reader["EmployeeName"] == DBNull.Value ? "0" : reader["EmployeeName"].ToString();
                info.TotalCompanies = reader["TotalCompanies"] == DBNull.Value ? "0" : reader["TotalCompanies"].ToString();
                info.PaidCompanies = reader["PaidCompanies"] == DBNull.Value ? "0" : reader["PaidCompanies"].ToString();
                info.UnPaidCompanies = reader["UnPaidCompanies"] == DBNull.Value ? "0" : reader["UnPaidCompanies"].ToString();
            }

            if (reader.NextResult())
            {
                if (reader.Read())
                {
                    info.Fresh = reader["FreshMeetings"] == DBNull.Value ? "0" : reader["FreshMeetings"].ToString();
                    info.FollowUp = reader["FollowupMeetings"] == DBNull.Value ? "0" : reader["FollowupMeetings"].ToString();
                    info.Service = reader["ServicesMeetings"] == DBNull.Value ? "0" : reader["ServicesMeetings"].ToString();
                    info.Total = reader["AllMeetings"] == DBNull.Value ? "0" : reader["AllMeetings"].ToString();
                }
            }

            return Task.FromResult(info);
        }
    }
}