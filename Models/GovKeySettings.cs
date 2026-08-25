namespace WEBLINK_CRM .Models
{
    public class GovKeySettings
    {
        public string UserName { get; set; } 
        public string Password { get; set; }
        public string GSTIN { get; set; }
        public string IP_Address { get; set; }
        public string E_Invoice_Client_Id { get; set; }
        public string E_Invoice_Client_Secret { get; set; }
        public string E_Waybill_Client_Id { get; set; }
        public string E_Waybill_Client_Secret { get; set; }
    }
}
