using bugtracker_back.Controllers;
using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace bugtracker_back.Tests
{
    [TestFixture]
    public class TestProjectController
    {
        private AppDbContext _db;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private ProjectController _controller;

        [SetUp]
        public void Setup()
        {
            _db = DbContextFactory.Create();

            //razgovara za bazom
            var store = new Mock<IUserStore<AppUser>>();
            //API za korisnike
            _userManagerMock = new Mock<UserManager<AppUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new ProjectController(_db, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test, Description("GetAll bi trebalo da vrati praznu listu ako nema projekata")]
        public async Task GetAll_Empty_wNoProjects()
        {
            var result = await _controller.GetAll(null);

            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;
            Assert.That(projects, Is.Empty);

        }

        [Test, Description("GetAll bi trebalo da vrati listu sa projektima ako postoje")]
        public async Task GettAll_NotEmpty_wProjects()
        {
            var manager = new Manager
            {
                Id = "1",
                UserName = "ManagerTest",
                Email = "managerTest@gmail.com"
            };

            _db.Users.Add(manager);

            var project1 = new Project
            {
                Id = 1,
                Name = "Test Project 1",
                Description = "Test Description 1",
                OwnerId = manager.Id,
                Owner = manager,
                Status = ProjectStatus.Active
            };

            var project2 = new Project
            {
                Id = 2,
                Name = "Test Project 2",
                Description = "Test Description 2",
                OwnerId = manager.Id,
                Owner = manager,
                Status = ProjectStatus.Active
            };

            _db.Projects.AddRange(project1, project2);
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(null); 

            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;

            Assert.That(projects, Has.Count.EqualTo(2));
        }
    }
}
