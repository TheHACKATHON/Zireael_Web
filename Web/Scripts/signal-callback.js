document.addEventListener('DOMContentLoaded', () => {
    var chat = $.connection.chatHub; 
    chat.client.addChat = function (group, creatorId) {
        let chats = document.querySelector(".chats ul");
        chats.insertBefore(Generator.DialogHTML(group), chats.firstChild);

    };

    chat.client.logout = function () {
        deleteCookie("SessionId");
        location.reload();
    };

    chat.client.addMessage = function (message, hash) {
        //найти сообщение по id и если оно не отправлено (class=notSended) пометить отправленным
        let messageElement = document.querySelector('.message-list-wrap li[data-hash="' + hash + '"]')
        messageElement.removeAttribute("data-hash");
        messageElement.setAttribute("data-id", message.Id);
        let time = messageElement.querySelector("p.time");
        time.textContent = new Date(message.DateTime).toTimeString();
        //todo: добавить класс который означает что сообщение пришло
    };

    $.connection.hub.start().done(function () {
        chat.server.connect().done((result) => {
            _currentUser = JSON.parse(result);
        });
    });
});

// #region cookie
function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    })
}

function setCookie(name, value, options) {
    options = options || {};

    var expires = options.expires;

    if (typeof expires == "number" && expires) {
        var d = new Date();
        d.setTime(d.getTime() + expires * 1000);
        expires = options.expires = d;
    }
    if (expires && expires.toUTCString) {
        options.expires = expires.toUTCString();
    }

    value = encodeURIComponent(value);

    var updatedCookie = name + "=" + value;

    for (var propName in options) {
        updatedCookie += "; " + propName;
        var propValue = options[propName];
        if (propValue !== true) {
            updatedCookie += "=" + propValue;
        }
    }

    document.cookie = updatedCookie;
}

//#endregion