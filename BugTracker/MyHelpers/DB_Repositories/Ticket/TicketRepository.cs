using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BugTracker.MyHelpers.DB_Repositories.Ticket
{
    [NotMapped]
    public class TicketRepository
    {
        private ApplicationDbContext DBContext { get; set; }

        public TicketRepository(ApplicationDbContext dBContext)
        {
            DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));
        }

        public Models.Domain.Ticket GetTicket(Guid id) => DBContext.Tickets.FirstOrDefault(ticket => ticket.Id == id);

        public List<Models.Domain.Ticket> GetAllTickets() => DBContext.Tickets.ToList();
    }
}