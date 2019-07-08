function SendCodeSuccess(data) {
    if (data) {
        $("input[type=button]").removeAttr("disabled");
        if (data.includes("/sendcodeforchangepassword")) {
            popover("Неверный логин или почта...", NotifyType.Warning);
        }
    }
}
function ChangePasswordSuccess(data) {
    if (data) {
        if (data.type) {
            switch (data.type) {
                case NotifyType.Warning: {
                    $("input:not([type='email'])").removeAttr("disabled");
                    popover(data.message, NotifyType.Warning);
                    break;
                }
                case NotifyType.Success: {
                    $(".go-to-auth").click();
                    popover(data.message, NotifyType.Success);
                    break;
                }
                default: popover(data.message, NotifyType.Notice);
            }
        }
    }
}
function OnBegin() {
    $("input").attr("disabled", "disabled");
}

document.onkeyup = function(e) {
    e = e || window.event;
    if (e.keyCode === 13) {
        const input = document.querySelector("input[type=submit]");
        if (input !== null && input !== undefined) {
            input.click();
        }
    }
};

$(document).ready(() => {
    $("#loading").css("display", "none");
})

function OnFailure() {
    popover("Ошибка", NotifyType.Error);
}