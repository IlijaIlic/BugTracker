using bugtracker_back.Controllers;
using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Tests.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace bugtracker_back.Tests
{
    [TestFixture]
    public class TestUserController
    {

        private AppDbContext _db;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private UserController _controller;

        #region SetUp + TearDown

        [SetUp]
        public void SetUp()
        {
            _db = DbContextFactory.Create();

            var userStore = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new UserController(_db, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        #endregion

        #region GetMe

        [Test, Description("GetMe bi trebalo da vrati UnaothorizedResult ako korisnik nije ulogovan")]
        [Category("GetMe")]
        public async Task GetMe_Unaothorized_noLoggedInUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetMe();

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test, Description("GetMe bi trebalo da vrati NotFound ako korisnik ne postoji u user manageru")]
        [Category("GetMe")]
        public async Task GetMe_NotFound_NoUserInUserManager()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((AppUser?)null);

            var result = await _controller.GetMe();

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test, Description("GetMe bi trebalo da vrati sve podatke vezane za odredjenog menadzera")]
        [Category("GetMe")]
        public async Task GetMe_ManagerData_wBugsAndProjects()
        {

            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var manager = EntityCreation.CreateManagerWData("1", "TEST", "test@gmail.com");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug = EntityCreation.CreateBug(1, project, manager);
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(manager);
            _userManagerMock.Setup(m => m.GetRolesAsync(manager)).ReturnsAsync(new List<string> { "Manager" });

            var result = await _controller.GetMe();
            var okResult = result as OkObjectResult;
            var profile = okResult!.Value as UserProfileDto;

            Assert.Multiple(() =>
            {
                Assert.That(profile!.Role, Is.EqualTo("Manager"));
                Assert.That(profile.Projects, Has.Count.EqualTo(1));
                Assert.That(profile.Bugs, Has.Count.EqualTo(1));
                Assert.That(profile.Username, Is.EqualTo("TEST"));
                Assert.That(profile.Email, Is.EqualTo("test@gmail.com"));

            });
        }

        [Test, Description("GetMe bi trebalo da vrati sve podatke vezane za odredjenog Testera")]
        [Category("GetMe")]
        public async Task GetMe_TesterDatawBugs()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "2") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var tester = EntityCreation.CreateTester("2");
            _db.Users.Add(tester);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug = EntityCreation.CreateBug(1, project, tester);
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(tester);
            _userManagerMock.Setup(m => m.GetRolesAsync(tester)).ReturnsAsync(new List<string> { "Tester" });

            var result = await _controller.GetMe();
            var okResult = result as OkObjectResult;
            var profile = okResult!.Value as UserProfileDto;


            Assert.That(profile!.Role, Is.EqualTo("Tester"));
            Assert.That(profile.Bugs, Has.Count.EqualTo(1));
        }


        #endregion

        #region UpdateMe

        [Test, Description("UpdateMe bi trebalo da vrati Unauthorized ako korisnik nije ulogovan")]
        [Category("UpdateMe")]
        public async Task UpdateMe_Unauthorized_wNoUserLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var dto = new UpdateUserDto { Username = "update", Email = "update@gmail.com" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());

        }

        [Test, Description("Update me bi trebalo da vrati NotFound ako korisnik ne postoji")]
        [Category("UpdateMe")]
        public async Task UpdateMe_NotFound_wNoUserInUserManager()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((AppUser?)null);

            var dto = new UpdateUserDto { Username = "newname", Email = "new@test.com" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test, Description("UpdateMe bi trebalo da vrati BadRequest ako korisnik pokusa da promeni email na vec zauzet")]
        [Category("UpdateMe")]
        public async Task UpdateMe_BadRequest_wEmailInUse()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var currentUser = EntityCreation.CreateManagerWData("1", "user1", "user1@gmail.com");
            var otherUser = EntityCreation.CreateManagerWData("2", "user2", "user2@gmail.com");

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(currentUser);
            _userManagerMock.Setup(m => m.FindByEmailAsync("user2@gmail.com")).ReturnsAsync(otherUser);

            var dto = new UpdateUserDto { Username = "user1", Email = "user2@gmail.com" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("UpdateMe bi trebalo da vrati BadRequest ako je username vec zauzet")]
        [Category("UpdateMe")]
        public async Task UpdateMe_BadRequest_wUsernameInUse()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var currentUser = EntityCreation.CreateManagerWData("1", "user1", "user1@gmail.com");
            var otherUser = EntityCreation.CreateManagerWData("2", "user2", "user2@gmail.com");

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(currentUser);
            _userManagerMock.Setup(m => m.FindByEmailAsync("user1@gmail.com")).ReturnsAsync(currentUser); 
            _userManagerMock.Setup(m => m.FindByNameAsync("user2")).ReturnsAsync(otherUser);

            var dto = new UpdateUserDto { Username = "user2", Email = "user1@test.com" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("UpdateMe bi trebalo da promeni vrednosti koje je korisnik prosledio")]
        [Category("UpdateMe")]
        public async Task UpdateMe_ChangedUser_wValidData()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var currentUser = EntityCreation.CreateManagerWData("1", "oldname", "old@gmail.com");

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(currentUser);
            _userManagerMock.Setup(m => m.FindByEmailAsync("new@gmail.com")).ReturnsAsync((AppUser?)null);
            _userManagerMock.Setup(m => m.FindByNameAsync("newname")).ReturnsAsync((AppUser?)null);
            _userManagerMock.Setup(m => m.UpdateAsync(currentUser)).ReturnsAsync(IdentityResult.Success);

            var dto = new UpdateUserDto { Username = "newname", Email = "new@gmail.com" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(currentUser.Email, Is.EqualTo("new@gmail.com"));
            Assert.That(currentUser.UserName, Is.EqualTo("newname"));
            _userManagerMock.Verify(m => m.UpdateAsync(currentUser), Times.Once);
        }


        [Test, Description("UpdateMe bi trebalo da vrati BadRequest ako korisnik pokusa da promeni email nevalidnim email-om")]
        [Category("UpdateMe")]
        public async Task UpdateMe_BadRequest_wInvalidEmail()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var dto = new UpdateUserDto { Username = "user1", Email = "asdf" };

            var result = await _controller.UpdateMe(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        
        }

        #endregion

        #region DeleteMe

        [Test,Description("DeleteMe bi trebalo da vrati Unauthorized ako korisnik nije ulogovan")]
        [Category("DeleteMe")]
        public async Task DeleteMe_Unauthorized_wNoUserLoggedIn()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.DeleteMe();

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test, Description("DeleteMe bi trebalo da vrati NotFound ako korisnik ne postoji u usermanageru")]
        [Category("DeleteMe")]
        public async Task DeleteMe_NotFound_wUserNotInUserManager()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((AppUser?)null);

            var result = await _controller.DeleteMe();

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test, Description("DeleteMe bi trebalo da obrise korisnika iz base kao i sve njegove projekte i bugove")]
        [Category("DeleteMe")]
        public async Task DeleteMe_UserDeleted_WithBugsAndProjects()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };


            var manager = EntityCreation.CreateManager("1");
            _db.Users.Add(manager);

            var project = EntityCreation.CreateProject(1, manager);
            _db.Projects.Add(project);

            var bug = EntityCreation.CreateBug(1, project, manager);
            _db.Bugs.Add(bug);

            await _db.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(manager);
            _userManagerMock.Setup(m => m.DeleteAsync(manager)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.DeleteMe();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(_db.Projects.Any(), Is.False);
            Assert.That(_db.Bugs.Any(), Is.False);
            _userManagerMock.Verify(m => m.DeleteAsync(manager), Times.Once);

        }

        #endregion
    }
}
