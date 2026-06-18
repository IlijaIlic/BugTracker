using bugtracker_back.Controllers;
using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Tests.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Runtime.CompilerServices;
using System.Security.Claims;


namespace bugtracker_back.Tests
{
    [TestFixture]
    public class TestProjectController
    {
        private AppDbContext _db;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private ProjectController _controller;

        #region SetUp + TearDown

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

        #endregion

        #region GetAllTests

        [Test, Description("GetAll bi trebalo da vrati praznu listu ako nema projekata.")]
        [Category("GetAll")]

        public async Task GetAll_Empty_wNoProjects()
        {
            var result = await _controller.GetAll(null);

            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;
            Assert.That(projects, Is.Empty);

        }

        [Test, Description("GetAll bi trebalo da vrati listu sa 2 projekata")]
        [Category("GetAll")]
        public async Task GetAll_NotEmpty_wProjects()
        {
            var manager = EntityCreation.CreateManager("1");

            _db.Users.Add(manager);

            var project1 = EntityCreation.CreateProject(1, manager);
            var project2 = EntityCreation.CreateProject(2, manager);

            _db.Projects.AddRange(project1, project2);
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(null);
            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;

            Assert.That(projects, Has.Count.EqualTo(2));
        }

        [Test, Description("GetAll bi trebalo da vrati listu projekata sa korektno mapiranim owner-ima")]
        [Category("GetAll")]
        public async Task GetAll_Correctly_Mapped_Owners()
        {
            var manager1 = EntityCreation.CreateManager("1");
            var manager2 = EntityCreation.CreateManager("2");

            _db.Users.AddRange(manager1, manager2);

            var project1 = EntityCreation.CreateProject(1, manager1);
            var project2 = EntityCreation.CreateProject(2, manager2);

            _db.Projects.AddRange(project1, project2);
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;

            Assert.That(projects!.Select(p => p.OwnerId), Is.EquivalentTo(new[] { manager1.Id, manager2.Id }));
        }

        //TestCase za "ab" vraca error samo tokom testiranja, u aplikaciji nema ove greske
        [Ignore("CaseSensitive - InMemoryDb")]
        [TestCase("AB", "ABCD")]
        [TestCase("ab", "ABCD")]
        [TestCase("E", "EFGH")]
        [Description("GetAll bi trebalo da vrati samo projekat ciji se naziv poklapa sa searchInput parametrom funkcije.")]
        [Category("GetAll")]
        public async Task GetAll_Returns1_wSearch(string searchInput, string expected)
        {
            var manager = EntityCreation.CreateManager("1");

            _db.Users.Add(manager);

            var project1 = EntityCreation.CreateProjectWData(1, manager, "ABCD", "desc", ProjectStatus.Active);
            var project2 = EntityCreation.CreateProjectWData(2, manager, "EFGH", "desc", ProjectStatus.Active);

            _db.Projects.AddRange(project1, project2);
            await _db.SaveChangesAsync();

            var result = await _controller.GetAll(searchInput);
            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponseDto>;

            Assert.That(projects, Has.Count.EqualTo(1));
            Assert.That(projects![0].Name, Is.EqualTo(expected));
        }

        #endregion

        #region GetByIdTests

        [Test, Description("GetById bi trebalo da baci gresku kada nema projekata u bazi")]
        [Category("GetById")]
        public async Task GetById_ProjectNotFound_wNoProjects()
        {
            var result = await _controller.GetById(0);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [Description("GetById bi trebalo da vrati projekte sa id-em koji prosledimo")]
        [Category("GetById")]
        public async Task GetById_ReturnsCorrectProject_w2ProjectsInDb(int id)
        {
            var manager = EntityCreation.CreateManager("1");

            _db.Users.Add(manager);

            var project1 = EntityCreation.CreateProject(1, manager);
            var project2 = EntityCreation.CreateProject(2, manager);
            var project3 = EntityCreation.CreateProject(3, manager);

            _db.Projects.AddRange(project1, project2, project3);
            await _db.SaveChangesAsync();

            var result = await _controller.GetById(id);
            var okResult = result as OkObjectResult;
            var project = okResult!.Value as ProjectResponseDto;

            Assert.That(project!.Id, Is.EqualTo(id));
        }

        [Test, Description("GetById bi trebalo da vrati korektan broj bugova koji su dodati nekom projeku")]
        [Category("GetById")]
        public async Task GetById_ReturnsProjectWithCorrectBugCount()
        {
            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug1 = EntityCreation.CreateBug(1, project, manager);
            var bug2 = EntityCreation.CreateBug(2, project, manager);
            var bug3 = EntityCreation.CreateBug(3, project, manager);
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetById(1);
            var okResult = result as OkObjectResult;
            var projectResp = okResult!.Value as ProjectResponseDto;

            Assert.That(projectResp!.BugCount, Is.EqualTo(3));
        }

        #endregion

        #region GetMyProjectsTests


        [Test, Description("GetMyProjects bi trebalo da vrati UnauthorizedResult kada nema ulogovanog korisnika")]
        [Category("GetMyProjects")]
        public async Task GetMyProjects_UnauthorizedResult_wNoUserIsLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var response = await _controller.GetMyProjects();

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test, Description("GetMyProjects bi trebalo da vrati praznu listu ako ulogovani korisnik nije dodavao projekte")]
        [Category("GetMyProjects")]
        public async Task GetMyProjects_Empty_wUserisLoggedIn_NoProjectsAdded()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManagerWData("1", "user1", "user1@gmail.com");
            _db.Users.Add(manager);

