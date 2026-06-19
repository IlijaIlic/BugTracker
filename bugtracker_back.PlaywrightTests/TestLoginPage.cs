using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;
using System.Collections.Generic;
using System.Text;

namespace bugtracker_back.PlaywrightTests
{
    public class TestLoginPage : PageTest
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
        [Category("LoginPage")]
        public async Task LoginPage_OnRegisterClick_NavigatesToRegisterPage()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");
            await Page.GetByTestId("log-register-redirect-btn").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/register");
        }

        [Test]
        [Category("LoginPage")]
        public async Task LoginPage_OnInvalidDataLoginTry_ErrorAlert()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            await Page.GetByTestId("log-email-inp").FillAsync("noUser");
            await Page.GetByTestId("log-password-inp").FillAsync("noUser");
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("LoginPage")]
        public async Task LoginPage_OnValidLogin_SuccessfulLoginAndRedirectedToProjects()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }


        [Test]
        [Category("LoginPage")]
        [Ignore("For Testing")]
        public async Task LoginPage_ValidData_NameInFooterChanged()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("foot-username-lbl")).ToContainTextAsync(GlobalSetup.TestUserNameManager);

        }

        [Test]
        [Category("LoginPage")]
        public async Task LoginPage_UserLoggedIn_RedirectedToProject()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }
    }
}
