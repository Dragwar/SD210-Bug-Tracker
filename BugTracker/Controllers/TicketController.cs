using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters.Actions;
using BugTracker.Models.Filters.Authorize;
using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using BugTracker.MyHelpers.DB_Repositories.Ticket;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext DbContext;
        private readonly TicketRepository TicketRepository;
        private readonly UserRoleRepository UserRoleRepository;
        private readonly UserRepository UserRepository;
        private readonly ProjectRepository ProjectRepository;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            TicketRepository = new TicketRepository(DbContext);
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
            ProjectRepository = new ProjectRepository(DbContext);
        }

        [NonAction]
        private IReadOnlyDictionary<UserRolesEnum, bool> UserRoleBools(string userId)
        {
            Dictionary<UserRolesEnum, bool> isInRole = Enum.GetValues(typeof(UserRolesEnum))
                .Cast<UserRolesEnum>()
                .ToDictionary(key => key, x => false);

            if (UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Admin))
            {
                isInRole[UserRolesEnum.Admin] = true;
            }

            if (UserRoleRepository.IsUserInRole(userId, UserRolesEnum.ProjectManager))
            {
                isInRole[UserRolesEnum.ProjectManager] = true;
            }

            if (UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Submitter))
            {
                isInRole[UserRolesEnum.Submitter] = true;
            }

            if (UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Developer))
            {
                isInRole[UserRolesEnum.Developer] = true;
            }

            if (isInRole.Any(pair => pair.Value == true))
            {
                isInRole[UserRolesEnum.None] = false;
            }
            else
            {
                isInRole[UserRolesEnum.None] = true;
            }

            return isInRole;
        }

        // GET: Ticket
        [BugTrackerAuthorize]
        public ActionResult Index(string error = "")
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser currentUser = UserRepository.GetUserById(userId);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("", error);
            }

            if (currentUser == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { controller = "Home" });
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isRoleDictionary = UserRoleBools(userId);


            List<TicketIndexViewModel> model;
            if (isRoleDictionary[UserRolesEnum.Admin] || isRoleDictionary[UserRolesEnum.ProjectManager])
            {
                //! ONLY admins/project-managers can see all tickets
                model = TicketRepository.GetAllTickets()
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else if (isRoleDictionary[UserRolesEnum.Submitter] && !isRoleDictionary[UserRolesEnum.Developer])
            {
                //! submitters can ONLY see their created tickets
                model = currentUser.CreatedTickets
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else if (isRoleDictionary[UserRolesEnum.Developer] && !isRoleDictionary[UserRolesEnum.Submitter])
            {
                //! developers can ONLY see their assigned tickets
                model = currentUser.AssignedTickets
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else
            {
                //! if user isn't in any of the roles above redirect home
                return RedirectToAction(nameof(HomeController.Index), new { controller = "Home" });
            }

            return View(model);
        }

        // GET: Ticket/Details/{id}
        [BugTrackerAuthorize]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        public ActionResult Details(Guid? id)
        {
            if (!id.HasValue || Guid.Empty == id.Value || string.IsNullOrWhiteSpace(id.Value.ToString()))
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            Ticket foundTicket = TicketRepository.GetTicket(id.Value);
            string userId = User.Identity.GetUserId();

            IReadOnlyDictionary<UserRolesEnum, bool> isRoleDictionary = UserRoleBools(userId);


            // TODO: Re-factor this
            if (isRoleDictionary[UserRolesEnum.Admin] || isRoleDictionary[UserRolesEnum.ProjectManager])
            {
                //! ONLY admins/project-managers can see all tickets
            }
            else if (isRoleDictionary[UserRolesEnum.Submitter] && !isRoleDictionary[UserRolesEnum.Developer])
            {
                //! submitters can ONLY see their created tickets
                if (foundTicket.Author == null || foundTicket.Author.Id != userId)
                {
                    foundTicket = null;
                }
            }
            else if (isRoleDictionary[UserRolesEnum.Developer] && !isRoleDictionary[UserRolesEnum.Submitter])
            {
                //! developers can ONLY see their assigned tickets
                if (foundTicket.AssignedUser == null || foundTicket.AssignedUser.Id != userId)
                {
                    foundTicket = null;
                }
            }
            else
            {
                //! if user isn't in any of the roles above redirect home
                return RedirectToAction(nameof(HomeController.Index), new { controller = "Home" });
            }

            if (foundTicket == null)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "You cannot view this ticket (you don't have permission)" });
            }

            TicketDetailsViewModel model = TicketDetailsViewModel.CreateNewViewModel(foundTicket, DbContext);

            return View(model);
        }

        // GET: Ticket/Create
        [BugTrackerAuthorize(nameof(UserRolesEnum.Submitter))]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();

            if (!UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Submitter))
            {
                throw new Exception("This shouldn't happen");
            }

            List<SelectListItem> userProjects = ProjectRepository
                .GetUserProjects(userId)
                .Select(project => new SelectListItem()
                {
                    Text = project.Name,
                    Value = project.Id.ToString(),
                })
                .ToList();

            if (userProjects == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }


            TicketCreateViewModel model = new TicketCreateViewModel()
            {
                AuthorId = userId,
                Projects = userProjects,
            };

            if (!model.Projects.Any())
            {
                return RedirectToAction(nameof(HomeController.Index), new { error = "You don't have any project to assign a ticket to.\nPLEASE CONTACT ADMIN to get assigned to a project" });
            }

            return View(model);
        }

        // POST: Ticket/Create
        [HttpPost]
        [BugTrackerAuthorize(nameof(UserRolesEnum.Submitter))]
        public ActionResult Create(TicketCreateViewModel formData)
        {
            if (formData == null || !ModelState.IsValid || !formData.Type.HasValue || !formData.Priority.HasValue)
            {
                ModelState.AddModelError("", "Error - Bad data");
                formData = GenerateCreateViewModel(formData) ?? throw new Exception("bad data");

                return View(formData);
            }

            formData.Status = TicketStatusesEnum.Open;

            try
            {
                Ticket newTicket = Ticket.CreateNewTicket(DbContext, formData);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error - Bad data");
                ModelState.AddModelError("", e.Message); // TODO: Remove after project completion (on staging phase)
                formData = GenerateCreateViewModel(formData) ?? throw new Exception("bad data");

                return View(formData);
            }
        }

        [NonAction]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        private TicketCreateViewModel GenerateCreateViewModel(TicketCreateViewModel formData)
        {
            string userId = User.Identity.GetUserId();
            List<SelectListItem> userProjects = ProjectRepository
                .GetUserProjects(userId)
                .Select(project => new SelectListItem()
                {
                    Text = project.Name,
                    Value = project.Id.ToString(),
                })
                .ToList();

            if (!userProjects?.Any() == null)
            {
                return null;
            }

            formData.Projects = userProjects;

            return formData;
        }

        [NonAction]
        private TicketEditViewModel GenerateTicketEditViewModel(Guid? id)
        {
            #region Null Checks
            if (!id.HasValue)
            {
                return null;
            }

            Ticket foundTicket = DbContext.Tickets.FirstOrDefault(ticket => ticket.Id == id.Value);

            if (foundTicket == null)
            {
                return null;
            }
            #endregion
            string userId = User.Identity.GetUserId();
            List<SelectListItem> userProjects = ProjectRepository
                .GetUserProjects(userId)
                .Select(project => new SelectListItem()
                {
                    Text = project.Name,
                    Value = project.Id.ToString(),
                })
                .ToList();

            List<SelectListItem> developers = UserRoleRepository
                .UsersInRole(UserRolesEnum.Developer)
                .Select(user => new SelectListItem()
                {
                    Text = user.DisplayName,
                    Value = user.Id,
                    Selected = user.Id == foundTicket.AssignedUserId,
                })
                .ToList();

            TicketEditViewModel model = TicketEditViewModel.CreateNewViewModel(foundTicket, developers, userProjects);

            return model;
        }

        // GET: Ticket/Edit/{id}
        [BugTrackerAuthorize(nameof(UserRolesEnum.Admin), nameof(UserRolesEnum.ProjectManager), nameof(UserRolesEnum.Submitter), nameof(UserRolesEnum.Developer))]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        public ActionResult Edit(Guid? id)
        {
            TicketEditViewModel model = GenerateTicketEditViewModel(id);
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Ticket/Edit/{id}
        [HttpPost]
        [BugTrackerAuthorize(nameof(UserRolesEnum.Admin), nameof(UserRolesEnum.ProjectManager), nameof(UserRolesEnum.Submitter), nameof(UserRolesEnum.Developer))]
        public ActionResult Edit(TicketEditViewModel formData)
        {
            if (formData?.Id == null || formData.Id == Guid.Empty || formData?.ProjectId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                Ticket.EditExistingTicket(DbContext, formData);

                return RedirectToAction(nameof(Details), new { Id = formData.Id });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message); // TODO: Remove after project completion (on staging phase)
                TicketEditViewModel model = GenerateTicketEditViewModel(formData.Id);

                if (model == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(nameof(Edit), model);
            }
        }
    }
}
