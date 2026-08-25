using Dapper;
using Microsoft.Data.SqlClient;
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

        public async Task<int> Delete(string ID, string CreatedBy)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_String")))
            {
                await connection.OpenAsync();

                var itemParameters = new DynamicParameters();
                itemParameters.Add("@id", ID);
                itemParameters.Add("@Action", "Deleteservices");
                itemParameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("SP_Quotation", itemParameters, commandType: CommandType.StoredProcedure);

                int result = itemParameters.Get<int>("@Result");
                return result;
            }
        }


        public async Task<List<Services>> GetDepartment(string action)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_String")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Action", action);

                var result = await connection.QueryAsync<Services>(
                    "SP_Quotation",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }
        public async Task<int?> Submitservices(Services Model, string Action)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_String")))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@id", Model.ID);
                parameters.Add("@Department", Model.DepartmentName);
                parameters.Add("@ServicesDesc", Model.ServicesDesc);
                parameters.Add("@Price", Model.Price);
                parameters.Add("@Currency", Model.Currency);
                parameters.Add("@Action", Action);
                parameters.Add("@TypeofIndustry", Model.TypeofIndustry);
                parameters.Add("@pagesize", Model.pagesize);
                parameters.Add("@Typeofwebsite", Model.Typeofwebsite);
                parameters.Add("@Years", Model.Years);
                parameters.Add("@City", Model.City);
                parameters.Add("@Keywords", Model.Keywords);
                parameters.Add("@CreatedBy", Model.CreatedBy);
                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync("SP_Quotation", parameters, commandType: CommandType.StoredProcedure);
                int isSuccess = parameters.Get<int>("@Result");
                return isSuccess;
            }
        }

        Task<List<Services>> IServicesRepo.GetDepartment(string Action)
        {
            throw new NotImplementedException();
        }
    }
}
