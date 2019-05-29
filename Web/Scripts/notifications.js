function popup(string, status) {// success, notice, warning, error
    var type = "bottom-right";
    status = status.toLowerCase();
    if (!$('.notify').hasClass('do-show')) {
        $('.notify')
            .text(string)
            .removeClass()
            .attr('data-notification-status', status)
            .addClass(type + ' notify')
            .addClass('do-show');
        setTimeout(() => { $('.notify').removeClass('do-show').text(""); }, 6000);

        event.preventDefault();
    }
    else {
        setTimeout(() => { popup(string, status ) }, 1000);
    }
}