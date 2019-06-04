var Generator = {
    DialogHTML: function(group, avatar, defaultAvatar, notReadMessageCount) {
        let tickdate = group.LastMessage.DateTime.substring(6, message.DateTime.indexOf(")"));
        let date = new Date(parseInt(tickdate));
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
            `<li>
    <a data-id="${group.Id}" class="group">
        <div} class="wrap-img letter">
            <h2>${usernameLetter}</h2>
            <img src="avatar" alt="logo">
        </div>
        <div class="super">
            <div class="super-top">
                <h3>${group.Name}</h3>
                <p class="time">${date.toLocaleTimeString()}</p>
            </div>
            <div class="super-bottom">
                <p class="last-message">${group.LastMessage.Text}</p>
                ${notReadMessageElem}
            </div>
        </div>
    </a>
</li>`;

        return li;
    },
    MessageHTML: function(message, avatar, defaultAvatar) {
        let tickdate = message.DateTime.substring(6, message.DateTime.indexOf(")"));
        let date = new Date(parseInt(tickdate));
        let usernameLetter = avatar.avatar == defaultAvatar ? message.Sender.DisplayName.substring(0, 1) : "";

        let li = document.createElement("li");
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
    <p class="time">${date.toLocaleTimeString()} ${date.toLocaleDateString()}</p >
</div>`;

        return li;
    },

}