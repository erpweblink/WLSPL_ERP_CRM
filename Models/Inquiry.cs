using System.Text.Json.Serialization;
using WEBLINK_CRM.Models;


namespace WEBLINK_CRM.Models
{
    public class Inquiry
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("mobile_number")]
        public string? MobileNumber { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonIgnore]
        public string? Location
        {
            get => City;
            set => City = value;
        }

        [JsonPropertyName("service_requested")]
        public string? ServiceRequested { get; set; }

        [JsonPropertyName("source_url")]
        public string? SourceUrl { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        public string? SavedAt { get; set; }
        public string? Date { get; set; }
        public string? InquiryText { get; set; }
        public string? PageUrl { get; set; }
        public string? SalesEmpCode { get; set; }
        public string? AssignTo { get; set; }

        public string? SalesPerson { get; set; }
        public string? Leadcode { get; set; }

        public string? Status { get; set; }
        [JsonIgnore]
        public List<Employee> SalesPersons { get; set; } = new();
    }
}