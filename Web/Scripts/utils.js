$(function () {
   
    // Объявление функции, которая хаб вызывает при получении сообщений
   
    
        // Открываем соединение
    
});

document.addEventListener('DOMContentLoaded', () => {
    var chat = $.connection.chatHub; 
    chat.client.addChat = function (group, creatorId) {
        console.log(group, creatorId);
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