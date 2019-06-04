//$(".main-menu").on('click', () => {
//    $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").removeClass("open")
//    $(".tg_head_logo_dropdown.dropdown").toggleClass("open");
//});

//$(".dots").on('click', () => {
//    $(".tg_head_logo_dropdown.dropdown").removeClass("open");
//    $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").toggleClass("open");
//});

document.addEventListener('click', function (e) {
    e.preventDefault();
    let target = e.target;

    if (target.matches(".main-menu") || target.closest(".main-menu")) {
        $(".tg_head_logo_dropdown.dropdown").toggleClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").removeClass("open");

    } else if (target.matches(".dots") || target.closest(".dots")) {
        $(".tg_head_logo_dropdown.dropdown").removeClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").toggleClass("open");
    }
    else if (target.closest(".menu-settings")) {
        document.querySelector(".modal-backdrop").classList.remove("hide");
    }
    else if (target.closest(".menu-exit")) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', `/logout`);
        xhr.send();
    }
    else {
        $(".tg_head_logo_dropdown.dropdown").
            add(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").
            removeClass("open");
    }
});