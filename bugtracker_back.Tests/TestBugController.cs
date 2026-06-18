using bugtracker_back.Controllers;
using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Services;
using bugtracker_back.Tests.Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace bugtracker_back.Tests
{
    [TestFixture]
    public class TestBugController
    {
        private AppDbContext _db;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<IWebHostEnvironment> _envMock;
        private ImageService _imageService;
        private BugController _controller;

        [SetUp]
        public void Setup()
        {
            _db = DbContextFactory.Create();

            var store = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            _imageService = new ImageService(_envMock.Object);

            _controller = new BugController(_db, _userManagerMock.Object, _imageService);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        #region GetAllTests

        [Test, Description("GetAll bi trebalo da vrati sve bugove svih projekata i vlasnika")]
        [Category("GetAll")]
        public async Task GetAll_AllBugs_FromAllProjectsAndOwners()
        {
            // 2 razlicita projekta i vlasnika
            var bug1 = EntityCreation.CreateCompleteBug(1);
            var bug2 = EntityCreation.CreateCompleteBug(2);
            _db.Bugs.AddRange(bug1, bug2);

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Is.Not.Empty);
            Assert.That(bugs, Has.Count.EqualTo(2));
        }

        [Test, Description("GetAll bi trebalo da vrati praznu listu ako nema bugova u bazi")]
        [Category("GetAll")]
        public async Task GetAll_Empty_NoBugsInDb()
        {
            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Is.Empty);
        }

        [Test, Description("GetAll bi trebalo da vrati sve bugove sa korektno mapiranim korisnicima i projektima")]
        [Category("GetAll")]
        public async Task GetAll_MappedProjectsAndOwners()
        {
            var owner1 = EntityCreation.CreateManager("1");
            var owner2 = EntityCreation.CreateManager("2");
            _db.Users.AddRange(owner1, owner2);

            var project1 = EntityCreation.CreateProject(1, owner1);
            var project2 = EntityCreation.CreateProject(2, owner2);
            _db.Projects.AddRange(project1, project2);

            var bug1 = EntityCreation.CreateBug(1, project1, owner1);
            var bug2 = EntityCreation.CreateBug(2, project1, owner2);
            var bug3 = EntityCreation.CreateBug(3, project2, owner2);
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs!.Select(b => b.OwnerId), Is.EquivalentTo(new[] { "1", "2", "2" }));
            Assert.That(bugs!.Select(b => b.ProjectId), Is.EquivalentTo(new[] { 1, 1, 2 }));
        }

        [Test, Description("GetAll bi trebalo da bug kao i ime vlasnika")]
        [Category("GetAll")]
        public async Task GetAll_OwnerNameCheck()
        {
            var owner1 = EntityCreation.CreateManagerWData("1", "TEST", "email@gmail.com");
            _db.Users.Add(owner1);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBug(1, project1, owner1);
            _db.Bugs.Add(bug1);

            await _db.SaveChangesAsync();

            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs!.First().OwnerName, Is.Not.Null.And.Not.Null);
            Assert.That(bugs!.First().OwnerName, Is.EqualTo("TEST"));
        }
        #endregion

        #region GetById

        [Test, Description("GetById bi trebalo da vrati Bug ciji se id poklapa sa trazenim")]
        [Category("GetById")]
        public async Task GetById_Bug_wBugInDatabase()
        {
            var bug1 = EntityCreation.CreateCompleteBug(1);
            _db.Bugs.Add(bug1);

            await _db.SaveChangesAsync();

            var result = await _controller.GetById(1);
            var okResult = result as OkObjectResult;
            var bug = okResult!.Value as BugResponseDto;

            Assert.That(bug, Is.Not.Null);
        }

        [Test, Description("GetById bi trebalo da vrati NotFound error ako ne postoji bug sa trazenim id-em")]
        [Category("GetById")]
        public async Task GetById_NotFound_wIncorrectId()
        {
            var bug1 = EntityCreation.CreateCompleteBug(1);
            _db.Bugs.Add(bug1);

            await _db.SaveChangesAsync();

            var result = await _controller.GetById(2);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }


        [Test, Description("GetById vraca korektno mapirane vrednosti vlasnika i projekta")]
        [Category("GetById")]
        public async Task GetById_MultipleMapping()
        {
            var owner1 = EntityCreation.CreateManagerWData("1", "ManagerName", "email@gmail.com");
            _db.Users.Add(owner1);

            var project1 = EntityCreation.CreateProjectWData(1, owner1, "ProjectName", "ProjectDesc", ProjectStatus.Active);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBugWData(1, project1, owner1, "BugName");
            _db.Bugs.Add(bug1);

            await _db.SaveChangesAsync();

            var result = await _controller.GetById(1);
            var okResult = result as OkObjectResult;
            var bug = okResult!.Value as BugResponseDto;

            Assert.Multiple(() =>
            {
                Assert.That(bug, Is.Not.Null);
                Assert.That(bug!.OwnerName, Is.EqualTo(bug1.Owner.UserName));
                Assert.That(bug!.ProjectName, Is.EqualTo(bug1.Project.Name));
                Assert.That(bug!.Name, Is.EqualTo(bug1.Name));
                Assert.That(bug!.Status, Is.EqualTo(bug1.Status));
            });
        }

        #endregion

        #region GetByProject

        [Test, Description("GetByProject bi trebalo da vrati sve bugove koji pripadaju jednom projektu ciji je Id prosledjen")]
        [Category("GetByProject")]
        public async Task GetByProject_AllBugsFromOneProject()
        {
            var owner1 = EntityCreation.CreateManager("1");
            _db.Users.Add(owner1);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBug(2, project1, owner1);
            var bug2 = EntityCreation.CreateBug(3, project1, owner1);
            var bug3 = EntityCreation.CreateBug(4, project1, owner1);
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetByProject(1);
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Has.Count.EqualTo(3));
        }

        [Test, Description("GetByProject bi treabalo da vrati praznu listu ako ne postoji projekat sa trazenim Id-em")]
        [Category("GetByProject")]
        public async Task GetByProject_NoProject()
        {
            var result = await _controller.GetByProject(1);
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Is.Empty);
        }

        [TestCase("A", "AAA")]
        [TestCase("B", "BBB")]
        [TestCase("c", "CCC")]
        [Description("GetByProject bi trebalo da vrati korektat bug u projektu ako se poklapa sa searchInput-om")]
        [Category("GetByProject")]
        [Ignore("Case sensitive ne radi InMemory pa failuje c za CCC")]
        public async Task GetByProject_ReturnsBugsBasedOnName_wSearchInput(string searchInput, string expected)
        {
            var owner1 = EntityCreation.CreateManager("1");
            _db.Users.Add(owner1);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBugWData(2, project1, owner1, "AAA");
            var bug2 = EntityCreation.CreateBugWData(3, project1, owner1, "BBB");
            var bug3 = EntityCreation.CreateBugWData(4, project1, owner1, "CCC");
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetByProject(1, searchInput);
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs!.First().Name, Is.EqualTo(expected));
        }


        [Test, Description("GetByProject bi trebalo da vrati praznu listu ako searchinput ne poklapa sa bugovima u bazi")]
        [Category("GetByProject")]
        public async Task GetByProject_SearchFail()
        {
            var owner1 = EntityCreation.CreateManager("1");
            _db.Users.Add(owner1);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBugWData(2, project1, owner1, "A");
            _db.Bugs.Add(bug1);

            await _db.SaveChangesAsync();

            var result = await _controller.GetByProject(1, "B");
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Has.Count.EqualTo(0));
        }

        #endregion

        #region GetMyBugsByProject

        [Test, Description("GetMyBugsByProject bi trebalo da vrati samo bugove iz projekta ciji je vlasnik ulogovan korisnik")]
        [Category("GetMyBugsByProject")]
        public async Task GetMyBugsByProject_ReturnsBugsFromCurrentUser_FromProjectWithId()
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

            var owner1 = EntityCreation.CreateManager("1");
            var owner2 = EntityCreation.CreateManager("2");
            _db.Users.AddRange(owner1, owner2);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBug(1, project1, owner1);
            var bug2 = EntityCreation.CreateBug(2, project1, owner1);
            var bug3 = EntityCreation.CreateBug(3, project1, owner2);
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetMyBugsByProject(1);
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Has.Count.EqualTo(2));
        }

        [Test, Description("GetMyBugsByProject bi trebalo da vrati praznu listu ako ne postoji projekat sa bugovima")]
        [Category("GetMyBugsByProject")]
        public async Task GetMyBugsByProject_Empty_IfNoProjectOrBugs()
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

            var result = await _controller.GetMyBugsByProject(1);
            var okResult = result as OkObjectResult;
            var bugs = okResult!.Value as List<BugResponseDto>;

            Assert.That(bugs, Is.Empty);
        }

        [Test, Description("GetMyBugsByProject bi trebalo da vrati unauthorized ako se funkcija pozove bez ulogovanog korisnika")]
        [Category("GetMyBugsByProject")]
        public async Task GetMyBugsByProject_Unauthorized_wNoUserLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var owner1 = EntityCreation.CreateManager("1");
            var owner2 = EntityCreation.CreateManager("2");
            _db.Users.AddRange(owner1, owner2);

            var project1 = EntityCreation.CreateProject(1, owner1);
            _db.Projects.Add(project1);

            var bug1 = EntityCreation.CreateBug(1, project1, owner1);
            var bug2 = EntityCreation.CreateBug(2, project1, owner2);
            var bug3 = EntityCreation.CreateBug(3, project1, owner2);
            _db.Bugs.AddRange(bug1, bug2, bug3);

            await _db.SaveChangesAsync();

            var result = await _controller.GetMyBugsByProject(1);
            Assert.That(result, Is.TypeOf<UnauthorizedResult>());

        }

        #endregion

        #region Create

        [Test]
        [Category("Create")]
        public async Task Create_BugSaved_wValidDataAndUserLogged()
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

            var createDto = new CreateBugDto
            {
                Name = "Bug",
                Platform = Platform.iOS,
                Priority = Priority.Low,
                Severity = Severity.Low,
                ProjectId = project.Id
            };

            var result = await _controller.Create(createDto);
            var bug = _db.Bugs.First();

            Assert.That(bug.Name, Is.EqualTo("Bug"));
        }

        [Test]
        [Category("Create")]
        public async Task Create_Unauthorized_wValidDataAndUserNotLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var createDto = new CreateBugDto
            {
                Name = "Bug",
                Platform = Platform.iOS,
                Priority = Priority.Low,
                Severity = Severity.Low,
                ProjectId = project.Id
            };

            var result = await _controller.Create(createDto);

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        [Category("Create")]
        public async Task Create_NotFound_NoProject()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var createDto = new CreateBugDto
            {
                Name = "Bug",
                Platform = Platform.iOS,
                Priority = Priority.Low,
                Severity = Severity.Low,
                ProjectId = 1
            };

            var result = await _controller.Create(createDto);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        [Category("Create")]
        public async Task Create_BugSaved_MultipleDataAssertion_wImage()
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

            var img = EntityCreation.CreateFakeFormFile("img.png");
            var createDto = new CreateBugDto
            {
                Name = "Bug",
                Platform = Platform.iOS,
                Priority = Priority.Low,
                Severity = Severity.Low,
                ProjectId = project.Id,
                Image = img
            };

            var result = await _controller.Create(createDto);
            var bug = _db.Bugs.First();

            Assert.Multiple(() =>
            {
                Assert.That(bug.DateAdded, Is.EqualTo(DateTime.UtcNow).Within(2).Minutes);
                Assert.That(bug.DateFixed, Is.Null);
                Assert.That(bug.ImageUrl, Does.StartWith("/uploads/"));
                Assert.That(bug.ImageUrl, Does.EndWith(".png"));
                Assert.That(bug.Name, Is.EqualTo("Bug"));
            });
        }

        #endregion

        #region Update

        [Test, Description("Update bi trebalo da promeni bug u bazi")]
        [Category("Update")]
        public async Task Update_BugChanged_wValidData()
        {
           
            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var dateTime = DateTime.UtcNow;
            var bug = EntityCreation.CreateBugWDateAdded(1, project, manager, dateTime, "BUG1");
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateBugDto
            {
                Name = "CHANGED",
                Platform = Platform.Android,
                Priority = Priority.Medium,
                Severity = Severity.Low
            };

            await _controller.Update(1, updateDto);
            var changedBug = _db.Bugs.First();

            Assert.That(changedBug.Name, Is.EqualTo("CHANGED"));
        }


        [Test, Description("Update bi trebalo da vrati gresku ako se prosledi prazno ime za bug")]
        [Category("Update")]
        public async Task Update_Error_wNoValidData()
        {

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var dateTime = DateTime.UtcNow;
            var bug = EntityCreation.CreateBugWDateAdded(1, project, manager, dateTime, "BUG1");
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateBugDto
            {
                Name = "",
                Platform = Platform.Android,
                Priority = Priority.Medium,
                Severity = Severity.Low
            };

            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("Update bi trebalo da vrati gresku ako ne postoji bug u bazi")]
        [Category("Update")]
        public async Task Update_Error_NoBugsOrProjects()
        {
            var updateDto = new UpdateBugDto
            {
                Name = "Test",
                Platform = Platform.Android,
                Priority = Priority.Medium,
                Severity = Severity.Low
            };

            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test, Description("Update dateFixed polja bi trebalo da promeni BugStatus na fixed")]
        [Category("Update")]
        public async Task Update_StatusFixed_AfterAddingDateFixedToBug()
        {
            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var dateTime = DateTime.UtcNow;
            var bug = EntityCreation.CreateBugWDateAdded(1, project, manager, dateTime, "BUG1");
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateBugDto
            {
                Name = "CHANGED",
                Platform = Platform.Android,
                Priority = Priority.Medium,
                Severity = Severity.Low,
                DateFixed = dateTime.AddDays(1)
            };

            await _controller.Update(1, updateDto);
            var changedBug = _db.Bugs.First();

            Assert.That(changedBug.Status, Is.EqualTo(BugStatus.Fixed));
        }

        [Test, Description("Update dateFixed polja na vrednost pre DateAdded bi trebalo da vrati gresku")]
        [Category("Update")]
        public async Task Update_Error_IfFixedBeforeAdded()
        {
           var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var dateTime = DateTime.UtcNow;
            var bug = EntityCreation.CreateBugWDateAdded(1, project, manager, dateTime, "BUG1");
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            var updateDto = new UpdateBugDto
            {
                Name = "CHANGED",
                Platform = Platform.Android,
                Priority = Priority.Medium,
                Severity = Severity.Low,
                DateFixed = dateTime.AddDays(-1)
            };

            var result = await _controller.Update(1, updateDto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        #endregion

        #region Delete

        [Test, Description("Delete bi trebalo da obrise bug iz baze")]
        [Category("Delete")]
        public async Task Delete_RemovesBug()
        {
            var bug = EntityCreation.CreateCompleteBug(1);
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            await _controller.Delete(1);

            Assert.That(_db.Bugs.Any(), Is.False);
        }

        [Test, Description("Delete bi trebalo da vrati gresku ako bug ne postoji u bazi")]
        [Category("Delete")]
        public async Task Delete_Error_NoBugsInDb()
        {
            var result = await _controller.Delete(1);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test, Description("Delete bi trebalo da obrise bug i sliku buga iz baze")]
        [Category("Delete")]
        public async Task Delete_RemovesBug_wImageAttached()
        {
            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug = EntityCreation.CreateBug(1, project, manager);
            bug.ImageUrl = "/uploads/fake-image.png"; 
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();
            await _controller.Delete(1);

            Assert.That(_db.Bugs.Any(), Is.False);
        }

        #endregion

    }
}
