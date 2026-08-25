using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public class GovServicesRepo : IGoveServices
    {
        private readonly IConfiguration _configuration;
        private readonly GovKeySettings _govKeySettings;
        public GovServicesRepo(IConfiguration configuration,GovKeySettings govKeySettings)
        {
            _configuration = configuration;
            _govKeySettings = govKeySettings;
        }

        public async Task<AuthKeyResponse> GetOrGenerateAuthKeyAsync()
        {
            var result = new AuthKeyResponse();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var queryParams = new DynamicParameters();
                queryParams.Add("@UserName", _govKeySettings.UserName);
                queryParams.Add("@Action", "GetToken");

                var existingTokens = await connection.QueryAsync<AuthKeyResponse>(
                    "SP_AUTH_TOKEN_MASTER",
                    queryParams,
                    commandType: CommandType.StoredProcedure
                );
                if (existingTokens != null)
                {
                    var token = existingTokens.FirstOrDefault();

                    if (token != null
                        && !string.IsNullOrWhiteSpace(token.TokenExpiry)
                        && DateTime.TryParse(token.TokenExpiry, out var expiry)
                        && expiry > DateTime.Now)
                    {
                        return token;
                    }
                }




                result = await GenerateAuthKeyAsync(_govKeySettings.UserName, _govKeySettings.Password, _govKeySettings.GSTIN);

                if (!string.IsNullOrWhiteSpace(result.AuthKey))
                {
                    var saveParams = new DynamicParameters();
                    saveParams.Add("@UserName", _govKeySettings.UserName);
                    saveParams.Add("@AuthToken", result.AuthKey);
                    saveParams.Add("@TokenExpiry", DateTime.TryParse(result.TokenExpiry, out var dt) ? dt : DateTime.Now.AddMinutes(30));
                    saveParams.Add("@Sek", result.Sek ?? "");
                    saveParams.Add("@ClientId", result.ClientId ?? "");  // ✅ Added ClientId
                    saveParams.Add("@Action", "UpsertToken");

                    await connection.ExecuteAsync(
                        "SP_AUTH_TOKEN_MASTER",
                        saveParams,
                        commandType: CommandType.StoredProcedure
                    );
                }

            }

            return result;
        }
        public async Task<AuthKeyResponse> GenerateAuthKeyAsync(string userName, string password, string sellerGstNo)
        {
            var result = new AuthKeyResponse();

            try
            {
                string url = "https://api.mastergst.com/einvoice/authenticate?email=erp%40weblinkservices.net";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("username", userName);
                client.DefaultRequestHeaders.Add("password", password);
                client.DefaultRequestHeaders.Add("ip_address", _govKeySettings.IP_Address);
                client.DefaultRequestHeaders.Add("client_id", _govKeySettings.E_Invoice_Client_Id);
                client.DefaultRequestHeaders.Add("client_secret", _govKeySettings.E_Invoice_Client_Secret);
                client.DefaultRequestHeaders.Add("gstin", sellerGstNo);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                JObject jo = JObject.Parse(json);
                result.Message = jo["status_desc"]?.ToString();

                // ✅ Correct check for success
                if (jo["status_cd"]?.ToString().Equals("Sucess", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var data = jo["data"];
                    if (data != null)
                    {
                        result.AuthKey = data["AuthToken"]?.ToString();
                        result.TokenExpiry = data["TokenExpiry"]?.ToString();
                        result.Sek = data["Sek"]?.ToString();
                        result.ClientId = data["ClientId"]?.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<JObject> GetGSTDetailsAsync(string gstNo)
        {
			try
			{
                var auth = await GetOrGenerateAuthKeyAsync();

                if (string.IsNullOrWhiteSpace(auth.AuthKey))
                    throw new Exception("Auth token generation failed: " + auth.Message);

                string url = $"https://api.mastergst.com/einvoice/type/GSTNDETAILS/version/V1_03?param1={gstNo}&email=erp%40weblinkservices.net";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("ip_address", "103.174.254.209");
                client.DefaultRequestHeaders.Add("client_id", _govKeySettings.E_Invoice_Client_Id);
                client.DefaultRequestHeaders.Add("client_secret", _govKeySettings.E_Invoice_Client_Secret);
                client.DefaultRequestHeaders.Add("username", _govKeySettings.UserName);
                client.DefaultRequestHeaders.Add("auth-token", auth.AuthKey);
                client.DefaultRequestHeaders.Add("gstin", _govKeySettings.GSTIN);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json);
            }
			catch (Exception)
			{

				throw;
			}
        }
    }
}
