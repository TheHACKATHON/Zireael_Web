var _currentUser = null;
var _defaultAvatar = "/Content/Images/Zireael_back.png";


document.addEventListener('click', function (e) {

    let target = e.target;

    if (!target.matches('label[for="file"], input[type="file"]')) {
        //e.preventDefault();
    }

    if (target.matches(".main-menu") || target.closest(".main-menu")) {
        $(".tg_head_logo_dropdown.dropdown").toggleClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").removeClass("open");

    } else if (target.matches(".dots") || target.closest(".dots")) {
        $(".tg_head_logo_dropdown.dropdown").removeClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").toggleClass("open");
    }
    else {
        $(".tg_head_logo_dropdown.dropdown").
            add(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").
            removeClass("open");
    }

    if (target.closest(".menu-settings")) {
        OpenMenu("menusettings");
    }
    else if (target.closest(".menu-exit")) {
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/logout`);
        xhr.send();
    }
    else if (target.closest(".menu-contacts")) {
        OpenMenu("menucontacts");
    }
    else if (target.closest(".menu-create-group")) {
        OpenMenu("menucreategroup");
    }
    else if (target.closest(".contact:not(.profile)")) {
        target.closest(".contact").classList.toggle("select");
        if ($("a.contact.select").length > 0) {
            $("a.button.delete-contact").removeClass("hide");
            $("a.button.new-contact").addClass("hide");
        }
        else {
            $("a.button.delete-contact").addClass("hide");
            $("a.button.new-contact").removeClass("hide");
        }
    }
    else if (target.closest(".new-contact")) {
        $(".sub-modal-dialog").removeClass("hide");
        $(".sub-modal-dialog").html("");
        $(".sub-modal-dialog").html(`
            <div class="sub-dialog-container">
                <h3>Добавление контакта</h3>
                <input required="" autocomplete="login" class="login" type="text" placeholder="Логин пользователя"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-add-contact">добавить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".create-group")) {
        let groupName = $(".group-name").val();
        console.log(groupName);
        if ($("a.contact.select").length == 1) {
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/createchat`);
            let data = new FormData();
            data.append("id", $("a.contact.select").data("id"));
            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    if (data.Code === NotifyType.Success) {
                        $(".modal-backdrop").addClass("hide");
                    }
                    else {
                        popup(data.Message, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popup(null, NotifyType.Error);
                }
            };
        }
        else if ($("a.contact.select").length > 1 && groupName.length > 0) {
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/creategroup`);
            let data = new FormData();
            let arr = new Array();
            $("a.contact.select").each(function (index) {
                arr.push($(this).data("id"));
            });
            data.append("idArr", JSON.stringify(arr));
            data.append("groupName", groupName);
            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    if (data.Code === NotifyType.Success) {
                        $(".modal-backdrop").addClass("hide");
                    }
                    else {
                        popup(data.Message, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popup(null, NotifyType.Error);
                }
            };
        }
    }
    else if (target.closest(".delete-contact")) {
        $(".sub-modal-dialog").removeClass("hide");
        $(".sub-modal-dialog").html("");
        $(".sub-modal-dialog").html(`
            <div class="sub-dialog-container">
                <h3>Вы уверены?</h3>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-delete-contact">удалить (${$("a.contact.select").length})</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".btn-delete-contact")) {//////////////
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/deletecontacts`);
        let data = new FormData();
        let arr = new Array();
        $("a.contact.select").each(function (index) {
            arr.push($(this).data("id"));
        });
        data.append("idArr", JSON.stringify(arr));
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    $(".sub-dialog-container .login").val("");
                    $(".sub-modal-dialog").addClass("hide");
                    $("a.button.delete-contact").addClass("hide");
                    $("a.button.new-contact").removeClass("hide");
                }
                else {
                    popup(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popup(null, NotifyType.Error);
            }
        };
    }
    else if (target.closest(".btn-cancel")) {
        $(".sub-modal-dialog").addClass("hide");
    }
    else if (target.closest(".btn-add-contact")) {
        let login = $(".sub-dialog-container .login").val();
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/addcontact`);
        let data = new FormData();
        data.append("login", login);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    $(".sub-dialog-container .login").val("");
                    $(".sub-modal-dialog").addClass("hide");
                }
                else {
                    popup(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popup(null, NotifyType.Error);
            }
        };
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

                let data2 = new FormData();
                data2.append("groupId", target.closest(".group").getAttribute("data-id"));
                let xhr2 = new XMLHttpRequest();
                xhr2.open('POST', `/readmessages`);
                xhr2.send(data2);

                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {

                    let chatsContainer = document.querySelector(".message-list-wrap.scroll-content");
                    let firstBoot = false;
                    if (chatsContainer == null) {
                        chatsContainer = document.querySelector(".message-list-wrap");
                        firstBoot = true;
                    }
                    let groupUl = document.querySelector('.message-list-wrap ul[data-id="' + data.groupId + '"]');
                    if (groupUl == null) {
                        groupUl = Generator.MessagesContainerHTML(data.groupId);
                        groupUl.classList.add("activeUl");

                        data.messages.forEach((message) => {
                            groupUl.appendChild(Generator.MessageHTML(message, null));
                        });
                        chatsContainer.appendChild(groupUl);
                    }
                    else {
                        groupUl.classList.add("activeUl");
                    }
                    changeActive(target.closest(".group").parentElement);
                    if (firstBoot) {
                        $('.scrollbar-macosx-messages').scrollbar({ disableBodyScroll: true });
                    }
                    $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
                }
                else {
                    popup(data.Error, data.Code);
                }
            } else if (xhr.readyState == 4 && xhr.status == 0) {
                popup(null, NotifyType.Error);
            }
        };
    }
    else if (target.matches("a.send")) {
        let hash = new Date().getTime();
        let text = $('.panel-write textarea[name=msg]').val();
        $('textarea[name=msg]').val("");
        if (text.trim().length > 0) {
            let groupId = document.querySelector(".message-list-wrap ul[data-id].activeUl").getAttribute("data-id");
            let messagesContainer = document.querySelector(".message-list-wrap ul.activeUl");

            messagesContainer.appendChild(
                Generator.MessageHTML({
                    Id: null,
                    Hash: hash,
                    Text: text,
                    GroupId: groupId,
                    DateTime: null,
                    Sender: {
                        Login: _currentUser.Login,
                        DisplayName: _currentUser.DisplayName,
                    },
                },
                    _currentUser.Avatar));
            $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);

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
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popup(null, NotifyType.Error);
                }
            };
        }
    } else if (target.closest(".message-list li:not(.systemMsg)")) {
        let li = target.closest(".message-list li");
        li.classList.toggle("active");
        if ($("ul.activeUl li.active").length > 0) {
            if (!$(".panel-write").hasClass("hide")) $(".panel-write").addClass("hide");
            if ($(".panel-select").hasClass("hide")) $(".panel-select").removeClass("hide");

        } else {
            if ($(".panel-write").hasClass("hide")) $(".panel-write").removeClass("hide");
            if (!$(".panel-select").hasClass("hide")) $(".panel-select").addClass("hide");
        }
    }
    else if (target.matches("a.btn-remove-messages")) {
        if ($("ul.activeUl li.active").length > 0) {
            let messages = [];
            $("ul.activeUl li.active").each((id, message) => {
                messages.push($(message).data("id"));
            });

            let data = new FormData();
            data.append("messagesIdJson", JSON.stringify(messages));

            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/deletemessages`);
            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    if (data.Code == NotifyType.Success) {
                        $("ul.activeUl li.active").removeClass("active");
                        if ($(".panel-write").hasClass("hide")) $(".panel-write").removeClass("hide");
                        if (!$(".panel-select").hasClass("hide")) $(".panel-select").addClass("hide");
                    } else if (data.Code == NotifyType.Error){
                        popup(data.Error, NotifyType.Error);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popup(null, NotifyType.Error);
                }
            };
        }
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
    var unreadMessages = elem.querySelector(".count-unred-messages");
    if (unreadMessages != null) unreadMessages.remove();
    $(".panel-write").removeClass("hide");

}

