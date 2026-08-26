using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class CallandMeetingRepo : ICallandMeetingRepo
    {
        private readonly IConfiguration _configuration;
        private readonly IMailingRepo _mailingRepo;
        private readonly IWebHostEnvironment _env;

        public CallandMeetingRepo(IConfiguration configuration, IMailingRepo mailingRepo, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _mailingRepo = mailingRepo;
            _env = env;
        }

        //Call and Meeting Creation functions
        public async Task<List<dynamic>> GetcompanyName(string Name, string actionby)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                //string query = "SELECT ccode,cname,name,UserName FROM Company cm LEFT JOIN employees em ON cm.sessionname = em.empcode WHERE (@actionBy = 'ddlList' AND cname LIKE '%' + @companyName + '%') OR (@actionBy <> 'ddlList' AND cname = @companyName)";
                string query = @"WITH Employee AS(
                                            SELECT ccode,cname,name,UserName 
                                            FROM Company cm 
                                            LEFT JOIN employees em 
                                            ON cm.sessionname = em.empcode 
                                            WHERE (@actionBy = 'ddlList' AND cname LIKE '%' + @companyName + '%') 
                                            OR (@actionBy <> 'ddlList' AND cname = @companyName)
                                ),NUMBERED AS(
                                   SELECT *,ROW_NUMBER() OVER (ORDER BY ccode ASC) AS Rn
                                   FROM Employee 
                                )
                                SELECT * FROM NUMBERED
                                WHERE @ShowRecords =0 OR Rn <= @ShowRecords";
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyName", Name);
                parameters.Add("@actionby", actionby);
                parameters.Add("@ShowRecords", _configuration["DatabaseRecords:ShowRecords"]);
                var result = await connection.QueryAsync<dynamic>(
                    query,
                    parameters);
                return result.ToList();
            }
        }

        public async Task<dynamic> GetcompanybyId(string Id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@id", Id);

                var data = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM [Company] WHERE ccode=@id",
                    parameters);

                if (data == null)
                    return null;

                var company = new
                {
                    Id = Convert.ToInt32(data.id),

                    CCode = data.ccode,
                    CName = data.cname,
                    OName = data.oname,
                    Email = data.email,
                    Mobile = data.mobile,
                    Address = data.address,
                    Area = data.area,
                };

                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SubmitDetails(CallandMeeting model)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();
                // For Table stswlspl.tbl_NewCompanyAdd commnet by Nikhil 
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@companyname", model.CompanyName);
                    parameters.Add("@personname", model.PersonName);
                    parameters.Add("@contactno", model.ContactNo);
                    parameters.Add("@address", model.Address);

                    parameters.Add(
                        "@feedback",
                        string.IsNullOrEmpty(model.FeedBack)
                            ? model.FeedBack
                            : model.FeedBack.Replace("\n", "<br />"));

                    parameters.Add("@updatefor", model.UpdateFor);
                    parameters.Add("@area", model.Area);
                    parameters.Add("@followdate", model.FollowDate);
                    parameters.Add("@Meetingwithmanager", model.MeetingwithManager);
                    parameters.Add("@CreatedBy", model.CreatedBy);
                    parameters.Add("@CreatedOn", DateTime.Now);
                    parameters.Add("@IsDeleted", "0");
                    parameters.Add("@Action", "Save");

                    await connection.ExecuteAsync(
                        "[stswlspl].SP_AddCompanyNew",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                }

                var companyCode = model.Ccode;

                // For new company commnet by Nikhil 
                if (string.IsNullOrWhiteSpace(companyCode))
                {
                    // Get company code
                    companyCode = await connection.ExecuteScalarAsync<string>("SELECT dbo.GetCompanyCode()");

                    if (string.IsNullOrWhiteSpace(companyCode))
                    {
                        throw new Exception("Unable to generate company code.");
                    }


                    var companyParameters = new DynamicParameters();

                    companyParameters.Add("@ccode", companyCode);
                    companyParameters.Add("@cname", model.CompanyName);
                    companyParameters.Add("@oname", model.PersonName);
                    companyParameters.Add("@mobile", model.ContactNo);
                    companyParameters.Add("@area", model.Area);
                    companyParameters.Add("@address", model.Address);
                    companyParameters.Add("@Meetingwithmanager", model.MeetingwithManager);
                    companyParameters.Add("@email", model.ClientMail);
                    companyParameters.Add("@status", "1");
                    companyParameters.Add("@type", "Unpaid");
                    companyParameters.Add("@BDE", model.CreatedBy);
                    companyParameters.Add("@regdate", DateTime.Now);
                    companyParameters.Add("@sessionname", model.CreatedBy);
                    companyParameters.Add("@Category", model.RegistrationCategory);
                    companyParameters.Add("@RegisterType", model.RegistrationFor);
                    companyParameters.Add("@gstno", model.GSTNo);

                    const string companySql = @"
                            INSERT INTO [Company]
                            (
                                ccode,cname,oname,mobile,area, address,
                                Meetingwithmanager,email, status,type, BDE,regdate, sessionname,Category,RegisterType, gstno
                            )
                            VALUES
                            (
                                @ccode,@cname,@oname, @mobile,@area, @address,@Meetingwithmanager,
                                @email, @status, @type, @BDE, @regdate, @sessionname, @Category, @RegisterType, @gstno
                            );";

                    await connection.ExecuteAsync(companySql, companyParameters);
                }

                // For comment history commnet by Nikhil 
                {
                    var commentParameters = new DynamicParameters();

                    commentParameters.Add("@sessionname", model.CreatedBy);
                    commentParameters.Add("@ccode", companyCode);
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
                }
                await connection.CloseAsync();

                // For mail send comment by Nikhil 
                {
                    //await MailStructure
                    //   (model?.UpdateFor ?? "NA", model?.FromMail ?? "NA", model?.CompanyName ?? "NA"
                    //   , model?.FeedBack ?? "NA", model?.ClientMail ?? "NA", model?.CreatedBy ?? "NA");
                }

                return 1;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task MailStructure(string action, string fromMailID, string companyName, string feedBack, string clientMail, string createdBy)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand("SELECT E1.empcode AS OwnId, E1.email AS OwnMail, E1.TL_Manager AS ItsTL, " +
                    "E2.empcode AS TLId, E2.email AS TLMail, E2.TL_Manager AS TLsTL, " +
                    "CASE WHEN E3.empcode = E2.empcode THEN NULL ELSE E3.empcode END AS MangId, " +
                    "CASE WHEN E3.empcode = E2.empcode THEN NULL ELSE E3.email END AS MangMail, " +
                    "CASE WHEN E3.TL_Manager = E2.TL_Manager THEN NULL ELSE E3.TL_Manager END AS MangTL, " +
                    "CASE WHEN E4.empcode IN (E3.empcode, E2.empcode) THEN NULL ELSE E4.empcode END AS AdminId, " +
                    "CASE WHEN E4.empcode IN (E3.empcode, E2.empcode) THEN NULL ELSE E4.email END AS AdminMail, " +
                    "CASE WHEN E4.TL_Manager IN (E3.TL_Manager, E2.TL_Manager) THEN NULL ELSE E4.TL_Manager END AS AdminTL " +
                    "FROM employees E1 " +
                    "JOIN employees E2 ON E2.EmpCode = E1.TL_Manager " +
                    "JOIN employees E3 ON E3.empcode = E2.TL_Manager " +
                    "JOIN employees E4 ON E4.empcode = E3.TL_Manager " +
                    "WHERE E1.email = @Email", connection))
                {
                    cmd.Parameters.AddWithValue("@Email", fromMailID);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                await connection.CloseAsync();

                MailRequest mailData = new MailRequest();
                mailData.From = fromMailID;

                if (action == "Call")
                {
                    mailData.Subject = companyName + " - CALL UPDATE - " + createdBy;
                    mailData.To = fromMailID;
                }
                else
                {
                    mailData.Subject = companyName + " - UPDATE - From Web Link Services Pvt Ltd.";
                    mailData.To = clientMail;
                }

                mailData.Cc = new List<string>();
                if (!string.IsNullOrEmpty(dt.Rows[0]["TLId"].ToString()))
                {
                    mailData.Cc.Add(dt.Rows[0]["TLMail"].ToString());
                }
                if (!string.IsNullOrEmpty(dt.Rows[0]["MangId"].ToString()))
                {
                    mailData.Cc.Add(dt.Rows[0]["MangMail"].ToString());
                }
                if (!string.IsNullOrEmpty(dt.Rows[0]["AdminId"].ToString()))
                {
                    mailData.Cc.Add(dt.Rows[0]["AdminMail"].ToString());
                }


                string filePath = Path.Combine(_env.WebRootPath, "EmailSentTemplete", "CommentPage_templet.html");

                StreamReader reader = new StreamReader(filePath);
                string readFile = reader.ReadToEnd();
                string myString = "";
                myString = readFile;

                string multilineText = feedBack;
                string formattedText = multilineText.Replace("\n", "<br />");

                myString = myString.Replace("$Comment$", formattedText);

                mailData.Body = myString.ToString();

                mailData.IsBodyHtml = true;

                await _mailingRepo.SendAsync(mailData);

            }
            catch (Exception)
            {
                throw;
            }

        }


        //Call and Meeting Report functions
        public async Task<List<CallandMeeting>> List(string SessionName)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await connection.OpenAsync();

            const string sql = @"
                SELECT DISTINCT TOP 100 ID_CommentHistory as Id,cname as CompanyName,message as FeedBack,
                commentdatetime as CommentDateTime,Updatefor as UpdateFor,followupdate as FollowDate, 
                Updatefor +'- '+ updateStatus  as CallUpdateStatus, AdminRemark as AdminRemark
                FROM stswlspl.[VW_FollowUpRpt]
                WHERE sessionname = @SessionName
                ORDER BY commentdatetime DESC";

            var result = await connection.QueryAsync<CallandMeeting>(sql, new { SessionName = SessionName });

            return result.ToList();
        }

        public async Task<dynamic> GetSalesPersonList(string empCode, string empRole)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT empcode as UserCode, name as FullName FROM [dbo].[employees] 
                                     WHERE status = '1' AND isdeleted = '0' 
                                     AND (@CurrentRole = 'Admin' OR empcode = @CurrentUser OR TL_Manager = @CurrentUser) 
                                     ORDER BY  CASE WHEN empcode = @CurrentUser THEN 0 ELSE 1 END, name;
         
                                     SELECT empcode as UserCode, name as FullName 
                                     FROM [dbo].[employees] 
                                     WHERE status = '1' AND isdeleted = '0' AND Sales_TL_Manager = '1'
                                     ORDER BY name;";

                    var parameters = new DynamicParameters();
                    parameters.Add("@CurrentUser", empCode);
                    parameters.Add("@CurrentRole", empRole);
                    using (var multi = await connection.QueryMultipleAsync(query, parameters))
                    {
                        var salesManagers = (await multi.ReadAsync<dynamic>()).ToList();
                        var meetingWithManagers = (await multi.ReadAsync<dynamic>()).ToList();

                        return new
                        {
                            SalesManagers = salesManagers,
                            MeetingWithManagers = meetingWithManagers
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CallandMeeting>> GetFilteredReport(FollowUpFilterModel filter)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();

                    string effectiveSalesManager = filter.SalesManager;
                    if (!string.Equals(filter.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrEmpty(effectiveSalesManager))
                    {
                        effectiveSalesManager = filter.EmpCode;
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@UpdateType", filter.UpdateFor);
                    parameters.Add("@UpdateStatus", filter.Status);
                    parameters.Add("@SalesManager", effectiveSalesManager);
                    parameters.Add("@MeetingWithManager", filter.MeetingWith);
                    parameters.Add("@FromDate", filter.FromDate);
                    parameters.Add("@ToDate", filter.ToDate);
                    parameters.Add("@FollowUpDate", filter.FollowUpDate);
                    parameters.Add("@Area", filter.Area);
                    parameters.Add("@CompanyName", filter.CompanyName);

                    var rawResult = await connection.QueryAsync<FollowUpReportRawDto>(
                        "[stswlspl].[SP_CallMeetingReport]", parameters, commandType: CommandType.StoredProcedure);

                    var mapped = rawResult.Select(r => new CallandMeeting
                    {
                        Id = r.ID_CommentHistory,
                        CompanyName = r.Cname,
                        PersonName = r.Oname,
                        ContactNo = r.Mobile,
                        Ccode = r.Ccode,
                        CommentDateTime = r.CommentDateTime,
                        ClientMail = r.ClientMail,
                        FeedBack = r.Message,
                        CreatedBy = r.SessionName,
                        Area = r.Area,
                        UpdateFor = r.UpdateFor,
                        CallUpdateStatus = r.UpdateStatus,
                        TypeofClient = r.TypeOfClient,
                        DealDetails = r.DealDetails,
                        FollowDate = r.FollowUpDate,
                        MeetingwithManager = r.MeetingWithManager,
                        AdminRemark = r.AdminRemark
                    }).ToList();

                    return mapped;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateRemarks(string id, string remark)
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                const string sql = @"UPDATE [dbo].[CommentHistory] SET AdminRemark = @Remark WHERE id = @ID";

                var parameters = new DynamicParameters();
                parameters.Add("@Remark", remark);
                parameters.Add("@ID", id);

                int rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<dynamic> GetCommentHistoryById(int Id)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT * FROM [stswlspl].[VW_FollowUpRpt] WHERE ID_CommentHistory = @CommentId";

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
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@Sp_Mode", "GetCommentHistoryList");
                    parameters.Add("@ActionBy", Ccode);
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CALLANDMEETING",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    return result.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<dynamic> GetHirechyWiseUser()
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

        public async Task<int> UpdateCompanyCreatedByName(string newName, string CompCode)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@ActionBy", newName);
                    parameters.Add("@Ccode", CompCode);
                    parameters.Add("@Sp_Mode", "UpdateCompanyCreatedByName");
                    parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    await connection.ExecuteAsync("SP_CALLANDMEETING", parameters, commandType: CommandType.StoredProcedure);
                    int isSuccess = parameters.Get<int>("@Result");
                    return isSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateOldCommentHistory(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@ActionBy", id);
                    parameters.Add("@Sp_Mode", "UpdateOldCommentHistory");
                    parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    await connection.ExecuteAsync("SP_CALLANDMEETING", parameters, commandType: CommandType.StoredProcedure);
                    int isSuccess = parameters.Get<int>("@Result");
                    return isSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> FromListSubmitCommentHistory(CallandMeeting Model)
        {
            return 1;
        }

        public async Task<List<dynamic>> GetNotUpdatedList(string days)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@Sp_Mode", "NotUpdateList");
                    parameters.Add("@Typeoftbl", days);
                    parameters.Add("@SessionName", "");
                    parameters.Add("@CompanyName", "");
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CALLANDMEETING",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    return result.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }    
    }
}
