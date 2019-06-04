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
    else if (target.closest(".group")) {
        let data = new FormData();
        data.append("groupId", target.closest(".group").getAttribute("data-id"));

        var xhr = new XMLHttpRequest();
        xhr.open('POST', `/getmessages`);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let messagesUl = document.querySelector(".message-list");
                messagesUl.innerHTML = "";
                let data = JSON.parse(xhr.responseText);
                data.messages.forEach((message) => {
                    messagesUl.appendChild(Generator.MessageHTML(message, data.avatarsDictionary.find(a => a.userId == message.Sender.Id), data._defaultAvatar));
                });
            }
        };
    }
    else {
        $(".tg_head_logo_dropdown.dropdown").
            add(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").
            removeClass("open");
    }
});