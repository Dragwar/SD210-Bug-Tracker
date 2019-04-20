using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;
using BugTracker.Models.Domain;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class TicketAttachmentRepository
    {
        private ApplicationDbContext DBContext { get; }

        public TicketAttachmentRepository(ApplicationDbContext dbContext) => DBContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public IQueryable<TicketAttachment> GetAllTicketAttachments() => DBContext.TicketAttachments.AsQueryable();
    }
}