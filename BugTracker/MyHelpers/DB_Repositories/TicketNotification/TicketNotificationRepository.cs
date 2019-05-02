using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Models;
using BugTracker.Models.Domain;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class TicketNotificationRepository
    {
        private readonly ApplicationDbContext DbContext;
        private readonly TicketRepository TicketRepository;
        private readonly UserRepository UserRepository;

        public TicketNotificationRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            TicketRepository = new TicketRepository(dbContext);
            UserRepository = new UserRepository(dbContext);
        }


        public TicketNotification CreateNewTicketNotification(string userId, Ticket ticket, bool saveChanges)
        {
            if (ticket == null)
            {
                throw new Exception("Ticket can't be null");
            }
            else if (TicketRepository.DoesTicketExistOnAnArchivedProject(ticket.Id))
            {
                throw new Exception("Can't add a notification to a ticket that belongs to an archived project");
            }

            ApplicationUser foundUser = UserRepository.GetUserById(userId) ?? throw new Exception("User not found");

            TicketNotification ticketNotification = new TicketNotification()
            {
                TicketId = ticket.Id,
                TicketTitle = ticket.Title,
                UserId = foundUser.Id,
                UserEmail = foundUser.Email,
            };

            DbContext.TicketNotifications.Add(ticketNotification);

            if (saveChanges)
            {
                DbContext.SaveChanges();
            }

            return ticketNotification;
        }
        public TicketNotification CreateNewTicketNotification(ApplicationUser user, Ticket ticket, bool saveChanges)
        {
            if (ticket == null)
            {
                throw new Exception("Ticket can't be null");
            }
            else if (TicketRepository.DoesTicketExistOnAnArchivedProject(ticket.Id))
            {
                throw new Exception("Can't add a notification to a ticket that belongs to an archived project");
            }
            else if (user == null)
            {
                throw new Exception("User can't be null");
            }

            TicketNotification ticketNotification = new TicketNotification()
            {
                TicketId = ticket.Id,
                TicketTitle = ticket.Title,
                UserId = user.Id,
                UserEmail = user.Email,
            };

            DbContext.TicketNotifications.Add(ticketNotification);

            if (saveChanges)
            {
                DbContext.SaveChanges();
            }

            return ticketNotification;
        }

        //public (bool hasRemoved, int removeCount) RemoveAllTicketNotificationsFromUser(string userId, Func<TicketNotification, bool> where)
        //{
        //    int removeCount = UserRepository.GetUserById(userId).TicketNotifications
        //        .RemoveAll(ticketNotification => where(ticketNotification));
        //    return (removeCount > 0, removeCount);
        //}

        public void RemoveTicketNotificationsFromUser(ApplicationUser user, Ticket ticket, bool saveChanges)
        {
            List<TicketNotification> ticketNotificationsToRemove = DbContext.TicketNotifications
                .Where(t => user.Id == t.UserId && t.TicketId == ticket.Id)
                .ToList();
            // should usually be around one ticket notification that will be removed
            DbContext.TicketNotifications.RemoveRange(ticketNotificationsToRemove);

            if (saveChanges)
            {
                DbContext.SaveChanges();
            }
        }

        public TicketNotification GetTicketNotification(int ticketNotificationId) => DbContext.TicketNotifications
            .FirstOrDefault(ticketNotification => ticketNotification.Id == ticketNotificationId);

        public bool IsUserSubscribedToTicket(string userId, Guid ticketId) => DbContext.TicketNotifications
            .Any(ticketNotification => ticketNotification.UserId == userId && ticketNotification.TicketId == ticketId);

        public IQueryable<TicketNotification> GetUsersTicketNotifications(string userId) => DbContext.TicketNotifications
            .Where(ticketNotification => ticketNotification.UserId == userId)
            .AsQueryable();

        public IQueryable<TicketNotification> GetTicketsTicketNotifications(Guid ticketId) => DbContext.TicketNotifications
            .Where(ticketNotification => ticketNotification.TicketId == ticketId)
            .AsQueryable();

        public IQueryable<TicketNotification> GetAllTicketNotifications() => DbContext.TicketNotifications.AsQueryable();
    }
}