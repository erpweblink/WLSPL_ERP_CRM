using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class LeadRepository : ILeadRepository
    {
        private readonly IConfiguration _configuration;

        public LeadRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        private string ConnectionString
        {
            get
            {
                return _configuration.GetConnectionString(
                    "Conn_Stringg"
                );
            }
        }


        // =========================================================
        // GET ALL
        // =========================================================
        public List<LeadGenration> GetAllLeads()
        {
            var list = new List<LeadGenration>();

            using SqlConnection con =
                new SqlConnection(ConnectionString);

            string query = @"
                SELECT *
                FROM LeadGenration
                WHERE ISNULL(IsDeleted, 0) = 0
                ORDER BY ID DESC";


            using SqlCommand cmd =
                new SqlCommand(query, con);

            con.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapLead(reader));
            }

            return list;
        }


        // =========================================================
        // GET BY ID
        // =========================================================
        public LeadGenration GetLeadById(int id)
        {
            using SqlConnection con =
                new SqlConnection(ConnectionString);

            string query = @"
                SELECT *
                FROM LeadGenration
                WHERE ID = @ID
                  AND ISNULL(IsDeleted, 0) = 0";


            using SqlCommand cmd =
                new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@ID", id);

            con.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapLead(reader);
            }

            return null;
        }


        // =========================================================
        // CREATE
        // =========================================================
        public bool CreateLead(LeadGenration model)
        {
            using SqlConnection con = new SqlConnection(ConnectionString);

            string query = @"
        INSERT INTO LeadGenration
        (
            Leadcode,
            CompanyName,
            CompanyId,
            Mobile,
            MessageId,
            Email,
            Requirements,
            Product,
            Status,
            Quantity,
            Source,
            City,
            UserName,
            UserID,
            Notes,
            CreatedOn,
            Createdby,
            UpdatedBy,
            UpdatedOn,
            IsDeleted,
            LeadId,
            PageId,
            Type,
            JsonData,
            OwnerName
        )
        VALUES
        (
            @Leadcode,
            @CompanyName,
            @CompanyId,
            @Mobile,
            @MessageId,
            @Email,
            @Requirements,
            @Product,
            @Status,
            @Quantity,
            @Source,
            @City,
            @UserName,
            @UserID,
            @Notes,
            @CreatedOn,
            @Createdby,
            @UpdatedBy,
            @UpdatedOn,
            @IsDeleted,
            @LeadId,
            @PageId,
            @Type,
            @JsonData,
            @OwnerName
        )";

            using SqlCommand cmd = new SqlCommand(query, con);

            // Leadcode
            cmd.Parameters.Add("@Leadcode", SqlDbType.NVarChar, 50).Value =
                (object?)model.Leadcode ?? DBNull.Value;

            // Company
            cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 255).Value =
                model.CompanyName ?? "";

            cmd.Parameters.Add("@CompanyId", SqlDbType.NVarChar, 50).Value =
                (object?)model.CompanyId ?? DBNull.Value;

            // Contact
            cmd.Parameters.Add("@Mobile", SqlDbType.NVarChar, 50).Value =
                model.Mobile ?? "";

            cmd.Parameters.Add("@MessageId", SqlDbType.NVarChar, 255).Value =
                (object?)model.MessageId ?? DBNull.Value;

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value =
                model.Email ?? "";

            // Lead details
            cmd.Parameters.Add("@Requirements", SqlDbType.NVarChar, -1).Value =
                model.Requirements ?? "";

            cmd.Parameters.Add("@Product", SqlDbType.NVarChar, 50).Value =
                model.Product ?? "";

            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                (object?)model.Status ?? "New";

            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value =
                model.Quantity;

            cmd.Parameters.Add("@Source", SqlDbType.NVarChar, 50).Value =
                model.Source ?? "";

            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 50).Value =
                model.City ?? "";

            // User
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value =
                model.UserName ?? "";

            cmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 50).Value =
                (object?)model.UserID ?? DBNull.Value;

            // Notes
            cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value =
                (object?)model.Notes ?? DBNull.Value;

            // Created information
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime).Value =
                model.CreatedOn;

            cmd.Parameters.Add("@Createdby", SqlDbType.NVarChar, 50).Value =
                (object?)model.Createdby ?? "System";

            // Updated information
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 50).Value =
                (object?)model.UpdatedBy ?? DBNull.Value;

            cmd.Parameters.Add("@UpdatedOn", SqlDbType.DateTime).Value =
                (object?)model.UpdatedOn ?? DBNull.Value;

            // Delete flag
            cmd.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value =
                model.IsDeleted;

            // Facebook / Meta information
            cmd.Parameters.Add("@LeadId", SqlDbType.NVarChar, 100).Value =
                (object?)model.LeadId ?? DBNull.Value;

            cmd.Parameters.Add("@PageId", SqlDbType.NVarChar, 100).Value =
                (object?)model.PageId ?? DBNull.Value;

            cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value =
                (object?)model.Type ?? DBNull.Value;

            cmd.Parameters.Add("@JsonData", SqlDbType.NVarChar, -1).Value =
                (object?)model.JsonData ?? DBNull.Value;

            cmd.Parameters.Add("@OwnerName", SqlDbType.NVarChar, 100).Value =
                (object?)model.OwnerName ?? DBNull.Value;

            try
            {
                con.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                // You can put a breakpoint here to see SQL errors
                throw new Exception(
                    "Error while saving Lead: " + ex.Message,
                    ex);
            }
        }


        // =========================================================
        // UPDATE
        // =========================================================
        public bool UpdateLead(LeadGenration model)
        {
            using SqlConnection con = new SqlConnection(ConnectionString);

            string query = @"
        UPDATE LeadGenration
        SET
            Leadcode = @Leadcode,
            CompanyName = @CompanyName,
            CompanyId = @CompanyId,
            Mobile = @Mobile,
            MessageId = @MessageId,
            Email = @Email,
            Requirements = @Requirements,
            Product = @Product,
            Status = @Status,
            Quantity = @Quantity,
            Source = @Source,
            City = @City,
            UserName = @UserName,
            UserID = @UserID,
            Notes = @Notes,

            UpdatedBy = @UpdatedBy,
            UpdatedOn = @UpdatedOn,

            LeadId = @LeadId,
            PageId = @PageId,
            Type = @Type,
            JsonData = @JsonData,
            OwnerName = @OwnerName

        WHERE ID = @ID
          AND IsDeleted = 0";

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@ID", model.ID);

            cmd.Parameters.AddWithValue("@Leadcode",
                (object?)model.Leadcode ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@CompanyName",
                (object?)model.CompanyName ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@CompanyId",
                (object?)model.CompanyId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Mobile",
                (object?)model.Mobile ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@MessageId",
                (object?)model.MessageId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Email",
                (object?)model.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Requirements",
                (object?)model.Requirements ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Product",
                (object?)model.Product ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Status",
                (object?)model.Status ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Quantity", model.Quantity);

            cmd.Parameters.AddWithValue("@Source",
                (object?)model.Source ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@City",
                (object?)model.City ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@UserName",
                (object?)model.UserName ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@UserID",
                (object?)model.UserID ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Notes",
                (object?)model.Notes ?? DBNull.Value);

            // IMPORTANT
            cmd.Parameters.AddWithValue("@UpdatedBy",
                string.IsNullOrEmpty(model.UpdatedBy)
                    ? "System"
                    : model.UpdatedBy);

            // IMPORTANT: Never allow DateTime.MinValue
            cmd.Parameters.AddWithValue(
                "@UpdatedOn",
                model.UpdatedOn == DateTime.MinValue
                    ? DateTime.Now
                    : model.UpdatedOn
            );

            cmd.Parameters.AddWithValue("@LeadId",
                (object?)model.LeadId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@PageId",
                (object?)model.PageId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Type",
                (object?)model.Type ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@JsonData",
                (object?)model.JsonData ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@OwnerName",
                (object?)model.OwnerName ?? DBNull.Value);

            con.Open();

            return cmd.ExecuteNonQuery() > 0;
        }


        // =========================================================
        // DELETE - SOFT DELETE
        // =========================================================
        public bool DeleteLead(int id, string deletedBy)
        {
            using SqlConnection con =
                new SqlConnection(ConnectionString);

            string query = @"
                UPDATE LeadGenration
                SET
                    IsDeleted = 1,
                    DeletedBy = @DeletedBy,
                    DeletedOn = @DeletedOn
                WHERE ID = @ID";


            using SqlCommand cmd =
                new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);
            cmd.Parameters.AddWithValue("@DeletedOn", DateTime.Now);

            con.Open();

            return cmd.ExecuteNonQuery() > 0;
        }


        // =========================================================
        // ADD PARAMETERS
        // =========================================================
        private void AddParameters(
            SqlCommand cmd,
            LeadGenration model)
        {
            cmd.Parameters.AddWithValue(
                "@Leadcode",
                (object?)model.Leadcode ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@CompanyName",
                (object?)model.CompanyName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@CompanyId",
                (object?)model.CompanyId ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Mobile",
                (object?)model.Mobile ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@MessageId",
                (object?)model.MessageId ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Email",
                (object?)model.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Requirements",
                (object?)model.Requirements ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Product",
                (object?)model.Product ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Status",
                (object?)model.Status ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Quantity",
                model.Quantity);

            cmd.Parameters.AddWithValue(
                "@Source",
                (object?)model.Source ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@City",
                (object?)model.City ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@UserName",
                (object?)model.UserName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@UserID",
                (object?)model.UserID ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Notes",
                (object?)model.Notes ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@CreatedOn",
                model.CreatedOn);

            cmd.Parameters.AddWithValue(
                "@Createdby",
                (object?)model.Createdby ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@UpdatedBy",
                (object?)model.UpdatedBy ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@UpdatedOn",
                model.UpdatedOn);

            cmd.Parameters.AddWithValue(
                "@IsDeleted",
                model.IsDeleted);

            cmd.Parameters.AddWithValue(
                "@LeadId",
                (object?)model.LeadId ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@PageId",
                (object?)model.PageId ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Type",
                (object?)model.Type ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@JsonData",
                (object?)model.JsonData ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@OwnerName",
                (object?)model.OwnerName ?? DBNull.Value);
        }


        // =========================================================
        // MAP DATABASE RECORD TO MODEL
        // =========================================================
        private LeadGenration MapLead(SqlDataReader reader)
        {
            return new LeadGenration
            {
                ID = Convert.ToInt32(reader["ID"]),

                Leadcode = GetString(reader, "Leadcode"),
                CompanyName = GetString(reader, "CompanyName"),
                CompanyId = GetString(reader, "CompanyId"),
                Mobile = GetString(reader, "Mobile"),
                MessageId = GetString(reader, "MessageId"),
                Email = GetString(reader, "Email"),
                Requirements = GetString(reader, "Requirements"),
                Product = GetString(reader, "Product"),
                Status = GetString(reader, "Status"),

                Quantity = reader["Quantity"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["Quantity"]),

                Source = GetString(reader, "Source"),
                City = GetString(reader, "City"),
                UserName = GetString(reader, "UserName"),
                UserID = GetString(reader, "UserID"),
                Notes = GetString(reader, "Notes"),

                CreatedOn = GetDateTime(reader, "CreatedOn"),
                Createdby = GetString(reader, "Createdby"),

                UpdatedBy = GetString(reader, "UpdatedBy"),
                UpdatedOn = GetDateTime(reader, "UpdatedOn"),

                IsDeleted = reader["IsDeleted"] != DBNull.Value &&
                            Convert.ToBoolean(reader["IsDeleted"]),

                DeletedBy = GetString(reader, "DeletedBy"),
                DeletedOn = GetDateTime(reader, "DeletedOn"),

                LeadId = GetString(reader, "LeadId"),
                PageId = GetString(reader, "PageId"),
                Type = GetString(reader, "Type"),
                JsonData = GetString(reader, "JsonData"),
                OwnerName = GetString(reader, "OwnerName")
            };
        }


        private string GetString(
            SqlDataReader reader,
            string column)
        {
            return reader[column] == DBNull.Value
                ? null
                : reader[column].ToString();
        }


        private DateTime GetDateTime(
            SqlDataReader reader,
            string column)
        {
            return reader[column] == DBNull.Value
                ? DateTime.MinValue
                : Convert.ToDateTime(reader[column]);
        }
    }
}