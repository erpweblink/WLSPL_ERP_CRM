var GetQuotationForm = function () {

    // =====================================================
    // VARIABLES
    // =====================================================
    var ID = window.location.pathname.split('/').pop();
    var IsCreate = $("#hdnCreate").val();
    var Companytext = "";

    // =====================================================
    // CHECK AUTHORIZATION
    // =====================================================
    if (!ID) {
        if (IsCreate == "F") {
            window.location.href = "/Login/LogIn";
            return;
        }
    }

    // =====================================================
    // BIND STATE LIST
    // =====================================================
    var BindStateList = function () {
        $.ajax({
            url: "/Quotation/GetState",
            data: { Status: "1" },
            type: "POST",
            cache: false,
            success: function (response) {
                if (response.success === true) {
                    var html = "<option value=''>-- Select State --</option>";
                    var users = response.data || [];

                    $.each(users, function (key, data) {
                        html += "<option value='" + (data.Name || "") + "'>" + (data.Name || "") + "</option>";
                    });

                    $("#ddlBillState").html(html);
                }
                else {
                    showToast(response.message || "State data not found.", "error");
                }
            },
            error: function (xhr) {
                console.error("Get State Error:", xhr.responseText);
                showToast("Unable to load State list.", "error");
            }
        });
    };

    // =====================================================
    // BIND COMPANY LIST
    // =====================================================
    var BindCompanyList = function () {
        $.ajax({
            url: "/Quotation/GetCompany",
            data: { Status: "1" },
            type: "POST",
            cache: false,
            success: function (response) {
                if (response.success === true) {
                    var users = response.data || [];
                    var html = "<option value=''>-- Select Company Name --</option>";

                    $.each(users, function (key, data) {
                        html += "<option value='" + (data.ID || "") + "'>" + (data.Name || "") + "</option>";
                    });

                    $("#ddlCompanyname").html(html);

                    // SELECT COMPANY DURING EDIT
                    if (Companytext && Companytext.trim() !== "") {
                        var company = users.find(function (x) {
                            return ((x.Name || "").toLowerCase().trim()) === Companytext.toLowerCase().trim();
                        });

                        if (company) {
                            $("#ddlCompanyname").val(company.ID).trigger("change");
                        }
                    }
                }
                else {
                    showToast(response.message || "Company data not found.", "error");
                }
            },
            error: function (xhr) {
                console.error("Get Company Error:", xhr.responseText);
                showToast("Unable to load Company list.", "error");
            }
        });
    };

    // =====================================================
    // COMPANY CHANGE - LOAD COMPANY DETAILS
    // =====================================================
    $("#ddlCompanyname")
        .off("change")
        .on("change", function () {
            var companyID = $(this).val();

            if (!companyID) {
                return;
            }

            $.ajax({
                url: "/Quotation/GetCompanyByCode",
                data: { ID: companyID },
                type: "POST",
                cache: false,
                success: function (response) {
                    if (response.success === true) {
                        var result = response.data && response.data.length > 0 ? response.data[0] : null;

                        if (!result) {
                            return;
                        }

                        // Only populate on CREATE, not on EDIT
                        if (ID != null && ID != undefined && ID != "Create") {
                            return;
                        }

                        $("#txtAddress").val(result.address || "");
                        $("#txtGSTNo").val(result.gstno == null ? "NA" : result.gstno);
                        $("#txtEmailID").val(result.email || "");
                        $("#ddlBillState").val(result.State || "").trigger("change");

                        var gstNo = result.gstno == null ? "NA" : result.gstno;

                        if (gstNo !== "NA" && gstNo.length >= 2) {
                            var stateCode = gstNo.substring(0, 2);

                            // Maharashtra
                            if (stateCode === "27") {
                                $("#txtCGST").val("9");
                                $("#txtSGST").val("9");
                                $("#txtIGST").val("0");
                            }
                            else {
                                // Other State
                                $("#txtCGST").val("0");
                                $("#txtSGST").val("0");
                                $("#txtIGST").val("18");
                            }
                        }
                    }
                    else {
                        showToast(response.message || "Unable to load Company details.", "error");
                    }
                },
                error: function (xhr) {
                    console.error("Company details error:", xhr.responseText);
                }
            });
        });

    // =====================================================
    // FORMAT DATE FUNCTION
    // =====================================================
    function formatDateToDDMMYYYY(dateValue) {
        if (!dateValue) {
            return "";
        }

        var date = new Date(dateValue);

        if (isNaN(date.getTime())) {
            return "";
        }

        var day = String(date.getDate()).padStart(2, "0");
        var month = String(date.getMonth() + 1).padStart(2, "0");
        var year = date.getFullYear();

        return year + "-" + month + "-" + day;
    }

    // =====================================================
    // CALCULATE DETAIL ROW
    // =====================================================
    function calculateDetailRow(row) {
        var qty = parseFloat(row.find(".qty").val()) || 0;
        var rate = parseFloat(row.find(".rate").val()) || 0;
        var cgstRate = parseFloat(row.find(".cgst-rate").val()) || 0;
        var sgstRate = parseFloat(row.find(".sgst-rate").val()) || 0;
        var igstRate = parseFloat(row.find(".igst-rate").val()) || 0;

        // BASIC AMOUNT
        var amount = qty * rate;

        // GST AMOUNT
        var cgstAmt = (amount * cgstRate) / 100;
        var sgstAmt = (amount * sgstRate) / 100;
        var igstAmt = (amount * igstRate) / 100;

        // FINAL TOTAL
        var allTotal = amount + cgstAmt + sgstAmt + igstAmt;

        // SET VALUES
        row.find(".cgst-amt").val(cgstAmt.toFixed(2));
        row.find(".sgst-amt").val(sgstAmt.toFixed(2));
        row.find(".igst-amt").val(igstAmt.toFixed(2));
        row.find(".amount").val(amount.toFixed(2));
        row.find(".all-total").val(allTotal.toFixed(2));
    }

    // =====================================================
    // CALCULATE GRAND TOTALS
    // =====================================================
    function calculateGrandTotals() {
        var basicAmount = 0;
        var gstAmount = 0;
        var finalAmount = 0;

        $("#tblDetailsBody .detail-row").each(function () {
            var row = $(this);
            var amount = parseFloat(row.find(".amount").val()) || 0;
            var cgst = parseFloat(row.find(".cgst-amt").val()) || 0;
            var sgst = parseFloat(row.find(".sgst-amt").val()) || 0;
            var igst = parseFloat(row.find(".igst-amt").val()) || 0;
            var total = parseFloat(row.find(".all-total").val()) || 0;

            basicAmount += amount;
            gstAmount += cgst + sgst + igst;
            finalAmount += total;
        });

        // SET HEADER TOTALS
        $("#txtTotalDealBasicAmount").val(basicAmount.toFixed(2));
        $("#txtTotalDealGSTAmount").val(gstAmount.toFixed(2));

        if ($("#txtTotalAmountBalance").length) {
            $("#txtTotalAmountBalance").val(finalAmount.toFixed(2));
        }
    }

    // =====================================================
    // ADD ROW BUTTON CLICK
    // =====================================================
    $("#btnAddRow")
        .off("click")
        .on("click", function () {
            addDetailRow(null);
        });

    // =====================================================
    // DELETE ROW
    // =====================================================
    $(document)
        .off("click", ".delete-row")
        .on("click", ".delete-row", function () {
            var rowCount = $("#tblDetailsBody .detail-row").length;

            // AT LEAST ONE ROW REQUIRED
            if (rowCount <= 1) {
                showToast("At least one Service is required.", "error");
                return;
            }

            $(this).closest("tr").remove();
            reIndexRows();
            calculateGrandTotals();
        });

    // =====================================================
    // INPUT CHANGE - RECALCULATE AMOUNTS
    // =====================================================
    $(document)
        .off("input", "#tblDetailsBody .qty, #tblDetailsBody .rate, #tblDetailsBody .cgst-rate, #tblDetailsBody .sgst-rate, #tblDetailsBody .igst-rate")
        .on("input", "#tblDetailsBody .qty, #tblDetailsBody .rate, #tblDetailsBody .cgst-rate, #tblDetailsBody .sgst-rate, #tblDetailsBody .igst-rate",
            function () {
                var row = $(this).closest("tr");

                // CGST ENTERED - Auto-sync SGST
                if ($(this).hasClass("cgst-rate")) {
                    var cgstValue = $(this).val();
                    row.find(".sgst-rate").val(cgstValue);
                    row.find(".igst-rate").val("0");
                }

                // SGST ENTERED - Auto-sync CGST
                if ($(this).hasClass("sgst-rate")) {
                    var sgstValue = $(this).val();
                    row.find(".cgst-rate").val(sgstValue);
                    row.find(".igst-rate").val("0");
                }

                // IGST ENTERED - Clear CGST/SGST
                if ($(this).hasClass("igst-rate")) {
                    var igstValue = parseFloat($(this).val()) || 0;
                    if (igstValue > 0) {
                        row.find(".cgst-rate").val("0");
                        row.find(".sgst-rate").val("0");
                    }
                }

                calculateDetailRow(row);
                calculateGrandTotals();
            }
        );

    // =====================================================
    // REINDEX ROWS
    // =====================================================
    function reIndexRows() {
        $("#tblDetailsBody .detail-row").each(function (index) {
            $(this).attr("data-index", index);
        });
    }

    // =====================================================
    // NUMERIC INPUT HANDLER
    // =====================================================
    $(document)
        .off("input", ".numeric")
        .on("input", ".numeric", function () {
            var val = $(this).val();
            val = val.replace(/[^0-9.]/g, "");

            // Only one decimal point
            var parts = val.split(".");
            if (parts.length > 2) {
                val = parts[0] + "." + parts.slice(1).join("");
            }

            $(this).val(val);
        });

    // =====================================================
    // ESCAPE HTML - PREVENT XSS
    // =====================================================
    function escapeHtml(value) {
        if (value === null || value === undefined) {
            return "";
        }

        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // =====================================================
    // BIND SERVICE LIST (GLOBAL)
    // =====================================================
    var Servicetext = "";
    var BindServiceList = function () {
        var Dept = "";

        $.ajax({
            url: "/WorkOrder/BindServiceList",
            data: { Dept: Dept },
            type: "POST",
            cache: false,
            success: function (response) {
                if (response.success === true) {
                    var users = response.data || [];
                    var html = "<option value=''>-- Select Service Name --</option>";

                    $.each(users, function (key, data) {
                        html += "<option value='" + escapeHtml(data.ID || data.Name) + "'>" + escapeHtml(data.Name) + "</option>";
                    });

                    // Update all service dropdowns
                    $(".ddlservice").html(html);

                    // Set selected service AFTER options are loaded
                    if (Servicetext) {
                        $(".ddlservice").val(Servicetext).trigger("change");
                    }
                }
            },
            error: function (xhr) {
                console.error("BindServiceList Error:", xhr.responseText);
            }
        });
    };

    // =====================================================
    // SERVICE DROPDOWN CHANGE - POPULATE ROW DETAILS
    // =====================================================
    $(document)
        .off("change", ".ddlservice")
        .on("change", ".ddlservice", function () {
            var selectedServiceID = $(this).val();
            var currentRow = $(this).closest("tr");

            if (!selectedServiceID || selectedServiceID.trim() === "") {
                // Clear fields if no selection
                currentRow.find(".service-name").val("");
                currentRow.find(".sac-code").val("00440013");
                return;
            }

            // AJAX call to fetch service details
            $.ajax({
                url: "/WorkOrder/GetServiceByID",
                data: { "ID": selectedServiceID },
                type: "POST",
                cache: false,
                dataType: "json",
                success: function (response) {
                    if (response.success === true && response.data && response.data.length > 0) {
                        var result = response.data[0];

                        // Populate description (handle both cases)
                        if (result.Description || result.description) {
                            currentRow.find(".service-name").val(result.Description || result.description);
                        }

                        // Populate SAC Code (handle both cases)
                        if (result.ServiceCode || result.serviceCode) {
                            currentRow.find(".sac-code").val(result.ServiceCode || result.serviceCode);
                        } else {
                            currentRow.find(".sac-code").val("00440013");
                        }

                        // Populate rate/price if available (handle both cases)
                        if (result.Price || result.price) {
                            currentRow.find(".rate").val(result.Price || result.price);
                        }

                        // Trigger calculations
                        calculateDetailRow(currentRow);
                        calculateGrandTotals();
                    }
                    else {
                        console.error("Service not found:", response);
                        showToast("Unable to load Service details.", "error");
                    }
                },
                error: function (xhr) {
                    console.error("Error loading Service:", xhr.responseText);
                    showToast("Error loading Service details.", "error");
                }
            });
        });

    // =====================================================
    // ADD DETAIL ROW
    // =====================================================
    function addDetailRow(item) {
        item = item || {};

        var serviceName = item.ServiceName || item.serviceName || "";
        Servicetext = serviceName;
        BindServiceList();
        var description = item.ProductDescription || item.productDescription || "";
        var sacCode = item.SACCode || item.sacCode || "00440013";
        var qty = item.Qty || item.qty || 1;
        var rate = item.Rate || item.rate || 0;

        var gstNo = ($("#txtGSTNo").val() || "").trim().toUpperCase();

        var cgstRate = 0;
        var sgstRate = 0;
        var igstRate = 0;

        // Determine GST based on GST Number
        if (gstNo !== "NA" && gstNo.length >= 2) {

            var stateCode = gstNo.substring(0, 2);

            // Maharashtra
            if (stateCode === "27") {
                cgstRate = 9;
                sgstRate = 9;
                igstRate = 0;
            }
            // Other State
            else {
                cgstRate = 0;
                sgstRate = 0;
                igstRate = 18;
            }
        }

        // If item already contains GST rates, use those values
        cgstRate = item.CGSTRate ?? item.cgstRate ?? cgstRate;
        sgstRate = item.SGSTRate ?? item.sgstRate ?? sgstRate;
        igstRate = item.IGSTRate ?? item.igstRate ?? igstRate;
        var cgstAmt = item.CGSTAmt || item.cgstAmt || 0;
        var sgstAmt = item.SGSTAmt || item.sgstAmt || 0;
        var igstAmt = item.IGSTAmt || item.igstAmt || 0;

        var amount = item.Amount || item.amount || 0;
        var total = item.Total || item.total || 0;

        var newRow = `
        <tr class="detail-row">
            <!-- ACTION: DELETE -->
            <td class="text-center">
                <button type="button"
                        class="btn btn-danger btn-sm delete-row"
                        title="Delete Row"
                        style="width:40px;">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

            <!-- SERVICE DROPDOWN -->
            <td class="text-center">
                <select class="form-control ddlservice" name="ddlService" style="width:100%;"></select>
            </td>

            <!-- DESCRIPTION/SERVICE NAME -->
            <td class="text-start">
                <textarea class="form-control description"
                          rows="1"
                          placeholder="Auto-filled from service selection">${escapeHtml(description)}</textarea>
            </td>

            <!-- SAC CODE -->
            <td class="text-center">
                <input type="text"
                       class="form-control sac-code"
                       value="${escapeHtml(sacCode)}"
                       maxlength="8"
                       minlength="6"
                       inputmode="numeric"
                       placeholder="SAC Code"
                       oninput="this.value=this.value.replace(/[^0-9]/g,'').slice(0,8);" />
            </td>

            <!-- QUANTITY -->
            <td class="text-center">
                <input type="number"
                       class="form-control qty"
                       value="${qty}"
                       min="1"
                       step="1"
                       placeholder="Qty" />
            </td>

            <!-- RATE -->
            <td class="text-center">
                <input type="number"
                       class="form-control rate"
                       value="${rate}"
                       min="0"
                       step="0.01"
                       placeholder="Rate" />
            </td>

            <!-- TAXABLE VALUE -->
            <td class="text-center">
                <input type="text"
                       class="form-control amount"
                       value="${parseFloat(amount || 0).toFixed(2)}"
                       readonly 
                       placeholder="0.00" />
            </td>

            <!-- CGST % -->
            <td class="text-center">
                <input type="number"
                       class="form-control cgst-rate"
                       value="${cgstRate}"
                       min="0"
                       step="0.01"
                       placeholder="CGST %" />
            </td>

            <!-- CGST AMOUNT -->
            <td class="text-center">
                <input type="text"
                       class="form-control cgst-amt"
                       value="${parseFloat(cgstAmt || 0).toFixed(2)}"
                       readonly 
                       placeholder="0.00" />
            </td>

            <!-- SGST % -->
            <td class="text-center">
                <input type="number"
                       class="form-control sgst-rate"
                       value="${sgstRate}"
                       min="0"
                       step="0.01"
                       placeholder="SGST %" />
            </td>

            <!-- SGST AMOUNT -->
            <td class="text-center">
                <input type="text"
                       class="form-control sgst-amt"
                       value="${parseFloat(sgstAmt || 0).toFixed(2)}"
                       readonly 
                       placeholder="0.00" />
            </td>

            <!-- IGST % -->
            <td class="text-center">
                <input type="number"
                       class="form-control igst-rate"
                       value="${igstRate}"
                       min="0"
                       step="0.01"
                       placeholder="IGST %" />
            </td>

            <!-- IGST AMOUNT -->
            <td class="text-center">
                <input type="text"
                       class="form-control igst-amt"
                       value="${parseFloat(igstAmt || 0).toFixed(2)}"
                       readonly 
                       placeholder="0.00" />
            </td>

            <!-- TOTAL AMOUNT -->
            <td class="text-center">
                <input type="text"
                       class="form-control all-total"
                       value="${parseFloat(total || 0).toFixed(2)}"
                       readonly 
                       placeholder="0.00" />
            </td>
        </tr>
    `;

        // Append row to table
        $("#tblDetailsBody").append(newRow);

        // Initialize the service dropdown for this row
        var newDropdown = $("#tblDetailsBody .detail-row").last().find(".ddlservice");
        if (newDropdown.length) {
            $.ajax({
                url: "/WorkOrder/BindServiceList",
                data: { Dept: "" },
                type: "POST",
                cache: false,
                success: function (response) {
                    if (response.success === true) {
                        var users = response.data || [];
                        var html = "<option value=''>-- Select Service Name --</option>";

                        $.each(users, function (key, data) {
                            html += "<option value='" + escapeHtml(data.ID || data.Name) + "'>" + escapeHtml(data.Name) + "</option>";
                        });

                        newDropdown.html(html);
                        newDropdown.select2({ selectOnClose: true, width: '100%' });
                    }
                }
            });
        }

        // Calculate newly added row
        var row = $("#tblDetailsBody .detail-row").last();
        calculateDetailRow(row);
        calculateGrandTotals();
    }

    // =====================================================
    // GET SERVICE DESCRIPTION LIST FOR SUBMIT
    // =====================================================
    function getServiceDescriptionList() {
        var ServiceDescriptionList = [];

        $("#tblDetailsBody .detail-row").each(function (index) {
            var row = $(this);

            var service = row.find(".ddlservice option:selected").text().trim();
            var serviceid = row.find(".ddlservice").val() || "";
            var description = row.find(".description").val() || "";
            var sacCode = row.find(".sac-code").val() || "00440013";
            var qty = parseFloat(row.find(".qty").val()) || 0;
            var rate = parseFloat(row.find(".rate").val()) || 0;

            var cgstRate = parseFloat(row.find(".cgst-rate").val()) || 0;
            var cgstAmt = parseFloat(row.find(".cgst-amt").val()) || 0;

            var sgstRate = parseFloat(row.find(".sgst-rate").val()) || 0;
            var sgstAmt = parseFloat(row.find(".sgst-amt").val()) || 0;

            var igstRate = parseFloat(row.find(".igst-rate").val()) || 0;
            var igstAmt = parseFloat(row.find(".igst-amt").val()) || 0;

            var amount = parseFloat(row.find(".amount").val()) || 0;
            var total = parseFloat(row.find(".all-total").val()) || 0;

            ServiceDescriptionList.push({
                ID: 0,
                QuotationID: parseInt($("#ID").val()) || 0,
                ServiceName: service,
                ServiceID: serviceid,
                ProductDescription: description,
                SACCode: sacCode,
                Qty: qty.toString(),
                Rate: rate.toString(),
                Amount: amount.toString(),
                TaxableValue: amount.toString(),
                CGSTRate: cgstRate.toString(),
                CGSTAmt: cgstAmt.toString(),
                SGSTRate: sgstRate.toString(),
                SGSTAmt: sgstAmt.toString(),
                IGSTRate: igstRate.toString(),
                IGSTAmt: igstAmt.toString(),
                Total: total.toString()
            });
        });

        return ServiceDescriptionList;
    }

    // =====================================================
    // FORM VALIDATOR & SUBMIT
    // =====================================================
    var formValidator = function () {
        $("#btnSubmit")
            .off("click")
            .on("click", function (e) {
                e.preventDefault();

                var errors = [];

                // VALIDATE: COMPANY
                if (!$("#ddlCompanyname").val()) {
                    errors.push("Please select Company Name.");
                    $("#ddlCompanyname").addClass("is-invalid");
                } else {
                    $("#ddlCompanyname").removeClass("is-invalid");
                }

                // VALIDATE: ADDRESS
                if (!$("#txtAddress").val().trim()) {
                    errors.push("Address is required.");
                    $("#txtAddress").addClass("is-invalid");
                } else {
                    $("#txtAddress").removeClass("is-invalid");
                }

                // VALIDATE: GST
                if (!$("#txtGSTNo").val().trim()) {
                    errors.push("GSTIN is required.");
                    $("#txtGSTNo").addClass("is-invalid");
                } else {
                    $("#txtGSTNo").removeClass("is-invalid");
                }

                // VALIDATE: STATE
                if (!$("#ddlBillState").val()) {
                    errors.push("Please select State.");
                    $("#ddlBillState").addClass("is-invalid");
                } else {
                    $("#ddlBillState").removeClass("is-invalid");
                }

                // VALIDATE: QUOTATION DATE
                if (!$("#txtQuotationDate").val()) {
                    errors.push("Quotation Date is required.");
                    $("#txtQuotationDate").addClass("is-invalid");
                } else {
                    $("#txtQuotationDate").removeClass("is-invalid");
                }

                // VALIDATE: AT LEAST ONE ROW
                var rowCount = $("#tblDetailsBody .detail-row").length;
                if (rowCount === 0) {
                    errors.push("Please add at least one Service.");
                    $("#tbldetails").addClass("table-invalid");
                } else {
                    $("#tbldetails").removeClass("table-invalid");
                }

                // VALIDATE: SERVICE NAME IN EACH ROW
                $("#tblDetailsBody .detail-row").each(function (index) {
                    var serviceName = $(this).find(".ddlservice").val().trim();

                    if (!serviceName) {
                        errors.push("Service Description is required in row " + (index + 1) + ".");
                        $(this).find(".ddlservice").addClass("is-invalid");
                    } else {
                        $(this).find(".ddlservice").removeClass("is-invalid");
                    }
                });

                // SHOW ALL ERRORS
                if (errors.length > 0) {
                    errors.forEach(function (msg) {
                        showToast(msg, "error");
                    });
                    return;
                }

                // GET DETAILS
                var ServiceDescriptionList = getServiceDescriptionList();

                // BUILD DATA OBJECT
                var DataList = {
                    ID: parseInt($("#ID").val()) || 0,
                    QuotationDate: $("#txtQuotationDate").val() || null,
                    ReverseCharge: $("#ddlReverseCharge").val() || "N",
                    CompanyName: $("#ddlCompanyname option:selected").text() || null,
                    CompanyCode: $("#ddlCompanyname").val() || null,
                    Address: $("#txtAddress").val() || null,
                    GSTNO: $("#txtGSTNo").val() || null,
                    BillState: $("#ddlBillState").val() || null,
                    State: $("#ddlBillState").val() || null,
                    TotalAmtBeforeTax: $("#txtTotalDealBasicAmount").val() || "0",
                    TotalAmtAfterTax: $("#txtTotalDealGSTAmount").val() || "0",
                    objtblQuotationDtl: ServiceDescriptionList
                };

                console.log("Quotation Data:", DataList);
                console.log("JSON:", JSON.stringify(DataList));

                // SHOW LOADER
                $("#loader").show();

                // SUBMIT TO SERVER
                $.ajax({
                    url: "/Quotation/CreateOrEdit",
                    type: "POST",
                    data: JSON.stringify(DataList),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    cache: false,
                    success: function (response) {
                        console.log("Response:", response);

                        if (response.success === true) {
                            showToast(response.message || "Quotation saved successfully.", "success");

                            setTimeout(function () {
                                window.location.href = "/Quotation/Index";
                            }, 1500);
                        }
                        else {
                            showToast(response.message || response.Message || "Unable to save Quotation.", "error");
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("AJAX Error:", error);
                        console.error("Response:", xhr.responseText);
                        showToast("Error saving Quotation. Please try again.", "error");
                    },
                    complete: function () {
                        $("#loader").hide();
                    }
                });
            });
    };

    // =====================================================
    // LOAD QUOTATION DATA (FOR EDIT MODE)
    // =====================================================
    var loadQuotationData = function () {
        if (ID == null || ID == undefined || ID == "") {
            return;
        }

        $.ajax({
            url: "/Quotation/GetQuotationDataById",
            data: { ID: ID },
            type: "POST",
            cache: false,
            success: function (response) {
                if (response.success !== true) {
                    showToast(response.message || "Unable to load Quotation.", "error");
                    return;
                }

                var result = response.data || {};
                var hdr = result.quotationHdr || {};
                var details = result.quotationDtls || [];

                // UPDATE MODE HEADER
                $("#btnSubmit").html("Update");
                $("#lblHeader").html("UPDATE Quotation");
                $("#ID").val(hdr.id || ID);

                // SET COMPANY
                Companytext = hdr.companyName || "";
                BindCompanyList();

                // SET OTHER HEADER VALUES
                $("#txtAddress").val(hdr.address || "");
                $("#txtGSTNo").val(hdr.gstno || "");
                $("#ddlReverseCharge").val(hdr.reverseCharge || "N").trigger("change");
                $("#ddlBillState").val(hdr.billState || "").trigger("change");
                $("#txtQuotationDate").val(formatDateToDDMMYYYY(hdr.quotationDate));

                $("#txtTotalDealBasicAmount").val(parseFloat(hdr.totalAmtBeforeTax || 0).toFixed(2));
                $("#txtTotalDealGSTAmount").val(parseFloat(hdr.totalAmtAfterTax || 0).toFixed(2));

                // SELECT COMPANY AFTER LIST LOADS
                setTimeout(function () {
                    if (hdr.companyCode) {
                        $("#ddlCompanyname").val(hdr.companyCode).trigger("change");
                    }
                }, 300);

                // CLEAR TABLE
                $("#tblDetailsBody").empty();

                // LOAD DETAIL ROWS
                if (details.length > 0) {
                    $.each(details, function (i, item) {
                        addDetailRow(item);
                    });
                }
                else {
                    addDetailRow(null);
                }

                // REINDEX
                reIndexRows();

                // CALCULATE TOTALS
                $("#tblDetailsBody .detail-row").each(function () {
                    calculateDetailRow($(this));
                });

                calculateGrandTotals();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.error("Error loading Quotation:", thrownError);
                console.error(xhr.responseText);
                showToast("Unable to load Quotation. Please try again.", "error");
            }
        });
    };

    // =====================================================
    // INITIALIZATION
    // =====================================================
    return {
        init: function () {
            // Load Company and State lists
      

            // Load existing data or create new
            if (ID != null && ID != undefined && ID != "Create") {
                loadQuotationData();
            }
            else {
                // Create mode - set today's date
                const today = new Date().toISOString().split("T")[0];
                $("#txtQuotationDate").val(today);

                // Add one empty row
                if ($("#tblDetailsBody tr").length === 0) {
                    addDetailRow(null);
                }
            }
            BindCompanyList();
            BindStateList();
            BindServiceList();
            // Initialize form validation
            formValidator();

            // Initial calculation
            calculateGrandTotals();
        }
    };
}();
