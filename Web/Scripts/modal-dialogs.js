﻿document.addEventListener('click', function (e) {
    let target = e.target;
    if (target.matches(".md_modal_action_close") || target.matches(".modal-backdrop")) {
        document.querySelector(".modal-backdrop").classList.add("hide");
        //todo: закрывать только второй попап если он открыт
    }
});