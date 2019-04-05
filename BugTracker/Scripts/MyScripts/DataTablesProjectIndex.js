const styleIt = () => {
    const search = $("#my-projects_filter").children("label").children("input");
    const pageBtn = $(".paginate_button");
    const dropdown = $("#my-projects_length").children("label").children("select");
    const projectWrapper = $("#my-projects_wrapper");

    search.addClass("form-control");
    pageBtn.addClass("btn btn-secondary");
    pageBtn.css("margin", "0px 5px")
    dropdown.addClass("form-control");
    projectWrapper.css("display", "flex");
    projectWrapper.css("flex-flow", "row wrap");
    projectWrapper.css("justify-content", "space-between");
};

$(document).ready(() => {
    $("#my-projects").DataTable();
    styleIt();

    // update page buttons when searching
    document.querySelector("#my-projects_filter label input").addEventListener("keyup", styleIt);

    // update page buttons when selecting from drop-down list
    document.querySelector("#my-projects_length label select").addEventListener("change", styleIt);

    // update page buttons when selecting from drop-down list
    document.body.addEventListener("click", styleIt);
});
