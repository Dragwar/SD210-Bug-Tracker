using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.Project
{
    public class DetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<HelperUserViewModel> Users { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public static DetailsViewModel CreateViewModel(BugTracker.Models.Domain.Project project, ApplicationDbContext dbContext)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                return new DetailsViewModel()
                {
                    Id = project.Id == null ? throw new ArgumentNullException() : project.Id,
                    Name = string.IsNullOrWhiteSpace(project.Name) ? throw new ArgumentNullException() : project.Name,
                    Users = project.Users?.Select(user => HelperUserViewModel.CreateViewModel(user, dbContext)).ToList() ?? throw new ArgumentNullException(),
                    DateCreated = project.DateCreated == null ? throw new ArgumentNullException() : project.DateCreated,
                    DateUpdated = project.DateUpdated,
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
        }
    }
}