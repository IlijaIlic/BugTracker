using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace bugtracker_back.PlaywrightTests
{
    [TestFixture]
    public class TestBugsPage : PageTest
    {

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

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);
            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
        }

        public async Task ChangeToTester()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            await Page.GetByTestId("foot-logout-btn").ClickAsync();
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/login");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/login");
            await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailTester);
            await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordTester);
            await Page.GetByTestId("log-login-btn").ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);
            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
        }

        public async Task AddLowAndroidBug(string bugName)
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

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
        }


        [Test]
        [Category("BugPage")]
        public async Task BugsPage_AddBugComponentCheck()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-addbug-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeVisibleAsync();

            await Page.GetByTestId("addbug-close-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeHiddenAsync();

        }

        [Test]
        [Category("BugPage")]
        public async Task BugsPage_MyBugsComponentCheck()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();

            await Page.GetByTestId("mybugs-close-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeHiddenAsync();
        }

        [Test]
        [Category("AddBug")]
        public async Task BugsPage_BugSaved_ManagerAddBug()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";

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

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(projectRow).ToHaveCountAsync(1);

            var newBugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var newBugCount = int.Parse(newBugCountText);

            Assert.That(newBugCount, Is.GreaterThan(bugCount));
        }

        [Test]
        [Category("AddBug")]
        public async Task BugsPage_BugSaved_TesterAddBug()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await ChangeToTester();

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";

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
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameTester);

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(projectRow).ToHaveCountAsync(1);

            var newBugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var newBugCount = int.Parse(newBugCountText);

            Assert.That(newBugCount, Is.GreaterThan(bugCount));
        }


        [Test]
        [Category("AddBug")]
        public async Task BugsPage_Fail_WhenNoBugNameIsProvided()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-addbug-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeVisibleAsync();

            await Page.GetByTestId("addbug-platform-cmb").ClickAsync();
            await Page.GetByTestId("addbug-platform-cmb").GetByText("Android").ClickAsync();

            await Page.GetByTestId("addbug-priority-cmb").ClickAsync();
            await Page.GetByTestId("addbug-priority-cmb").GetByText("Low").ClickAsync();

            await Page.GetByTestId("addbug-severity-cmb").ClickAsync();
            await Page.GetByTestId("addbug-severity-cmb").GetByText("Low").ClickAsync();

            await Page.GetByTestId("addbug-description-inp").FillAsync("Description");

            await Page.GetByTestId("addbug-submit-btn").ClickAsync();
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);

        }

        [Test]
        [Category("AddBug")]
        public async Task BugsPage_LowAndroidBugAdded_WithOnlyNamePrivded()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("bug-addbug-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeVisibleAsync();

            await Page.GetByTestId("addbug-name-inp").FillAsync(bugName);

            await Page.GetByTestId("addbug-submit-btn").ClickAsync();

            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Expect(newRow).ToBeVisibleAsync();
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameManager);

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(projectRow).ToHaveCountAsync(1);

            var newBugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var newBugCount = int.Parse(newBugCountText);

            Assert.That(newBugCount, Is.GreaterThan(bugCount));
        }


        [Test]
        [Category("AddBug")]
        public async Task BugsPage_BugSaved_WithImage()
        {

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";

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

            var imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "image.png");
            await Page.Locator("input[type='file']").SetInputFilesAsync(imagePath);

            await Page.GetByTestId("addbug-submit-btn").ClickAsync();

            await Expect(Page.GetByTestId("bug-addbug-div")).ToBeHiddenAsync();

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Expect(newRow).ToBeVisibleAsync();
            await Expect(newRow).ToContainTextAsync(GlobalSetup.TestUserNameManager);

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(projectRow).ToHaveCountAsync(1);

            var newBugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var newBugCount = int.Parse(newBugCountText);

            Assert.That(newBugCount, Is.GreaterThan(bugCount));

            await projectRow.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });
            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();


            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            await Page.GetByTestId("details-image-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-image-div")).ToBeVisibleAsync();

            var img = Page.Locator(".upload-img");
            await Expect(img).ToHaveAttributeAsync("src", new Regex(@"/uploads/.*\.(png|jpg|jpeg)$"));

            var width = await img.EvaluateAsync<int>("img => img.naturalWidth");
            Assert.That(width, Is.GreaterThan(0));

        }

        [Test]
        [Category("MyBugs")]
        public async Task BugsPage_BugAddedToMyBugs_OnValidBugAdd()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();
        }


        [Test]
        [Category("MoreDetails")]
        public async Task BugsPage_OnMyBugClick_DataCheck()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            await Expect(Page.GetByTestId("details-name-inp")).ToHaveValueAsync(bugName);
            await Expect(Page.GetByTestId("details-description-inp")).ToHaveValueAsync("Description");
            await Expect(Page.GetByTestId("details-platform-cmb").Locator("p")).ToContainTextAsync("Android");
            await Expect(Page.GetByTestId("details-severity-cmb").Locator("p")).ToContainTextAsync("Low");
            await Expect(Page.GetByTestId("details-priority-cmb").Locator("p")).ToContainTextAsync("Low");

        }

        [Test]
        [Category("MoreDetails")]
        public async Task BugsPage_EditBugFails_OnNoName()
        {
            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            await Page.GetByTestId("details-name-inp").FillAsync("");
            await Page.GetByTestId("details-submit-btn").ClickAsync();

            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("MoreDetails")]
        public async Task BugsPage_EditBug_WithValidData()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            var newBugName = $"NewBugName{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("details-name-inp").FillAsync(newBugName);
            await Page.GetByTestId("details-platform-cmb").ClickAsync();
            await Page.GetByTestId("details-platform-cmb").GetByText("iOS").ClickAsync();

            await Page.GetByTestId("details-priority-cmb").ClickAsync();
            await Page.GetByTestId("details-priority-cmb").GetByText("High").ClickAsync();

            await Page.GetByTestId("details-severity-cmb").ClickAsync();
            await Page.GetByTestId("details-severity-cmb").GetByText("Critical").ClickAsync();

            await Page.GetByTestId("details-description-inp").FillAsync("new description");


            await Page.GetByTestId("details-image-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-image-div")).ToBeVisibleAsync();

            var imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "image2.jpg");
            await Page.Locator("input[type='file']").SetInputFilesAsync(imagePath);

            await Page.GetByTestId("image-close-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-image-div")).ToBeHiddenAsync();

            await Page.GetByTestId("details-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-details-div")).ToBeHiddenAsync();

            var changed = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = newBugName });
            await Expect(changed).ToBeVisibleAsync();
            await Expect(changed).ToContainTextAsync("iOS");
            await Expect(changed).ToContainTextAsync("Critical");
            await Expect(changed).ToContainTextAsync("High");

            await changed.ClickAsync();
            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            await Page.GetByTestId("details-image-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-image-div")).ToBeVisibleAsync();

            var img = Page.Locator(".upload-img");
            await Expect(img).ToHaveAttributeAsync("src", new Regex(@"/uploads/.*\.(png|jpg|jpeg)$"));

            var width = await img.EvaluateAsync<int>("img => img.naturalWidth");
            Assert.That(width, Is.GreaterThan(0));
        }

        [Test]
        [Category("MoreDetails")]
        public async Task BugsPage_AddFixDate_Valid()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            var futureDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            await Page.GetByTestId("details-fixed-date").FillAsync(futureDate);

            await Page.GetByTestId("details-submit-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-details-div")).ToBeHiddenAsync();

            var changed = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });
            await Expect(changed).ToBeVisibleAsync();
            await Expect(changed).ToContainTextAsync("Fixed");

        }

        [Test]
        [Category("MoreDetails")]
        public async Task BugsPage_AddFixDate_Invalid()
        {

            string? alertMessage = null;
            Page.Dialog += (_, dialog) =>
            {
                alertMessage = dialog.Message;
                dialog.DismissAsync();
            };

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            var pastDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            await Page.GetByTestId("details-fixed-date").FillAsync(pastDate);

            await Page.GetByTestId("details-submit-btn").ClickAsync();

            await Page.WaitForTimeoutAsync(1000);
            Assert.That(alertMessage, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("Search")]
        public async Task BugsPage_NoResult_SearchNoBugAdded()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";

            await Page.GetByTestId("bug-search-inp").FillAsync(bugName);

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Page.WaitForTimeoutAsync(2000);
            await Expect(newRow).ToHaveCountAsync(0);

        }

        [Test]
        [Category("Search")]
        public async Task BugsPage_BugDisplayed_SearchBugAdded()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Page.GetByTestId("bug-search-inp").FillAsync(bugName);

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Page.WaitForTimeoutAsync(2000);
            await Expect(newRow).ToBeVisibleAsync();

        }

        [Test]
        [Category("Search")]
        public async Task BugsPage_BugDisplayed_SearchBugAdded_PartOfName()
        {
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Page.GetByTestId("bug-search-inp").FillAsync("TestName");

            var newRow = Page.GetByTestId("bug-table").Locator("tr", new() { HasText = bugName });

            await Page.WaitForTimeoutAsync(2000);
            await Expect(newRow).ToBeVisibleAsync();

        }

        [Test]
        [Category("Delete")]
        public async Task BugsPage_DeletedBug_Check()
        {
           
            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            var bugName = $"TestNameBug{Guid.NewGuid().ToString("N")[..8]}";
            await AddLowAndroidBug(bugName);

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));
            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            var projectRow = Page.GetByTestId("proj-table").Locator("tr", new() { HasText = GlobalSetup.SharedProjectName });
            await Expect(projectRow).ToHaveCountAsync(1);

            var bugCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var bugCount = int.Parse(bugCountText);

            await projectRow.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex($"^{GlobalSetup.FrontendUrl}/bug"));

            await Page.GetByTestId("bug-mybugs-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            var mybug = Page.GetByTestId("mybugs-table").Locator("tr", new() { HasText = bugName });

            await Expect(mybug).ToBeVisibleAsync();
            await mybug.ClickAsync();

            await Expect(Page.GetByTestId("bug-details-div")).ToBeVisibleAsync();

            await Page.GetByTestId("details-delete-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-details-div")).ToBeHiddenAsync();
            await Expect(mybug).ToHaveCountAsync(0);

            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeVisibleAsync();
            await Page.GetByTestId("mybugs-close-btn").ClickAsync();
            await Expect(Page.GetByTestId("bug-mybugs-div")).ToBeHiddenAsync();

            var mybugGlobal = Page.GetByTestId("bugs-table").Locator("tr", new() { HasText = bugName });
            await Expect(mybugGlobal).ToHaveCountAsync(0);

            await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/projects");
            await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

            await Expect(projectRow).ToHaveCountAsync(1);

            var newCountText = await projectRow.Locator("th").Nth(4).InnerTextAsync();
            var newCount = int.Parse(newCountText);
            Assert.That(newCount, Is.LessThan(bugCount));

        }
    }
}
