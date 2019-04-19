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
        public bool DoesTicketExist(Guid id) => DBContext.Tickets.Any(ticket => ticket.Id == id);
        public bool CanUserViewTicket(string userId, Guid ticketId)
        {
            IReadOnlyDictionary<UserRolesEnum, bool> IsInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            Models.Domain.Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

            bool isUserAssignedToProject = foundTicket.Project.Users.Any(user => user.Id == userId);

            if (IsInRole[UserRolesEnum.Admin] || IsInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (foundTicket.Author != null && IsInRole[UserRolesEnum.Submitter] && foundTicket.Author.Id == userId)
            {
                return true;
            }
            else if (foundTicket.AssignedUser != null && IsInRole[UserRolesEnum.Developer] && foundTicket.AssignedUser.Id == userId)
            {
                return true;
            }
            else if (isUserAssignedToProject)
            {
                return true;
            }

            return false;
        }
        #region CanUserViewTicket (overloads)
        public bool CanUserViewTicket(string userId, Models.Domain.Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            IReadOnlyDictionary<UserRolesEnum, bool> IsInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            bool isUserAssignedToProject = ticket.Project.Users.Any(user => user.Id == userId);

            if (IsInRole[UserRolesEnum.Admin] || IsInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (ticket.Author != null && IsInRole[UserRolesEnum.Submitter] && ticket.Author.Id == userId)
            {
                return true;
            }
            else if (ticket.AssignedUser != null && IsInRole[UserRolesEnum.Developer] && ticket.AssignedUser.Id == userId)
            {
                return true;
            }
            else if (isUserAssignedToProject)
            {
                return true;
            }

            return false;
        }
        #endregion

        public IQueryable<Models.Domain.Ticket> GetAllTickets() => DBContext.Tickets.AsQueryable();
    }
}