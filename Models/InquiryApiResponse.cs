using System.Text.Json.Serialization;

namespace WEBLINK_CRM.Models
{
    public class InquiryApiResponse
    {

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("data")]
        public List<InquiryApiItem>? Data { get; set; }
    }

    public class InquiryApiItem
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("i_name")]
        public string? Name { get; set; }

        [JsonPropertyName("mobile_no")]
        public long? MobileNo { get; set; }

        [JsonPropertyName("mail")]
        public string? Email { get; set; }

        [JsonPropertyName("inq")]
        public string? Inquiry { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("ip_address")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("page_url")]
        public string? PageUrl { get; set; }
    }


}
