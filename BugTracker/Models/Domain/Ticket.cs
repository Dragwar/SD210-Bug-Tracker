using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.Models.Domain
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }



        public virtual TicketPriorities Priority { get; set; }
        public int PriorityId { get; set; }

        public virtual TicketStatuses Status { get; set; }
        public int StatusId { get; set; }

        public virtual TicketTypes Type { get; set; }
        public int TypeId { get; set; }

        public virtual List<TicketAttachment> Attachments { get; set; }

        public virtual List<TicketComment> Comments { get; set; }

        public virtual Project Project { get; set; }
        public Guid ProjectId { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public string AuthorId { get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }
        public string AssignedUserId { get; set; }

        public Ticket()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
            Attachments = new List<TicketAttachment>();
            Comments = new List<TicketComment>();
        }

        public static Ticket CreateNewTicket(ApplicationDbContext dbContext, TicketCreateViewModel model)
        {
            if (dbContext == null ||
                model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }

            Project foundProject = new ProjectRepository(dbContext).GetProject(model.ProjectId) ?? throw new Exception($"project not found {model.ProjectId}");
            ApplicationUser foundAuthor = new UserRepository(dbContext).GetUserById(model.AuthorId) ?? throw new Exception($"user not found {model.AuthorId}");

            Ticket newTicket = new Ticket()
            {
                Title = model.Title,
                Description = model.Description,
                TypeId = (int)model.Type.Value,
                PriorityId = (int)model.Priority.Value,
                StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open,
            };

            foundProject.Tickets.Add(newTicket);
            foundAuthor.CreatedTickets.Add(newTicket);

            dbContext.SaveChanges();

            return newTicket;
        }

        public static Ticket EditExistingTicket(
           ApplicationDbContext dbContext,
           TicketEditViewModel model)
        {
            if (dbContext == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description) || !model.Priority.HasValue || !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }

            Ticket foundTicket = dbContext.Tickets.First(ticket => ticket.Id == model.Id);

            foundTicket.Title = model.Title;
            foundTicket.Description = model.Description;
            foundTicket.PriorityId = (int)model.Priority;
            foundTicket.StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open;
            foundTicket.TypeId = (int)model.Type;
            foundTicket.ProjectId = model.ProjectId;
            foundTicket.AssignedUserId = model.DeveloperId;
            foundTicket.DateUpdated = DateTime.Now;

            dbContext.SaveChanges();

            return foundTicket;
        }
    }
}