using Microsoft.Playwright.NUnit;

namespace bugtracker_back.PlaywrightTests;

[TestFixture]
public class TestLandingPage : PageTest
{

    [Test]
    [Category("LandingPage")]
    public async Task LandingPage_OnLoginClick_NavigatesToLoginPage()
    {
        await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/");
        await Page.GetByTestId("land-login").ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/login");
    }

    [Test]
    [Category("LandingPage")]
    public async Task LandingPage_OnRegisterClick_NavigatesToRegisterPage()
    {
        await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/");
        await Page.GetByTestId("land-register").ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/register");
    }

    [Test]
    [Category("LandingPage")]
    public async Task LandingPage_UserLoggedIn_RedirectedToProjects()
    {
        await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/");
        await Page.GetByTestId("land-login").ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/login");

        await Page.GetByTestId("log-email-inp").FillAsync(GlobalSetup.TestUserEmailManager);
        await Page.GetByTestId("log-password-inp").FillAsync(GlobalSetup.TestUserPasswordManager);
        await Page.GetByTestId("log-login-btn").ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");

        await Page.GotoAsync($"{GlobalSetup.FrontendUrl}/");

        await Expect(Page).ToHaveURLAsync($"{GlobalSetup.FrontendUrl}/projects");
    }
}
