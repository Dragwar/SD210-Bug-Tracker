using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public virtual List<TicketAttachments> TicketAttachments { get; set; }

        public virtual List<TicketComments> TicketComments { get; set; }

        public ApplicationUser()
        {
            CreatedTickets = new List<Ticket>();
            AssignedTickets = new List<Ticket>();
            TicketAttachments = new List<TicketAttachments>();
            TicketComments = new List<TicketComments>();
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
        public DbSet<TicketAttachments> TicketAttachments { get; set; }
        public DbSet<TicketComments> TicketComments { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}