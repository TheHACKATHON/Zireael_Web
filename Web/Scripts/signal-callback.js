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

    chat.client.deleteMessage = function(messageId) {
        if (messageId) {
            $('ul.message-list li[data-id="' + messageId + '"]').remove();

        }
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

    chat.client.newLastMessage = function(message) {
        if (message) {
            var date = new Date(message.DateTime);
            let li = document.querySelector('.chats li a[data-id="' + message.GroupId + '"]');
            li = li.parentElement;
            li.querySelector(".last-message").textContent = message.Text;
            li.querySelector("p.time").textContent = `${date.getHours()}:${date.getMinutes()}`;
            let unreadElement = li.querySelector("span.count-unred-messages");
            if (unreadElement != null) {
                let unreadCount = parseInt(unreadElement.textContent);
                unreadElement.textContent = unreadCount + 1;
            } else {
                if (!li.classList.contains("active")) li.querySelector(".super-bottom").appendChild(Generator.UnreadMessages());
            }
            li.remove();

            let chats = document.querySelector(".chats ul");
            chats.insertBefore(li, chats.firstChild);
        }
    };

    chat.client.addMessage = function (message, hash) {
        let messageElement = document.querySelector('.message-list-wrap li[data-hash="' + hash + '"]');
        if (messageElement != null) {
            let time = messageElement.querySelector("p.time");
            let date = new Date(message.DateTime);
            time.textContent = `${date.toLocaleTimeString()} ${date.toLocaleDateString()}`;
        } else {
            let ul = document.querySelector('.message-list-wrap ul[data-id="' + message.GroupId + '"]');
            if (ul != null) {
                message.Hash = hash;
                ul.appendChild(Generator.MessageHTML(message, null));
                $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
            }
        }
    }; 
    chat.client.removeContact = function (id) {
        $(".contacts-container .contact[data-id=" + id + "]").parent().remove();
    }
    chat.client.addContact = function (user, hash) {
        let date = new Date(user.LastTimeOnline);
        let contactHtml =
            `
                <li>
                    <a data-id="${user.Id}" class="contact">
                        <div class="wrap-img">
                            <img src="/user/${user.Id}/${hash}" alt="logo">
                        </div>
                        <div class="super">
                            <div class="super-top">
                                <h3>${user.DisplayName}</h3>
                            </div>
                            <div class="super-bottom">
                                <p class="time">последний онлайн ${date.toLocaleDateString()} ${date.toLocaleTimeString()}</p>
                            </div>
                        </div>
                    </a>
                </li>
            `;
        //console.log(contactHtml);
        //console.log($(".contacts-container:last-child"));
        if ($(".contacts-container li").length > 0) {
            $(".contacts-container li").last().after(contactHtml);
        }
        else {
            $(".contacts-container").html(contactHtml);
        }
    }

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