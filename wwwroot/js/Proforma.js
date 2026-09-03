var GetProformaForm = function () {

    var ID = window.location.pathname.split('/').pop();
    var IsCreate = $("#hdnCreate").val();

    if (!ID) {
        if (IsCreate == "F") {
            window.location.href = "/Login/LogIn";
        }
    }

    var Companytext = "";
    var AgainstNo = "";

    // Default GST rates applied to newly added rows (set after company/state is known)
    var defaultCGST = "0";
    var defaultSGST = "0";
    var defaultIGST = "0";

    var isEditLoad = false;

    // =====================================================================
    // STATE LIST
    // =====================================================================
    var BindStateList = function (callback) {
        $.ajax({
            url: "/Proforma/GetState",
            data: { "Status": "1" },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var html = "<option value='' selected='selected'>-- Select State --</option>";
                    var users = response.data || [];
                    $.each(users, function (key, data) {
                        html += "<option value='" + data.Name + "'>" + data.Name + "</option>";
                    });
                    $("#ddlBillState").html(html);
                }

                if (typeof callback === "function") {
                    callback();
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.error(xhr.responseText);
                if (typeof callback === "function") {
                    callback();
                }
            }
        });
    };

    // =====================================================================
    // AGAINST BY / AGAINST NO
    // =====================================================================
    $("#ddlAgainstBy")
        .off("change")
        .on("change", function () {
            var AgainstByVal = $(this).val();
            if (AgainstByVal == "Direct") {
                return;
            }
            BindAgainstNumber();
        });

    var BindAgainstNumber = function (callback) {
        var Companyname = $("#ddlCompanyname option:selected").val() || Companytext;

        $.ajax({
            url: "/Proforma/GetQuotationNo",
            data: { Companyname: Companyname },
            type: "POST",
            cache: false,
            success: function (response) {
                if (response.success === true) {

                    var html = "<option value=''>-- Select Quotation No. --</option>";
                    var users = response.data || [];

                    $.each(users, function (key, data) {
                        html += "<option value='" + (data.Name || "") + "'>" + (data.Name || "") + "</option>";
                    });

                    $("#ddlAgainstNo").html(html);

                    // The list was just rebuilt from scratch -- re-apply the saved
                    // selection or it silently reverts to the placeholder.
                    if (AgainstNo && AgainstNo.trim() !== "") {
                        var match = users.find(function (x) {
                            return (x.Name || "").toLowerCase().trim() === AgainstNo.toLowerCase().trim();
                        });
                        if (match) {
                            $("#ddlAgainstNo").val(match.Name);
                        }
                    }

                    // Re-lock it if we're restoring a saved Proforma. The rebuild
                    // itself doesn't clear the disabled attribute, but do this
                    // defensively so the lock always survives a rebind.
                    if (isEditLoad) {
                        $("#ddlAgainstNo").prop("disabled", true);
                    }

                    if (isEditLoad) {
                        $("#ddlAgainstNo").prop("disabled", true);
                    }
                }
                else {
                    showToast(response.message || "Quotation data not found.", "error");
                }

                if (typeof callback === "function") {
                    callback();
                }
            },
            error: function (xhr) {
                console.error("Get Quotation No Error:", xhr.responseText);
                showToast("Unable to load Quotation No list.", "error");
                if (typeof callback === "function") {
                    callback();
                }
            }
        });
    };

    $("#ddlAgainstNo")
        .off("change")
        .on("change", function () {

            // While restoring a saved Proforma, don't re-fetch and overwrite the
            // detail rows we've already loaded from the saved record.
            if (isEditLoad) {
                return;
            }

            var AgainstNoVal = $(this).val();

            if (!AgainstNoVal) {
                $("#tblDetailsBody").empty();
                addDetailRow(null);
                reIndexRows();
                calculateGrandTotals();
                return;
            }

            $.ajax({
                url: "/Proforma/GetDetailsByQuotationNo",
                type: "POST",
                data: { AgainstNo: AgainstNoVal },
                cache: false,

                beforeSend: function () {
                    $("#tblDetailsBody").html('<tr><td colspan="13" class="text-center">Loading...</td></tr>');
                },

                success: function (response) {

                    if (response && response.success === true) {

                        var details = response.data || [];

                        $("#tblDetailsBody").empty();

                        if (details.length > 0) {
                            $.each(details, function (i, item) {
                                addDetailRow(item);
                            });
                        }
                        else {
                            addDetailRow(null);
                        }

                        reIndexRows();

                        $("#tblDetailsBody .detail-row").each(function () {
                            calculateDetailRow($(this));
                        });

                        calculateGrandTotals();
                    }
                    else {
                        $("#tblDetailsBody").empty();
                        addDetailRow(null);
                        reIndexRows();
                        calculateGrandTotals();

                        showToast(response.message || "Unable to load quotation details.", "error");
                    }
                },

                error: function (xhr) {
                    console.error("GetDetailsByQuotationNo Error:", xhr.responseText);

                    $("#tblDetailsBody").empty();
                    addDetailRow(null);
                    reIndexRows();
                    calculateGrandTotals();

                    showToast("Error while loading quotation details.", "error");
                }
            });
        });

    // =====================================================================
    // COMPANY LIST
    // =====================================================================
    var BindCompanyList = function (callback) {
        $.ajax({
            url: "/Proforma/GetCompany",
            data: { Status: "1" },
            type: "POST",
            cache: false,
            success: function (response) {

                if (response.success === true) {

                    var users = response.data || [];
                    var html = "<option value=''>-- Select Company Name --</option>";

                    $.each(users, function (key, data) {
                        html += "<option value='" + data.ID + "'>" + data.Name + "</option>";
                    });

                    $("#ddlCompanyname").html(html);
                }
                else {
                    showToast(response.message || "Company data not found.", "error");
                }

                if (typeof callback === "function") {
                    callback();
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                if (typeof callback === "function") {
                    callback();
                }
            }
        });
    };

    $('#ddlCompanyname').on("change", function () {

        var CompanyID = $('#ddlCompanyname option:selected').val();

        if (!CompanyID) {
            return;
        }

        $.ajax({
            url: "/Proforma/GetCompanyByCode",
            data: { "ID": CompanyID },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {

                    var result = (response.data || [])[0];

                    if (result) {

                        $('#txtAddress').val(result.address);
                        $('#txtGSTNo').val(result.gstno == null ? "NA" : result.gstno);
                        $('#txtEmailID').val(result.email);

                        // While restoring a saved Proforma, keep the historically saved
                        // Bill State instead of overwriting it with the company's current
                        // default state.
                        if (!isEditLoad) {
                            $("#ddlBillState").val(result.State).trigger("change");
                        }

                        var gstNo = result.gstno == null ? "NA" : result.gstno;

                        // Check first 2 digits of GSTIN to decide CGST+SGST vs IGST
                        if (gstNo !== "NA" && gstNo.length >= 2) {

                            var stateCode = gstNo.substring(0, 2);

                            if (stateCode === "27") {
                                // Maharashtra - CGST + SGST
                                defaultCGST = "9";
                                defaultSGST = "9";
                                defaultIGST = "0";
                            } else {
                                // Other State - IGST
                                defaultCGST = "0";
                                defaultSGST = "0";
                                defaultIGST = "18";
                            }

                            // Only auto-apply the freshly computed default rates to rows that
                            // are already on screen when this is a live, user-driven company
                            // change (e.g. Create mode). Never do this while restoring a saved
                            // Proforma -- that would clobber each row's actually-saved GST rates.
                            if (!isEditLoad) {
                                $("#tblDetailsBody .detail-row").each(function () {
                                    var $row = $(this);
                                    $row.find(".cgst-rate").val(defaultCGST);
                                    $row.find(".sgst-rate").val(defaultSGST);
                                    $row.find(".igst-rate").val(defaultIGST);
                                    calculateDetailRow($row);
                                });

                                calculateGrandTotals();
                            }
                        }
                    }
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
            }
        });
    });

    // =====================================================================
    // DETAIL ROW HELPERS
    // =====================================================================
    function calculateDetailRow(row) {

        var qty = parseFloat(row.find(".qty").val()) || 0;
        var rate = parseFloat(row.find(".rate").val()) || 0;
        var cgstRate = parseFloat(row.find(".cgst-rate").val()) || 0;
        var sgstRate = parseFloat(row.find(".sgst-rate").val()) || 0;
        var igstRate = parseFloat(row.find(".igst-rate").val()) || 0;

        // Basic amount
        var amount = qty * rate;

        // GST amounts
        var cgstAmt = (amount * cgstRate) / 100;
        var sgstAmt = (amount * sgstRate) / 100;
        var igstAmt = (amount * igstRate) / 100;

        // Final total
        var allTotal = amount + cgstAmt + sgstAmt + igstAmt;

        row.find(".cgst-amt").val(cgstAmt.toFixed(2));
        row.find(".sgst-amt").val(sgstAmt.toFixed(2));
        row.find(".igst-amt").val(igstAmt.toFixed(2));
        row.find(".amount").val(amount.toFixed(2));
        row.find(".all-total").val(allTotal.toFixed(2));
    }

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

        $("#txtTotalDealBasicAmount").val(basicAmount.toFixed(2));
        $("#txtTotalDealGSTAmount").val(gstAmount.toFixed(2));

        if ($("#txtTotalAmountBalance").length) {
            $("#txtTotalAmountBalance").val(finalAmount.toFixed(2));
        }
    }

    function reIndexRows() {
        $("#tblDetailsBody .detail-row").each(function (index) {
            $(this).attr("data-index", index);
        });
    }

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

    function addDetailRow(item) {

        item = item || {};

        var serviceName = item.productDescription || "";
        var sacCode = item.sacCode || "00440013";
        var qty = item.qty || 1;
        var rate = item.rate || 0;

        var cgstRate = item.cgstRate || 0;
        var cgstAmt = item.cgstAmt || 0;

        var sgstRate = item.sgstRate || 0;
        var sgstAmt = item.sgstAmt || 0;

        var igstRate = item.igstRate || 0;
        var igstAmt = item.igstAmt || 0;

        var amount = item.amount || 0;
        var total = item.total || 0;

        var newRow =
            '<tr class="detail-row">' +

            '<td class="text-center">' +
            '<button type="button" class="btn btn-danger btn-sm delete-row" title="Delete" style="width:40px;">' +
            '<i class="fa fa-trash"></i>' +
            '</button>' +
            '</td>' +

            '<td>' +
            '<textarea class="form-control service-name" rows="1">' + escapeHtml(serviceName) + '</textarea>' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control sac-code" value="' + escapeHtml(sacCode) + '" ' +
            'maxlength="8" minlength="6" inputmode="numeric" ' +
            'oninput="this.value=this.value.replace(/[^0-9]/g,\'\').slice(0,8);" />' +
            '</td>' +

            '<td>' +
            '<input type="number" class="form-control qty" value="' + qty + '" min="1" step="1" />' +
            '</td>' +

            '<td>' +
            '<input type="number" class="form-control rate" value="' + rate + '" min="0" step="0.01" />' +
            '</td>' +

            '<td>' +
            '<input type="number" class="form-control cgst-rate" value="' + cgstRate + '" min="0" step="0.01" />' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control cgst-amt" value="' + parseFloat(cgstAmt || 0).toFixed(2) + '" readonly />' +
            '</td>' +

            '<td>' +
            '<input type="number" class="form-control sgst-rate" value="' + sgstRate + '" min="0" step="0.01" />' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control sgst-amt" value="' + parseFloat(sgstAmt || 0).toFixed(2) + '" readonly />' +
            '</td>' +

            '<td>' +
            '<input type="number" class="form-control igst-rate" value="' + igstRate + '" min="0" step="0.01" />' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control igst-amt" value="' + parseFloat(igstAmt || 0).toFixed(2) + '" readonly />' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control amount" value="' + parseFloat(amount || 0).toFixed(2) + '" readonly />' +
            '</td>' +

            '<td>' +
            '<input type="text" class="form-control all-total" value="' + parseFloat(total || 0).toFixed(2) + '" readonly />' +
            '</td>' +

            '</tr>';

        $("#tblDetailsBody").append(newRow);

        var row = $("#tblDetailsBody .detail-row").last();
        calculateDetailRow(row);
        calculateGrandTotals();
    }

    // Add a new blank row
    $("#btnAddRow").off("click").on("click", function () {
        addDetailRow(null);
        reIndexRows();
    });

    // Delete a row
    $(document).off("click", ".delete-row").on("click", ".delete-row", function () {

        var rowCount = $("#tblDetailsBody .detail-row").length;

        if (rowCount <= 1) {
            showToast("At least one Service is required.", "error");
            return;
        }

        $(this).closest("tr").remove();
        reIndexRows();
        calculateGrandTotals();
    });

    // Recalculate on field change, and link CGST/SGST/IGST rates
    $(document)
        .off(
            "input",
            "#tblDetailsBody .qty, #tblDetailsBody .rate, #tblDetailsBody .cgst-rate, " +
            "#tblDetailsBody .sgst-rate, #tblDetailsBody .igst-rate"
        )
        .on(
            "input",
            "#tblDetailsBody .qty, #tblDetailsBody .rate, #tblDetailsBody .cgst-rate, " +
            "#tblDetailsBody .sgst-rate, #tblDetailsBody .igst-rate",
            function () {

                var row = $(this).closest("tr");

                if ($(this).hasClass("cgst-rate")) {
                    row.find(".sgst-rate").val($(this).val());
                    row.find(".igst-rate").val("0");
                }

                if ($(this).hasClass("sgst-rate")) {
                    row.find(".cgst-rate").val($(this).val());
                    row.find(".igst-rate").val("0");
                }

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

    $(document).off("input", ".numeric").on("input", ".numeric", function () {

        var val = $(this).val();
        val = val.replace(/[^0-9.]/g, "");

        var parts = val.split(".");
        if (parts.length > 2) {
            val = parts[0] + "." + parts.slice(1).join("");
        }

        $(this).val(val);
    });

    // =====================================================================
    // FORM VALIDATION + SAVE
    // =====================================================================
    var formValidator = function () {

        $("#btnSubmit")
            .off("click")
            .on("click", function (e) {

                e.preventDefault();

                var errors = [];

                if (!$("#ddlCompanyname").val()) {
                    errors.push("Please select Company Name.");
                    $("#ddlCompanyname").addClass("is-invalid");
                } else {
                    $("#ddlCompanyname").removeClass("is-invalid");
                }

                if (!$("#txtAddress").val().trim()) {
                    errors.push("Address is required.");
                    $("#txtAddress").addClass("is-invalid");
                } else {
                    $("#txtAddress").removeClass("is-invalid");
                }

                if (!$("#txtGSTNo").val().trim()) {
                    errors.push("GSTIN is required.");
                    $("#txtGSTNo").addClass("is-invalid");
                } else {
                    $("#txtGSTNo").removeClass("is-invalid");
                }

                if (!$("#ddlBillState").val()) {
                    errors.push("Please select State.");
                    $("#ddlBillState").addClass("is-invalid");
                } else {
                    $("#ddlBillState").removeClass("is-invalid");
                }

                if (!$("#ddlAgainstBy").val()) {
                    errors.push("Please select Against By.");
                    $("#ddlAgainstBy").addClass("is-invalid");
                } else {
                    $("#ddlAgainstBy").removeClass("is-invalid");
                }

                if (!$("#txtProformaDate").val()) {
                    errors.push("Proforma Date is required.");
                    $("#txtProformaDate").addClass("is-invalid");
                } else {
                    $("#txtProformaDate").removeClass("is-invalid");
                }

                var $rows = $("#tblDetailsBody .detail-row");

                if ($rows.length === 0) {
                    errors.push("Please add at least one Service.");
                    $("#tbldetails").addClass("table-invalid");
                }
                else {

                    $("#tbldetails").removeClass("table-invalid");

                    $rows.each(function (i) {
                        var $row = $(this);
                        var name = $row.find(".service-name").val();
                        var qty = $row.find(".qty").val();
                        var rate = $row.find(".rate").val();

                        if (!name || !qty || !rate) {
                            errors.push("Please fill Service Name, QTY and Rate for row " + (i + 1) + ".");
                        }
                    });
                }

                if (errors.length > 0) {
                    errors.forEach(function (msg) {
                        showToast(msg, "error");
                    });
                    return;
                }

                var ServiceDescriptionList = [];

                $rows.each(function () {
                    var $row = $(this);

                    ServiceDescriptionList.push({
                        ID: 0,
                        ProformaID: parseInt($("#ID").val()) || 0,
                        ProductDescription: $row.find(".service-name").val() || null,
                        SACCode: $row.find(".sac-code").val() || null,
                        Qty: (parseFloat($row.find(".qty").val()) || 0).toString(),
                        Rate: (parseFloat($row.find(".rate").val()) || 0).toString(),
                        Amount: (parseFloat($row.find(".amount").val()) || 0).toString(),
                        TaxableValue: (parseFloat($row.find(".amount").val()) || 0).toString(),
                        CGSTRate: (parseFloat($row.find(".cgst-rate").val()) || 0).toString(),
                        CGSTAmt: (parseFloat($row.find(".cgst-amt").val()) || 0).toString(),
                        SGSTRate: (parseFloat($row.find(".sgst-rate").val()) || 0).toString(),
                        SGSTAmt: (parseFloat($row.find(".sgst-amt").val()) || 0).toString(),
                        IGSTRate: (parseFloat($row.find(".igst-rate").val()) || 0).toString(),
                        IGSTAmt: (parseFloat($row.find(".igst-amt").val()) || 0).toString(),
                        Total: (parseFloat($row.find(".all-total").val()) || 0).toString()
                    });
                });

                var DataList = {
                    ID: parseInt($("#ID").val()) || 0,
                    ProformaDate: $("#txtProformaDate").val() || null,
                    ReverseCharge: $("#ddlReverseCharge").val() || "N",
                    CompanyName: $("#ddlCompanyname option:selected").text() || null,
                    CompanyCode: $("#ddlCompanyname").val() || null,
                    Address: $("#txtAddress").val() || null,
                    GSTNO: $("#txtGSTNo").val() || null,
                    BillState: $("#ddlBillState").val() || null,
                    State: $("#ddlBillState").val() || null,
                    AgainstNo: $("#ddlAgainstNo").val() || null,
                    AgainstBy: $("#ddlAgainstBy").val() || null,
                    TotalAmtBeforeTax: $("#txtTotalDealBasicAmount").val() || "0",
                    TotalAmtAfterTax: $("#txtTotalDealGSTAmount").val() || "0",
                    objtblProformaDtl: ServiceDescriptionList
                };

                $("#loader").show();

                $.ajax({
                    url: "/Proforma/CreateOrEdit",
                    type: "POST",
                    data: JSON.stringify(DataList),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    cache: false,

                    success: function (response) {

                        if (response.success === true) {

                            showToast(response.message || "Proforma saved successfully.", "success");

                            setTimeout(function () {
                                window.location.href = "/Proforma/Index";
                            }, 1500);
                        }
                        else {
                            showToast(response.message || response.Message || "Unable to save Proforma.", "error");
                        }
                    },

                    error: function (xhr, status, error) {
                        console.error("AJAX Error:", error);
                        console.error(xhr.responseText);
                        showToast("Error saving Proforma. Please try again.", "error");
                    },

                    complete: function () {
                        $("#loader").hide();
                    }
                });
            });
    };

    // =====================================================================
    // LOAD EXISTING PROFORMA (EDIT MODE)
    // =====================================================================
    var loadProformaData = function () {

        if (!ID) {
            return;
        }

        isEditLoad = true;

        try {
            $.ajax({
                url: "/Proforma/GetProformaDataById",
                data: { "ID": ID },
                type: "post",
                cache: false,
                success: function (response) {

                    if (response.success !== true) {
                        showToast(response.message || "Unable to load Proforma.", "error");
                        isEditLoad = false;
                        return;
                    }

                    var result = response.data || {};
                    var hdr = result.proformaHdr || {};
                    var details = result.proformaDtls || [];

                    $("#btnSubmit").html("Update");
                    $("#lblHeader").html("UPDATE Proforma");

                    $("#ID").val(hdr.id || ID);

                    Companytext = hdr.companyCode || "";
                    AgainstNo = hdr.againstNo || "";

                    $("#txtAddress").val(hdr.address || "");
                    $("#txtGSTNo").val(hdr.gstno || "NA");

                    $("#ddlReverseCharge").val(hdr.reverseCharge || "N").trigger("change");
                    $("#ddlAgainstBy").val(hdr.againstBy || "Direct").trigger("change");
                    $("#ddlBillState").val(hdr.billState || "").trigger("change");
                    $("#ddlAgainstNo").val(hdr.againstNo || "").trigger("change");

                    $("#txtProformaDate").val(formatDateToDDMMYYYY(hdr.proformaDate));

                    $("#txtTotalDealBasicAmount").val(parseFloat(hdr.totalAmtBeforeTax || 0).toFixed(2));
                    $("#txtTotalDealGSTAmount").val(parseFloat(hdr.totalAmtAfterTax || 0).toFixed(2));

                    // -----------------------------------------------------
                    // DETAIL ROWS -- load from the saved record itself,
                    // independent of any dropdown timing below.
                    // -----------------------------------------------------
                    $("#tblDetailsBody").empty();

                    if (details.length > 0) {
                        $.each(details, function (i, item) {
                            addDetailRow(item);
                        });
                    }
                    else {
                        addDetailRow(null);
                    }

                    reIndexRows();
                    calculateGrandTotals();

                    BindCompanyList(function () {

                        if (hdr.companyCode) {                         
                            $("#ddlCompanyname").val(hdr.companyCode).trigger("change");
                        }

                    });
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.error("Error loading Proforma:", thrownError);
                    showToast("Error loading Proforma. Please try again.", "error");
                    isEditLoad = false;
                }
            });
        }
        catch (err) {
            console.log(err);
            isEditLoad = false;
        }
    };

    function formatDateToDDMMYYYY(dateValue) {
        if (!dateValue) return "";

        var date = new Date(dateValue);
        if (isNaN(date.getTime())) return "";

        var day = String(date.getDate()).padStart(2, '0');
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var year = date.getFullYear();

        return year + "-" + month + "-" + day;
    }

    return {
        init: function () {

            formValidator();

            if (ID != null && ID != undefined && ID != "Create") {
                loadProformaData();
            }
            else {
                BindCompanyList();
                BindStateList();
                BindAgainstNumber();

                var today = new Date().toISOString().split("T")[0];
                $("#txtProformaDate").val(today);

                if ($("#tblDetailsBody tr").length === 0) {
                    addDetailRow(null);
                }

                
            }
            calculateGrandTotals();
        }
    };
}();
