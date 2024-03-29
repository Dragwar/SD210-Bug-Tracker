﻿using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Models.ViewModels.TicketComment;
using BugTracker.MyHelpers.DB_Repositories;

namespace BugTracker.Models.ViewModels.Ticket
{
    public class TicketDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        public List<TicketAttachment.TicketAttachmentIndexViewModel> Attachments { get; set; }

        public List<TicketCommentIndexViewModel> Comments { get; set; }

        public List<TicketHistory.TicketHistoryIndexViewModel> TicketHistories { get; set; }

        public Domain.Project Project { get; set; }
        public Guid ProjectId { get; set; }

        public HelperUserViewModel Author { get; set; }
        public HelperUserViewModel AssignedUser { get; set; }

        public bool IsWatching { get; set; }

        public static TicketDetailsViewModel CreateNewViewModel(string currentUserId, Domain.Ticket ticket, ApplicationDbContext dbContext)
        {
            if (ticket == null || dbContext == null || !new UserRepository(dbContext).DoesUserExist(currentUserId))
            {
                throw new ArgumentNullException();
            }

            try
            {
                return new TicketDetailsViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Priority = ticket.Priority.PriorityString,
                    Status = ticket.Status.StatusString,
                    Type = ticket.Type.TypeString,

                    Comments = ticket.Comments?
                        .OrderByDescending(comment => comment.DateCreated)
                        .Select(ticketComment => TicketCommentIndexViewModel.CreateNewViewModel(ticketComment))
                        .ToList() ?? new List<TicketCommentIndexViewModel>(),

                    Attachments = ticket.Attachments?
                        .OrderByDescending(attachment => attachment.DateCreated)
                        .Select(ticketAttachment => TicketAttachment.TicketAttachmentIndexViewModel.CreateNewViewModel(currentUserId, ticketAttachment, dbContext))
                        .ToList() ?? new List<TicketAttachment.TicketAttachmentIndexViewModel>(),

                    TicketHistories = ticket.TicketHistories?
                        .OrderByDescending(ticketHistory => ticketHistory.DateChanged)
                        .Select(ticketHistory => TicketHistory.TicketHistoryIndexViewModel.CreateNewViewModel(dbContext, ticketHistory))
                        .ToList() ?? new List<TicketHistory.TicketHistoryIndexViewModel>(),

                    DateCreated = ticket.DateCreated,
                    DateUpdated = ticket.DateUpdated,
                    Author = HelperUserViewModel.CreateNewViewModel(ticket.Author, dbContext),
                    AssignedUser = ticket.AssignedUser == null ? null : HelperUserViewModel.CreateNewViewModel(ticket.AssignedUser, dbContext),
                    Project = ticket.Project,
                    ProjectId = ticket.ProjectId,

                    IsWatching = new TicketNotificationRepository(dbContext).IsUserSubscribedToTicket(currentUserId, ticket.Id),
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Something went wrong\n {e.Message}");
            }
        }
    }
}