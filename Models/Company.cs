namespace WEBLINK_CRM.Models
{
   
    public class Company
    {
        public string? Id { get; set; }  

        public string ? CompanyName { get; set; } 

        public string? CompanyCode { get; set; }  

        public string? vendorCode { get; set; }  

        public string? supplytype { get; set; }  

        public string? OwnerName { get; set; }  

        public string? GSTNo { get; set; }

        public string? BillAddress { get; set; } 

        public string? ShippAddress { get; set; }  

        public string? BillLocation { get; set; }  

        public string? ShippLocation { get; set; }  

        public string? BillingPincode { get; set; }  

        public string? ShippingPincode { get; set; }  
        public string? BillStateCode { get; set; }  
        public string? ShippStateCode { get; set; }  
        public string? BillingAddress { get; set; }  
        public string? ShippingAddress { get; set; }
        public string? Registerfor { get; set; }
        public bool ? IsDeleted { get; set; }  
        public string? CreatedBy { get; set; }  
        public DateTime ? CreatedOn { get; set; } 
        public string? UpdatedBy { get; set; }  

        public DateTime? UpdatedOn { get; set; }  

        public DateTime? DeletedOn { get; set; }  

        public string? DeletedBy { get; set; }  

        public string? AreaNAme { get; set;}

        public string? Name { get; set; }
        public string? MobileNo { get; set; }
        public string? EmailID { get; set; }
        public string ?Designation { get; set; }

        public List<ContactModel> Contacts { get; set; } = new();
        public class ContactModel
        {
            public string? Name { get; set; }
            public string? MobileNo { get; set; }
            public string? EmailID { get; set; }
            public string? Designation { get; set; }
        }
    }
}
