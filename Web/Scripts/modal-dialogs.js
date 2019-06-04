document.addEventListener('click', function (e) {
    let target = e.target;

    if (target.matches(".md_modal_action_close")) {
        document.querySelector(".modal-backdrop").classList.add("hide");
    }

});

