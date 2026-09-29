using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
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

            const string companySql = @"select ccode As compCode,cname As companyName from Company where isdeleted = 0  and status =1 and type='paid';";

            var companies = await connection.QueryAsync<TaxInvoiceCreate>(companySql);

            return companies.ToList();

        }

        public async Task<TaxInvoiceCreate> Getcompanybycname(string cname)
        {
            try
            {
                using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@cname", cname);

                const string companySql = @"select ccode AS compCode,gstno As gstIn,address As Address, Billing_location As Location ,Billing_pincode As PinCode, State As state,Billing_statecode As statecode   from Company  where isdeleted = 0  and status =1 and type='paid' and cname = @cname;";

                var result = await connection.QueryFirstOrDefaultAsync<TaxInvoiceCreate>(companySql, parameters);

                return result ?? new TaxInvoiceCreate();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetFinancialYearSummary(string financialYear)
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

            using (SqlConnection con =new SqlConnection(connectionString))
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

        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetInfo(string financialYear,int? month)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            int startYear = int.Parse(financialYear.Substring(0, 4));

            DateTime startDate;
            DateTime endDate;

            // Month = 0 or null => Full financial year
            if (!month.HasValue || month.Value == 0)
            {
                startDate = new DateTime(startYear, 4, 1);
                endDate = new DateTime(startYear + 1, 4, 1);
            }
            else
            {
                int selectedMonth = month.Value;

                int year = selectedMonth >= 4
                    ? startYear
                    : startYear + 1;

                startDate = new DateTime(year, selectedMonth, 1);
                endDate = startDate.AddMonths(1);
            }

            const string query = @"
                        SELECT
                            id,
                            invoicedate,
                            invoiceno,
                            companyname,
                            cgstin AS gstin,

                            ISNULL(
                                TRY_CAST(totalamtbeforetax AS DECIMAL(18,2)),
                                0
                            ) AS totalamtbeforetax,

                            ISNULL(
                                TRY_CAST(sgstamt AS DECIMAL(18,2)),
                                0
                            )
                            +
                            ISNULL(
                                TRY_CAST(cgstamt AS DECIMAL(18,2)),
                                0
                            )
                            +
                            ISNULL(
                                TRY_CAST(igstamt AS DECIMAL(18,2)),
                                0
                            ) AS total_tax_amount,

                            ISNULL(
                                TRY_CAST(totalamtaftertax AS DECIMAL(18,2)),
                                0
                            ) AS totalamtaftertax,

                            isapprove,
                            isreject,
                            ExportInvoiceNo,
                            NAME

                        FROM [stswlspl].vw_invoicebyemp

                        WHERE invoicedate >= @StartDate
                          AND invoicedate < @EndDate

                        ORDER BY invoicedate ASC;
                    ";

            var parameters = new DynamicParameters();

            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var result = await connection.QueryAsync<Taxinvoice.TaxInvoiceCreate>(query,parameters);

            return result.ToList();
        }


        public async Task<dynamic> Getinvoicebyid(int ID)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await con.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@id", ID);

            const string companySql = @"SELECT id, invoiceno, reversecharge, InvoiceType, invoicedate,
                companyname AS companyName, cgstin AS gstIn, address AS Address, '' As Location, BillingPincode AS PinCode,
                state, BillingStatecode As statecode, BillingLocation, TransMode, TransNo, TransDate, TransAmt, cgst, cgstamt, sgst,
                sgstamt, igst, igstamt, gstonreversecharge, totalqty, totalrate, taxablevalue, totalamtbeforetax, totalamtaftertax,
                amtinwords, servicedescription, sessionname, createddate, IsApprove, IsReject, ApprovedRejectedBy, Remarks, Remarkss,
                ExportInvoiceNo, BillingAddress, BillingGST, BillingPincode, BillingStatecode, AgainstBy, AgainstByValue,
                TotalPayable, TdsPer, TdsAmt, Remarks FROM InvoiceMain
                WHERE id = @id;
               
                SELECT id, invoiceid, productdescription, saccode, qty, rate, amount, taxablevalue, cgstrate,
                cgstamt, sgstrate, sgstamt, igstrate, igstamt, total, ServiceName, ServiceId, ValidateTill as serviceTill 
                FROM InvoiceDetails WHERE invoiceid = @id ORDER BY id;
            ";

            using var multi = await con.QueryMultipleAsync(companySql, parameters);

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
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

                const string companySql = @"SELECT [WLSPL].[FN_GenerateTaxInvoiceNo]() AS InvoiceNo";

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreateVM>(companySql);

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
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

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
                parameters.Add("@companyCode", model.main.compCode);
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

                parameters.Add("@TotalPayable", model.main.TotalPayable);
                parameters.Add("@TdsPer", model.main.TdsPer);
                parameters.Add("@TdsAmt", model.main.TdsAmt);
                parameters.Add("@Remarks", model.main.Remarks);

                // =====================================================
                // OTHER
                // =====================================================

                parameters.Add("@amtinwords", model.main.amtinwords);
                parameters.Add("@sessionname", model.main.sessionname);
                parameters.Add("@AgainstBy", model.main.AgainstBy);
                parameters.Add("@AgainstByValue", model.main.AgainstByValue);

                // =====================================================
                // BILLING
                // =====================================================

                parameters.Add("@BillingAddress", model.main.Address);
                parameters.Add("@BillingLocation", model.main.Location);
                parameters.Add("@BillingGST", model.main.gstIn);
                parameters.Add("@BillingPincode", model.main.PinCode);
                parameters.Add("@BillingStatecode", model.main.statecode);
                parameters.Add("@action", Action);

                parameters.Add("@myinvoice", dbType: DbType.Int32, direction: ParameterDirection.Output);

                int rowsAffected = await connection.ExecuteAsync("[dbo].[SP_AddInvoice]", parameters, commandType: CommandType.StoredProcedure);

                int myInvoice = parameters.Get<int>("@myinvoice");

                // If UPDATE, use existing invoice ID
                if (Action.Equals("updateOldData", StringComparison.OrdinalIgnoreCase))
                {
                    myInvoice = Convert.ToInt32(model.main.Id);

                    await DeleteInvoiceDetails(
                        myInvoice,
                        connection
                    );
                }


                if (model.details != null && model.details.Count > 0 && myInvoice > 0)
                {
                    foreach (var detail in model.details)
                    {
                        var parametersd = new DynamicParameters();

                        parametersd.Add("@invoiceid", myInvoice);
                        parametersd.Add("@serviceId", detail.serviceId);
                        parametersd.Add("@serviceName", detail.serviceName);
                        parametersd.Add("@productdescription", detail.productdescription);
                        parametersd.Add("@saccode", detail.saccode);
                        parametersd.Add("@qty", detail.qty);
                        parametersd.Add("@rate", detail.rate);
                        parametersd.Add("@taxablevalue", detail.taxablevalue);
                        parametersd.Add("@cgstrate", detail.cgstrate);
                        parametersd.Add("@cgstamt", detail.cgstamt);
                        parametersd.Add("@sgstrate", detail.sgstrate);
                        parametersd.Add("@sgstamt", detail.sgstamt);
                        parametersd.Add("@igstrate", detail.igstrate);
                        parametersd.Add("@igstamt", detail.igstamt);
                        parametersd.Add("@total", detail.total);
                        parametersd.Add("@ServiceId", detail.serviceId);
                        parametersd.Add("@ServiceName", detail.serviceName);
                        parametersd.Add("@ValidateTill", detail.serviceTill);

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
                                    [total],
                                    [ServiceName],
                                    [ServiceId],
                                    [ValidateTill]
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
                                    @total,
                                    @ServiceName,
                                    @ServiceId,
                                    @ValidateTill
                                );";

                        await connection.ExecuteAsync(invoicedetailsSql,parametersd);

                        if (Action == "insert" && !string.IsNullOrEmpty(detail.proformadetailsId))
                        {
                            var bankIds = detail.proformadetailsId
                                .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                                .Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();

                            if (bankIds.Any())
                            {
                                const string proformadtls = @"
                                    UPDATE dbo.tbl_ProformaInvoiceBankDetails 
                                    SET TaxInvoiceID = @invoiceid 
                                    WHERE id IN @bankIds";

                                await connection.ExecuteAsync(proformadtls, new
                                {
                                    invoiceid = myInvoice,
                                    bankIds = bankIds  
                                });
                            }
                        }
                    }
                }

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string BuildChangeComment(Taxinvoice.TaxInvoiceCreate oldMain, List<Taxinvoice.InvoiceDetails> oldDetails,
          Taxinvoice.TaxInvoiceCreate newMain, List<Taxinvoice.InvoiceDetails> newDetails)
        {
            var changes = new List<string>();

            void Check(string label, object? oldVal, object? newVal)
            {
                var o = oldVal?.ToString()?.Trim() ?? "";
                var n = newVal?.ToString()?.Trim() ?? "";
                if (!string.Equals(o, n, StringComparison.OrdinalIgnoreCase))
                    changes.Add($"{label} changed from '{(string.IsNullOrEmpty(o) ? "N/A" : o)}' to '{(string.IsNullOrEmpty(n) ? "N/A" : n)}'");
            }

            // ── Invoice Header ──
            Check("Invoice Date", oldMain.invoicedate, newMain.invoicedate);
            Check("Reverse Charge", oldMain.reversecharge, newMain.reversecharge);

            // ── Bill To ──
            Check("Company Name", oldMain.companyName, newMain.companyName);
            Check("GST No", oldMain.gstIn, newMain.gstIn);
            Check("Address", oldMain.Address, newMain.Address);
            Check("Billing Location", oldMain.BillingLocation, newMain.BillingLocation);
            Check("Pincode", oldMain.PinCode, newMain.PinCode);
            Check("State", oldMain.state, newMain.state);
            Check("State Code", oldMain.statecode, newMain.statecode);

            // ── Transaction ──
            Check("Transaction Mode", oldMain.TransMode, newMain.TransMode);
            Check("Transaction No", oldMain.TransNo, newMain.TransNo);
            Check("Transaction Date", oldMain.TransDate, newMain.TransDate);
            Check("Transaction Amt", oldMain.TransAmt, newMain.TransAmt);

            // ── Against By ──
            Check("Against By", oldMain.AgainstBy, newMain.AgainstBy);
            Check("Against By Value", oldMain.AgainstByValue, newMain.AgainstByValue);

            // ── Totals ──
            Check("Taxable Value", oldMain.taxablevalue, newMain.taxablevalue);
            Check("Grand Total", oldMain.totalamtaftertax, newMain.totalamtaftertax);
            Check("TDS %", oldMain.TdsPer, newMain.TdsPer);
            Check("TDS Amount", oldMain.TdsAmt, newMain.TdsAmt);
            Check("Total Payable", oldMain.TotalPayable, newMain.TotalPayable);

            // ── Service Detail Rows ──
            oldDetails ??= new List<Taxinvoice.InvoiceDetails>();
            newDetails ??= new List<Taxinvoice.InvoiceDetails>();

            // Rows added
            if (newDetails.Count > oldDetails.Count)
                changes.Add($"{newDetails.Count - oldDetails.Count} new service row(s) added");

            // Rows removed
            if (newDetails.Count < oldDetails.Count)
                changes.Add($"{oldDetails.Count - newDetails.Count} service row(s) removed");

            // Compare matching rows
            int compareCount = Math.Min(oldDetails.Count, newDetails.Count);
            for (int i = 0; i < compareCount; i++)
            {
                var od = oldDetails[i];
                var nd = newDetails[i];
                string prefix = $"Row {i + 1}";

                Check($"{prefix} Service", od.serviceName, nd.serviceName);
                Check($"{prefix} Description", od.productdescription, nd.productdescription);
                Check($"{prefix} SAC Code", od.saccode, nd.saccode);
                Check($"{prefix} Rate", od.rate, nd.rate);
                Check($"{prefix} CGST %", od.cgstrate, nd.cgstrate);
                Check($"{prefix} SGST %", od.sgstrate, nd.sgstrate);
                Check($"{prefix} IGST %", od.igstrate, nd.igstrate);
                Check($"{prefix} Total", od.total, nd.total);
            }

            if (!changes.Any()) return string.Empty;

            return $"Invoice {newMain.invoiceno} updated on {DateTime.Now:dd-MMM-yyyy HH:mm}: "
                   + string.Join("; ", changes) + ".";
        }

        public async Task SaveInvoiceChangeHistory(string sessionName, string invoiceNo, string message)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

            const string sql = @"
                INSERT INTO [dbo].[CommentHistory] (sessionname, ccode, commentdatetime, message)
                VALUES (@sessionname, @ccode, @commentdatetime, @message)";

            await connection.ExecuteAsync(sql, new
            {
                sessionname = sessionName,
                ccode = invoiceNo,
                commentdatetime = DateTime.Now,
                message = message
            });
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

        public async Task<List<TaxInvoiceCreate>> GetApprovelList()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

            const string companySql = @" SELECT [WLSPL].[FN_GenerateTaxInvoiceNo]() AS InvoiceNo";

            var result = await connection.QueryAsync<TaxInvoiceCreate>(companySql);

            return result.ToList();

        }

        public async Task<bool> Approve(int id, string user)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                string query = @"UPDATE InvoiceMain
                         SET IsApprove = 1,
                             ApprovedRejectedBy = @Createdby
                         WHERE ID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Createdby", user);

                    await con.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    return result > 0;
                }
            }
        }

        public async Task<bool> Reject(int id, string user)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                string query = @"UPDATE InvoiceMain
                         SET IsReject = 1,
                             ApprovedRejectedBy = @Createdby
                         WHERE ID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Createdby", user);

                    await con.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    return result > 0;
                }
            }
        }

        public async Task<List<InvoiceDetails>> SearchServices(string q)
        {
            var results = new List<InvoiceDetails>();

            using var con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")
                ?? throw new Exception("Connection string 'Conn_Stringg' not found."));

            string query = @"
                        SELECT ID,ServiceName, ServiceCode, Price 
                        FROM Tbl_servicemaster 
                        WHERE ServiceName LIKE @Search ";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Search", $"%{q}%");

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new InvoiceDetails
                {
                    serviceId = reader["ID"]?.ToString() ?? "",
                    serviceName = reader["ServiceName"]?.ToString() ?? "",
                    saccode = reader["ServiceCode"]?.ToString() ?? "",
                    rate = reader["Price"] != DBNull.Value
                                  ? Convert.ToDecimal(reader["Price"])
                                  : 0
                });
            }

            return results;
        }

        public async Task<TaxInvoiceCreateVM?> GetProformaDetails(string ProformaId)
        {
            const string sql = @"
                SELECT [WLSPL].[FN_GenerateTaxInvoiceNo]() AS invoiceno,
                       PM.reversecharge, 'Proforma' AS AgainstBy, PM.invoiceno AS AgainstByValue,
                       PM.state,PM.companycode AS compCode, PM.companyname AS companyName, PM.address AS Address, PM.billstate,
                       PM.BillingAddress, PM.BillingLocation, PM.BillingGST,
                       PM.BillingPincode, PM.BillingStatecode
                FROM dbo.tbl_ProformaInvoiceMain PM WHERE PM.id = @id;

                SELECT CAST(ISNULL(TDSPercentage,0) AS decimal(9,2))
                FROM dbo.tbl_ProformaInvoiceMain WHERE id = @id;

                SELECT PD.productdescription,  
                       STUFF((
                           SELECT ',' + CAST(id AS VARCHAR)
                           FROM dbo.tbl_ProformaInvoiceBankDetails
                           WHERE InvoiceMainId = @id AND ISNULL(IsDeleted,0) = 0
                           FOR XML PATH('')
                       ), 1, 1, '')     AS proformadetailsId,
                       PD.ServiceName AS serviceName, PD.ServiceId AS serviceId,
                       PD.saccode AS saccode, PD.ValidateTill AS serviceTill,PD.rate, PD.taxablevalue
                FROM dbo.tbl_ProformaInvoiceDetails PD WHERE PD.invoiceid = @id ORDER BY PD.id;

                SELECT id as proformadetailsId,[mode], ChequeNo, CreatedDate, Amount
                FROM dbo.tbl_ProformaInvoiceBankDetails
                WHERE InvoiceMainId = @id AND ISNULL(IsDeleted,0) = 0 ORDER BY id;";

            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));
            using var multi = await connection.QueryMultipleAsync(sql, new { id = ProformaId });

            var main = await multi.ReadFirstOrDefaultAsync<TaxInvoiceCreate>();
            if (main == null) return null;

            decimal tdsPct = await multi.ReadFirstAsync<decimal>();
            var details = (await multi.ReadAsync<InvoiceDetails>()).ToList();
            var payments = (await multi.ReadAsync<ProformaPaymentRow>()).ToList();

            // ── 1. Received amount ──
            decimal received = payments.Sum(p => p.Amount);

            // ── 2. GST % from customer GSTIN ──
            string gstin = (main.BillingGST ?? main.gstIn ?? "").Trim();
            decimal gstPct = gstin.Length < 2 ? 0m : 18m;   // 27 → 9+9, else 18 IGST (same total)

            // ── 3. Back-calculate basic ──
            decimal divisor = 1m + gstPct / 100m - tdsPct / 100m;
            decimal basic = divisor > 0 ? Math.Round(received / divisor, 2) : 0m;

            // ── 4. Distribute equally across services ──
            if (details.Count > 0)
            {
                decimal equalShare = Math.Round(basic / details.Count, 2);
                decimal allocated = 0m;

                for (int i = 0; i < details.Count; i++)
                {
                    decimal share = (i == details.Count - 1)
                        ? basic - allocated      // remainder goes to the last row
                        : equalShare;

                    allocated += share;
                    details[i].rate = share;
                    details[i].taxablevalue = share;
                }
            }

            // ── 5. Transaction info ──
            main.TransAmt = received.ToString("0.00");
            main.TransMode = string.Join(", ", payments.Select(p => p.mode).Distinct());
            main.TransNo = string.Join(", ", payments.Where(p => !string.IsNullOrEmpty(p.ChequeNo)).Select(p => p.ChequeNo));
            main.TransDate = payments.Max(p => (DateTime?)p.CreatedDate);
            main.TdsPer = tdsPct.ToString("0.##");

            return new TaxInvoiceCreateVM { main = main, details = details };
        }

        public class ProformaPaymentRow
        {
            public string? mode { get; set; }
            public string? ChequeNo { get; set; }
            public DateTime? CreatedDate { get; set; }
            public decimal Amount { get; set; }
        }
    }
}


