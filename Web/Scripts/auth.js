function SendCodeSuccess(data) {
    if (data) {
        $("input[type=button]").removeAttr("disabled");
    }
}
function ChangePasswordSuccess(data) {
    if (data) {
        if (data.type) {
            switch (data.type) {
                case NotifyType.Error: {
                    $("input:not([type='email'])").removeAttr("disabled");
                    console.log(data.message);
                    break;
                }
                case NotifyType.Success: {
                    console.log(data.message);
                    break;
                }
                default: console.log(data.message);
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