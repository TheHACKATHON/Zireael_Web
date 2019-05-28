function SendCodeSuccess(data) {
    if (data) {
        $("input[type=button]").removeAttr("disabled");
    }
}
function ChangePasswordSuccess(data) {
    if (data) {
        if (data.type) {
            switch (data.type) {
                case "ERROR": {
                    $("input:not([type='email'])").removeAttr("disabled");
                    console.log(data.message);
                    break;
                }
                case "OK": {
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