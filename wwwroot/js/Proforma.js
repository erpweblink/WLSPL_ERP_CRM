var GetProformaForm = function () {
    var ID = window.location.pathname.split('/').pop();

    var IsCreate = $("#hdnCreate").val();

    if (!ID) {

        if (IsCreate == "F") {

            window.location.href = "/Login/LogIn";
        }
    }

<<<<<<< Updated upstream
=======

    var BindStateList = function () {
>>>>>>> Stashed changes

    var BindStateList = function () {       
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

                        html += "<option value='" + data.Name + "'>" +
                            data.Name +
                            "</option>";
                    });

                    $("#ddlBillState").html(html);


                }
                else {
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
            }
        });
    }

<<<<<<< Updated upstream
    var Companytext = "";
=======
    $("#ddlAgainstBy")
        .off("change")
        .on("change", function () {

            var AgainstBy =
                $(this).val();


            if (AgainstBy == "Direct") {

                return;
            }
      
            BindAgainstNumber();

        });

    var AgainstNo = "";
    var BindAgainstNumber = function () {
        var Companyname = $("#ddlCompanyname option:selected").val() || Companytext;
        $.ajax({

            url: "/Proforma/GetQuotationNo",

            data: {
                Companyname: Companyname
            },

            type: "POST",

            cache: false,

            success: function (response) {

                if (response.success === true) {

                    var html =
                        "<option value=''>" +
                        "-- Select Quotation No. --" +
                        "</option>";

                    var users =
                        response.data || [];


                    $.each(users, function (key, data) {

                        html +=
                            "<option value='" +
                            (data.Name || "") +
                            "'>" +
                            (data.Name || "") +
                            "</option>";

                    });


                    if (
                        AgainstNo &&
                        AgainstNo.trim() !== ""
                    ) {

                        var numbers =
                            users.find(function (x) {

                                return (
                                    (x.Name || "")
                                        .toLowerCase()
                                        .trim()
                                ) ===
                                    AgainstNo
                                        .toLowerCase()
                                        .trim();

                            });


                        if (numbers) {

                            $("#ddlAgainstNo")
                                .val(numbers.ID)
                                .trigger("change");
                        }
                    }

                }
                else {

                    showToast(
                        response.message ||
                        "Quotation data not found.",
                        "error"
                    );
                }
            },

            error: function (xhr) {

                console.error(
                    "Get Quotation No Error:",
                    xhr.responseText
                );

                showToast(
                    "Unable to load  Quotation No list.",
                    "error"
                );
            }
        });
    }


    $("#ddlAgainstNo")
        .off("change")
        .on("change", function () {

            var AgainstNo = $(this).val();

            // Clear existing rows if no quotation selected
            if (!AgainstNo) {
                $("#tblDetailsBody").empty();
                addDetailRow(null);
                reIndexRows();
                calculateGrandTotals();
                return;
            }

            $.ajax({
                url: "/Proforma/GetDetailsByQuotationNo",
                type: "POST",
                data: {
                    AgainstNo: AgainstNo
                },
                cache: false,

                beforeSend: function () {
                    // Optional loading
                    $("#tblDetailsBody").html(
                        '<tr><td colspan="10" class="text-center">Loading...</td></tr>'
                    );
                },

                success: function (response) {

                    console.log("Quotation Details Response:", response);

                    if (response && response.success === true) {

                        var details = response.data || [];

                        // Clear old rows
                        $("#tblDetailsBody").empty();

                        // ============================================
                        // ADD DETAIL ROWS
                        // ============================================
                        if (details.length > 0) {

                            $.each(details, function (i, item) {

                                addDetailRow(item);

                            });

                        }
                        else {

                            // No details found
                            addDetailRow(null);
                        }

                        // ============================================
                        // REINDEX ROWS
                        // ============================================
                        reIndexRows();

                        // ============================================
                        // CALCULATE EACH ROW
                        // ============================================
                        $("#tblDetailsBody .detail-row").each(function () {

                            calculateDetailRow($(this));

                        });

                        // ============================================
                        // GRAND TOTAL
                        // ============================================
                        calculateGrandTotals();

                    }
                    else {

                        $("#tblDetailsBody").empty();
                        addDetailRow(null);
                        reIndexRows();
                        calculateGrandTotals();

                        showToast(
                            response?.message ||
                            "Unable to load quotation details.",
                            "error"
                        );
                    }
                },

                error: function (xhr, status, error) {

                    console.error(
                        "GetDetailsByQuotationNo Error:",
                        xhr.responseText
                    );

                    $("#tblDetailsBody").empty();
                    addDetailRow(null);
                    reIndexRows();
                    calculateGrandTotals();

                    showToast(
                        "Error while loading quotation details.",
                        "error"
                    );
                }
            });

        });


