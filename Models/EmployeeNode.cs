namespace WLSPL_ERP_CRM.Models
{
    public class EmployeeNode
    {
        public string EmpCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CustRole { get; set; } = string.Empty;
        public string OrgRole { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public string SalesTLManager { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string HierarchyPath { get; set; } = string.Empty;
        public List<EmployeeNode> Children { get; set; } = new List<EmployeeNode>();

        // Renewal property
        public int InvoiceRenewal { get; set; }
    }

    public class EmployeeNodeInfo
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        //Employee Compnaies Properties
        public string TotalCompanies { get; set; } = string.Empty;
        public string PaidCompanies { get; set; } = string.Empty;
        public string UnPaidCompanies { get; set; } = string.Empty;

        //Meeting Properties
        public string Fresh { get; set; } = string.Empty;
        public string FollowUp { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;


    }


    public class InvoiceRenewalModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string SalesPerson { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public string RenewalDate { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
        public decimal TotalAmount { get; set; }
    }

}