document.addEventListener("DOMContentLoaded", function () {
    $('.scrollbar-macosx-chats').scrollbar({ disableBodyScroll: true });
});

$('.scrollbar-macosx-messages').mousewheel(function (event) {
    if (parseFloat($(".scrollbar-macosx-messages .scroll-element.scroll-y .scroll-bar").css("top")) < 36) {
        console.log("loading");
    }
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

$('textarea[name=msg]').on('keydown', (e) => {
    e = e || window.event;
    if (e.keyCode === 13) {
        e.preventDefault();
    }
});

$('textarea[name=msg]').on('keyup', (e) => {
    e = e || window.event;
    if (e.keyCode === 13) {
        const input = document.querySelector("a.send");
        if (input !== null && input !== undefined) {
            input.click();
        }
    }
});

document.addEventListener('input', function (e) {
    let target = e.target;
    if (target.matches(".search")) {
        let filter = $(".search").val().toLowerCase();
        $(".contact").removeClass("hide");
        $(".contact h3").each(function (i, elem) {
            let login = $(elem).text().toLowerCase();
            if (login.indexOf(filter) == -1) {
                $(elem).parent().parent().parent().addClass('hide');
            }
            else {
                $(elem).parent().parent().parent().removeClass('hide');
            }
        });

    }
});
//$('input[type=text].search').change(function (e) {
//    console.log(e);
//});

function OpenMenu (methodName) {
    let xhr = new XMLHttpRequest();
    xhr.open('POST', `/` + methodName);
    xhr.send();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            let data = JSON.parse(xhr.responseText);
            if (data.Code === NotifyType.Success) {
                $(".dialog-container").html("");
                $(".dialog-container").html(data.view);
                $(".dialog-title > h2").text(data.title);
                $('.scrollbar-macosx-contacts').scrollbar({ disableBodyScroll: true });
                document.querySelector(".modal-backdrop").classList.remove("hide");
            }
            else {
                popup(data.Error, data.Code);
            }
        } else if (xhr.readyState == 4 && xhr.status == 0) {
            popup(null, NotifyType.Error);
        }
    };
}