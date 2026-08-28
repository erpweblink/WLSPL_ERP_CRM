namespace WLSPL_ERP_CRM.Models
{
    public class Taxinvoice
    {
        public class InvoiceMain
        {
            public int id { get; set; }

            public string? invoiceno { get; set; }

            public DateTime? invoicedate { get; set; }

            public string? reversecharge { get; set; }

            public string? state { get; set; }

            public string? companyname { get; set; }

            public string? address { get; set; }

            public string? TransMode { get; set; }

            public string? TransNo { get; set; }

            public DateTime? TransDate { get; set; }

            public decimal? TransAmt { get; set; }

            public string? cgstin { get; set; }

            public string? billstate { get; set; }

            public decimal? totalqty { get; set; }

            public decimal? totalrate { get; set; }

            public decimal? taxablevalue { get; set; }

            public decimal? cgst { get; set; }

            public decimal? cgstamt { get; set; }

            public decimal? sgst { get; set; }

            public decimal? sgstamt { get; set; }

            public decimal? igst { get; set; }

            public decimal? igstamt { get; set; }

            public string? gstonreversecharge { get; set; }

            public decimal? totalamtbeforetax { get; set; }

            public decimal? totalamtaftertax { get; set; }

            public string? amtinwords { get; set; }

            public string? servicedescription { get; set; }

            public DateTime? createddate { get; set; }

            public string? sessionname { get; set; }

            public bool? IsApprove { get; set; }

            public bool? IsReject { get; set; }

            public string? ApprovedRejectedBy { get; set; }

            public string? Remarks { get; set; }

            public string? ExportInvoiceNo { get; set; }

            public string? BillingAddress { get; set; }

            public string? BillingLocation { get; set; }

            public string? BillingGST { get; set; }

            public string? BillingPincode { get; set; }

            public string? BillingStatecode { get; set; }

            public string? AckNo { get; set; }

            public DateTime? AckDt { get; set; }

            public string? Irn { get; set; }

            public string? SignedInvoice { get; set; }

            public string? SignedQRCode { get; set; }

            public string? Status { get; set; }

            public string? Remarkss { get; set; }

            public string? e_invoice_status { get; set; }

            public string? e_invoice_cancel_status { get; set; }

            public string? e_invoice_cancel_by { get; set; }

            public DateTime? e_invoice_cancel_date { get; set; }

            public string? JsonFile { get; set; }

            public string? InvoiceType { get; set; }

            public string ? total_tax_amount { get; set; }

            public string? BillingState { get; set; }
        }

        public class InvoiceDetails
        {
            public int id { get; set; }

            public int invoiceid { get; set; }

            public string? productdescription { get; set; }

            public string? saccode { get; set; }

            public decimal? qty { get; set; }

            public decimal? rate { get; set; }

            public decimal? amount { get; set; }

            public decimal? taxablevalue { get; set; }

            public decimal? cgstrate { get; set; }

            public decimal? cgstamt { get; set; }

            public decimal? sgstrate { get; set; }

            public decimal? sgstamt { get; set; }

            public decimal? igstrate { get; set; }

            public decimal? igstamt { get; set; }

            public decimal? total { get; set; }
        }

        public class InvoiceMonthInfo
        {
            public int TotalInvoice { get; set; }

            public int Mon { get; set; }

            public decimal TotalTaxableValue { get; set; }

            public decimal TotalTaxAmt { get; set; }

            public decimal GTotal { get; set; }
        }

        public class InvoiceList
        {
            public int id { get; set; }

            public DateTime? invoicedate { get; set; }

            public string? invoiceno { get; set; }

            public string? companyname { get; set; }

            public string? cgstin { get; set; }

            public decimal totalamtbeforetax { get; set; }

            public decimal total_tax_amount { get; set; }

            public decimal totalamtaftertax { get; set; }

            public bool? isapprove { get; set; }

            public bool? isreject { get; set; }

            public string? ExportInvoiceNo { get; set; }

            public string? NAME { get; set; }
        }

        public class InvoiceMonthSummary
        {
            public int Mon { get; set; }
            public int TotalInvoice { get; set; }
            public decimal TotalTaxableValue { get; set; }
            public decimal TotalTaxAmount { get; set; }
            public decimal GrandTotal { get; set; }
        }
        public class TaxInvoiceIndexViewModel
        {
            public IEnumerable<InvoiceList> Invoices { get; set; }
                = new List<InvoiceList>();

            public List<InvoiceMonthSummary> MonthSummary { get; set; }
                = new List<InvoiceMonthSummary>();

            public string FinancialYear { get; set; } = "";

            public int Month { get; set; }
        }
        public class TaxInvoicePdfViewModel
        {
            // Invoice Header
            public InvoiceMain Main { get; set; } = new InvoiceMain();

            // Invoice Items
            public List<InvoiceDetails> Details { get; set; }
                = new List<InvoiceDetails>();


            // =====================================================
            // COMPANY DETAILS
            // =====================================================

            public string? CompanyLogo { get; set; }

            
            public string? CompanyName { get; set; }

            public string? CompanyAddress { get; set; }

            public string? CompanyPhone { get; set; }

            public string? CompanyEmail { get; set; }

            public string? CompanyGSTIN { get; set; }


            // =====================================================
            // BANK DETAILS
            // =====================================================

            public string? BankName { get; set; }

            public string? AccountNo { get; set; }

            public string? IFSC { get; set; }

            public string? Branch { get; set; }


            // =====================================================
            // TOTALS
            // =====================================================

            public decimal TotalQuantity { get; set; }

            public decimal TotalAmount { get; set; }

            public decimal TotalTaxableValue { get; set; }

            public decimal TotalCGST { get; set; }

            public decimal TotalSGST { get; set; }

            public decimal TotalIGST { get; set; }

            public decimal TotalTaxAmount { get; set; }

            public decimal GrandTotal { get; set; }

            public decimal RoundOff { get; set; }


            public string? AmountInWords { get; set; }

            public string? Remark { get; set; }
        }




    }
}
