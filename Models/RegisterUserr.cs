using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WEBLINK_CRM.Models
{
    public class RegisterUserr
    {
        public List<SelectListItem>? SalesTLList { get; set; }

        public int id { get; set; }

        public string? empcode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string? name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string? email { get; set; }

        [Required(ErrorMessage = "Email password is required")]
        public string? emailpsw { get; set; }

        [Required(ErrorMessage = "Panel password is required")]
        public string? panelpsw { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Enter valid 10 digit mobile number")]
        public string? mobile { get; set; }

        [Required(ErrorMessage = "Please select role")]
        public string? role { get; set; }

        public bool status { get; set; }

        [Required(ErrorMessage = "Please select Team Leader")]
        public string? TL_Manager { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Please select Sales TL Manager")]
        public bool Sales_TL_Manager { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        public string? Designation { get; set; }

        public bool isdeleted { get; set; }

        public DateTime regdate { get; set; }

        public string? ProfileImagePath { get; set; }
    }
}
