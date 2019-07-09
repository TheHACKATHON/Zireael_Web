function openGroup(groupId, newActiveElem) {
    hideChats();
    let groupUl = document.querySelector('.message-list-wrap ul[data-id="' + groupId + '"]');
    if (groupUl != null) {
        groupUl.classList.add("activeUl");
        changeActive(newActiveElem);
    }
    else {
        let data = new FormData();
        data.append("groupId", groupId);

        let xhr = new XMLHttpRequest();
        xhr.open('POST', `/getmessages`);
        xhr.send(data);

        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                if ($(window).width() <= 1030) $('.wrap').add('.my-head').addClass('checkDialog');

                let data2 = new FormData();
                data2.append("groupId", groupId);
                let xhr2 = new XMLHttpRequest();
                xhr2.open('POST', `/readmessages`);
                xhr2.send(data2);

                let data = JSON.parse(xhr.responseText);
                if (data.Code === NotifyType.Success) {

                    let chatsContainer = document.querySelector(".message-list-wrap.scroll-content");
                    let firstBoot = false;
                    if (chatsContainer == null) {
                        chatsContainer = document.querySelector(".message-list-wrap");
                        firstBoot = true;
                    }
                    let groupUl = Generator.MessagesContainerHTML(data.groupId);
                    groupUl.classList.add("activeUl");

                    data.messages.forEach((message) => {
                        groupUl.appendChild(Generator.MessageHTML(message, null));
                    });
                    chatsContainer.appendChild(groupUl);

                    if (firstBoot) {
                        $('.scrollbar-macosx-messages').scrollbar({ disableBodyScroll: true });
                    }
                    calc();
                    $('.scrollbar-macosx-messages').scrollTop($('.scrollbar-macosx-messages').height() * 100);
                    changeActive(newActiveElem);
                }
                else {
                    popover(data.Error, data.Code);
                }
            } else if (xhr.readyState == 4 && xhr.status == 0) {
                popover(null, NotifyType.Error);
            }
        }
    }
}

function hideChats() {
    $(".message-list-wrap ul").removeClass("activeUl");
    $(".panel-write").addClass("hide");
    $(".chats li.active").removeClass("active");
}

function changeActive(elem) {
    $(".chats li.active").removeClass("active");
    elem.classList.add("active");
    var unreadMessages = elem.querySelector(".count-unred-messages");
    if (unreadMessages != null) unreadMessages.remove();
    $(".panel-write").removeClass("hide");

}