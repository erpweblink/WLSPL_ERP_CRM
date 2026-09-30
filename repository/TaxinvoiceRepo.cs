using Dapper;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Data;
using WEBLINK_CRM.Helpers;
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

        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetFinancialYearSummary(string financialYear, string? salesManager, string empCode, string empRole)
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

            using (SqlConnection con = new SqlConnection(connectionString))
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
                            AND (@SalesManager IS NULL OR sessionname  = @SalesManager)
                            AND (
                                        @CurrentRole = 'Admin'
                                        OR sessionname  = @CurrentUser
                                        OR sessionname  IN (SELECT empcode FROM [dbo].[employees]
                                                       WHERE TL_Manager = @CurrentUser AND status = '1' AND isdeleted = '0')
                                    )

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
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@SalesManager", string.IsNullOrWhiteSpace(salesManager) ? DBNull.Value : salesManager);
                    cmd.Parameters.AddWithValue("@CurrentUser", empCode);
                    cmd.Parameters.AddWithValue("@CurrentRole", empRole);

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

        public async Task<dynamic> GetSalesPersonList(string empCode, string empRole)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT empcode as UserCode, name as FullName FROM [dbo].[employees] 
                                     WHERE status = '1' AND isdeleted = '0' 
                                     AND (@CurrentRole = 'Admin' OR empcode = @CurrentUser OR TL_Manager = @CurrentUser) 
                                     ORDER BY  CASE WHEN empcode = @CurrentUser THEN 0 ELSE 1 END, name;
         
                                     SELECT empcode as UserCode, name as FullName 
                                     FROM [dbo].[employees] 
                                     WHERE status = '1' AND isdeleted = '0' AND Sales_TL_Manager = '1'
                                     ORDER BY name;";

                    var parameters = new DynamicParameters();
                    parameters.Add("@CurrentUser", empCode);
                    parameters.Add("@CurrentRole", empRole);
                    using (var multi = await connection.QueryMultipleAsync(query, parameters))
                    {
                        var salesManagers = (await multi.ReadAsync<dynamic>()).ToList();
                        var meetingWithManagers = (await multi.ReadAsync<dynamic>()).ToList();

                        return new
                        {
                            SalesManagers = salesManagers,
                            MeetingWithManagers = meetingWithManagers
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Taxinvoice.TaxInvoiceCreate>> GetInfo(string financialYear, int? month, string? salesManager, string empCode, string empRole)
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
                            Remarks,
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
                            AND (@SalesManager IS NULL OR empcode = @SalesManager)
                              AND (
                                      @CurrentRole = 'Admin'
                                      OR empcode = @CurrentUser
                                      OR empcode IN (SELECT empcode FROM [dbo].[employees]
                                                     WHERE TL_Manager = @CurrentUser AND status = '1' AND isdeleted = '0')
                                  )

                        ORDER BY invoicedate ASC;
                    ";

            var parameters = new DynamicParameters();

            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);
            parameters.Add("@SalesManager", string.IsNullOrWhiteSpace(salesManager) ? null : salesManager, DbType.String);
            parameters.Add("@CurrentUser", empCode);
            parameters.Add("@CurrentRole", empRole);

            var result = await connection.QueryAsync<Taxinvoice.TaxInvoiceCreate>(query, parameters);

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


        public async Task<Taxinvoice.TaxInvoiceCreate?> Getinvoiceno()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

                const string companySql = @"SELECT [WLSPL].[FN_GenerateTaxInvoiceNo]() AS InvoiceNo";

                var result = await connection.QueryFirstOrDefaultAsync<Taxinvoice.TaxInvoiceCreate>(companySql);

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
                parameters.Add("@newinvoiceno", dbType: DbType.String, size: 900, direction: ParameterDirection.Output);

                int rowsAffected = await connection.ExecuteAsync("[dbo].[SP_AddInvoice]", parameters, commandType: CommandType.StoredProcedure);

                int myInvoice = parameters.Get<int>("@myinvoice");

                if (Action.Equals("insert", StringComparison.OrdinalIgnoreCase))
                {
                    string? generated = parameters.Get<string>("@newinvoiceno");
                    if (!string.IsNullOrWhiteSpace(generated))
                        model.main.invoiceno = generated;   // the number that was really saved
                }

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

                        await connection.ExecuteAsync(invoicedetailsSql, parametersd);

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
                WHERE TaxInvoiceID IS NULL AND InvoiceMainId = @id AND ISNULL(IsDeleted,0) = 0 ORDER BY id;";

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



        // PDF Methods


        public TaxInvoicePdfResult GenerateInvoicePdf(int invoiceId)
        {
            // =========================================================
            // 2. CONNECTION STRING
            // =========================================================

            string connectionString =
                _configuration.GetConnectionString("Conn_Stringg");


            // =========================================================
            // 3. GET INVOICE HEADER
            // =========================================================

            DataTable invoiceTable = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT
                    id,
                    invoiceno,
                    invoicedate,
                    reversecharge,
                    state,
                    companyname,
                    address,
                    TransMode,
                    TransNo,
                    TransDate,
                    TransAmt,
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
                    InvoiceType,
                    AgainstBy,
                    AgainstByValue,
                    TdsPer,
                    TdsAmt,
                    TotalPayable
                FROM InvoiceMain
                WHERE id = @id";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@id",
                        SqlDbType.Int
                    ).Value = invoiceId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(invoiceTable);
                    }
                }
            }


            // =========================================================
            // 4. GET INVOICE DETAILS
            // =========================================================

            DataTable detailTable = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
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
                    total,
                    ServiceName,
                    ServiceId,
                    ValidateTill
                FROM invoicedetails
                WHERE invoiceid = @invoiceid
                ORDER BY id";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@invoiceid",
                        SqlDbType.Int
                    ).Value = invoiceId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(detailTable);
                    }
                }
            }


            // =========================================================
            // 5. MAIN INVOICE ROW
            // =========================================================

            DataRow invoice =
                invoiceTable.Rows[0];


            // =========================================================
            // 6. CREATE PDF MEMORY STREAM
            // =========================================================
            byte[] pdfBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(
                    PageSize.A4,
                    30f,
                    30f,
                    30f,
                    30f
                );


                PdfWriter writer =
                    PdfWriter.GetInstance(
                        document,
                        stream
                    );


                document.Open();


                // =====================================================
                // FONTS
                // =====================================================

                iTextSharp.text.Font boldFont =
                    FontFactory.GetFont(
                        FontFactory.HELVETICA_BOLD,
                        9,
                        BaseColor.BLACK
                    );


                iTextSharp.text.Font normalFont =
                    FontFactory.GetFont(
                        FontFactory.HELVETICA,
                        9,
                        BaseColor.BLACK
                    );


                iTextSharp.text.Font smallFont =
                    FontFactory.GetFont(
                        FontFactory.HELVETICA,
                        8,
                        BaseColor.BLACK
                    );


                // =====================================================
                // COMPANY HEADER - LOGO + COMPANY DETAILS
                // =====================================================

                float logoWidth = 190f;
                float logoHeight = 90f;

                float logoLeft = 0f;
                float logoTop = 0f;
                float logoBottom = 0f;


                float companyNameSize = 18f;
                float companyNameLeft = 130f;
                float companyNameTop = 0f;
                float companyNameBottom = 0f;


                float addressSize = 10f;
                float addressLeft = 130f;
                float addressTop = 10f;
                float addressBottom = 0f;


                float gstPanSize = 9f;
                float gstPanLeft = 130f;
                float gstPanTop = 0f;
                float gstPanBottom = 0f;


                // =====================================================
                // COMPANY HEADER TABLE
                // =====================================================

                PdfPTable companyHeader =
                    new PdfPTable(2);

                companyHeader.WidthPercentage = 100;

                companyHeader.SetWidths(
                    new float[]
                    {
                    25f,
                    75f
                    }
                );


                // =====================================================
                // LOGO CELL
                // =====================================================

                PdfPCell logoCell =
                    new PdfPCell();

                logoCell.Border =
                    Rectangle.NO_BORDER;

                logoCell.VerticalAlignment =
                    Element.ALIGN_MIDDLE;

                logoCell.HorizontalAlignment =
                    Element.ALIGN_LEFT;

                logoCell.PaddingLeft =
                    logoLeft;

                logoCell.PaddingRight =
                    0f;

                logoCell.PaddingTop =
                    logoTop;

                logoCell.PaddingBottom =
                    logoBottom;


                // =====================================================
                // LOGO
                // =====================================================

                string logoPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "assets",
                        "images",
                        "WLSPL_logo.png"
                    );


                if (System.IO.File.Exists(logoPath))
                {
                    Image logo =
                        Image.GetInstance(logoPath);

                    logo.ScaleToFit(
                        logoWidth,
                        logoHeight
                    );

                    logo.Alignment =
                        Element.ALIGN_LEFT;

                    logoCell.AddElement(
                        logo
                    );
                }


                // =====================================================
                // INVOICE NO
                // =====================================================

                Paragraph invoiceNoParagraph =
                    new Paragraph(
                        "Invoice No : " +
                        GetValue(invoice, "invoiceno"),
                        boldFont
                    );

                invoiceNoParagraph.Alignment =
                    Element.ALIGN_LEFT;

                invoiceNoParagraph.SpacingBefore = 10f;
                invoiceNoParagraph.SpacingAfter = 2f;

                logoCell.AddElement(
                    invoiceNoParagraph
                );


                // =====================================================
                // INVOICE DATE
                // =====================================================

                Paragraph invoiceDateParagraph =
                    new Paragraph(
                        "Invoice Date : " +
                        FormatDate(invoice["invoicedate"]),
                        boldFont
                    );

                invoiceDateParagraph.Alignment =
                    Element.ALIGN_LEFT;

                invoiceDateParagraph.SpacingBefore = 0f;
                invoiceDateParagraph.SpacingAfter = 0f;

                logoCell.AddElement(
                    invoiceDateParagraph
                );


                // =====================================================
                // COMPANY DETAILS CELL
                // =====================================================

                PdfPCell companyCell =
                    new PdfPCell();

                companyCell.Border =
                    Rectangle.NO_BORDER;

                companyCell.VerticalAlignment =
                    Element.ALIGN_MIDDLE;

                companyCell.HorizontalAlignment =
                    Element.ALIGN_LEFT;


                // =====================================================
                // COMPANY NAME FONT - BOOKOSB.TTF
                // =====================================================




                string fontPath2 = Path.Combine(
                   Directory.GetCurrentDirectory(),
                   "wwwroot",
                   "Fonts",
                   "BOOKOSB.TTF"
               );

                BaseFont bookBold2 = BaseFont.CreateFont(
                    fontPath2,
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED
                );
                iTextSharp.text.Font companyFont =
                                      new iTextSharp.text.Font(
                                          bookBold2,
                                          companyNameSize,
                                          iTextSharp.text.Font.NORMAL,
                                          BaseColor.BLACK
                                      );


                // =====================================================
                // COMPANY NAME
                // =====================================================

                Paragraph company =
                    new Paragraph(
                        "Web Link Services Pvt. Ltd.",
                        companyFont
                    );

                company.Alignment =
                    Element.ALIGN_LEFT;

                company.IndentationLeft =
                    companyNameLeft;

                company.SpacingBefore =
                    companyNameTop;

                company.SpacingAfter =
                    companyNameBottom;

                companyCell.AddElement(
                    company
                );


                // =====================================================
                // ADDRESS LINE 1
                // =====================================================

                iTextSharp.text.Font addressFont =
                    FontFactory.GetFont(
                        FontFactory.HELVETICA,
                        addressSize,
                        BaseColor.BLACK
                    );


                Paragraph address =
                    new Paragraph(
                        "12th Floor, Vintage 21 Commercial Complex, Above Max",
                        addressFont
                    );

                address.Alignment =
                    Element.ALIGN_LEFT;

                address.IndentationLeft =
                    addressLeft;

                address.SpacingBefore =
                    addressTop;

                address.SpacingAfter =
                    addressBottom;

                companyCell.AddElement(
                    address
                );


                // =====================================================
                // ADDRESS LINE 2
                // =====================================================

                Paragraph address2 =
                    new Paragraph(
                        "Showroom, P.K. Chowk, Pimple Saudagar, Pune - 411027, Maharashtra, India",
                        addressFont
                    );

                address2.Alignment =
                    Element.ALIGN_LEFT;

                address2.IndentationLeft =
                    addressLeft;

                address2.SpacingBefore =
                    0f;

                address2.SpacingAfter =
                    addressBottom;

                companyCell.AddElement(
                    address2
                );


                // =====================================================
                // GST / PAN
                // =====================================================

                iTextSharp.text.Font gstPanFont =
                    FontFactory.GetFont(
                        FontFactory.HELVETICA,
                        gstPanSize,
                        BaseColor.BLACK
                    );


                Paragraph contact =
                    new Paragraph(
                        "GSTIN: 27AABCW8929J2ZP | PAN: AABCW8929J",
                        gstPanFont
                    );

                contact.Alignment =
                    Element.ALIGN_LEFT;

                contact.IndentationLeft =
                    gstPanLeft;

                contact.SpacingBefore =
                    gstPanTop;

                contact.SpacingAfter =
                    gstPanBottom;

                companyCell.AddElement(
                    contact
                );


                // =====================================================
                // ADD CELLS
                // =====================================================

                companyHeader.AddCell(
                    logoCell
                );

                companyHeader.AddCell(
                    companyCell
                );


                document.Add(
                    companyHeader
                );


                document.Add(
                    new Paragraph(" ")
                );



                iTextSharp.text.Font titleFont =
                    new iTextSharp.text.Font(
                        bookBold2,
                        23f,
                        iTextSharp.text.Font.NORMAL,
                        BaseColor.BLACK
                    );


                // =====================================================
                // TAX INVOICE TITLE
                // =====================================================

                Paragraph title =
                    new Paragraph(
                        "TAX INVOICE",
                        titleFont
                    );

                title.Alignment = Element.ALIGN_CENTER;

                document.Add(title);

                document.Add(
                    new Paragraph(" ")
                );

                // =====================================================
                // INVOICE INFORMATION
                // =====================================================

                PdfPTable invoiceInfo =
                    new PdfPTable(2);

                invoiceInfo.WidthPercentage = 100;

                invoiceInfo.SetWidths(
                    new float[]
                    {
                    50f,
                    50f
                    }
                );


                document.Add(invoiceInfo);


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // E-INVOICE INFORMATION
                // =====================================================

                string ackNo =
                    GetValue(invoice, "AckNo");

                string ackDate =
                    FormatDate(invoice["AckDt"]);

                string irn =
                    GetValue(invoice, "Irn");


                if (!string.IsNullOrWhiteSpace(ackNo) ||
                    !string.IsNullOrWhiteSpace(irn))
                {
                    PdfPTable einvoiceTable =
                        new PdfPTable(2);

                    einvoiceTable.WidthPercentage = 100;

                    einvoiceTable.SetWidths(
                        new float[]
                        {
                        50f,
                        50f
                        }
                    );


                    AddCell(
                        einvoiceTable,
                        "Ack No: " + ackNo,
                        normalFont
                    );


                    AddCell(
                        einvoiceTable,
                        "Ack Date: " + ackDate,
                        normalFont
                    );


                    AddCell(
                        einvoiceTable,
                        "IRN:",
                        boldFont
                    );


                    AddCell(
                        einvoiceTable,
                        irn,
                        smallFont
                    );


                    document.Add(einvoiceTable);


                    document.Add(
                        new Paragraph(" ")
                    );
                }


                // =====================================================
                // BILL TO / BILLING ADDRESS
                // =====================================================

                PdfPTable customerTable =
                    new PdfPTable(2);

                customerTable.WidthPercentage = 100f;

                customerTable.SetWidths(
                    new float[]
                    {
                    50f,
                    50f
                    }
                );


                // =====================================================
                // TOP HORIZONTAL LINE
                // =====================================================

                PdfPCell topLineCell =
                    new PdfPCell();

                topLineCell.Colspan = 2;

                topLineCell.Border =
                    Rectangle.TOP_BORDER;

                topLineCell.BorderWidthTop = 0.8f;

                topLineCell.PaddingTop = 0f;
                topLineCell.PaddingBottom = 0f;
                topLineCell.FixedHeight = 1f;

                customerTable.AddCell(
                    topLineCell
                );


                // =====================================================
                // BILL TO
                // =====================================================

                PdfPCell billCell =
                    new PdfPCell();

                billCell.Border =
                    Rectangle.NO_BORDER;

                billCell.Padding = 8f;


                billCell.AddElement(
                    new Paragraph(
                        "Bill To,",
                        boldFont
                    )
                );


                billCell.AddElement(
                    new Paragraph(
                        GetValue(
                            invoice,
                            "companyname"
                        ),
                        boldFont
                    )
                );


                billCell.AddElement(
                    new Paragraph(
                        "GSTIN: " +
                        GetValue(
                            invoice,
                            "BillingGST"
                        ),
                        normalFont
                    )
                );


                billCell.AddElement(
                    new Paragraph(
                        "Place of Supply/State Code: " +
                        GetValue(
                            invoice,
                            "BillingStatecode"
                        ),
                        normalFont
                    )
                );


                billCell.AddElement(
                    new Paragraph(
                        "Pincode: " +
                        GetValue(
                            invoice,
                            "BillingPincode"
                        ),
                        normalFont
                    )
                );


                // =====================================================
                // BILLING ADDRESS
                // =====================================================

                PdfPCell shipCell =
                    new PdfPCell();

                shipCell.Border =
                    Rectangle.NO_BORDER;

                shipCell.Padding = 8f;


                shipCell.AddElement(
                    new Paragraph(
                        "Billing Address,",
                        boldFont
                    )
                );


                shipCell.AddElement(
                    new Paragraph(
                        GetValue(
                            invoice,
                            "address"
                        ),
                        normalFont
                    )
                );


                shipCell.AddElement(
                    new Paragraph(
                        GetValue(
                            invoice,
                            "billstate"
                        ),
                        normalFont
                    )
                );


                customerTable.AddCell(
                    billCell
                );

                customerTable.AddCell(
                    shipCell
                );


                document.Add(
                    customerTable
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // CHECK WHETHER IGST OR CGST + SGST
                // =====================================================

                bool isIGST = false;


                foreach (DataRow detail in detailTable.Rows)
                {
                    decimal igstRate =
                        GetDecimal(
                            detail,
                            "igstrate"
                        );


                    if (igstRate > 0)
                    {
                        isIGST = true;
                        break;
                    }
                }


                // =====================================================
                // ITEM TABLE
                // =====================================================

                PdfPTable itemTable;


                if (isIGST)
                {
                    itemTable =
                        CreateIGSTItemTable(
                            detailTable,
                            boldFont,
                            normalFont
                        );
                }
                else
                {
                    itemTable =
                        CreateCGSTSGSTItemTable(
                            detailTable,
                            boldFont,
                            normalFont
                        );
                }


                document.Add(
                    itemTable
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // TAX SUMMARY
                // =====================================================

                decimal taxableValue =
                    GetDecimal(
                        invoice,
                        "taxablevalue"
                    );


                decimal cgstAmount =
                    GetDecimal(
                        invoice,
                        "cgstamt"
                    );


                decimal sgstAmount =
                    GetDecimal(
                        invoice,
                        "sgstamt"
                    );


                decimal igstAmount =
                    GetDecimal(
                        invoice,
                        "igstamt"
                    );


                decimal beforeTax =
                    GetDecimal(
                        invoice,
                        "totalamtbeforetax"
                    );


                decimal afterTax =
                    GetDecimal(
                        invoice,
                        "totalamtaftertax"
                    );


                decimal tdsAmount =
                    GetDecimal(
                        invoice,
                        "TdsAmt"
                    );


                decimal totalPayable =
                    GetDecimal(
                        invoice,
                        "TotalPayable"
                    );


                PdfPTable taxTable =
                    new PdfPTable(2);

                taxTable.WidthPercentage = 45;

                taxTable.HorizontalAlignment =
                    Element.ALIGN_RIGHT;


                AddCell(
                    taxTable,
                    "Subtotal",
                    boldFont
                );


                AddCell(
                    taxTable,
                    taxableValue.ToString("N2"),
                    normalFont,
                    Element.ALIGN_RIGHT
                );


                if (beforeTax > 0 &&
                    beforeTax != taxableValue)
                {
                    AddCell(
                        taxTable,
                        "Total Before Tax",
                        normalFont
                    );


                    AddCell(
                        taxTable,
                        beforeTax.ToString("N2"),
                        normalFont,
                        Element.ALIGN_RIGHT
                    );
                }


                if (cgstAmount > 0)
                {
                    decimal cgstRate =
                        GetDecimal(
                            invoice,
                            "cgst"
                        );


                    AddCell(
                        taxTable,
                        "CGST @ " +
                        cgstRate.ToString("0.##") +
                        "%",
                        normalFont
                    );


                    AddCell(
                        taxTable,
                        cgstAmount.ToString("N2"),
                        normalFont,
                        Element.ALIGN_RIGHT
                    );
                }


                if (sgstAmount > 0)
                {
                    decimal sgstRate =
                        GetDecimal(
                            invoice,
                            "sgst"
                        );


                    AddCell(
                        taxTable,
                        "SGST @ " +
                        sgstRate.ToString("0.##") +
                        "%",
                        normalFont
                    );


                    AddCell(
                        taxTable,
                        sgstAmount.ToString("N2"),
                        normalFont,
                        Element.ALIGN_RIGHT
                    );
                }


                if (igstAmount > 0)
                {
                    decimal igstRate =
                        GetDecimal(
                            invoice,
                            "igst"
                        );


                    AddCell(
                        taxTable,
                        "IGST @ " +
                        igstRate.ToString("0.##") +
                        "%",
                        normalFont
                    );


                    AddCell(
                        taxTable,
                        igstAmount.ToString("N2"),
                        normalFont,
                        Element.ALIGN_RIGHT
                    );
                }


                AddCell(
                    taxTable,
                    "Grand Total",
                    boldFont
                );


                AddCell(
                    taxTable,
                    afterTax.ToString("N2"),
                    boldFont,
                    Element.ALIGN_RIGHT
                );


                if (tdsAmount > 0)
                {
                    decimal tdsPer =
                        GetDecimal(
                            invoice,
                            "TdsPer"
                        );


                    AddCell(
                        taxTable,
                        "TDS @ " +
                        tdsPer.ToString("0.##") +
                        "%",
                        normalFont
                    );


                    AddCell(
                        taxTable,
                        tdsAmount.ToString("N2"),
                        normalFont,
                        Element.ALIGN_RIGHT
                    );


                    AddCell(
                        taxTable,
                        "Total Payable",
                        boldFont
                    );


                    AddCell(
                        taxTable,
                        totalPayable.ToString("N2"),
                        boldFont,
                        Element.ALIGN_RIGHT
                    );
                }


                document.Add(
                    taxTable
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // AMOUNT IN WORDS
                // =====================================================

                PdfPTable amountTable =
                    new PdfPTable(1);

                amountTable.WidthPercentage = 100;


                AddCell(
                    amountTable,
                    "Amount in Words: " +
                    GetValue(
                        invoice,
                        "amtinwords"
                    ),
                    boldFont
                );


                document.Add(
                    amountTable
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // BANK DETAILS
                // =====================================================

                Paragraph bankTitle =
                    new Paragraph(
                        "Bank Details :",
                        boldFont
                    );

                bankTitle.SpacingBefore = 0f;
                bankTitle.SpacingAfter = 5f;

                document.Add(bankTitle);


                document.Add(
                    new Paragraph(
                        "Bank A/C :- 916020085136854",
                        smallFont
                    )
                );


                document.Add(
                    new Paragraph(
                        "Bank IFSC :- UTIB0001641",
                        smallFont
                    )
                );


                document.Add(
                    new Paragraph(
                        "Axis Bank Ltd - Rahatani Branch, Pune",
                        smallFont
                    )
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // GST DECLARATION / REMARK
                // =====================================================

                string remarks =
                    GetValue(
                        invoice,
                        "Remarks"
                    );

                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    Paragraph remarkParagraph = new Paragraph();

                    remarkParagraph.Add(
                        new Chunk(
                            "Remark : ",
                            boldFont
                        )
                    );

                    remarkParagraph.Add(
                        new Chunk(
                            remarks,
                            smallFont
                        )
                    );

                    document.Add(
                        remarkParagraph
                    );
                }

                document.Add(
                    new Paragraph(" ")
                );
                // =====================================================
                // SIGNATURE TABLE
                // =====================================================

                PdfPTable signatureTable =
                    new PdfPTable(2);

                signatureTable.WidthPercentage = 100f;

                signatureTable.SetWidths(
                    new float[]
                    {
                    55f,
                    45f
                    }
                );


                // =====================================================
                // LEFT EMPTY CELL
                // =====================================================

                PdfPCell emptyCell =
                    new PdfPCell();

                emptyCell.Border =
                    Rectangle.NO_BORDER;


                // =====================================================
                // SIGNATURE CELL
                // =====================================================

                PdfPCell signatureCell =
                    new PdfPCell();

                signatureCell.Border =
                    Rectangle.NO_BORDER;

                signatureCell.HorizontalAlignment =
                    Element.ALIGN_CENTER;

                signatureCell.VerticalAlignment =
                    Element.ALIGN_MIDDLE;

                signatureCell.PaddingTop = 5f;
                signatureCell.PaddingBottom = 5f;
                signatureCell.PaddingLeft = 0f;
                signatureCell.PaddingRight = 0f;



                // =====================================================
                // COMPANY NAME
                // =====================================================

                Paragraph companyName =
                    new Paragraph(
                        "For Web Link Services Pvt. Ltd.",
                        boldFont
                    );

                companyName.Alignment =
                    Element.ALIGN_CENTER;

                companyName.SpacingBefore = 0f;
                companyName.SpacingAfter = 3f;

                signatureCell.AddElement(
                    companyName
                );


                // =====================================================
                // LOGO / STAMP
                // =====================================================

                string stampPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "assets",
                        "images",
                        "WLSPL_Stamp.png"
                    );


                if (System.IO.File.Exists(stampPath))
                {
                    Image stamp =
                        Image.GetInstance(stampPath);

                    stamp.ScaleToFit(
                        100f,
                        70f
                    );

                    stamp.Alignment =
                        Element.ALIGN_CENTER;

                    stamp.SpacingBefore = 0f;
                    stamp.SpacingAfter = 3f;

                    signatureCell.AddElement(
                        stamp
                    );
                }
                else
                {
                    throw new Exception(
                        "STAMP NOT FOUND: " + stampPath
                    );
                }


                // =====================================================
                // AUTHORIZED SIGNATORY
                // =====================================================

                Paragraph authorizedSignatory =
                    new Paragraph(
                        "Authorized Signatory",
                        boldFont
                    );

                authorizedSignatory.Alignment =
                    Element.ALIGN_CENTER;

                authorizedSignatory.SpacingBefore = 2f;
                authorizedSignatory.SpacingAfter = 0f;

                signatureCell.AddElement(
                    authorizedSignatory
                );


                // =====================================================
                // ADD CELLS
                // =====================================================

                signatureTable.AddCell(
                    emptyCell
                );

                signatureTable.AddCell(
                    signatureCell
                );


                // =====================================================
                // ADD TO DOCUMENT
                // =====================================================

                document.Add(
                    signatureTable
                );


                document.Add(
                    new Paragraph(" ")
                );


                // =====================================================
                // GENERATED BY / PRINT DATE
                // =====================================================

                PdfPTable footerTable =
                    new PdfPTable(1);

                footerTable.WidthPercentage = 100;


                AddCell(
                    footerTable,
                    "Print Date : " +
                    DateTime.Now.ToString(
                        "dd/MM/yyyy HH:mm:ss"
                    ),
                    smallFont
                );


                document.Add(
                    footerTable
                );


                // =====================================================
                // CANCELLED INVOICE
                // =====================================================

                string cancelStatus =
                    GetValue(
                        invoice,
                        "e_invoice_cancel_status"
                    );


                if (
                    cancelStatus.Equals(
                        "1",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    cancelStatus.Equals(
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    string cancelPath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "image",
                            "CancelInvoice.png"
                        );


                    if (
                        System.IO.File.Exists(
                            cancelPath
                        )
                    )
                    {
                        Image cancelImage =
                            Image.GetInstance(
                                cancelPath
                            );


                        cancelImage.ScaleToFit(
                            160f,
                            100f
                        );


                        cancelImage.Alignment =
                            Element.ALIGN_CENTER;


                        document.Add(
                            cancelImage
                        );
                    }
                }

                // =====================================================
                // LIGHT PROFESSIONAL A4 PAGE BORDER
                // =====================================================

                PdfContentByte canvas =
                    writer.DirectContent;

                canvas.SaveState();

                // Light grey border
                canvas.SetColorStroke(
                    new BaseColor(190, 195, 200)
                );

                // Thin professional line
                canvas.SetLineWidth(0.7f);

                // Border inset from A4 edge
                float borderInset = 12f;

                canvas.Rectangle(
                    document.PageSize.Left + borderInset,
                    document.PageSize.Bottom + borderInset,
                    document.PageSize.Width - (borderInset * 2),
                    document.PageSize.Height - (borderInset * 2)
                );

                canvas.Stroke();

                canvas.RestoreState();


                // =====================================================
                // CLOSE DOCUMENT
                // =====================================================

                document.Close();


                pdfBytes = stream.ToArray();
            }

            string invoiceNo = GetValue(invoice, "invoiceno");
            if (string.IsNullOrWhiteSpace(invoiceNo)) invoiceNo = "TaxInvoice";

            return new TaxInvoicePdfResult
            {
                Status = PdfStatus.Ok,
                Bytes = pdfBytes,
                FileName = invoiceNo.Replace("/", "-") + ".pdf"
            };


        }


        private PdfPTable CreateCGSTSGSTItemTable(
            DataTable detailTable,
            iTextSharp.text.Font boldFont,
            iTextSharp.text.Font normalFont)
        {
            PdfPTable table = new PdfPTable(11);

            table.WidthPercentage = 100f;

            table.SetWidths(new float[]
            {
        5f,     // Sr
        25f,    // Description
        9f,     // HSN/SAC
        7f,     // Qty
        9f,     // Rate
        11f,    // Taxable
        6f,     // CGST %
        9f,     // CGST Amt
        6f,     // SGST %
        9f,     // SGST Amt
        11f     // Total
            });

            table.HeaderRows = 1;
            table.SplitLate = false;
            table.SplitRows = true;

            // ========================================================
            // COLORS
            // ========================================================

            BaseColor navy =
                new BaseColor(41, 59, 96);

            BaseColor navyDark =
                new BaseColor(31, 47, 78);

            BaseColor headerBorder =
                new BaseColor(255, 255, 255);

            BaseColor borderColor =
                new BaseColor(180, 186, 196);

            BaseColor alternateRow =
                new BaseColor(248, 249, 251);

            BaseColor totalBackground =
                new BaseColor(238, 241, 245);

            BaseColor grandTotalBackground =
                new BaseColor(225, 231, 240);

            // ========================================================
            // FONTS
            // ========================================================

            iTextSharp.text.Font headerFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    7f,
                    BaseColor.WHITE
                );

            iTextSharp.text.Font itemFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA,
                    7.5f,
                    BaseColor.BLACK
                );

            iTextSharp.text.Font totalFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    8f,
                    BaseColor.BLACK
                );

            iTextSharp.text.Font amountFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    8f,
                    navy
                );

            // ========================================================
            // HEADER CELL
            // ========================================================

            Func<string, PdfPCell> HeaderCell =
                delegate (string text)
                {
                    PdfPCell cell =
                        new PdfPCell(
                            new Phrase(
                                text,
                                headerFont
                            )
                        );

                    cell.BackgroundColor = navy;

                    cell.HorizontalAlignment =
                        Element.ALIGN_CENTER;

                    cell.VerticalAlignment =
                        Element.ALIGN_MIDDLE;

                    // VERY IMPORTANT
                    cell.Border = Rectangle.BOX;

                    // WHITE BORDER MAKES EVERY HEADER CELL VISIBLE
                    cell.BorderColor = headerBorder;

                    cell.BorderWidth = 0.8f;

                    cell.PaddingTop = 6f;
                    cell.PaddingBottom = 6f;
                    cell.PaddingLeft = 3f;
                    cell.PaddingRight = 3f;

                    return cell;
                };

            // ========================================================
            // HEADER
            // ========================================================

            table.AddCell(HeaderCell("Sr."));
            table.AddCell(HeaderCell("Description"));
            table.AddCell(HeaderCell("HSN/SAC"));
            table.AddCell(HeaderCell("Qty"));
            table.AddCell(HeaderCell("Rate"));
            table.AddCell(HeaderCell("Taxable Value"));
            table.AddCell(HeaderCell("CGST %"));
            table.AddCell(HeaderCell("CGST Amount"));
            table.AddCell(HeaderCell("SGST %"));
            table.AddCell(HeaderCell("SGST Amount"));
            table.AddCell(HeaderCell("Total"));

            // ========================================================
            // TOTAL VARIABLES
            // ========================================================

            decimal totalTaxable = 0m;
            decimal totalCgst = 0m;
            decimal totalSgst = 0m;
            decimal totalAmount = 0m;

            int srNo = 1;

            // ========================================================
            // DATA ROWS
            // ========================================================

            foreach (DataRow row in detailTable.Rows)
            {
                decimal qty =
                    GetDecimal(row, "qty");

                decimal rate =
                    GetDecimal(row, "rate");

                decimal taxable =
                    GetDecimal(row, "taxablevalue");

                decimal cgstRate =
                    GetDecimal(row, "cgstrate");

                decimal cgstAmt =
                    GetDecimal(row, "cgstamt");

                decimal sgstRate =
                    GetDecimal(row, "sgstrate");

                decimal sgstAmt =
                    GetDecimal(row, "sgstamt");

                decimal amount =
                    GetDecimal(row, "total");

                totalTaxable += taxable;
                totalCgst += cgstAmt;
                totalSgst += sgstAmt;
                totalAmount += amount;

                // ====================================================
                // ALTERNATE ROW
                // ====================================================

                BaseColor rowBackground =
                    (srNo % 2 == 0)
                        ? alternateRow
                        : BaseColor.WHITE;

                // ====================================================
                // DATA CELL
                // ====================================================

                Func<string, int, PdfPCell> DataCell =
                    delegate (
                        string text,
                        int alignment)
                    {
                        PdfPCell cell =
                            new PdfPCell(
                                new Phrase(
                                    text,
                                    itemFont
                                )
                            );

                        cell.BackgroundColor =
                            rowBackground;

                        cell.HorizontalAlignment =
                            alignment;

                        cell.VerticalAlignment =
                            Element.ALIGN_MIDDLE;

                        // FULL VISIBLE BORDER
                        cell.Border =
                            Rectangle.BOX;

                        cell.BorderColor =
                            borderColor;

                        cell.BorderWidth =
                            0.6f;

                        cell.PaddingTop =
                            5f;

                        cell.PaddingBottom =
                            5f;

                        cell.PaddingLeft =
                            3f;

                        cell.PaddingRight =
                            3f;

                        return cell;
                    };

                // ====================================================
                // DESCRIPTION
                // ====================================================

                string description =
                    GetValue(
                        row,
                        "productdescription"
                    );

                if (string.IsNullOrWhiteSpace(description))
                {
                    description =
                        GetValue(
                            row,
                            "ServiceName"
                        );
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "-";
                }

                // ====================================================
                // CELLS
                // ====================================================

                table.AddCell(
                    DataCell(
                        srNo.ToString(),
                        Element.ALIGN_CENTER
                    )
                );

                table.AddCell(
                    DataCell(
                        description,
                        Element.ALIGN_LEFT
                    )
                );

                table.AddCell(
                    DataCell(
                        GetValue(row, "saccode"),
                        Element.ALIGN_CENTER
                    )
                );

                table.AddCell(
                    DataCell(
                        qty.ToString("0.##"),
                        Element.ALIGN_CENTER
                    )
                );

                table.AddCell(
                    DataCell(
                        rate.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                table.AddCell(
                    DataCell(
                        taxable.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                table.AddCell(
                    DataCell(
                        cgstRate.ToString("0.##") + "%",
                        Element.ALIGN_CENTER
                    )
                );

                table.AddCell(
                    DataCell(
                        cgstAmt.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                table.AddCell(
                    DataCell(
                        sgstRate.ToString("0.##") + "%",
                        Element.ALIGN_CENTER
                    )
                );

                table.AddCell(
                    DataCell(
                        sgstAmt.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                // ====================================================
                // TOTAL CELL
                // ====================================================

                PdfPCell amountCell =
                    new PdfPCell(
                        new Phrase(
                            amount.ToString("N2"),
                            amountFont
                        )
                    );

                amountCell.BackgroundColor =
                    rowBackground;

                amountCell.HorizontalAlignment =
                    Element.ALIGN_RIGHT;

                amountCell.VerticalAlignment =
                    Element.ALIGN_MIDDLE;

                amountCell.Border =
                    Rectangle.BOX;

                amountCell.BorderColor =
                    borderColor;

                amountCell.BorderWidth =
                    0.6f;

                amountCell.PaddingTop =
                    5f;

                amountCell.PaddingBottom =
                    5f;

                amountCell.PaddingLeft =
                    3f;

                amountCell.PaddingRight =
                    3f;

                table.AddCell(
                    amountCell
                );

                srNo++;
            }

            // ========================================================
            // TOTAL ROW
            // ========================================================

            PdfPCell totalLabel =
                new PdfPCell(
                    new Phrase(
                        "TOTAL",
                        totalFont
                    )
                );

            totalLabel.Colspan = 5;

            totalLabel.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            totalLabel.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            totalLabel.BackgroundColor =
                totalBackground;

            totalLabel.Border =
                Rectangle.BOX;

            totalLabel.BorderColor =
                borderColor;

            totalLabel.BorderWidth =
                0.8f;

            totalLabel.PaddingTop =
                6f;

            totalLabel.PaddingBottom =
                6f;

            totalLabel.PaddingRight =
                6f;

            table.AddCell(
                totalLabel
            );

            // ========================================================
            // TAXABLE TOTAL
            // ========================================================

            PdfPCell taxableCell =
                new PdfPCell(
                    new Phrase(
                        totalTaxable.ToString("N2"),
                        totalFont
                    )
                );

            taxableCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            taxableCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            taxableCell.BackgroundColor =
                totalBackground;

            taxableCell.Border =
                Rectangle.BOX;

            taxableCell.BorderColor =
                borderColor;

            taxableCell.BorderWidth =
                0.8f;

            taxableCell.PaddingTop =
                6f;

            taxableCell.PaddingBottom =
                6f;

            taxableCell.PaddingLeft =
                3f;

            taxableCell.PaddingRight =
                3f;

            table.AddCell(
                taxableCell
            );

            // ========================================================
            // CGST % BLANK
            // ========================================================

            PdfPCell cgstRateBlank =
                new PdfPCell(
                    new Phrase("", totalFont)
                );

            cgstRateBlank.BackgroundColor =
                totalBackground;

            cgstRateBlank.Border =
                Rectangle.BOX;

            cgstRateBlank.BorderColor =
                borderColor;

            cgstRateBlank.BorderWidth =
                0.8f;

            table.AddCell(
                cgstRateBlank
            );

            // ========================================================
            // CGST TOTAL
            // ========================================================

            PdfPCell cgstCell =
                new PdfPCell(
                    new Phrase(
                        totalCgst.ToString("N2"),
                        totalFont
                    )
                );

            cgstCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            cgstCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            cgstCell.BackgroundColor =
                totalBackground;

            cgstCell.Border =
                Rectangle.BOX;

            cgstCell.BorderColor =
                borderColor;

            cgstCell.BorderWidth =
                0.8f;

            cgstCell.PaddingTop =
                6f;

            cgstCell.PaddingBottom =
                6f;

            cgstCell.PaddingLeft =
                3f;

            cgstCell.PaddingRight =
                3f;

            table.AddCell(
                cgstCell
            );

            // ========================================================
            // SGST % BLANK
            // ========================================================

            PdfPCell sgstRateBlank =
                new PdfPCell(
                    new Phrase("", totalFont)
                );

            sgstRateBlank.BackgroundColor =
                totalBackground;

            sgstRateBlank.Border =
                Rectangle.BOX;

            sgstRateBlank.BorderColor =
                borderColor;

            sgstRateBlank.BorderWidth =
                0.8f;

            table.AddCell(
                sgstRateBlank
            );

            // ========================================================
            // SGST TOTAL
            // ========================================================

            PdfPCell sgstCell =
                new PdfPCell(
                    new Phrase(
                        totalSgst.ToString("N2"),
                        totalFont
                    )
                );

            sgstCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            sgstCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            sgstCell.BackgroundColor =
                totalBackground;

            sgstCell.Border =
                Rectangle.BOX;

            sgstCell.BorderColor =
                borderColor;

            sgstCell.BorderWidth =
                0.8f;

            sgstCell.PaddingTop =
                6f;

            sgstCell.PaddingBottom =
                6f;

            sgstCell.PaddingLeft =
                3f;

            sgstCell.PaddingRight =
                3f;

            table.AddCell(
                sgstCell
            );

            // ========================================================
            // GRAND TOTAL
            // ========================================================

            PdfPCell finalAmountCell =
                new PdfPCell(
                    new Phrase(
                        totalAmount.ToString("N2"),
                        amountFont
                    )
                );

            finalAmountCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            finalAmountCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            finalAmountCell.BackgroundColor =
                grandTotalBackground;

            finalAmountCell.Border =
                Rectangle.BOX;

            finalAmountCell.BorderColor =
                navyDark;

            finalAmountCell.BorderWidth =
                1.2f;

            finalAmountCell.PaddingTop =
                6f;

            finalAmountCell.PaddingBottom =
                6f;

            finalAmountCell.PaddingLeft =
                3f;

            finalAmountCell.PaddingRight =
                3f;

            table.AddCell(
                finalAmountCell
            );

            // ========================================================
            // TABLE OUTER BORDER
            // ========================================================

            table.DefaultCell.Border =
                Rectangle.BOX;

            table.DefaultCell.BorderColor =
                borderColor;

            table.DefaultCell.BorderWidth =
                0.6f;

            return table;
        }

        // ============================================================
        // PROFESSIONAL IGST ITEM TABLE
        // ============================================================
        private PdfPTable CreateIGSTItemTable(
            DataTable detailTable,
            iTextSharp.text.Font boldFont,
            iTextSharp.text.Font normalFont)
        {
            PdfPTable table =
                new PdfPTable(9);

            table.WidthPercentage =
                100f;

            table.SetWidths(new float[]
            {
        5f,     // Sr
        31f,    // Description
        10f,    // HSN/SAC
        8f,     // Qty
        10f,    // Rate
        12f,    // Taxable
        7f,     // IGST %
        9f,     // IGST Amount
        11f     // Total
            });

            table.HeaderRows = 1;
            table.SplitLate = false;
            table.SplitRows = true;

            // ========================================================
            // COLORS
            // ========================================================

            BaseColor navy =
                new BaseColor(41, 59, 96);

            BaseColor lightHeader =
                new BaseColor(235, 239, 245);

            BaseColor borderColor =
                new BaseColor(205, 210, 218);

            BaseColor alternateRow =
                new BaseColor(249, 250, 252);

            BaseColor totalBackground =
                new BaseColor(242, 244, 247);

            // ========================================================
            // FONTS
            // ========================================================

            iTextSharp.text.Font headerFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    7f,
                    BaseColor.WHITE
                );

            iTextSharp.text.Font itemFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA,
                    7.5f,
                    BaseColor.BLACK
                );

            iTextSharp.text.Font totalFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    8f,
                    BaseColor.BLACK
                );

            iTextSharp.text.Font amountFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    8f,
                    navy
                );

            // ========================================================
            // HEADER CELL
            // ========================================================
            Func<string, PdfPCell> HeaderCell =
      delegate (string text)
      {
          PdfPCell cell =
              new PdfPCell(
                  new Phrase(
                      text,
                      headerFont
                  )
              );

          cell.BackgroundColor =
              navy;

          cell.HorizontalAlignment =
              Element.ALIGN_CENTER;

          cell.VerticalAlignment =
              Element.ALIGN_MIDDLE;

          // IMPORTANT
          cell.Border =
              Rectangle.BOX;

          // Visible against navy
          cell.BorderColor =
              BaseColor.WHITE;

          cell.BorderWidth =
              0.8f;

          cell.PaddingTop =
              6f;

          cell.PaddingBottom =
              6f;

          cell.PaddingLeft =
              3f;

          cell.PaddingRight =
              3f;

          return cell;
      };

            // ========================================================
            // HEADER
            // ========================================================

            table.AddCell(
                HeaderCell("Sr.")
            );

            table.AddCell(
                HeaderCell("Description")
            );

            table.AddCell(
                HeaderCell("HSN/SAC")
            );

            table.AddCell(
                HeaderCell("Qty")
            );

            table.AddCell(
                HeaderCell("Rate")
            );

            table.AddCell(
                HeaderCell("Taxable Value")
            );

            table.AddCell(
                HeaderCell("IGST %")
            );

            table.AddCell(
                HeaderCell("IGST Amount")
            );

            table.AddCell(
                HeaderCell("Total")
            );

            // ========================================================
            // TOTAL VARIABLES
            // ========================================================

            decimal totalTaxable = 0m;

            decimal totalIgst = 0m;

            decimal totalAmount = 0m;

            int srNo = 1;

            // ========================================================
            // DATA ROWS
            // ========================================================

            foreach (DataRow row in detailTable.Rows)
            {
                decimal qty =
                    GetDecimal(
                        row,
                        "qty"
                    );

                decimal rate =
                    GetDecimal(
                        row,
                        "rate"
                    );

                decimal taxable =
                    GetDecimal(
                        row,
                        "taxablevalue"
                    );

                decimal igstRate =
                    GetDecimal(
                        row,
                        "igstrate"
                    );

                decimal igstAmt =
                    GetDecimal(
                        row,
                        "igstamt"
                    );

                decimal amount =
                    GetDecimal(
                        row,
                        "total"
                    );

                totalTaxable += taxable;

                totalIgst += igstAmt;

                totalAmount += amount;

                // ====================================================
                // ALTERNATE ROW BACKGROUND
                // ====================================================

                BaseColor rowBackground =
                    (srNo % 2 == 0)
                        ? alternateRow
                        : BaseColor.WHITE;

                // ====================================================
                // DATA CELL FUNCTION
                // ====================================================

                Func<string, int, PdfPCell> DataCell =
                    delegate (
                        string text,
                        int alignment)
                    {
                        PdfPCell cell =
                            new PdfPCell(
                                new Phrase(
                                    text,
                                    itemFont
                                )
                            );

                        cell.BackgroundColor =
                            rowBackground;

                        cell.HorizontalAlignment =
                            alignment;

                        cell.VerticalAlignment =
                            Element.ALIGN_MIDDLE;

                        // FULL CELL BORDER
                        cell.Border =
                            Rectangle.BOX;

                        cell.BorderColor =
                            borderColor;

                        cell.BorderWidth =
                            0.5f;

                        cell.PaddingTop =
                            5f;

                        cell.PaddingBottom =
                            5f;

                        cell.PaddingLeft =
                            3f;

                        cell.PaddingRight =
                            3f;

                        return cell;
                    };

                // ====================================================
                // DESCRIPTION
                // ====================================================

                string description =
                    GetValue(
                        row,
                        "productdescription"
                    );

                if (
                    string.IsNullOrWhiteSpace(
                        description
                    )
                )
                {
                    description =
                        GetValue(
                            row,
                            "ServiceName"
                        );
                }

                if (
                    string.IsNullOrWhiteSpace(
                        description
                    )
                )
                {
                    description = "-";
                }

                // ====================================================
                // SR
                // ====================================================

                table.AddCell(
                    DataCell(
                        srNo.ToString(),
                        Element.ALIGN_CENTER
                    )
                );

                // ====================================================
                // DESCRIPTION
                // ====================================================

                table.AddCell(
                    DataCell(
                        description,
                        Element.ALIGN_LEFT
                    )
                );

                // ====================================================
                // HSN/SAC
                // ====================================================

                table.AddCell(
                    DataCell(
                        GetValue(
                            row,
                            "saccode"
                        ),
                        Element.ALIGN_CENTER
                    )
                );

                // ====================================================
                // QTY
                // ====================================================

                table.AddCell(
                    DataCell(
                        qty.ToString("0.##"),
                        Element.ALIGN_CENTER
                    )
                );

                // ====================================================
                // RATE
                // ====================================================

                table.AddCell(
                    DataCell(
                        rate.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                // ====================================================
                // TAXABLE VALUE
                // ====================================================

                table.AddCell(
                    DataCell(
                        taxable.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                // ====================================================
                // IGST %
                // ====================================================

                table.AddCell(
                    DataCell(
                        igstRate.ToString("0.##") + "%",
                        Element.ALIGN_CENTER
                    )
                );

                // ====================================================
                // IGST AMOUNT
                // ====================================================

                table.AddCell(
                    DataCell(
                        igstAmt.ToString("N2"),
                        Element.ALIGN_RIGHT
                    )
                );

                // ====================================================
                // TOTAL CELL
                // ====================================================

                PdfPCell amountCell =
                    new PdfPCell(
                        new Phrase(
                            amount.ToString("N2"),
                            amountFont
                        )
                    );

                amountCell.BackgroundColor =
                    rowBackground;

                amountCell.HorizontalAlignment =
                    Element.ALIGN_RIGHT;

                amountCell.VerticalAlignment =
                    Element.ALIGN_MIDDLE;

                // FULL CELL BORDER
                amountCell.Border =
                    Rectangle.BOX;

                amountCell.BorderColor =
                    borderColor;

                amountCell.BorderWidth =
                    0.5f;

                amountCell.PaddingTop =
                    5f;

                amountCell.PaddingBottom =
                    5f;

                amountCell.PaddingLeft =
                    3f;

                amountCell.PaddingRight =
                    3f;

                table.AddCell(
                    amountCell
                );

                srNo++;
            }

            // ========================================================
            // TOTAL LABEL
            // Sr + Description + HSN + Qty + Rate = 5 columns
            // ========================================================

            PdfPCell totalLabel =
                new PdfPCell(
                    new Phrase(
                        "TOTAL",
                        totalFont
                    )
                );

            totalLabel.Colspan = 5;

            totalLabel.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            totalLabel.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            totalLabel.BackgroundColor =
                totalBackground;

            // FULL BORDER
            totalLabel.Border =
                Rectangle.BOX;

            totalLabel.BorderColor =
                borderColor;

            totalLabel.BorderWidth =
                0.7f;

            totalLabel.PaddingTop =
                6f;

            totalLabel.PaddingBottom =
                6f;

            totalLabel.PaddingLeft =
                3f;

            totalLabel.PaddingRight =
                6f;

            table.AddCell(
                totalLabel
            );

            // ========================================================
            // TAXABLE TOTAL
            // ========================================================

            PdfPCell taxableCell =
                new PdfPCell(
                    new Phrase(
                        totalTaxable.ToString("N2"),
                        totalFont
                    )
                );

            taxableCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            taxableCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            taxableCell.BackgroundColor =
                totalBackground;

            // FULL BORDER
            taxableCell.Border =
                Rectangle.BOX;

            taxableCell.BorderColor =
                borderColor;

            taxableCell.BorderWidth =
                0.7f;

            taxableCell.PaddingTop =
                6f;

            taxableCell.PaddingBottom =
                6f;

            taxableCell.PaddingLeft =
                3f;

            taxableCell.PaddingRight =
                3f;

            table.AddCell(
                taxableCell
            );

            // ========================================================
            // IGST % BLANK CELL
            // ========================================================

            PdfPCell igstRateBlank =
                new PdfPCell(
                    new Phrase(
                        "",
                        totalFont
                    )
                );

            igstRateBlank.BackgroundColor =
                totalBackground;

            igstRateBlank.HorizontalAlignment =
                Element.ALIGN_CENTER;

            igstRateBlank.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            // FULL BORDER
            igstRateBlank.Border =
                Rectangle.BOX;

            igstRateBlank.BorderColor =
                borderColor;

            igstRateBlank.BorderWidth =
                0.7f;

            igstRateBlank.PaddingTop =
                6f;

            igstRateBlank.PaddingBottom =
                6f;

            table.AddCell(
                igstRateBlank
            );

            // ========================================================
            // IGST TOTAL
            // ========================================================

            PdfPCell igstCell =
                new PdfPCell(
                    new Phrase(
                        totalIgst.ToString("N2"),
                        totalFont
                    )
                );

            igstCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            igstCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            igstCell.BackgroundColor =
                totalBackground;

            // FULL BORDER
            igstCell.Border =
                Rectangle.BOX;

            igstCell.BorderColor =
                borderColor;

            igstCell.BorderWidth =
                0.7f;

            igstCell.PaddingTop =
                6f;

            igstCell.PaddingBottom =
                6f;

            igstCell.PaddingLeft =
                3f;

            igstCell.PaddingRight =
                3f;

            table.AddCell(
                igstCell
            );

            // ========================================================
            // GRAND TOTAL
            // ========================================================

            PdfPCell finalAmountCell =
                new PdfPCell(
                    new Phrase(
                        totalAmount.ToString("N2"),
                        amountFont
                    )
                );

            finalAmountCell.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            finalAmountCell.VerticalAlignment =
                Element.ALIGN_MIDDLE;

            finalAmountCell.BackgroundColor =
                lightHeader;

            // STRONG FULL BORDER
            finalAmountCell.Border =
                Rectangle.BOX;

            finalAmountCell.BorderColor =
                navy;

            finalAmountCell.BorderWidth =
                0.9f;

            finalAmountCell.PaddingTop =
                6f;

            finalAmountCell.PaddingBottom =
                6f;

            finalAmountCell.PaddingLeft =
                3f;

            finalAmountCell.PaddingRight =
                3f;

            table.AddCell(
                finalAmountCell
            );

            return table;
        }


        // =========================================================
        // PROFESSIONAL HEADER CELL
        // =========================================================

        private void AddHeaderCell(
            PdfPTable table,
            string text)
        {
            iTextSharp.text.Font headerFont =
                FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    7f,
                    BaseColor.WHITE
                );


            PdfPCell cell =
                new PdfPCell(
                    new Phrase(
                        text ?? "",
                        headerFont
                    )
                );


            // =====================================================
            // BACKGROUND
            // =====================================================

            cell.BackgroundColor =
                new BaseColor(
                    31,
                    53,
                    87
                );


            // =====================================================
            // ALIGNMENT
            // =====================================================

            cell.HorizontalAlignment =
                Element.ALIGN_CENTER;

            cell.VerticalAlignment =
                Element.ALIGN_MIDDLE;


            // =====================================================
            // PADDING
            // =====================================================

            cell.PaddingTop = 5f;
            cell.PaddingBottom = 5f;
            cell.PaddingLeft = 2f;
            cell.PaddingRight = 2f;


            cell.UseAscender = true;
            cell.UseDescender = true;


            // =====================================================
            // BORDER
            // =====================================================

            cell.BorderWidth = 0.5f;

            cell.BorderColor =
                new BaseColor(
                    255,
                    255,
                    255
                );


            table.AddCell(cell);
        }


        // =========================================================
        // PROFESSIONAL DATA CELL
        // =========================================================

        private void AddCell(
            PdfPTable table,
            string text,
            iTextSharp.text.Font font,
            int alignment = Element.ALIGN_LEFT)
        {
            Phrase phrase =
                new Phrase(
                    text ?? "",
                    font
                );


            phrase.SetLeading(
                0f,
                1.0f
            );


            PdfPCell cell =
                new PdfPCell(
                    phrase
                );


            cell.HorizontalAlignment =
                alignment;

            cell.VerticalAlignment =
                Element.ALIGN_MIDDLE;


            cell.PaddingTop = 4f;
            cell.PaddingBottom = 4f;
            cell.PaddingLeft = 3f;
            cell.PaddingRight = 3f;


            cell.UseAscender = true;
            cell.UseDescender = true;


            cell.NoWrap = false;


            cell.BorderWidth = 0.5f;

            cell.BorderColor =
                new BaseColor(
                    180,
                    180,
                    180
                );


            table.AddCell(cell);
        }


        // =========================================================
        // TOTAL CELL
        // =========================================================

        private void AddTotalCell(
            PdfPTable table,
            string text,
            iTextSharp.text.Font font,
            int alignment = Element.ALIGN_LEFT)
        {
            Phrase phrase =
                new Phrase(
                    text ?? "",
                    font
                );


            phrase.SetLeading(
                0f,
                1.0f
            );


            PdfPCell cell =
                new PdfPCell(
                    phrase
                );


            cell.HorizontalAlignment =
                alignment;

            cell.VerticalAlignment =
                Element.ALIGN_MIDDLE;


            cell.PaddingTop = 5f;
            cell.PaddingBottom = 5f;
            cell.PaddingLeft = 3f;
            cell.PaddingRight = 3f;


            cell.UseAscender = true;
            cell.UseDescender = true;


            cell.BackgroundColor =
                new BaseColor(
                    245,
                    247,
                    250
                );


            cell.BorderWidth = 0.6f;

            cell.BorderColor =
                new BaseColor(
                    120,
                    120,
                    120
                );


            table.AddCell(cell);
        }


        // =========================================================
        // GET VALUE
        // =========================================================

        private string GetValue(
            DataRow row,
            string column)
        {
            if (!row.Table.Columns.Contains(column))
            {
                return "";
            }


            if (row[column] == DBNull.Value)
            {
                return "";
            }


            return Convert.ToString(
                row[column]
            );
        }


        // =========================================================
        // GET DECIMAL
        // =========================================================

        private decimal GetDecimal(
            DataRow row,
            string column)
        {
            if (!row.Table.Columns.Contains(column))
            {
                return 0;
            }


            if (row[column] == DBNull.Value)
            {
                return 0;
            }


            decimal value;


            if (
                decimal.TryParse(
                    Convert.ToString(
                        row[column]
                    ),
                    out value
                )
            )
            {
                return value;
            }


            return 0;
        }


        // =========================================================
        // FORMAT DATE
        // =========================================================

        private string FormatDate(
            object value)
        {
            if (
                value == null ||
                value == DBNull.Value
            )
            {
                return "";
            }


            DateTime date;


            if (
                DateTime.TryParse(
                    Convert.ToString(value),
                    out date
                )
            )
            {
                return date.ToString(
                    "dd/MM/yyyy"
                );
            }


            return Convert.ToString(value);
        }

    }
}


