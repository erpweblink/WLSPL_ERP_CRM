var GetWorkOrderForm = function () {
    var WorkOrderId = window.location.pathname.split('/').pop();

    var IsCreate = $("#hdnCreate").val();

    if (!WorkOrderId) {

        if (IsCreate == "F") {

            window.location.href = "/Login/LogIn";
        }
    }


    var BindDepartmentList = function () {
        var Status = $('#ddlServicenname option:selected').text();
        $.ajax({
            url: "/WorkOrder/GetDepartment",
            data: { "Status": Status },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.Success == true) {

                    var html = "<option value='' selected='selected'>-- Select Department --</option>";
                    var users = response.Data;                 
                    if (users.length > 0) {
                        $.each(users, function (key, data) {
                            $("#txtDepartment").val(data.Name);

                        });
                    }


                }
                else {                  
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
            }
        });
    }


    $('#ddlServicenname').change(function () {
        BindDepartmentList();
        BindServiceByID();
    });

    var Companytext = ""; // Example: "ABC Pvt Ltd"

    var BindCompanyList = function () {

        $.ajax({
            url: "/WorkOrder/GetCompany",
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

                    if (Companytext && Companytext.trim() !== "") {

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

    var AddDeleteRow = function () {

        $(".add-row").click(function () {
            if ($('#ddlPaymentMode option:selected').text() == '--Select Payment Mode--') {     
                showToast(
                    "Please select payment mode first", error
                );
            }
            else {

                var BankName = $("#txtBankName").val();
                var ChequeNo = $("#txtChequeNo").val();
                var Chequedate = $("#txtChequedate").val();
                var Amount = $("#txtAmount").val();

                if (BankName != "" && ChequeNo != "" && Chequedate != "" && Amount != "") {
                    $('#divtable').show();
                    var table = $('#tblUser');

                    var markup = "<tr><td><input type='checkbox' name='record'></td><td>" + BankName + "</td><td>" + ChequeNo + "</td><td>" + Chequedate + "</td><td>" + Amount + "</td></tr>";
                    $("#tblBankDetail tbody").append(markup);


                }
                else {                
                    showToast(
                        "Please Fill Required Fields", error
                    );
                }

                $("#txtBankName").val("");
                $("#txtChequeNo").val("");
                $("#txtAmount").val("");
            }
        });

        // Find and remove selected table rows
        $(".delete-row").click(function () {
            var checkCount = $('table').find('input[name="record"]:checked').length;
            if (checkCount > 0) {
                $("#tblBankDetail tbody").find('input[name="record"]').each(function () {
                    if ($(this).is(":checked")) {
                        $(this).parents("tr").remove();
                    }
                });
            }
            else {              
                showToast(
                    "Please Select Record", error
                );
            }

        });
    }

    $('#ddlCompanyname').change(function () {
        var ID = $('#ddlCompanyname option:selected').val();

        $.ajax({
            url: "/WorkOrder/GetCompanyDataByCode",
            data: { "ID": ID },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var result = response.data[0];
                    if (result != null && result != undefined && result != "") {

                        if (!WorkOrderId) {
                            $('#ddlType').val(result.RegisterType).trigger('change');
                        }
                       
                        $('#txtOwnerName').val(result.oname);
                        $('#txtAddress').val(result.address);
                        $('#txtGSTNo').val(result.gstno == null ? "NA" : result.gstno);
                        $('#txtEmailID').val(result.email);
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
        $("#loader").show();
        // ---- Validation ----
        function validateForm() {
            var errors = [];
        
            // Company Name
            if (!$('#ddlCompanyname').val()) {
                errors.push("Please select Company Name.");
                $('#ddlCompanyname').addClass('is-invalid');
            } else {
                $('#ddlCompanyname').removeClass('is-invalid');
            }

            // Type
            if (!$('#ddlType').val()) {
                errors.push("Please select Type.");
                $('#ddlType').addClass('is-invalid');
            } else {
                $('#ddlType').removeClass('is-invalid');
            }

            // W.O. Status
            if (!$('#ddlWOStatus').val()) {
                errors.push("Please select W.O. Status.");
                $('#ddlWOStatus').addClass('is-invalid');
            } else {
                $('#ddlWOStatus').removeClass('is-invalid');
            }

            // Owner Name
            if (!$('#txtOwnerName').val().trim()) {
                errors.push("Owner Name is required.");
                $('#txtOwnerName').addClass('is-invalid');
            } else {
                $('#txtOwnerName').removeClass('is-invalid');
            }

            // Address
            if (!$('#txtAddress').val().trim()) {
                errors.push("Address is required.");
                $('#txtAddress').addClass('is-invalid');
            } else {
                $('#txtAddress').removeClass('is-invalid');
            }

            if (!$('#txtGSTNo').val().trim()) {
                errors.push("GST No. is required.");
                $('#txtGSTNo').addClass('is-invalid');
            } else {
                $('#txtGSTNo').removeClass('is-invalid');
            }

            var email = $('#txtEmailID').val().trim();
            var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!email) {
                errors.push("Email ID is required.");
                $('#txtEmailID').addClass('is-invalid');
            } else if (!emailPattern.test(email)) {
                errors.push("Please enter a valid Email ID.");
                $('#txtEmailID').addClass('is-invalid');
            } else {
                $('#txtEmailID').removeClass('is-invalid');
            }

            if (!$('#txtRenewalDate').val().trim()) {
                errors.push("Renewal Date is required.");
                $('#txtRenewalDate').addClass('is-invalid');
            } else {
                $('#txtRenewalDate').removeClass('is-invalid');
            }

            if (!$('#txtTodayDate').val().trim()) {
                errors.push("Today Date is required.");
                $('#txtTodayDate').addClass('is-invalid');
            } else {
                $('#txtTodayDate').removeClass('is-invalid');
            }

            if (!$('#ddlPaymentMode').val()) {
                errors.push("Please select Payment Mode.");
                $('#ddlPaymentMode').addClass('is-invalid');
            } else {
                $('#ddlPaymentMode').removeClass('is-invalid');
            }

            if ($("#tblService >tbody >tr").length === 0) {
                errors.push("Please add at least one Service.");
                $("#tblService").addClass('table-invalid');
            } else {
                $("#tblService").removeClass('table-invalid');
            }

            //if ($("#tblBankDetail >tbody >tr").length === 0) {
            //    errors.push("Please add at least one Bank Detail.");
            //}

            return errors;
        }

        function toIsoDate(ddmmyyyy) {
            var parts = ddmmyyyy.split('/');
            return parts[2] + '-' + parts[1] + '-' + parts[0];
        }

        $("#btnSubmit").click(function (e) {
            e.preventDefault();

            // ---- Run validation first ----
            var errors = validateForm();
            if (errors.length > 0) {
                // show each missing/invalid field as its own toast
                errors.forEach(function (msg) {
                    showToast(msg, "error");
                });
                return; // stop here — do not proceed to build DataList or call AJAX
            }

            var submitted = false;

            var WorkOrderID = $("#WorkOrderID").val();

            var Type = $('#ddlType option:selected').val();
            var WOStatus = $('#ddlWOStatus option:selected').val();
            var CompanyCode = $('#ddlCompanyname option:selected').val();
            var Companyname = $('#ddlCompanyname option:selected').text();
            var OwnerName = $('#txtOwnerName').val();
            var Address = $('#txtAddress').val();
            var GSTNo = $('#txtGSTNo').val();
            var EmailID = $("#txtEmailID").val();

            var RenewalDate = toIsoDate($("#txtRenewalDate").val().trim());
            var TodayDate = toIsoDate($("#txtTodayDate").val().trim());

            var PaymentMode = $('#ddlPaymentMode option:selected').val();

            var TotalDealBasicAmount = $("#txtTotalDealBasicAmount").val();
            var TotalDealGSTAmount = $("#txtTotalDealGSTAmount").val();
            var BasicAmountReceived = $("#txtBasicAmountReceived").val();
            var GSTAmountReceived = $("#txtGSTAmountReceived").val();
            var BalanceBasicAmount = $("#txtBalanceBasicAmount").val();
            var BalanceGSTAmount = $("#txtBalanceGSTAmount").val();
            var TotalAmountBalance = $("#txtTotalAmountBalance").val();
            var count = 0;

            var ServiceDescriptionList = new Array();
            var rowCount = $("#tblService >tbody >tr").length;

            if (rowCount > 0) {
                $("#tblService tbody tr").each(function () {
                    var row = $(this);

                    var ServiceDescriptiondata = {};
                    ServiceDescriptiondata.Department = row.find("TD").eq(2).html();
                    ServiceDescriptiondata.ServicesDescription = row.find("TD").eq(1).html();
                    ServiceDescriptiondata.Remark = row.find("TD").eq(3).html();
                    ServiceDescriptiondata.Qty = row.find("TD").eq(4).html();
                    ServiceDescriptiondata.NoofYr = row.find("TD").eq(5).html();
                    ServiceDescriptiondata.Rate = row.find("TD").eq(6).html();
                    ServiceDescriptiondata.Amount = row.find("TD").eq(7).html();
                    ServiceDescriptionList.push(ServiceDescriptiondata);
                });
            } else {
                ServiceDescriptionList = null;
            }

            var BankDetailList = new Array();
            var bankRowCount = $('#tblBankDetail >tbody >tr').length;
            if (bankRowCount > 0) {
                $("#tblBankDetail TBODY TR").each(function () {
                    var row = $(this);
                    var BankDetaildata = {};
                    BankDetaildata.BankName = row.find("TD").eq(1).html();
                    BankDetaildata.ChequeNo = row.find("TD").eq(2).html();

                    var cheqDateCell = row.find("TD").eq(3).text().trim();
                    if (cheqDateCell) {
                        BankDetaildata.ChequeDate = toIsoDate(cheqDateCell);
                    } else {
                        console.warn("Skipping row — missing cheque date cell:", row.html());
                        return true; // skip this row, continue .each()
                    }

                    BankDetaildata.Amount = row.find("TD").eq(4).html();
                    BankDetailList.push(BankDetaildata);
                });
            } else {
                BankDetailList = null;
            }

            var DataList = {
                ID: WorkOrderID || null,
                Type: Type || null,
                WOStatus: WOStatus || null,
                WONo: null,
                CompanyName: Companyname || null,
                CompanyCode: CompanyCode || null,
                OwnerName: OwnerName || null,
                Address: Address || null,
                GSTNO: GSTNo || null,
                EmailID: EmailID || null,

                TodayDt: TodayDate || null,
                RenewalDt: RenewalDate || null,

                PaymentMode: PaymentMode || null,

                TotalDealBasicAmount: TotalDealBasicAmount || 0,
                TotalDealGSTAmount: TotalDealGSTAmount || 0,
                BasicAmountReceived: BasicAmountReceived || 0,
                GSTAmountReceived: GSTAmountReceived || 0,
                BalanceBasicAmount: BalanceBasicAmount || 0,
                BalanceGSTAmount: BalanceGSTAmount || 0,
                TotalAmountBalance: TotalAmountBalance || 0,

                CreatedBy: null,
                UpdatedBy: null,

                objtblWorkOrderDtl: ServiceDescriptionList || [],
                objtblBankDetail: BankDetailList || []
            };

            if (count == 0) {
                $("#loader").show();
                $.ajax({
                    url: "/WorkOrder/CreateOrEdit",
                    data: JSON.stringify(DataList),
                    type: "post",
                    contentType: "application/json; charset=utf-8",
                    cache: false,
                    success: function (response) {
                        if (response.success == true) {
                            showToast(response.message, "success");
                            setTimeout(function () {
                                window.location.href = "/WorkOrder/Index";
                            }, 2000);
                        } else {
                            showToast(response.Message, response.MsgType);
                        }
                    },
                    complete: function (data) {
                        $("#loader").hide();
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error(xhr.responseText);
                        showToast("Error saving Work Order. Please try again.", "error");
                    }
                });
            } else {
                showToast("Please Enter Remarks", "error");
            }
        });
        $("#loader").hide();
    };

    var loadWorkOrderData = function () {
        if (WorkOrderId != null && WorkOrderId != undefined && WorkOrderId != "") {
            try {
                $.ajax({
                    url: "/WorkOrder/GetWorkOrderDataById",
                    data: { "ID": WorkOrderId },
                    type: "post",
                    cache: false,
                    success: function (response) {
                        if (response.success == true) {
                            var result = response.data || [];
                            if (result != null && result != undefined && result != "") {
                                $("#btnSubmit").html("Update");
                                $("#lblHeader").html("UPDATE WorkOrder");
                                var WorkOrderDtls = result.workOrderDtls;
                                var WorkOrderBankList = result.workOrderBankList;

                                $("#WorkOrderID").val(result.workOrderHdr.workOrderID);

                                $('#ddlType').val(result.workOrderHdr.type).trigger('change');
                                $('#ddlWOStatus').val(result.workOrderHdr.woStatus).trigger('change');

                                Companytext = result.workOrderHdr.companyName;
                                BindCompanyList();

                                $('#txtOwnerName').val(result.workOrderHdr.ownerName);
                                $('#txtAddress').val(result.workOrderHdr.address);
                                $('#txtGSTNo').val(result.workOrderHdr.gstno);
                                $("#txtEmailID").val(result.workOrderHdr.emailID);

                                var RenewalDate = formatDate(result.workOrderHdr.renewalDate);
                                $("#txtRenewalDate").val(RenewalDate);

                                var TodayDate = formatDate(result.workOrderHdr.todayDate);
                                $("#txtTodayDate").val(TodayDate);

                                $('#ddlPaymentMode')
                                    .val(result.workOrderHdr.paymentMode)
                                    .trigger('change');

                                $("#txtTotalDealBasicAmount").val(result.workOrderHdr.totalDealBasicAmount);
                                $("#txtTotalDealGSTAmount").val(result.workOrderHdr.totalDealGSTAmount);
                                $("#txtBasicAmountReceived").val(result.workOrderHdr.basicAmountReceived);
                                $("#txtGSTAmountReceived").val(result.workOrderHdr.gSTAmountReceived);
                                $("#txtBalanceBasicAmount").val(result.workOrderHdr.balanceBasicAmount);
                                $("#txtBalanceGSTAmount").val(result.workOrderHdr.balanceGSTAmount);
                                $("#txtTotalAmountBalance").val(result.workOrderHdr.totalAmountBalance);

                                $("#tblService tbody").empty();

                                if (WorkOrderDtls.length > 0) {

                                    $('#divtableservice').show();

                                    $.each(WorkOrderDtls, function (i, item) {

                                        var ServiceName = item.servicesDescription;
                                        var Dept = item.department;
                                        var QTY = item.qty;
                                        var Year = item.noofYr;
                                        var Remark = item.remark;
                                        var Rate = item.rate;
                                        var Total = item.amount;

                                        var markup =
                                            "<tr>" +
                                            "<td>" +
                                            "<input type='button' value='Edit' " +
                                            "class='edit_btn btn btn-warning btn-sm' " +
                                            "name='editrow'>" +
                                            "</td>" +
                                            "<td>" + (ServiceName || "") + "</td>" +
                                            "<td>" + (Dept || "") + "</td>" +
                                            "<td>" + (Remark || "") + "</td>" +
                                            "<td>" + (QTY || 0) + "</td>" +
                                            "<td>" + (Year || 0) + "</td>" +
                                            "<td>" + (Rate || 0) + "</td>" +
                                            "<td>" + (Total || 0) + "</td>" +
                                            "</tr>";

                                        $("#tblService tbody").append(markup);
                                    });
                                }

                                bindBankData(WorkOrderBankList);

                          
                            }
                        }
                        else {
                         
                        

                        }
                    },
                    complete: function (data) {
                   
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
                    }
                });
            }
            catch (err) {
                console.log(err);
            }
        }
    }

    function bindBankData(data) {
        $('#divtable').show();
        $('#tblBankDetail').find('tbody').remove();
        var arrayReturn = [];
        var resultData = data;
        for (var i = 0; i < resultData.length; i++) {
            var result = resultData[i];
            var Chk = "<tr><td><input type='checkbox' name='record'></td></tr>";

            var ChequeDate;
            if (result.chequeDate != null) {
                ChequeDate = formatDate(result.chequeDate);
            }
            else {
                ChequeDate = "";
            }

            arrayReturn.push([Chk, result.bankName, result.chequeNo, ChequeDate, result.amount]);
        }

        var $datatable = $('#tblBankDetail');

        $datatable.DataTable({
            "bDestroy": true,
            data: arrayReturn,
            sort: [0, 'desc'],
            "ordering": false,
            'order': [[1, 'asc']],
            'columnDefs': [
                {
                    orderable: false, targets: [1], "visible": true
                },
            ],
            responsive: true,
            dom: '<"toolbar">lfrtip',
            "bLengthChange": false,
            "bPaginate": false,
            "bFilter": false,
            "bInfo": false,
            oLanguage: {
                sLengthMenu: "Show _MENU_",
            },
        });
    }


    $('#txtTotalDealBasicAmount').change(function () {
        var TotalDealBasicAmount = $('#txtTotalDealBasicAmount').val();
        var gstper = $('#spnGSTper').text();

        var gstamount = (parseFloat(TotalDealBasicAmount) * gstper / 100).toFixed(2);
        $('#txtTotalDealGSTAmount').val(gstamount);


        //  var BalanceBasicAmount = $('#txtBalanceBasicAmount').val();

        var BasicAmountReceived = $('#txtBasicAmountReceived').val();
        var GSTAmountReceived = $('#txtGSTAmountReceived').val();
        var BasicTotal = (parseFloat(TotalDealBasicAmount) - parseFloat(BasicAmountReceived)).toFixed(2);
        var BasicGSTTotal = (parseFloat(gstamount) - parseFloat(GSTAmountReceived)).toFixed(2);
        var Total = (parseFloat(BasicTotal) + parseFloat(BasicGSTTotal)).toFixed(2);
        $('#txtBalanceBasicAmount').val(BasicTotal);
        $('#txtBalanceGSTAmount').val(BasicGSTTotal);
        $('#txtTotalAmountBalance').val(Total);


    });

    $('#txtBasicAmountReceived').change(function () {
        var BasicAmountReceived = $('#txtBasicAmountReceived').val();
        if (BasicAmountReceived != "") {
            var txtGSTAmountBalance = $('#txtBalanceGSTAmount').val();
            if (parseFloat(txtGSTAmountBalance) == "0.00") {
                var BalanceGSTAmount = "0.00";
            }
            else {
                var BalanceGSTAmount = (parseFloat(txtGSTAmountBalance));
            }

            var BalanceBasicAmount = $('#txtBalanceBasicAmount').val();
            if (parseFloat(BalanceBasicAmount) >= parseFloat(BasicAmountReceived)) {
                var tBalanceBasicAmount = (parseFloat(BalanceBasicAmount) - parseFloat(BasicAmountReceived)).toFixed(2);

                $('#txtBalanceGSTAmount').val(BalanceGSTAmount);
                var Total = (parseFloat(tBalanceBasicAmount) + parseFloat(BalanceGSTAmount)).toFixed(2);
                $('#txtBalanceBasicAmount').val(tBalanceBasicAmount);
                $('#txtTotalAmountBalance').val(Total);
            } else {
                pnotifymsg("Please Fill Received Amount is less then Total Amount");
            }
        }
        else {
            pnotifymsg("Please Fill Received Amount");
        }

    });

    $('#txtGSTAmountReceived').change(function () {

        var GSTAmountReceived = $('#txtGSTAmountReceived').val();
        if (GSTAmountReceived != "") {
            var txtGSTAmountReceived = $('#txtBalanceGSTAmount').val();

            var GSTAmountReceived = $('#txtGSTAmountReceived').val();
            var BalanceGSTAmount = (parseFloat(txtGSTAmountReceived) - parseFloat(GSTAmountReceived)).toFixed(2);
            if (parseFloat(GSTAmountReceived) <= parseFloat(txtGSTAmountReceived)) {
                $('#txtBalanceGSTAmount').val(BalanceGSTAmount);
                var BalanceBasicAmount = $('#txtBalanceBasicAmount').val();
                var Total = (parseFloat(BalanceBasicAmount) + parseFloat(BalanceGSTAmount)).toFixed(2);

                $('#txtTotalAmountBalance').val(Total);
            } else {
                pnotifymsg("Please Fill Received GST Amount is less then Total GST Amount");
            }
        }
        else {
            pnotifymsg("Please Fill Received GST Amount");
        }

    });

    //datatable input number only

    $('#ddlType').change(function () {

        if ($('#ddlType option:selected').val() == "WLSPL") {
            $('#spnGSTper').text('18');
            $('#trTotalDealGSTAmount').show();
            $('#trGSTAmountReceived').show();
            $('#trBalanceGSTAmount').show();
        }
        else {
            $('#spnGSTper').text('0');
            $('#txtGSTAmountReceived').prop('readonly', true);
            $('#trTotalDealGSTAmount').hide();
            $('#trGSTAmountReceived').hide();
            $('#trBalanceGSTAmount').hide();
        }
    });

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

    function formatDate(dateString) {
        if (!dateString) return "";

        var date = new Date(dateString);

        var day = String(date.getDate()).padStart(2, "0");
        var month = String(date.getMonth() + 1).padStart(2, "0");
        var year = date.getFullYear();

        return day + "/" + month + "/" + year;
    }

  
    //------------------------------------------------------------------------------------

    var Servicetext = "";
    var BindServiceList = function () {
        var Dept = "";
        $.ajax({
            url: "/WorkOrder/BindServiceList",
            data: { "Dept": Dept },
            type: "post",
            cache: false,
            success: function (response) {          
                if (response.success === true) {           // lowercase wrapper
                    var users = response.data || [];        // lowercase wrapper
                    var html = "<option value=''>-- Select Service Name --</option>";
                    $.each(users, function (key, data) {
                        html += "<option value='" + data.ID + "'>" +   // PascalCase row data
                            data.Name +
                            "</option>";
                    });
                    $("#ddlServicenname").html(html);
                }
                else {
                    pnotifymsg(
                        response.Message,
                        response.MsgType,
                        response.TitleMsg
                    );
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
            }
        });
    }


    var BindServiceByID = function () {
        var ID = $('#ddlServicenname option:Selected').val();
        $.ajax({
            url: "/WorkOrder/GetServiceByID",
            data: { "ID": ID },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var result = response.data[0];   // index into the array
                    if (result != null && result != undefined && result != "") {
                        var Qty = $("#txtQTY").val();
                        var Year = $("#txtYear").val();
                        $("#txtRate").val(result.Price);   // PascalCase, matches the row shape
                        var totalamt = (parseFloat(Qty) * parseFloat(Year)) * parseFloat(result.Price);
                        $("#txtTotal").val(totalamt);
                        $("#txtDepartment").val(result.DepartmentName);
                    }
                    else {
                        // nothing found — worth a PNotify here too, since currently this fails silently
                    }


                }
                else {
                    pnotifymsg(
                        response.Message,
                        response.MsgType,
                        response.TitleMsg
                    );
                    console.log(response.Error);

                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
            }
        });
    }

    $('#txtQTY').change(function () {
        var Qty = $("#txtQTY").val();
        var Year = $("#txtYear").val();
        var rate = $("#txtRate").val();

        var totalamt = (parseFloat(Qty) * parseFloat(Year)) * parseFloat(rate);
        $("#txtTotal").val(totalamt);
    });

    $('#txtYear').change(function () {
        var Qty = $("#txtQTY").val();
        var Year = $("#txtYear").val();
        var rate = $("#txtRate").val();

        var totalamt = (parseFloat(Qty) * parseFloat(Year)) * parseFloat(rate);
        $("#txtTotal").val(totalamt);
    });

    var AddDeleteEffortsRow = function () {

        $(".add-row1").click(function () {
            var ServiceName = $("#ddlServicenname option:selected").text();
            var Dept = $("#txtDepartment").val();
            var QTY = $("#txtQTY").val();
            var Year = $("#txtYear").val();
            var Remark = $("#txtRemark").val();
            var Rate = $("#txtRate").val();
            var Total = $("#txtTotal").val();

            if (Dept != "" && ServiceName != "" && QTY != "" && Year != "" && Rate != "" && Total != "") {
                $('#divtableservice').show();
                var table = $('#tblUser');

                var markup = "<tr><td><input type='button' value='Edit' class='edit_btn btn btn-warning btn-sm' name='editrow'></td><td >" + ServiceName + "</td><td >" + Dept + "</td><td >" + Remark + "</td><td>" + QTY + "</td><td>" + Year + "</td><td>" + Rate + "</td><td>" + Total + "</td></tr>";
                $("#tblService tbody").append(markup);

            }
            else {
                pnotifymsg(
                    response.Message,
                    response.MsgType,
                    response.TitleMsg
                );
            }

            $("#txtRate").val("");
            $("#txtTotal").val("");
            $("#txtRemark").val("");
            $("#txtQTY").val("1");
            $("#txtYear").val("1");
            $("#txtDepartment").val("");

            $("#ddlServicenname").val('0').change();
            //-----------------------------------------------calculations of payment
            //$('#txtGSTAmountReceived').val("0");
            //$('#txtBasicAmountReceived').val("0");
            //$('#txtBalanceBasicAmount').val("0");
            //$('#txtBalanceGSTAmount').val("0");
            var sum = 0;
            var rowCount = $("#tblService >tbody >tr").length;

            if (rowCount > 0) {
                $("#tblService tbody tr").each(function () {
                    var row = $(this);

                    var columnData = row.find("TD").eq(7).html();
                    sum += parseFloat(columnData);

                });
            }
            var totaldealbasicamount = $('#txtTotalDealBasicAmount').val();
            var txtBalanceBasicAmount = $('#txtBalanceBasicAmount').val();
            var BalanceGSTAmount = $('#txtBalanceGSTAmount').val();
            var TotalDealGSTAmount = $('#txtTotalDealGSTAmount').val();
            $('#txtTotalDealBasicAmount').val(sum);
            var gstper = $('#spnGSTper').text();
            var gstamount = (parseFloat(sum) * gstper / 100).toFixed(2);
            $('#txtTotalDealGSTAmount').val(gstamount);
            //--------------------Amount calculate----------------------------------------------------
            for (var i = 1; i >= 1; i++) {
                if (parseFloat(txtBalanceBasicAmount) == parseFloat(totaldealbasicamount)) {
                    var tbasicbasicamount = parseFloat(sum);
                } else {
                    var tbasicbasicamount = ((parseFloat(sum) - parseFloat(totaldealbasicamount))) + parseFloat(txtBalanceBasicAmount);
                }
                break;
            }

            $('#txtBalanceBasicAmount').val(tbasicbasicamount);
            // ----------------GST calculation-----------------------------------------------------------
            for (var i = 1; i >= 1; i++) {
                if (parseFloat(BalanceGSTAmount) == parseFloat(TotalDealGSTAmount)) {
                    var tbasicbasicGSTamount = parseFloat(gstamount);
                } else {
                    var tbasicbasicGSTamount = ((parseFloat(gstamount) - parseFloat(TotalDealGSTAmount))) + parseFloat(BalanceGSTAmount);
                }
                break;
            }
            $('#txtBalanceGSTAmount').val(tbasicbasicGSTamount);

            var Total = (parseFloat(tbasicbasicamount) + parseFloat(tbasicbasicGSTamount));
            $('#txtTotalAmountBalance').val(Total);
            //$('#txtTotalDealBasicAmount').val(sum);
            //var gstper = $('#spnGSTper').text();
            //var gstamount = (parseFloat(sum) * gstper / 100).toFixed(2);
            //$('#txtTotalDealGSTAmount').val(gstamount);
            //var BasicAmountReceived = $('#txtBasicAmountReceived').val();
            //var TotalDealBasicAmount = $('#txtTotalDealBasicAmount').val();

            //var BalanceBasicAmount = (parseFloat(TotalDealBasicAmount) - parseFloat(BasicAmountReceived)).toFixed(2);

            //$('#txtBalanceBasicAmount').val(BalanceBasicAmount);


            //var BalanceBasicAmount = $('#txtBalanceBasicAmount').val();
            ////var BalanceGSTAmount = $('#txtBalanceGSTAmount').val();
            //var TotalDealGSTAmount = $('#txtTotalDealGSTAmount').val();
            //var txtGSTAmountReceived= $('#txtGSTAmountReceived').val();
            //if (txtGSTAmountReceived == 0) {
            //    var GST = TotalDealGSTAmount;
            //}
            //else {
            //    var GST = (parseFloat(TotalDealGSTAmount) - parseFloat(txtGSTAmountReceived)).toFixed(2);
            //}
            //$('#txtBalanceGSTAmount').val(GST);
            //var Total = (parseFloat(BalanceBasicAmount) + parseFloat(GST)).toFixed(2);


        });

        // Find and remove selected table rows
        $(".delete-row1").click(function () {
            var checkCount = $('table').find('input[name="record"]:checked').length;

            if (checkCount > 0) {
                $("#tblService tbody").find('input[name="record"]').each(function () {
                    if ($(this).is(":checked")) {
                        $(this).parents("tr").remove();
                    }
                });
            }
            else {
                pnotifymsg(
                    response.Message,
                    response.MsgType,
                    response.TitleMsg
                );
            }

        });
        $('#tblService').on('click', 'tbody .edit_btn', function () {
            Servicetext = "";
            //$('#tblService').dataTable({
            //    "bPaginate": false,
            //    "bFilter": false,
            //    "bInfo": false,
            //    "bSort": false,
            //    "searching": false
            //});
            $("#btnaddrow").hide();

            $("#btnupdaterow").show();

            var table = $('#tblService').DataTable();

            var data_row = table.row($(this).closest('tr')).data();

            var tr = $(this).closest("tr");
            var rowindex = tr.index() + 1;

            $('#rowID').val(rowindex);
            Servicetext = data_row[1];
            BindServiceList();
            $("#txtDepartment").val(data_row[2]);
            $("#txtRemark").val(data_row[3]);
            $("#txtQTY").val(data_row[4]);
            $("#txtYear").val(data_row[5]);
            $("#txtRate").val(data_row[6]);
            $("#txtTotal").val(data_row[7]);
            //$('#spnQtyQuat').hide();
        });

        $(".update-row").click(function () {

            var rowid = $("#rowID").val();
            var ServiceName = $("#ddlServicenname option:selected").text();
            var Dept = $("#txtDepartment").val();
            var QTY = $("#txtQTY").val();
            var Year = $("#txtYear").val();
            var Remark = $("#txtRemark").val();
            var Rate = $("#txtRate").val();
            var Total = $("#txtTotal").val();
            if (Dept != "" && ServiceName != "" && QTY != "" && Year != "" && Rate != "" && Total != "") {
                var action = "<input type='button' value='Edit' class='edit_btn btn btn-warning btn-sm' name='editrow'>";

                var table1 = $('#tblService').DataTable();
                someId = parseInt(rowid) - 1;
                newData = [action, ServiceName, Dept, Remark, QTY, Year, Rate, Total] //Array, data here must match structure of table data
                table1.row(someId).data(newData).draw();
            }
            else {
                pnotifymsg(
                    response.Message,
                    response.MsgType,
                    response.TitleMsg
                );
            }

            $("#rowID").val("");
            $("#txtRate").val("");
            $("#txtTotal").val("");
            $("#txtRemark").val("");
            $("#txtQTY").val("1");
            $("#txtYear").val("1");
            $("#txtDepartment").val("");

            $("#ddlServicenname").val('0').change();

            $("#btnaddrow").show();
            $("#btnupdaterow").hide();
            //-----------------------------------------------calculations of payment
            //$('#txtGSTAmountReceived').val("0");
            //$('#txtBasicAmountReceived').val("0");
            //$('#txtBalanceBasicAmount').val("0");
            //$('#txtBalanceGSTAmount').val("0");

            var rowCount = $("#tblService >tbody >tr").length;
            var sum = 0;
            if (rowCount > 0) {
                $("#tblService tbody tr").each(function () {
                    var row = $(this);
                    var columnData = row.find("TD").eq(7).html();
                    sum += parseFloat(columnData);
                });
            }
            var totaldealbasicamount = $('#txtTotalDealBasicAmount').val();
            var txtBalanceBasicAmount = $('#txtBalanceBasicAmount').val();
            var BalanceGSTAmount = $('#txtBalanceGSTAmount').val();
            var TotalDealGSTAmount = $('#txtTotalDealGSTAmount').val();
            $('#txtTotalDealBasicAmount').val(sum);
            var gstper = $('#spnGSTper').text();
            var gstamount = (parseFloat(sum) * gstper / 100).toFixed(2);
            $('#txtTotalDealGSTAmount').val(gstamount);
            //--------------------Amount calculate----------------------------------------------------
            for (var i = 1; i >= 1; i++) {
                if (parseFloat(txtBalanceBasicAmount) == parseFloat(totaldealbasicamount)) {
                    var tbasicbasicamount = parseFloat(sum);
                } else {
                    var tbasicbasicamount = ((parseFloat(sum) - parseFloat(totaldealbasicamount))) + parseFloat(txtBalanceBasicAmount);
                }
                break;
            }

            $('#txtBalanceBasicAmount').val(tbasicbasicamount);
            // ----------------GST calculation-----------------------------------------------------------
            for (var i = 1; i >= 1; i++) {
                if (parseFloat(BalanceGSTAmount) == parseFloat(TotalDealGSTAmount)) {
                    var tbasicbasicGSTamount = parseFloat(gstamount);
                } else {
                    var tbasicbasicGSTamount = ((parseFloat(gstamount) - parseFloat(TotalDealGSTAmount))) + parseFloat(BalanceGSTAmount);
                }
                break;
            }
            $('#txtBalanceGSTAmount').val(tbasicbasicGSTamount);

            var Total = (parseFloat(tbasicbasicamount) + parseFloat(tbasicbasicGSTamount));
            $('#txtTotalAmountBalance').val(Total);
            //  var BalanceGSTAmount = $('#txtBalanceGSTAmount').val();  

            //var TotalDealBasicAmount = $('#txtTotalDealBasicAmount').val();           
            //var TBalanceBasicAmount = (parseFloat(txtBalanceBasicAmount) - parseFloat(BasicAmountReceived)).toFixed(2);

            //$('#txtBalanceBasicAmount').val(BalanceBasicAmount);


            ////var BalanceBasicAmount = $('#txtBalanceBasicAmount').val();
            //var BalanceGSTAmount = $('#txtBalanceGSTAmount').val();           
            //var TotalDealGSTAmount = $('#txtTotalDealGSTAmount').val();
            //var txtGSTAmountReceived = $('#txtGSTAmountReceived').val();
            //if (txtGSTAmountReceived == 0) {
            //    var GST = TotalDealGSTAmount;
            //}
            //else {
            //    var GST = (parseFloat(TotalDealGSTAmount) - parseFloat(BalanceGSTAmount)).toFixed(2);
            //}
            //$('#txtBalanceGSTAmount').val(GST);
            //var Total = (parseFloat(BalanceBasicAmount) + parseFloat(GST)).toFixed(2);

            //$('#txtTotalAmountBalance').val(Total);
        });

    }
    

    return {
        init: function () {
            $('.Date').datepicker({
                todayHighlight: true,
                format: 'dd/mm/yyyy',
                startDate: '-30d',
                autoclose: true,
                orientation: "top left",
                clearBtn: true
            });
            $('.Date').datepicker('setDate', 'today');
            var today = new Date();

            // Add 11 months to today's date
            var elevenMonthsLater = new Date(today.getFullYear(), today.getMonth() + 11, today.getDate());
            $('.RDate').datepicker({
                todayHighlight: true,
                format: 'dd/mm/yyyy',
                startDate: '-30d',
                autoclose: true,
                orientation: "top left",
                clearBtn: true
            });
            // Set the datepicker to 11 months later
            $('.RDate').datepicker('setDate', elevenMonthsLater);
            $("#txtTodayDate").attr('disabled', 'disabled');

            if (WorkOrderId != null && WorkOrderId != undefined && WorkOrderId != "") {
                loadWorkOrderData();
            }
            formValidator();
            BindCompanyList();
            AddDeleteRow();
            AddDeleteEffortsRow();
            BindServiceList();


        }
    };
}();
