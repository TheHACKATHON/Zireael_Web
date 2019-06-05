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
    };

    $.connection.hub.start().done(function () {
        chat.server.connect(1);
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