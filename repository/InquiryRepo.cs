using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Text.Json;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class InquiryRepo : IinquiryRepo
    {
        private readonly IConfiguration _configuration;
        public InquiryRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Inquiry>> GetInquiries()
        {
            string apiToken = "your_super_secret_token_12345";

            string apiUrl =
                "https://www.weblinkservices.net/career/inquiry/wlspl-api/inquiry_api.php"
                + "?token="
                + Uri.EscapeDataString(apiToken);

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("API-Token", apiToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync(apiUrl);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse =
                JsonSerializer.Deserialize<InquiryApiResponse>(
                    json,
                    options
                );

            if (apiResponse?.Data == null)
            {
                return new List<Inquiry>();
            }

            var inquiries = apiResponse.Data.Select(x => new Inquiry
            {
                Id = x.Id,

                Name = x.Name,

                Email = x.Email,

                MobileNumber = x.MobileNo?.ToString(),

                City = x.Location,

                ServiceRequested = x.Inquiry,

                Date = x.Date,

                InquiryText = x.Remarks,

                CreatedAt = x.CreatedAt,

                PageUrl = x.PageUrl,

                // These don't exist in the API response
                Department = null,
                SourceUrl = null,
                SavedAt = null,
                SalesEmpCode = null
            }).ToList();

            return inquiries;
        }

        public async Task<List<Inquiry>> GetInquiriesFromDatabase()
        {
            var inquiries = new List<Inquiry>();

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection con =
                new SqlConnection(connectionString);

            using SqlCommand cmd =
                new SqlCommand("SP_InsertInquiry", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue(
                  "@Action", "Getinquiry");

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                inquiries.Add(new Inquiry
                {
                    Id = reader["Id"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["Id"]),

                    Department = reader["Department"] == DBNull.Value
                        ? null
                        : reader["Department"].ToString(),

                    Name = reader["Name"] == DBNull.Value
                        ? null
                        : reader["Name"].ToString(),

                    Email = reader["Email"] == DBNull.Value
                        ? null
                        : reader["Email"].ToString(),

                    MobileNumber = reader["MobileNumber"] == DBNull.Value
                        ? null
                        : reader["MobileNumber"].ToString(),

                    City = reader["City"] == DBNull.Value
                        ? null
                        : reader["City"].ToString(),

                    ServiceRequested = reader["ServiceRequested"] == DBNull.Value
                        ? null
                        : reader["ServiceRequested"].ToString(),

                    SourceUrl = reader["SourceUrl"] == DBNull.Value
                        ? null
                        : reader["SourceUrl"].ToString(),

                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : reader["CreatedAt"].ToString(),

                    SavedAt = reader["SavedAt"] == DBNull.Value
                        ? null
                        : reader["SavedAt"].ToString(),
                    SalesEmpCode = reader["AssignTo"] == DBNull.Value
                ? null
                : reader["AssignTo"].ToString()
                });
            }

            return inquiries;
        }

        public async Task<List<Employee>> GetSalesPersons()
        {
            var employees = new List<Employee>();

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection con =
                new SqlConnection(connectionString);

            using SqlCommand cmd =
                new SqlCommand("SP_InsertInquiry", con);

            cmd.CommandType = CommandType.StoredProcedure;

            // IMPORTANT
            cmd.Parameters.AddWithValue(
                "@Action",
                "Getsalespersons"
            );

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    id = reader["id"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["id"]),

                    empcode = reader["empcode"] == DBNull.Value
                        ? null
                        : reader["empcode"].ToString(),

                    name = reader["name"] == DBNull.Value
                        ? null
                        : reader["name"].ToString(),

                    email = reader["email"] == DBNull.Value
                        ? null
                        : reader["email"].ToString(),

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
                        ? false
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
                        : reader["Designation"].ToString()
                });
            }

            return employees;
        }



        public async Task<int> Insertinquiry(Inquiry Model, string Action)
        {
            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection connection =
                new SqlConnection(connectionString);

            using SqlCommand command =
                new SqlCommand("SP_InsertInquiry", connection);

            command.CommandType = CommandType.StoredProcedure;

            // Id
            command.Parameters.AddWithValue(
                "@Id",
                Model.Id ?? (object)DBNull.Value
            );

            // Department
            command.Parameters.AddWithValue(
                "@Department",
                Model.Department ?? (object)DBNull.Value
            );

            // Name
            command.Parameters.AddWithValue(
                "@Name",
                Model.Name ?? (object)DBNull.Value
            );

            // Email
            command.Parameters.AddWithValue(
                "@Email",
                Model.Email ?? (object)DBNull.Value
            );

            // Mobile Number
            command.Parameters.AddWithValue(
                "@MobileNumber",
                Model.MobileNumber ?? (object)DBNull.Value
            );

            // City
            command.Parameters.AddWithValue(
                "@City",
                Model.City ?? (object)DBNull.Value
            );

            // Service Requested
            command.Parameters.AddWithValue(
                "@ServiceRequested",
                Model.ServiceRequested ?? (object)DBNull.Value
            );

            // Source URL
            command.Parameters.AddWithValue(
                "@SourceUrl",
                Model.SourceUrl ?? (object)DBNull.Value
            );

            // Created At
            DateTime? createdAt = null;

            if (DateTime.TryParse(Model.CreatedAt, out DateTime parsedCreatedAt))
            {
                createdAt = parsedCreatedAt;
            }
            else if (DateTime.TryParse(Model.Date, out DateTime parsedDate))
            {
                createdAt = parsedDate;
            }

            command.Parameters.AddWithValue(
                "@CreatedAt",
                createdAt ?? (object)DBNull.Value
            );

            // Action
            command.Parameters.AddWithValue(
                "@Action",
                Action ?? (object)DBNull.Value
            );

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToInt32(reader["Success"]);
            }

            return 0;
        }


        //public async Task<List<Inquiry>> GetWhatsappInquiries()
        //{


        //    string apiToken =
        //        "your_super_secret_token_12345";

        //    string apiUrl =
        //        "https://www.weblinkservices.net/career/inquiry/wlspl-api/whatsapp_api.php"
        //        + "?token=" + Uri.EscapeDataString(apiToken);

        //    using var client = new HttpClient();

        //    client.DefaultRequestHeaders.Add("Accept", "application/json");

        //    var response = await client.GetAsync(apiUrl);

        //    var json = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw new HttpRequestException(
        //            $"WhatsApp API returned {(int)response.StatusCode} " +
        //            $"{response.ReasonPhrase}. Response: {json}"
        //        );
        //    }

        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    };

        //    var apiResponse =
        //        JsonSerializer.Deserialize<InquiryApiResponse>(
        //            json,
        //            options
        //        );

        //    return apiResponse?.Data ?? new List<Inquiry>();
        //}


        public async Task<List<Inquiry>> GetWhatsappInquiries()
        {
            string apiToken =
                "your_super_secret_token_12345";

            string apiUrl =
                "https://www.weblinkservices.net/career/inquiry/wlspl-api/whatsapp_api.php"
                + "?token="
                + Uri.EscapeDataString(apiToken);

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("API-Token", apiToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync(apiUrl);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"WhatsApp API returned {(int)response.StatusCode} " +
                    $"{response.ReasonPhrase}. Response: {json}"
                );
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse =
                JsonSerializer.Deserialize<WhatsappApiResponse>(
                    json,
                    options
                );

            if (apiResponse?.Data == null)
            {
                return new List<Inquiry>();
            }

            return apiResponse.Data.Select(x => new Inquiry
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                MobileNumber = x.MobileNumber?.ToString(),
                City = x.City,
                ServiceRequested = x.ServiceRequested,
                Date = x.CreatedAt,

            }).ToList();
        }
        public async Task<List<Inquiry>> GetWhatsappInquiriesFromDatabase()
        {
            var inquiries = new List<Inquiry>();

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection con =
                new SqlConnection(connectionString);

            using SqlCommand cmd =
                new SqlCommand("SP_InsertInquiry", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Action",
                "GetWhatsappInquiry"
            );

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                inquiries.Add(new Inquiry
                {
                    Id = reader["ID"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["ID"]),

                    Department = reader["Department"] == DBNull.Value
                        ? null
                        : reader["Department"].ToString(),

                    Name = reader["Name"] == DBNull.Value
                        ? null
                        : reader["Name"].ToString(),

                    Email = reader["Email"] == DBNull.Value
                        ? null
                        : reader["Email"].ToString(),

                    MobileNumber = reader["MobileNumber"] == DBNull.Value
                        ? null
                        : reader["MobileNumber"].ToString(),

                    City = reader["City"] == DBNull.Value
                        ? null
                        : reader["City"].ToString(),

                    ServiceRequested = reader["ServiceRequested"] == DBNull.Value
                        ? null
                        : reader["ServiceRequested"].ToString(),

                    SourceUrl = reader["SourceUrl"] == DBNull.Value
                        ? null
                        : reader["SourceUrl"].ToString(),

                    CreatedAt = reader["CreatedAt"] == DBNull.Value
                        ? null
                        : reader["CreatedAt"].ToString(),

                    SalesEmpCode = reader["AssignTo"] == DBNull.Value
                ? null
                : reader["AssignTo"].ToString()
                });
            }

            return inquiries;
        }
        public async Task<int> InsertWhatsappinquiry(Inquiry Model, string Action = "Insertwhatsappinquiry")
        {
            try
            {
                string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand("SP_InsertInquiry", connection);

                command.CommandType = CommandType.StoredProcedure;

                // Id
                command.Parameters.AddWithValue(
                    "@Id",
                    Model.Id ?? (object)DBNull.Value
                );

                command.Parameters.Add(
        "@Department",
        SqlDbType.NVarChar,
        100
    ).Value = (object?)Model.Department ?? DBNull.Value;
                // Name
                command.Parameters.AddWithValue(
                    "@Name",
                    Model.Name ?? (object)DBNull.Value
                );

                // Email
                command.Parameters.AddWithValue(
                    "@Email",
                    Model.Email ?? (object)DBNull.Value
                );

                // Mobile Number
                command.Parameters.AddWithValue(
                    "@MobileNumber",
                    Model.MobileNumber?.ToString()
                    ?? (object)DBNull.Value
                );

                // City
                command.Parameters.AddWithValue(
                    "@City",
                    Model.Location ?? (object)DBNull.Value
                );

                // Inquiry
                command.Parameters.AddWithValue(
                    "@ServiceRequested",
                    Model.ServiceRequested ?? (object)DBNull.Value
                );

                // Source URL
                command.Parameters.AddWithValue(
                    "@SourceUrl",
                    Model.PageUrl ?? (object)DBNull.Value
                );

                // Created At
                DateTime? createdAt = null;

                if (DateTime.TryParse(Model.CreatedAt, out DateTime parsedCreatedAt))
                {
                    createdAt = parsedCreatedAt;
                }
                else if (DateTime.TryParse(Model.Date, out DateTime parsedDate))
                {
                    createdAt = parsedDate;
                }

                command.Parameters.AddWithValue(
                    "@CreatedAt",
                    createdAt ?? (object)DBNull.Value
                );


                // Action
                command.Parameters.AddWithValue(
                    "@Action",
                    Action ?? (object)DBNull.Value
                );

                await connection.OpenAsync();


                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return Convert.ToInt32(reader["Success"]);
                }

                return 0;
            }

            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<int> AssignSalesPerson(
            int inquiryId,
            string salesEmpCode, string Action)
        {
            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection connection =
                new SqlConnection(connectionString);

            using SqlCommand command =
                new SqlCommand("SP_InsertInquiry", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@Id",
                inquiryId
            );

            command.Parameters.AddWithValue(
                "@SalesEmpCode",
                salesEmpCode
            );

            command.Parameters.AddWithValue(
                "@Action",
                Action
            );

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToInt32(reader["Success"]);
            }

            return 0;
        }

        public async Task<List<Inquiry>> Getlead()
        {
            var Lead = new List<Inquiry>();

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using SqlConnection con =
                new SqlConnection(connectionString);

            using SqlCommand cmd =
                new SqlCommand("SP_InsertInquiry", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Action",
                "GetLead"
            );

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Lead.Add(new Inquiry
                {
                    SalesPerson = reader["salesperson"] == DBNull.Value
                        ? null
                        : reader["salesperson"].ToString(),

                    Name = reader["Name"] == DBNull.Value
                        ? null
                        : reader["Name"].ToString(),

                    Email = reader["Email"] == DBNull.Value
                        ? null
                        : reader["Email"].ToString(),

                    MobileNumber = reader["MobileNumber"] == DBNull.Value
                        ? null
                        : reader["MobileNumber"].ToString(),

                    City = reader["City"] == DBNull.Value
                        ? null
                        : reader["City"].ToString(),

                    ServiceRequested = reader["ServiceRequested"] == DBNull.Value
                        ? null
                        : reader["ServiceRequested"].ToString(),
                    // ADD THIS
                    Leadcode = reader["LeadCode"] == DBNull.Value
                ? null
                : reader["LeadCode"].ToString(),
                    Status = reader["Status"] == DBNull.Value
                ? null
                : reader["Status"].ToString()
                });
            }

            return Lead;
        }
    }
}
