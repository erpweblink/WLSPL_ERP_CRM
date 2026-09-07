namespace WLSPL_ERP_CRM.Models
{
    public class EmployeeNode
    {
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string CustRole { get; set; }
        public string OrgRole { get; set; }
        public string Designation { get; set; }
        public string Status { get; set; }
        public string ParentCode { get; set; }
        public string SalesTLManager { get; set; }
        public int HierarchyLevel { get; set; }
        public string HierarchyPath { get; set; }
        public List<EmployeeNode> Children { get; set; } = new List<EmployeeNode>();
    }
}