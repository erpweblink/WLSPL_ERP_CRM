using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WEBLINK_CRM.Models
{

    public class LeadGenration
    {
        [Key]
        public int ID { get; set; }

        public string? Leadcode { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; }

        public string? CompanyId { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        public string Requirements { get; set; }

        [Required]
        [StringLength(50)]
        public string Product { get; set; }

        public string? Status { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        public string UserName { get; set; }

        public string? UserID { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? Createdby { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string? MessageId { get; set; }

        public string? LeadId { get; set; }

        public string? PageId { get; set; }

        public string? Type { get; set; }

        public string? JsonData { get; set; }

        public string? OwnerName { get; set; }
    }
}
