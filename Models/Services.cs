using Microsoft.AspNetCore.Http.HttpResults;

namespace WEBLINK_CRM.Models
{
    public class Services
    {
        public string ID { get; set; }  
        public string Department { get; set; }
        public string ServicesDesc { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool ISDeleted { get; set; }
        public string DepartmentName { get; set; }
        public string TypeofIndustry { get; set; }
        public string Years { get; set; }
        public decimal City { get; set; }
        public decimal Keywords { get; set; }
        public string pagesize { get; set; }
        public string Typeofwebsite { get; set; }

    }

    public class Department
    {
        public string ID { get; set; }
        public string DeptID { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ServicesDesc { get; set; }

    }



}
