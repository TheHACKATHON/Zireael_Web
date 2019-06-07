function popup(string, status) {// success, notice, warning, error
    if (string == null) {
        string = "Мы потеряли связь с космосом, пытаемся восстановить квантовый соединитель. Попробуйте позже";
    }
    var type = "bottom-right";
    status = status.toLowerCase();
    if (!$('.notify').hasClass('do-show')) {
        $('.notify')
            .text(string)
            .removeClass()
            .attr('data-notification-status', status)
            .addClass(type + ' notify')
            .addClass('do-show');
        setTimeout(() => { $('.notify').removeClass('do-show').text(""); }, 4500);
        
    }
    else {
        setTimeout(() => { popup(string, status ) }, 1000);
    }
}