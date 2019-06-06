var _currentUser = null;

document.addEventListener('click', function (e) {

    let target = e.target;

    if (!target.matches('label[for="file"], input[type="file"]')) {
        e.preventDefault();
    }

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
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/logout`);
        xhr.send();
    }
    else if (target.closest(".group")) {
        let data = new FormData();
        data.append("groupId", target.closest(".group").getAttribute("data-id"));

        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/getmessages`);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                hideChats();
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    changeActive(target.closest(".group").parentElement);

                    let messages = data.messages;
                    let chatsContainer = document.querySelector(".message-list-wrap");
                    let groupUl = document.querySelector('.message-list-wrap ul[data-id="' + data.groupId + '"]');
                    if (groupUl == null) {
                        groupUl = Generator.MessagesContainerHTML(data.groupId);
                        groupUl.classList.add("activeUl");

                        data.messages.forEach((message) => {
                            groupUl.appendChild(Generator.MessageHTML(message, data.avatarsDictionary.find(a => a.userId == message.Sender.Id), data._defaultAvatar));
                        });
                        chatsContainer.appendChild(groupUl);
                    }
                    else {
                        groupUl.classList.add("activeUl");
                    }
                }
                else {
                    popup(data.Error, data.Code);
                }
            }
        };
    }
    else if (target.matches("a.send")) {
        //let avatarData = new FormData();
        //avatarData.append("ids", []);
        //var xhr = new XMLHttpRequest();
        //xhr.open('POST', `/sendmessage`);
        //xhr.send(data);
        //xhr.onreadystatechange = function () {
        //    if (xhr.readyState == 4 && xhr.status == 200) {
        //        let data = JSON.parse(xhr.responseText);
        //        if (data.Code != NotifyType.Success) {
        //            popup(data.Error, data.Code);
        //        }
        //    }
        //};

        let hash = new Date().getUTCMilliseconds();
        let text = document.querySelector(".panel-write textarea").value;
        if (text.length > 0) {
            let groupId = document.querySelector(".message-list-wrap ul[data-id]").getAttribute("data-id");
            let messagesContainer = document.querySelector(".message-list-wrap ul.activeUl");

            messagesContainer.appendChild(
                Generator.MessageHTML({
                    Id: null,
                    Hash: hash,
                    Text: text,
                    GroupId: groupId,
                    DateTime: null,
                    Sender: {
                        Login: null,
                        DisplayName: "Я",
                    },
                }, { avatar: "/Content/Images/Zireael_back.png" }, "/Content/Images/Zireael_back.png"))

            let data = new FormData();
            data.append("text", text);
            data.append("groupId", groupId);
            data.append("hash", hash);

            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/sendmessage`);
            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    if (data.Code != NotifyType.Success) {
                        popup(data.Error, data.Code);
                    }
                }
            };
        }
    }
    else {
        $(".tg_head_logo_dropdown.dropdown").
            add(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").
            removeClass("open");
    }
});

function hideChats() {
    $(".message-list-wrap ul").removeClass("activeUl");
    $(".panel-write").addClass("hide");
    $(".chats li.active").removeClass("active");
}

function changeActive(elem) {
    $(".chats li.active").removeClass("active");
    elem.classList.add("active");
    $(".panel-write").removeClass("hide");

}