using Dapper;
using Microsoft.Data.SqlClient;
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
            

                    GSTNo = data.gstno,

                    Address = data.address,
                    ShippingAddress = data.shippingaddress,

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

                    IsDeleted = data.isdeleted,

                    RegDate = data.regdate,
                    UpdatedDate = data.updateddate,
                    UpdatedBy = data.updatedby
                };

                return company;
            }
            catch (Exception ex)
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

        public async Task<List<dynamic>> GetcompanyNameDashboard(string Name, string UserName, string role)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyName", Name);
                parameters.Add("@CreatedBy", UserName);
                parameters.Add("@OwnerName", role);
                parameters.Add("@Action", "GetComapnyNameDashboard");
                var result = await connection.QueryAsync<dynamic>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        public async Task<dynamic> GetcompanybyIdDashboard(string Id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@id", Id);
                parameters.Add("@Action", "Getcomapnybyid"); // Corrected spelling

                var data = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (data == null)
                    return new Company();

                var company = new Company
                {
                    Id = data.Id?.ToString(),
                    CompanyName = data.CompanyName,
                    CompanyCode = data.CompanyCode,
                    Registerfor = data.Registerfor,
                    supplytype = data.supplytype,
                    OwnerName = data.OwnerName,
                    GSTNo = data.GSTNo,
                    BillAddress = data.BillAddress,
                    ShippAddress = data.ShippAddress,
                    BillLocation = data.BillLocation,
                    ShippLocation = data.ShippLocation,
                    BillingPincode = data.BillingPincode,
                    ShippingPincode = data.ShippingPincode,
                    BillStateCode = data.BillStateCode,
                    ShippStateCode = data.ShippStateCode,
                    BillingAddress = data.BillingAddress,
                    ShippingAddress = data.ShippingAddress,
                    IsDeleted = data.IsDeleted,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedOn = data.UpdatedOn,
                    DeletedOn = data.DeletedOn,
                    DeletedBy = data.DeletedBy,
                    AreaNAme = data.AreaNAme, // spelling in DB?
                    vendorCode = data.vendorCode
                };

                void AddContact(string? name, string? mobile, string? email, string? designation)
                {
                    if (!string.IsNullOrWhiteSpace(name) ||
                        !string.IsNullOrWhiteSpace(mobile) ||
                        !string.IsNullOrWhiteSpace(email) ||
                        !string.IsNullOrWhiteSpace(designation))
                    {
                        company.Contacts.Add(new Company.ContactModel
                        {
                            Name = name,
                            MobileNo = mobile,
                            EmailID = email,
                            Designation = designation
                        });
                    }
                }

                AddContact(data.Name1, data.MobileNo1, data.EmailID1, data.Designation1);
                AddContact(data.Name2, data.MobileNo2, data.EmailID2, data.Designation2);
                AddContact(data.Name3, data.MobileNo3, data.EmailID3, data.Designation3);
                AddContact(data.Name4, data.MobileNo4, data.EmailID4, data.Designation4);
                AddContact(data.Name5, data.MobileNo5, data.EmailID5, data.Designation5);

                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<dynamic> GetcompanybyCCodeFromMainSearch(string CCode)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyCode", CCode);
                parameters.Add("@Action", "GetcomapnybyCCode"); // Corrected spelling

                var data = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_companymaster",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (data == null)
                    return new Company();

                var company = new Company
                {
                    Id = data.Id?.ToString(),
                    CompanyName = data.CompanyName,
                    CompanyCode = data.CompanyCode,
                    Registerfor = data.Registerfor,
                    supplytype = data.supplytype,
                    OwnerName = data.OwnerName,
                    GSTNo = data.GSTNo,
                    BillAddress = data.BillAddress,
                    ShippAddress = data.ShippAddress,
                    BillLocation = data.BillLocation,
                    ShippLocation = data.ShippLocation,
                    BillingPincode = data.BillingPincode,
                    ShippingPincode = data.ShippingPincode,
                    BillStateCode = data.BillStateCode,
                    ShippStateCode = data.ShippStateCode,
                    BillingAddress = data.BillingAddress,
                    ShippingAddress = data.ShippingAddress,
                    IsDeleted = data.IsDeleted,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedOn = data.UpdatedOn,
                    DeletedOn = data.DeletedOn,
                    DeletedBy = data.DeletedBy,
                    AreaNAme = data.AreaNAme, // spelling in DB?
                    vendorCode = data.vendorCode
                };

                void AddContact(string? name, string? mobile, string? email, string? designation)
                {
                    if (!string.IsNullOrWhiteSpace(name) ||
                        !string.IsNullOrWhiteSpace(mobile) ||
                        !string.IsNullOrWhiteSpace(email) ||
                        !string.IsNullOrWhiteSpace(designation))
                    {
                        company.Contacts.Add(new Company.ContactModel
                        {
                            Name = name,
                            MobileNo = mobile,
                            EmailID = email,
                            Designation = designation
                        });
                    }
                }

                AddContact(data.Name1, data.MobileNo1, data.EmailID1, data.Designation1);
                AddContact(data.Name2, data.MobileNo2, data.EmailID2, data.Designation2);
                AddContact(data.Name3, data.MobileNo3, data.EmailID3, data.Designation3);
                AddContact(data.Name4, data.MobileNo4, data.EmailID4, data.Designation4);
                AddContact(data.Name5, data.MobileNo5, data.EmailID5, data.Designation5);

                return company;
            }
            catch (Exception)
            {
                throw;
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

                    parameters.Add("@ShippLocation", Model.ShippingLocation);
                    parameters.Add("@ShippingPincode", Model.ShippingPincode);
                    parameters.Add("@ShippStateCode", Model.ShippingStateCode);

                    parameters.Add("@CreatedBy", Model.CreatedBy);

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
    }
}


