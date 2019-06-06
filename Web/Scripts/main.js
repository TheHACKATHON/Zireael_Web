var _currentUser = null;
var _defaultAvatar = "/Content/Images/Zireael_back.png"
var y_chats = 0;
var last_ypx_chats = -1;
document.addEventListener('click', function (e) {

    let target = e.target;

    if (!target.matches('label[for="file"], input[type="file"]')) {
    //    e.preventDefault();
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
                    let chatsContainer = document.querySelector(".message-list-wrap.scroll-content");
                    if (chatsContainer == null) {
                        chatsContainer = document.querySelector(".message-list-wrap");
                    }
                    let groupUl = document.querySelector('.message-list-wrap ul[data-id="' + data.groupId + '"]');
                    if (groupUl == null) {
                        groupUl = Generator.MessagesContainerHTML(data.groupId);
                        groupUl.classList.add("activeUl");

                        data.messages.forEach((message) => {
                            groupUl.appendChild(Generator.MessageHTML(message, `/user/${_currentUser.Id}/${Crypto.MD5(_currentUser.DisplayName)}`));
                        });
                        chatsContainer.appendChild(groupUl);
                    }
                    else {
                        groupUl.classList.add("activeUl");
                    }
                    $('.scrollbar-macosx-messages').scrollbar({ disableBodyScroll: true });
                    $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
                    $('.scrollbar-macosx-messages').scrollbar({ disableBodyScroll: true });


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
                        DisplayName: _currentUser.DisplayName,
                    },
                }, _currentUser.Avatar))

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

document.addEventListener("DOMContentLoaded", function () {
    $('.scrollbar-macosx-chats').scrollbar({ disableBodyScroll: true});
});

$('.scrollbar-macosx-chats').mousewheel(function (event) {
    //event.preventDefault();
    //if (event.deltaY > 0) {
    //    y_chats -= 30;
    //    if (y_chats < 0) {
    //        y_chats = 0;
    //    }
    //    $('.scrollbar-macosx-chats').scrollTop(y_chats);

    //}
    //else {
    //    if (parseFloat($(".scrollbar-macosx > .scroll-element.scroll-y .scroll-bar").css("top")) != last_ypx_chats) {
    //        y_chats += 30;
    //        last_ypx_chats = parseFloat($(".scrollbar-macosx > .scroll-element.scroll-y .scroll-bar").css("top"));
    //    }
    //    $('.scrollbar-macosx-chats').scrollTop(y_chats );

    //}
   // console.log(event);
   // console.log(parseFloat($(".scrollbar-macosx > .scroll-element.scroll-y .scroll-bar").css("top")), $('.scrollbar-macosx-messages').not('.scroll-content').height(),  y_chats)
});