using System.ComponentModel.DataAnnotations;

namespace WEBLINK_CRM.Models
{
    public class VM_WorkOrder
    {
        public int? ID { get; set; }

        public string? Type { get; set; }
        public string? WOStatus { get; set; }
        public string? WONo { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? OwnerName { get; set; }
        public string? Address { get; set; }
        public string? GSTNO { get; set; }
        public string? EmailID { get; set; }

        public DateTime? TodayDt { get; set; }
        public DateTime? RenewalDt { get; set; }

        public string? PaymentMode { get; set; }

        public decimal? TotalDealBasicAmount { get; set; }
        public decimal? TotalDealGSTAmount { get; set; }
        public decimal? BasicAmountReceived { get; set; }
        public decimal? GSTAmountReceived { get; set; }
        public decimal? BalanceBasicAmount { get; set; }
        public decimal? BalanceGSTAmount { get; set; }
        public decimal? TotalAmountBalance { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public List<WorkOrderDetailVM>? objtblWorkOrderDtl { get; set; }

        public List<BankDetailVM>? objtblBankDetail { get; set; }

        public class WorkOrderDetailVM
        {
            public string? ID { get; set; }
            public string? Department { get; set; }
            public string? ServicesDescription { get; set; }
            public string? Remark { get; set; }

            public decimal? Qty { get; set; }
            public int? NoofYr { get; set; }

            public decimal? Rate { get; set; }
            public decimal? Amount { get; set; }

            public bool? IsComplete { get; set; }

            public string? Status { get; set; }
        }

        public class BankDetailVM
        {
            public string? ID { get; set; }
            public string? BankName { get; set; }
            public string? ChequeNo { get; set; }
            public DateTime? ChequeDate { get; set; }
            public decimal? Amount { get; set; }
        }
    }
}
