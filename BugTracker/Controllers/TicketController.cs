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

        // GET: Ticket
        [BugTrackerAuthorize]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser currentUser = UserRepository.GetUserById(userId);

            if (currentUser == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { controller = "Home" });
            }

            List<TicketIndexViewModel> model;
            if (User.IsInRole(nameof(UserRolesEnum.Admin)) || User.IsInRole(nameof(UserRolesEnum.ProjectManager)))
            {
                //! ONLY admins/project-managers can see all tickets
                model = TicketRepository.GetAllTickets()
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else if (User.IsInRole(nameof(UserRolesEnum.Submitter)) && !User.IsInRole(nameof(UserRolesEnum.Developer)))
            {
                //! submitters can ONLY see their created tickets
                model = currentUser.CreatedTickets
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else if (User.IsInRole(nameof(UserRolesEnum.Developer)) && !User.IsInRole(nameof(UserRolesEnum.Submitter)))
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

            // TODO: Re-factor this
            if (User.IsInRole(nameof(UserRolesEnum.Admin)) || User.IsInRole(nameof(UserRolesEnum.ProjectManager)))
            {
                //! ONLY admins/project-managers can see all tickets
            }
            else if (User.IsInRole(nameof(UserRolesEnum.Submitter)) && !User.IsInRole(nameof(UserRolesEnum.Developer)))
            {
                //! submitters can ONLY see their created tickets
                if (foundTicket.Author == null || foundTicket.Author.Id != userId)
                {
                    foundTicket = null;
                }
            }
            else if (User.IsInRole(nameof(UserRolesEnum.Developer)) && !User.IsInRole(nameof(UserRolesEnum.Submitter)))
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

            if (UserRoleRepository.IsUserInRole(userId, UserRolesEnum.Submitter))
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

            if (!userProjects?.Any() == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            TicketCreateViewModel model = new TicketCreateViewModel()
            {
                AuthorId = userId,
                Projects = userProjects,
            };

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
                formData = GenerateCreateViewModel(formData.ProjectId, formData) ?? throw new Exception("bad data");

                return View(formData);
            }

            // TODO: Move this check to the edit method for tickets and decide whether to remove this
            //! because only admins/project managers can give tickets statuses
            //! BUT only submitters can create tickets, therefore this check is useless
            if (!formData.Status.HasValue)
            {
                if (User.IsInRole(nameof(UserRolesEnum.Submitter)) && !User.IsInRole(nameof(UserRolesEnum.Admin)) && !User.IsInRole(nameof(UserRolesEnum.ProjectManager)))
                {
                    formData.Status = TicketStatusesEnum.Open;
                }
                else
                {
                    ModelState.AddModelError(nameof(TicketCreateViewModel.Status), "You need to provide a ticket status");
                    formData = GenerateCreateViewModel(formData.ProjectId, formData) ?? throw new Exception("bad data");

                    return View(formData);
                }
            }

            try
            {
                Project foundProject = ProjectRepository.GetProject(formData.ProjectId) ?? throw new Exception();
                ApplicationUser foundAuthor = UserRepository.GetUserById(formData.AuthorId) ?? throw new Exception();

                TicketPriorities priority = DbContext.TicketPriorities.First(tp => tp.Id == (int)formData.Priority.Value);
                TicketTypes type = DbContext.TicketTypes.First(tt => tt.Id == (int)formData.Type.Value);
                TicketStatuses status = DbContext.TicketStatuses.First(ts => ts.Id == (int)formData.Status.Value);


                Ticket newTicket = new Ticket()
                {
                    Title = formData.Title,
                    Description = formData.Description,
                    PriorityId = priority.Id,
                    //Priority = priority,
                    StatusId = status.Id,
                    //Status = status,
                    TypeId = type.Id,
                    //Type = type,
                    AuthorId = foundAuthor.Id,
                    //Author = foundAuthor,
                    ProjectId = foundProject.Id,
                    //Project = foundProject,
                };

                foundAuthor.CreatedTickets.Add(newTicket);
                foundProject.Tickets.Add(newTicket);

                DbContext.SaveChanges();


                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error - Bad data");
                ModelState.AddModelError("", e.Message); // TODO: Remove after project completion (on staging phase)
                formData = GenerateCreateViewModel(formData.ProjectId, formData) ?? throw new Exception("bad data");

                return View(formData);
            }
        }

        [NonAction]
        [OverrideCurrentNavLinkStyle("ticket-index")]
        private TicketCreateViewModel GenerateCreateViewModel(Guid projectId, TicketCreateViewModel formData)
        {
            //ViewBag.OverrideCurrentPage = "ticket-index";
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

            TicketCreateViewModel model = new TicketCreateViewModel()
            {
                Title = formData?.Title,
                Description = formData?.Description,
                Priority = formData?.Priority,
                Status = formData?.Status,
                Type = formData?.Type,
                Projects = userProjects,
                ProjectId = formData.ProjectId == Guid.Empty ? projectId : formData.ProjectId, // can't be left empty or null
                AuthorId = formData?.AuthorId,
            };

            return model;
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

            TicketEditViewModel model = new TicketEditViewModel()
            {
                Id = foundTicket.Id,
                Title = foundTicket.Title,
                Description = foundTicket.Description,
                Priority = (TicketPrioritiesEnum)foundTicket.PriorityId,
                Status = (TicketStatusesEnum)foundTicket.StatusId,
                Type = (TicketTypesEnum)foundTicket.TypeId,
                ProjectId = foundTicket.ProjectId,
                Projects = userProjects,
                DeveloperUsers = developers,
                DeveloperId = foundTicket.AssignedUserId,
            };

            return model;
        }

        // GET: Ticket/Edit/{id}
        [BugTrackerAuthorize(nameof(UserRolesEnum.Admin), nameof(UserRolesEnum.ProjectManager), nameof(UserRolesEnum.Submitter), nameof(UserRolesEnum.Developer))]
        public ActionResult Edit(Guid? id)
        {
            ViewBag.OverrideCurrentPage = "ticket-index";

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
                Ticket foundTicket = TicketRepository.GetTicket(formData.Id);

                foundTicket.Title = formData.Title;
                foundTicket.Description = formData.Description;
                foundTicket.PriorityId = (int)formData.Priority;
                foundTicket.StatusId = ((int?)formData?.Status ?? (DbContext.TicketStatuses.First(ts => ts.StatusString == TicketStatusesEnum.Open.ToString()).Id));
                foundTicket.TypeId = (int)formData.Type;
                foundTicket.ProjectId = formData.ProjectId;
                foundTicket.AssignedUserId = formData.DeveloperId;
                foundTicket.DateUpdated = DateTime.Now;

                DbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
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

        //// GET: Ticket/Delete/{id}
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Ticket/Delete/{id}
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
