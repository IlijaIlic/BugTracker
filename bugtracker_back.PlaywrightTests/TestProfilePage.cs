using Microsoft.Playwright.NUnit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace bugtracker_back.PlaywrightTests
{
    [TestFixture]
    public class TestProfilePage : PageTest
    {

      
        public async Task ManagerLogin()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/profile");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");


        }

        public async Task TesterLogin()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailTester);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordTester);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/profile");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");
        }

        public async Task AddLowAndroidBug(string bugName)
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/profile"));

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-addbug-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeVisibleAsync();

            await Page.GetByTestId("addbug-name-inp").FillAsync(bugName);
            await Page.GetByTestId("addbug-platform-cmb").ClickAsync();
            await Page.GetByTestId("addbug-platform-cmb").GetByText("Android").ClickAsync();

            await Page.GetByTestId("addbug-priority-cmb").ClickAsync();
            await Page.GetByTestId("addbug-priority-cmb").GetByText("Low").ClickAsync();

            await Page.GetByTestId("addbug-severity-cmb").ClickAsync();
            await Page.GetByTestId("addbug-severity-cmb").GetByText("Low").ClickAsync();

            await Page.GetByTestId("addbug-description-inp").FillAsync("Description");

            await Page.GetByTestId("addbug-submit-btn").ClickAsync();

            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Expect(newRow).ToBeVisibleAsync();
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameManager);

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/profile");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");
        }

        [Test]
        [Category("ProfilePage")]
        public async Task Profile_CorrectDataDisplyedManager_Check()
        {
            await ManagerLogin();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");

            await Expect(Page.GetByTestId("profile-username-lbl")).ToContainTextAsync($"{GlobalSetup.TestUserNameManager} Profile");
            await Expect(Page.GetByTestId("profile-role-lbl")).ToContainTextAsync("Manager");
            await Expect(Page.GetByTestId("profile-username-inp")).ToHaveValueAsync(GlobalSetup.TestUserNameManager);
            await Expect(Page.GetByTestId("profile-email-inp")).ToHaveValueAsync(GlobalSetup.TestUserEmailManager);
        }

        [Test]
        [Category("ProfilePage")]
        public async Task Profile_CorrectDataDisplyedTester_Check()
        {
            await TesterLogin();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");

            await Expect(Page.GetByTestId("profile-username-lbl")).ToContainTextAsync($"{GlobalSetup.TestUserNameTester} Profile");
            await Expect(Page.GetByTestId("profile-role-lbl")).ToContainTextAsync("Tester");
            await Expect(Page.GetByTestId("profile-username-inp")).ToHaveValueAsync(GlobalSetup.TestUserNameTester);
            await Expect(Page.GetByTestId("profile-email-inp")).ToHaveValueAsync(GlobalSetup.TestUserEmailTester);
        }

        [Test]
        [Category("ProfilePage")]
        public async Task Profile_MyProjectsButtonInvisible_TesterProfile()
        {
            await TesterLogin();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");

            await Expect(Page.GetByTestId("profile-mybugs-btn")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("profile-myprojects-btn")).ToBeHiddenAsync();
        }

        [Test]
        [Category("EditProfile")]
        public async Task Profile_DataEddited_WithValidEditUserData()
        {
            await ManagerLogin();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");
            var usernameNew = $"Username{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("profile-username-inp").FillAsync(usernameNew);
            await Page.GetByTestId("profile-email-inp").FillAsync($"{usernameNew}@test.com");
            await Page.GetByTestId("profile-edit-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/profile");
            await Expect(Page.GetByTestId("profile-username-lbl")).ToContainTextAsync(usernameNew);
            await Expect(Page.GetByTestId("profile-username-inp")).ToHaveValueAsync(usernameNew);
            await Expect(Page.GetByTestId("profile-email-inp")).ToHaveValueAsync($"{usernameNew}@test.com");

            //CLEANUP
            await Page.GetByTestId("profile-username-inp").FillAsync(GlobalSetup.TestUserNameManager);
            await Page.GetByTestId("profile-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("profile-edit-btn").ClickAsync();
        }

        [Test]
        [Category("EditProfile")]
        public async Task Profile_AlertPopup_WithInvalidUsername()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            await ManagerLogin();
            await Page.GetByTestId("profile-username-inp").FillAsync("");
            await Page.GetByTestId("profile-edit-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("EditProfile")]
        public async Task Profile_AlertPopup_WithInvalidEmail()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };


            await ManagerLogin();
            await Page.GetByTestId("profile-email-inp").FillAsync("asdf");
            await Page.GetByTestId("profile-edit-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("ProfilePage")]
        public async Task Profile_MyBugs_Check()
        {

            var bugName = $"BugName{Guid.NewGuid().ToString("N")[..8]}";

            await ManagerLogin();
            await AddLowAndroidBug(bugName);

            await Page.GetByTestId("profile-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("profile-mybugs-div")).ToBeVisibleAsync();

            var bug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });
            await Expect(bug).ToBeVisibleAsync();
        }

        [Test]
        [Category("ProfilePage")]
        public async Task Profile_MyProjects_Check()
        {
            await ManagerLogin();

            await Page.GetByTestId("profile-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("profile-myprojects-div")).ToBeVisibleAsync();

            var project = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(project).ToBeVisibleAsync();
        }


        [Test]
        [Category("DeleteProfile")]
        public async Task Profile_CantLoginToDeletedProfile_AfterProfileDelete()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

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
            await Page.GetByTestId("profile-delete-btn").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/");

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(email);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();


            await Page.WaitForTimeoutAsync(1000);
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }


    }
}
