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

                var result = await connection.QuerySingleAsync<int>(
                    "SP_Services",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }
        public async Task<List<Department>> Getdepartments(
            Department model,
            string Action)
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

                var data = await connection.QueryFirstOrDefaultAsync<Services>(
                    "SP_Services",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SubmitServices(Services Model, string Action)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", Action);
                parameters.Add("@ID", Model.ID);
                parameters.Add("@ServiceName", Model.ServiceName);
                parameters.Add("@ServiceCode", Model.ServiceCode);
                parameters.Add("@ServicesDesc", Model.ServicesDesc);
                parameters.Add("@Price", Model.Price);
                parameters.Add("@Currency", Model.Currency);
                parameters.Add("@Years", Model.Years);
                parameters.Add("@City", Model.City);
                parameters.Add("@IsActive", Model.IsActive);

                parameters.Add("@DepartmentName", Model.DepartmentName);
                parameters.Add("@CreatedBy", Model.CreatedBy);
                parameters.Add("@UpdatedBy", Model.UpdatedBy);

                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_Services",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }


        public async Task<List<Services>> GetServices(Services Model, string Action)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", Action);

                var result = await connection.QueryAsync<Services>(
                    "SP_Services",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }
    }
}
