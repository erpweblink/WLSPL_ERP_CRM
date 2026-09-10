using Microsoft.Data.SqlClient;
using WEBLINK_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public class ShortcutRepo : IShortcutRepo
    {
        private readonly IConfiguration _configuration;
        public ShortcutRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<List<ShortcutItem>> GetShortcutsAsync()
        {
            var items = GetAll();
            return Task.FromResult(items);
        }

        public List<ShortcutItem> GetAll()
        {
            return new List<ShortcutItem>
            {
                    new ShortcutItem
                    {
                        Name        = "Dashboard",
                        Keywords    = new List<string> { "dashboard", "home", "summary", "overview" },
                        Url         = "/Dashboard/Index",
                        Icon        = "las la-tachometer-alt",
                        Description = "View dashboard"
                    },
                    new ShortcutItem
                    {
                        Name        = "User",
                        Keywords    = new List<string> { "user", "us", "li" },
                        Url         = "/UserMaster/Index",
                        Icon        = "las la-users",
                        Description = "View user list"
                    },
                    new ShortcutItem
                    {
                        Name        = "User Master",
                        Keywords    = new List<string> { "user master", "u", "mas" },
                        Url         = "/UserMaster/Create",
                        Icon        = "las la-user-plus",
                        Description = "Create new user"
                    },
                    new ShortcutItem
                    {
                        Name        = "Quotation",
                        Keywords    = new List<string> { "quotation", "quote", "quo" },
                        Url         = "/Quotation/Index",
                        Icon        = "las la-file-signature",
                        Description = "View quotation list"
                    },
                    new ShortcutItem
                    {
                        Name        = "Quotation Master",
                        Keywords    = new List<string> { "quotation master", "quote" },
                        Url         = "/Quotation/Create",
                        Icon        = "las la-edit",
                        Description = "Create new quotation"
                    },
                    new ShortcutItem
                    {
                        Name        = "Invoice Master",
                        Keywords    = new List<string> { "invoice", "inv", "billing" },
                        Url         = "/TaxInvoice/Create",
                        Icon        = "las la-file-invoice-dollar",
                        Description = "Create new invoice"
                    },
                    new ShortcutItem
                    {
                        Name        = "Invoice List",
                        Keywords    = new List<string> { "invoice list", "invoices" },
                        Url         = "/TaxInvoice/Index",
                        Icon        = "las la-list-alt",
                        Description = "View invoice list"
                    },
                    new ShortcutItem
                    {
                        Name        = "Proforma",
                        Keywords    = new List<string> { "proforma", "pro" },
                        Url         = "/Proforma/Create",
                        Icon        = "las la-file-alt",
                        Description = "Create new proforma"
                    },
                    new ShortcutItem
                    {
                        Name        = "Proforma List",
                        Keywords    = new List<string> { "proforma list", "proformas" },
                        Url         = "/Proforma/Index",
                        Icon        = "las la-clipboard-list",
                        Description = "View proforma list"
                    },
                    new ShortcutItem
                    {
                        Name        = "Company",
                        Keywords    = new List<string> { "company", "companies" },
                        Url         = "/Companymaster/Create",
                        Icon        = "las la-building",
                        Description = "Create new company"
                    },
                    new ShortcutItem
                    {
                        Name        = "Company List",
                        Keywords    = new List<string> { "company list", "companies" },
                        Url         = "/Companymaster/Index",
                        Icon        = "las la-list",
                        Description = "View company list"
                    }
            };
        }

        public async Task<List<ShortcutItem>> SearchEmployees(string q, string userRole, string userCode)
        {
            var results = new List<ShortcutItem>();

            if (string.IsNullOrWhiteSpace(q))
                return results;

            using var con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")
                ?? throw new Exception("Connection string 'Conn_Stringg' not found."));

            string query;

            if (userRole == "Admin" || userRole == "SubAdmin")
            {
                query = @"
                    SELECT empcode AS EmpCode, name AS EmpName, id AS Id
                    FROM employees
                    WHERE (empcode LIKE @Search OR name LIKE @Search)";
            }
            else
            {
                query = @"
                    WITH EmployeeHierarchy AS
                    (
                        -- Start: the logged-in employee
                        SELECT empcode, name, id, TL_Manager
                        FROM employees
                        WHERE empcode = @UserCode

                        UNION ALL

                        -- Recurse: find all employees whose TL_Manager is someone in the hierarchy
                        SELECT e.empcode, e.name, e.id, e.TL_Manager
                        FROM employees e
                        INNER JOIN EmployeeHierarchy eh ON e.TL_Manager = eh.empcode
                    )
                    SELECT empcode AS EmpCode, name AS EmpName, id AS Id
                    FROM EmployeeHierarchy
                    WHERE (empcode LIKE @Search OR name LIKE @Search)";
            }

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Search", $"%{q}%");

            if (userRole != "Admin" && userRole != "SubAdmin")
                cmd.Parameters.AddWithValue("@UserCode", userCode);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new ShortcutItem
                {
                    Name = "Edit: " + reader["EmpName"].ToString(),
                    Description = "Emp Code: " + reader["EmpCode"].ToString(),
                    Url = "/UserMaster/Edit/" + reader["Id"].ToString(),
                    Icon = "las la-user-edit",
                    Keywords = new List<string>()
                });
            }

            return results;
        }

        public async Task<List<ShortcutItem>> SearchCompanies(string q, string userRole, string userCode)
        {
            var results = new List<ShortcutItem>();

            if (string.IsNullOrWhiteSpace(q))
                return results;

            using var con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")
                ?? throw new Exception("Connection string 'Conn_Stringg' not found."));

            string query;

            if (userRole == "Admin")
            {
                query = @"
                    SELECT c.id          AS id,
                           c.ccode       AS company_code,
                           c.cname       AS company_name,
                           e.name        AS empname,
                           c.sessionname AS empcode
                    FROM Company c
                    INNER JOIN employees e ON c.sessionname = e.empcode
                    WHERE c.cname LIKE @Search";
            }
            else if (userRole == "SubAdmin")
            {
                query = @"
                    SELECT c.id          AS id,
                           c.ccode       AS company_code,
                           c.cname       AS company_name,
                           e.name        AS empname,
                           c.sessionname AS empcode
                    FROM Company c
                    INNER JOIN employees e ON c.sessionname = e.empcode
                    WHERE c.sessionname = @UserCode
                    AND c.cname LIKE @Search";
            }
            else
            {
                query = @"
                    WITH EmployeeHierarchy AS
                    (
                        SELECT empcode, name
                        FROM employees
                        WHERE empcode = @UserCode

                        UNION ALL

                        SELECT e.empcode, e.name
                        FROM employees e
                        INNER JOIN EmployeeHierarchy eh ON e.TL_Manager = eh.empcode
                    )
                    SELECT c.id          AS id,
                           c.ccode       AS company_code,
                           c.cname       AS company_name,
                           eh.name       AS empname,
                           c.sessionname AS empcode
                    FROM company c
                    INNER JOIN EmployeeHierarchy eh ON c.sessionname = eh.empcode
                    WHERE c.cname LIKE @Search";
            }

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Search", $"%{q}%");

            if (userRole != "Admin")
                cmd.Parameters.AddWithValue("@UserCode", userCode);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new ShortcutItem
                {
                    Name = "Edit: " + reader["company_name"].ToString(),
                    Description = "Emp Name: " + reader["empname"] + " (" + reader["empcode"] + ")",
                    Url = "/Companymaster/Edit/" + reader["id"].ToString(),
                    Icon = "las la-building",
                    Keywords = new List<string>()
                });
            }

            return results;
        }
    }
}
