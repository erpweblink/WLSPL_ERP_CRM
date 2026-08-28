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
                    "SP_WorkOrder",
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
                    "SP_WorkOrder",
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
                    parameters.Add("@@CreatedBy", model.CreatedBy);

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
                Document document = new Document(iTextSharp.text.PageSize.A4, 18f, 18f, 18f, 18f);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                BaseColor sectionBlue = new BaseColor(197, 217, 241);
                BaseColor borderBlack = new BaseColor(0, 0, 0);
                BaseColor textDark = new BaseColor(20, 20, 20);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED);

                Font titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD, textDark);
                Font addressFont = FontFactory.GetFont("Arial", 9.5f, Font.BOLD, textDark);
                Font gstinFont = FontFactory.GetFont("Arial", 12, Font.BOLD, textDark);
                Font sectionFont = FontFactory.GetFont("Arial", 12, Font.BOLD, textDark);
                Font labelFont = FontFactory.GetFont("Arial", 10, Font.BOLD, textDark);
                Font valueFont = FontFactory.GetFont("Arial", 10, Font.NORMAL, textDark);
                Font tableHeaderFont = FontFactory.GetFont("Arial", 8.5f, Font.BOLD, textDark);
                Font tableBodyFont = FontFactory.GetFont("Arial", 8.5f, Font.NORMAL, textDark);
                Font totalLabelFont = FontFactory.GetFont("Arial", 14, Font.BOLD, textDark);
                Font wordsFont = FontFactory.GetFont("Arial", 9.5f, Font.BOLD, textDark);
                Font termsFont = FontFactory.GetFont("Arial", 8.5f, Font.NORMAL, textDark);

                // ================= TOP: Logo + Company Header =================
                PdfPTable headerTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingAfter = 0f };
                headerTable.SetWidths(new float[] { 140f, 420f });

                // Logo cell
                PdfPCell logoCell = new PdfPCell { BorderColor = borderBlack, BorderWidth = 0.75f, Padding = 10f, VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                string logoPath = Path.Combine(_env.WebRootPath, "assets", "images", "WLSPL_MAIN_LOGO.png");
                if (File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(120, 65);
                    logo.Alignment = Element.ALIGN_CENTER;
                    logoCell.AddElement(logo);
                }
                headerTable.AddCell(logoCell);

                // Company details cell
                PdfPCell companyCell = new PdfPCell { BorderColor = borderBlack, BorderWidth = 0.75f, Padding = 8f, VerticalAlignment = Element.ALIGN_MIDDLE };
                Paragraph companyPara = new Paragraph();
                companyPara.Add(new Chunk("Web Link Services Pvt. Ltd.\n", titleFont) { });
                companyPara.Alignment = Element.ALIGN_CENTER;
                Paragraph addrPara = new Paragraph("12th Floor, Vintage 21 Commercial Complex, Above Max\nShowroom, P.K. Chowk, Pimple Saudagar, Pune, Maharashtra\nMobile :- 8421060192 | info@weblinkservices.net", addressFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                companyCell.AddElement(companyPara);
                companyCell.AddElement(addrPara);
                headerTable.AddCell(companyCell);

                document.Add(headerTable);

                // ================= GSTIN Bar =================
                PdfPTable gstinTable = new PdfPTable(1) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                gstinTable.AddCell(new PdfPCell(new Phrase("GSTIN : 27AABCW8929J2ZP", gstinFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BorderColor = borderBlack,
                    BorderWidth = 0.75f,
                    PaddingTop = 6f,
                    PaddingBottom = 6f
                });
                document.Add(gstinTable);

                // ================= "Proforma (This is not Tax Invoice)" bar =================
                document.Add(SectionBar("Proforma", sectionFont, sectionBlue, borderBlack));

                // **Fetching Data via VM_Proforma**
                VM_Proforma vm = GetProformaData(id);

                if (vm != null && vm.ID != null)
                {
                    string ProformaNo = vm.ProformaNo ?? "N/A";
                    string ProformaDate = vm.ProformaDate.HasValue ? vm.ProformaDate.Value.ToString("dd/MM/yyyy") : "N/A";
                    string ReverseCharge = string.IsNullOrWhiteSpace(vm.ReverseCharge) ? "N" : vm.ReverseCharge;
                    string CompanyState = vm.State ?? "N/A";
                    string CompanyName = vm.CompanyName ?? "N/A";
                    string Address = vm.Address ?? "N/A";
                    string Gstno = vm.GSTNO ?? "N/A";
                    string BillState = vm.BillState ?? "N/A";

                    // ---- Proforma No / Date / Reverse Charge / State ----
                    PdfPTable infoTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    infoTable.SetWidths(new float[] { 260f, 300f });

                    infoTable.AddCell(LabelCell("Proforma No :", labelFont, borderBlack));
                    infoTable.AddCell(ValueCell(ProformaNo, valueFont, borderBlack));
                    infoTable.AddCell(LabelCell("Proforma Date :", labelFont, borderBlack));
                    infoTable.AddCell(ValueCell(ProformaDate, valueFont, borderBlack));
                    infoTable.AddCell(LabelCell("Reverse Charge (Y/N) :", labelFont, borderBlack));
                    infoTable.AddCell(ValueCell(ReverseCharge, valueFont, borderBlack));
                    infoTable.AddCell(LabelCell("State :", labelFont, borderBlack));
                    infoTable.AddCell(ValueCell(CompanyState, valueFont, borderBlack));

                    document.Add(infoTable);

                    // ================= "Proforma To Party" bar =================
                    document.Add(SectionBar("Proforma To Party", sectionFont, sectionBlue, borderBlack));

                    // ---- Party details ----
                    PdfPTable partyTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    partyTable.SetWidths(new float[] { 260f, 300f });

                    partyTable.AddCell(LabelCell("Company Name :", labelFont, borderBlack));
                    partyTable.AddCell(ValueCell(CompanyName, valueFont, borderBlack));
                    partyTable.AddCell(LabelCell("Address :", labelFont, borderBlack));
                    partyTable.AddCell(ValueCell(Address, valueFont, borderBlack));
                    partyTable.AddCell(LabelCell("GSTIN :", labelFont, borderBlack));
                    partyTable.AddCell(ValueCell(Gstno, valueFont, borderBlack));
                    partyTable.AddCell(LabelCell("State :", labelFont, borderBlack));
                    partyTable.AddCell(ValueCell(BillState, valueFont, borderBlack));

                    document.Add(partyTable);

                    // ================= Product Details Table (two-tier header) =================
                    bool isIGST = !string.Equals((CompanyState ?? "").Trim(), (BillState ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

                    double amountTotal = 0, taxableTotal = 0, cgstTotal = 0, sgstTotal = 0, igstTotal = 0, grandTotal = 0;

                    PdfPTable prodTable;
                    if (isIGST)
                    {
                        // Sr, Desc, SAC, Qty, Rate, Amount, TaxableVal, IGST%(Rate,Amt), Total = 10 cols
                        prodTable = new PdfPTable(10) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                        prodTable.SetWidths(new float[] { 5f, 20f, 8f, 5f, 7f, 7f, 8f, 6f, 7f, 9f });
                    }
                    else
                    {
                        // Sr, Desc, SAC, Qty, Rate, Amount, TaxableVal, CGST%(Rate,Amt), SGST%(Rate,Amt), Total = 12 cols
                        prodTable = new PdfPTable(12) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                        prodTable.SetWidths(new float[] { 5f, 17f, 7f, 4f, 6f, 6f, 7f, 5f, 6f, 5f, 6f, 8f });
                    }

                    PdfPCell HCell(string text, int rowspan = 1, int colspan = 1) => new PdfPCell(new Phrase(text, tableHeaderFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = sectionBlue,
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        Rowspan = rowspan,
                        Colspan = colspan,
                        PaddingTop = 5f,
                        PaddingBottom = 5f,
                        MinimumHeight = 22f
                    };

                    // Row 1
                    prodTable.AddCell(HCell("Sr.\nNo.", 2));
                    prodTable.AddCell(HCell("Description", 2));
                    prodTable.AddCell(HCell("SAC\nCode", 2));
                    prodTable.AddCell(HCell("Qty", 2));
                    prodTable.AddCell(HCell("Rate", 2));
                    prodTable.AddCell(HCell("Amount", 2));
                    prodTable.AddCell(HCell("Taxable\nValue", 2));

                    if (isIGST)
                    {
                        prodTable.AddCell(HCell("IGST %", 1, 2));
                    }
                    else
                    {
                        prodTable.AddCell(HCell("CGST %", 1, 2));
                        prodTable.AddCell(HCell("SGST %", 1, 2));
                    }
                    prodTable.AddCell(HCell("Total", 2));

                    // Row 2 (only the split Rate/Amount sub-columns)
                    if (isIGST)
                    {
                        prodTable.AddCell(HCell("Rate"));
                        prodTable.AddCell(HCell("Amount"));
                    }
                    else
                    {
                        prodTable.AddCell(HCell("Rate"));
                        prodTable.AddCell(HCell("Amount"));
                        prodTable.AddCell(HCell("Rate"));
                        prodTable.AddCell(HCell("Amount"));
                    }

                    PdfPCell BCell(string text, int align = Element.ALIGN_CENTER) => new PdfPCell(new Phrase(text ?? "", tableBodyFont))
                    {
                        HorizontalAlignment = align,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        PaddingTop = 5f,
                        PaddingBottom = 5f,
                        MinimumHeight = 26f
                    };

                    int rowid = 1;
                    if (vm.objtblProformaDtl != null)
                    {
                        foreach (var d in vm.objtblProformaDtl)
                        {
                            double amt = ParseD(d.Amount);
                            double taxableVal = ParseD(d.TaxableValue);
                            double lineTotal = ParseD(d.Total);

                            prodTable.AddCell(BCell(rowid.ToString()));
                            prodTable.AddCell(BCell(d.ProductDescription, Element.ALIGN_LEFT));
                            prodTable.AddCell(BCell(d.SACCode));
                            prodTable.AddCell(BCell(d.Qty));
                            prodTable.AddCell(BCell(d.Rate));
                            prodTable.AddCell(BCell(amt.ToString("0.##")));
                            prodTable.AddCell(BCell(taxableVal.ToString("0.##")));

                            if (isIGST)
                            {
                                prodTable.AddCell(BCell(d.IGSTRate));
                                prodTable.AddCell(BCell(ParseD(d.IGSTAmt).ToString("0.##")));
                                igstTotal += ParseD(d.IGSTAmt);
                            }
                            else
                            {
                                prodTable.AddCell(BCell(d.CGSTRate));
                                prodTable.AddCell(BCell(ParseD(d.CGSTAmt).ToString("0.##")));
                                prodTable.AddCell(BCell(d.SGSTRate));
                                prodTable.AddCell(BCell(ParseD(d.SGSTAmt).ToString("0.##")));
                                cgstTotal += ParseD(d.CGSTAmt);
                                sgstTotal += ParseD(d.SGSTAmt);
                            }

                            prodTable.AddCell(BCell(lineTotal.ToString("0.##")));

                            amountTotal += amt;
                            taxableTotal += taxableVal;
                            grandTotal += lineTotal;
                            rowid++;
                        }
                    }

                    // ---- Total row (bold, spans Sr.No + Description) ----
                    Font totalRowFont = FontFactory.GetFont("Arial", 9.5f, Font.BOLD, textDark);
                    PdfPCell TCell(string text, int colspan = 1) => new PdfPCell(new Phrase(text, totalRowFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = sectionBlue,
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        Colspan = colspan,
                        PaddingTop = 6f,
                        PaddingBottom = 6f,
                        MinimumHeight = 26f
                    };

                    prodTable.AddCell(TCell("Total", 3));
                    prodTable.AddCell(TCell("")); // Qty total left blank (or sum if needed)
                    prodTable.AddCell(TCell(""));
                    prodTable.AddCell(TCell(amountTotal.ToString("0.##")));
                    prodTable.AddCell(TCell(taxableTotal.ToString("0.##")));

                    if (isIGST)
                    {
                        prodTable.AddCell(TCell(""));
                        prodTable.AddCell(TCell(igstTotal.ToString("0.##")));
                    }
                    else
                    {
                        prodTable.AddCell(TCell(""));
                        prodTable.AddCell(TCell(cgstTotal.ToString("0.##")));
                        prodTable.AddCell(TCell(""));
                        prodTable.AddCell(TCell(sgstTotal.ToString("0.##")));
                    }
                    prodTable.AddCell(TCell(grandTotal.ToString("0.##")));

                    document.Add(prodTable);

                    // ================= Amount in Words + Tax Summary =================
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
                    string AmountInWords = Dts.Rows.Count > 0 ? (Dts.Rows[0]["AmountInWords"]?.ToString() ?? "N/A") : "N/A";

                    PdfPTable summaryTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    summaryTable.SetWidths(new float[] { 350f, 210f });

                    // Left column: words (top) + bank details (bottom) stacked as inner table
                    PdfPCell leftCell = new PdfPCell { BorderColor = borderBlack, BorderWidth = 0.5f, Padding = 0f };
                    PdfPTable leftInner = new PdfPTable(1) { WidthPercentage = 100 };

                    leftInner.AddCell(new PdfPCell(new Phrase(AmountInWords, wordsFont))
                    {
                        BackgroundColor = sectionBlue,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 6f,
                        PaddingBottom = 6f,
                        PaddingLeft = 6f,
                        MinimumHeight = 24f
                    });

                    leftInner.AddCell(new PdfPCell(new Phrase(
                        "Bank Details\n\nBank A/C :- 916020085136854\n\nBank IFSC :- UTIB0001641\n\nAxis Bank Ltd- Rahatani Branch, Pune",
                        valueFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10f,
                        PaddingBottom = 10f,
                        PaddingLeft = 8f
                    });

                    leftInner.AddCell(new PdfPCell(new Phrase("Remark :", labelFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10f,
                        PaddingBottom = 10f,
                        PaddingLeft = 8f
                    });

                    leftCell.AddElement(leftInner);
                    summaryTable.AddCell(leftCell);

                    // Right column: tax summary rows
                    PdfPCell rightCell = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 0f };
                    PdfPTable rightInner = new PdfPTable(2) { WidthPercentage = 100 };
                    rightInner.SetWidths(new float[] { 130f, 80f });

                    void TaxRow(string label, string value, bool shaded = false)
                    {
                        rightInner.AddCell(new PdfPCell(new Phrase(label, labelFont))
                        {
                            BorderColor = borderBlack,
                            BorderWidth = 0.5f,
                            BackgroundColor = shaded ? sectionBlue : BaseColor.WHITE,
                            PaddingTop = 5f,
                            PaddingBottom = 5f,
                            PaddingLeft = 6f
                        });
                        rightInner.AddCell(new PdfPCell(new Phrase(value, valueFont))
                        {
                            BorderColor = borderBlack,
                            BorderWidth = 0.5f,
                            BackgroundColor = shaded ? sectionBlue : BaseColor.WHITE,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            PaddingTop = 5f,
                            PaddingBottom = 5f
                        });
                    }

                    TaxRow("Total Amount Before Tax", taxableTotal.ToString("0.##"));
                    if (isIGST)
                    {
                        TaxRow("Add: IGST", igstTotal.ToString("0.##"));
                    }
                    else
                    {
                        TaxRow("Add: CGST [9%]", cgstTotal.ToString("0.##"));
                        TaxRow("Add: SGST [9%]", sgstTotal.ToString("0.##"));
                    }
                    TaxRow("Total Tax Amount", (isIGST ? igstTotal : cgstTotal + sgstTotal).ToString("0.##"));
                    TaxRow("Total Amount After Tax", grandTotal.ToString("0.##"));
                    TaxRow("GST On Reverse Charge", "0.00", true);

                    rightCell.AddElement(rightInner);
                    summaryTable.AddCell(rightCell);

                    document.Add(summaryTable);

                    // ================= Terms & Conditions + Signature =================
                    PdfPTable footerTable = new PdfPTable(2) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
                    footerTable.SetWidths(new float[] { 350f, 210f });

                    PdfPCell termsCell = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 0f };
                    PdfPTable termsInner = new PdfPTable(1) { WidthPercentage = 100 };

                    termsInner.AddCell(new PdfPCell(new Phrase("Terms & Conditions", labelFont))
                    {
                        BackgroundColor = sectionBlue,
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        PaddingTop = 5f,
                        PaddingBottom = 5f
                    });

                    string terms =
                        "1. Website Data provide by client in soft copy via mail. We will not copy data\n" +
                        "    from any portal. [NEED UNIQUE DATA]\n" +
                        "    Send data for website on design@weblinkservices.net\n" +
                        "2. SEO will take minimum 90 days after hosting of website [UNIQUE DATA\n" +
                        "    WILL GIVE BEST RESULT]\n" +
                        "3. Payment will not refund at any circumstances.\n" +
                        "4. We are not committing any Enquiry or business from given services to you.\n" +
                        "5. Cheque Bounce Charges 500+GST.\n" +
                        "6. TAX INVOICE will be generated after full payment of deal.\n" +
                        "7. We do not provide website source code files.";

                    termsInner.AddCell(new PdfPCell(new Phrase(terms, termsFont))
                    {
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        PaddingTop = 8f,
                        PaddingBottom = 8f,
                        PaddingLeft = 6f,
                        PaddingRight = 6f
                    });

                    termsCell.AddElement(termsInner);
                    footerTable.AddCell(termsCell);

                    PdfPCell signCell = new PdfPCell(new Phrase(
                        "For,\n\nWeb Link Services Pvt. Ltd.\n\n\n\nAuthorised Signatory",
                        labelFont))
                    {
                        BorderColor = borderBlack,
                        BorderWidth = 0.5f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_TOP,
                        Padding = 10f
                    };
                    footerTable.AddCell(signCell);

                    document.Add(footerTable);
                }

                document.Close();
                writer.Close();

                return stream.ToArray();
            }
        }

        private PdfPTable SectionBar(string text, Font font, BaseColor bg, BaseColor border)
        {
            PdfPTable t = new PdfPTable(1) { TotalWidth = 560f, LockedWidth = true, SpacingBefore = 0f, SpacingAfter = 0f };
            t.AddCell(new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = bg,
                BorderColor = border,
                BorderWidth = 0.75f,
                PaddingTop = 6f,
                PaddingBottom = 6f
            });
            return t;
        }

 
        private PdfPCell LabelCell(string text, Font font, BaseColor border) => new PdfPCell(new Phrase(text, font))
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            BorderColor = border,
            BorderWidth = 0.5f,
            PaddingTop = 6f,
            PaddingBottom = 6f,
            PaddingLeft = 8f,
            MinimumHeight = 24f
        };

      
        private PdfPCell ValueCell(string text, Font font, BaseColor border) => new PdfPCell(new Phrase(text, font))
        {
            HorizontalAlignment = Element.ALIGN_CENTER,
            BorderColor = border,
            BorderWidth = 0.5f,
            PaddingTop = 6f,
            PaddingBottom = 6f,
            MinimumHeight = 24f
        };

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
        WHERE ID = 5097;

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
