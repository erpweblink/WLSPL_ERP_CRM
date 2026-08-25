using System.ComponentModel.DataAnnotations;

namespace WEBLINK_CRM.Models
{
    public class Companymaster 
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Company Code is required")]
        public string ? CCode  { get; set; }
        [Required(ErrorMessage = "Company Name is required")]
        public string? CName { get; set; }
        [Required(ErrorMessage = "Owner Name is required")]
        public string? OName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Mobile Number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits")]
        public string ? Mobile { get; set; }

        public string? VisitingCard { get; set; }

        public string? Type { get; set; }

        public string? Address { get; set; }

        public string? ShippingAddress { get; set; }

        public string? MeetingWithManager { get; set; }

        public DateTime? VisitDate { get; set; }

        public string? Website { get; set; }

        public bool? Status { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime? RegDate { get; set; }

        public string? SessionName { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? BDE { get; set; }

        public string? UpdatedBy { get; set; }

        public string? Email2 { get; set; }

        public string?  GSTNo { get; set; }

        public string? Area { get; set; }

        public string? Category { get; set; }

        public string? State { get; set; }

        public string? RegisterType { get; set; }

        public bool? IsUpdated { get; set; }

        public string? RequestedBy { get; set; }

        public DateTime? RequestOn { get; set; }

        public string? BillingLocation { get; set; }

        public string? BillingPincode { get; set; }

        public string? BillingStateCode { get; set; }

        public string? ShippingLocation { get; set; }

        public string? ShippingPincode { get; set; }

        public string ShippingStateCode { get; set; }

        public string? EInvTypeOfSupply { get; set; }

        public string? CountryCode { get; set; }

        public string? CountryName { get; set; }

        public string? CreatedBy { get; set; }

        public string? type { get; set; }
    }
}
