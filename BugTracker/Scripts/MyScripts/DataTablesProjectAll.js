$(document).ready(() => {
    $("#all-projects").DataTable();
    const search = $("#all-projects_filter").children("label").children("input");
    const pageBtn = $(".paginate_button");
    const dropdown = $("#all-projects_length").children("label").children("select");
    const projectWrapper = $("#all-projects_wrapper");

    search.addClass("form-control");
    pageBtn.addClass("btn btn-secondary");
    pageBtn.css("margin", "0px 5px")
    dropdown.addClass("form-control");
    projectWrapper.css("display", "flex");
    projectWrapper.css("flex-flow", "row wrap");
    projectWrapper.css("justify-content", "space-between");
});
