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
            .Where(ticket => !ticket.Project.IsArchived && where(ticket))
            .Select(ticket => select(ticket))
            .ToList();

        /// <summary>This would get all tickets first, then this applies <paramref name="where"/> parameter</summary>
        public List<Ticket> GetTickets(Func<Ticket, bool> where) => GetAllTickets()
            .ToList()
            .Where(ticket => !ticket.Project.IsArchived && where(ticket))
            .ToList();

        public Ticket GetTicket(Guid id) => DbContext.Tickets.FirstOrDefault(ticket => !ticket.Project.IsArchived && ticket.Id == id);
        public bool DoesTicketExist(Guid id) => DbContext.Tickets.Any(ticket => !ticket.Project.IsArchived && ticket.Id == id);
        public bool DoesTicketExistOnAnArchivedProject(Guid id) => DbContext.Tickets.Any(ticket => ticket.Project.IsArchived && ticket.Id == id);
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
        public bool CanUserViewTicket(string userId, Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }
            else if (DoesTicketExistOnAnArchivedProject(ticket.Id))
            {
                throw new ArgumentException("You can't interact with a ticket that belongs to an Archived project");
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
            else if (DoesTicketExistOnAnArchivedProject(ticketId))
            {
                throw new ArgumentException("You can't interact with a ticket that belongs to an Archived project");
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

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
        public bool CanUserEditTicket(string userId, Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }
            else if (DoesTicketExistOnAnArchivedProject(ticket.Id))
            {
                throw new ArgumentException("You can't interact with a ticket that belongs to an Archived project");
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

        public IQueryable<Ticket> GetUserCreatedTickets(string userId) => GetAllTickets()
            .Where(ticket => !ticket.Project.IsArchived && ticket.Author.Id == userId)
            .AsQueryable();
        public IQueryable<Ticket> GetUserAssignedTickets(string userId) => GetAllTickets()
            .Where(ticket => !ticket.Project.IsArchived && ticket.AssignedUser != null && ticket.AssignedUser.Id == userId)
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


        public Ticket CreateNewTicket(TicketCreateViewModel model, (bool saveChanges, bool addNotifications) config)
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
            UserRepository userRepository = new UserRepository(DbContext);
            //UserRoleRepository userRoleRepository = new UserRoleRepository(DbContext);
            //TicketNotificationRepository ticketNotificationRepository = new TicketNotificationRepository(DbContext);

            Project foundProject = new ProjectRepository(DbContext).GetProject(model.ProjectId) ?? throw new Exception($"project not found {model.ProjectId}");
            ApplicationUser foundAuthor = userRepository.GetUserById(model.AuthorId) ?? throw new Exception($"user not found {model.AuthorId}");

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

            //!Don't need to worry about adding all Admins/ProjectManagers to a ticket notifications
            //!Also don't need to assign a user to the newly created ticket (only on ticket edit)
            //if (config.addNotifications)
            //{
            //    // Subscribe the new ticket to the admins and project notifications
            //    List<ApplicationUser> adminsAndProjectManagers = userRoleRepository
            //        .UsersInRole(UserRolesEnum.Admin)
            //        .Concat(userRoleRepository.UsersInRole(UserRolesEnum.ProjectManager))
            //        //in case of Author is an Admin and at the Creator of the newTicket
            //        //in case of AssignedUser is an Admin and a the Developer of the newTicket
            //        .Where(user => user.Id != foundAuthor.Id && user.Id != newTicket.AssignedUserId)
            //        .ToList();

            //    adminsAndProjectManagers
            //        .ForEach(user =>
            //        {
            //            TicketNotification userSubscribed = ticketNotificationRepository.CreateNewTicketNotification(user.Id, newTicket, false);
            //            DbContext.TicketNotifications.Add(userSubscribed);
            //        });

            //    // This won't happen because I'm not assigning a user on ticket creation
            //    if (!string.IsNullOrWhiteSpace(newTicket.AssignedUserId) && userRepository.DoesUserExist(newTicket.AssignedUserId))
            //    {
            //        TicketNotification userSubscribed = new TicketNotification()
            //        {
            //            TicketId = newTicket.Id,
            //            UserId = newTicket.AssignedUserId,
            //        };
            //        DbContext.TicketNotifications.Add(userSubscribed);
            //    }
            //}


            if (config.saveChanges)
            {
                DbContext.SaveChanges();
            }

            return newTicket;
        }

        public (Ticket editedTicket, bool wasChanged) EditExistingTicket(
            Ticket ticket,
            TicketEditViewModel model,
            string currentUserId,
            string callBackUrl)
        {
            if (ticket == null || model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                string.IsNullOrWhiteSpace(callBackUrl) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }
            else if (DoesTicketExistOnAnArchivedProject(ticket.Id))
            {
                throw new ArgumentException("You can't interact with tickets that belong to archived projects");
            }

            ApplicationUser userWhoMadeChanges = new UserRepository(DbContext).GetUserById(currentUserId) ?? throw new Exception("Editor wasn't found");
            TicketHistoryRepository ticketHistoryRepository = new TicketHistoryRepository(DbContext);
            TicketNotificationRepository ticketNotificationRepository = new TicketNotificationRepository(DbContext);
            UserRepository userRepository = new UserRepository(DbContext);
            EmailSystemRepository emailRepo = new EmailSystemRepository();

            ticket.Title = model.Title;
            ticket.Description = model.Description;
            ticket.PriorityId = (int)model.Priority;
            ticket.StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open;
            ticket.TypeId = (int)model.Type;
            ticket.ProjectId = model.ProjectId;

            // ON DEV CHANGE
            bool isNewDev = ticket.AssignedUserId != model.DeveloperId;
            if (isNewDev)
            {
                // remove any notification belonging to this ticket
                if (ticket.AssignedUser != null)
                {
                    // NOTE: save changes is called later (Line - ~383) because the ChangeTracker will give unintended data
                    ticketNotificationRepository.RemoveTicketNotificationsFromUser(ticket.AssignedUser, ticket, false);
                }

                // Add new notification for new developer
                ApplicationUser foundNewDeveloper = userRepository.GetUserById(model.DeveloperId);
                if (foundNewDeveloper != null)
                {
                    ticketNotificationRepository.CreateNewTicketNotification(foundNewDeveloper, ticket, false);
                }

                ticket.AssignedUser = foundNewDeveloper;

                if (foundNewDeveloper != null)
                {
                    // Send email to new assigned developer
                    string body = emailRepo.GetSampleBodyString(
                        $"You were assigned to <i>\"{ticket.Title}\"</i>",
                        $"assigned by {userWhoMadeChanges.Email}",
                        $"Click here for the ticket details",
                        $"{callBackUrl}");

                    emailRepo.Send(foundNewDeveloper.Id, ("New assigned ticket", body));
                }
            }


            // If "ticketHistoryRepository.GetTicketState(ticket.Id)" returned "null", then make "wasEdited" = "false"
            bool wasEdited = (ticketHistoryRepository.GetTicketState(ticket.Id) ?? EntityState.Unchanged) == EntityState.Modified;

            if (wasEdited)
            {
                ticket.DateUpdated = DateTime.Now;

                // Make TicketHistories and DbContext.SaveChanges();
                DbContext.TicketHistories.AddRange(ticketHistoryRepository.GetTicketChanges(ticket, currentUserId));
                DbContext.SaveChanges();


                List<TicketNotification> ticketNotifications = ticketNotificationRepository
                    .GetTicketsTicketNotifications(ticket.Id)
                    .ToList();

                if (ticketNotifications.Count > 0)
                {
                    //! should i prevent this email from being sent to the new developer??
                    //! don't email the new assigned developer?
                    //if (isNewDev && ticket?.AssignedUserId != null)
                    //{
                    //    ticketNotifications.RemoveAll(ticketNotification => ticketNotification.UserId == ticket?.AssignedUserId);
                    //}

                    string body = emailRepo.GetSampleBodyString(
                        $"<i>\"{ticket.Title}\"</i> was modified",
                        $"changes made by {userWhoMadeChanges.Email}",
                        $"Click here for the ticket details",
                        $"{callBackUrl}");

                    emailRepo.SendAll(($"\"{ticket.Title}\" (Ticket) was changed", body), ticketNotifications);
                }
            }

            return (ticket, wasEdited);
        }

        public IQueryable<Ticket> GetAllTickets() => DbContext.Tickets.Where(ticket => !ticket.Project.IsArchived).AsQueryable();

        [Obsolete("Shouldn't really need to use this")]
        public IQueryable<Ticket> GetAllTicketsOnArchivedProjects() => DbContext.Tickets.Where(ticket => ticket.Project.IsArchived).AsQueryable();
    }
}