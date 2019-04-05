const styleIt = () => {
    const search = $("#project-users_filter").children("label").children("input");
    const pageBtn = $(".paginate_button");
    const dropdown = $("#project-users_length").children("label").children("select");
    const projectWrapper = $("#project-users_wrapper");

    search.addClass("form-control");
    pageBtn.addClass("btn btn-secondary");
    pageBtn.css("margin", "0px 5px")
    dropdown.addClass("form-control");
    projectWrapper.css("display", "flex");
    projectWrapper.css("flex-flow", "row wrap");
    projectWrapper.css("justify-content", "space-between");
};

$(document).ready(() => {
    $("#project-users").DataTable();
    styleIt();

    // update page buttons when searching
    document.querySelector("#project-users_filter label input").addEventListener("keyup", styleIt);

    // update page buttons when selecting from drop-down list
    document.querySelector("#project-users_length label select").addEventListener("change", styleIt);

    // update page buttons when selecting from drop-down list
    document.body.addEventListener("click", styleIt);
});
