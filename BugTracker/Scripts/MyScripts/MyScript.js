/** @description this will switch the active class on the my-page-link (nav-links) according to the current page*/
(() => {
    const currentPage = document.getElementById("currentPage").dataset.page;
    const pageLinks = [...document.getElementsByClassName("my-page-link")];
    let isHomePage = true;
    let isNone = currentPage.toLowerCase().includes("none");

    pageLinks.forEach(element => {
        if (currentPage == element.id) {
            isHomePage = false;
            element.classList.add("active");
        } else {
            element.classList.remove("active");
        }
    });

    if (isHomePage && !isNone) {
        pageLinks.find(ele => ele.id == "home-index").classList.add("active");
    }
})();