>>>>>>> Stashed changes
    var BindCompanyList = function () {

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

                        html += "<option value='" + data.ID + "'>" +
                            data.Name + 
                            "</option>";
                    });

                    $("#ddlCompanyname").html(html);

<<<<<<< Updated upstream
                    if (Companytext && Companytext.trim() !== "") {
=======
                    $("#ddlCompanyname")
                        .html(html);


                    // -----------------------------------------
                    // SELECT COMPANY DURING EDIT
                    // -----------------------------------------
                    if (
                        Companytext &&
                        Companytext.trim() !== ""
                    ) {

                        var company =
                            users.find(function (x) {

                                return (
                                    (x.Name || "")
                                        .toLowerCase()
                                        .trim()
                                ) ===
                                    Companytext
                                        .toLowerCase()
                                        .trim();

                            });
>>>>>>> Stashed changes

                        var company = users.find(function (x) {
                            return (x.Name || "").toLowerCase().trim() ===
                                Companytext.toLowerCase().trim();
                        });

                        if (company) {
                            $("#ddlCompanyname")
                                .val(company.ID)
                                .trigger("change");
                        }
                    }
                }
                else {
                    showToast(
                        "Data not found", error
                    );
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr.responseText);
            }
        });
    };

<<<<<<< Updated upstream
=======
    $("#ddlCompanyname")
        .off("change")
        .on("change", function () {

            var companyID =
                $(this).val();


            if (!companyID) {

                return;
            }


            $.ajax({

                url: "/Proforma/GetCompanyByCode",

                data: {
                    ID: companyID
                },

                type: "POST",

                cache: false,

                success: function (response) {

                    if (response.success === true) {

                        var result =
                            response.data &&
                                response.data.length > 0
                                ? response.data[0]
                                : null;


                        if (!result) {

                            return;
                        }
                        if (
                            ID != null &&
                            ID != undefined &&
                            ID != "Create"
                        ) {

                            return;
                        }


                        $("#txtAddress")
                            .val(result.address || "");


                        $("#txtGSTNo")
                            .val(
                                result.gstno == null
                                    ? "NA"
                                    : result.gstno
                            );


                        $("#txtEmailID")
                            .val(result.email || "");
>>>>>>> Stashed changes

    $('#ddlCompanyname').change(function () {
        var ID = $('#ddlCompanyname option:selected').val();

        $.ajax({
            url: "/Proforma/GetCompanyByCode",
            data: { "ID": ID },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var result = response.data[0];
                    if (result != null && result != undefined && result != "") {
                        $("#ddlBillState")
                            .val(result.State)
                            .trigger("change");                 
                        $('#txtAddress').val(result.address);
                        $('#txtGSTNo').val(result.gstno == null ? "NA" : result.gstno);
                        $('#txtEmailID').val(result.email);
                        var gstNo = result.gstno == null ? "NA" : result.gstno;                 

                        // Check first 2 digits of GSTIN
                        if (gstNo !== "NA" && gstNo.length >= 2) {

                            var stateCode = gstNo.substring(0, 2);

                            if (stateCode === "27") {

                                // Maharashtra - CGST + SGST
                                $("#txtCGST").val("9");
                                $("#txtSGST").val("9");
                                $("#txtIGST").val("0");

                            } else {

                                // Other State - IGST
                                $("#txtCGST").val("0");
                                $("#txtSGST").val("0");
                                $("#txtIGST").val("18");
                            }
                        }
                    }
                }
                else {
                
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
            }
        });
    });


    var formValidator = function () {
        $("#btnSubmit")
            .off("click")
            .on("click", function (e) {

                e.preventDefault();


                var errors = [];


                // Company
                if (!$("#ddlCompanyname").val()) {
                    errors.push(
                        "Please select Company Name."
                    );
                    $("#ddlCompanyname")
                        .addClass("is-invalid");
                }
                else {
                    $("#ddlCompanyname")
                        .removeClass("is-invalid");
                }
                // Address
                if (!$("#txtAddress").val().trim()) {
                    errors.push(
                        "Address is required."
                    );
                    $("#txtAddress")
                        .addClass("is-invalid");
                }
                else {
                    $("#txtAddress")
                        .removeClass("is-invalid");
                }
                                // GST
                if (!$("#txtGSTNo").val().trim()) {
                    errors.push(
                        "GSTIN is required."
                    );
                    $("#txtGSTNo")
                        .addClass("is-invalid");

                }
                else {

                    $("#txtGSTNo")
                        .removeClass("is-invalid");
                }


                // State
                if (!$("#ddlBillState").val()) {

                    errors.push(
                        "Please select State."
                    );

                    $("#ddlBillState")
                        .addClass("is-invalid");

                }
                else {

                    $("#ddlBillState")
                        .removeClass("is-invalid");
                }
           
                 // -----------------------------------------
                // STATE
                // -----------------------------------------
                if (!$("#ddlAgainstBy").val()) {

                    errors.push(
                        "Please select Against By"
                    );

                    $("#ddlAgainstBy")
                        .addClass("is-invalid");

                }
                else {

                    $("#ddlAgainstBy")
                        .removeClass("is-invalid");
                }

                // Proforma Date
                if (!$("#txtProformaDate").val()) {

                    errors.push(
                        "Proforma Date is required."
                    );

                    $("#txtProformaDate")
                        .addClass("is-invalid");

                }
                else {

                    $("#txtProformaDate")
                        .removeClass("is-invalid");
                }


                // Service
                var table =
                    $("#tblService").DataTable();

                if (table.rows().count() === 0) {

                    errors.push(
                        "Please add at least one Service."
                    );

                    $("#tblService")
                        .addClass("table-invalid");

                }
                else {

                    $("#tblService")
                        .removeClass("table-invalid");
                }


                if (errors.length > 0) {

                    errors.forEach(function (msg) {

                        showToast(
                            msg,
                            "error"
                        );

                    });

                    return;
                }


                var ServiceDescriptionList = [];

                var table = $("#tblService").DataTable();

                table.rows().every(function () {

                    var data = this.data();

                    ServiceDescriptionList.push({

                        ID: 0,

                        ProformaID:
                            parseInt($("#ID").val()) || 0,

                        ProductDescription:
                            data[1] || null,

                        SACCode:
                            data[2] || null,

                        Qty:
                            (parseFloat(data[3]) || 0).toString(),

                        Rate:
                            (parseFloat(data[4]) || 0).toString(),

                        Amount:
                            (parseFloat(data[11]) || 0).toString(),

                        TaxableValue:
                            (parseFloat(data[11]) || 0).toString(),

                        CGSTRate:
                            (parseFloat(data[5]) || 0).toString(),

                        CGSTAmt:
                            (parseFloat(data[6]) || 0).toString(),

                        SGSTRate:
                            (parseFloat(data[7]) || 0).toString(),

                        SGSTAmt:
                            (parseFloat(data[8]) || 0).toString(),

                        IGSTRate:
                            (parseFloat(data[9]) || 0).toString(),

                        IGSTAmt:
                            (parseFloat(data[10]) || 0).toString(),

                        Total:
                            (parseFloat(data[12]) || 0).toString()
                    });

                });

                var DataList = {

                    ID: parseInt($("#ID").val()) || 0,
                    ProformaDate: $("#txtProformaDate").val() || null,
                    ReverseCharge: $("#ddlReverseCharge").val() || "N",
                    CompanyName: $("#ddlCompanyname option:selected").text() || null,
                    CompanyCode:   $("#ddlCompanyname").val() || null,
                    Address: $("#txtAddress").val() || null,
                    GSTNO:    $("#txtGSTNo").val() || null,
                    BillState:$("#ddlBillState").val() || null,
                    State:$("#ddlBillState").val() || null,
                    TotalAmtBeforeTax:  $("#txtTotalDealBasicAmount").val()  || 0,
                    TotalAmtAfterTax:   $("#txtTotalDealGSTAmount").val() || 0,  
                    objtblProformaDtl:ServiceDescriptionList

<<<<<<< Updated upstream
=======
                    ProformaDate:
                        $("#txtProformaDate").val() ||
                        null,

                    ReverseCharge:
                        $("#ddlReverseCharge").val() ||
                        "N",

                    CompanyName:
                        $("#ddlCompanyname option:selected")
                            .text() ||
                        null,

                    CompanyCode:
                        $("#ddlCompanyname").val() ||
                        null,

                    Address:
                        $("#txtAddress").val() ||
                        null,

                    GSTNO:
                        $("#txtGSTNo").val() ||
                        null,

                    BillState:
                        $("#ddlBillState").val() ||
                        null,
                          AgainstNo:
                        $("#ddlAgainstNo").val() ||
                        null,
                          AgainstBy:
                        $("#ddlAgainstBy").val() ||
                        null,

                    State:
                        $("#ddlBillState").val() ||
                        null,

                    TotalAmtBeforeTax:
                        $("#txtTotalDealBasicAmount")
                            .val() ||
                        "0",

                    TotalAmtAfterTax:
                        $("#txtTotalDealGSTAmount")
                            .val() ||
                        "0",

                    objtblProformaDtl:
                        ServiceDescriptionList
>>>>>>> Stashed changes
                };


                console.log(
                    "Proforma Data:",
                    DataList
                );

                console.log(
                    JSON.stringify(DataList)
                );
                $("#loader").show();

                $.ajax({
                    url: "/Proforma/CreateOrEdit",
                    type: "POST",
                    data: JSON.stringify(DataList),
                    contentType: "application/json; charset=utf-8",
                    dataType:"json",
                    cache: false,

                    success: function (response) {

                        console.log(
                            "Response:",
                            response
                        );


                        if (
                            response.success === true
                        ) {

                            showToast(
                                response.message ||
                                "Proforma saved successfully.",
                                "success"
                            );


                            setTimeout(function () {

                                window.location.href =
                                    "/Proforma/Index";

                            }, 1500);

                        }
                        else {

                            showToast(
                                response.message ||
                                response.Message ||
                                "Unable to save Proforma.",
                                "error"
                            );

                        }

                    },


                    error: function (
                        xhr,
                        status,
                        error
                    ) {

                        console.error(
                            "AJAX Error:",
                            error
                        );

                        console.error(
                            xhr.responseText
                        );


                        showToast(
                            "Error saving Proforma. Please try again.",
                            "error"
                        );

                    },


                    complete: function () {

                        $("#loader").hide();

                    }

                });

            });
    };

    var loadProformaData = function () {
        if (ID != null && ID != undefined && ID != "") {
            try {
                $.ajax({
                    url: "/Proforma/GetProformaDataById",
                    data: { "ID": ID },
                    type: "post",
                    cache: false,
                    success: function (response) {
                        if (response.success == true) {
                            var result = response.data || [];
                            if (result != null && result != undefined) {

                                $("#btnSubmit").html("Update");
                                $("#lblHeader").html("UPDATE Proforma");

                                var hdr = result.proformaHdr || [];
                                var details = result.proformaDtls;

                                $("#ID").val(hdr.id);

                                Companytext = hdr.companyName;
                                BindCompanyList(); // assumes this sets ddlCompanyname based on Companytext
                                $("#ddlCompanyname").val(hdr.companyCode).trigger('change');

                                $("#txtAddress").val(hdr.address);
                                $("#txtGSTNo").val(hdr.gstno);
                                $("#ddlBillState").val(hdr.billState).trigger('change');
                                $("#ddlReverseCharge").val(hdr.reverseCharge).trigger('change');

                                $("#txtProformaDate").val(formatDateToDDMMYYYY(hdr.proformaDate));

                                $("#txtTotalDealBasicAmount").val(hdr.totalAmtBeforeTax);
                                $("#txtTotalDealGSTAmount").val(hdr.totalAmtAfterTax);

<<<<<<< Updated upstream
                                var table = $("#tblService").DataTable();
                                table.clear();
=======
            cache:
                false,


            success:
                function (response) {

                    if (
                        response.success !== true
                    ) {

                        showToast(
                            response.message ||
                            "Unable to load Proforma.",
                            "error"
                        );

                        return;
                    }


                    var result =
                        response.data || {};


                    // =================================================
                    // HEADER
                    // =================================================
                    var hdr =
                        result.proformaHdr || {};


                    // =================================================
                    // DETAILS
                    // =================================================
                    var details =
                        result.proformaDtls || [];


                    // =================================================
                    // UPDATE MODE
                    // =================================================
                    $("#btnSubmit")
                        .html("Update");


                    $("#lblHeader")
                        .html("UPDATE Proforma");


                    $("#ID")
                        .val(hdr.id || ID);


                    // =================================================
                    // COMPANY
                    // =================================================
                    Companytext =
                        hdr.companyName || "";

                    AgainstNo =
                        hdr.againstNo || "";


                    // =================================================
                    // OTHER HEADER VALUES
                    // =================================================
                    $("#txtAddress")
                        .val(hdr.address || "");


                    $("#txtGSTNo")
                        .val(hdr.gstno || "NA");


                    $("#ddlReverseCharge")
                        .val(hdr.reverseCharge || "N")
                        .trigger("change");

                             $("#ddlAgainstBy")
                        .val(hdr.againstBy || "N")
                        .trigger("change");

                             $("#ddlAgainstNo")
                        .val(hdr.againstNo || "N")
                        .trigger("change");


                    $("#ddlBillState")
                        .val(hdr.billState || "NA")
                        .trigger("change");


                    $("#txtProformaDate")
                        .val(
                            formatDateToDDMMYYYY(
                                hdr.proformaDate
                            )
                        );


                    $("#txtTotalDealBasicAmount")
                        .val(
                            parseFloat(
                                hdr.totalAmtBeforeTax || 0
                            ).toFixed(2)
                        );


                    $("#txtTotalDealGSTAmount")
                        .val(
                            parseFloat(
                                hdr.totalAmtAfterTax || 0
                            ).toFixed(2)
                        );


                    // =================================================
                    // SELECT COMPANY AFTER LIST IS LOADED
                    // =================================================
                    setTimeout(function () {

                        if (hdr.companyCode) {

                            $("#ddlCompanyname")
                                .val(hdr.companyCode)
                                .trigger("change");
                        }

                    }, 300);


                    // =================================================
                    // CLEAR DETAIL TABLE
                    // =================================================
                    $("#tblDetailsBody")
                        .empty();


                    // =================================================
                    // LOAD DETAIL ROWS
                    // =================================================
                    if (details.length > 0) {

                        $.each(
                            details,
                            function (i, item) {

                                addDetailRow(item);
>>>>>>> Stashed changes

                                var action =
                                    "<button type='button' class='edit_btn btn btn-warning btn-sm' title='Edit'>" +
                                    "<i class='fa-solid fa-pen-to-square'></i>" +
                                    "</button> ";
                                if (details.length > 0) {
                                    $.each(details, function (i, item) {
                                        table.row.add([
                                            action,
                                            item.productDescription || "",
                                            item.sacCode || "",
                                            item.qty || 0,
                                            item.rate || 0,
                                            item.cgstRate || 0,
                                            item.cgstAmt || 0,
                                            item.sgstRate || 0,
                                            item.sgstAmt || 0,
                                            item.igstRate || 0,
                                            item.igstAmt || 0,
                                            item.amount || 0,
                                            item.total || 0
                                        ]);
                                    });
                                    table.draw();
                                    $("#divtableservice").show();
                                }
                            }
                        }
                        else {
                            showToast(response.message || "Unable to load Proforma.", "error");
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("Error loading Proforma:", thrownError);
                        showToast("Error loading Proforma. Please try again.", "error");
                    }
                });
            }
            catch (err) {
                console.log(err);
            }
        }
    }
    function formatDateToDDMMYYYY(dateValue) {
        if (!dateValue) return "";

        var date = new Date(dateValue);

        if (isNaN(date.getTime())) return "";

        var day = String(date.getDate()).padStart(2, '0');
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var year = date.getFullYear();

        return year + "-" + month + "-" + day ;
    }
    function calculateSpareDetails() {

        var qty = parseFloat($('#txtQTY').val()) || 0;
        var rate = parseFloat($('#txtRate').val()) || 0;

        var cgst = parseFloat($('#txtCGST').val()) || 0;
        var sgst = parseFloat($('#txtSGST').val()) || 0;
        var igst = parseFloat($('#txtIGST').val()) || 0;

        // Basic Amount
        var basicAmount = qty * rate;

        // GST Amounts
        var cgstAmt = (basicAmount * cgst) / 100;
        var sgstAmt = (basicAmount * sgst) / 100;
        var igstAmt = (basicAmount * igst) / 100;

        // Total GST
        var totalGST = cgstAmt + sgstAmt + igstAmt;

        // Final Total
        var total = basicAmount + totalGST;

        // Set values
        $('#txtCGSTAmt').val(cgstAmt.toFixed(2));
        $('#txtSGSTAmt').val(sgstAmt.toFixed(2));
        $('#txtIGSTAmt').val(igstAmt.toFixed(2));

        $('#txtTotal').val(basicAmount.toFixed(2));
        $('#txtAllTotal').val(total.toFixed(2));
    }

    //input number only
    $('.numeric').keyup(function () {
        var val = $(this).val();
        if (isNaN(val)) {
            val = val.replace(/[^0-9\.]/g, '');
            if (val.split('.').length > 2)
                val = val.replace(/\.+$/, "");
        }
        $(this).val(val);
    });

    $('#tblService').DataTable({
        paging: false,
        searching: false,
        info: false,
        lengthChange: false
    });

    $('#txtQTY, #txtRate, #txtCGST, #txtSGST, #txtIGST').on('keyup', function () {
        if ($(this).attr('id') === 'txtCGST') {
            $('#txtSGST').val($(this).val());
            $('#txtIGST').val('0');
        }
        if ($(this).attr('id') === 'txtSGST') {
            $('#txtCGST').val($(this).val());
            $('#txtIGST').val('0');
        }
        if ($(this).attr('id') === 'txtIGST') {
            if ($(this).val() !== '' && parseFloat($(this).val()) > 0) {
                $('#txtCGST').val('0');
                $('#txtSGST').val('0');
            }
        }
        calculateSpareDetails();
    });

   
    var Servicetext = "";
    var AddDeleteEffortsRow = function () {

        $(".add-row1").off("click").on("click", function () {

            var ServiceName = $("#txtServicenname").val();

            var SacCode = $("#txtSacCode").val();
            var QTY = $("#txtQTY").val();
            var Rate = $("#txtRate").val();

            var CGST = $("#txtCGST").val();
            var CGSTAmt = $("#txtCGSTAmt").val();

            var SGST = $("#txtSGST").val();
            var SGSTAmt = $("#txtSGSTAmt").val();

            var IGST = $("#txtIGST").val();
            var IGSTAmt = $("#txtIGSTAmt").val();

            var Total = $("#txtTotal").val();
            var AllTotal = $("#txtAllTotal").val();


            // Validation
            if (
                !ServiceName ||
                !QTY ||
                !Rate) {

                showToast("Please enter all required service details.", "warning");
                return;
            }


            // Show table
            $("#divtableservice").show();


            // DataTable
            var table = $('#tblService').DataTable();


            // Action button
            var action =
                "<button type='button' class='edit_btn btn btn-warning btn-sm' title='Edit'>" +
                "<i class='fa-solid fa-pen-to-square'></i>" +
                "</button> ";


            // Add row
            table.row.add([
                action,
                ServiceName,
                SacCode || "",
                QTY || "0",
                Rate || "0",
                CGST || "0",
                CGSTAmt || "0",
                SGST || "0",
                SGSTAmt || "0",
                IGST || "0",
                IGSTAmt || "0",
                Total || "0",
                AllTotal || "0"
            ]).draw(false);


            // Clear fields
            ClearServiceFields();


            // Calculate total
            CalculateServiceTotal();

        });

        $('#tblService')
            .off('click', 'tbody .edit_btn')
            .on('click', 'tbody .edit_btn', function () {

                var table = $('#tblService').DataTable();

                var row = table.row($(this).closest('tr'));

                var data_row = row.data();

                if (!data_row) {
                    return;
                }


                // Store DataTable row index
                $("#rowID").val(row.index());

                $("#txtServicenname").val(data_row[1] || "");

                // Other fields
                $("#txtSacCode").val(data_row[2] || "00440013");
                $("#txtQTY").val(data_row[3] || "1");
                $("#txtRate").val(data_row[4] || "0");

                $("#txtCGST").val(data_row[5] || "0");
                $("#txtCGSTAmt").val(data_row[6] || "0");

                $("#txtSGST").val(data_row[7] || "0");
                $("#txtSGSTAmt").val(data_row[8] || "0");

                $("#txtIGST").val(data_row[9] || "0");
                $("#txtIGSTAmt").val(data_row[10] || "0");

                $("#txtTotal").val(data_row[11] || "0");
                $("#txtAllTotal").val(data_row[12] || "0");


                // Button visibility
                $("#btnaddrow").hide();
                $("#btnupdaterow").show();

            });


        $(".update-row")
            .off("click")
            .on("click", function () {

                var rowID = $("#rowID").val();

                if (rowID === "" || rowID === null) {

                    showToast("Please select a service to update.", "warning");
                    return;
                }


                var ServiceName = $("#txtServicenname").val();     

                var SacCode = $("#txtSacCode").val();
                var QTY = $("#txtQTY").val();
                var Rate = $("#txtRate").val();

                var CGST = $("#txtCGST").val();
                var CGSTAmt = $("#txtCGSTAmt").val();

                var SGST = $("#txtSGST").val();
                var SGSTAmt = $("#txtSGSTAmt").val();

                var IGST = $("#txtIGST").val();
                var IGSTAmt = $("#txtIGSTAmt").val();

                var Total = $("#txtTotal").val();
                var AllTotal = $("#txtAllTotal").val();


                // Validation
                if (
                    !ServiceName ||
                    !QTY ||
                    !Rate) {

                    showToast("Please enter all required service details.", "warning");
                    return;
                }


                var action =
                    "<button type='button' class='edit_btn btn btn-warning btn-sm' title='Edit'>" +
                    "<i class='fa-solid fa-pen-to-square'></i>" +
                    "</button> ";


                var table = $('#tblService').DataTable();


                // Update exact DataTable row
                table.row(parseInt(rowID)).data([
                    action,
                    ServiceName,
                    SacCode || "",
                    QTY || "0",
                    Rate || "0",
                    CGST || "0",
                    CGSTAmt || "0",
                    SGST || "0",
                    SGSTAmt || "0",
                    IGST || "0",
                    IGSTAmt || "0",
                    Total || "0",
                    AllTotal || "0"
                ]).draw(false);


                // Clear
                $("#rowID").val("");

                ClearServiceFields();


                // Buttons
                $("#btnaddrow").show();
                $("#btnupdaterow").hide();


                // Calculate
                CalculateServiceTotal();

            });


        $(".delete-row1")
            .off("click")
            .on("click", function () {

                var table = $('#tblService').DataTable();

                var checkedRows =
                    $('#tblService tbody input[name="record"]:checked');

                if (checkedRows.length === 0) {

                    showToast("Please select a row to delete.", "warning");
                    return;
                }


                checkedRows.each(function () {

                    table
                        .row($(this).closest('tr'))
                        .remove();

                });


                table.draw(false);

                CalculateServiceTotal();


                if (table.rows().count() === 0) {
                    $("#divtableservice").hide();
                }

            });

    };

    function ClearServiceFields() {

        $("#txtServicenname").val("");
        $("#txtSacCode").val("00440013");

        $("#txtRate").val("0");

        $("#txtCGST").val("0");
        $("#txtCGSTAmt").val("0");

        $("#txtSGST").val("0");
        $("#txtSGSTAmt").val("0");

        $("#txtIGST").val("0");
        $("#txtIGSTAmt").val("0");

        $("#txtTotal").val("0");
        $("#txtAllTotal").val("0");

        $("#txtQTY").val("1");

        Servicetext = "";
    }

    var CalculateServiceTotal = function () {

        var table = $('#tblService').DataTable();

        var basicAmount = 0;
        var gstAmount = 0;

        table.rows().every(function () {

            var data = this.data();

            var basic = parseFloat(data[11]) || 0;

            var cgstAmt = parseFloat(data[6]) || 0;
            var sgstAmt = parseFloat(data[8]) || 0;
            var igstAmt = parseFloat(data[10]) || 0;

            basicAmount += basic;
            gstAmount += cgstAmt + sgstAmt + igstAmt;
        });

        var totalAmount = basicAmount + gstAmount;

        $("#txtTotalDealBasicAmount").val(basicAmount.toFixed(2));
        $("#txtTotalDealGSTAmount").val(gstAmount.toFixed(2));
        $("#txtTotalAmountBalance").val(totalAmount.toFixed(2));
    };
    

<<<<<<< Updated upstream
=======
    $("#serviceBody").hide();


>>>>>>> Stashed changes
    return {
        init: function () {          
            const today = new Date().toISOString().split('T')[0];
            $("#txtProformaDate").val(today);

            if (ID != null && ID != undefined && ID != "") {
                loadProformaData();
            }
            formValidator();
            BindCompanyList();      
            AddDeleteEffortsRow();
            BindStateList();


        }
    };
}();
