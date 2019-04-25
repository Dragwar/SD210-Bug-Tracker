using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BugTracker.Models;
using BugTracker.Models.Domain;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class TicketHistoryRepository
    {
        private readonly ApplicationDbContext DbContext;

        public TicketHistoryRepository(ApplicationDbContext dbContext) => DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public TicketHistory CreateNewTicketHistory(bool saveChanges, string userIdWhoMadeChange, Guid ticketId, string property, string oldValue, string newValue)
        {
            if (ticketId == null || ticketId == Guid.Empty ||
                string.IsNullOrWhiteSpace(userIdWhoMadeChange) ||
                string.IsNullOrWhiteSpace(property) ||
                string.IsNullOrWhiteSpace(oldValue) ||
                string.IsNullOrWhiteSpace(newValue))
            {
                throw new ArgumentNullException();
            }

            bool canUserEditTicket = new TicketRepository(DbContext).CanUserEditTicket(userIdWhoMadeChange, ticketId);

            if (!canUserEditTicket)
            {
                throw new Exception("User doesn't have the proper permissions to edit the ticket");
            }

            TicketHistory newTicketHistory = new TicketHistory()
            {
                Property = property,
                OldValue = oldValue,
                NewValue = newValue,
                TicketId = ticketId,
                UserId = userIdWhoMadeChange,
            };

            if (saveChanges)
            {
                DbContext.TicketHistories.Add(newTicketHistory);
                DbContext.SaveChanges();
            }

            return newTicketHistory;
        }
    }
}