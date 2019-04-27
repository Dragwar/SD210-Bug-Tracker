using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters.Actions;
using BugTracker.Models.Filters.Authorize;
using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [BugTrackerAuthorize]
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

        // GET: Ticket
        [Route("Ticket/{whatTickets?}")]
        public ActionResult Index(string whatTickets = "", string error = "")
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

            List<TicketIndexViewModel> model;

            if (!string.IsNullOrWhiteSpace(whatTickets))
            {
                if (whatTickets.ToLower() == "assigned")
                {
                    ViewBag.whatTickets = "Assigned";
                    model = TicketRepository.GetUserAssignedTickets(userId)
                        .ToList()
                        //.Where(ticket => TicketRepository.CanUserViewTicket(userId, ticket.Id)) // shouldn't need to check, if the user is assigned to the ticket
                        .Select(ticket => TicketIndexViewModel.CreateNewViewModel(userId, ticket))
                        .ToList();
                }
                else if (whatTickets.ToLower() == "created")
                {
                    ViewBag.whatTickets = "Created";
                    model = TicketRepository.GetUserCreatedTickets(userId)
                    .ToList()
                    //.Where(ticket => TicketRepository.CanUserViewTicket(userId, ticket.Id)) // shouldn't need to check, if the user created the ticket
                    .Select(ticket => TicketIndexViewModel.CreateNewViewModel(userId, ticket))
                    .ToList();
                }
                else //if (whatTickets.ToLower() == "all") // defaults to this else block { ... }
                {
                    model = TicketRepository.GetAllTickets()
                        .ToList()
                        .Where(ticket => TicketRepository.CanUserViewTicket(userId, ticket.Id))
                        .Select(ticket => TicketIndexViewModel.CreateNewViewModel(userId, ticket))
                        .ToList();
                }
            }
            else
            {
                model = TicketRepository.GetAllTickets()
                    .ToList()
                    .Where(ticket => TicketRepository.CanUserViewTicket(userId, ticket.Id))
                    .Select(ticket => TicketIndexViewModel.CreateNewViewModel(userId, ticket))
                    .ToList();
            }

            return View(model);
        }

        // GET: Ticket/Details/{id}
        [OverrideCurrentNavLinkStyle("ticket-index")]
        public ActionResult Details(Guid? id)
        {
            if (!id.HasValue || Guid.Empty == id.Value || string.IsNullOrWhiteSpace(id.Value.ToString()))
            {
                return RedirectToAction(nameof(Index));
            }

            Ticket foundTicket = TicketRepository.GetTicket(id.Value);

            if (foundTicket == null)
            {
                return RedirectToAction(nameof(Index), new { error = "Ticket wasn't found" });
            }

            string userId = User.Identity.GetUserId();
            bool canViewTicket = TicketRepository.CanUserViewTicket(userId, foundTicket);

            if (!canViewTicket)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "You cannot view this ticket (you don't have permission)" });
            }

            TicketDetailsViewModel model = TicketDetailsViewModel.CreateNewViewModel(userId, foundTicket, DbContext);

            return View(model);
        }

        // GET: Ticket/Create
        [BugTrackerAuthorize(nameof(UserRolesEnum.Submitter))]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        [Route("Ticket/Create")] //! this is needed because of the route on the index (I don't know why)
        public ActionResult Create(Guid? projectId)
        {
            string userId = User.Identity.GetUserId();

            if (!UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Submitter))
            {
                throw new Exception("This shouldn't happen");
            }

            if (projectId.HasValue && ProjectRepository.IsUserAssignedToProject(userId, projectId.Value))
            {
                projectId = null;
            }

            List<SelectListItem> userProjects = null;

            if (User.IsInRole(UserRolesEnum.Admin.ToString()) || User.IsInRole(UserRolesEnum.ProjectManager.ToString()))
            {
                userProjects = ProjectRepository.GetAllProjects()
                    .Select(project => new SelectListItem()
                    {
                        Text = project.Name,
                        Value = project.Id.ToString(),
                        Selected = projectId.HasValue ? project.Id == projectId.Value : false,
                    }).ToList();
            }
            else
            {
                userProjects = ProjectRepository.GetUserProjects(userId)
                    .Select(project => new SelectListItem()
                    {
                        Text = project.Name,
                        Value = project.Id.ToString(),
                        Selected = projectId.HasValue ? project.Id == projectId.Value : false,
                    }).ToList();
            }


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
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "You don't have any project to assign a ticket to.\nPLEASE CONTACT ADMIN to get assigned to a project" });
            }

            return View(model);
        }

        // POST: Ticket/Create
        [HttpPost]
        [BugTrackerAuthorize(nameof(UserRolesEnum.Submitter))]
        [Route("Ticket/Create")] //! this is needed because of the route on the index (I don't know why)
        public ActionResult Create(TicketCreateViewModel formData)
        {
            if (formData == null || !ModelState.IsValid || !formData.Type.HasValue || !formData.Priority.HasValue)
            {
                ModelState.AddModelError("", "Error - Bad data");
                formData = GenerateTicketCreateViewModelFromExisting(formData) ?? throw new Exception("bad data");

                return View(formData);
            }

            // if user is submitter and admin or project manager, then allow him/her to give a different status on ticket creation
            if (User.IsInRole(UserRolesEnum.Admin.ToString()) || User.IsInRole(UserRolesEnum.ProjectManager.ToString()))
            {
                formData.Status = formData.Status ?? TicketStatusesEnum.Open;
            }
            else
            {
                formData.Status = TicketStatusesEnum.Open;
            }

            try
            {
                Ticket newTicket = TicketRepository.CreateNewTicket(formData, true);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error - Bad data");
                ModelState.AddModelError("", e.Message); // TODO: Remove after project completion (on staging phase)
                formData = GenerateTicketCreateViewModelFromExisting(formData) ?? throw new Exception("bad data");

                return View(formData);
            }
        }

        [NonAction]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        private TicketCreateViewModel GenerateTicketCreateViewModelFromExisting(TicketCreateViewModel formData)
        {
            string userId = User.Identity.GetUserId();
            List<SelectListItem> userProjects = null;

            if (User.IsInRole(UserRolesEnum.Admin.ToString()) || User.IsInRole(UserRolesEnum.ProjectManager.ToString()))
            {
                userProjects = ProjectRepository.GetAllProjects()
                       .Select(project => new SelectListItem()
                       {
                           Text = project.Name,
                           Value = project.Id.ToString(),
                       }).ToList();
            }
            else
            {
                userProjects = ProjectRepository.GetUserProjects(userId)
                    .Select(project => new SelectListItem()
                    {
                        Text = project.Name,
                        Value = project.Id.ToString(),
                    }).ToList();
            }

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

            Ticket foundTicket = TicketRepository.GetTicket(id.Value);

            if (foundTicket == null)
            {
                return null;
            }
            #endregion

            string userId = User.Identity.GetUserId();
            List<SelectListItem> userProjects = null;

            if (User.IsInRole(UserRolesEnum.Admin.ToString()) || User.IsInRole(UserRolesEnum.ProjectManager.ToString()))
            {
                userProjects = ProjectRepository.GetAllProjects()
                    .Select(project => new SelectListItem()
                    {
                        Text = project.Name,
                        Value = project.Id.ToString(),
                    }).ToList();
            }
            else
            {
                userProjects = ProjectRepository.GetUserProjects(userId)
                    .Select(project => new SelectListItem()
                    {
                        Text = project.Name,
                        Value = project.Id.ToString(),
                    }).ToList();
            }

            List<SelectListItem> developers = UserRoleRepository
                .UsersInRole(UserRolesEnum.Developer)
                .Select(user => new SelectListItem()
                {
                    Text = user.DisplayName,
                    Value = user.Id,
                    Selected = user.Id == foundTicket.AssignedUserId,
                })
                .ToList();

            return TicketEditViewModel.CreateNewViewModel(foundTicket, developers, userProjects);
        }

        // GET: Ticket/Edit/{id}
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
        public ActionResult Edit(TicketEditViewModel formData)
        {
            if (formData?.Id == null || formData.Id == Guid.Empty || formData?.ProjectId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string userId = User.Identity.GetUserId();
                Ticket foundTicket = TicketRepository.GetTicket(formData.Id);

                // Generate link to ticket details when sending a email
                string callBackUrl = Url.Action(nameof(Details), "Ticket", new { id = foundTicket.Id }, Request.Url.Scheme);

                (_, bool wasChanged) = TicketRepository.EditExistingTicket(foundTicket, formData, userId, callBackUrl, false);

                if (!wasChanged)
                {
                    throw new Exception("Ticket was not edited");
                }


                DbContext.SaveChanges();

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
