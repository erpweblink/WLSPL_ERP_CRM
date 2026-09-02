using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBLINK_CRM.Models
{
    public class VM_Quotation
    {
        public int? ID { get; set; } = default;

        public string? QuotationNo { get; set; } = default;

        public DateTime? QuotationDate { get; set; }
        public string? ReverseCharge { get; set; } = default;

        public string? State { get; set; } = default;

        public string? CompanyName { get; set; } = default;
        public string? CompanyCode { get; set; } = default;

        public string? Address { get; set; } = default;

        public string? GSTNO { get; set; } = default;

        public string? BillState { get; set; } = default;

        [NotMapped]
        public string? CreatedBy { get; set; } = default;


        public string? TotalAmtBeforeTax { get; set; } = default;
        public string? TotalAmtAfterTax { get; set; } = default;

        public List<QuotationDetailVM>? objtblQuotationDtl { get; set; } = default;


        public class QuotationDetailVM
        {
            public int? ID { get; set; }

            public int? QuotationID { get; set; }

            public string? ProductDescription { get; set; }

            public string? SACCode { get; set; }

            public string? Qty { get; set; }

            public string? Rate { get; set; }

            public string? Amount { get; set; }

            public string? TaxableValue { get; set; }

            public string? CGSTRate { get; set; }

            public string? CGSTAmt { get; set; }

            public string? SGSTRate { get; set; }

            public string? SGSTAmt { get; set; }

            public string? IGSTRate { get; set; }

            public string? IGSTAmt { get; set; }

            public string? Total { get; set; }
        }
    }

}

