namespace WEBLINK_CRM.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Icon { get; set; } = "fa-triangle-exclamation";
        public string Theme { get; set; } = "danger";   // danger | warning | info
        public string? RequestId { get; set; }
        public string? Path { get; set; }
        public string? Details { get; set; }
    }
}
