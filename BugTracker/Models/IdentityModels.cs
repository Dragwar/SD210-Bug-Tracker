using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual string DisplayName { get; set; }
        public virtual List<Project> Projects { get; set; }

        [InverseProperty(nameof(Ticket.Author))]
        public virtual List<Ticket> CreatedTickets { get; set; }

        [InverseProperty(nameof(Ticket.AssignedUser))]
        public virtual List<Ticket> AssignedTickets { get; set; }

        public virtual List<TicketAttachment> TicketAttachments { get; set; }

        public virtual List<TicketComment> TicketComments { get; set; }

        public virtual List<TicketHistory> TicketHistories { get; set; }

        public ApplicationUser()
        {
            CreatedTickets = new List<Ticket>();
            AssignedTickets = new List<Ticket>();
            TicketAttachments = new List<TicketAttachment>();
            TicketComments = new List<TicketComment>();
            TicketHistories = new List<TicketHistory>();
            Projects = new List<Project>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            ClaimsIdentity userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketPriorities> TicketPriorities { get; set; }
        public DbSet<TicketStatuses> TicketStatuses { get; set; }
        public DbSet<TicketTypes> TicketTypes { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }

        public static ApplicationDbContext Create() => new ApplicationDbContext();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TODO: Learn what .WillCascadeOnDelete() does and where/when should I use this
            // TODO: Learn how to make a (Many To Many) Relationship with fluentAPI

            // NOTE: Maybe look into moving these into separate files
            // and maybe look at "EntityTypeConfiguration<TEntityType>()"

            //! NOTE: For explicitly making relationships, you need to do something like this:
            //! "<ENTITY-1>.HasRequired()..." and "<ENITITY-2>.HasMany()..." <-- this represents a one-to-many relationship.
            //! In conclusion since I'm following relationship convention I don't need to explicitly make these relationships here
            //! but this would be necessary when you're not following conventions.

            #region ApplicationUser (Entity)
            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.DisplayName)
                    .IsRequired()
                    .HasMaxLength(100);

            //! Example: TicketComment Relationship (One To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.TicketComments)
                    .WithRequired(ticketComment => ticketComment.User)
                    .HasForeignKey(ticketComment => ticketComment.UserId);

            //! Example: TicketAttachment Relationship (One To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.TicketAttachments)
                    .WithRequired(ticketAttachment => ticketAttachment.User)
                    .HasForeignKey(ticketAttachment => ticketAttachment.UserId);

            //? Maybe Example: Project Relationship (Many To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.Projects)
                    .WithMany(project => project.Users);

            //! Example: Ticket [Author] Relationship (One To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.CreatedTickets)
                    .WithRequired(ticket => ticket.Author)
                    .HasForeignKey(ticket => ticket.AuthorId);

            //! Example: Ticket [Assigned] Relationship (One To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.AssignedTickets)
                    .WithOptional(ticket => ticket.AssignedUser)
                    .HasForeignKey(ticket => ticket.AssignedUserId);

            //! Example: TicketHistory Relationship (One To Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.TicketHistories)
                    .WithRequired(ticketHistory => ticketHistory.User)
                    .HasForeignKey(ticketHistory => ticketHistory.UserId);
            #endregion

            #region Project (Entity)
            modelBuilder.Entity<Project>()
                .HasKey(ticketPriority => ticketPriority.Id)
                .Property(project => project.Id)
                    .IsRequired();

            modelBuilder.Entity<Project>()
                .Property(project => project.Name)
                    .IsRequired()
                    .HasMaxLength(100);

            modelBuilder.Entity<Project>()
                .Property(project => project.IsArchived)
                    .IsRequired();

            modelBuilder.Entity<Project>()
                .Property(project => project.DateCreated)
                    .IsRequired();

            modelBuilder.Entity<Project>()
                .Property(project => project.DateUpdated)
                    .IsOptional();

            //! Example: Tickets Relationship (One To Many)
            modelBuilder.Entity<Project>()
                .HasMany(project => project.Tickets)
                    .WithRequired(ticket => ticket.Project)
                    .HasForeignKey(ticket => ticket.ProjectId);

            //? Maybe Example: ApplicationUser Relationship (Many To Many)
            modelBuilder.Entity<Project>()
                .HasMany(project => project.Users)
                .WithMany(user => user.Projects);
            #endregion

            #region Ticket (Entity)
            modelBuilder.Entity<Ticket>()
                .HasKey(ticket => ticket.Id)
                .Property(ticket => ticket.Id)
                    .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(ticket => ticket.Title)
                    .IsRequired()
                    .HasMaxLength(100);

            modelBuilder.Entity<Ticket>()
                .Property(ticket => ticket.Description)
                    .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(ticket => ticket.DateCreated)
                    .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(ticket => ticket.DateUpdated)
                    .IsOptional();

            //! Example: TicketPriorities Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasRequired(ticket => ticket.Priority)
                    .WithMany(ticketPriority => ticketPriority.Tickets)
                    .HasForeignKey(ticket => ticket.PriorityId);

            //! Example: TicketTypes Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasRequired(ticket => ticket.Type)
                    .WithMany(ticketType => ticketType.Tickets)
                    .HasForeignKey(ticket => ticket.TypeId);

            //! Example: TicketStatuses Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasRequired(ticket => ticket.Status)
                    .WithMany(ticketStatus => ticketStatus.Tickets)
                    .HasForeignKey(ticket => ticket.StatusId);

            //! Example: ApplicationUser [Author] Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasRequired(ticket => ticket.Author)
                    .WithMany(user => user.CreatedTickets)
                    .HasForeignKey(ticket => ticket.AuthorId);

            //! Example: ApplicationUser[Assigned] Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasOptional(ticket => ticket.AssignedUser)
                    .WithMany(user => user.AssignedTickets)
                    .HasForeignKey(ticket => ticket.AssignedUserId);

            //! Example: Project Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasRequired(ticket => ticket.Project)
                    .WithMany(project => project.Tickets)
                    .HasForeignKey(ticket => ticket.ProjectId);

            //! Example: TicketComment Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasMany(ticket => ticket.Comments)
                    .WithRequired(ticketComment => ticketComment.Ticket)
                    .HasForeignKey(ticketComment => ticketComment.TicketId);

            //! Example: TicketAttachment Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasMany(ticket => ticket.Attachments)
                    .WithRequired(ticketAttachment => ticketAttachment.Ticket)
                    .HasForeignKey(ticketAttachment => ticketAttachment.TicketId);

            //! Example: TicketHistory Relationship (One To Many)
            modelBuilder.Entity<Ticket>()
                .HasMany(ticket => ticket.TicketHistories)
                    .WithRequired(ticketHistory => ticketHistory.Ticket)
                    .HasForeignKey(ticketHistory => ticketHistory.TicketId);
            #endregion

            #region TicketPriorities (Entity)
            modelBuilder.Entity<TicketPriorities>()
                .HasKey(ticketPriority => ticketPriority.Id)
                .Property(ticketPriority => ticketPriority.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<TicketPriorities>()
                .Ignore(ticketPriority => ticketPriority.Priority)
                .Property(ticketPriority => ticketPriority.PriorityString)
                    .IsRequired()
                    .HasColumnName("Priority")
                    .HasMaxLength(75);

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketPriorities>()
                .HasMany(ticketPriorities => ticketPriorities.Tickets)
                    .WithRequired(ticket => ticket.Priority)
                    .HasForeignKey(ticket => ticket.PriorityId);
            #endregion

            #region TicketStatuses (Entity)
            modelBuilder.Entity<TicketStatuses>()
                .HasKey(ticketStatus => ticketStatus.Id)
                .Property(ticketStatus => ticketStatus.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<TicketStatuses>()
                .Ignore(ticketStatus => ticketStatus.Status)
                .Property(ticketStatus => ticketStatus.StatusString)
                    .IsRequired()
                    .HasColumnName("Status")
                    .HasMaxLength(75);

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketStatuses>()
                .HasMany(ticketStatuses => ticketStatuses.Tickets)
                    .WithRequired(ticket => ticket.Status)
                    .HasForeignKey(ticket => ticket.StatusId);
            #endregion

            #region TicketTypes (Entity)
            modelBuilder.Entity<TicketTypes>()
                .HasKey(ticketType => ticketType.Id)
                .Property(ticketType => ticketType.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<TicketTypes>()
                .Ignore(ticketType => ticketType.Type)
                .Property(ticketType => ticketType.TypeString)
                    .IsRequired()
                    .HasColumnName("Type")
                    .HasMaxLength(75);

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketTypes>()
                .HasMany(ticketStatuses => ticketStatuses.Tickets)
                    .WithRequired(ticket => ticket.Type)
                    .HasForeignKey(ticket => ticket.TypeId);
            #endregion

            #region TicketComment (Entity)
            modelBuilder.Entity<TicketComment>()
                .HasKey(ticketComment => ticketComment.Id)
                .Property(ticketComment => ticketComment.Id)
                    .IsRequired();

            modelBuilder.Entity<TicketComment>()
                .Property(ticketType => ticketType.Comment)
                    .IsRequired();

            modelBuilder.Entity<TicketComment>()
                .Property(ticketType => ticketType.DateCreated)
                    .IsRequired();

            //! Example: ApplicationUser Relationship (One To Many)
            modelBuilder.Entity<TicketComment>()
                .HasRequired(ticketComment => ticketComment.User)
                    .WithMany(user => user.TicketComments)
                    .HasForeignKey(ticketComment => ticketComment.UserId);

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketComment>()
                .HasRequired(ticketComment => ticketComment.Ticket)
                    .WithMany(ticket => ticket.Comments)
                    .HasForeignKey(ticketComment => ticketComment.TicketId)
                    .WillCascadeOnDelete(false);
            #endregion

            #region TicketAttachment (Entity)
            modelBuilder.Entity<TicketAttachment>()
                .HasKey(ticketAttachment => ticketAttachment.Id)
                .Property(ticketAttachment => ticketAttachment.Id)
                    .IsRequired();

            modelBuilder.Entity<TicketAttachment>()
                .Property(ticketType => ticketType.Id)
                    .IsRequired();

            modelBuilder.Entity<TicketAttachment>()
                .Property(ticketType => ticketType.Description)
                    .IsRequired()
                    .HasMaxLength(100);

            modelBuilder.Entity<TicketAttachment>()
                .Property(ticketType => ticketType.FilePath)
                    .IsRequired();

            modelBuilder.Entity<TicketAttachment>()
                .Property(ticketType => ticketType.FileUrl)
                    .IsRequired();

            modelBuilder.Entity<TicketAttachment>()
                .Property(ticketType => ticketType.DateCreated)
                    .IsRequired();

            //! Example: ApplicationUser Relationship (One To Many)
            modelBuilder.Entity<TicketAttachment>()
                .HasRequired(ticketComment => ticketComment.User)
                    .WithMany(user => user.TicketAttachments)
                    .HasForeignKey(ticketComment => ticketComment.UserId);

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketAttachment>()
                .HasRequired(ticketAttachment => ticketAttachment.Ticket)
                    .WithMany(ticket => ticket.Attachments)
                    .HasForeignKey(ticketAttachment => ticketAttachment.TicketId)
                    .WillCascadeOnDelete(false);
            #endregion

            #region TicketHistory (Entity)
            modelBuilder.Entity<TicketHistory>()
                .HasKey(ticketHistory => ticketHistory.Id)
                .Property(ticketHistory => ticketHistory.Id)
                    .IsRequired();

            modelBuilder.Entity<TicketHistory>()
                .Property(ticketHistory => ticketHistory.Property)
                    .IsRequired()
                    .HasMaxLength(150);

            modelBuilder.Entity<TicketHistory>()
               .Property(ticketHistory => ticketHistory.NewValue)
                   .IsRequired();

            modelBuilder.Entity<TicketHistory>()
               .Property(ticketHistory => ticketHistory.OldValue)
                   .IsRequired();

            modelBuilder.Entity<TicketHistory>()
               .Property(ticketHistory => ticketHistory.DateChanged)
                   .IsRequired();

            //! Example: Ticket Relationship (One To Many)
            modelBuilder.Entity<TicketHistory>()
                .HasRequired(ticketHistory => ticketHistory.Ticket)
                    .WithMany(ticket => ticket.TicketHistories)
                    .HasForeignKey(ticketHistory => ticketHistory.TicketId);

            //! Example: ApplicationUser Relationship (One To Many)
            modelBuilder.Entity<TicketHistory>()
                .HasRequired(ticketHistory => ticketHistory.User)
                    .WithMany(user => user.TicketHistories)
                    .HasForeignKey(ticketHistory => ticketHistory.UserId)
                    .WillCascadeOnDelete(false);
            #endregion
        }
    }
}