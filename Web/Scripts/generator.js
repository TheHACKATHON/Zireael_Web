var Generator = {
    DialogHTML: function (group, avatar, defaultAvatar, notReadMessageCount) {
        if (avatar == null) {
            avatar = `/group/${group.Id}/${Crypto.MD5(group.Name)}`;
        }

        let dateString = convertDateToShortTimeString(group.LastMessage.DateTime);
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
        <div class="wrap-img">
            <img src="${avatar}" alt="logo">
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
    MessageHTML: function (message, avatar) {
        if (avatar == null) {
            if (message.Sender.Id == _currentUser.Id) {
                avatar = _currentUser.Avatar;
            } else {
                avatar = `/user/${message.Sender.Id}/${Crypto.MD5(message.Sender.DisplayName)}`;
            }
            
        }

        let li = document.createElement("li");
        if (message.Id == null || message.Id == 0) {
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
            dateString = convertDateToFullDateString(message.DateTime);
        }
        else {
            dateString = "загрузка";
            // todo: переделать на знак
        }
        
        li.innerHTML =
`<div class="checked-btn">
   <a href="#" class="checked-btn-on" style="background-image: url(../Content/Images/Icons2.png);background-repeat: no-repeat; background-position: -9px -481px;"></a>
</div>
<div class="wrap-img letter">
    <img src="${avatar}" alt="logo">
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
        ul.classList.add("permanent");
        
        return ul;
    },
    UnreadMessages: function () {
        let span = document.createElement("span");
        span.classList.add("count-unred-messages");
        span.textContent = 1;
        return span;
    },
}

function convertJsonToDate(jsonDate) {
    let date = null;
    if (jsonDate.startsWith("/Date")) {
        let tickdate = jsonDate.substring(6, jsonDate.indexOf(")"));
        date = new Date(parseInt(tickdate));
    } else {
        date = new Date(jsonDate);
    }
    return date;
}

function convertDateToShortTimeString(dateTime) {
    if (typeof dateTime == "string") {
        dateTime = convertJsonToDate(dateTime);
    }
    
    dateString = dateTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    return dateString;
}

function convertDateToFullDateString(dateTime) {
    if (typeof dateTime == "string") {
        dateTime = convertJsonToDate(dateTime);
    }
    
    dateString = `${dateTime.toLocaleTimeString()} ${dateTime.toLocaleDateString()}`;
    return dateString;
}

function DateConvert(dateTime) {
    if (typeof dateTime == "string") {
        dateTime = convertJsonToDate(dateTime);
    }

    if (dateTime)
    {
        var now = new Date();
        if (dateTime.getFullYear() == now.getFullYear() && dateTime.getMonth() == now.getMonth() && dateTime.getDate() == now.getDate()) {
            let result = `был онлайн сегодня в ${dateTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;

            let tmpDate = now - dateTime;
            let minutes = Math.floor(tmpDate / 1000 / 60);
            let hours = Math.floor(tmpDate / 1000 / 60 / 60);

            if (hours < 1) {
                if (minutes >= 1) {
                    result = `был онлайн ${minutes} ${minutesToStringConverter(minutes)} назад`;
                }
                else {
                    result = `был онлайн только что`;
                }

            }
            else {
                if (hours == 1) {
                    result = `был онлайн час назад`;
                }
                else {
                    result = `был онлайн ${hours} ${hoursToStringConverter(hours)} назад`;
                }

            }

            return result;

        }

        if (dateTime.getFullYear() == now.getFullYear() && dateTime.getMonth() == now.getMonth() && dateTime.getDate() == now.getDate() - 1) {
            return `был онлайн вчера в ${dateTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;
        }

        if (dateTime.getFullYear() != now.getFullYear()) {
            

        }
        return `был онлайн ${dateTime.toLocaleDateString([], { day: "2-digit", month: "2-digit", year: "numeric" })} в ${dateTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;
        //return `был онлайн ${dateTime.toLocaleDateString([], { day: "2-digit", month: "2-digit" })} в ${dateTime.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;
    }

    return null;
}


function minutesToStringConverter(min) {
    min = parseInt(min);
    var minString = "минут";

    if (min < 10) {
        switch (min) {
            case 1:
                {
                    minString += "у";
                }
                break;
            case 2:
            case 3:
            case 4:
                {
                    minString += "ы";
                }
                break;
        }
    }
    else if (min < 20) {

    }
    else {
        while (min > 10) {
            min -= 10;
        }
        switch (min) {
            case 1:
                {
                    minString += "у";
                }
                break;
            case 2:
            case 3:
            case 4:
                {
                    minString += "ы";
                }
                break;
        }
    }

    return minString;
}

function hoursToStringConverter(hour)
{
    hour = parseInt(hour);
    var minString = "час";

    if (hour < 10) {
        switch (hour) {
            case 2:
            case 3:
            case 4:
                {
                    minString += "а";
                }
                break;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                {
                    minString += "ов";
                }
                break;
        }
    }
    else if (hour <= 20) {
        minString += "ов";
    }
    else if (hour < 24) {
        while (hour > 10) {
            hour -= 10;
        }
        switch (hour) {
            case 2:
            case 3:
                {
                    minString += "а";
                }
                break;
        }
    }

    return minString;
}