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
    }
}