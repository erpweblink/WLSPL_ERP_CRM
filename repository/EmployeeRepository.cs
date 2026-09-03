using Microsoft.Data.SqlClient;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _configuration;

        public EmployeeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Employee? Login(string username, string password)
        {
            Employee? employee = null;

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg")
                ?? throw new Exception("Connection string 'Conn_Stringg' not found.");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 1
                        id,
                        empcode,
                        name,
                        email,
                        emailpsw,
                        panelpsw,
                        mobile,
                        role,
                        status,
                        isdeleted,
                        regdate,
                        TL_Manager,
                        UserName,
                        Sales_TL_Manager,
                        Designation,
                        ProfileImagePath
                    FROM employees
                    WHERE UserName = @UserName
                      AND panelpsw = @Password
                      AND ISNULL(isdeleted, 0) = 0
                      AND status = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employee = new Employee
                            {
                                id = Convert.ToInt32(reader["id"]),

                                empcode = reader["empcode"] == DBNull.Value
                                    ? null
                                    : reader["empcode"].ToString(),

                                name = reader["name"] == DBNull.Value
                                    ? null
                                    : reader["name"].ToString(),

                                email = reader["email"] == DBNull.Value
                                    ? null
                                    : reader["email"].ToString(),

                                emailpsw = reader["emailpsw"] == DBNull.Value
                                    ? null
                                    : reader["emailpsw"].ToString(),

                                panelpsw = reader["panelpsw"] == DBNull.Value
                                    ? null
                                    : reader["panelpsw"].ToString(),

                                mobile = reader["mobile"] == DBNull.Value
                                    ? null
                                    : reader["mobile"].ToString(),

                                role = reader["role"] == DBNull.Value
                                    ? null
                                    : reader["role"].ToString(),

                                status = reader["status"] == DBNull.Value
                                    ? null
                                    : reader["status"].ToString(),

                                isdeleted = reader["isdeleted"] == DBNull.Value
                                    ? null
                                    : Convert.ToBoolean(reader["isdeleted"]),

                                regdate = reader["regdate"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["regdate"]),

                                TL_Manager = reader["TL_Manager"] == DBNull.Value
                                    ? null
                                    : reader["TL_Manager"].ToString(),

                                UserName = reader["UserName"] == DBNull.Value
                                    ? null
                                    : reader["UserName"].ToString(),

                                Sales_TL_Manager = reader["Sales_TL_Manager"] == DBNull.Value
                                    ? null
                                    : reader["Sales_TL_Manager"].ToString(),

                                Designation = reader["Designation"] == DBNull.Value
                                    ? null
                                    : reader["Designation"].ToString(),

                                ProfileImagePath= reader["ProfileImagePath"] == DBNull.Value
                                    ? null
                                    : reader["ProfileImagePath"].ToString()
                            };
                        }
                    }
                }
            }

            return employee;
        }

        public Employee GetByEmail(string email)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")
                    ?? throw new Exception("Connection string 'Conn_Stringg' not found."));

            using var cmd = new SqlCommand(
                "SELECT * FROM employees WHERE email = @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Employee
                {
                    id = (int)reader["id"],
                    email = reader["Email"].ToString(),
                    name = reader["Name"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    role = reader["Role"].ToString()
                };
            }
            return null;
        }

        public void UpdatePassword(int employeeId, string newPassword)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")
                    ?? throw new Exception("Connection string 'Conn_Stringg' not found."));

            using var cmd = new SqlCommand(
                "UPDATE employees SET panelpsw = @Password WHERE id = @Id", con);
            cmd.Parameters.AddWithValue("@Password", newPassword); 
            cmd.Parameters.AddWithValue("@Id", employeeId);
            con.Open();
            cmd.ExecuteNonQuery();
        }
    }

}