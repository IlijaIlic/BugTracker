using bugtracker_back.Controllers;
using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace bugtracker_back.Tests
{
    [TestFixture]
    public class TestAuthController
    {

        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private Mock<IConfiguration> _configMock;
        private AuthController _controller;

        [SetUp]
        public void SetUp()
        {
            var userStore = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null!, null!, null!, null!);

            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["Jwt:Key"]).Returns("8J2SfWlv9QXSpOwCjraVMppCHuYoVeTw");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("BugTrackerAPI");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("BugTrackerClient");
            _configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            _controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _configMock.Object);
        }

        //Bez TearDown jer ne radimo sa db

        #region Register

        [Test, Description("Register bi trebalo da vrati gresku ako korisnik pokusa da se registruje sa role-om koji nije Manager ili Tester")]
        [Category("Register")]
        public async Task Register_Error_wInvalidRole()
        {
            var dto = new RegisterDto { Username = "user1", Email = "user1@gmaiil.com", Password = "Password123!", Role = "Test" };

            var result = await _controller.Register(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("Register bi trebalo da vrati BadRequest ako CreateAsync ne uspe (jednostavna sifra)")]
        [Category("Register")]
        public async Task Register_Error_wCreateAsyncFail()
        {
            var errors = new[] { new IdentityError { Description = "Weak password" } };
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var dto = new RegisterDto { Username = "user1", Email = "user1@gmail.com", Password = "asdf", Role = "Tester" };
            var result = await _controller.Register(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

		[Test, Description("Register bi trebalo da vrati BadRequest ako je email nevalidan")]
        [Category("Register")]
        public async Task Register_Error_wInvalidEmail()
        {
            var errors = new[] { new IdentityError { Description = "Invalid email" } };
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var dto = new RegisterDto { Username = "user1", Email = "asdfasdf", Password = "Password123", Role = "Tester" };
            var result = await _controller.Register(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test, Description("Register bi trebalo da sacuva novog korisnika")]
        [Category("Register")]
        public async Task Register_ManagerAdded_wValidData()
        {
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(r => r.RoleExistsAsync("Manager"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Manager"))
                .ReturnsAsync(IdentityResult.Success);

            var dto = new RegisterDto
            {
                Username = "manager1",
                Email = "manager1@test.com",
                Password = "Password123!",
                Role = "Manager"
            };

            var result = await _controller.Register(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());

            _userManagerMock.Verify(m => m.CreateAsync(It.Is<AppUser>(u => u is Manager), "Password123!"), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Manager"), Times.Once);
        }


        [Test, Description("Register bi trebalo da sacuva novog Testera")]
        [Category("Register")]
        public async Task Register_TesterAdded_wValidData()
        {
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(r => r.RoleExistsAsync("Tester"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Tester"))
                .ReturnsAsync(IdentityResult.Success);

            var dto = new RegisterDto
            {
                Username = "manager1",
                Email = "manager1@test.com",
                Password = "Password123!",
                Role = "Tester"
            };

            var result = await _controller.Register(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());

            _userManagerMock.Verify(m => m.CreateAsync(It.Is<AppUser>(u => u is Tester), "Password123!"), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Tester"), Times.Once);
        }

        #endregion

        #region Login

        [Test, Description("Login bi trebalo da vrati Unauthorized ako korisnik sa datim email-om ne postoji")]
        [Category("Login")]
        public async Task Login_Unauthorized_wUserDoesNotExist()
        {
            _userManagerMock.Setup(m => m.FindByEmailAsync("email@test.com"))
                .ReturnsAsync((AppUser?)null);

            var dto = new LoginDto { Email = "email@test.com", Password = "Password123!" };

            var result = await _controller.Login(dto);

            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
        }

        [Test, Description("Login bi trebalo da vrati Unauthorized ako je lozinka pogresna")]
        [Category("Login")]
        public async Task Login_Unauthorized_wWrongPassword()
        {
            var user = EntityCreation.CreateManager("1");

            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrong"))
                .ReturnsAsync(false);

            var dto = new LoginDto { Email = user.Email!, Password = "wrong" };

            var result = await _controller.Login(dto);

            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
        }

        [Test, Description("Login bi trebalo da vrati token i podatke o korisniku ako su email i lozinka tacni")]
        [Category("Login")]
        public async Task Login_ReturnsToken_wValidCredentials()
        {
            var user = EntityCreation.CreateManager("1");

            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Password123!"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Manager" });

            var dto = new LoginDto { Email = user.Email!, Password = "Password123!" };

            var result = await _controller.Login(dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            var response = okResult!.Value as AuthResponseDto;

            Assert.That(response!.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Email, Is.EqualTo(user.Email));
            Assert.That(response.Role, Is.EqualTo("Manager"));
        }

        [Test, Description("Login bi trebalo da koristi 'Tester' kao default role ako korisnik nema dodeljenu rolu")]
        [Category("Login")]
        public async Task Login_DefaultsToTester_wNoRoleAssigned()
        {
            var user = EntityCreation.CreateManager("1");

            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!))
                .ReturnsAsync(user);

            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Password123!"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string>()); 

            var dto = new LoginDto { Email = user.Email!, Password = "Password123!" };

            var result = await _controller.Login(dto);
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as AuthResponseDto;

            Assert.That(response!.Role, Is.EqualTo("Tester"));
        }

        #endregion


    }
}
