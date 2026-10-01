using Dapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Threading.Channels;
using WEBLINK_CRM.Models;
using WLSPL_ERP_CRM.Models;
using static WEBLINK_CRM.Models.VM_Proforma;
using static WLSPL_ERP_CRM.Models.ProformaInvoice;

namespace WEBLINK_CRM.repository
{
    public class RepoProforma : IProforma
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public RepoProforma(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public async Task<List<ProformaInvoiceCreate>> Getcompany()
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));


            const string companySql = @"select  cname As companyName from Company  where isdeleted = 0  and status =1 and type='paid';";

            var companies = await connection.QueryAsync<ProformaInvoiceCreate>(companySql);

            return companies.ToList();

        }

        public async Task<List<dynamic>> GetQuotationsByCompany(string cname, string type)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

            const string companySql = @"            
                  SELECT qm.id, Quotationno AS displayText 
                    FROM stswlspl.tblQuotationMain as qm
                    LEFT JOIN [WLSPLCRM].[dbo].[tbl_ProformaInvoiceMain] as pm ON pm.AgainstByValue=qm.id
                    WHERE qm.companyname = @cname  
                   AND  pm.id is null
                    ORDER BY id DESC
               ";

            var result = await connection.QueryAsync<dynamic>(companySql, new { cname, type });

            return result.ToList();
        }

        public async Task<object> GetQuotationProformaDetails(int id, string type)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg"));

            const string detailSql = @"                    
                              SELECT  serviceId, serviceName, sacCode, productdescription,rate,
                                taxablevalue,cgstrate,cgstamt,sgstrate,sgstamt,igstrate,igstamt,total
                            FROM stswlspl.tblQuotationDetails 
                            WHERE Quotationid = @id
                      ";

            var param = new { id, type };

            var details = await connection.QueryAsync<dynamic>(detailSql, param);

            return new
            {
                details = details.ToList()
            };
        }

        public async Task<ProformaInvoiceCreate> Getcompanybycname(string cname)
        {
            try
            {
                using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

                var parameters = new DynamicParameters();

                parameters.Add("@cname", cname);

                const string companySql = @"select ccode AS companyCode,gstno As gstIn,address As Address, Billing_location As Location ,Billing_pincode As PinCode, State As state,Billing_statecode As statecode   from Company  where isdeleted = 0  and status =1 and type='paid' and cname = @cname;";

                var result = await connection.QueryFirstOrDefaultAsync<ProformaInvoiceCreate>(companySql, parameters);

                return result ?? new ProformaInvoiceCreate();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<dynamic> Getinvoicebyid(int ID)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await con.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@id", ID);

            const string companySql = @"SELECT  *
  FROM [WLSPLCRM].[dbo].[tbl_ProformaInvoiceMain] WHERE id=@id

  SELECT  *,ValidateTill as ServiceTill
  FROM [WLSPLCRM].[dbo].[tbl_ProformaInvoiceDetails]  WHERE invoiceid=@id


  SELECT  *,PaymentDate as date
  FROM [WLSPLCRM].[dbo].[tbl_ProformaInvoiceBankDetails]  WHERE invoicemainid=@id
            ";

            using var multi = await con.QueryMultipleAsync(companySql, parameters);

            var main = await multi.ReadFirstOrDefaultAsync<ProformaInvoice.ProformaInvoiceCreate>();

            if (main == null)
            {
                return null;
            }

            var details = (await multi.ReadAsync<ProformaInvoice.InvoiceDetails>())
                .ToList();
            var bankdetails = (await multi.ReadAsync<ProformaInvoice.InvoiceBankDetail>())
                .ToList();

            return new ProformaInvoice.ProformaInvoiceCreateVM
            {
                main = main,
                details = details,
                BankDetails = bankdetails
            };
        }

      
        public async Task<ProformaInvoice.ProformaInvoiceCreate?> GetBlankModelWithinvoiceno()
        {
            try
            {
                using var connection = new SqlConnection(
           _configuration.GetConnectionString("Conn_Stringg"));

                const string query = @"
           SELECT [WLSPL].FN_GenerateProformaNo()";

                var result = await connection.QueryFirstOrDefaultAsync<ProformaInvoice.ProformaInvoiceCreate>(
                    query
                );

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateSave(ProformaInvoice.ProformaInvoiceCreateVM model, string Action)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (model.main == null)
                throw new Exception("Main invoice data is required.");

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg"));

            await connection.OpenAsync();

            using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                bool isUpdate = Action.Equals("updateolddata", StringComparison.OrdinalIgnoreCase);

                Dictionary<string, object?> oldValues = null;

                if (isUpdate)
                {
                    var oldRow = await connection.QueryFirstOrDefaultAsync(
                        "SELECT * FROM tbl_ProformaInvoiceMain WHERE id = @id",
                        new { id = model.main.Id },
                        transaction);

                    if (oldRow != null)
                    {
                        oldValues = new Dictionary<string, object?>(
                            (IDictionary<string, object>)oldRow,
                            StringComparer.OrdinalIgnoreCase);
                    }
                }
                // =========================================================
                // MAIN INVOICE PARAMETERS
                // =========================================================

                var parameters = new DynamicParameters();

                parameters.Add("@id", model.main.Id);

                parameters.Add(
                    "@invoiceno",
                    model.main.invoiceno);

                parameters.Add(
                    "@invoicedate",
                    model.main.invoicedate);

                parameters.Add(
                    "@reversecharge",
                    model.main.reversecharge);

                parameters.Add(
                    "@InvoiceType",
                    model.main.InvoiceType);

                parameters.Add(
                    "@AgainstBy",
                    model.main.AgainstBy);

                parameters.Add(
                    "@AgainstByValue",
                    model.main.AgainstByValue);


                // =========================================================
                // COMPANY
                // =========================================================

                parameters.Add(
                    "@companyname",
                    model.main.companyName);

                parameters.Add(
                    "@companyCode",
                    model.main.companyCode);

                parameters.Add(
                    "@cgstin",
                    model.main.gstIn);

                parameters.Add(
                    "@address",
                    model.main.Address);

                parameters.Add(
                    "@BillingLocation",
                    model.main.Location);

                parameters.Add(
                    "@BillingPincode",
                    model.main.PinCode);

                parameters.Add(
                    "@state",
                    model.main.state);

                parameters.Add(
                    "@billstate",
                    model.main.state);

                parameters.Add(
                    "@BillingStatecode",
                    model.main.statecode);


                // =========================================================
                // GST
                // =========================================================

                parameters.Add(
                    "@cgst",
                    model.main.cgst ?? 0);

                parameters.Add(
                    "@cgstamt",
                    model.main.cgstamt ?? 0);

                parameters.Add(
                    "@sgst",
                    model.main.sgst ?? 0);

                parameters.Add(
                    "@sgstamt",
                    model.main.sgstamt ?? 0);

                parameters.Add(
                    "@igst",
                    model.main.igst ?? 0);

                parameters.Add(
                    "@igstamt",
                    model.main.igstamt ?? 0);

                parameters.Add(
                    "@gstonreversecharge",
                    model.main.gstonreversecharge);


                // =========================================================
                // INVOICE TOTALS
                // =========================================================

                parameters.Add(
                    "@totalqty",
                    model.main.totalqty ?? 0);

                parameters.Add(
                    "@totalrate",
                    model.main.totalrate ?? 0);

                parameters.Add(
                    "@taxablevalue",
                    model.main.taxablevalue ?? 0);

                parameters.Add(
                    "@totalamtbeforetax",
                    model.main.totalamtbeforetax ?? 0);

                parameters.Add(
                    "@totalamtaftertax",
                    model.main.totalamtaftertax ?? 0);

                parameters.Add(
                    "@total_tax_amount",
                    model.main.total_tax_amount);

                parameters.Add(
                    "@amtinwords",
                    model.main.totalamtaftertax);


                // =========================================================
                // PAYMENT / SUMMARY TOTALS
                // =========================================================

                parameters.Add(
                    "@TotalBasicAmount",
                    model.main.TotalBasicAmount ?? 0);

                parameters.Add(
                    "@TotalTaxableAmount",
                    model.main.TotalTaxableAmount ?? 0);

                parameters.Add(
                    "@TotalCGSTAmount",
                    model.main.TotalCGSTAmount ?? 0);

                parameters.Add(
                    "@TotalSGSTAmount",
                    model.main.TotalSGSTAmount ?? 0);

                parameters.Add(
                    "@TotalIGSTAmount",
                    model.main.TotalIGSTAmount ?? 0);

                parameters.Add(
                    "@TotalGSTAmount",
                    model.main.TotalGSTAmount ?? 0);

                parameters.Add(
                    "@TotalInvoiceAmount",
                    model.main.TotalInvoiceAmount ?? 0);

                parameters.Add(
                    "@BasicAmountReceived",
                    model.main.BasicAmountReceived ?? 0);

                parameters.Add(
                    "@GSTAmountReceived",
                    model.main.GSTAmountReceived ?? 0);

                parameters.Add(
                    "@TotalAmountReceived",
                    model.main.TotalAmountReceived ?? 0);

                parameters.Add(
                    "@PendingBasicAmount",
                    model.main.PendingBasicAmount ?? 0);

                parameters.Add(
                    "@PendingGSTAmount",
                    model.main.PendingGSTAmount ?? 0);

                parameters.Add(
                    "@PendingAmountBeforeTDS",
                    model.main.PendingAmountBeforeTDS ?? 0);

                parameters.Add(
                    "@TDSPercentage",
                    model.main.TDSPercentage ?? 0);

                parameters.Add(
                    "@TDSAmount",
                    model.main.TDSAmount ?? 0);

                parameters.Add(
                    "@BankAmountReceived",
                    model.main.BankAmountReceived ?? 0);

                parameters.Add(
                    "@TotalTDSAmount",
                    model.main.TotalTDSAmount ?? 0);

                parameters.Add(
                    "@FinalPendingAmount",
                    model.main.FinalPendingAmount ?? 0);

                parameters.Add(
                    "@TotalAmountBalance",
                    model.main.TotalAmountBalance ?? 0);


                // =========================================================
                // SERVICE
                // =========================================================

                parameters.Add(
                    "@servicedescription",
                    model.main.servicedescription);


                // =========================================================
                // SESSION
                // =========================================================

                parameters.Add(
                    "@sessionname",
                    model.main.sessionname);

                parameters.Add(
                    "@NAME",
                    model.main.NAME);


                // =========================================================
                // BILLING
                // =========================================================

                parameters.Add(
                    "@BillingAddress",
                    model.main.BillingAddress ?? model.main.Address);

                parameters.Add(
                    "@BillingLocation",
                    model.main.BillingLocation ?? model.main.Location);

                parameters.Add(
                    "@BillingGST",
                    model.main.BillingGST ?? model.main.gstIn);

                parameters.Add(
                    "@BillingPincode",
                    model.main.BillingPincode ?? model.main.PinCode);

                parameters.Add(
                    "@BillingStatecode",
                    model.main.BillingStatecode ?? model.main.statecode);


                // =========================================================
                // ACTION
                // =========================================================

                parameters.Add(
                    "@action",
                    Action);


                // =========================================================
                // OUTPUT INVOICE ID
                // =========================================================

                parameters.Add(
                    "@myinvoice",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output);


                // =========================================================
                // SAVE MAIN INVOICE
                // =========================================================

                await connection.ExecuteAsync(
                    "[WLSPL].[SP_SaveProforma]",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);


                // =========================================================
                // GET INVOICE ID
                // =========================================================

                int myInvoice =
                    parameters.Get<int>("@myinvoice");


                // =========================================================
                // UPDATE EXISTING RECORD
                // =========================================================

                if (Action.Equals(
                    "updateOldData",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(model.main.Id))
                        throw new Exception(
                            "Invoice ID is required for update.");

                    myInvoice =
                        Convert.ToInt32(model.main.Id);


                    // -----------------------------------------------------
                    // DELETE OLD INVOICE DETAILS
                    // -----------------------------------------------------

                    await connection.ExecuteAsync(
                        @"
                DELETE FROM tbl_ProformaInvoiceDetails
                WHERE invoiceid = @invoiceid
                ",
                        new
                        {
                            invoiceid = myInvoice
                        },
                        transaction);


                    // -----------------------------------------------------
                    // DELETE OLD BANK DETAILS
                    // -----------------------------------------------------

                    await connection.ExecuteAsync(
                        @"
                DELETE FROM [WLSPLCRM].[dbo].tbl_ProformaInvoiceBankDetails
                WHERE InvoiceMainId = @InvoiceId
                ",
                        new
                        {
                            InvoiceId = myInvoice
                        },
                        transaction);
                }


                if (myInvoice <= 0)
                {
                    throw new Exception(
                        "Invoice ID was not generated.");
                }


                // =========================================================
                // INSERT INVOICE DETAILS
                // =========================================================

                if (model.details != null &&
                    model.details.Count > 0)
                {
                    const string detailSql = @"
                INSERT INTO tbl_ProformaInvoiceDetails
                (
                    invoiceid,
                    productdescription,
                    saccode,
                    qty,
                    rate,                  
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


                    foreach (var detail in model.details)
                    {
                        var detailParameters =
                            new DynamicParameters();


                        detailParameters.Add(
                            "@invoiceid",
                            myInvoice);

                        detailParameters.Add(
                            "@productdescription",
                            detail.productdescription);

                        detailParameters.Add(
                            "@saccode",
                            detail.saccode);

                        detailParameters.Add(
                            "@qty",
                            detail.qty ?? 1);

                        detailParameters.Add(
                            "@rate",
                            detail.rate ?? 0);

                        detailParameters.Add(
                            "@taxablevalue",
                            detail.taxablevalue ?? 0);

                        detailParameters.Add(
                            "@cgstrate",
                            detail.cgstrate ?? 0);

                        detailParameters.Add(
                            "@cgstamt",
                            detail.cgstamt ?? 0);

                        detailParameters.Add(
                            "@sgstrate",
                            detail.sgstrate ?? 0);

                        detailParameters.Add(
                            "@sgstamt",
                            detail.sgstamt ?? 0);

                        detailParameters.Add(
                            "@igstrate",
                            detail.igstrate ?? 0);

                        detailParameters.Add(
                            "@igstamt",
                            detail.igstamt ?? 0);

                        detailParameters.Add(
                            "@total",
                            detail.total ?? 0);

                        detailParameters.Add(
                            "@ServiceName",
                            detail.serviceName);

                        detailParameters.Add(
                            "@ServiceId",
                            detail.serviceId);

                        detailParameters.Add(
                            "@ValidateTill",
                            detail.serviceTill);


                        await connection.ExecuteAsync(
                            detailSql,
                            detailParameters,
                            transaction);
                    }
                }


                // =========================================================
                // INSERT BANK DETAILS
                // =========================================================

                if (model.BankDetails != null &&
                    model.BankDetails.Count > 0)
                {
                    const string bankSql = @"
                INSERT INTO tbl_ProformaInvoiceBankDetails
                (
                    InvoiceMainId,
taxInvoiceId,
                    BankName,
                    ChequeNo,                   
                    Amount,
PaymentDate,
                    CreatedDate,
mode
                )
                VALUES
                (
                    @InvoiceId,
@taxInvoiceId,
                    @BankName,
                    @ChequeNo,               
                    @Amount,
                    @BankDate,
GETDATE(),
@mode
                );";


                    foreach (var bank in model.BankDetails)
                    {
                        var bankParameters =
                            new DynamicParameters();


                        bankParameters.Add(
                            "@InvoiceId",
                            myInvoice);

                        bankParameters.Add(
                      "@taxInvoiceId",
                      bank.taxinvoiceid);

                        bankParameters.Add(
                            "@BankName",
                            bank.bankName);

                        bankParameters.Add(
                           "@mode",
                           bank.mode);

                        bankParameters.Add(
                            "@ChequeNo",
                            bank.chequeNo);

                        bankParameters.Add(
                            "@BankDate",
                            bank.date);

                        bankParameters.Add(
                            "@Amount",
                            bank.amount ?? 0);


                        await connection.ExecuteAsync(
                            bankSql,
                            bankParameters,
                            transaction);
                    }
                }


                // =========================================================
                // COMMIT EVERYTHING
                // =========================================================


                string now = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                string historyMessage;
           

                if (isUpdate)
                {
                  
                    var newValues = new (string Label, string Column, object? Value)[]
                                 {
        ("Invoice Date",        "invoicedate",         model.main.invoicedate),
        ("Invoice Type",        "InvoiceType",         model.main.InvoiceType),
        ("Company Name",        "companyname",         model.main.companyName),
        ("GST No",              "cgstin",              model.main.gstIn),
        ("Address",             "address",             model.main.Address),
        ("State",               "state",               model.main.state),
        ("Service Description", "servicedescription",  model.main.servicedescription),
        ("Taxable Value",       "taxablevalue",        model.main.taxablevalue ?? 0),
        ("CGST Amount",         "cgstamt",             model.main.cgstamt ?? 0),
        ("SGST Amount",         "sgstamt",             model.main.sgstamt ?? 0),
        ("IGST Amount",         "igstamt",             model.main.igstamt ?? 0),
        ("Total Amount",        "totalamtaftertax",    model.main.totalamtaftertax ?? 0),
        ("Amount Received",     "TotalAmountReceived", model.main.TotalAmountReceived ?? 0),
        ("TDS Amount",          "TDSAmount",           model.main.TDSAmount ?? 0),
        ("Pending Amount",      "FinalPendingAmount",  model.main.FinalPendingAmount ?? 0),
                                 };

                    var changes = BuildChangeList(oldValues, newValues);

                    string header =
                        $"Proforma Invoice {model.main.invoiceno} |  Updated On: {now}";

                    historyMessage = changes.Count > 0
                        ? $"{header} | Changes: {string.Join("; ", changes)}"
                        : $"{header} | No field changes";
                }
                else
                {
                    historyMessage =
                        $"Proforma Invoice {model.main.invoiceno} | Created Date: {now}";
                }

                // keep within column size (adjust to your column length / use NVARCHAR(MAX))
                if (historyMessage.Length > 4000)
                    historyMessage = historyMessage.Substring(0, 3997) + "...";

                const string historySql = @"
    INSERT INTO [dbo].[CommentHistory]
        (sessionname, ccode, commentdatetime, message)
    VALUES
        (@sessionname, @ccode, @commentdatetime, @message)";

                await connection.ExecuteAsync(
                    historySql,
                    new
                    {
                        sessionname = model.main.sessionname,
                        ccode = model.main.companyCode,
                        commentdatetime = DateTime.Now,
                        message = historyMessage
                    },
                    transaction);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // =========================================================
                // ROLLBACK EVERYTHING
                // =========================================================

                await transaction.RollbackAsync();

                throw ex;
            }
        }

        private static List<string> BuildChangeList(Dictionary<string, object?> oldValues,IEnumerable<(string Label, string Column, object? Value)> newValues)
        {
            var changes = new List<string>();
            if (oldValues == null) return changes;

            foreach (var (label, column, newVal) in newValues)
            {
                oldValues.TryGetValue(column, out var oldVal);

                if (!AreEqual(oldVal, newVal))
                {
                    changes.Add($"{label}: '{Display(oldVal)}' → '{Display(newVal)}'");
                }
            }

            return changes;
        }

        private static bool AreEqual(object? a, object? b)
        {
            a = a == DBNull.Value ? null : a;
            b = b == DBNull.Value ? null : b;

            if (a == null && b == null) return true;
            if (a == null) return string.IsNullOrWhiteSpace(b?.ToString());
            if (b == null) return string.IsNullOrWhiteSpace(a.ToString());

            if (a is DateTime da && b is DateTime db) return da.Date == db.Date;

            if (decimal.TryParse(a.ToString(), out var na) &&
                decimal.TryParse(b.ToString(), out var nb))
                return Math.Round(na, 2) == Math.Round(nb, 2);

            return string.Equals(a.ToString()?.Trim(), b.ToString()?.Trim(),
                                 StringComparison.OrdinalIgnoreCase);
        }

        private static string Display(object? value)
        {
            if (value == null || value == DBNull.Value) return "-";
            if (value is DateTime d) return d.ToString("dd-MMM-yyyy");
            return value.ToString()?.Trim() ?? "-";
        }

        public async Task<bool> DeleteInvoiceDetails(int id, string name)
        {
            using var connection = new SqlConnection(
         _configuration.GetConnectionString("Conn_Stringg"));

            const string sql = @"
        DELETE tbl_ProformaInvoiceMain  WHERE id = @invoiceId
DELETE tbl_ProformaInvoiceDetails  WHERE invoiceid = @invoiceId
DELETE tbl_ProformaInvoiceBankDetails  WHERE InvoiceMainId = @invoiceId";

            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                sql,
                new
                {
                    invoiceId = id,
                    deletedBy = name
                });

            return rowsAffected > 0;
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

        public byte[] ProformaPdf(int id)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Header panel occupies y = 680 to 815 (135pt tall).
                // A4 height ≈ 841.89pt, so top margin must push flow content down to exactly y=680.
                float pageHeight = iTextSharp.text.PageSize.A4.Height;
                float topMargin = pageHeight - 680f;   // ≈ 161.89f
                float bottomMargin = 40f;

                Document document = new Document(iTextSharp.text.PageSize.A4, 10f, 10f, topMargin, bottomMargin);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                // Removed document.NewPage() and document.SetMargins() — margins are already
                // set correctly via the Document constructor; calling NewPage() right after
                // Open() and re-setting margins caused page-1 margin timing issues.

                // ---- Simple 2-color palette ----
                BaseColor brand = new BaseColor(24, 74, 128);
                BaseColor lightTint = new BaseColor(240, 244, 249);
                BaseColor altRow = new BaseColor(248, 249, 251);
                BaseColor borderGray = new BaseColor(200, 205, 212);
                BaseColor textDark = new BaseColor(45, 45, 45);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED);

                // ================= TOP BLOCK — one clean colored panel =================
                PdfContentByte cb = writer.DirectContent;

                cb.SetColorFill(brand);
                cb.Rectangle(17f, 680f, 560f, 135f);
                cb.Fill();

                cb.SetColorStroke(BaseColor.WHITE);
                cb.SetLineWidth(0.75f);
                cb.MoveTo(17f, 710f);
                cb.LineTo(577f, 710f);
                cb.Stroke();

                cb.SetColorStroke(borderGray);
                cb.SetLineWidth(0.75f);
                cb.Rectangle(17f, 680f, 560f, 135f);
                cb.Stroke();

                cb.BeginText();
                cb.SetColorFill(BaseColor.WHITE);
                cb.SetFontAndSize(bf, 22);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "WEB LINK SERVICES PVT. LTD.", 175, 792, 0);
                cb.SetFontAndSize(bf, 10);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "12th Floor, Vintage 21, Above Max Showroom, Near Pantaloons, P.K. Chowk,", 175, 776, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Pimple Saudagar, Pune, Maharashtra - 411027", 175, 764, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "weblinkservices.net   |   GST: 27AANFP3412E1ZE   |   PAN: AANFP3412E", 175, 748, 0);
                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 10);
                cb.SetColorFill(BaseColor.WHITE);

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Email ID : info@weblinkservices.net", 25f, 720f, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Phone No. : 8421060192", 570f, 720f, 0);

                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 15);
                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "P R O F O R M A", 297, 690, 0);
                cb.EndText();

                // ---- Logo: white rounded box + logo drawn directly via DirectContent (single draw path) ----
                cb.SetColorFill(BaseColor.WHITE);
                cb.RoundRectangle(30f, 745f, 110f, 60f, 6f);
                cb.Fill();

                string logoPath = Path.Combine(_env.WebRootPath, "assets", "images", "WLSPL_MAIN_LOGO.png");

                if (File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(95, 45);
                    float logoX = 30f + (110f - logo.ScaledWidth) / 2f;
                    float logoY = 745f + (60f - logo.ScaledHeight) / 2f;
                    logo.SetAbsolutePosition(logoX, logoY);

                    // FIX: draw straight into DirectContent instead of document.Add(logo).
                    // document.Add() queues into the flowing-content buffer, which can render
                    // at a different time/z-order than the DirectContent fills above it —
                    // that mismatch was causing the logo to appear missing, faint, or duplicated.
                    cb.AddImage(logo);
                }

                // **Fetching Data via VM_Proforma**
                VM_Proforma vm = GetProformaData(id);

                if (vm != null && vm.ID != null)
                {
                    string CompanyName = vm.CompanyName ?? "N/A";
                    string ProformaDate = vm.ProformaDate.HasValue ? vm.ProformaDate.Value.ToString("dd-MM-yyyy") : "N/A";
                    string ProformaNo = vm.ProformaNo ?? "N/A";
                    string Address = vm.Address ?? "N/A";
                    string Gstno = vm.GSTNO ?? "N/A";
                    string AgainstBy = vm.AgainstBy ?? "N/A";
                    string AgainstNo = vm.AgainstNo ?? "N/A";

                    Font boldFont12White = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE);
                    Font boldFont10Brand = FontFactory.GetFont("Arial", 10, Font.BOLD, brand);
                    Font boldFont11 = FontFactory.GetFont("Arial", 11, Font.BOLD, textDark);
                    Font boldFont10 = FontFactory.GetFont("Arial", 10, Font.BOLD, textDark);
                    Font Font10 = FontFactory.GetFont("Arial", 10, Font.NORMAL, textDark);
                    Font Font9 = FontFactory.GetFont("Arial", 9, Font.NORMAL, textDark);
                    Font italicFont10Gray = FontFactory.GetFont("Arial", 10, Font.ITALIC, new BaseColor(100, 100, 100));

                    // ---- Company / Proforma info table — SpacingBefore removed (real margin handles it now) ----
                    Paragraph paragraphTable1 = new Paragraph { SpacingBefore = 0f, SpacingAfter = 0f };

                    PdfPTable table = new PdfPTable(4) { TotalWidth = 560f, LockedWidth = true };
                    table.SetWidths(new float[] { 150, 300, 150, 300 });

                    PdfPCell InfoLabelCell(string text) => new PdfPCell(new Phrase(text, boldFont10Brand))
                    {
                        BackgroundColor = lightTint,
                        BorderColor = borderGray,
                        BorderWidth = 0.5f,
                        PaddingTop = 7f,
                        PaddingBottom = 7f,
                        PaddingLeft = 8f,
                        MinimumHeight = 26f
                    };
                    PdfPCell InfoValueCell(string text) => new PdfPCell(new Phrase(text, Font10))
                    {
                        BorderColor = borderGray,
                        BorderWidth = 0.5f,
                        PaddingTop = 7f,
                        PaddingBottom = 7f,
                        PaddingLeft = 8f,
                        MinimumHeight = 26f
                    };

                    table.AddCell(InfoLabelCell("Company Name:"));
                    table.AddCell(InfoValueCell(CompanyName));
                    table.AddCell(InfoLabelCell("Proforma No:"));
                    table.AddCell(InfoValueCell(ProformaNo));

                    table.AddCell(InfoLabelCell("Address:"));
                    table.AddCell(InfoValueCell(Address));
                    table.AddCell(InfoLabelCell("Proforma Date:"));
                    table.AddCell(InfoValueCell(ProformaDate));

                    if (!string.IsNullOrWhiteSpace(Gstno))
                    {
                        table.AddCell(InfoLabelCell("GST No:"));
                        table.AddCell(InfoValueCell(Gstno));
                        table.AddCell(new PdfPCell(new Phrase("", Font10)) { BorderColor = borderGray, BorderWidth = 0.5f });
                        table.AddCell(new PdfPCell(new Phrase("", Font10)) { BorderColor = borderGray, BorderWidth = 0.5f });
                    }
                    if (!string.IsNullOrWhiteSpace(AgainstBy) && AgainstBy == "Quotation")
                    {
                        table.AddCell(InfoLabelCell("Against By :"));
                        table.AddCell(InfoValueCell(AgainstBy));

                        table.AddCell(InfoLabelCell("Against No. :"));
                        table.AddCell(InfoValueCell(AgainstNo));

                    }


                    paragraphTable1.Add(table);
                    document.Add(paragraphTable1);

                    // ---- Section title bar ----
                    table = new PdfPTable(1) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    table.SetWidths(new float[] { 560f });
                    table.AddCell(new PdfPCell(new Phrase("SERVICE DETAILS", boldFont12White))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        BackgroundColor = brand,
                        BorderColor = borderGray,
                        BorderWidth = 0.5f,
                        PaddingTop = 8f,
                        PaddingBottom = 8f,
                        MinimumHeight = 28f
                    });
                    document.Add(table);



                    double taxableTotal = 0, cgstTotal = 0, sgstTotal = 0, igstTotal = 0, grandTotal = 0;

                    if (vm.objtblProformaDtl != null && vm.objtblProformaDtl.Count > 0)
                    {
                        bool isIGST = !string.Equals(
                            (vm.State ?? "").Trim(),
                            (vm.BillState ?? "").Trim(),
                            StringComparison.OrdinalIgnoreCase);

                        Paragraph paragraphTable2 = new Paragraph
                        {
                            SpacingBefore = 0f,
                            SpacingAfter = 0f
                        };

                        PdfPTable prodTable;

                        if (isIGST)
                        {
                            // SN, Description, HSN, Qty, Rate, Taxable, IGST, Total
                            prodTable = new PdfPTable(8);

                            prodTable.SetWidths(new float[]
                            {
            2f, 16f, 5f, 4f, 4f, 5f, 7f, 6f
                            });
                        }
                        else
                        {
                            // SN, Description, HSN, Qty, Rate, Taxable, CGST, SGST, Total
                            prodTable = new PdfPTable(9);

                            prodTable.SetWidths(new float[]
                            {
            2f, 14f, 5f, 4f, 5f, 6f, 6f, 6f, 6f
                            });
                        }

                        prodTable.TotalWidth = 560f;
                        prodTable.LockedWidth = true;
                        prodTable.SpacingBefore = 0f;
                        prodTable.SpacingAfter = 0f;

                        Font headerFontWhite = FontFactory.GetFont(
                            "Arial",
                            10,
                            Font.BOLD,
                            BaseColor.WHITE
                        );

                        PdfPCell HeaderCell(string text)
                        {
                            return new PdfPCell(
                                new Phrase(text, headerFontWhite))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                BackgroundColor = brand,
                                BorderColor = borderGray,
                                BorderWidth = 0.5f,
                                PaddingTop = 7f,
                                PaddingBottom = 9f,
                                MinimumHeight = 26f
                            };
                        }

                        prodTable.AddCell(HeaderCell("SN."));
                        prodTable.AddCell(HeaderCell("Description"));
                        prodTable.AddCell(HeaderCell("Hsn/Sac"));
                        prodTable.AddCell(HeaderCell("Qty"));
                        prodTable.AddCell(HeaderCell("Rate"));
                        prodTable.AddCell(HeaderCell("Taxable Val"));

                        if (isIGST)
                        {
                            prodTable.AddCell(HeaderCell("IGST"));
                        }
                        else
                        {
                            prodTable.AddCell(HeaderCell("CGST"));
                            prodTable.AddCell(HeaderCell("SGST"));
                        }

                        prodTable.AddCell(HeaderCell("Total"));


                        Font gstRateFont = FontFactory.GetFont(
                            "Arial",
                            7,
                            Font.NORMAL,
                            BaseColor.DARK_GRAY
                        );

                        Font gstAmountFont = FontFactory.GetFont(
                            "Arial",
                            9,

                            BaseColor.DARK_GRAY
                        );

                        PdfPCell BodyCell(string text, bool shaded)
                        {
                            return new PdfPCell(
                                new Phrase(text ?? "", Font9))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                BackgroundColor = shaded ? altRow : BaseColor.WHITE,
                                BorderColor = borderGray,
                                BorderWidth = 0.5f,
                                PaddingTop = 7f,
                                PaddingBottom = 9f,
                                MinimumHeight = 26f
                            };
                        }


                        // GST cell:
                        // Percentage on top in ()
                        // Amount below
                        PdfPCell GstCell(string rate, double amount, bool shaded)
                        {
                            Paragraph gstParagraph = new Paragraph();
                            gstParagraph.Alignment = Element.ALIGN_CENTER;
                            gstParagraph.SpacingBefore = 0f;
                            gstParagraph.SpacingAfter = 0f;

                            Chunk rateChunk = new Chunk(
                                "(" + (rate ?? "0") + "%)",
                                gstRateFont
                            );

                            Chunk amountChunk = new Chunk(
                                amount.ToString("#"),
                                gstAmountFont
                            );

                            gstParagraph.Add(rateChunk);
                            gstParagraph.Add(Chunk.NEWLINE);
                            gstParagraph.Add(amountChunk);

                            return new PdfPCell(gstParagraph)
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                BackgroundColor = shaded ? altRow : BaseColor.WHITE,
                                BorderColor = borderGray,
                                BorderWidth = 0.5f,
                                PaddingTop = 5f,
                                PaddingBottom = 5f,
                                MinimumHeight = 26f
                            };
                        }


                        int rowid = 1;

                        foreach (var d in vm.objtblProformaDtl)
                        {
                            bool shaded = rowid % 2 == 0;

                            double taxableVal = ParseD(d.TaxableValue);
                            double lineTotal = ParseD(d.Total);

                            prodTable.AddCell(
                                BodyCell(rowid.ToString(), shaded));

                            prodTable.AddCell(
     BodyCell($"{d.ServiceName} - {d.ProductDescription}", shaded)
 );

                            prodTable.AddCell(
                                BodyCell(d.SACCode, shaded));

                            prodTable.AddCell(
                                BodyCell(d.Qty, shaded));

                            prodTable.AddCell(
                                BodyCell(d.Rate, shaded));

                            prodTable.AddCell(
                                BodyCell(taxableVal.ToString("#"), shaded));


                            // =========================
                            // GST
                            // =========================

                            if (isIGST)
                            {
                                double igstAmt = ParseD(d.IGSTAmt);

                                prodTable.AddCell(
                                    GstCell(d.IGSTRate, igstAmt, shaded));

                                igstTotal += igstAmt;
                            }
                            else
                            {
                                double cgstAmt = ParseD(d.CGSTAmt);
                                double sgstAmt = ParseD(d.SGSTAmt);

                                prodTable.AddCell(
                                    GstCell(d.CGSTRate, cgstAmt, shaded));

                                prodTable.AddCell(
                                    GstCell(d.SGSTRate, sgstAmt, shaded));

                                cgstTotal += cgstAmt;
                                sgstTotal += sgstAmt;
                            }


                            prodTable.AddCell(
                                BodyCell(lineTotal.ToString("#"), shaded));

                            taxableTotal += taxableVal;
                            grandTotal += lineTotal;

                            rowid++;
                        }

                        paragraphTable2.Add(prodTable);
                        document.Add(paragraphTable2);


                        // =========================
                        // TOTALS
                        // =========================

                        AddTotalRow(
                            document,
                            "Sub Total",
                            taxableTotal,
                            boldFont10,
                            Font10,
                            lightTint,
                            false,
                            borderGray
                        );

                        if (isIGST)
                        {
                            AddTotalRow(
                                document,
                                "IGST Amount",
                                igstTotal,
                                boldFont10,
                                Font10,
                                lightTint,
                                false,
                                borderGray
                            );
                        }
                        else
                        {
                            AddTotalRow(
                                document,
                                "CGST Amount",
                                cgstTotal,
                                boldFont10,
                                Font10,
                                lightTint,
                                false,
                                borderGray
                            );

                            AddTotalRow(
                                document,
                                "SGST Amount",
                                sgstTotal,
                                boldFont10,
                                Font10,
                                lightTint,
                                false,
                                borderGray
                            );
                        }

                        Font grandLabelWhite = FontFactory.GetFont(
                            "Arial",
                            11,
                            Font.BOLD,
                            BaseColor.WHITE
                        );

                        Font grandValWhite = FontFactory.GetFont(
                            "Arial",
                            11,
                            Font.BOLD,
                            BaseColor.WHITE
                        );

                        AddTotalRow(
                            document,
                            "Grand Total",
                            grandTotal,
                            grandLabelWhite,
                            grandValWhite,
                            brand,
                            true,
                            borderGray
                        );
                    }
                    // ---- Grand Total in Words ----
                    DataTable Dts = new DataTable();
                    using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                    {
                        string query = "SELECT dbo.[ToWords]('" + grandTotal + "') AS AmountInWords";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            SqlDataAdapter Da = new SqlDataAdapter(cmd);
                            Da.Fill(Dts);
                        }
                    }
                    if (Dts.Rows.Count > 0)
                    {
                        string AmountInWords = Dts.Rows[0]["AmountInWords"]?.ToString() ?? "N/A";

                        table = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                        table.SetWidths(new float[] { 140f, 420f });

                        table.AddCell(new PdfPCell(new Phrase("Amount In Words (Rs.)", boldFont10))
                        {
                            BackgroundColor = lightTint,
                            BorderColor = borderGray,
                            BorderWidth = 0.5f,
                            PaddingTop = 7f,
                            PaddingBottom = 7f,
                            PaddingLeft = 8f,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        });
                        table.AddCell(new PdfPCell(new Phrase(AmountInWords, italicFont10Gray))
                        {
                            BorderColor = borderGray,
                            BorderWidth = 0.5f,
                            PaddingTop = 7f,
                            PaddingBottom = 7f,
                            PaddingLeft = 8f,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        });

                        document.Add(table);
                    }

                    // ---- Bank Details ----
                    PdfPTable bankTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    bankTable.SetWidths(new float[] { 310f, 250f });

                    bankTable.AddCell(new PdfPCell(new Phrase(
                        "Account Name : Web Link Services Pvt. Ltd\n\n" +
                        "A/c No. : 916020085136854\n\n" +
                        "IFSC/Neft Code : UTIB0001641\n\n" +
                        "Bank Name : Axis Bank Ltd - Rahatani Branch, Pune",
                        Font10))
                    {
                        BackgroundColor = lightTint,
                        BorderColor = borderGray,
                        BorderWidth = 0.5f,
                        Padding = 10f
                    });

                    bankTable.AddCell(new PdfPCell(new Phrase(
                        "For,\n\n " +
                        "                 Web Link Services Pvt. Ltd\n\n\n" +
                        "                     Authorised Signatory",
                        boldFont11))
                    {
                        BorderColor = borderGray,
                        BorderWidth = 0.5f,
                        Padding = 10f,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    document.Add(bankTable);

                    // ---- Watermark ----
                    PdfContentByte under = writer.DirectContentUnder;
                    string watermarkPath = Path.Combine(_env.WebRootPath, "assets", "images", "WLSPL_MAIN_LOGO.png");

                    if (File.Exists(watermarkPath))
                    {
                        iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(watermarkPath);
                        watermark.ScaleToFit(200, 200);
                        watermark.SetAbsolutePosition(180, 450);

                        PdfGState gState = new PdfGState { FillOpacity = 0.07f };

                        under.SaveState();
                        under.SetGState(gState);
                        under.AddImage(watermark);
                        under.RestoreState();
                    }
                }

                document.Close();
                writer.Close();

                return stream.ToArray();
            }
        }

        // ---- Helper: totals row ----
        private void AddTotalRow(Document document, string label, double value, Font labelFont, Font valueFont, BaseColor bgColor, bool highlight, BaseColor borderColor)
        {
            var t = new PdfPTable(3) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
            t.SetWidths(new float[] { 380f, 100f, 80f });

            t.AddCell(new PdfPCell(new Phrase("")) { BorderColor = borderColor, BorderWidth = 0.5f, BackgroundColor = highlight ? bgColor : BaseColor.WHITE });

            var lbl = new PdfPCell(new Phrase(label, labelFont))
            {
                PaddingRight = 8f,
                PaddingTop = 7f,
                PaddingBottom = 7f,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = bgColor,
                BorderColor = borderColor,
                BorderWidth = 0.5f
            };
            t.AddCell(lbl);

            var val = new PdfPCell(new Phrase(value.ToString("#.00"), valueFont))
            {
                PaddingTop = 7f,
                PaddingBottom = 7f,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = bgColor,
                BorderColor = borderColor,
                BorderWidth = 0.5f
            };
            t.AddCell(val);

            document.Add(t);
        }

        // ---- Helper: parse string amount safely ----
        private double ParseD(string s) =>
            double.TryParse(s, out double v) ? v : 0;

        private VM_Proforma GetProformaData(int id)
        {
            try
            {
                var vm = new VM_Proforma();

                string query = @"
         SELECT ID,invoiceno AS ProformaNo,invoicedate AS ProformaDate, ReverseCharge, State, CompanyName,Againstby,AgainstByValue AS AgainstNo,
               CompanyCode, Address, cgstin as GSTNO, BillState, TotalAmtBeforeTax, TotalAmtAfterTax
        FROM tbl_ProformaInvoiceMain
        WHERE ID = @ID;

         SELECT ID,invoiceid AS ProformaID,ServiceName,  ProductDescription, SACCode, CAST(qty as float) as qty,
      CAST(Rate as float) as  Rate, Amount, TaxableValue,
               CAST(CGSTRate as float) as CGSTRate, CGSTAmt, CAST(SGSTRate as float) as SGSTRate,
               SGSTAmt,CAST(IGSTRate as float) as  IGSTRate, IGSTAmt, Total
        FROM [tbl_ProformaInvoiceDetails]      
        WHERE invoiceid = @ID
        ORDER BY ID;";

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    con.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            vm.ID = rdr["ID"] as int?;
                            vm.ProformaNo = rdr["ProformaNo"]?.ToString();
                            vm.ProformaDate = rdr["ProformaDate"] as DateTime?;
                            vm.ReverseCharge = rdr["ReverseCharge"]?.ToString();
                            vm.State = rdr["State"]?.ToString();
                            vm.CompanyName = rdr["CompanyName"]?.ToString();
                            vm.CompanyCode = rdr["CompanyCode"]?.ToString();
                            vm.Address = rdr["Address"]?.ToString();
                            vm.GSTNO = rdr["GSTNO"]?.ToString();
                            vm.AgainstBy = rdr["AgainstBy"]?.ToString();
                            vm.AgainstNo = rdr["AgainstNo"]?.ToString();
                            vm.BillState = rdr["BillState"]?.ToString();
                            vm.TotalAmtBeforeTax = rdr["TotalAmtBeforeTax"]?.ToString();
                            vm.TotalAmtAfterTax = rdr["TotalAmtAfterTax"]?.ToString();
                        }

                        vm.objtblProformaDtl = new List<VM_Proforma.ProformaDetailVM>();

                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {
                                vm.objtblProformaDtl.Add(new VM_Proforma.ProformaDetailVM
                                {
                                    ID = rdr["ID"] as int?,
                                    ProformaID = rdr["ProformaID"] as int?,
                                    ServiceName = rdr["ServiceName"]?.ToString(),
                                    ProductDescription = rdr["ProductDescription"]?.ToString(),
                                    SACCode = rdr["SACCode"]?.ToString(),
                                    Qty = rdr["Qty"]?.ToString(),
                                    Rate = rdr["Rate"]?.ToString(),
                                    Amount = rdr["Amount"]?.ToString(),
                                    TaxableValue = rdr["TaxableValue"]?.ToString(),
                                    CGSTRate = rdr["CGSTRate"]?.ToString(),
                                    CGSTAmt = rdr["CGSTAmt"]?.ToString(),
                                    SGSTRate = rdr["SGSTRate"]?.ToString(),
                                    SGSTAmt = rdr["SGSTAmt"]?.ToString(),
                                    IGSTRate = rdr["IGSTRate"]?.ToString(),
                                    IGSTAmt = rdr["IGSTAmt"]?.ToString(),
                                    Total = rdr["Total"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return vm;

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<List<ProformaInvoice.ProformaInvoiceCreate>> GetFinancialYearSummary(string financialYear, string? salesManager, string empCode, string empRole)
        {
            var result = new List<ProformaInvoice.ProformaInvoiceCreate>();

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

                        FROM tbl_ProformaInvoiceMain

                        WHERE 
                            e_invoice_cancel_status IS NULL
                            AND invoicedate >= @StartDate
                            AND invoicedate < DATEADD(DAY, 1, @EndDate)
                            AND (@SalesManager IS NULL OR sessionname  = @SalesManager)                     
                                      AND EXISTS
        (
            SELECT 1
            FROM dbo.FN_EmployeeHierarchy(@CurrentUser) h
            WHERE h.empcode = sessionname
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
                                new ProformaInvoice.ProformaInvoiceCreate
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
                    string query = @"  SELECT empcode as UserCode, name as FullName 
              FROM dbo.FN_EmployeeHierarchy(@CurrentUser) 
              WHERE isdeleted=0 And status=1";
                    var parameters = new DynamicParameters();
                    parameters.Add("@CurrentUser", empCode);           
                    using (var multi = await connection.QueryMultipleAsync(query, parameters))
                    {
                        var salesManagers = (await multi.ReadAsync<dynamic>()).ToList();
                        return new
                        {
                            SalesManagers = salesManagers
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProformaInvoice.ProformaInvoiceCreate>> GetInfo(string financialYear, int? month, string? salesManager, string empCode, string empRole)
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
                            pm.id,
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
          ISNULL(
                                TRY_CAST(TotalAmountReceived AS DECIMAL(18,2)),
                                0
                            ) AS TotalAmountReceived,
                              ISNULL(
                                TRY_CAST(FinalPendingAmount AS DECIMAL(18,2)),
                                0
                            ) AS FinalPendingAmount,
                            isapprove,
                            isreject,
                            ExportInvoiceNo,
                            NAME, CASE
        WHEN EXISTS (
            SELECT 1
            FROM tbl_ProformaInvoiceBankDetails d
            WHERE d.InvoiceMainId = pm.id
            AND TaxInvoiceID IS NULL
        )
        THEN 'Open'
        ELSE 'Close'
    END AS Status

                        FROM tbl_ProformaInvoiceMain as pm
                        INNER JOIN employees as e on e.empcode=pm.sessionname

                        WHERE invoicedate >= @StartDate
                          AND invoicedate < @EndDate
                          AND (@SalesManager IS NULL OR empcode = @SalesManager)                        
                               AND EXISTS
          (
              SELECT 1
              FROM dbo.FN_EmployeeHierarchy(@CurrentUser) h
              WHERE h.empcode = pm.sessionname
          )

                        ORDER BY id DESC;
                    ";

            var parameters = new DynamicParameters();

            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);
            parameters.Add("@SalesManager", string.IsNullOrWhiteSpace(salesManager) ? null : salesManager, DbType.String);
            parameters.Add("@CurrentUser", empCode);
            parameters.Add("@CurrentRole", empRole);

            var result = await connection.QueryAsync<ProformaInvoice.ProformaInvoiceCreate>(query, parameters);

            return result.ToList();
        }

    }
}
