using System;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters.Authorize;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    public class TicketNotificationController : Controller
    {
        private readonly ApplicationDbContext DbContext;
        private readonly UserRepository UserRepository;
        private readonly UserRoleRepository UserRoleRepository;
        private readonly TicketRepository TicketRepository;
        private readonly TicketNotificationRepository TicketNotificationRepository;

        public TicketNotificationController()
        {
            DbContext = new ApplicationDbContext();
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
            TicketRepository = new TicketRepository(DbContext);
            TicketNotificationRepository = new TicketNotificationRepository(DbContext);
        }

        // GET: TicketNotification
        public ActionResult Index(Guid? ticketId, string error)
        {
            if (ticketId.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Details), "Ticket", new { id = ticketId });
            }
            else if (!string.IsNullOrWhiteSpace(error))
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error });
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [BugTrackerAuthorize(UserRolesEnum.Admin, UserRolesEnum.ProjectManager)]
        public ActionResult ToggleWatchingTicket(Guid? ticketId, bool goToAllTickets = false)
        {
            if (!ticketId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }
            else if (!TicketRepository.DoesTicketExist(ticketId.Value))
            {
                return RedirectToAction(nameof(Index), new { error = "Ticket doesn't exist" });
            }
            string userId = User.Identity.GetUserId();
            bool isAdminOrProjectManager = User.IsInRole(UserRolesEnum.Admin.ToString()) || User.IsInRole(UserRolesEnum.ProjectManager.ToString());

            if (!isAdminOrProjectManager)
            {
                return RedirectToAction(nameof(Index), new { error = "You don't have the permissions to opt-in/opt-out of watching a ticket's changes" });
            }

            try
            {
                bool isWatching = TicketNotificationRepository.IsUserSubscribedToTicket(userId, ticketId.Value);
                ApplicationUser foundUser = UserRepository.GetUserById(userId) ?? throw new Exception("User Not Found");
                Ticket foundTicket = TicketRepository.GetTicket(ticketId.Value) ?? throw new Exception("Ticket Not Found");
                if (isWatching)
                {
                    TicketNotificationRepository.RemoveTicketNotificationsFromUser(foundUser, foundTicket, true);
                }
                else
                {
                    TicketNotificationRepository.CreateNewTicketNotification(foundUser, foundTicket, true);
                }

                if (goToAllTickets)
                {
                    return RedirectToAction(nameof(TicketController.Index), "Ticket");
                }

                return RedirectToAction(nameof(Index), new { ticketId = ticketId.Value });
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index), new { error = e.Message });
            }
        }
    }
}