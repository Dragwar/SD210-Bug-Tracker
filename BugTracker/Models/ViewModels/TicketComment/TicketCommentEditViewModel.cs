﻿using System;
using System.ComponentModel.DataAnnotations;
using BugTracker.Models.Domain;

namespace BugTracker.Models.ViewModels.TicketComment
{
    public class TicketCommentEditViewModel : ITicketCommentCreateEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Comment { get; set; }

        public Guid TicketId { get; set; }
        public string TicketTitle { get; set; }
        public string UserId { get; set; }

        public static TicketCommentEditViewModel CreateNewViewModel(Domain.TicketComment ticketComment)
        {
            if (ticketComment == null)
            {
                throw new ArgumentNullException();
            }

            return new TicketCommentEditViewModel()
            {
                Id = ticketComment.Id,
                Comment = ticketComment.Comment,
                TicketId = ticketComment.TicketId,
                TicketTitle = ticketComment.Ticket.Title,
                UserId = ticketComment.UserId,
            };
        }
    }
}