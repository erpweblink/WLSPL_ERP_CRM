using Newtonsoft.Json.Linq;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Controllers.CompanymasterController;

namespace WEBLINK_CRM.repository
{
    public interface IcomapnymasterRepo
    {
        Task<string> Getcompcode(string Action);

        Task<int> SubmitDetails(Companymaster Model, string Action);

        Task<List<Company>> checkcomapnies(string action, string company, string GstNo);

        Task<List<Company>> GetLeadlist(string Action, Company Model);

        Task<List<Companymaster>> GetcompanyList(Companymaster Model, string Action);  
        //Task<dynamic> GetcompanybyId(string Id);
        Task<Companymaster> GetcompanybyId(string Id);
        Task<List<dynamic>> GetcompanyName(string Name);

        Task<List<dynamic>> GetcompanyNameDashboard(string Name,string userName, string role);

        Task<dynamic> GetcompanybyIdDashboard(string Id);
        Task<dynamic> GetcompanybyCCodeFromMainSearch(string CCode);

        Task<int> DeleteReord(string ID, String CreatedBy);

        Task<List<Company>> SearchCompanyAsync(string Action);

        Task<List<Employee>> GetBDE(string Action);


    }
}