            await _db.SaveChangesAsync();

            var response = await _controller.GetMyProjects();
            var okResponse = response as OkObjectResult;
            var projects = okResponse!.Value as List<ProjectResponseDto>;

            Assert.That(projects, Has.Count.EqualTo(0));
        }

        [Test, Description("GetMyProjects bi trebalo da vrati sve projekte koji pripadaju ulogovanom korisniku")]
        [Category("GetMyProjects")]
        public async Task GetMyProjects_UsersProjects_wUserIsLoggedIn()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager1 = EntityCreation.CreateManagerWData("1", "user1", "user1@gmail.com");
            var manager2 = EntityCreation.CreateManagerWData("2", "user2", "user2@gmail.com");
            _db.Users.AddRange(manager1, manager2);

            var project1 = EntityCreation.CreateProject(1, manager1);
            var project2 = EntityCreation.CreateProject(2, manager1);
            var project3 = EntityCreation.CreateProject(3, manager2);
            _db.Projects.AddRange(project1, project2, project3);

            await _db.SaveChangesAsync();

            var response = await _controller.GetMyProjects();
            var okResponse = response as OkObjectResult;
            var projects = okResponse!.Value as List<ProjectResponseDto>;

            Assert.That(projects, Has.Count.EqualTo(2));
            Assert.That(projects!.All(p => p.OwnerId == "1"), Is.True);
        }

        [Test, Description("GetMyProjects bi trebalo da vrati listu Bugova koji se nalaze u projektu korisnika")]
        [Category("GetMyProjects")]
        public async Task GetMyProjects_UserProjectswBugs_wProjectsAndBugsAdded()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug1 = EntityCreation.CreateBugWData(1, project, manager, "Bug1");
            var bug2 = EntityCreation.CreateBugWData(2, project, manager, "Bug2");
            _db.Bugs.AddRange(bug1, bug2);

            await _db.SaveChangesAsync();

            var response = await _controller.GetMyProjects();
            var okResponse = response as OkObjectResult;
            var projects = okResponse!.Value as List<ProjectResponseDto>;

            Assert.That(projects![0].BugsSum, Has.Count.EqualTo(2));
            Assert.That(projects![0].BugsSum!.Select(b => b.Name), Is.EquivalentTo(new[] { "Bug1", "Bug2" }));
        }

        #endregion

        #region CreateTests

        [Test, Description("Create bi trebalo da vrati UnauthorizedResult ako korisnik nije ulogovan")]
        [Category("Create")]
        public async Task Create_UnauthorizedResult_wNoUserLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var createDto = new CreateProjectDto { Name = "Test Project", Description = "Desc", Status = ProjectStatus.Active };
            var response = await _controller.Create(createDto);

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test, Description("Create bi trebalo da sacuva projekat u bazi ako su mu prosledjeni korekni podaci i korisnik je ulogovan i postoji u bazi")]
        [Category("Create")]
        public async Task Create_SavedProject_wManagerLoggedInAndValidData()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            await _db.SaveChangesAsync();

            var createDto = new CreateProjectDto { Name = "Test Project", Description = "Desc", Status = ProjectStatus.Active };
            var response = await _controller.Create(createDto);
            var project = _db.Projects.First();

            Assert.That(project.Name, Is.EqualTo("Test Project"));
            Assert.That(project.Description, Is.EqualTo("Desc"));
            Assert.That(project.Status, Is.EqualTo(ProjectStatus.Active));
            Assert.That(project.OwnerId, Is.EqualTo("1"));
        }

        [Test, Description("Create baca BadRequest gresku ako se projekat kreira sa praznim Name poljem")]
        [Category("Create")]
        public async Task Create_BadRequest_wNoValidData()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            await _db.SaveChangesAsync();

            var createDto = new CreateProjectDto { Name = "", Description = "Desc", Status = ProjectStatus.Active };
            var response = await _controller.Create(createDto);
            Assert.That(response, Is.TypeOf<BadRequestObjectResult>());
        }


        [Test, Description("Create baca BadRequest ako ulogovan korisnik nije u bazi")]
        [Category("Create")]
        public async Task Create_BadRequest_NoUserInDb()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var createDto = new CreateProjectDto { Name = "Test Project", Description = "Desc", Status = ProjectStatus.Active };
            var response = await _controller.Create(createDto);

            Assert.That(response, Is.TypeOf<BadRequestObjectResult>());
        }

        #endregion

        #region UpdateTests

        [Test]
        [Category("Update")]
        public async Task Update_ProjectChanged_wValidDataAndUserLoggedIn()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProjectWData(1, manager, "BEFORE", "", ProjectStatus.Active);
            _db.Projects.Add(project);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateProjectDto { Name = "UPDATE", Status = ProjectStatus.Planning };

            var result = await _controller.Update(1, updateDto);
            var updatedProject = _db.Projects.First();

            Assert.That(updatedProject.Name, Is.EqualTo("UPDATE"));
            Assert.That(updatedProject.Status, Is.EqualTo(ProjectStatus.Planning));
        }

        [Test, Description("Update bi trebalo da vrati Forbid ako korisnik A pokusa da promeni projekat korisnika B")]
        [Category("Update")]
        public async Task Update_Forbid_wDifferentOwner()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var difManager = EntityCreation.CreateManager("2");
            _db.Users.Add(difManager);

            var project = EntityCreation.CreateProject(1, difManager);
            _db.Projects.Add(project);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateProjectDto { Name = "TEST", Status = ProjectStatus.Active };
            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test, Description("Update bi trebalo da vrati BadRequest ako korisnik pokusa da proemni projekat nevalidnim podacima (prazno ime)")]
        [Category("Update")]
        public async Task Update_BadRequest_wNotValidData()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateProjectDto { Name = "", Status = ProjectStatus.Active };
            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("Update bi trebalo da vrati NotFound ako korisnik pokusa da azurira nepostojeci projekat")]
        [Category("Update")]
        public async Task Update_NotFound_wNoProjects()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateProjectDto { Name = "TEST", Status = ProjectStatus.Active };
            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        #endregion

        #region DeleteTests

        [Test, Description("Delete bi trebalo da obrise projekat koji je korisnik dodao")]
        [Category("Delete")]
        public async Task Delete_NoProject_AfterUserDeletesHisProject()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            await _db.SaveChangesAsync();

            await _controller.Delete(1);
            var projectsInDb = _db.Projects.Any();

            Assert.That(projectsInDb, Is.False);
        }

        [Test, Description("Delete bi trebalo da vrati Forbid ako korisnik pokusa da obrise tudji projekat")]
        [Category("Delete")]
        public async Task Delete_Forbid_UserTriesToDeleteSomeoneElsesProject()
        {

            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var difManager = EntityCreation.CreateManager("2");
            _db.Users.Add(difManager);

            var project = EntityCreation.CreateProject(1, difManager);
            _db.Projects.Add(project);

            await _db.SaveChangesAsync();

            var result = await _controller.Delete(1);
            var projectsInDb = _db.Projects.Any();

            Assert.That(result, Is.TypeOf<ForbidResult>());
            Assert.That(projectsInDb, Is.True);
        }

        [Test, Description("Delete bi trebalo da obrise samo projekat ciji se Id prosledi")]
        [Category("Delete")]
        public async Task Delete_DeletesJustOne()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var difManager = EntityCreation.CreateManager("1");
            _db.Users.Add(difManager);

            var project1 = EntityCreation.CreateProjectWData(1, difManager, "P1", "DESC", ProjectStatus.Active);
            var project2 = EntityCreation.CreateProjectWData(2, difManager, "P2", "DESC", ProjectStatus.Active);
            _db.Projects.AddRange(project1, project2);

            await _db.SaveChangesAsync();

            await _controller.Delete(1);

            Assert.That(_db.Projects.Count(), Is.EqualTo(1));
            Assert.That(_db.Projects.First().Name, Is.EqualTo("P2"));


        }

        [Test, Description("Delete bi trebalo da obrise i bugove koji se nalaze u projektu")]
        [Category("Delete")]
        public async Task Delete_BugsConnectedToProject()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.AddRange(project);

            var bug1 = EntityCreation.CreateBug(1, project, manager);
            var bug2 = EntityCreation.CreateBug(2, project, manager);
            _db.Bugs.AddRange(bug1, bug2);

            await _db.SaveChangesAsync();

            await _controller.Delete(1);

            Assert.That(_db.Projects.Any(), Is.False);
            Assert.That(_db.Bugs.Any(), Is.False);
        }


        #endregion


    }
}
