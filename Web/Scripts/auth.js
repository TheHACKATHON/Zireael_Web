function SendCodeSuccess(data) {
    if (data) {
        $("input[type=button]").removeAttr("disabled");
    }
}
function ChangePasswordSuccess(data) {
    if (data) {
        if (data.type) {
            console.log(data.type);
        }
    }
}
function OnBegin() {
    $("input").attr("disabled", "disabled");
}