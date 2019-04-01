﻿using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext { get; set; } = new ApplicationDbContext();

        public ActionResult Index()
        {
            // Testing DBRepositories
            string userId = User.Identity.GetUserId();
            UserRepository userRepository = new UserRepository(DbContext);
            ProjectRepository projectRepository = new ProjectRepository(DbContext);

            List<ApplicationUser> allUsers = userRepository.GetAllUsers();
            List<Project> allProjects = projectRepository.GetAllProjects();

            ApplicationUser currentUser = userRepository.GetUserById(userId);
            List<Project> currentUsersProjects = projectRepository.GetUserProjects(userId);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}