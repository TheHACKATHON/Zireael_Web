//$(".main-menu").on('click', () => {
//    $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").removeClass("open")
//    $(".tg_head_logo_dropdown.dropdown").toggleClass("open");
//});

//$(".dots").on('click', () => {
//    $(".tg_head_logo_dropdown.dropdown").removeClass("open");
//    $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").toggleClass("open");
//});

document.addEventListener('click', function (e) {
    e.preventDefault();
    let target = e.target;

    if (target.matches(".main-menu") || target.closest(".main-menu")) {
        $(".tg_head_logo_dropdown.dropdown").toggleClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").removeClass("open");

    } else if (target.matches(".dots") || target.closest(".dots")) {
        $(".tg_head_logo_dropdown.dropdown").removeClass("open");
        $(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").toggleClass("open");
    }
    else if (target.closest(".menu-settings")) {
        document.querySelector(".modal-backdrop").classList.remove("hide");
    }
    else if (target.closest(".menu-exit")) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', `/logout`);
        xhr.send();
    }
    else if (target.closest(".group")) {
        let data = new FormData();
//console.log(target.closest(".group").getAttribute("data-id"));
        data.append("groupId", target.closest(".group").getAttribute("data-id"));

        var xhr = new XMLHttpRequest();
        xhr.open('POST', `/getmessages`);
        xhr.send(data);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                let messagesUl = document.querySelector(".message-list");
                //messagesUl.innerHTML = "";
                let data = JSON.parse(xhr.responseText);

                data.messages.forEach((message) => {
                    messagesUl.appendChild(getMessageHTML(message, data.avatarsDictionary.find(a => a.userId == message.Sender.Id), data._defaultAvatar));
                });
            }
        };
    }
    else {
        $(".tg_head_logo_dropdown.dropdown").
            add(".tg_head_peer_dropdown.tg_head_peer_media_dropdown.dropdown").
            removeClass("open");
    }
});

function getMessageHTML(message, avatar, defaultAvatar) {
    let date = message.DateTime.substring(6, message.DateTime.indexOf(")"));
    let tickDate = new Date(parseInt(date));
    let usernameLetter = avatar.avatar == defaultAvatar ? message.Sender.DisplayName.substring(0, 1) : "";

    console.log(tickDate);
    let li = document.createElement("li");
    li.innerHTML = `
 <div class="checked-btn">
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
     <p class="time">${tickDate.toLocaleTimeString()} ${tickDate.toLocaleDateString()}</p >
 </div>`;

    return li;
}

/*
 * 
 * <li>
                    <div class="checked-btn">
                        <a href="#" class="checked-btn-on" style="    background-image: url(../Content/Images/Icons2.png);background-repeat: no-repeat; background-position: -9px -481px;"></a>
                    </div>
                    <div class="wrap-img">
                        <img src="/Content/Images/Zireael_logo.128.png" alt="logo">
                    </div>
                    <div class="mega-left" style="
    width: calc(100% - 300px);
    padding-right: 10px;
">
                        <h3>Title</h3>
                        <p>Lorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds dfLorem fdsfsdf dfsdf dfds df</p>
                        <p>Lorem fdsfsdf dfsdf dfds df</p>
                        <p>Lorem fdsfsdf dfsdf dfds df</p>
                        <p>Lorem fdsfsdf dfsdf dfds df</p>
                        <p>Lorem fdsfsdf dfsdf dfds df</p>


                    </div>
                    <div class="mega-right">

                        <p class="time">10.01.01</p>
                    </div>
                </li>

*/