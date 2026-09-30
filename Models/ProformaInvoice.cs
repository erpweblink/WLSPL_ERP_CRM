namespace WLSPL_ERP_CRM.Models
{
    public class ProformaInvoice
    {

        public class ProformaInvoiceCreate
        {
            public string? Id { get; set; }

            // =====================================================
            // INVOICE
            // =====================================================

            public string? invoiceno { get; set; }

            public DateTime? invoicedate { get; set; }

            public string? reversecharge { get; set; }

            public string? InvoiceType { get; set; }

            public string? AgainstBy { get; set; }

            public string? AgainstByValue { get; set; }


            // =====================================================
            // COMPANY
            // =====================================================

            public string? companyName { get; set; }
            public string? companyCode { get; set; }

            public string? gstIn { get; set; }

            public string? Address { get; set; }

            public string? Location { get; set; }

            public string? PinCode { get; set; }

            public string? state { get; set; }

            public string? statecode { get; set; }


            // =====================================================
            // TRANSACTION
            // =====================================================

            public string? TransMode { get; set; }

            public string? TransNo { get; set; }

            public DateTime? TransDate { get; set; }

            public string? TransAmt { get; set; }


            // =====================================================
            // GST
            // =====================================================

            public decimal? cgst { get; set; }

            public decimal? cgstamt { get; set; }

            public decimal? sgst { get; set; }

            public decimal? sgstamt { get; set; }

            public decimal? igst { get; set; }

            public decimal? igstamt { get; set; }

            public string? gstonreversecharge { get; set; }


            // =====================================================
            // INVOICE TOTALS
            // =====================================================

            public decimal? totalqty { get; set; }

            public decimal? totalrate { get; set; }

            public decimal? taxablevalue { get; set; }

            public decimal? totalamtbeforetax { get; set; }

            public decimal? totalamtaftertax { get; set; }

            public string? total_tax_amount { get; set; }

            public string? amtinwords { get; set; }


            // =====================================================
            // PAYMENT SUMMARY
            // =====================================================

            public decimal? TotalBasicAmount { get; set; }

            public decimal? TotalTaxableAmount { get; set; }

            public decimal? TotalCGSTAmount { get; set; }

            public decimal? TotalSGSTAmount { get; set; }

            public decimal? TotalIGSTAmount { get; set; }

            public decimal? TotalGSTAmount { get; set; }

            public decimal? TotalInvoiceAmount { get; set; }

            public decimal? BasicAmountReceived { get; set; }

            public decimal? GSTAmountReceived { get; set; }

            public decimal? TotalAmountReceived { get; set; }

            public decimal? PendingBasicAmount { get; set; }

            public decimal? PendingGSTAmount { get; set; }

            public decimal? PendingAmountBeforeTDS { get; set; }

            public decimal? TDSPercentage { get; set; }

            public decimal? TDSAmount { get; set; }

            public decimal? BankAmountReceived { get; set; }

            public decimal? TotalTDSAmount { get; set; }

            public decimal? FinalPendingAmount { get; set; }

            public decimal? TotalAmountBalance { get; set; }


            // =====================================================
            // SERVICE
            // =====================================================

            public string? servicedescription { get; set; }


            // =====================================================
            // SESSION / EMPLOYEE
            // =====================================================

            public string? sessionname { get; set; }

            public string? NAME { get; set; }


            // =====================================================
            // SYSTEM
            // =====================================================

            public DateTime? createddate { get; set; }

            public bool? IsApprove { get; set; }

            public bool? IsReject { get; set; }

            public string? ApprovedRejectedBy { get; set; }

            public string? Remarks { get; set; }

            public string? Remarkss { get; set; }


            // =====================================================
            // EXPORT
            // =====================================================

            public string? ExportInvoiceNo { get; set; }


            // =====================================================
            // BILLING
            // =====================================================

            public string? BillingAddress { get; set; }

            public string? BillingLocation { get; set; }

            public string? BillingGST { get; set; }

            public string? BillingPincode { get; set; }

            public string? BillingStatecode { get; set; }


            // =====================================================
            // E-INVOICE
            // =====================================================

            public string? AckNo { get; set; }

            public DateTime? AckDt { get; set; }

            public string? Irn { get; set; }

            public string? SignedInvoice { get; set; }

            public string? SignedQRCode { get; set; }

            public string? Status { get; set; }

            public string? e_invoice_status { get; set; }

            public string? e_invoice_cancel_status { get; set; }

            public string? e_invoice_cancel_by { get; set; }

            public string? e_invoice_created_by { get; set; }

            public DateTime? e_invoice_cancel_date { get; set; }

            public string? JsonFile { get; set; }


            // =====================================================
            // FINANCIAL YEAR
            // =====================================================

            public int? mon { get; set; }

            public int? TotalInvoice { get; set; }

            public decimal? TotalTaxableValue { get; set; }

            public decimal? TotalTaxAmount { get; set; }

            public decimal? GrandTotal { get; set; }
        }


        public class InvoiceDetails
        {
            public int id { get; set; }

            public int invoiceid { get; set; }

            public string? serviceName { get; set; }

            public string? serviceId { get; set; }

            public string? serviceTill { get; set; }

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

        public class InvoiceBankDetail
        {
            public string? mode { get; set; }
            public string? bankName { get; set; }

            public string? chequeNo { get; set; }

            public DateTime? date { get; set; }

            public decimal? amount { get; set; }
            public string? taxinvoiceid { get; set; }

        }


        public class ProformaInvoiceCreateVM
        {
            public ProformaInvoiceCreate main { get; set; }
                = new ProformaInvoiceCreate();

            public List<InvoiceDetails> details { get; set; }
                = new List<InvoiceDetails>();

            public List<ProformaInvoiceCreate> companies { get; set; }
                = new List<ProformaInvoiceCreate>();

            public List<InvoiceBankDetail> BankDetails { get; set; }
        = new();
        }
    }
}
