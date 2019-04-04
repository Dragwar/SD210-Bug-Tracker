/** @description this will switch the active class on the my-page-link (nav-links) according to the current page*/
(() => {
    const currentPage = document.getElementById("currentPage");
    const pageLinks = [...document.getElementsByClassName("my-page-link")];
    let isHomePage = true;

    pageLinks.forEach(element => {
        if (currentPage.textContent == element.id) {
            isHomePage = false;
            element.classList.add("active");
        } else {
            element.classList.remove("active");
        }
    });

    if (isHomePage) {
        pageLinks.find(ele => ele.id == "home-index").classList.add("active");
    }
})();
