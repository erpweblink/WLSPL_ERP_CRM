namespace WEBLINK_CRM.Models
{
    public class CallandMeeting
    {
        public int Id { get; set; }
        public string? RegistrationCategory { get; set; }
        public string? RegistrationFor { get; set; }
        public string? CompanyName { get; set; }
        public string? PersonName { get; set; }
        public string? ContactNo { get; set; }
        public string? Address { get; set; }
        public string? FeedBack { get; set; }
        public string? UpdateFor { get; set; }
        public string? GSTNo { get; set; }
        public string? Area { get; set; }
        public DateTime? FollowDate { get; set; }
        public string? MeetingwithManager { get; set; }
        public bool? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }

        // CommentHistory Fields
        public string? SessionName { get; set; }
        public string? Ccode { get; set; }
        public string? FromMail { get; set; }
        public string? AdminMail { get; set; }
        public string? ClientMail { get; set; }
        public string? BdeMail { get; set; }
        public string? AdditionalMail { get; set; }
        public DateTime? CommentDateTime { get; set; }
        public string? Typeoftbl { get; set; }
        public string? ScreenShoots { get; set; }
        public string? CallUpdateStatus { get; set; }
        public string? MeetUpdateStatus { get; set; }
        public string? TypeofClient { get; set; }
        public string? DealDetails { get; set; }
        public TimeOnly? MeetingTime { get; set; }
        public string? MeetingTimeView { get; set; }
        public string? Type { get; set; }
        public string? AdminRemark { get; set; }
        public bool? ReminderMailStatus { get; set; }
    }

    public class FollowUpFilterModel
    {
        public string EmpCode { get; set; }
        public string Role { get; set; }
        public string UpdateFor { get; set; }
        public string Status { get; set; }
        public string SalesManager { get; set; }
        public string MeetingWith { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string Area { get; set; }
        public string CompanyName { get; set; }
    }

    public class FollowUpReportRawDto
    {
        public int Id { get; set; }
        public int ID_CommentHistory { get; set; }
        public string Cname { get; set; }
        public string Oname { get; set; }
        public string Mobile { get; set; }
        public string BDE { get; set; }
        public string Ccode { get; set; }
        public DateTime? CommentDateTime { get; set; }
        public string ClientMail { get; set; }
        public string Message { get; set; }
        public string SessionName { get; set; }
        public string Area { get; set; }
        public string UpdateFor { get; set; }
        public string UpdateStatus { get; set; }
        public string TypeOfClient { get; set; }
        public string DealDetails { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string MeetingWithManager { get; set; }

        public string AdminRemark { get; set; }
    }
}
