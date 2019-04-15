using BugTracker.Models;
using BugTracker.Models.Domain;
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
        [Authorize]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser currentUser;

            currentUser = UserRepository.GetUserById(userId);
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
            else if (User.IsInRole(nameof(UserRolesEnum.Submitter)))
            {
                //! submitters can ONLY see their created tickets
                model = currentUser.CreatedTickets
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList();
            }
            else if (User.IsInRole(nameof(UserRolesEnum.Developer)))
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
        public ActionResult Details(Guid id)
        {
            ViewBag.OverrideCurrentPage = "ticket-index";
            if (Guid.Empty == id || string.IsNullOrWhiteSpace(id.ToString()))
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            TicketDetailsViewModel model = TicketDetailsViewModel.CreateNewViewModel(TicketRepository.GetTicket(id), DbContext);

            return View(model);
        }

        // GET: Ticket/Create
        [Authorize(Roles = nameof(UserRolesEnum.Submitter))]
        public ActionResult Create()
        {
            ViewBag.OverrideCurrentPage = "ticket-index";
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
        [Authorize(Roles = nameof(UserRolesEnum.Submitter))]
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


                //// TODO: Maybe this is a bad way of doing this, maybe I should query the database?
                //bool validTicketAttributes = false;
                //validTicketAttributes = CONSTANTS.TicketTypes.TryGetValue(formData.Type.Value, out TicketTypes type);
                //validTicketAttributes = CONSTANTS.TicketPriorites.TryGetValue(formData.Priority.Value, out TicketPriorities priority);
                //validTicketAttributes = CONSTANTS.TicketStatuses.TryGetValue(formData.Status.Value, out TicketStatuses status);

                //if (!validTicketAttributes)
                //{
                //    ModelState.AddModelError("", "Invalid data");
                //    formData = GenerateCreateViewModel(formData.ProjectId, formData) ?? throw new Exception("bad data");
                //    return View(formData);
                //}

                TicketPriorities priority = DbContext.TicketPriorities.First(tp => tp.Id == (int)formData.Priority.Value);
                TicketTypes type = DbContext.TicketTypes.First(tt => tt.Id == (int)formData.Type.Value);
                TicketStatuses status = DbContext.TicketStatuses.First(ts => ts.Id == (int)formData.Status.Value);


                Ticket newTicket = new Ticket()
                {
                    Title = formData.Title,
                    Description = formData.Description,
                    PriorityId = status.Id,
                    Priority = priority,
                    StatusId = status.Id,
                    Status = status,
                    TypeId = type.Id,
                    Type = type,
                    Author = foundAuthor,
                    AuthorId = foundAuthor.Id,
                    Project = foundProject,
                    ProjectId = foundProject.Id,
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

        private TicketCreateViewModel GenerateCreateViewModel(Guid projectId, TicketCreateViewModel formData)
        {
            ViewBag.OverrideCurrentPage = "ticket-index";
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

        // GET: Ticket/Edit/{id}
        public ActionResult Edit(int id)
        {
            ViewBag.OverrideCurrentPage = "ticket-index";
            return View();
        }

        // POST: Ticket/Edit/{id}
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
