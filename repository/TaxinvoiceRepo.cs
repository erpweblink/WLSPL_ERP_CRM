using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WLSPL_ERP_CRM.Models;

namespace WLSPL_ERP_CRM.repository
{
    public class TaxinvoiceRepo : ITaxinvoiceRepo
    {
        private readonly IConfiguration _configuration;
        public TaxinvoiceRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Taxinvoice.InvoiceMain>> Getcompany()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            var parameters = new DynamicParameters();
            parameters.Add("@Action", "Getcompany");

            var companies = await connection.QueryAsync<Taxinvoice.InvoiceMain>(
                "SP_TaxInvoice",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return companies
                .Where(x => !string.IsNullOrWhiteSpace(x.cname))
                .ToList();
        }

        public async Task<Taxinvoice.InvoiceMain?> Getcompanybycname(string cname)
        {
            try
            {
                using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "Getcompanybycname");
                parameters.Add("@cname", cname);

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.InvoiceMain>(
                    "SP_TaxInvoice",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public async Task<List<Taxinvoice.InvoiceMonthSummary>> GetFinancialYearSummary(
       string financialYear)
        {
            var result = new List<Taxinvoice.InvoiceMonthSummary>();

            if (string.IsNullOrWhiteSpace(financialYear))
                return result;

            var parts = financialYear.Split('-');

            if (parts.Length != 2)
                return result;

            int startYear = Convert.ToInt32(parts[0]);
            int endYear = 2000 + Convert.ToInt32(parts[1]);

            DateTime startDate = new DateTime(startYear, 4, 1);
            DateTime endDate = new DateTime(endYear, 3, 31);

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string query = @"
            SELECT 
                MONTH(invoicedate) AS Mon,

                COUNT(invoiceno) AS TotalInvoice,

                ISNULL(
                    SUM(
                        CAST(totalamtbeforetax AS DECIMAL(18,2))
                    ), 0
                ) AS TotalTaxableValue,

                ISNULL(
                    SUM(
                        CAST(ISNULL(cgstamt, 0) AS DECIMAL(18,2))
                        +
                        CAST(ISNULL(sgstamt, 0) AS DECIMAL(18,2))
                        +
                        CAST(ISNULL(igstamt, 0) AS DECIMAL(18,2))
                    ), 0
                ) AS TotalTaxAmount,

                ISNULL(
                    SUM(
                        CAST(totalamtaftertax AS DECIMAL(18,2))
                    ), 0
                ) AS GrandTotal

            FROM invoicemain

            WHERE 
                e_invoice_cancel_status IS NULL
                AND invoicedate >= @StartDate
                AND invoicedate < DATEADD(DAY, 1, @EndDate)

            GROUP BY MONTH(invoicedate)

            ORDER BY
                CASE
                    WHEN MONTH(invoicedate) = 4 THEN 1
                    WHEN MONTH(invoicedate) = 5 THEN 2
                    WHEN MONTH(invoicedate) = 6 THEN 3
                    WHEN MONTH(invoicedate) = 7 THEN 4
                    WHEN MONTH(invoicedate) = 8 THEN 5
                    WHEN MONTH(invoicedate) = 9 THEN 6
                    WHEN MONTH(invoicedate) = 10 THEN 7
                    WHEN MONTH(invoicedate) = 11 THEN 8
                    WHEN MONTH(invoicedate) = 12 THEN 9
                    WHEN MONTH(invoicedate) = 1 THEN 10
                    WHEN MONTH(invoicedate) = 2 THEN 11
                    WHEN MONTH(invoicedate) = 3 THEN 12
                END;
        ";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@StartDate",
                        startDate);

                    cmd.Parameters.AddWithValue(
                        "@EndDate",
                        endDate);

                    using (SqlDataReader reader =
                           await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(
                                new Taxinvoice.InvoiceMonthSummary
                                {
                                    Mon = Convert.ToInt32(
                                        reader["Mon"]),

                                    TotalInvoice = Convert.ToInt32(
                                        reader["TotalInvoice"]),

                                    TotalTaxableValue = Convert.ToDecimal(
                                        reader["TotalTaxableValue"]),

                                    TotalTaxAmount = Convert.ToDecimal(
                                        reader["TotalTaxAmount"]),

                                    GrandTotal = Convert.ToDecimal(
                                        reader["GrandTotal"])
                                });
                        }
                    }
                }
            }

