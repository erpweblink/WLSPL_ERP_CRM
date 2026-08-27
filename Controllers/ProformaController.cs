using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Data;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.repository;


namespace WEBLINK_CRM.Controllers
{
    [Authorize]
    public class ProformaController : Controller
    {
        private readonly IProforma objProforma;
        public ProformaController(IProforma proforma)
        {
            objProforma = proforma;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string pageSize = "10";
            var list = await objProforma.GetProformaList(pageSize);
            return View(list);

        }

        [HttpPost]
        public async Task<IActionResult> GetDetailsById(string ID)
        {
            try
            {
                var list = await objProforma.GetDetailsById(ID);

                return Json(new { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int ID)
        {
            try
            {
                if (ID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Proforma ID."
                    });
                }

                var result = await objProforma.Delete(ID);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Proforma deleted successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Proforma could not be deleted."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] VM_Proforma DataList)
        {
            try
            {

                var loginId = HttpContext.Session.GetString("EmployeeId");

                if (DataList == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }
                DataList.CreatedBy = loginId;
                int ID = await objProforma.Save(DataList);

                return Json(new
                {
                    success = true,
                    ID = ID,
                    Message = "Proforma saved successfully."
                });


            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        public async Task<IActionResult> GetCompany(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetCompanyList(Status);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetCompanyByCode(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetCompanyByCode(ID);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public async Task<IActionResult> GetProformaDataById(string ID)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var loginId = HttpContext.Session.GetString("EmployeeId");

                if (string.IsNullOrEmpty(loginId))
                {
                    return RedirectToAction("Login", "Login");
                }

                // IMPORTANT: await async methods
                var list = await objProforma.GetProformaById(ID);
                var Dtlslist = await objProforma.GetDetailsById(ID);

                if (list == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Work Order not found"
                    });
                }
                var result = new
                {
                    proformaHdr = list,
                    proformaDtls = Dtlslist
                };

                return Json(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    Success = false,
                    Message = e.Message
                });
            }
        }

        public async Task<IActionResult> GetState(string Status)
        {
            try
            {
                if (HttpContext.Session.GetString("EmployeeId") != null)
                {
                    var loginId = HttpContext.Session.GetString("EmployeeId");
                    if (loginId != null)
                    {
                        var list = await objProforma.GetStateList(Status);
                        if (list != null)
                        {
                            return Json(new { Success = true, Data = list });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Data Not Found.......!" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewPDF(string ID)
        {
            try
            {
               
                // Get Proforma Header
                var list = await objProforma.GetProformaById(ID);

                // Get Proforma Details
                var Dtlslist = await objProforma.GetDetailsById(ID);
              
                var data = list;

                string html = $@"
<!DOCTYPE html>
<html>
<head>

<meta charset='utf-8' />

<style>

    * {{
        box-sizing: border-box;
    }}

    body {{
        font-family: Arial, Helvetica, sans-serif;
        font-size: 11px;
        color: #222;
        margin: 0;
        padding: 0;
    }}

    .page {{
        width: 210mm;
        min-height: 297mm;
        padding: 12mm;
        margin: auto;
    }}

    .header {{
        display: flex;
        justify-content: space-between;
        border-bottom: 2px solid #222;
        padding-bottom: 10px;
    }}

    .company {{
        width: 60%;
    }}

    .company-name {{
        font-size: 20px;
        font-weight: bold;
        margin-bottom: 5px;
    }}

    .invoice-title {{
        width: 40%;
        text-align: right;
    }}

    .invoice-title h1 {{
        margin: 0;
        font-size: 20px;
    }}

    .info {{
        margin-top: 15px;
        display: flex;
        justify-content: space-between;
    }}

    .box {{
        width: 48%;
        border: 1px solid #999;
        padding: 8px;
        min-height: 80px;
    }}

    .box-title {{
        font-weight: bold;
        margin-bottom: 5px;
        font-size: 12px;
    }}

    table {{
        width: 100%;
        border-collapse: collapse;
    }}

    .items {{
        margin-top: 15px;
    }}

    .items th {{
        background: #eeeeee;
        text-align: center;
        font-weight: bold;
    }}

    .items th,
    .items td {{
        border: 1px solid #555;
        padding: 6px;
    }}

    .center {{
        text-align: center;
    }}

    .right {{
        text-align: right;
    }}

    .summary {{
        width: 40%;
        margin-left: auto;
        margin-top: 10px;
    }}

    .summary td {{
        border: 1px solid #999;
        padding: 6px;
    }}

    .grand-total {{
        font-weight: bold;
        font-size: 13px;
        background: #eeeeee;
    }}

    .terms {{
        margin-top: 20px;
    }}

    .signature {{
        margin-top: 60px;
        display: flex;
        justify-content: space-between;
    }}

    .signature div {{
        width: 30%;
        text-align: center;
    }}

    .footer {{
        margin-top: 30px;
        border-top: 1px solid #999;
        padding-top: 8px;
        text-align: center;
        font-size: 9px;
    }}

</style>

</head>

<body>

<div class='page'>

    <!-- HEADER -->

    <div class='header'>

        <div class='company'>

            <div class='company-name'>
                {data.CompanyName}
            </div>

            <div>
                {data.Address}
                <br />

                <strong>GSTIN:</strong>
                {data.GSTNO}

                <br />

                <strong>State:</strong>
                {data.State}
            </div>

        </div>


        <div class='invoice-title'>

            <h1>PROFORMA INVOICE</h1>

            <br />

            <strong>Proforma No:</strong>
            {data.ProformaNo}

            <br />

            <strong>Date:</strong>
            {data.ProformaDate:dd-MM-yyyy}

        </div>

    </div>


    <!-- CUSTOMER -->

    <div class='info'>

        <div class='box'>

            <div class='box-title'>
                BILL TO
            </div>

            <strong>{data.CompanyName}</strong>

            <br />

            {data.Address}

            <br />

            <strong>GSTIN:</strong>
            {data.GSTNO}

        </div>


        <div class='box'>

            <div class='box-title'>
                PROFORMA DETAILS
            </div>

            <strong>Proforma No:</strong>
            {data.ProformaNo}

            <br />

            <strong>Date:</strong>
            {data.ProformaDate:dd-MM-yyyy}

            <br />

            <strong>Place of Supply:</strong>
            {data.State}

        </div>

    </div>


    <!-- DETAILS -->

    <table class='items'>

        <thead>

            <tr>

                <th style='width:5%'>Sr.</th>

                <th style='width:35%'>
                    Description
                </th>

                <th style='width:12%'>
                    SAC Code
                </th>

                <th style='width:10%'>
                    Qty
                </th>

                <th style='width:13%'>
                    Rate
                </th>

                <th style='width:15%'>
                    Amount
                </th>

            </tr>

        </thead>

        <tbody>";

                // -------------------------------------------------------
                // DETAIL ROWS
                // -------------------------------------------------------

                int sr = 1;

                if (Dtlslist != null)
                {
                    foreach (var item in Dtlslist)
                    {
                        html += $@"

                <tr>

                    <td class='center'>
                        {sr}
                    </td>

                    <td>
                        {item.ProductDescription}
                    </td>

                    <td class='center'>
                        {item.SACCode}
                    </td>

                    <td class='right'>
                        {item.Qty:N2}
                    </td>

                    <td class='right'>
                        ₹ {item.Rate:N2}
                    </td>

                    <td class='right'>
                        ₹ {item.Amount:N2}
                    </td>

                </tr>";

                        sr++;
                    }
                }

                // -------------------------------------------------------
                // SUMMARY
                // -------------------------------------------------------

                html += $@"

        </tbody>

    </table>


    <table class='summary'>

        <tr>

            <td>
                Sub Total
            </td>

            <td class='right'>
                ₹ {data.TotalAmtBeforeTax:N2}
            </td>

        </tr>

        <tr>

            <td>
                CGST
            </td>

            <td class='right'>
                ₹ {data.TotalAmtBeforeTax:N2}
            </td>

        </tr>

        <tr>

            <td>
                SGST
            </td>

            <td class='right'>
                ₹ {data.TotalAmtBeforeTax:N2}
            </td>

        </tr>

        <tr>

            <td>
                IGST
            </td>

            <td class='right'>
                ₹ {data.TotalAmtBeforeTax:N2}
            </td>

        </tr>

        <tr class='grand-total'>

            <td>
                GRAND TOTAL
            </td>

            <td class='right'>
                ₹ {data.TotalAmtAfterTax:N2}
            </td>

        </tr>

    </table>


    <!-- TERMS -->

    <div class='terms'>

        <strong>Terms & Conditions</strong>

        <ol>

            <li>
                This is a Proforma Invoice and is not a tax invoice.
            </li>

            <li>
                Payment shall be made as per agreed terms.
            </li>

            <li>
                Prices are subject to applicable taxes.
            </li>

        </ol>

    </div>


    <!-- SIGNATURE -->

    <div class='signature'>

        <div>
            Prepared By
        </div>

        <div>
            Checked By
        </div>

        <div>
            Authorized Signatory
        </div>

    </div>


    <div class='footer'>

        This is a computer generated Proforma Invoice.

    </div>

</div>

</body>

</html>";


                // -------------------------------------------------------
                // PLAYWRIGHT PDF
                // -------------------------------------------------------

                using var playwright = await Playwright.CreateAsync();

                await using var browser =
                    await playwright.Chromium.LaunchAsync(
                        new BrowserTypeLaunchOptions
                        {
                            Headless = true
                        });

                var page = await browser.NewPageAsync();

                await page.SetContentAsync(
                    html,
                    new PageSetContentOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle
                    });


                var pdf = await page.PdfAsync(
                    new PagePdfOptions
                    {
                        Format = "A4",
                        PrintBackground = true,

                        Margin = new Margin
                        {
                            Top = "10mm",
                            Bottom = "10mm",
                            Left = "10mm",
                            Right = "10mm"
                        }
                    });


                return File(
                    pdf,
                    "application/pdf",
                    $"Proforma_{data.ProformaNo}.pdf"
                );
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
