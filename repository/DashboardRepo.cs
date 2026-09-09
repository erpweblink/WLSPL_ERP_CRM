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
              IF OBJECT_ID('tempdb..#FilteredHierarchy') IS NOT NULL
               DROP TABLE #FilteredHierarchy;

                ;WITH EmployeeHierarchy AS
                (
                    SELECT
                        e.empcode,
                        e.name,
                        e.role,
                        e.status,
                        e.TL_Manager AS ParentCode,
                        ISNULL(e.Sales_TL_Manager, 0) AS SalesTLManager,
                        0 AS HierarchyLevel,
                        CAST('/' + e.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
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
                            WHEN e.HierarchyLevel = 0
                                THEN 'Admin'

                            WHEN e.role = 'SubAdmin'
                                THEN 'Sub Admin'

                            WHEN e.HierarchyLevel = 1
                                 AND e.role = 'Sales'
                                THEN 'Sales Manager'

                            WHEN e.HierarchyLevel >= 3
                                 AND (
                                     e.SalesTLManager = 1
                                     OR EXISTS
                                     (
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

                            WHEN e.role = 'Sales'
                                THEN 'Sales Executive'

                            ELSE 'Other'
                        END AS CustRole
                    FROM EmployeeHierarchy e
                )

                SELECT *
                INTO #FilteredHierarchy
                FROM RoleHierarchy e
                WHERE
                    -- Logged-in employee
                    e.empcode = @EmployeeCode

                    -- Employees below logged-in employee
                    OR e.HierarchyPath LIKE
                    (
                        SELECT HierarchyPath + '%'
                        FROM EmployeeHierarchy
                        WHERE empcode = @EmployeeCode
                    )

                    -- Ancestors of logged-in employee
                    OR
                    (
                        SELECT HierarchyPath
                        FROM EmployeeHierarchy
                        WHERE empcode = @EmployeeCode
                    ) LIKE e.HierarchyPath + '%'

                OPTION (MAXRECURSION 100);

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
                FROM #FilteredHierarchy
                ORDER BY HierarchyPath;

                DECLARE @LoggedInPath VARCHAR(MAX);
                DECLARE @LoggedInRole VARCHAR(50);

                SELECT
                    @LoggedInPath = HierarchyPath,
                    @LoggedInRole = CustRole
                FROM #FilteredHierarchy
                WHERE empcode = @EmployeeCode;

                SELECT
                    ISNULL(COUNT(*), 0) AS InvoiceRenewal
                FROM InvoiceMain I
                WHERE
                    DATEADD(MONTH, 11, I.invoicedate)
                        BETWEEN CAST(GETDATE() AS DATE)
                        AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE))
                    AND
                    (
                        @LoggedInRole IN ('Admin', 'Sub Admin')
                        OR
                        EXISTS
                        (
                            SELECT 1
                            FROM #FilteredHierarchy E
                            WHERE E.HierarchyPath LIKE @LoggedInPath + '%'
                              AND E.empcode = I.sessionname
                        )
                    );";

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

            if (reader.NextResult() && reader.Read() && list.Count > 0)
            {
                list[0].InvoiceRenewal = reader["InvoiceRenewal"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["InvoiceRenewal"]);
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

        public List<InvoiceRenewalModel> GetInvoiceRenewals(string employeeCode, bool isAdmin)
        {
            var list = new List<InvoiceRenewalModel>();
            const string sql = @"
                  WITH EmployeeHierarchy AS
                        (
                            -- Root employees
                            SELECT
                                e.empcode,
                                e.name,
                                e.role,
                                e.status,
                                e.TL_Manager AS ParentCode,
                                ISNULL(e.Sales_TL_Manager, 0) AS SalesTLManager,
                                0 AS HierarchyLevel,
                                CAST('/' + e.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
                            FROM employees e
                            WHERE e.isdeleted = 0
                              AND e.status = 1
                              AND e.TL_Manager = e.empcode

                            UNION ALL

                            -- Employees under managers
                            SELECT
                                c.empcode,
                                c.name,
                                c.role,
                                c.status,
                                c.TL_Manager AS ParentCode,
                                ISNULL(c.Sales_TL_Manager, 0) AS SalesTLManager,
                                p.HierarchyLevel + 1 AS HierarchyLevel,
                                CAST(p.HierarchyPath + c.empcode + '/' AS VARCHAR(MAX)) AS HierarchyPath
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
                                    WHEN e.HierarchyLevel = 0
                                        THEN 'Admin'

                                    WHEN e.role = 'SubAdmin'
                                        THEN 'Sub Admin'

                                    WHEN e.HierarchyLevel = 1
                                         AND e.role = 'Sales'
                                        THEN 'Sales Manager'

                                    WHEN e.HierarchyLevel >= 3
                                         AND (
                                                e.SalesTLManager = 1
                                                OR EXISTS
                                                (
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

                                    WHEN e.role = 'Sales'
                                        THEN 'Sales Executive'

                                    ELSE 'Other'
                                END AS CustRole
                            FROM EmployeeHierarchy e
                        ),

                        MyTeam AS
                        (
                            SELECT child.empcode
                            FROM RoleHierarchy child
                            CROSS JOIN
                            (
                                SELECT *
                                FROM RoleHierarchy
                                WHERE empcode = @EmployeeCode
                            ) loggedIn
                            WHERE
                                -- Admin / Sub Admin can see everybody
                                loggedIn.CustRole IN ('Admin', 'Sub Admin')

                                OR
                                -- Logged-in employee and everyone below him
                                child.HierarchyPath LIKE loggedIn.HierarchyPath + '%'
                        )

                        SELECT
                            I.id,
                            I.invoiceno,
                            I.companyname,
                            E.name AS SalesPerson,
                            I.sessionname,
                            I.invoicedate,
                            DATEADD(MONTH, 11, I.invoicedate) AS RenewalDate,
                            DATEDIFF(
                                DAY,
                                GETDATE(),
                                DATEADD(MONTH, 11, I.invoicedate)
                            ) AS DaysLeft,
                            I.totalamtaftertax
                        FROM InvoiceMain I
                        LEFT JOIN employees E
                            ON I.sessionname = E.empcode
                        WHERE
                            DATEADD(MONTH, 11, I.invoicedate)
                                BETWEEN CAST(GETDATE() AS DATE)
                                AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE))

                            AND
                            (
                                -- Existing admin flag behavior
                                @IsAdmin = 1

                                OR

                                -- Employee hierarchy
                                I.sessionname IN
                                (
                                    SELECT empcode
                                    FROM MyTeam
                                )
                            )
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
                    InvoiceDate = reader["invoicedate"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy"),
                    RenewalDate = reader["RenewalDate"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToDateTime(reader["RenewalDate"]).ToString("dd-MM-yyyy"),
                    DaysRemaining = reader["DaysLeft"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["DaysLeft"]),
                    TotalAmount = reader["totalamtaftertax"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["totalamtaftertax"])
                });
            }

            return list;
        }

    }
}