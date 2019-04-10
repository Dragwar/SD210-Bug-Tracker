using BugTracker.MyHelpers.DB_Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels.Project
{
    public class ProjectEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        public List<SelectListItem> UsersAddList { get; set; }
        public List<SelectListItem> UsersRemoveList { get; set; }

        [Display(Name = "Assign Users")]
        public string[] SelectedUsersToAdd { get; set; }

        [Display(Name = "Unassign Users")]
        public string[] SelectedUsersToRemove { get; set; }

        public static ProjectEditViewModel CreateNewViewModel(Domain.Project project, ApplicationDbContext dbContext, UserRepository userRepository)
        {
            if (project == null || dbContext == null || userRepository == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                List<SelectListItem> usersRemove = new List<SelectListItem>();
                List<SelectListItem> usersAdd = new List<SelectListItem>();

                SelectListGroup userAddGroup = new SelectListGroup()
                {
                    Name = "Users To Add",
                    Disabled = false,
                };
                SelectListGroup userRemoveGroup = new SelectListGroup()
                {
                    Name = "Users To Remove",
                    Disabled = false,
                };

                foreach (ApplicationUser user in userRepository.GetAllUsers().ToList())
                {
                    if (project.Users.Contains(user))
                    {
                        usersRemove.Add(new SelectListItem()
                        {
                            Text = user.DisplayName,
                            Value = user.Id,
                            Group = userRemoveGroup,
                            Selected = false,
                            Disabled = false
                        });
                    }
                    else
                    {
                        usersAdd.Add(new SelectListItem()
                        {
                            Text = user.DisplayName,
                            Value = user.Id,
                            Group = userAddGroup,
                            Selected = false,
                            Disabled = false
                        });
                    }
                }

                ProjectEditViewModel model = new ProjectEditViewModel()
                {
                    Id = project.Id,
                    Name = project.Name,
                    UsersAddList = usersAdd,
                    UsersRemoveList = usersRemove,
                    SelectedUsersToAdd = null,
                    SelectedUsersToRemove = null,
                };

                return model;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}