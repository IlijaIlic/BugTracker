using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;
using System.Collections.Generic;
using System.Text;

namespace bugtracker_back.PlaywrightTests
{
    [TestFixture]
    public class TestRegisterPage : PageTest
    {
        //SCREENSHOT CONFIG
        [TearDown]
        public async Task TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                var dir = Path.Combine("TestResults", "Screenshots");
                Directory.CreateDirectory(dir);

                var fileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var path = Path.Combine(dir, fileName);

                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = path,
                    FullPage = true
                });
            }
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_OnLoginClick_NavigatesToLoginPage()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-login-redirect-btn").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/login");
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_ValidDataManagerRole()
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"manager{id}";
            var email = $"manager{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Manager").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/profile");

            await Expect(Page.GetByTestId("profile-role-lbl")).ToContainTextAsync("Manager");
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_ValidDataTesterRole()
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"tester{id}";
            var email = $"tester{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Tester").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/profile");

            await Expect(Page.GetByTestId("profile-role-lbl")).ToContainTextAsync("Tester");
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_WithoutRoleSelected_Error()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"tester{id}";
            var email = $"tester{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);

            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_WithWrongConfirmPassword_Error()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"tester{id}";
            var email = $"tester{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync("pass" + username);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Tester").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_WithInvalidEmail_Error()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"tester{id}";
            var email = $"tester{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync("asd");
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Tester").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_WithWeakPassword_Error()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"tester{id}";
            var email = $"tester{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync("asd");
            await Page.GetByTestId("reg-password2-inp").FillAsync("asd");
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Tester").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("RegisterPage")]
        [Ignore("For Testing")]
        public async Task RegisterPage_ValidData_NameInFooterChanged()
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"manager{id}";
            var email = $"manager{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Manager").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("foot-username-lbl")).ToContainTextAsync(username);

        }

        [Test]
        [Category("RegisterPage")]
        public async Task RegisterPage_UserLoggedIn_Redirect()
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            var username = $"manager{id}";
            var email = $"manager{id}@test.com";

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Page.GetByTestId("reg-username-inp").FillAsync(username);
            await Page.GetByTestId("reg-email-inp").FillAsync(email);
            await Page.GetByTestId("reg-password1-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-password2-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("reg-role-cmb").ClickAsync();

            await Page.GetByTestId("reg-role-cmb").GetByText("Manager").ClickAsync();
            await Page.GetByTestId("reg-register-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/register");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }
    }
}
