using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class TicketCommentRepository
    {
        private ApplicationDbContext DBContext { get; }

        public TicketCommentRepository(ApplicationDbContext dbContext) => DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public Models.Domain.TicketComment GetTicketComment(Guid id) => DBContext.TicketComments.FirstOrDefault(ticketComment => ticketComment.Id == id);
        public bool DoesTicketCommentExist(Guid id) => DBContext.TicketComments.Any(ticketComment => ticketComment.Id == id);

        public bool DeleteTicketComment(Guid id)
        {
            Models.Domain.TicketComment foundTicketComment = DBContext.TicketComments.FirstOrDefault(ticketComment => ticketComment.Id == id);
            if (foundTicketComment != null)
            {
                DBContext.TicketComments.Remove(foundTicketComment);
                int numberOfChangedEntities = DBContext.SaveChanges();
                return numberOfChangedEntities == 1;
            }
            else
            {
                return false;
            }
        }

        public IQueryable<Models.Domain.TicketComment> GetAllTicketCommentsFromTicketId(Guid ticketId) => DBContext.Tickets.FirstOrDefault(ticket => ticket.Id == ticketId)?.Comments.AsQueryable();
        public IQueryable<Models.Domain.TicketComment> GetAllTicketComments() => DBContext.TicketComments.AsQueryable();
    }
}