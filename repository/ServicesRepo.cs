using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WEBLINK_CRM.Models;


namespace WEBLINK_CRM.repository
{
    public class ServicesRepo : IServicesRepo
    {
        private readonly IConfiguration _configuration;
        public ServicesRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> DeleteServices(string ID, string UpdatedBy)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@ID", ID);
                parameters.Add("@UpdatedBy", UpdatedBy);
                parameters.Add("@Action", "DeleteRecords");

                const string companySql = @"UPDATE [WLSPLCRM].[Tbl_servicemaster] SET IsActive = 0 , UpdatedBy= @UpdatedBy, UpdatedOn = GETDATE() ;";

                var result = await connection.QuerySingleAsync<int>(companySql,parameters);

                return result;
            }
        }

        public async Task<List<Department>> Getdepartments(Department model, string Action)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", Action);

                var result = await connection.QueryAsync<Department>(
                    "SP_Department",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<Services> GetServicesById(string ID)
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));

                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@ID", ID);
                parameters.Add("@Action", "GetByID");


                const string companySql = @"SELECT * FROM [WLSPLCRM].[Tbl_servicemaster] WHERE IsActive = 1 and ID = @ID;";

                var result = await connection.QueryFirstOrDefaultAsync<Services>(companySql,parameters);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SubmitServices(Services Model, string Action)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                   
                    parameters.Add("@ServiceName", Model.ServiceName.ToString().Trim());
                    parameters.Add("@ServiceCode", Model.ServiceCode.ToString().Trim());
                    parameters.Add("@Price", Model.Price);
                    parameters.Add("@IsActive", "1");
                    parameters.Add("@CreatedBy", Model.CreatedBy);

                    string companySql = string.Empty;

                    if (Action == "Insert")
                    {
                        companySql = @"
                        IF EXISTS (
                            SELECT 1 FROM [WLSPLCRM].[Tbl_servicemaster]
                            WHERE ServiceName = @ServiceName AND ServiceCode = @ServiceCode
                        )
                        BEGIN
                            SELECT -1 AS Result;
                            RETURN;
                        END

                        INSERT INTO [WLSPLCRM].[Tbl_servicemaster]
                        (
                            ServiceName, ServiceCode, Price, IsActive, CreatedBy, CreatedOn
                        )
                        VALUES
                        (
                            @ServiceName, @ServiceCode, @Price, @IsActive, @CreatedBy, GETDATE()
                        );

                        SELECT 1 AS Result;";
                    }
                    else
                    {
                        parameters.Add("@ID", Model.ID);
                        companySql = @"
                        UPDATE [WLSPLCRM].[Tbl_servicemaster] SET
                            ServiceName = @ServiceName,
                            ServiceCode = @ServiceCode,
                            Price       = @Price,
                            UpdatedBy   = @CreatedBy,
                            UpdatedOn   = GETDATE()
                        WHERE ID = @ID;

                        SELECT 1 AS Result;";
                    }

                    var result = await connection.QueryFirstOrDefaultAsync<int>(companySql, parameters);

                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
          
        }

        public async Task<List<Services>> GetServices(Services Model, string Action)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                const string companySql = @"SELECT * FROM [WLSPLCRM].[Tbl_servicemaster] WHERE IsActive = 1 ORDER BY ID DESC;";

                var result = await connection.QueryAsync<Services>(companySql);

                return result.ToList();
            }
        }
    }
}
