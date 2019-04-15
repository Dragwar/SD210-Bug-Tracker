using BugTracker.Models;
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

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            TicketRepository = new TicketRepository(DbContext);
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
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
        public ActionResult Create()
        {
            ViewBag.OverrideCurrentPage = "ticket-index";
            return View();
        }

        // POST: Ticket/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
