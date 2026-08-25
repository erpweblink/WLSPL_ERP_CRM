var GetWorkOrderIndex = function () {
    var GetWorkOrder = function () {

        var statusvalue = $('#ddlStatus option:selected').val();

        $.ajax({
            url: "/WorkOrder/GetAllWorkOrder",
            data: { "ID": statusvalue },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var users = response.data || [];
                    bindWorkOrderData(users);
                }
                else {
                    new PNotify({
                        title: 'Error',
                        text: response.Message,
                        type: 'error',
                        styling: 'bootstrap3'
                    });
                    console.log(response.Error);
                    window.location.href = "/Login/Login";
                }
            },
            complete: function (data) {
                $("#loader").hide();
            },
            error: function (xhr, ajaxOptions, thrownError) {
            }
        });
    }

    function bindWorkOrderData(data) {
        var arrayReturn = [];
        var resultData = data;

        for (var i = 0; i < resultData.length; i++) {
            var result = resultData[i];
            var TodayDate;

            if (result.todayDt != null) {
                TodayDate = convert(new Date(result.todayDt));
            } else {
                TodayDate = "";
            }

            var editButtton, Approve;

            if (result.status == 1 && result.role == "Admin") {
                editButtton = "<a class='btn btn-primary btn-xs edit' style='font-size: 10px;padding: 4px 14px;' href='/WorkOrder/Create?ID=" + result.id + "'><i class='fa fa-edit' style='font-size:20px;'></i></a>";
                Approve = " <a class='btn btn-success btn-xs mybtn' style='font-size: 10px;padding: 4px 14px;' data-id='" + result.id + "' data-target='#delete' data-title='Delete' data-toggle='modal'><i class='fa fa-check' style='font-size:20px;'></i></a>";
            } else if (result.status == 2 && result.role == "Admin") {
                editButtton = "<a class='btn btn-primary btn-xs edit' style='font-size: 10px;padding: 4px 14px;' href='/WorkOrder/Create?ID=" + result.id + "'><i class='fa fa-edit' style='font-size:20px;'></i></a>";
                Approve = "";
            } else if (result.status == 2) {
                editButtton = "<a class='btn btn-success btn-xs edit' style='font-size: 10px;padding: 4px 14px;' href='/WorkOrder/Create?ID=" + result.id + "'><i class='fa fa-edit' style='font-size:20px;'></i></a>";
                Approve = "";
            } else {
                editButtton = "<a class='btn btn-primary btn-xs edit' style='font-size: 10px;padding: 4px 14px;' href='/WorkOrder/Create?ID=" + result.id + "'><i class='fa fa-edit' style='font-size:20px;'></i></a>";
                Approve = "";
            }

            var viewButton = "<a class='btn btn-warning btn-xs btnview' data-id='" + result.id + "' style='font-size: 10px;padding: 4px 14px;'><i class='fa fa-eye' style='font-size:20px;'></i></a>";
            var Edit_Action = Approve + "  " + editButtton + "  " + viewButton;

            arrayReturn.push([result.id, TodayDate, result.woNo, result.companyName, result.totalDealBasicAmount, result.totalDealGSTAmount, Edit_Action]);
        }

        var $datatable = $('#tblWorkOrder');

        $datatable.DataTable({
            "bDestroy": true,
            data: arrayReturn,
            "ordering": true,
            "order": [[1, 'asc']],
            "columnDefs": [
                { orderable: false, targets: [0], "visible": false },
                { orderable: false, targets: [6] }
            ],
            responsive: true,
            dom: '<"toolbar">lfrtip',
            "bLengthChange": false,
            oLanguage: {
                sLengthMenu: "Show _MENU_",
            },
        });
    }

    $('#ddlStatus').change(function () {
        GetWorkOrder();
    });

    function convert(str) {
        var date = new Date(str),
            mnth = ("0" + (date.getMonth() + 1)).slice(-2),
            day = ("0" + date.getDate()).slice(-2);
        return [day, mnth, date.getFullYear()].join("-");
    }

    $("#tblWorkOrder").on('click', 'tbody .btnview', function () {
        var currentRow = $(this).closest("tr");
        var ID = currentRow.find("td:eq(1)").text();
        LoadWorkOrderDts(ID);
        $('#WorkOrderModal').modal('show');
    });

    $('#tblWorkOrder').on('click', '.mybtn', function () {
        var Dtable = $("#tblWorkOrder").DataTable();
        var RowIndex = $(this).closest('tr');
        var data = Dtable.row(RowIndex).data();
        var ID = data[0];

        swal({
            title: "Are you sure?",
            text: "You won't be able to revert this!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: '#DD6B55',
            confirmButtonText: 'Yes, I am sure!',
            cancelButtonText: "No, cancel it!",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {

                if (isConfirm) {
                    try {
                        $.ajax({
                            url: "/WorkOrder/ApproveWorkOrder",
                            data: JSON.stringify({ "ID": ID }),
                            contentType: "application/json; charset=utf-8",
                            type: "post",
                            dataType: "json",
                            cache: false,
                            beforeSend: function () {
                                $("#loader").show();
                            },
                            success: function (response) {
                                if (response.Success == true) {
                                    swal(
                                        'Approved',
                                        '',
                                        'success'
                                    )
                                    setTimeout(function () {
                                        window.location.href = "/WorkOrder/Index";
                                    }, 3500);
                                }
                                else {
                                    new PNotify({
                                        title: response.TitleMsg,
                                        text: response.Message,
                                        type: response.MsgType,
                                        delay: response.TimeOutMsg,
                                        styling: 'bootstrap3'
                                    });
                                    console.log(response.Error);
                                }
                            },
                            complete: function (data) {
                                $("#loader").hide();
                            },
                            error: function (xhr, ajaxOptions, thrownError) {
                                //$('#lblCommentsNotification').text("Error encountered while saving the comments.");
                            }
                        });
                    }
                    catch (err) {
                        console.log(err);
                    }
                } else {
                    swal("Cancelled", "Your task is In-Process! :)", "error");
                }
            });
    });

    var LoadWorkOrderDts = function (WONo) {

        $.ajax({
            url: "/WorkOrder/GetWorkOrderDEtailsByID",
            data: { "ID": WONo },
            type: "post",
            cache: false,
            success: function (response) {
                if (response.success == true) {
                    var users = response.data || [];
                    bindWorkOrderDetails(users);
                }
                else {
                    new PNotify({
                        title: 'Error',
                        text: response.Message,
                        type: 'error',
                        styling: 'bootstrap3'
                    });
                    console.log(response.Error);
                    window.location.href = "/UserLogin/LoginPage";
                }
            },
            complete: function (data) {
                $("#loader").hide();
            },
            error: function (xhr, ajaxOptions, thrownError) {
            }
        });
    }

    function bindWorkOrderDetails(data) {
        $('#tblWorkOrderList').find('tbody').remove();
        var arrayReturn = [];
        var resultData = data;

        for (var i = 0; i < resultData.length; i++) {
            var result = resultData[i];
            arrayReturn.push([result.department, result.servicesDescription, result.remark, result.qty, result.noofYr, result.rate, result.amount]);
        }

        var $datatable = $('#tblWorkOrderList');

        $datatable.DataTable({
            "bDestroy": true,
            data: arrayReturn,
            sort: [0, 'desc'],
            "ordering": false,
            'order': [[1, 'asc']],
            'columnDefs': [
                {
                    orderable: false, targets: [0], "visible": true
                },
            ],
            responsive: true,
            dom: '<"toolbar">lfrtip',
            "bLengthChange": false,
            oLanguage: {
                sLengthMenu: "Show _MENU_",
            },
        });
    }

    return {
        init: function () {
            GetWorkOrder();
        }
    };
}();
