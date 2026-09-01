using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class UserRepository : IUserRepository
    {

        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ================= GET ALL USERS =================

        public List<RegisterUserr> GetAllUsers()
        {

            List<RegisterUserr> list = new();


            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));


            string query = @"
                SELECT *
                FROM employees
                WHERE isdeleted = 0
                ORDER BY id DESC";


            SqlCommand cmd = new SqlCommand(query, con);


            con.Open();


            SqlDataReader dr = cmd.ExecuteReader();



            while (dr.Read())
            {

                list.Add(new RegisterUserr
                {

                    id = Convert.ToInt32(dr["id"]),

                    empcode = dr["empcode"]?.ToString(),

                    name = dr["name"]?.ToString(),

                    email = dr["email"]?.ToString(),

                    mobile = dr["mobile"]?.ToString(),

                    role = dr["role"]?.ToString(),

                    status = Convert.ToBoolean(dr["status"]),

                    UserName = dr["UserName"]?.ToString(),

                    Designation = dr["Designation"]?.ToString(),

                    TL_Manager = dr["TL_Manager"] == DBNull.Value
    ? null
    : dr["TL_Manager"].ToString(),

                    Sales_TL_Manager = dr["Sales_TL_Manager"] != DBNull.Value
    && Convert.ToBoolean(dr["Sales_TL_Manager"]),


                });

            }


            return list;

        }

        // ================= GET USER BY ID =================


        public RegisterUserr GetUserById(int id)
        {

            RegisterUserr user = null;

            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));


            string query = @"
                SELECT *
                FROM employees
                WHERE id=@id";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@id", id);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {

                user = new RegisterUserr
                {

                    id = Convert.ToInt32(dr["id"]),

                    empcode = dr["empcode"]?.ToString(),

                    name = dr["name"]?.ToString(),

                    email = dr["email"]?.ToString(),

                    mobile = dr["mobile"]?.ToString(),

                    role = dr["role"]?.ToString(),

                    emailpsw = dr["emailpsw"]?.ToString(),

                    panelpsw = dr["panelpsw"]?.ToString(),

                    status = Convert.ToBoolean(dr["status"]),

                    UserName = dr["UserName"]?.ToString(),

                    Designation = dr["Designation"]?.ToString(),

                    TL_Manager = dr["TL_Manager"] == DBNull.Value
                            ? null
                            : dr["TL_Manager"].ToString(),

                                            Sales_TL_Manager = dr["Sales_TL_Manager"] != DBNull.Value
                            && Convert.ToBoolean(dr["Sales_TL_Manager"]),

                   ProfileImagePath = dr["ProfileImagePath"] == DBNull.Value
                            ? null
                            : dr["ProfileImagePath"].ToString()

                };

            }
            return user;

        }

        // ================= CREATE USER =================

        public bool CreateUser(RegisterUserr model)
        {

            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            con.Open();



            // Generate Employee Code

            string empQuery = @"
                SELECT TOP 1 empcode
                FROM employees
                WHERE empcode LIKE 'WLSPL/%'
                ORDER BY id DESC";


            SqlCommand empCmd = new SqlCommand(empQuery, con);


            string lastCode = empCmd.ExecuteScalar()?.ToString();



            int nextNo = 1;


            if (!string.IsNullOrEmpty(lastCode))
            {

                string number = lastCode.Split('/')[1];

                nextNo = Convert.ToInt32(number) + 1;

            }



            string empCode = "WLSPL/" + nextNo.ToString("D5");




            string query = @"
            INSERT INTO employees
            (
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
                Designation
            )
            VALUES
            (
                @empcode,
                @name,
                @email,
                @emailpsw,
                @panelpsw,
                @mobile,
                @role,
                @status,
                0,
                GETDATE(),
                @TL_Manager,
                @UserName,
                @Sales_TL_Manager,
                @Designation
            )";



            SqlCommand cmd = new SqlCommand(query, con);



            cmd.Parameters.AddWithValue("@empcode", empCode);

            cmd.Parameters.AddWithValue("@name", model.name ?? "");

            cmd.Parameters.AddWithValue("@email", model.email ?? "");

            cmd.Parameters.AddWithValue("@emailpsw", model.emailpsw ?? "");

            cmd.Parameters.AddWithValue("@panelpsw", model.panelpsw ?? "");

            cmd.Parameters.AddWithValue("@mobile", model.mobile ?? "");

            cmd.Parameters.AddWithValue("@role", model.role ?? "");

            cmd.Parameters.AddWithValue("@status", model.status);

            cmd.Parameters.AddWithValue("@TL_Manager", model.TL_Manager);

            cmd.Parameters.AddWithValue("@UserName", model.UserName ?? "");

            cmd.Parameters.AddWithValue("@Sales_TL_Manager",
     model.Sales_TL_Manager);

            cmd.Parameters.AddWithValue("@Designation",
                model.Designation ?? "");





            return cmd.ExecuteNonQuery() > 0;

        }


        // ================= UPDATE USER =================

        public bool UpdateUser(RegisterUserr model)
        {
            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));


            string query = @"
                UPDATE employees SET

                    name = @name,
                    email = @email,
                    mobile = @mobile,
                    role = @role,
                    status = @status,
                    TL_Manager = @TL_Manager,
                    UserName = @UserName,
                    Sales_TL_Manager = @Sales_TL_Manager,
                    Designation = @Designation

                WHERE id = @id";


            SqlCommand cmd = new SqlCommand(query, con);


            cmd.Parameters.AddWithValue("@id", model.id);

            cmd.Parameters.AddWithValue("@name",
                model.name ?? "");

            cmd.Parameters.AddWithValue("@email",
                model.email ?? "");

            cmd.Parameters.AddWithValue("@mobile",
                model.mobile ?? "");

            cmd.Parameters.AddWithValue("@role",
                model.role ?? "");

            cmd.Parameters.AddWithValue("@status",
                model.status);


            // Stores WLSPL/01
            cmd.Parameters.AddWithValue("@TL_Manager",
                model.TL_Manager ?? (object)DBNull.Value);


            cmd.Parameters.AddWithValue("@UserName",
                model.UserName ?? "");


            // Stores 1 or 0
            cmd.Parameters.AddWithValue("@Sales_TL_Manager",
                model.Sales_TL_Manager);


            cmd.Parameters.AddWithValue("@Designation",
                model.Designation ?? "");


            con.Open();


            return cmd.ExecuteNonQuery() > 0;
        }


        // ================= DELETE USER =================

        public bool DeleteUser(int id)
        {

            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));



            string query =
            "UPDATE employees SET isdeleted=1 WHERE id=@id";



            SqlCommand cmd = new SqlCommand(query, con);


            cmd.Parameters.AddWithValue("@id", id);



            con.Open();


            return cmd.ExecuteNonQuery() > 0;

        }

        // ================= SALES TL DROPDOWN =================

        public List<SelectListItem> GetSalesTLManagers()
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            string query = @"
        SELECT empcode, name
        FROM employees
        WHERE Sales_TL_Manager = 1
        AND isdeleted = 0
        ORDER BY name";

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = dr["empcode"].ToString(),
                    Text = dr["name"].ToString()
                });
            }

            return list;
        }


        public bool UpdateUserAvatar(int userId, string avatarPath)
        {
            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));
            string query = @"
                UPDATE employees
                SET ProfileImagePath = @avatarPath
                WHERE id = @userId";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@avatarPath", avatarPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@userId", userId);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ================= UPDATE USER =================

        public bool UpdateUserProfile(RegisterUserr model)
        {
            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));


            string query = @"
                UPDATE employees SET
                    name = @name,
                    email = @email,
                    emailpsw = @emailpsw,
                    mobile = @mobile,
                    UserName = @UserName,
                    panelpsw = @panelpsw

                WHERE id = @id";


            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@id", model.id);
            cmd.Parameters.AddWithValue("@name",model.name ?? "");
            cmd.Parameters.AddWithValue("@email",model.email ?? "");
            cmd.Parameters.AddWithValue("@emailpsw",model.emailpsw ?? "");       
            cmd.Parameters.AddWithValue("@mobile",model.mobile ?? "");
            cmd.Parameters.AddWithValue("@UserName", model.UserName ?? "");
            cmd.Parameters.AddWithValue("@panelpsw", model.panelpsw ?? "");

            con.Open();


            return cmd.ExecuteNonQuery() > 0;
        }

    }
}
