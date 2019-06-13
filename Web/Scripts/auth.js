function SendCodeSuccess(data) {
    if (data) {
        $("input[type=button]").removeAttr("disabled");
        if (data.includes("/sendcodeforchangepassword")) {
            popup("Неверный логин или почта...", NotifyType.Warning);
        }
    }
}
function ChangePasswordSuccess(data) {
    if (data) {
        if (data.type) {
            switch (data.type) {
                case NotifyType.Warning: {
                    $("input:not([type='email'])").removeAttr("disabled");
                    popup(data.message, NotifyType.Warning);
                    break;
                }
                case NotifyType.Success: {
                    $(".go-to-auth").click();
                    popup(data.message, NotifyType.Success);
                    break;
                }
                default: popup(data.message, NotifyType.Notice);
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
    popup("Ошибка", NotifyType.Error);
}