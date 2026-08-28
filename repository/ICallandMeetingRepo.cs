using WEBLINK_CRM.Models;

namespace WEBLINK_CRM.repository
{
    public interface ICallandMeetingRepo
    {
        Task<int> SubmitDetails(CallandMeeting Model);
        Task<int> FromListSubmitCommentHistory(CallandMeeting Model);

        Task<List<CallandMeeting>> List(string SessionName);

        Task<dynamic> GetSalesPersonList(string empCode, string empRole);
        Task<List<dynamic>> GetcompanyName(string Name,string actionby);

        Task<List<CallandMeeting>> GetFilteredReport(FollowUpFilterModel filter);
        Task<bool> UpdateRemarks(string id, string remark);

        Task<dynamic> GetcompanybyId(string Id);
        Task<dynamic>GetCommentHistoryById(int Id);
        Task<List<dynamic>> GetCommentHistoryList(string Ccode);
        Task<dynamic> GetHirechyWiseUser();
        Task<int> UpdateCompanyCreatedByName(string newName, string CompCode, string SessionName);
        Task<int> UpdateOldCommentHistory(int id);
        Task<List<dynamic>> GetNotUpdatedList(string days);

    }
}
