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
                IF OBJECT_ID('tempdb..#FilteredHierarchy') IS NOT NULL DROP TABLE #FilteredHierarchy;
                ;WITH EmployeeHierarchy AS
                (
                    SELECT e.empcode, e.name, e.role, e.status, e.ProfileImagePath, e.TL_Manager AS ParentCode,
                           ISNULL(e.Sales_TL_Manager, 0) AS SalesTLManager,
                           0 AS HierarchyLevel,
                           CAST('/' + e.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                    FROM employees e
                    WHERE e.isdeleted = 0 AND e.status = 1 AND e.TL_Manager = e.empcode
                    UNION ALL
                    SELECT c.empcode, c.name, c.role, c.status, c.ProfileImagePath, c.TL_Manager AS ParentCode,
                           ISNULL(c.Sales_TL_Manager, 0) AS SalesTLManager,
                           p.HierarchyLevel + 1 AS HierarchyLevel,
                           CAST(p.HierarchyPath + c.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                    FROM employees c
                    INNER JOIN EmployeeHierarchy p ON c.TL_Manager = p.empcode
                    WHERE c.isdeleted = 0 AND c.status = 1 AND c.empcode <> c.TL_Manager
                ),
                CompanyCounts AS
                (
                    SELECT
                        c.sessionname AS EmpCode,
                        COUNT(c.sessionname) AS TotalCompanies,
                        SUM(CASE WHEN LOWER(c.type) = 'paid'   THEN 1 ELSE 0 END) AS PaidCompanies,
                        SUM(CASE WHEN LOWER(c.type) = 'unpaid' THEN 1 ELSE 0 END) AS UnPaidCompanies
                    FROM Company c
                    WHERE c.status = 1
                    GROUP BY c.sessionname
                ),
                RoleHierarchy AS
                (
                    SELECT e.*, LTRIM(RTRIM(e.role)) AS CustRole
                    FROM EmployeeHierarchy e
                )
                SELECT  e.*,
                    ISNULL(cc.TotalCompanies, 0) AS TotalCompanies,
                    ISNULL(cc.PaidCompanies, 0) AS PaidCompanies,
                    ISNULL(cc.UnPaidCompanies, 0) AS UnPaidCompanies,
                    ISNULL(
                        (
                            SELECT SUM(cc2.TotalCompanies)
                            FROM EmployeeHierarchy h
                            INNER JOIN CompanyCounts cc2
                                ON cc2.EmpCode = h.empcode
                            WHERE h.HierarchyPath LIKE e.HierarchyPath + '%'
                              AND h.empcode <> e.empcode
                        ),
                        0
                    ) AS HierarchyTotalCompanies
                INTO #FilteredHierarchy
                FROM RoleHierarchy e
                LEFT JOIN CompanyCounts cc
                    ON cc.EmpCode = e.empcode
                WHERE e.empcode = @EmployeeCode
                   OR e.HierarchyPath LIKE (SELECT HierarchyPath + '%' FROM EmployeeHierarchy WHERE empcode = @EmployeeCode)
                   OR (SELECT HierarchyPath FROM EmployeeHierarchy WHERE empcode = @EmployeeCode) LIKE e.HierarchyPath + '%'
                OPTION (MAXRECURSION 100);
                SELECT empcode, name, role, status, ProfileImagePath, ParentCode, SalesTLManager, HierarchyLevel, 
                CustRole, HierarchyPath,TotalCompanies,PaidCompanies,UnPaidCompanies,HierarchyTotalCompanies
                FROM #FilteredHierarchy
                ORDER BY HierarchyPath;
                DECLARE @LoggedInPath VARCHAR(MAX);
                DECLARE @LoggedInRole VARCHAR(50);
                SELECT @LoggedInPath = HierarchyPath, @LoggedInRole = CustRole
                FROM #FilteredHierarchy
                WHERE empcode = @EmployeeCode;
                SELECT ISNULL(COUNT(*), 0) AS InvoiceRenewal
                FROM InvoiceMain I
                WHERE DATEADD(MONTH, 11, I.invoicedate) BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE))
                  AND (@LoggedInRole = 'CEO'
                       OR EXISTS (SELECT 1 FROM #FilteredHierarchy E
               WHERE E.HierarchyPath LIKE @LoggedInPath + '%' AND E.empcode = I.sessionname));";
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
                    ProfileImagePath = reader["ProfileImagePath"] == DBNull.Value ? null : reader["ProfileImagePath"].ToString(),
                    SelfCompnaies = reader["TotalCompanies"] == DBNull.Value ? null : reader["TotalCompanies"].ToString(),
                    PaidCompanies = reader["PaidCompanies"] == DBNull.Value ? null : reader["PaidCompanies"].ToString(),
                    UnPaidCompanies = reader["UnPaidCompanies"] == DBNull.Value ? null : reader["UnPaidCompanies"].ToString(),
                    TeamTotalCompanies = reader["HierarchyTotalCompanies"] == DBNull.Value ? null : reader["HierarchyTotalCompanies"].ToString(),
                    Children = new List<EmployeeNode>()
                });
            }
            if (reader.NextResult() && reader.Read() && list.Count > 0)
            {
                list[0].InvoiceRenewal = reader["InvoiceRenewal"] == DBNull.Value ? 0 : Convert.ToInt32(reader["InvoiceRenewal"]);
            }
            return list;
        }
   
        public async Task<List<EmployeeNodeInfo>> GetEmployeePerformance(string sessionName,DateTime selectedMonth)
        {
            var result = new List<EmployeeNodeInfo>();

            const string sql = @"
                DECLARE @StartOfMonth DATE = DATEFROMPARTS(YEAR(@SelectedMonth), MONTH(@SelectedMonth), 1);
                DECLARE @StartOfNextMonth DATE = DATEADD(MONTH, 1, @StartOfMonth);

                ;WITH EmployeeHierarchy AS
                (
                    SELECT
                        e.empcode,
                        e.name,
                        e.TL_Manager AS ParentCode,
                        CAST('/' + e.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                    FROM employees e
                    WHERE e.isdeleted = 0
                      AND e.status = 1
                      AND e.empcode = @SessionName

                    UNION ALL

                    SELECT
                        c.empcode,
                        c.name,
                        c.TL_Manager AS ParentCode,
                        CAST(p.HierarchyPath + c.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                    FROM employees c
                    INNER JOIN EmployeeHierarchy p
                        ON c.TL_Manager = p.empcode
                    WHERE c.isdeleted = 0
                      AND c.status = 1
                      AND c.empcode <> c.TL_Manager
                ),
                Meetings AS
                (
                    SELECT
                        sessionname AS EmpCode,
                        SUM(CASE WHEN Type = 'Fresh' THEN 1 ELSE 0 END) AS FreshMeetings,
                        SUM(CASE WHEN Type = 'Follow-up' THEN 1 ELSE 0 END) AS FollowupMeetings,
                        SUM(CASE WHEN Type = 'Services' THEN 1 ELSE 0 END) AS ServicesMeetings,
                        COUNT(*) AS AllMeetings
                    FROM stswlspl.VW_FollowUpRpt
                    WHERE commentdatetime >= @StartOfMonth
                      AND commentdatetime < @StartOfNextMonth
                      AND Updatefor = 'Meeting'
                    GROUP BY sessionname
                ),
                Companies AS
                (
                    SELECT
                        sessionname AS EmpCode,
                        COUNT(*) AS NewCompanies
                    FROM Company
                    WHERE status = 1
                      AND regdate >= @StartOfMonth
                      AND regdate < @StartOfNextMonth
                    GROUP BY sessionname
                ),
                Invoices AS
                (
                    SELECT
                        sessionname AS EmpCode,
                        COUNT(*) AS TotalInvoices
                    FROM InvoiceMain
                    WHERE createddate >= @StartOfMonth
                      AND createddate < @StartOfNextMonth
                    GROUP BY sessionname
                ),
                Proformas AS
                (
                    SELECT
                        sessionname AS EmpCode,
                        COUNT(*) AS TotalProformas
                    FROM tbl_ProformaInvoiceMain
                    WHERE createddate >= @StartOfMonth
                      AND createddate < @StartOfNextMonth
                    GROUP BY sessionname
                )
                SELECT
                    h.empcode,
                    h.name,
                    ISNULL(m.FreshMeetings, 0) AS FreshMeetings,
                    ISNULL(m.FollowupMeetings, 0) AS FollowupMeetings,
                    ISNULL(m.ServicesMeetings, 0) AS ServicesMeetings,
                    ISNULL(m.AllMeetings, 0) AS AllMeetings,
                    ISNULL(c.NewCompanies, 0) AS NewCompanies,
                    ISNULL(i.TotalInvoices, 0) AS TotalInvoices,
                    ISNULL(p.TotalProformas, 0) AS TotalProformas
                FROM EmployeeHierarchy h
                LEFT JOIN Meetings m
                    ON m.EmpCode = h.empcode
                LEFT JOIN Companies c
                    ON c.EmpCode = h.empcode
                LEFT JOIN Invoices i
                    ON i.EmpCode = h.empcode
                LEFT JOIN Proformas p
                    ON p.EmpCode = h.empcode
                ORDER BY h.HierarchyPath
                OPTION (MAXRECURSION 100);";

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@SessionName", sessionName);
            cmd.Parameters.AddWithValue("@SelectedMonth", selectedMonth);

            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new EmployeeNodeInfo
                {
                    EmployeeCode = reader["empcode"]?.ToString() ?? "",
                    EmployeeName = reader["name"]?.ToString() ?? "",

                    Fresh = reader["FreshMeetings"]?.ToString() ?? "0",
                    FollowUp = reader["FollowupMeetings"]?.ToString() ?? "0",
                    Service = reader["ServicesMeetings"]?.ToString() ?? "0",
                    Total = reader["AllMeetings"]?.ToString() ?? "0",

                    NewCompanies = reader["NewCompanies"]?.ToString() ?? "0",
                    NewInvoice = reader["TotalInvoices"]?.ToString() ?? "0",
                    NewProforma = reader["TotalProformas"]?.ToString() ?? "0"
                });
            }

            return result;
        }

        public List<InvoiceRenewalModel> GetInvoiceRenewals(string employeeCode, bool isAdmin)
        {
            var list = new List<InvoiceRenewalModel>();
            const string sql = @"
            WITH EmployeeHierarchy AS
            (
                SELECT e.empcode, e.name, e.role, e.status, e.TL_Manager AS ParentCode,
                       ISNULL(e.Sales_TL_Manager, 0) AS SalesTLManager,
                       0 AS HierarchyLevel,
                       CAST('/' + e.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                FROM employees e
                WHERE e.isdeleted = 0 AND e.status = 1 AND e.TL_Manager = e.empcode
                UNION ALL
                SELECT c.empcode, c.name, c.role, c.status, c.TL_Manager AS ParentCode,
                       ISNULL(c.Sales_TL_Manager, 0) AS SalesTLManager,
                       p.HierarchyLevel + 1 AS HierarchyLevel,
                       CAST(p.HierarchyPath + c.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                FROM employees c
                INNER JOIN EmployeeHierarchy p ON c.TL_Manager = p.empcode
                WHERE c.isdeleted = 0 AND c.status = 1 AND c.empcode <> c.TL_Manager
            ),
            RoleHierarchy AS
            (
                SELECT e.*, LTRIM(RTRIM(e.role)) AS CustRole
                FROM EmployeeHierarchy e
            ),
            MyTeam AS
            (
                SELECT child.empcode
                FROM RoleHierarchy child
                CROSS JOIN (SELECT * FROM RoleHierarchy WHERE empcode = @EmployeeCode) loggedIn
                WHERE loggedIn.CustRole = 'CEO'
                   OR child.HierarchyPath LIKE loggedIn.HierarchyPath + '%'
            )
            SELECT I.id, I.invoiceno, I.companyname, E.name AS SalesPerson, I.sessionname, I.invoicedate,
                   DATEADD(MONTH, 11, I.invoicedate) AS RenewalDate,
                   DATEDIFF(DAY, GETDATE(), DATEADD(MONTH, 11, I.invoicedate)) AS DaysLeft,
                   I.totalamtaftertax
            FROM InvoiceMain I
            LEFT JOIN employees E ON I.sessionname = E.empcode
            WHERE DATEADD(MONTH, 11, I.invoicedate) BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE))
              AND (@IsAdmin = 1 OR I.sessionname IN (SELECT empcode FROM MyTeam))
            ORDER BY DaysLeft ASC
            OPTION (MAXRECURSION 100);";
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@IsAdmin", isAdmin ? 1 : 0);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new InvoiceRenewalModel
                {
                    Id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]),
                    InvoiceNo = reader["invoiceno"] == DBNull.Value ? string.Empty : reader["invoiceno"].ToString(),
                    CustomerName = reader["companyname"] == DBNull.Value ? string.Empty : reader["companyname"].ToString(),
                    SalesPerson = reader["SalesPerson"] == DBNull.Value ? string.Empty : reader["SalesPerson"].ToString(),
                    SessionName = reader["sessionname"] == DBNull.Value ? string.Empty : reader["sessionname"].ToString(),
                    InvoiceDate = reader["invoicedate"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy"),
                    RenewalDate = reader["RenewalDate"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["RenewalDate"]).ToString("dd-MM-yyyy"),
                    DaysRemaining = reader["DaysLeft"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DaysLeft"]),
                    TotalAmount = reader["totalamtaftertax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamtaftertax"])
                });
            }
            return list;
        }
    }
}