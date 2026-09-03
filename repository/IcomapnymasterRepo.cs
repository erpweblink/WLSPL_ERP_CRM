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

        Task<List<Companymaster>> GetFilteredcompanyList(Companymaster model);
        //Task<dynamic> GetcompanybyId(string Id);
        Task<Companymaster> GetcompanybyId(string Id);
        Task<List<dynamic>> GetcompanyName(string Name);

        Task<int> DeleteReord(string ID, String CreatedBy);

        Task<List<Company>> SearchCompanyAsync(string Action);

        Task<List<Employee>> GetBDE(string Action);

        Task<List<dynamic>> GetHirechyEmployees(string code);

        Task<dynamic> GetCommentHistoryById(int Id);
        Task<List<dynamic>> GetCommentHistoryList(string Ccode);
        Task<dynamic> GetActiveEmployeeList();
        Task<int> UpdateCompanyCreatedByName(string newName, string CompCode, string SessionName);
        Task<int> FromListSubmitCommentHistory(CallandMeeting Model);
        Task<int> UpdateOldCommentHistory(int id);

    }
}
