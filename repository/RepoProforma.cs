using Dapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using WEBLINK_CRM.Models;
using static WEBLINK_CRM.Models.VM_Proforma;

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

        public async Task<List<object>> GetCompanyList(string Status)
        {
            using (var connection = new SqlConnection(
               _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetCompanyList");
                parameters.Add("@Status", Status);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }

        public async Task<List<object>> GetCompanyByCode(string Code)
        {
            using (var connection = new SqlConnection(
                 _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetCompanyDataByCode");
                parameters.Add("@Code", Code);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<List<object>> GetDetailsByQuotationNo(string Code)
        {
            using (var connection = new SqlConnection(
                 _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetDetailsByQuotationNo");
                parameters.Add("@QuotationNo", Code);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }
        public async Task<int> Save(VM_Proforma model)
        {
            try
            {
                using (SqlConnection con =
                       new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
                {
                    await con.OpenAsync();

                    var parameters = new DynamicParameters();

                    parameters.Add("@ID", model.ID);                
                    parameters.Add("@ProformaDate", model.ProformaDate);
                    parameters.Add("@ReverseCharge", model.ReverseCharge);
                    parameters.Add("@State", model.State);
                    parameters.Add("@CompanyName", model.CompanyName);
                    parameters.Add("@CompanyCode", model.CompanyCode);
                    parameters.Add("@Address", model.Address);
                    parameters.Add("@GSTNO", model.GSTNO);
                    parameters.Add("@BillState", model.BillState);
                    parameters.Add("@TotalAmtBeforeTax", model.TotalAmtBeforeTax);
                    parameters.Add("@TotalAmtAfterTax", model.TotalAmtAfterTax);
                    parameters.Add("@CreatedBy", model.CreatedBy);
                    parameters.Add("@AgainstBy", model.AgainstBy);
                    parameters.Add("@AgainstNo", model.AgainstNo);

                    DataTable dtDetails = new DataTable();
                    dtDetails.Columns.Add("ProductDescription", typeof(string));
                    dtDetails.Columns.Add("SACCode", typeof(string));
                    dtDetails.Columns.Add("Qty", typeof(decimal));
                    dtDetails.Columns.Add("Rate", typeof(decimal));
                    dtDetails.Columns.Add("Amount", typeof(decimal));
                    dtDetails.Columns.Add("TaxableValue", typeof(decimal));
                    dtDetails.Columns.Add("CGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("CGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("SGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("SGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("IGSTRate", typeof(decimal));
                    dtDetails.Columns.Add("IGSTAmt", typeof(decimal));
                    dtDetails.Columns.Add("Total", typeof(decimal));

                    if (model.objtblProformaDtl != null)
                    {
                        foreach (var item in model.objtblProformaDtl)
                        {
                            dtDetails.Rows.Add(
        item.ProductDescription,
        item.SACCode,
        item.Qty,
        item.Rate,
        item.Amount,
        item.TaxableValue,
        item.CGSTRate,
        item.CGSTAmt,
        item.SGSTRate,
        item.SGSTAmt,
        item.IGSTRate,
        item.IGSTAmt,
        item.Total
    );
                        }
                    }

                    parameters.Add(
                        "@ProformaDetails",
                        dtDetails.AsTableValuedParameter("WLSPL.ProformaDetailType")
                    );

                    var result = await con.QuerySingleAsync<int>(
                        "[WLSPL].[SP_SaveProforma]",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<VM_Proforma>> GetProformaList(string size)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetProformaList");
                parameters.Add("@PageSize", size);

                var result = await connection.QueryAsync<VM_Proforma>(
     "SP_Proforma",
     parameters,
     commandType: CommandType.StoredProcedure
 );

                return result.Cast<VM_Proforma>().ToList();
            }
        }

        public async Task<VM_Proforma> GetProformaById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetProformaDataById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryFirstOrDefaultAsync<VM_Proforma>(
               "SP_Proforma",
               parameters,
               commandType: CommandType.StoredProcedure
           );

                return result;
            }
        }
        public async Task<List<ProformaDetailVM>> GetDetailsById(string ID)
        {
            using (var connection = new SqlConnection(
                _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetDetailsById");
                parameters.Add("@ID", ID);

                var result = await connection.QueryAsync<ProformaDetailVM>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<ProformaDetailVM>().ToList();
            }
        }

        public async Task<bool> Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Conn_Stringg")))
            {
                using (SqlCommand cmd = new SqlCommand("SP_Proforma", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "Delete");
                    cmd.Parameters.AddWithValue("@ID", id);

                    await con.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    return result > 0;
                }
            }
        }

        public async Task<List<object>> GetQuotationNoList(string CompanyCode)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetQuotationNoList");
                parameters.Add("@CompanyCode", CompanyCode);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
        }

        public async Task<List<object>> GetStateList(string Status)
        {
            using (var connection = new SqlConnection(
              _configuration.GetConnectionString("Conn_Stringg")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                parameters.Add("@Action", "GetStateList");
                parameters.Add("@Status", Status);

                var result = await connection.QueryAsync<object>(
                    "SP_Proforma",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.Cast<object>().ToList();
            }
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

                    paragraphTable1.Add(table);
                    document.Add(paragraphTable1);

                    // ---- Section title bar ----
                    table = new PdfPTable(1) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    table.SetWidths(new float[] { 560f });
                    table.AddCell(new PdfPCell(new Phrase("PRODUCT DETAILS", boldFont12White))
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

                    // ---- Product Details Table ----
                    double taxableTotal = 0, cgstTotal = 0, sgstTotal = 0, igstTotal = 0, grandTotal = 0;

                    if (vm.objtblProformaDtl != null && vm.objtblProformaDtl.Count > 0)
                    {
                        bool isIGST = !string.Equals(
                            (vm.State ?? "").Trim(),
                            (vm.BillState ?? "").Trim(),
                            StringComparison.OrdinalIgnoreCase);

                        Paragraph paragraphTable2 = new Paragraph { SpacingBefore = 0f, SpacingAfter = 0f };

                        PdfPTable prodTable;
                        if (isIGST)
                        {
                            prodTable = new PdfPTable(9);
                            prodTable.SetWidths(new float[] { 2f, 16f, 5f, 4f, 4f, 5f, 4f, 5f, 6f });
                        }
                        else
                        {
                            prodTable = new PdfPTable(11);
                            prodTable.SetWidths(new float[] { 2f, 14f, 5f, 4f, 4f, 5f, 4f, 5f, 4f, 5f, 6f });
                        }
                        prodTable.TotalWidth = 560f;
                        prodTable.LockedWidth = true;
                        prodTable.SpacingBefore = 0f;
                        prodTable.SpacingAfter = 0f;

                        Font headerFontWhite = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);

                        PdfPCell HeaderCell(string text) => new PdfPCell(new Phrase(text, headerFontWhite))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            BackgroundColor = brand,
                            BorderColor = borderGray,
                            BorderWidth = 0.5f,
                            PaddingTop = 7f,
                            PaddingBottom = 9f,
                            MinimumHeight = 26f
                        };

                        prodTable.AddCell(HeaderCell("SN."));
                        prodTable.AddCell(HeaderCell("Description"));
                        prodTable.AddCell(HeaderCell("Hsn/Sac"));
                        prodTable.AddCell(HeaderCell("Qty"));
                        prodTable.AddCell(HeaderCell("Rate"));
                        prodTable.AddCell(HeaderCell("Taxable Val"));

                        if (isIGST)
                        {
                            prodTable.AddCell(HeaderCell("IGST(%)"));
                            prodTable.AddCell(HeaderCell("IGST Amt"));
                        }
                        else
                        {
                            prodTable.AddCell(HeaderCell("CGST(%)"));
                            prodTable.AddCell(HeaderCell("CGST Amt"));
                            prodTable.AddCell(HeaderCell("SGST(%)"));
                            prodTable.AddCell(HeaderCell("SGST Amt"));
                        }
                        prodTable.AddCell(HeaderCell("Total"));

                        PdfPCell BodyCell(string text, bool shaded) => new PdfPCell(new Phrase(text ?? "", Font9))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            BackgroundColor = shaded ? altRow : BaseColor.WHITE,
                            BorderColor = borderGray,
                            BorderWidth = 0.5f,
                            PaddingTop = 7f,
                            PaddingBottom = 9f,
                            MinimumHeight = 26f
                        };

                        int rowid = 1;
                        foreach (var d in vm.objtblProformaDtl)
                        {
                            bool shaded = rowid % 2 == 0;
                            double taxableVal = ParseD(d.TaxableValue);
                            double lineTotal = ParseD(d.Total);

                            prodTable.AddCell(BodyCell(rowid.ToString(), shaded));
                            prodTable.AddCell(BodyCell(d.ProductDescription, shaded));
                            prodTable.AddCell(BodyCell(d.SACCode, shaded));
                            prodTable.AddCell(BodyCell(d.Qty, shaded));
                            prodTable.AddCell(BodyCell(d.Rate, shaded));
                            prodTable.AddCell(BodyCell(taxableVal.ToString("#.00"), shaded));

                            if (isIGST)
                            {
                                prodTable.AddCell(BodyCell(d.IGSTRate, shaded));
                                prodTable.AddCell(BodyCell(ParseD(d.IGSTAmt).ToString("#.00"), shaded));
                                igstTotal += ParseD(d.IGSTAmt);
                            }
                            else
                            {
                                prodTable.AddCell(BodyCell(d.CGSTRate, shaded));
                                prodTable.AddCell(BodyCell(ParseD(d.CGSTAmt).ToString("#.00"), shaded));
                                prodTable.AddCell(BodyCell(d.SGSTRate, shaded));
                                prodTable.AddCell(BodyCell(ParseD(d.SGSTAmt).ToString("#.00"), shaded));
                                cgstTotal += ParseD(d.CGSTAmt);
                                sgstTotal += ParseD(d.SGSTAmt);
                            }

                            prodTable.AddCell(BodyCell(lineTotal.ToString("#.00"), shaded));

                            taxableTotal += taxableVal;
                            grandTotal += lineTotal;
                            rowid++;
                        }

                        paragraphTable2.Add(prodTable);
                        document.Add(paragraphTable2);

                        // ---- Totals ----
                        AddTotalRow(document, "Sub Total", taxableTotal, boldFont10, Font10, lightTint, false, borderGray);

                        if (isIGST)
                            AddTotalRow(document, "IGST Amount", igstTotal, boldFont10, Font10, lightTint, false, borderGray);
                        else
                        {
                            AddTotalRow(document, "CGST Amount", cgstTotal, boldFont10, Font10, lightTint, false, borderGray);
                            AddTotalRow(document, "SGST Amount", sgstTotal, boldFont10, Font10, lightTint, false, borderGray);
                        }

                        Font grandLabelWhite = FontFactory.GetFont("Arial", 11, Font.BOLD, BaseColor.WHITE);
                        Font grandValWhite = FontFactory.GetFont("Arial", 11, Font.BOLD, BaseColor.WHITE);
                        AddTotalRow(document, "Grand Total", grandTotal, grandLabelWhite, grandValWhite, brand, true, borderGray);
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
      SELECT ID, ProformaNo, ProformaDate, ReverseCharge, State, CompanyName,
               CompanyCode, Address, cgstin as GSTNO, BillState, TotalAmtBeforeTax, TotalAmtAfterTax
        FROM [WLSPLCRM].[stswlspl].[tblProformaMain]
        WHERE ID = @ID;

        SELECT ID, ProformaID, ProductDescription, SACCode, Qty, Rate, Amount, TaxableValue,
               CGSTRate, CGSTAmt, SGSTRate, SGSTAmt, IGSTRate, IGSTAmt, Total
        FROM [WLSPLCRM].[stswlspl].[tblProformaDetails]
        WHERE ProformaID = @ID
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


    }
}