            return result;
        }



        public async Task<List<Taxinvoice.InvoiceList>> GetInfo(
     string financialYear,
     int? month)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            var parameters = new DynamicParameters();

            parameters.Add("@Action", "GetMonthInfo");
            parameters.Add("@FinancialYear", financialYear);
            parameters.Add("@Month", month);

            var result = await connection.QueryAsync<Taxinvoice.InvoiceList>(
                "SP_comapnylistDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<Taxinvoice.TaxInvoicePdfViewModel?> GetInvoiceForPdfAsync(int id)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await con.OpenAsync();

            // =========================================================
            // MAIN INVOICE
            // =========================================================

            string mainQuery = @"
        SELECT
            id,
            invoiceno,
            invoicedate,
            reversecharge,
            state,
            companyname,
            address,
            cgstin,
            billstate,
            totalqty,
            totalrate,
            taxablevalue,
            cgst,
            cgstamt,
            sgst,
            sgstamt,
            igst,
            igstamt,
            gstonreversecharge,
            totalamtbeforetax,
            totalamtaftertax,
            amtinwords,
            servicedescription,
            createddate,
            sessionname,
            IsApprove,
            IsReject,
            ApprovedRejectedBy,
            Remarks,
            ExportInvoiceNo,
            BillingAddress,
            BillingLocation,
            BillingGST,
            BillingPincode,
            BillingStatecode,
            AckNo,
            AckDt,
            Irn,
            SignedInvoice,
            SignedQRCode,
            Status,
            Remarkss,
            e_invoice_status,
            e_invoice_cancel_status,
            e_invoice_cancel_by,
            e_invoice_cancel_date,
            JsonFile,
            InvoiceType
        FROM InvoiceMain
        WHERE id = @id;
    ";

            var main = await con.QueryFirstOrDefaultAsync<Taxinvoice.InvoiceMain>(
                mainQuery,
                new { id }
            );

            if (main == null)
            {
                return null;
            }


            // =========================================================
            // INVOICE DETAILS
            // =========================================================

            string detailQuery = @"
        SELECT
            id,
            invoiceid,
            productdescription,
            saccode,
            qty,
            rate,
            amount,
            taxablevalue,
            cgstrate,
            cgstamt,
            sgstrate,
            sgstamt,
            igstrate,
            igstamt,
            total
        FROM InvoiceDetails
        WHERE invoiceid = @invoiceid
        ORDER BY id;
    ";

            var details = await con.QueryAsync<Taxinvoice.InvoiceDetails>(
                detailQuery,
                new { invoiceid = id }
            );




            var model = new Taxinvoice.TaxInvoicePdfViewModel
            {
                Main = main,
                Details = details.ToList()
            };




            model.CompanyName = "Web Link Services Pvt. Ltd.";

            model.CompanyAddress =
                "12th Floor, City Avenue Complex, Above Max Showroom, " +
                "W.K. Pimpri, Pune, Maharashtra";

            model.CompanyPhone = "+91 8420610192";

            model.CompanyEmail = "info@weblinkservices.net";

            model.CompanyGSTIN = "27AABCW8929Z2P";

            model.CompanyLogo = "~/assets/images/logo-dark.png";


            // =========================================================
            // BANK DETAILS
            // =========================================================

            model.BankName = "ICICI BANK";

            model.AccountNo = "91600208536854";

            model.IFSC = "ICIC0001641";

            model.Branch =
                "Aundh Bank Ltd - Ratnani Branch, Pune";


            // =========================================================
            // TOTALS
            // =========================================================

            model.TotalQuantity =
                main.totalqty ?? 0;

            model.TotalAmount =
                main.totalrate ?? 0;

            model.TotalTaxableValue =
                main.taxablevalue ?? 0;

            model.TotalCGST =
                main.cgstamt ?? 0;

            model.TotalSGST =
                main.sgstamt ?? 0;

            model.TotalIGST =
                main.igstamt ?? 0;

            model.TotalTaxAmount =
                (main.cgstamt ?? 0)
                + (main.sgstamt ?? 0)
                + (main.igstamt ?? 0);

            model.GrandTotal =
                main.totalamtaftertax ?? 0;

            model.AmountInWords =
                main.amtinwords;

            model.Remark =
                main.Remarks ?? main.Remarkss;




            model.RoundOff =
                Math.Round(model.GrandTotal, 0)
                - model.GrandTotal;


            return model;
        }

        public async Task<Taxinvoice.InvoiceMain?> Getinvoiceno()
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetInvoiceNo");
                //parameters.Add("@id", id);

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.InvoiceMain>(
                    "SP_TaxInvoice",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        
    }
}


