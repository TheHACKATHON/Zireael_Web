var _currentUser = null;
var _defaultAvatar = "/Content/Images/Zireael_back.png";


document.addEventListener('click', function (e) {

    let target = e.target;

    if (!target.matches('label, img, label[for="file"], input[type="file"]')) {
        e.preventDefault();
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
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>Добавление контакта</h3>
                <input required="" autocomplete="login" class="text login" type="text" placeholder="Логин пользователя"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-add-contact">добавить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".settings-container .change-login")) {
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>Изменение логина для ${_currentUser.Login}</h3>
                <div class="info-container">
                      <h4>Требования для логина:</h4>
                      <p>только латинские символы и цифры<br>минимум 6 символов<br>максимум 24 символа</p>
                </div>
                <input required="" autocomplete="login" class="text login" type="text" placeholder="Новый логин"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-change-login">изменить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".settings-container .change-password")) {
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>Изменение пароля для ${_currentUser.Login}</h3>
                <div class="info-container">
                      <h4>Требования для пароля:</h4>
                      <p>минимум 8 символов<br>максимум 32 символа<br>буква в нижнем регистре<br>буква в верхнем регистре</p>
                </div>
                     <input required="" class="text new-password" type="password" placeholder="Новый пароль"/>
                     <input required="" class="text rep-new-password" type="password" placeholder="Повторите пароль"/>
                     <input required="" class="text old-password" type="password" placeholder="Текущий пароль"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-change-password">изменить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".settings-container .change-display-name")) {
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>Изменение отображаемого имени</h3>
                <input required="" class="text display-name" type="text" placeholder="Новое имя"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-change-display-name">изменить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".settings-container .change-email")) {
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>${_currentUser.Email == null ? "Почта не прикреплена" : _currentUser.Email}</h3>
                <div class="new-email">
                    <input required="" class="text email" type="email" placeholder="Новая почта"/>
                    <input required="" class="text password" type="password" placeholder="Пароль от аккаунта"/>
                    <a class="button btn-send-code">отправить код на почту</a>
                </div>
                <input required="" class="text code hide" type="text" placeholder="Код отправленный на почту"/>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a disabled class="button btn-change-email">изменить</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".create-group")) {
        let groupName = $(".group-name").val();
        if ($("a.contact.select").length > 0 && groupName.length > 0) {
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
                        popover(data.Message, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
        else if ($("a.contact.select").length == 1) {
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
                        popover(data.Message, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
    }
    else if (target.closest(".delete-contact")) {
        OpenSubModalDialog(`
            <div class="sub-dialog-container">
                <h3>Вы уверены?</h3>
                <div class="new-contact-btn-container">
                    <a class="button btn-cancel">отмена</a>
                    <a class="button btn-delete-contact">удалить (${$("a.contact.select").length})</a>
                </div>
            </div>
        `);
    }
    else if (target.closest(".btn-delete-contact")) {
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
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
    else if (target.closest(".btn-change-display-name")) {
        let displayName = $(".sub-dialog-container .display-name").val();
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/changedisplayname`);
        let data = new FormData();
        data.append("displayName", displayName);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    $(".sub-dialog-container .displayName").val("");
                    $(".sub-modal-dialog").addClass("hide");
                    _currentUser.DisplayName = displayName;
                    _currentUser.Avatar = `/user/${_currentUser.Id}/${Crypto.MD5(_currentUser.DisplayName)}`;
                    $(".profile .wrap-img img").attr("src", `/user/${_currentUser.Id}/${Crypto.MD5(_currentUser.DisplayName)}`);
                    $(".profile h3").html(displayName);
                    $("li[sender-id=\"" + _currentUser.Id + "\"] img").attr("src", `/user/${_currentUser.Id}/${Crypto.MD5(_currentUser.DisplayName)}`);
                    $("li[sender-id='" + _currentUser.Id + "'] h3").html(displayName);
                    popover(data.Message, data.Code);
                }
                else {
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
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
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
    else if (target.closest(".btn-change-login")) {
        let login = $(".sub-dialog-container .login").val();
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/changelogin`);
        let data = new FormData();
        data.append("login", login);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    $(".sub-dialog-container .login").val("");
                    $(".sub-modal-dialog").addClass("hide");
                    _currentUser.Login = login;
                    popover(data.Message, data.Code);
                }
                else {
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
    else if (target.closest(".btn-change-password")) {
        let newPass = $(".sub-dialog-container .new-password").val();
        let repNewPass = $(".sub-dialog-container .rep-new-password").val();
        let oldPass = $(".sub-dialog-container .old-password").val();
        if (newPass != repNewPass) {
            popover("Пароли НЕ совпадают!", NotifyType.Warning);
        }
        else {
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/changepassword`);
            let data = new FormData();
            data.append("newPass", newPass);
            data.append("repNewPass", repNewPass);
            data.append("oldPass", oldPass);

            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    if (data.Code === NotifyType.Success) {
                        $(".sub-modal-dialog").addClass("hide");
                        $(".sub-dialog-container .new-password").val("");
                        $(".sub-dialog-container .rep-new-password").val("")
                        $(".sub-dialog-container .old-password").val("");
                        popover(data.Message, data.Code);
                    }
                    else {
                        popover(data.Message, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
    }
    else if (target.closest(".btn-change-email")) {
        let code = $(".sub-dialog-container .code").val();
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/changeemail`);
        let data = new FormData();
        data.append("code", code);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    $(".sub-modal-dialog").addClass("hide");
                    $(".sub-dialog-container .new-email .password").val("");
                    $(".sub-dialog-container .new-email .email").val("");
                    $(".sub-dialog-container .code").val("");
                    _currentUser.Email = _currentUser.tempEmail;
                    _currentUser.tempEmai = undefined;
                    $(".sub-dialog-container .new-email .email").val(_currentUser.Email);
                    popover(data.Message, data.Code);
                }
                else {
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
    else if (target.closest(".btn-send-code")) {
        let pass = $(".sub-dialog-container .new-email .password").val();
        let newEmail = $(".sub-dialog-container .new-email .email").val();

        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/changeemailsendcode`);
        let data = new FormData();
        data.append("newEmail", newEmail);
        data.append("pass", pass);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    popover(data.Message, data.Code);
                    $(".sub-dialog-container .code").val("");
                    $(".sub-dialog-container .code.hide").removeClass("hide");
                    _currentUser.tempEmail = newEmail;
                }
                else {
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
    else if (target.matches('.backBtn')) {
        $('.wrap').add('.my-head').removeClass('checkDialog');
        hideChats();
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
                if ($(window).width() <= 1030) $('.wrap').add('.my-head').addClass('checkDialog');

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
                    calc();
                    $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
                }
                else {
                    popover(data.Error, data.Code);
                }
            } else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
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
                    Sender: _currentUser,
                }, _currentUser.Avatar));
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
                        popover(data.Error, data.Code);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
    }
    else if (target.closest(".message-list li:not(.systemMsg) img")) {
        let obj = target.closest(".message-list li:not(.systemMsg)");
        let login = obj.getAttribute("sender-login");
        
    }
    else if (target.closest(".message-list li:not(.systemMsg) .mega-left h3")) {
        let obj = target.closest(".message-list li:not(.systemMsg)");
        let login = obj.getAttribute("sender-login");
        
    }
    else if (target.closest(".message-list li:not(.systemMsg)")) {
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
                    } else if (data.Code == NotifyType.Error) {
                        popover(data.Error, NotifyType.Error);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
    }

});
function OpenSubModalDialog(html)
{
    $(".sub-modal-dialog").removeClass("hide");
    $(".sub-modal-dialog").html("");
    $(".sub-modal-dialog").html(html);
    // todo посчитать размер окна
}


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

function calc() {
    let check = ($(window).width() > 1030) ? true : false;
    let wHeight = $(window).height();
    let headHeight = $('.my-head').height() + ((check) ? 20 : 0);
    let panelHeight = $('.panel-write').height() + 10 + (check ? 20 : 20);

    let popupHeaderHeight = $(".dialog-header").height();
    let popupSearchHeight = $(".search-container").height();
    let popupButton = $(".dialog-container .button").height();

    $(".modal-backdrop-dialog").height(wHeight - 30);
    $(".scroll-wrapper.scrollbar-macosx-creategroup").height(wHeight - popupButton - popupHeaderHeight - popupSearchHeight - popupSearchHeight - 100);
    $(".scroll-wrapper.scrollbar-macosx-contacts").height(wHeight - popupButton - popupHeaderHeight - popupSearchHeight - 100);

    $('.wrap').height(wHeight - headHeight);
    $('.message-list-wrap').height(wHeight - headHeight - panelHeight);
    $('.back').height($(window).height() - 30);
}

$(window).on("load", function () {
    $('.scrollbar-macosx-chats').scrollbar({ disableBodyScroll: true });
    calc();
})

$(window).on("resize", function () {
    calc();
});

$('.scrollbar-macosx-messages').mousewheel(function (event) {
    if (parseFloat($(".scrollbar-macosx-messages .scroll-element.scroll-y .scroll-bar").css("top")) < 40) {
        let activeUl = $(".messages ul.activeUl");
        if (!activeUl.hasClass("loading")) {
            activeUl.addClass("loading");
            let data = new FormData();
            data.append("groupId", activeUl.data("id"));
            data.append("lastMessageId", $(".messages ul.activeUl li").first().data("id"));

            let xhr = new XMLHttpRequest();
            xhr.open('POST', `/getoldmessages`);
            xhr.send(data);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    let data = JSON.parse(xhr.responseText);
                    let messages = data.messages;
                    if (data.Code == NotifyType.Success) {
                        if (messages != null && messages.length > 0) {
                            $('.scrollbar-macosx-messages').scrollTop(1000);
                            messages.forEach((msg, i) => {
                                activeUl.prepend(Generator.MessageHTML(msg));
                            });
                            activeUl.removeClass("loading");
                            
                        }
                        
                    } else if (data.Code == NotifyType.Error) {
                        popover(data.Error, NotifyType.Error);
                    }
                }
                else if (xhr.readyState == 4 && xhr.status == 0) {
                    popover(null, NotifyType.Error);
                }
            };
        }
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
document.addEventListener("change", function (e) {
    let target = e.target;
    if (target.matches(".wrap-img input[type=file]")) {
        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/changeimage`);
        let data = new FormData();
        let files = document.querySelector(".wrap-img input[type=file]").files;
        console.log(files);
        for (let i = 0; i < files.length; i++) {
            data.append(files[i].name, files[i]);
        }
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {
                    popover(data.Message, data.Code);
                }
                else {
                    popover(data.Message, data.Code);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        };
    }
});


function OpenMenu(methodName) {
    $("#loading").css("display", "block");
    let xhr = new XMLHttpRequest();
    xhr.open('POST', `/` + methodName);
    xhr.send();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            $("#loading").css("display", "none");
            let data = JSON.parse(xhr.responseText);
            if (data.Code === NotifyType.Success) {
                let div = document.createElement("div");
                div.innerHTML = data.view;
                let img = div.querySelector("img");
                img.setAttribute("src", "/" + _currentUser.Avatar);
                data.view = div.innerHTML;
                $(".dialog-container").html("");
                $(".dialog-container").html(data.view);
                $(".dialog-title > h2").text(data.title);
                $('.scrollbar-macosx-contacts').scrollbar({ disableBodyScroll: true });
                $('.scrollbar-macosx-creategroup').scrollbar({ disableBodyScroll: true });
                $(".modal-backdrop").removeClass("hide");
                $(".sub-modal-dialog").addClass("hide");

                calc();
            }
            else {
                popover(data.Error, data.Code);
            }
        } else if (xhr.readyState == 4 && xhr.status == 0) {
            popover(null, NotifyType.Error);
        }
    };
}