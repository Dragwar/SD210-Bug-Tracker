using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels.Ticket;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class TicketRepository
    {
        private readonly ApplicationDbContext DbContext;

        public TicketRepository(ApplicationDbContext dBContext) => DbContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));

        /// <summary>This would get all tickets first, then this applies parameters</summary>
        public List<TSelectType> GetTickets<TSelectType>(Func<Ticket, bool> where, Func<Ticket, TSelectType> select) => GetAllTickets()
            .ToList()
            .Where(ticket => where(ticket))
            .Select(ticket => select(ticket))
            .ToList();

        /// <summary>This would get all tickets first, then this applies <paramref name="where"/> parameter</summary>
        public List<Ticket> GetTickets(Func<Ticket, bool> where) => GetAllTickets()
            .ToList()
            .Where(ticket => where(ticket))
            .ToList();

        public Ticket GetTicket(Guid id) => DbContext.Tickets.FirstOrDefault(ticket => ticket.Id == id);
        public bool DoesTicketExist(Guid id) => DbContext.Tickets.Any(ticket => ticket.Id == id);
        public bool CanUserViewTicket(string userId, Guid ticketId)
        {
            if (ticketId == null || ticketId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

            bool isUserAssignedToProject = foundTicket.Project.Users.Any(user => user.Id == userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (foundTicket.Author != null && isInRole[UserRolesEnum.Submitter] && foundTicket.Author.Id == userId)
            {
                return true;
            }
            else if (foundTicket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && foundTicket.AssignedUser.Id == userId)
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
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            bool isUserAssignedToProject = ticket.Project.Users.Any(user => user.Id == userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (ticket.Author != null && isInRole[UserRolesEnum.Submitter] && ticket.Author.Id == userId)
            {
                return true;
            }
            else if (ticket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && ticket.AssignedUser.Id == userId)
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

        public bool CanUserEditTicket(string userId, Guid ticketId)
        {
            if (ticketId == null || ticketId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            Models.Domain.Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (foundTicket.Author != null && isInRole[UserRolesEnum.Submitter] && foundTicket.Author.Id == userId)
            {
                return true;
            }
            else if (foundTicket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && foundTicket.AssignedUser.Id == userId)
            {
                return true;
            }

            return false;
        }
        #region CanUserViewTicket (overloads)
        public bool CanUserEditTicket(string userId, Models.Domain.Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (ticket.Author != null && isInRole[UserRolesEnum.Submitter] && ticket.Author.Id == userId)
            {
                return true;
            }
            else if (ticket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && ticket.AssignedUser.Id == userId)
            {
                return true;
            }

            return false;
        }
        #endregion

        public IQueryable<Models.Domain.Ticket> GetUserCreatedTickets(string userId) => GetAllTickets()
            .Where(ticket => ticket.Author.Id == userId)
            .AsQueryable();
        public IQueryable<Models.Domain.Ticket> GetUserAssignedTickets(string userId) => GetAllTickets()
            .Where(ticket => ticket.AssignedUser != null && ticket.AssignedUser.Id == userId)
            .AsQueryable();

        //! Easier way than above (using the List that exists on the ApplicationUser instead of directly querying the all ticket List)
        //public IQueryable<Models.Domain.Ticket> GetUserCreatedTickets(string userId) => DBContext.Users
        //    .FirstOrDefault(user => user.Id == userId)?
        //    .CreatedTickets
        //    .AsQueryable() ?? new List<Models.Domain.Ticket>().AsQueryable();
        //public IQueryable<Models.Domain.Ticket> GetUserAssignedTickets(string userId) => DBContext.Users
        //    .FirstOrDefault(user => user.Id == userId)?
        //    .AssignedTickets
        //    .AsQueryable() ?? new List<Models.Domain.Ticket>().AsQueryable();


        public Ticket CreateNewTicket(TicketCreateViewModel model, bool saveChanges)
        {
            if (DbContext == null ||
                model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }

            Project foundProject = new ProjectRepository(DbContext).GetProject(model.ProjectId) ?? throw new Exception($"project not found {model.ProjectId}");
            ApplicationUser foundAuthor = new UserRepository(DbContext).GetUserById(model.AuthorId) ?? throw new Exception($"user not found {model.AuthorId}");

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

            if (saveChanges)
            {
                DbContext.SaveChanges();
            }

            return newTicket;
        }

        public (Ticket editedTicket, bool wasChanged) EditExistingTicket(Ticket ticket, TicketEditViewModel model, string currentUserId, bool saveDatabase)
        {
            if (ticket == null || model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }

            if (ticket.Title != model.Title)
            {
                ticket.Title = model.Title;
            }

            if (ticket.Description != model.Description)
            {
                ticket.Description = model.Description;
            }

            if (ticket.PriorityId != (int)model.Priority)
            {
                ticket.PriorityId = (int)model.Priority;
            }

            if (ticket.StatusId != ((int?)model?.Status ?? (int)TicketStatusesEnum.Open))
            {
                ticket.StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open;
            }

            if (ticket.TypeId != (int)model.Type)
            {
                ticket.TypeId = (int)model.Type;
            }

            if (ticket.ProjectId != model.ProjectId)
            {
                ticket.ProjectId = model.ProjectId;
            }

            if (ticket.AssignedUserId != model.DeveloperId)
            {
                ticket.AssignedUserId = model.DeveloperId;
            }


            TicketHistoryRepository ticketHistoryRepository = new TicketHistoryRepository(DbContext);

            // if "ticketHistoryRepository.GetTicketState(ticket.Id)" returned "null", then make "wasEdited" = "false"
            bool wasEdited = (ticketHistoryRepository.GetTicketState(ticket.Id) ?? EntityState.Unchanged) == EntityState.Modified;

            if (wasEdited)
            {
                ticket.DateUpdated = DateTime.Now;

                DbContext.TicketHistories.AddRange(ticketHistoryRepository.GetTicketChanges(ticket, currentUserId, null));
            }


            if (saveDatabase && wasEdited)
            {
                DbContext.SaveChanges();
            }

            return (ticket, wasEdited);
        }

        public IQueryable<Ticket> GetAllTickets() => DbContext.Tickets.AsQueryable();
    }
}