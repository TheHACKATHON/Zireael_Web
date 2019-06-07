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

    chat.client.giveMessageId = function (data) {
        if (data) {
            data = JSON.parse(data);
            data.forEach((item) => {
                let messageElement = document.querySelector('.message-list-wrap li[data-hash="' + item.Key + '"]');
                if (messageElement != null) {
                    messageElement.removeAttribute("data-hash");
                    messageElement.setAttribute("data-id", item.Value);
                }
            });
        }
    };

    chat.client.addMessage = function (message, hash) {
        let messageElement = document.querySelector('.message-list-wrap li[data-hash="' + hash + '"]');
        if (messageElement != null) {
            let time = messageElement.querySelector("p.time");
            let date = new Date(message.DateTime);
            time.textContent = `${date.toLocaleTimeString()} ${date.toLocaleDateString()}`;
        } else {
            console.log(message);
            let ul = document.querySelector('.message-list-wrap ul[data-id="' + message.GroupId + '"]');
            if (ul != null) {
                message.Hash = hash;
                ul.appendChild(Generator.MessageHTML(message, null));
                $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
            }
        }
    };

    $.connection.hub.start().done(function () {
        chat.server.connect().done((result) => {
            _currentUser = JSON.parse(result);
            $("#loading").css("display", "none");
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

function propName(prop, value) {
    for (var i in prop) {
        if (prop[i] == value) {
            return i;
        }
    }
    return false;
}
//#endregion