using System;
using System.Collections.Generic;
using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;

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

        public virtual List<TicketHistory> TicketHistories { get; set; }

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

        public override bool Equals(object obj)
        {
            if (obj is Ticket ticketToCompare)
            {
                return ticketToCompare.Id == Id &&
                    ticketToCompare.Title == Title &&
                    ticketToCompare.Description == Description &&
                    ticketToCompare.AssignedUserId == AssignedUserId &&
                    ticketToCompare.ProjectId == ticketToCompare.ProjectId &&
                    ticketToCompare.StatusId == StatusId &&
                    ticketToCompare.TypeId == TypeId &&
                    ticketToCompare.PriorityId == PriorityId &&
                    ticketToCompare.Attachments.Count == Attachments.Count &&
                    ticketToCompare.Comments.Count == Comments.Count;
            }
            else if (obj is TicketEditViewModel viewModelToCompare)
            {
                return viewModelToCompare.Id == Id &&
                    viewModelToCompare.Title == Title &&
                    viewModelToCompare.Description == Description &&
                    viewModelToCompare.DeveloperId == AssignedUserId &&
                    viewModelToCompare.ProjectId == viewModelToCompare.ProjectId &&
                    ((int?)viewModelToCompare.Status ?? (int)TicketStatusesEnum.Open) == StatusId &&
                    (int)viewModelToCompare.Type == TypeId &&
                    (int)viewModelToCompare.Priority == PriorityId;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString() => $"Ticket: {Title}";
        public override int GetHashCode() => base.GetHashCode();
    }
}