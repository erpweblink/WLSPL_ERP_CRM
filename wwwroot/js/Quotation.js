
var GetQuotationForm = function () {

    var ID = window.location.pathname.split('/').pop();

    var IsCreate = $("#hdnCreate").val();

    if (!ID) {

        if (IsCreate == "F") {

            window.location.href = "/Login/LogIn";

            return;
        }
    }
    var Companytext = "";

    var BindStateList = function () {

        $.ajax({

            url: "/Quotation/GetState",

            data: {
                Status: "1"
            },

            type: "POST",

            cache: false,

            success: function (response) {

                if (response.success === true) {

                    var html =
                        "<option value=''>" +
                        "-- Select State --" +
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


                    $("#ddlBillState")
                        .html(html);

                }
                else {

                    showToast(
                        response.message ||
                        "State data not found.",
                        "error"
                    );
                }
            },

            error: function (xhr) {

                console.error(
                    "Get State Error:",
                    xhr.responseText
                );

                showToast(
                    "Unable to load State list.",
                    "error"
                );
            }
        });
    };

    var BindCompanyList = function () {

        $.ajax({

            url: "/Quotation/GetCompany",

            data: {
                Status: "1"
            },

            type: "POST",

            cache: false,

            success: function (response) {

                if (response.success === true) {

                    var users =
                        response.data || [];

                    var html =
                        "<option value=''>" +
                        "-- Select Company Name --" +
                        "</option>";


                    $.each(users, function (key, data) {

                        html +=
                            "<option value='" +
                            (data.ID || "") +
                            "'>" +
                            (data.Name || "") +
                            "</option>";

                    });


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


                        if (company) {

                            $("#ddlCompanyname")
                                .val(company.ID)
                                .trigger("change");
                        }
                    }
                }
                else {

                    showToast(
                        response.message ||
                        "Company data not found.",
                        "error"
                    );
                }
            },

            error: function (xhr) {

                console.error(
                    "Get Company Error:",
                    xhr.responseText
                );

                showToast(
                    "Unable to load Company list.",
                    "error"
                );
            }
        });
    };

    $("#ddlCompanyname")
        .off("change")
        .on("change", function () {

            var companyID =
                $(this).val();


            if (!companyID) {

                return;
            }


            $.ajax({

                url: "/Quotation/GetCompanyByCode",

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


                        $("#ddlBillState")
                            .val(result.State || "")
                            .trigger("change");

                        var gstNo =
                            result.gstno == null
                                ? "NA"
                                : result.gstno;


                        if (
                            gstNo !== "NA" &&
                            gstNo.length >= 2
                        ) {

                            var stateCode =
                                gstNo.substring(0, 2);


                            // Maharashtra
                            if (stateCode === "27") {

                                $("#txtCGST")
                                    .val("9");

                                $("#txtSGST")
                                    .val("9");

                                $("#txtIGST")
                                    .val("0");
                            }
                            else {

                                // Other State
                                $("#txtCGST")
                                    .val("0");

                                $("#txtSGST")
                                    .val("0");

                                $("#txtIGST")
                                    .val("18");
                            }
                        }
                    }
                    else {

                        showToast(
                            response.message ||
                            "Unable to load Company details.",
                            "error"
                        );
                    }
                },

                error: function (xhr) {

                    console.error(
                        "Company details error:",
                        xhr.responseText
                    );
                }
            });
        });

    function formatDateToDDMMYYYY(dateValue) {

        if (!dateValue) {

            return "";
        }


        var date =
            new Date(dateValue);


        if (isNaN(date.getTime())) {

            return "";
        }


        var day =
            String(date.getDate())
                .padStart(2, "0");

        var month =
            String(date.getMonth() + 1)
                .padStart(2, "0");

        var year =
            date.getFullYear();


        return (
            year +
            "-" +
            month +
            "-" +
            day
        );
    }

    function calculateDetailRow(row) {

        var qty =
            parseFloat(
                row.find(".qty").val()
            ) || 0;


        var rate =
            parseFloat(
                row.find(".rate").val()
            ) || 0;


        var cgstRate =
            parseFloat(
                row.find(".cgst-rate").val()
            ) || 0;


        var sgstRate =
            parseFloat(
                row.find(".sgst-rate").val()
            ) || 0;


        var igstRate =
            parseFloat(
                row.find(".igst-rate").val()
            ) || 0;


        // -----------------------------------------
        // BASIC AMOUNT
        // -----------------------------------------
        var amount =
            qty * rate;


        // -----------------------------------------
        // GST AMOUNT
        // -----------------------------------------
        var cgstAmt =
            (amount * cgstRate) / 100;


        var sgstAmt =
            (amount * sgstRate) / 100;


        var igstAmt =
            (amount * igstRate) / 100;


        // -----------------------------------------
        // FINAL TOTAL
        // -----------------------------------------
        var allTotal =
            amount +
            cgstAmt +
            sgstAmt +
            igstAmt;


        // -----------------------------------------
        // SET VALUES
        // -----------------------------------------
        row.find(".cgst-amt")
            .val(cgstAmt.toFixed(2));


        row.find(".sgst-amt")
            .val(sgstAmt.toFixed(2));


        row.find(".igst-amt")
            .val(igstAmt.toFixed(2));


        row.find(".amount")
            .val(amount.toFixed(2));


        row.find(".all-total")
            .val(allTotal.toFixed(2));
    }


    function calculateGrandTotals() {

        var basicAmount = 0;

        var gstAmount = 0;

        var finalAmount = 0;


        $("#tblDetailsBody .detail-row")
            .each(function () {

                var row =
                    $(this);


                var amount =
                    parseFloat(
                        row.find(".amount").val()
                    ) || 0;


                var cgst =
                    parseFloat(
                        row.find(".cgst-amt").val()
                    ) || 0;


                var sgst =
                    parseFloat(
                        row.find(".sgst-amt").val()
                    ) || 0;


                var igst =
                    parseFloat(
                        row.find(".igst-amt").val()
                    ) || 0;


                var total =
                    parseFloat(
                        row.find(".all-total").val()
                    ) || 0;


                basicAmount += amount;


                gstAmount +=
                    cgst +
                    sgst +
                    igst;


                finalAmount += total;

            });


        // -----------------------------------------
        // SET HEADER TOTALS
        // -----------------------------------------
        $("#txtTotalDealBasicAmount")
            .val(
                basicAmount.toFixed(2)
            );


        $("#txtTotalDealGSTAmount")
            .val(
                gstAmount.toFixed(2)
            );


        if ($("#txtTotalAmountBalance").length) {

            $("#txtTotalAmountBalance")
                .val(
                    finalAmount.toFixed(2)
                );
        }
    }

    $("#btnAddRow")
        .off("click")
        .on("click", function () {

            var rowCount =
                $("#tblDetailsBody .detail-row")
                    .length;


            var newRow = `

    < tr class="detail-row" >

                    < !--ACTION -->
                    <td class="text-center">

                        <button type="button"
                                class="btn btn-danger btn-sm delete-row"
                                title="Delete">

                            <i class="fa fa-trash"></i>

                        </button>

                    </td>


                    <!--SERVICE NAME-- >
                    <td>

                        <textarea
                            class="form-control service-name"></textarea>

                    </td>


                    <!--SAC CODE-- >
                    <td>

                        <input type="text"
                               class="form-control sac-code"
                               value="00440013"
                               maxlength="8"
                               minlength="6"
                               inputmode="numeric"
                               oninput="this.value=this.value.replace(/[^0-9]/g,'').slice(0,8);" />

                    </td>


                    <!--QTY -->
                    <td>

                        <input type="number"
                               class="form-control qty"
                               value="1"
                               min="1"
                               step="1" />

                    </td>


                    <!--RATE -->
                    <td>

                        <input type="number"
                               class="form-control rate"
                               value="0"
                               min="0"
                               step="0.01" />

                    </td>


                    <!--CGST RATE-- >
                    <td>

                        <input type="number"
                               class="form-control cgst-rate"
                               value="0"
                               min="0"
                               step="0.01" />

                    </td>


                    <!--CGST AMOUNT-- >
                    <td>

                        <input type="text"
                               class="form-control cgst-amt"
                               value="0.00"
                               readonly />

                    </td>


                    <!--SGST RATE-- >
                    <td>

                        <input type="number"
                               class="form-control sgst-rate"
                               value="0"
                               min="0"
                               step="0.01" />

                    </td>


                    <!--SGST AMOUNT-- >
                    <td>

                        <input type="text"
                               class="form-control sgst-amt"
                               value="0.00"
                               readonly />

                    </td>


                    <!--IGST RATE-- >
                    <td>

                        <input type="number"
                               class="form-control igst-rate"
                               value="0"
                               min="0"
                               step="0.01" />

                    </td>


                    <!--IGST AMOUNT-- >
                    <td>

                        <input type="text"
                               class="form-control igst-amt"
                               value="0.00"
                               readonly />

                    </td>


                    <!--AMOUNT -->
                    <td>

                        <input type="text"
                               class="form-control amount"
                               value="0.00"
                               readonly />

                    </td>


                    <!--ALL TOTAL-- >
    <td>

        <input type="text"
            class="form-control all-total"
            value="0.00"
            readonly />

    </td>

                </tr >
    `;


            $("#tblDetailsBody")
                .append(newRow);


            reIndexRows();


            calculateGrandTotals();
        });

    $(document)
        .off("click", ".delete-row")
        .on("click", ".delete-row", function () {

            var rowCount =
                $("#tblDetailsBody .detail-row")
                    .length;


            // -----------------------------------------
            // AT LEAST ONE ROW
            // -----------------------------------------
            if (rowCount <= 1) {

                showToast(
                    "At least one Service is required.",
                    "error"
                );

                return;
            }


            $(this)
                .closest("tr")
                .remove();


            reIndexRows();


            calculateGrandTotals();
        });

    $(document)
        .off(
            "input",
            "#tblDetailsBody .qty, " +
            "#tblDetailsBody .rate, " +
            "#tblDetailsBody .cgst-rate, " +
            "#tblDetailsBody .sgst-rate, " +
            "#tblDetailsBody .igst-rate"
        )
        .on(
            "input",
            "#tblDetailsBody .qty, " +
            "#tblDetailsBody .rate, " +
            "#tblDetailsBody .cgst-rate, " +
            "#tblDetailsBody .sgst-rate, " +
            "#tblDetailsBody .igst-rate",

            function () {

                var row =
                    $(this).closest("tr");


                // -----------------------------------------
                // CGST ENTERED
                // -----------------------------------------
                if (
                    $(this).hasClass("cgst-rate")
                ) {

                    var cgstValue =
                        $(this).val();


                    row.find(".sgst-rate")
                        .val(cgstValue);


                    row.find(".igst-rate")
                        .val("0");
                }


                // -----------------------------------------
                // SGST ENTERED
                // -----------------------------------------
                if (
                    $(this).hasClass("sgst-rate")
                ) {

                    var sgstValue =
                        $(this).val();


                    row.find(".cgst-rate")
                        .val(sgstValue);


                    row.find(".igst-rate")
                        .val("0");
                }


                // -----------------------------------------
                // IGST ENTERED
                // -----------------------------------------
                if (
                    $(this).hasClass("igst-rate")
                ) {

                    var igstValue =
                        parseFloat(
                            $(this).val()
                        ) || 0;


                    if (igstValue > 0) {

                        row.find(".cgst-rate")
                            .val("0");

                        row.find(".sgst-rate")
                            .val("0");
                    }
                }


                calculateDetailRow(row);

                calculateGrandTotals();
            }
        );


    function reIndexRows() {

        $("#tblDetailsBody .detail-row")
            .each(function (index) {

                $(this)
                    .attr(
                        "data-index",
                        index
                    );
            });
    }


    $(document)
        .off("input", ".numeric")
        .on("input", ".numeric", function () {

            var val =
                $(this).val();


            val =
                val.replace(
                    /[^0-9.]/g,
                    ""
                );


            // Only one decimal
            var parts =
                val.split(".");


            if (parts.length > 2) {

                val =
                    parts[0] +
                    "." +
                    parts.slice(1).join("");
            }


            $(this)
                .val(val);
        });

    function getServiceDescriptionList() {

        var ServiceDescriptionList = [];


        $("#tblDetailsBody .detail-row")
            .each(function () {

                var row =
                    $(this);


                var productDescription =
                    row.find(".service-name")
                        .val() || "";


                var sacCode =
                    row.find(".sac-code")
                        .val() || "";


                var qty =
                    parseFloat(
                        row.find(".qty").val()
                    ) || 0;


                var rate =
                    parseFloat(
                        row.find(".rate").val()
                    ) || 0;


                var cgstRate =
                    parseFloat(
                        row.find(".cgst-rate").val()
                    ) || 0;


                var cgstAmt =
                    parseFloat(
                        row.find(".cgst-amt").val()
                    ) || 0;


                var sgstRate =
                    parseFloat(
                        row.find(".sgst-rate").val()
                    ) || 0;


                var sgstAmt =
                    parseFloat(
                        row.find(".sgst-amt").val()
                    ) || 0;


                var igstRate =
                    parseFloat(
                        row.find(".igst-rate").val()
                    ) || 0;


                var igstAmt =
                    parseFloat(
                        row.find(".igst-amt").val()
                    ) || 0;


                var amount =
                    parseFloat(
                        row.find(".amount").val()
                    ) || 0;


                var total =
                    parseFloat(
                        row.find(".all-total").val()
                    ) || 0;


                ServiceDescriptionList.push({

                    ID: 0,

                    QuotationID:
                        parseInt(
                            $("#ID").val()
                        ) || 0,

                    ProductDescription:
                        productDescription,

                    SACCode:
                        sacCode,

                    Qty:
                        qty.toString(),

                    Rate:
                        rate.toString(),

                    Amount:
                        amount.toString(),

                    TaxableValue:
                        amount.toString(),

                    CGSTRate:
                        cgstRate.toString(),

                    CGSTAmt:
                        cgstAmt.toString(),

                    SGSTRate:
                        sgstRate.toString(),

                    SGSTAmt:
                        sgstAmt.toString(),

                    IGSTRate:
                        igstRate.toString(),

                    IGSTAmt:
                        igstAmt.toString(),

                    Total:
                        total.toString()
                });

            });


        return ServiceDescriptionList;
    }

    var formValidator = function () {

        $("#btnSubmit")
            .off("click")
            .on("click", function (e) {

                e.preventDefault();


                var errors = [];


                // -----------------------------------------
                // COMPANY
                // -----------------------------------------
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


                // -----------------------------------------
                // ADDRESS
                // -----------------------------------------
                if (
                    !$("#txtAddress")
                        .val()
                        .trim()
                ) {

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


                // -----------------------------------------
                // GST
                // -----------------------------------------
                if (
                    !$("#txtGSTNo")
                        .val()
                        .trim()
                ) {

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


                // -----------------------------------------
                // STATE
                // -----------------------------------------
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
                // Quotation DATE
                // -----------------------------------------
                if (!$("#txtQuotationDate").val()) {

                    errors.push(
                        "Quotation Date is required."
                    );

                    $("#txtQuotationDate")
                        .addClass("is-invalid");

                }
                else {

                    $("#txtQuotationDate")
                        .removeClass("is-invalid");
                }


                // -----------------------------------------
                // DETAIL ROW
                // -----------------------------------------
                var rowCount =
                    $("#tblDetailsBody .detail-row")
                        .length;


                if (rowCount === 0) {

                    errors.push(
                        "Please add at least one Service."
                    );

                    $("#tbldetails")
                        .addClass("table-invalid");

                }
                else {

                    $("#tbldetails")
                        .removeClass("table-invalid");
                }


                // -----------------------------------------
                // SERVICE NAME VALIDATION
                // -----------------------------------------
                $("#tblDetailsBody .detail-row")
                    .each(function (index) {

                        var serviceName =
                            $(this)
                                .find(".service-name")
                                .val()
                                .trim();


                        if (!serviceName) {

                            errors.push(
                                "Service Name is required in row " +
                                (index + 1) +
                                "."
                            );


                            $(this)
                                .find(".service-name")
                                .addClass("is-invalid");
                        }
                        else {

                            $(this)
                                .find(".service-name")
                                .removeClass("is-invalid");
                        }

                    });


                // -----------------------------------------
                // SHOW ERRORS
                // -----------------------------------------
                if (errors.length > 0) {

                    errors.forEach(function (msg) {

                        showToast(
                            msg,
                            "error"
                        );

                    });

                    return;
                }


                // =================================================
                // GET DETAILS
                // =================================================
                var ServiceDescriptionList =
                    getServiceDescriptionList();


                // =================================================
                // MAIN DATA
                // =================================================
                var DataList = {

                    ID:
                        parseInt(
                            $("#ID").val()
                        ) || 0,

                    QuotationDate:
                        $("#txtQuotationDate").val() ||
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

                    objtblQuotationDtl:
                        ServiceDescriptionList
                };


                // =================================================
                // CONSOLE
                // =================================================
                console.log(
                    "Quotation Data:",
                    DataList
                );


                console.log(
                    "JSON:",
                    JSON.stringify(DataList)
                );


                // =================================================
                // LOADER
                // =================================================
                $("#loader")
                    .show();


                // =================================================
                // SAVE / UPDATE
                // =================================================
                $.ajax({

                    url:
                        "/Quotation/CreateOrEdit",

                    type:
                        "POST",

                    data:
                        JSON.stringify(DataList),

                    contentType:
                        "application/json; charset=utf-8",

                    dataType:
                        "json",

                    cache:
                        false,


                    success:
                        function (response) {

                            console.log(
                                "Response:",
                                response
                            );


                            if (
                                response.success === true
                            ) {

                                showToast(

                                    response.message ||
                                    "Quotation saved successfully.",

                                    "success"
                                );


                                setTimeout(
                                    function () {

                                        window.location.href =
                                            "/Quotation/Index";

                                    },
                                    1500
                                );
                            }
                            else {

                                showToast(

                                    response.message ||
                                    response.Message ||
                                    "Unable to save Quotation.",

                                    "error"
                                );
                            }
                        },


                    error:
                        function (
                            xhr,
                            status,
                            error
                        ) {

                            console.error(
                                "AJAX Error:",
                                error
                            );


                            console.error(
                                "Response:",
                                xhr.responseText
                            );


                            showToast(
                                "Error saving Quotation. Please try again.",
                                "error"
                            );
                        },


                    complete:
                        function () {

                            $("#loader")
                                .hide();
                        }
                });

            });
    };

    var loadQuotationData = function () {

        if (
            ID == null ||
            ID == undefined ||
            ID == ""
        ) {

            return;
        }


        $.ajax({

            url:
                "/Quotation/GetQuotationDataById",

            data: {
                ID: ID
            },

            type:
                "POST",

            cache:
                false,


            success:
                function (response) {

                    if (
                        response.success !== true
                    ) {

                        showToast(
                            response.message ||
                            "Unable to load Quotation.",
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
                        result.quotationHdr || {};


                    // =================================================
                    // DETAILS
                    // =================================================
                    var details =
                        result.quotationDtls || [];


                    // =================================================
                    // UPDATE MODE
                    // =================================================
                    $("#btnSubmit")
                        .html("Update");


                    $("#lblHeader")
                        .html("UPDATE Quotation");


                    $("#ID")
                        .val(hdr.id || ID);


                    // =================================================
                    // COMPANY
                    // =================================================
                    Companytext =
                        hdr.companyName || "";


                    // First bind company
                    BindCompanyList();


                    // =================================================
                    // OTHER HEADER VALUES
                    // =================================================
                    $("#txtAddress")
                        .val(hdr.address || "");


                    $("#txtGSTNo")
                        .val(hdr.gstno || "");


                    $("#ddlReverseCharge")
                        .val(hdr.reverseCharge || "N")
                        .trigger("change");


                    $("#ddlBillState")
                        .val(hdr.billState || "")
                        .trigger("change");


                    $("#txtQuotationDate")
                        .val(
                            formatDateToDDMMYYYY(
                                hdr.quotationDate
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

                            }
                        );

                    }
                    else {

                        // Add one empty row
                        addDetailRow(null);
                    }


                    // =================================================
                    // REINDEX
                    // =================================================
                    reIndexRows();


                    // =================================================
                    // CALCULATE TOTAL
                    // =================================================
                    $("#tblDetailsBody .detail-row")
                        .each(function () {

                            calculateDetailRow(
                                $(this)
                            );

                        });


                    calculateGrandTotals();

                },


            error:
                function (
                    xhr,
                    ajaxOptions,
                    thrownError
                ) {

                    console.error(
                        "Error loading Quotation:",
                        thrownError
                    );


                    console.error(
                        xhr.responseText
                    );


                    showToast(
                        "Unable to load Quotation. Please try again.",
                        "error"
                    );
                }
        });
    };

    $("#btnAddRow").off("click").on("click", function () {

        addDetailRow(null);

    });
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


        var newRow = `
        <tr class="detail-row">

            <!-- 1. ACTION -->
            <td class="text-center">
                <button type="button"
                        class="btn btn-danger btn-sm delete-row"
                        title="Delete"
                        style="width:40px;">
                    <i class="fa fa-trash"></i>
                </button>
            </td>


            <!-- 2. SERVICE NAME -->
            <td>
                <textarea class="form-control service-name"
                          rows="1">${escapeHtml(serviceName)}</textarea>
            </td>


            <!-- 3. SAC CODE -->
            <td>
                <input type="text"
                       class="form-control sac-code"
                       value="${escapeHtml(sacCode)}"
                       maxlength="8"
                       minlength="6"
                       inputmode="numeric"
                       oninput="this.value=this.value.replace(/[^0-9]/g,'').slice(0,8);" />
            </td>


            <!-- 4. QTY -->
            <td>
                <input type="number"
                       class="form-control qty"
                       value="${qty}"
                       min="1"
                       step="1" />
            </td>


            <!-- 5. RATE -->
            <td>
                <input type="number"
                       class="form-control rate"
                       value="${rate}"
                       min="0"
                       step="0.01" />
            </td>


            <!-- 6. CGST % -->
            <td>
                <input type="number"
                       class="form-control cgst-rate"
                       value="${cgstRate}"
                       min="0"
                       step="0.01" />
            </td>


            <!-- 7. CGST AMOUNT -->
            <td>
                <input type="text"
                       class="form-control cgst-amt"
                       value="${parseFloat(cgstAmt || 0).toFixed(2)}"
                       readonly />
            </td>


            <!-- 8. SGST % -->
            <td>
                <input type="number"
                       class="form-control sgst-rate"
                       value="${sgstRate}"
                       min="0"
                       step="0.01" />
            </td>


            <!-- 9. SGST AMOUNT -->
            <td>
                <input type="text"
                       class="form-control sgst-amt"
                       value="${parseFloat(sgstAmt || 0).toFixed(2)}"
                       readonly />
            </td>


            <!-- 10. IGST % -->
            <td>
                <input type="number"
                       class="form-control igst-rate"
                       value="${igstRate}"
                       min="0"
                       step="0.01" />
            </td>


            <!-- 11. IGST AMOUNT -->
            <td>
                <input type="text"
                       class="form-control igst-amt"
                       value="${parseFloat(igstAmt || 0).toFixed(2)}"
                       readonly />
            </td>


            <!-- 12. AMOUNT -->
            <td>
                <input type="text"
                       class="form-control amount"
                       value="${parseFloat(amount || 0).toFixed(2)}"
                       readonly />
            </td>


            <!-- 13. ALL TOTAL -->
            <td>
                <input type="text"
                       class="form-control all-total"
                       value="${parseFloat(total || 0).toFixed(2)}"
                       readonly />
            </td>

        </tr>
    `;


        // IMPORTANT
        // Add TR inside existing tbody
        $("#tblDetailsBody").append(newRow);


        // Calculate newly added row
        var row = $("#tblDetailsBody .detail-row").last();

        calculateDetailRow(row);

        // Recalculate grand totals
        calculateGrandTotals();
    }

    function escapeHtml(value) {

        if (
            value === null ||
            value === undefined
        ) {

            return "";
        }


        return String(value)

            .replace(/&/g, "&amp;")

            .replace(/</g, "&lt;")

            .replace(/>/g, "&gt;")

            .replace(/"/g, "&quot;")

            .replace(/'/g, "&#039;");
    }

    $("#serviceBody").hide();


    return {

        init: function () {

            BindCompanyList();

            BindStateList();


            if (
                ID != null &&
                ID != undefined &&
                ID != "Create"
            ) {

                loadQuotationData();
            }
            else {
                const today =
                    new Date()
                        .toISOString()
                        .split("T")[0];


                $("#txtQuotationDate")
                    .val(today);
                if ($("#tblDetailsBody tr").length === 0) {
                    addDetailRow(null);
                }
            }


            formValidator();

            calculateGrandTotals();
        }
    };

}();




