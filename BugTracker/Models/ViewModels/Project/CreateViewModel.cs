using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models.ViewModels.Project
{
    public class CreateViewModel
    {
        public List<HelperUserViewModel> Users { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Add yourself to the new project")]
        public bool AddProjectCreatorToNewProject { get; set; }
    }
}