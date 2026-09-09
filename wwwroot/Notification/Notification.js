$(document).ready(function () {
    loadNotificationCount();
});

function loadNotificationCount() {
    $.get("/Notification/GetCount")
        .done(function (count) {
            if (count > 0) {
                $(".notification-dot").text(count).addClass("notification-count-badge");
                $(".notification-icon").addClass("bell-jingle");
            } else {
                $(".notification-dot").text("").removeClass("notification-count-badge");
                $(".notification-icon").removeClass("bell-jingle");
            }
        })
        .fail(function (xhr) {
            console.error("Count load failed:", xhr.status);
        });
}

$(document).on("click", "#notificationBtn", function (e) {
    e.preventDefault();
    $(".notification-icon").removeClass("bell-jingle");
    $.get("/Notification/Index")
        .done(function (response) {
            $("#notificationContainer").html(response);
        })
        .fail(function (xhr) {
            console.error("Notification loading failed:", xhr.status, xhr.responseText);
        });
});

$(document).on("click", "#closeNotifications", function () {
    $("#notificationContainer").empty();
    restartJingleIfActive();
});

$(document).on("click", function (e) {
    if (!$(e.target).closest("#notificationContainer, #notificationBtn").length) {
        $("#notificationContainer").empty();
        restartJingleIfActive();
    }
});

$(document).on("click", ".notification-category-header", function () {
    var $category = $(this).closest(".notification-category");
    $category.toggleClass("expanded");
});

function restartJingleIfActive() {
    var count = parseInt($(".notification-dot").text());
    if (!isNaN(count) && count > 0) {
        $(".notification-icon").removeClass("bell-jingle");
        setTimeout(function () {
            $(".notification-icon").addClass("bell-jingle");
        }, 50);
    }
}