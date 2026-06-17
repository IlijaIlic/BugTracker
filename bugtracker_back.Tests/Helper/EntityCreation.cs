using bugtracker_back.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace bugtracker_back.Tests.Helper
{
    public static class EntityCreation
    {
        public static Manager CreateManager(string id)
        {
            return new Manager
            {
                Id = id,
                UserName = $"Manager - {id}",
                Email = $"manager{id}@gmail.com"
            };
        }

        public static Manager CreateManagerWData(string id, string userName, string email)
        {
            return new Manager
            {
                Id = id,
                UserName = userName,
                Email = email
            };
        }

        public static Project CreateProject(int id, Manager owner)
        {
            return new Project
            {
                Id = id,
                Name = $"Project - {id}",
                Description = $"Project Description - {id}",
                OwnerId = owner.Id,
                Owner = owner,
                Status = ProjectStatus.Active
            };
        }

        public static Project CreateProjectWBug(int id, Manager owner)
        {
            var project = new Project
            {
                Id = id,
                Name = $"Project - {id}",
                Description = $"Project Description - {id}",
                OwnerId = owner.Id,
                Owner = owner,
                Status = ProjectStatus.Active,
                Bugs = new List<Bug>()
            };

            var bug = new Bug
            {
                Id = id,
                Name = "Bug1",
                ProjectId = id,
                Project = project,
                Status = BugStatus.Active,
                Priority = Priority.Low,
                Severity = Severity.Low,
                Platform = Platform.Android,
                Owner = owner,
                OwnerId = owner.Id
            };

            project.Bugs.Add(bug);

            return project;
        }

        public static Project CreateProjectWData(int id, Manager owner, string name, string description, ProjectStatus status)
        {
            return new Project
            {
                Id = id,
                Name = name,
                Description = description,
                OwnerId = owner.Id,
                Owner = owner,
                Status = status
            };
        }

        public static Bug CreateBug(int id, Project project, AppUser owner)
        {
            return new Bug
            {
                Id = id,
                Name = "Bug1",
                ProjectId = project.Id,
                Project = project,
                Status = BugStatus.Active,
                Priority = Priority.Low,
                Severity = Severity.Low,
                Platform = Platform.Android,
                Owner = owner,
                OwnerId = owner.Id
            };
        }

        public static Bug CreateBugWData(int id, Project project, AppUser owner, string name)
        {
            return new Bug
            {
                Id = id,
                Name = name,
                ProjectId = project.Id,
                Project = project,
                Status = BugStatus.Active,
                Priority = Priority.Low,
                Severity = Severity.Low,
                Platform = Platform.Android,
                Owner = owner,
                OwnerId = owner.Id
            };
        }
    }
}
