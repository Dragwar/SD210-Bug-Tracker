using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels.Project
{
    public class ProjectCreateViewModel
    {
        public List<HelperUserViewModel> Users { get; set; }

        [Required]
        public string Name { get; set; }

        public List<SelectListItem> UsersAddList { get; set; }

        [Display(Name = "Assign Users")]
        public string[] SelectedUsersToAdd { get; set; }
    }
}