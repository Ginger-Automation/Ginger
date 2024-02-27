var buttons = document.getElementsByClassName("sectionbutton");
var i;

for (i = 0; i < buttons.length; i++) {
    buttons[i].addEventListener("click", function () {
        var expandoText = this.getElementsByClassName("buttonExpandoText")[0];

        this.classList.toggle("active");

        var content = this.nextElementSibling;
        if (expandoText.innerHTML === "-") {
            content.style.maxHeight = 0;
            expandoText.innerHTML = "+";
        } else {
            content.style.maxHeight = content.scrollHeight + "px";
            expandoText.innerHTML = "-";
        }
    });
}

var thumbnail = document.getElementById("screenshotThumbnail");
var thumbnailStyle = getComputedStyle(thumbnail);
var modal = document.getElementById("modal");
var modalimg = modal.getElementsByTagName("img")[0];

modal.addEventListener("click", function () {
    modal.style.display = "none";
    modalimg.style.content = "";
    modalimg.alt = "";
});

thumbnail.addEventListener("click", function () {
    modal.style.display = "flex";
    modalimg.style.content = thumbnailStyle.content;
    modalimg.alt = thumbnail.alt;
});