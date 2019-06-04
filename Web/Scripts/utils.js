﻿$(function () {
   
    // Объявление функции, которая хаб вызывает при получении сообщений
   
    
        // Открываем соединение
    
});

document.addEventListener('DOMContentLoaded', () => {
    var chat = $.connection.chatHub; 
    chat.client.addChat = function (group, creatorId) {
        console.log(group, creatorId);
    };

    chat.client.logout = function () {
        deleteCookie("SessionId");
        location.reload();
    };

    $.connection.hub.start().done(function () {
        chat.server.connect(1);
    });
});

// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
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