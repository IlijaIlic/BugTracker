using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace bugtracker_back.PlaywrightTests
{
    [TestFixture]
    public class TestProjectsPage : PageTest
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

        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");

            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }

        public async Task ChangeToTester()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Page.GetByTestId("foot-logout-btn").ClickAsync();
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/login");
            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailTester);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordTester);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }

        public async Task OpenMyProjects()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeHiddenAsync();

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();
        }

        public async Task OpenAddProject()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

            await Page.GetByTestId("proj-addproject-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeVisibleAsync();
        }

        public async Task AddActiveProject(string projName)
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();


            await Page.GetByTestId("addproj-name-inp").FillAsync(projName);

            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();
        }

        public async Task AddBlockedProj(string projName)
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();


            await Page.GetByTestId("addproj-name-inp").FillAsync(projName);

            await Page.GetByTestId("addproj-status-cmb").ClickAsync();
            await Page.GetByTestId("addproj-status-cmb").GetByText("Planning").ClickAsync();


            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();
        }


        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_ManagerLoggedIn_HaveAddProjectAndMyProjectButtons()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(Page.GetByTestId("proj-myprojects-btn")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("proj-addproject-btn")).ToBeVisibleAsync();
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_TesterLoggedIn_DontHaveAddProjectAndMyProjectButtons()
        {
            await ChangeToTester();
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(Page.GetByTestId("proj-myprojects-btn")).ToBeHiddenAsync();
            await Expect(Page.GetByTestId("proj-addproject-btn")).ToBeHiddenAsync();
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnAddProjectClick_OpensAddProjectComponent()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

            await Page.GetByTestId("proj-addproject-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeVisibleAsync();
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnMyProjectClick_OpenMyProjectsComponent()
        {

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeHiddenAsync();

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

        }


        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnCloseAddProject_CloseAddProjectComponent()
        {

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();
            await Page.GetByTestId("addproj-close-btn").ClickAsync();

            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnCloseMyProjects_CloseMyProjectsComponent()
        {

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenMyProjects();
            await Page.GetByTestId("myproj-close-btn").ClickAsync();

            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeHiddenAsync();
        }


        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnProjectClick_RedirectToBugs()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Expect(projectRow).ToHaveCountAsync(1);
            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_OnBlockedProjectClick_DontRedirect()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddBlockedProj(projName);

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Expect(projectRow).ToHaveCountAsync(1);
            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_TesterOnProjectClick_RedirectToBugs()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await ChangeToTester();

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Expect(projectRow).ToHaveCountAsync(1);
            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
        }

        [Test]
        [Category("ProjectsPage")]
        public async Task ProjectPage_DescriptionCheck()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();

            var projectName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("addproj-name-inp").FillAsync(projectName);
            await Page.GetByTestId("addproj-status-cmb").ClickAsync();
            await Page.GetByTestId("addproj-status-cmb").GetByText("Active").ClickAsync();
            await Page.GetByTestId("addproj-desc-inp").FillAsync("TestDescription");

            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projectName });

            await Expect(newRow).ToBeVisibleAsync();
            await newRow.ClickAsync(new (){Timeout = 3000});

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            await Page.GetByTestId("bug-desc-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-desc-div")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("bug-desc-div")).ToContainTextAsync("TestDescription");

            await Page.GetByTestId("desc-close-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-desc-div")).ToBeHiddenAsync();

        }

        [Test]
        [Category("AddProject")]
        public async Task ProjectPage_AddProjectWValidData_ProjectAddedToTable()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();

            var projectName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("addproj-name-inp").FillAsync(projectName);
            await Page.GetByTestId("addproj-status-cmb").ClickAsync();
            await Page.GetByTestId("addproj-status-cmb").GetByText("Planning").ClickAsync();
            await Page.GetByTestId("addproj-desc-inp").FillAsync("TestDescription");

            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projectName });

            await Expect(newRow).ToBeVisibleAsync();
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameManager);
            await Expect(newRow).ToContainTextAsync("0");
            await Expect(newRow).ToContainTextAsync("Planning");
        }

        [Test]
        [Category("AddProject")]
        public async Task ProjectPage_AddProjectWithoutName_AlertPopup()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };


            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();
            await Page.GetByTestId("addproj-status-cmb").ClickAsync();
            await Page.GetByTestId("addproj-status-cmb").GetByText("Active").ClickAsync();
            await Page.GetByTestId("addproj-desc-inp").FillAsync("TestDescription");

            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);

        }

        [Test]
        [Category("AddProject")]
        public async Task ProjectPage_AddProjectWithOnlyName_ProjectAddedWithStatusActive()
        {
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await OpenAddProject();

            var projectName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("addproj-name-inp").FillAsync(projectName);

            await Page.GetByTestId("addproj-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-addproject-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projectName });

            await Expect(newRow).ToBeVisibleAsync();
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameManager);
            await Expect(newRow).ToContainTextAsync("0");
            await Expect(newRow).ToContainTextAsync("Active");
        }

        [Test]
        [Category("MyProjects")]
        public async Task ProjectPage_AfterAddingProject_MyProjectsTableNotEmpty()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

            var row = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(row).ToBeVisibleAsync();
            await Expect(row).ToContainTextAsync(projName);
        }

        [Test]
        [Category("MyProjects")]
        public async Task ProjectPage_EditPageOpen_AfterClickOnProjectInMyProjectTable()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

            var row = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(row).ToBeVisibleAsync();

            await row.ClickAsync();

            await Expect(Page.GetByTestId("proj-details-div")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("details-name-inp")).ToHaveValueAsync(projName);
            await Expect(Page.GetByTestId("details-status-cmb").Locator("p")).ToContainTextAsync("Active");
        }

        [Test]
        [Category("MyProjects")]
        public async Task ProjectPage_ProjectChanged_AfterValidEditData()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

            var row = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(row).ToBeVisibleAsync();

            await row.ClickAsync();

            var newProjName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";
            await Page.GetByTestId("details-name-inp").FillAsync(newProjName);
            await Page.GetByTestId("details-status-cmb").ClickAsync();
            await Page.GetByTestId("details-status-cmb").GetByText("Blocked").ClickAsync();

            await Page.GetByTestId("details-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-details-div")).ToBeHiddenAsync();

            var editedRow = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = newProjName });

            await Expect(editedRow).ToBeVisibleAsync();
            await Expect(editedRow).ToContainTextAsync("Blocked");

            await Page.GetByTestId("myproj-close-btn").ClickAsync();

            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeHiddenAsync();

            var editedRowGlobal = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = newProjName });
            await Expect(editedRowGlobal).ToBeVisibleAsync();
            await Expect(editedRowGlobal).ToContainTextAsync("Blocked");
        }

        [Test]
        [Category("MyProjects")]
        public async Task ProjectPage_AlertPopup_AfterInvalidEditData()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

            var row = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(row).ToBeVisibleAsync();

            await row.ClickAsync();

            await Page.GetByTestId("details-name-inp").FillAsync("");
            await Page.GetByTestId("details-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-details-div")).ToBeVisibleAsync();
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);

        }

        [Test]
        [Category("MyProjects")]
        public async Task ProjectPage_ProjectDeleted_AfterDeleteBtnClick()
        {

            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-myprojects-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeVisibleAsync();

            var row = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(row).ToBeVisibleAsync();

            await row.ClickAsync();

            await Page.GetByTestId("details-delete-btn").ClickAsync();
            await Expect(Page.GetByTestId("proj-details-div")).ToBeHiddenAsync();

            var deletedRow = Page.GetByTestId("myproj-table").Locator("tr", new() { HasText = projName });

            await Expect(deletedRow).ToHaveCountAsync(0);

            await Page.GetByTestId("myproj-close-btn").ClickAsync();

            await Expect(Page.GetByTestId("proj-myprojects-div")).ToBeHiddenAsync();

            var deletedRowGlobal = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Expect(deletedRowGlobal).ToHaveCountAsync(0);

        }


        [Test]
        [Category("SearchProjects")]
        public async Task ProjectPage_TableNotEmpty_OnSearchWithAddedProjectName()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-search-inp").FillAsync(projName);

            var row = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Page.WaitForTimeoutAsync(2000);
            await Expect(row).ToBeVisibleAsync();
        }

        [Test]
        [Category("SearchProjects")]
        public async Task ProjectPage_TableEmpty_OnSearchWithProjectThasIsntAdded()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Page.GetByTestId("proj-search-inp").FillAsync(projName);

            var row = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Page.WaitForTimeoutAsync(2000);

            await Expect(row).ToHaveCountAsync(0);

        }

        [Test]
        [Category("SearchProjects")]
        public async Task ProjectPage_TableNotEmpty_OnPartialSearchProject()
        {
            var projName = $"TestName{Guid.NewGuid().ToString("N")[..8]}";

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
            await AddActiveProject(projName);

            await Page.GetByTestId("proj-search-inp").FillAsync("TestName");

            await Page.WaitForTimeoutAsync(2000);

            var row = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = projName });
            await Expect(row).ToHaveCountAsync(1);

        }

    }
}
