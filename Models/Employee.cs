namespace WEBLINK_CRM.Models
{
    public class Employee
    {
        public int id { get; set; }
        public string? empcode { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? emailpsw { get; set; }
        public string? panelpsw { get; set; }
        public string? mobile { get; set; }
        public string? role { get; set; }
        public string? status { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? regdate { get; set; }
        public string? TL_Manager { get; set; }
        public string? UserName { get; set; }
        public string? Sales_TL_Manager { get; set; }
        public string? Designation { get; set; }
    }
}