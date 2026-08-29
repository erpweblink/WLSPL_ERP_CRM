using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WEBLINK_CRM.Models;
using WLSPL_ERP_CRM.Models;
using static WLSPL_ERP_CRM.Models.Taxinvoice;

namespace WLSPL_ERP_CRM.repository
{
    public class TaxinvoiceRepo : ITaxinvoiceRepo
    {
        private readonly IConfiguration _configuration;
        public TaxinvoiceRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> Deletereords(int ID)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await con.OpenAsync();

            var result = await con.QueryFirstOrDefaultAsync<bool>(
                "SP_DeleteInvoice",
                new
                {
                    ID = ID
                },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }


        public async Task<List<TaxInvoiceCreate>> Getcompany()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            var parameters = new DynamicParameters();
            parameters.Add("@Action", "Getcompany");

            var companies = await connection.QueryAsync<TaxInvoiceCreate>(
                "SP_TaxInvoice",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return companies.ToList();
        }

        public async Task<TaxInvoiceCreate> Getcompanybycname(string cname)
        {
            try
            {
                using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "Getcompanybycname");
                parameters.Add("@cname", cname);

                var result = await connection.QueryFirstOrDefaultAsync<TaxInvoiceCreate>(
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

        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetFinancialYearSummary(
           string financialYear)
        {
            var result = new List<Taxinvoice.TaxInvoiceCreate>();

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
                                new Taxinvoice.TaxInvoiceCreate
                                {
                                    mon = Convert.ToInt32(
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


        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetInfo(
         string financialYear,
         int? month)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            var parameters = new DynamicParameters();

            parameters.Add("@Action", "GetMonthInfo");
            parameters.Add("@FinancialYear", financialYear);
            parameters.Add("@Month", month);

            var result = await connection.QueryAsync<Taxinvoice.TaxInvoiceCreate>(
                "SP_comapnylistDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }



        public async Task<dynamic> Getinvoicebyid(int ID)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await con.OpenAsync();

            using var multi = await con.QueryMultipleAsync(
                "SP_TaxInvoice",
                new
                {
                    id = ID,
                    Action = "GetInvoiceById"
                },
                commandType: CommandType.StoredProcedure
            );

            var main = await multi.ReadFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreate>();

            if (main == null)
            {
                return null;
            }

            var details = (await multi.ReadAsync<Taxinvoice.InvoiceDetails>())
                .ToList();

            return new Taxinvoice.TaxInvoiceCreateVM
            {
                main = main,
                details = details
            };
        }

        public async Task<Taxinvoice.TaxInvoiceCreateVM?> GetInvoiceForPdfAsync(int id)
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
            e_invoice_created_by,
            e_invoice_cancel_date,
            JsonFile,
            InvoiceType
        FROM InvoiceMain
        WHERE id = @id;
    ";

            var main = await con.QueryFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreate>(
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


            // =========================================================
            // VIEW MODEL
            // =========================================================

            var model = new Taxinvoice.TaxInvoiceCreateVM
            {
                main = main,
                details = details.ToList()
            };


            // =========================================================
            // RETURN
            // =========================================================

            return model;
        }


        public async Task<Taxinvoice.TaxInvoiceCreateVM?> Getinvoiceno()
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetInvoiceNo");
                //parameters.Add("@id", id);

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreateVM>(
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

        public async Task<Taxinvoice.TaxInvoiceCreate?> Getinvoicenoss()
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetInvoiceNo");
                //parameters.Add("@id", id);

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreate>(
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

        public async Task<bool> UpdateSave(TaxInvoiceCreateVM model, string Action)
        {
            try
            {
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("Conn_Stringg"));

                await connection.OpenAsync();

                // =====================================================
                // MAIN INVOICE PARAMETERS
                // =====================================================

                var parameters = new DynamicParameters();

                parameters.Add("@id", model.main.Id);
                parameters.Add("@invoiceno", model.main.invoiceno);
                parameters.Add("@invoicedate", model.main.invoicedate);
                parameters.Add("@reversecharge", model.main.reversecharge);

                // Company
                parameters.Add("@companyname", model.main.companyName);
                parameters.Add("@cgstin", model.main.gstIn);
                parameters.Add("@address", model.main.Address);
                parameters.Add("@BillingLocation", model.main.Location);
                parameters.Add("@BillingPincode", model.main.PinCode);

                parameters.Add("@state", "Maharashtra");
                parameters.Add("@billstate", model.main.state);
                parameters.Add("@BillingStatecode", model.main.statecode);

                // Transaction
                parameters.Add("@TransMode", model.main.TransMode);
                parameters.Add("@TransNo", model.main.TransNo);
                parameters.Add("@TransDate", model.main.TransDate);
                parameters.Add("@TransAmt", model.main.TransAmt);

                // =====================================================
                // GST
                // =====================================================

                parameters.Add("@cgst", model.main.cgst);
                parameters.Add("@cgstamt", model.main.cgstamt);

                parameters.Add("@sgst", model.main.sgst);
                parameters.Add("@sgstamt", model.main.sgstamt);

                parameters.Add("@igst", model.main.igst);
                parameters.Add("@igstamt", model.main.igstamt);

                // =====================================================
                // TOTALS
                // =====================================================

                parameters.Add("@totalqty", model.main.totalqty);
                parameters.Add("@totalrate", model.main.totalrate);
                parameters.Add("@taxablevalue", model.main.taxablevalue);
                parameters.Add("@totalamtbeforetax", model.main.totalamtbeforetax);
                parameters.Add("@totalamtaftertax", model.main.totalamtaftertax);

                // =====================================================
                // OTHER
                // =====================================================

                parameters.Add("@amtinwords", model.main.amtinwords);
                parameters.Add("@sessionname", model.main.sessionname);

                // =====================================================
                // BILLING
                // =====================================================

                parameters.Add("@BillingAddress", model.main.Address);
                parameters.Add("@BillingLocation", model.main.Location);
                parameters.Add("@BillingGST", model.main.gstIn);
                parameters.Add("@BillingPincode", model.main.PinCode);
                parameters.Add("@BillingStatecode", model.main.statecode);

                // =====================================================
                // ACTION
                // =====================================================

                parameters.Add("@action", Action);

                // =====================================================
                // OUTPUT INVOICE ID
                // =====================================================

                parameters.Add(
                    "@myinvoice",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output
                );

                // =====================================================
                // SAVE MAIN INVOICE
                // =====================================================

                int rowsAffected = await connection.ExecuteAsync(
                    "[dbo].[SP_AddInvoice]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // =====================================================
                // GET INVOICE ID
                // =====================================================

                int myInvoice = parameters.Get<int>("@myinvoice");

                // If UPDATE, use existing invoice ID
                if (Action.Equals(
                        "updateOldData",
                        StringComparison.OrdinalIgnoreCase))
                {
                    myInvoice = model.main.Id;

                    // =================================================
                    // DELETE OLD DETAILS
                    // =================================================

                    await DeleteInvoiceDetails(
                        myInvoice,
                        connection
                    );
                }

                // =====================================================
                // INSERT NEW DETAILS
                // =====================================================

                if (model.details != null &&
                    model.details.Count > 0 &&
                    myInvoice > 0)
                {
                    foreach (var detail in model.details)
                    {
                        var parametersd = new DynamicParameters();

                        parametersd.Add(
                            "@invoiceid",
                            myInvoice
                        );

                        parametersd.Add(
                            "@productdescription",
                            detail.productdescription
                        );

                        parametersd.Add(
                            "@saccode",
                            detail.saccode
                        );

                        parametersd.Add(
                            "@qty",
                            detail.qty
                        );

                        parametersd.Add(
                            "@rate",
                            detail.rate
                        );

                        parametersd.Add(
                            "@taxablevalue",
                            detail.taxablevalue
                        );

                        // CGST
                        parametersd.Add(
                            "@cgstrate",
                            detail.cgstrate
                        );

                        parametersd.Add(
                            "@cgstamt",
                            detail.cgstamt
                        );

                        // SGST
                        parametersd.Add(
                            "@sgstrate",
                            detail.sgstrate
                        );

                        parametersd.Add(
                            "@sgstamt",
                            detail.sgstamt
                        );

                        // IGST
                        parametersd.Add(
                            "@igstrate",
                            detail.igstrate
                        );

                        parametersd.Add(
                            "@igstamt",
                            detail.igstamt
                        );

                        // Total
                        parametersd.Add(
                            "@total",
                            detail.total
                        );

                        // =================================================
                        // INSERT DETAIL
                        // =================================================

                        const string invoicedetailsSql = @"
                    INSERT INTO [InvoiceDetails]
                    (
                        [invoiceid],
                        [productdescription],
                        [saccode],
                        [qty],
                        [rate],
                        [taxablevalue],
                        [cgstrate],
                        [cgstamt],
                        [sgstrate],
                        [sgstamt],
                        [igstrate],
                        [igstamt],
                        [total]
                    )
                    VALUES
                    (
                        @invoiceid,
                        @productdescription,
                        @saccode,
                        @qty,
                        @rate,
                        @taxablevalue,
                        @cgstrate,
                        @cgstamt,
                        @sgstrate,
                        @sgstamt,
                        @igstrate,
                        @igstamt,
                        @total
                    );";

                        await connection.ExecuteAsync(
                            invoicedetailsSql,
                            parametersd
                        );
                    }
                }

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task DeleteInvoiceDetails(int invoiceId, SqlConnection connection)
        {
            const string sql = @"
        DELETE FROM [InvoiceDetails]
        WHERE [invoiceid] = @invoiceId;
    ";

            await connection.ExecuteAsync(
                sql,
                new
                {
                    invoiceId = invoiceId
                }
            );
        }


    }
}


