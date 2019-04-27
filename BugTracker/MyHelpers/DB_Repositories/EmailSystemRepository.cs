using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class EmailSystemRepository
    {
        private readonly UserManager<ApplicationUser> UserManager;

        public EmailSystemRepository() => UserManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContext.Current.GetOwinContext());

        public string GetSampleBodyString(string title, string subtitle, string linkText, string callBackUrl) => (
        $@"
            <div style=""border: 1px solid black; border-radius: 5px; padding: 0px 30px 15px 25px; display: flex; flex-flow: row wrap; justify-content: flex-end;"">
                <div style=""margin-bottom: 15px; width: 100%;"">
                    <h3 style=""margin-bottom: 5px;"">{title}</h1>
                    <small>{subtitle}</small>
                    <p style=""margin-top: 50px;""><a href=""{callBackUrl}"">{linkText}</a></p>
                </div>
                <small style=""color: dimgray;"">Everett Grassler's Bugtracker</small>
            </div>
        ");

        public void Send(string userId, IdentityMessage message)
        {
            if (string.IsNullOrWhiteSpace(userId) || message == null)
            {
                throw new ArgumentNullException();
            }
            else if (string.IsNullOrWhiteSpace(message.Body) || string.IsNullOrWhiteSpace(message.Subject))
            {
                throw new ArgumentException("Invalid message (empty content)");
            }

            if (string.IsNullOrWhiteSpace(message.Destination))
            {
                message.Destination = UserManager.FindById(userId)?.Email ?? throw new ArgumentException("User not found");
            }

            UserManager.EmailService.Send(message);
        }
        public void Send(string userId, (string Subject, string Body)message)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }
            else if (string.IsNullOrWhiteSpace(message.Body) || string.IsNullOrWhiteSpace(message.Subject))
            {
                throw new ArgumentException("Invalid message (empty content)");
            }

            UserManager.SendEmail(userId, message.Subject, message.Body);
        }
    }
}