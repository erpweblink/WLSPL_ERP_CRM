using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.Company;

namespace WEBLINK_CRM.repository
{

    public class CompanymasterRepo : IcomapnymasterRepo
    {
        private readonly IConfiguration _configuration;

        public CompanymasterRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<Company>> checkcomapnies(string action, string company, string GstNo)
        {
          
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Action", action);
                parameters.Add("@CompanyName", company);
                parameters.Add("@GSTNo", GstNo);
                var result = await connection.QueryAsync<Company>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
        }
        public async Task<int> DeleteReord(string ID, string CreatedBy)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@id", ID);
                parameters.Add("@Action", "DeleteRecords");

                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync("SP_companymasterAWS", parameters, commandType: CommandType.StoredProcedure);
                int isSuccess = parameters.Get<int>("@Result");
                return isSuccess;
            }
        }
        public async Task<Companymaster> GetcompanybyId(string Id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@id", Id);
                parameters.Add("@Action", "GetcompanybyID");

                var data = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_companymasterAWS",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (data == null)
                    return null;

                var company = new Companymaster
                {
                    Id = Convert.ToInt32(data.id),

                    CCode = data.ccode,
                    CName = data.cname,
                    OName = data.oname,
                    State = data.State,
                    Email = data.email,
                    Email2 = data.email2,
                    Mobile = data.mobile,
                    CountryName = data.CountryName,
                    typess = data.type,
                    Category = data.Category,
                    GSTNo = data.gstno,
                    VisitDate = data.visitdate,

                    Address = data.address,
                    ShippingAddress = data.shippingaddress,
                    BillingAddress = data.BillingAddress,

                    BillingLocation = data.BillingLocation,
                    ShippingLocation = data.ShippingLocation,

                    BillingPincode = data.BillingPincode,
                    ShippingPincode = data.ShippingPincode,

                    BillingStateCode = data.BillingStateCode,
                    ShippingStateCode = data.ShippingStateCode,

                    RegisterType = data.RegisterType,
                    EInvTypeOfSupply = data.EInvTypeOfSupply,

                    Area = data.area,
                    Website = data.website,
                    BDE = data.BDE,

                    IsDeleted = data.isdeleted,

                    RegDate = data.regdate,
                    UpdatedDate = data.updateddate,
                    UpdatedBy = data.updatedby
                };

                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<dynamic>> GetcompanyName(string Name)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyName", Name);
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM [Company] WHERE cname LIKE '%'+ @CompanyName + '%'",
                    parameters);
                return result.ToList();
            }
        }

        public async Task<string> Getcompcode(string Action)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@Action", Action);
                    var result = await connection.QueryFirstOrDefaultAsync<string>(
                        "SP_companymaster",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    return result.ToString();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<List<Company>> GetLeadlist(string Action, Company Model)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@Action", Action);
                var result = await connection.QueryAsync<Company>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
        }

        public async Task<List<Company>> SearchCompanyAsync(string Action)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyName", Action);
                parameters.Add("@Action", "SearchCompany");

                var result = await connection.QueryAsync<Company>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        private static DataTable ToContactDataTable(List<ContactModel> contacts)
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("MobileNo", typeof(string));
            table.Columns.Add("EmailID", typeof(string));
            table.Columns.Add("Designation", typeof(string));

            foreach (var contact in contacts)
            {
                table.Rows.Add(contact.Name, contact.MobileNo, contact.EmailID, contact.Designation);
            }

            return table;
        }

        public async Task<List<Companymaster>> GetcompanyList(Companymaster model, string action)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Mode", action);

                var result = await connection.QueryAsync<Companymaster>(
                    "stswlspl.SP_CompnayList",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<List<Companymaster>> GetFilteredcompanyList(Companymaster model)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                string query = @"WITH Employee AS( 
                                    SELECT id, ccode, cname, oname, email, mobile,
                                         visitingcard, TRIM(type) AS typess, sessionname
                                       FROM Company   
                                       WHERE (
                                            @EmpCode = '' OR @EmpCode IS NULL OR BDE = @EmpCode
                                       )
                                       AND (
                                           @SearchIP = '' OR @SearchIP IS NULL 
					                            OR ccode LIKE '%' + @SearchIP + '%'
					                            OR cname LIKE '%' + @SearchIP + '%'
					                            OR email LIKE '%' + @SearchIP + '%'
					                            OR mobile LIKE '%' + @SearchIP + '%'
					                            OR gstno LIKE '%' + @SearchIP + '%'
                                       )
                                       AND (
                                          @Status = '' OR @Status IS NULL 
					                            OR type = @Status 
                                        )
                            ),NUMBERED AS(
                                       SELECT *,ROW_NUMBER() OVER (ORDER BY ccode ASC) AS Rn
                                       FROM Employee 
                                    )
                            SELECT Top 1500 * FROM NUMBERED 
                            ORDER BY id DESC";
                var parameters = new DynamicParameters();
                parameters.Add("@EmpCode", model.SessionName);
                parameters.Add("@SearchIP", model.CName);
                parameters.Add("@Status", model.typess);
                var result = await connection.QueryAsync<Companymaster>(
                    query,
                    parameters);
                return result.ToList();
            }

          
        }

        public async Task<int> SubmitDetails(Companymaster Model, string Action)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();

                    parameters.Add("@ccode", Model.CCode);
                    parameters.Add("@Id", Model.Id);

                    parameters.Add("@CompanyName", Model.CName);
                    parameters.Add("@OwnerName", Model.OName);

                    parameters.Add("@Email", Model.Email);
                    parameters.Add("@Mobile", Model.Mobile);

                    parameters.Add("@GSTNo", Model.GSTNo);
                    parameters.Add("@AreaNAme", Model.Area);

                    parameters.Add("@website", Model.Website);

                    parameters.Add("@address", Model.Address);
                    parameters.Add("@shippaddress", Model.ShippingAddress);

                    parameters.Add("@Category", Model.Category);

                    parameters.Add("@State", Model.State);
                    parameters.Add("@RegisterType", Model.RegisterType);

                    parameters.Add("@CountryCode", Model.CountryCode);
                    parameters.Add("@CountryName", Model.CountryName);

                    parameters.Add("@BillLocation", Model.BillingLocation);
                    parameters.Add("@BillingPincode", Model.BillingPincode);
                    parameters.Add("@BillStateCode", Model.BillingStateCode);
                    parameters.Add("@type", Model.typess);

                    parameters.Add("@visitdate", Model.VisitDate);
                    parameters.Add("@ShippLocation", Model.ShippingLocation);
                    parameters.Add("@ShippingPincode", Model.ShippingPincode);
                    parameters.Add("@ShippStateCode", Model.ShippingStateCode);
                    parameters.Add("@LeadCode", Model.LeadCode);
                    parameters.Add("@CreatedBy", Model.CreatedBy);
                    parameters.Add("@BDE", Model.BDE);
                    parameters.Add("@Action", Action);

                    parameters.Add("@Result",
                        dbType: DbType.Int32,
                        direction: ParameterDirection.Output);


                


                    await connection.ExecuteAsync(
                        "SP_companymasterAWS",
                        parameters,
                        commandType: CommandType.StoredProcedure);


                    return parameters.Get<int>("@Result");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Employee>> GetBDE(string Action)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Action", Action);

                var result = await connection.QueryAsync<Employee>(
                    "SP_companymasterAWS",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<List<dynamic>> GetHirechyEmployees(string code)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                string query = @"WITH employee_hierarchy AS
                        (
                            SELECT
                                e.empcode,
                                e.name,
                                e.TL_Manager,
                                0 AS level,
                                CAST('|' + e.empcode + '|' AS VARCHAR(MAX)) AS path
                            FROM employees e
                            WHERE e.status = 1
                              AND e.isdeleted = 0
                              AND e.empcode = @Empcode

                            UNION ALL

                            SELECT
                                e.empcode,
                                e.name,
                                e.TL_Manager,
                                eh.level + 1,
                                CAST(eh.path + e.empcode + '|' AS VARCHAR(MAX))
                            FROM employees e
                            INNER JOIN employee_hierarchy eh
                                ON e.TL_Manager = eh.empcode
                            WHERE e.status = 1
                              AND e.isdeleted = 0

                              AND eh.path NOT LIKE '%|' + e.empcode + '|%'
                        )

                        SELECT
                            empcode as UserCode,
                            name as FullName,
                            TL_Manager,
                            level,
                            path
                        FROM employee_hierarchy
                        ORDER BY level, empcode
                        OPTION (MAXRECURSION 1000);";
                var parameters = new DynamicParameters();
                parameters.Add("@Empcode", code);
                var result = await connection.QueryAsync<dynamic>(
                    query,
                    parameters);
                return result.ToList();
            }
        }


        public async Task<dynamic> GetCommentHistoryById(int Id)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT A.id,A.[ccode],A.[cname],A.[oname],A.[email],A.[mobile],A.[visitingcard],A.[type]
                        ,A.[BDE],A.[address],format(A.[visitdate],'dd-MMM-yyyy') as visitdate,A.[website],format(A.[regdate]
                        ,'dd-MMM-yyyy hh:mm tt') as [regdate],A.[sessionname],B.name,B.email as Empemail 
                        FROM [Company] A 
                        LEFT JOIN employees B on A.sessionname=B.empcode where A.id= @CommentId";

                    var parameters = new DynamicParameters();
                    parameters.Add("@CommentId", Id);
                    var companydetails = await connection.QueryAsync(query, parameters);

                    return companydetails.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<dynamic>> GetCommentHistoryList(string Ccode)
        {
            try
            {
                string sql = @"SELECT [commentdatetime],[typeoftbl],[message],[name] 
                            FROM (SELECT format(A.[commentdatetime],'dd-MMM-yyyy hh:mm tt') as [commentdatetime],
                            A.[typeoftbl],A.[message],B.[name] FROM [CommentHistory] A 
                            left join [employees] B ON A.[sessionname]=B.[empcode] 
                            where A.ccode= @Ccode 

                            UNION 

                            SELECT format(A.[updateddatetime],'dd-MMM-yyyy hh:mm tt') as [commentdatetime]
                            ,A.[typeoftbl],A.[message],B.[name] FROM [CompanyHistory] A 
                            left join [employees] B ON A.[sessionname]=B.[empcode]
                            where A.ccode= @Ccode 

                            UNION 

                            SELECT format(A.[setdatetime],'dd-MMM-yyyy hh:mm tt') as [commentdatetime],
                            A.[typeoftbl],A.[remark] as message,B.[name] FROM [RemainderData] A 
                            left join [employees] B ON A.[sessionname]=B.[empcode]
                            where A.ccode= @Ccode) AS T order by convert(datetime, [commentdatetime]) desc";

                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@Ccode", Ccode);
                    var result = await connection.QueryAsync<dynamic>(sql, parameters);
                    return result.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<dynamic> GetActiveEmployeeList()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT empcode as UserCode, name as FullName FROM [dbo].[employees] 
                                     WHERE status = '1' AND isdeleted = '0'";
                    var companydetails = await connection.QueryAsync(query);

                    return companydetails.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateCompanyCreatedByName(string newName, string CompCode, string SessionName)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                bool flag = await SetCompanyHistory(newName, CompCode, SessionName);
                const string sql = @"UPDATE Company SET sessionname = @newName, BDE=@newName WHERE id = @CompCode";

                var parameters = new DynamicParameters();
                parameters.Add("@newName", newName);
                parameters.Add("@CompCode", CompCode);

                int rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SetCompanyHistory(string newName, string Id, string CurrentSessionName)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
            await connection.OpenAsync();

            const string sql = @"SELECT c.ccode, c.sessionname, e1.name AS oldSessionName, c.BDE, e2.name AS oldBDEName
                         FROM Company c
                         LEFT JOIN employees e1 ON c.sessionname = e1.empcode
                         LEFT JOIN employees e2 ON c.BDE = e2.empcode
                         WHERE c.id = @Id;

                         SELECT name AS NewSalesPName
                         FROM employees
                         WHERE empcode = @empcode;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", Id);
            parameters.Add("@empcode", newName);

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var companyDetails = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var employeeDetails = await multi.ReadFirstOrDefaultAsync<dynamic>();

            if (companyDetails == null || employeeDetails == null)
                return false;

            string ccode = companyDetails.ccode?.ToString() ?? "";
            string oldSessionName = companyDetails.oldSessionName?.ToString() ?? "";
            string oldBDEName = companyDetails.oldBDEName?.ToString() ?? "";
            string newSalesPersonName = employeeDetails.NewSalesPName?.ToString() ?? "";

            string updateHistoryMsg = $"Sales Person and BDE/TME Person has been changed from {oldSessionName} / {oldBDEName} to {newSalesPersonName}";

            var historyParameters = new DynamicParameters();
            historyParameters.Add("@Action", "Insert");
            historyParameters.Add("@sessionname", CurrentSessionName);
            historyParameters.Add("@ccode", ccode);
            historyParameters.Add("@message", updateHistoryMsg);

            await connection.ExecuteAsync("SP_CompanyHistory", historyParameters, commandType: CommandType.StoredProcedure);

            return true;
        }

        public async Task<int> UpdateOldCommentHistory(int id)
        {
            try
            {
                using var connection = new SqlConnection(
                   _configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                const string sql = @"UPDATE [dbo].[CommentHistory] SET UpdateStatus=@UpdateStatus WHERE id = @ID";

                var parameters = new DynamicParameters();
                parameters.Add("@UpdateStatus", "Closed");
                parameters.Add("@ID", id);

                int rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> FromListSubmitCommentHistory(CallandMeeting model)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                var commentParameters = new DynamicParameters();

                commentParameters.Add("@sessionname", model.CreatedBy);
                commentParameters.Add("@ccode", model.Ccode);
                commentParameters.Add("@commentdatetime", DateTime.Now);
                commentParameters.Add("@message", string.IsNullOrEmpty(model.FeedBack) ? model.FeedBack : model.FeedBack.Replace("\n", "<br />"));
                commentParameters.Add("@UpdateStatus", model.CallUpdateStatus);
                commentParameters.Add("@Typeofclient", model.TypeofClient);
                commentParameters.Add("@Dealdetails", model.DealDetails);
                commentParameters.Add("@followupdate", model.FollowDate);
                commentParameters.Add("@meetingwithmanager", model.MeetingwithManager);
                commentParameters.Add("@meetingtime", model.MeetingTime);
                commentParameters.Add("@clientmail", model.ClientMail);
                commentParameters.Add("@frommail", model.FromMail);
                commentParameters.Add("@adminmail", _configuration["MailSettings:AdminMail"]);
                commentParameters.Add("@Updatefor", model.UpdateFor);
                commentParameters.Add("@Type", model.Type);

                const string commentSql = @"
                    INSERT INTO [CommentHistory]
                    (
                        sessionname, ccode, commentdatetime, message, UpdateStatus, Typeofclient,
                        Dealdetails,followupdate,meetingwithmanager, meetingtime, clientmail, frommail, adminmail, Updatefor, Type
                    )
                    VALUES
                    (
                        @sessionname, @ccode, @commentdatetime, @message, @UpdateStatus, @Typeofclient, @Dealdetails,
                        @followupdate, @meetingwithmanager,@meetingtime, @clientmail, @frommail,  @adminmail, @Updatefor, @Type
                    );";
                await connection.ExecuteAsync(commentSql, commentParameters);

                return 1;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}


