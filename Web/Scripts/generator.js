var Generator = {
    DialogHTML: function(group, avatar, defaultAvatar, notReadMessageCount) {
        let dateString = convertAjaxDate(group.LastMessage.DateTime);
        let usernameLetter = avatar.avatar == defaultAvatar ? group.Name.substring(0, 1) : "";
        let notReadMessageElem = "";
        if (parseInt(notReadMessageCount) > 0) {
            notReadMessageElem =
`<span class="count-unred-messages">
    ${notReadMessageCount}
</span>`;
        }


        let li = document.createElement("li");
        li.innerHTML =
`<a data-id="${group.Id}" class="group">
        <div} class="wrap-img letter">
            <h2>${usernameLetter}</h2>
            <img src="avatar" alt="logo">
        </div>
        <div class="super">
            <div class="super-top">
                <h3>${group.Name}</h3>
                <p class="time">${dateString}</p>
            </div>
            <div class="super-bottom">
                <p class="last-message">${group.LastMessage.Text}</p>
                ${notReadMessageElem}
            </div>
        </div>
    </a>`;

        return li;
    },
    MessageHTML: function (message, avatar, defaultAvatar) {
        let li = document.createElement("li");
        if (message.Id == null) {
            li.setAttribute("data-hash", message.Hash);
        }
        else {
            li.setAttribute("data-id", message.Id);
        }

        if (message.Sender.Login != null && message.Sender.Login === "system" && message.Sender.Id == 1) {
            li.classList.add("systemMsg");
            li.innerHTML =
`
    <span>${message.Text}</span>
`;
            return li;
        }

        let dateString = "";
        if (message.DateTime !=  null) {
            dateString = convertAjaxDate(message.DateTime);
        }
        else {
            dateString = "загрузка";
            // todo: переделать на знак
        }
        
        let usernameLetter = avatar.avatar == defaultAvatar ? message.Sender.DisplayName.substring(0, 1) : "";
        li.innerHTML =
`<div class="checked-btn">
   <a href="#" class="checked-btn-on" style="background-image: url(../Content/Images/Icons2.png);background-repeat: no-repeat; background-position: -9px -481px;"></a>
</div>
<div class="wrap-img letter">
    <h2>${usernameLetter}</h2>
    <img src="${avatar.avatar}" alt="logo">
</div>
<div class="mega-left" style=" width: calc(100% - 300px);padding-right: 10px;">
   <h3>${message.Sender.DisplayName}</h3>
   <p>${message.Text}</p>
</div>
<div class="mega-right">
    <p class="time">${dateString}</p >
</div>`;

        return li;
    },
    MessagesContainerHTML: function (groupId) {
        let ul = document.createElement("ul");
        ul.setAttribute("data-id", groupId);
        ul.classList.add("message-list");
        return ul;
    },
}

function convertAjaxDate(ajaxDate) {
    let tickdate = ajaxDate.substring(6, ajaxDate.indexOf(")"));
    let date = new Date(parseInt(tickdate));
    dateString = `${date.toLocaleTimeString()} ${date.toLocaleDateString()}`;

    // todo: в засивимости от дня редактировать строку
    return dateString;
}