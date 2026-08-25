using Microsoft.AspNetCore.Http.HttpResults;

namespace WEBLINK_CRM.Models
{
    public class Services
    {
        public string ID { get; set; }

        public string DepartmentID { get; set; }

        public string ServiceName { get; set; }

        public string ServiceCode { get; set; }

        public string ServicesDesc { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public string TypeofIndustry { get; set; }

        public string TypeofWebsite { get; set; }

        public int? Years { get; set; }

        public string City { get; set; }

        public string Keywords { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public bool ISDeleted { get; set; }

        // For displaying department name
        public string DepartmentName { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        // Pagination
        public int pagesize { get; set; }

    }
}
