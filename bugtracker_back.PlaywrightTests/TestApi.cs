using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace bugtracker_back.PlaywrightTests;

[TestFixture]
public class TestAuthApi : PlaywrightTest
{
    private IAPIRequestContext _request;
    public static string AuthApi = "/api/auth";

    [SetUp]
    public async Task SetUp()
    {
        _request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = GlobalSetup.BackendUrl,
            IgnoreHTTPSErrors = true
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _request.DisposeAsync();
    }

    #region Register

    [Test, Description("/register bi trebalo da vrati Ok(200) kada se registruje Manager sa validnim podacima")]
    [Category("Register")]
    public async Task Register_200_wValidManagerData()
    {
        var response = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"testmanager{Guid.NewGuid()}",
                email = $"testmanager{Guid.NewGuid()}@test.com",
                password = "Password123!",
                role = "Manager"
            }
        });
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test, Description("/register bi trebalo da vrati Ok(200) ako se registruje Tester sa validnim podacima")]
    [Category("Register")]
    public async Task Register_200_wValidTesterData()
    {
        var response = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"testtester{Guid.NewGuid()}",
                email = $"testtester{Guid.NewGuid()}@test.com",
                password = "Password123!",
                role = "Tester"
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    [Category("/register bi trebalo da vrati BadRequest(400) kada se korisnik registruje nevalidnom rolom")]
    public async Task Register_400_wInvalidRole()
    {
        var response = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"test{Guid.NewGuid()}",
                email = $"test{Guid.NewGuid()}@test.com",
                password = "Password123!",
                role = "Invalid"
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }


    [Test, Description("/register bi trebalo da vrati BadRequest(400) kada se korisnik registruje nevalidnom sifrom")]
    [Category("Register")]
    public async Task Register_400_wWeakPassword()
    {
        var response = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"test{Guid.NewGuid()}",
                email = $"test{Guid.NewGuid()}@test.com",
                password = "pass",
                role = "Manager"
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }


    [Test, Description("/register bi trebalo da vrati BadRequest(400) kada se korisnik registruje nevalidnim email-om")]
    [Category("Register")]
    public async Task Register_400_wInvalidEmail()
    {
        var response = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"test{Guid.NewGuid()}",
                email = $"test",
                password = "Password123!",
                role = "Manager"
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }

    [Test, Description("/register bi trebalo da vrati BadRequest(400) kada se korisnik registruje emailom koji je vec u upotrebi")]
    [Category("Register")]
    public async Task Register_400_wUsedEmail()
    {
        var email = $"test{Guid.NewGuid()}@test.com";
        var response1 = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"test{Guid.NewGuid()}",
                email,
                password = "Password123!",
                role = "Manager"
            }
        });

        Assert.That(response1.Status, Is.EqualTo(200));

        var response2 = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"test{Guid.NewGuid()}",
                email,
                password = "Password123!",
                role = "Manager"
            }
        });

        Assert.That(response2.Status, Is.EqualTo(400));
    }

    [Test, Description("/register bi trebalo da vrati BadRequest(400) kada se korisnik registruje usernameom koji je vec u upotrebi")]
    [Category("Register")]
    public async Task Register_400_wUsedUserName()
    {
        var username = $"test{Guid.NewGuid()}";
        var response1 = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username,
                email = $"test{Guid.NewGuid()}@test.com",
                password = "Password123!",
                role = "Manager"
            }
        });

        Assert.That(response1.Status, Is.EqualTo(200));

        var response2 = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username,
                email = $"test{Guid.NewGuid()}@test.com",
                password = "Password123!",
                role = "Manager"
            }
        });

        Assert.That(response2.Status, Is.EqualTo(400));
    }

    #endregion

    #region Login

    [Test, Description("/login bi trebalo da vrati Ok(200) kada se Manager loginuje validnim podacima")]
    [Category("Login")]
    public async Task Login_200_wValidManager()
    {

        var email = $"testmanager{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        var registerResponse = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"testmanager{Guid.NewGuid()}",
                email,
                password,
                role = "Manager"
            }
        });
        Assert.That(registerResponse.Status, Is.EqualTo(200));

        var loginResponse = await _request.PostAsync($"{AuthApi}/login", new()
        {
            DataObject = new
            {
                email,
                password

            }
        });

        Assert.That(loginResponse.Status, Is.EqualTo(200));

        var body = await loginResponse.JsonAsync();
        Assert.That(body!.Value.GetProperty("role").GetString(), Is.EqualTo("Manager"));
    }

    [Test, Description("/login bi trebalo da vrati Ok(200) kada se Tester loginuje validnim podacima")]
    [Category("Login")]
    public async Task Login_200_wValidTester()
    {
        var email = $"testtester{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        var registerResponse = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"testtester{Guid.NewGuid()}",
                email,
                password,
                role = "Tester"
            }
        });
        Assert.That(registerResponse.Status, Is.EqualTo(200));

        var loginResponse = await _request.PostAsync($"{AuthApi}/login", new()
        {
            DataObject = new
            {
                email,
                password

            }
        });

        Assert.That(loginResponse.Status, Is.EqualTo(200));
        var body = await loginResponse.JsonAsync();
        Assert.That(body!.Value.GetProperty("role").GetString(), Is.EqualTo("Tester"));
    }

    [Test, Description("/login bi trebalo da vrati Unauthorized(401) kada se korisnik loginuje pogresnom sifrom")]
    [Category("Login")]
    public async Task Login_401_wInvalidPassword()
    {
        var email = $"testtester{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        var registerResponse = await _request.PostAsync($"{AuthApi}/register", new()
        {
            DataObject = new
            {
                username = $"testtester{Guid.NewGuid()}",
                email,
                password,
                role = "Tester"
            }
        });
        Assert.That(registerResponse.Status, Is.EqualTo(200));

        var loginResponse = await _request.PostAsync($"{AuthApi}/login", new()
        {
            DataObject = new
            {
                email,
                password = "Invalid"

            }
        });

        Assert.That(loginResponse.Status, Is.EqualTo(401));
    }

    [Test, Description("/login bi trebalo da vrati Unauthorized(401) kada se korisnik loginuje bez prethodno kreiranog naloga")]
    [Category("Login")]
    public async Task Login_401_wNoUser()
    {
        var email = $"testtester{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        var loginResponse = await _request.PostAsync($"{AuthApi}/login", new()
        {
            DataObject = new
            {
                email,
                password

            }
        });

        Assert.That(loginResponse.Status, Is.EqualTo(401));
    }

    #endregion
}

[TestFixture]
public class TestUserApi : PlaywrightTest
{
    private IAPIRequestContext _request;
    public static string UserApi = "/api/user";

    [SetUp]
    public async Task SetUp()
    {
        _request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = GlobalSetup.BackendUrl,
            IgnoreHTTPSErrors = true
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _request.DisposeAsync();
    }

    public async Task<string> GetToken(string email, string username, string role)
    {
        var password = "Password123!";

        await _request.PostAsync($"{TestAuthApi.AuthApi}/register", new()
        {
            DataObject = new
            {
                username,
                email,
                password,
                role
            }
        });

        var loginResponse = await _request.PostAsync($"{TestAuthApi.AuthApi}/login", new()
        {
            DataObject = new { email, password }
        });

        var jsonResponse = await loginResponse.JsonAsync();
        return jsonResponse!.Value.GetProperty("token").GetString()!;
    }

    #region Get

    [Test]
    [Category("Get")]
    public async Task UserGet_200_wManagerRegistered()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");
        var response = await _request.GetAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(response.Status, Is.EqualTo(200));

        var json = await response.JsonAsync();
        Assert.That(json!.Value.GetProperty("role").ToString(), Is.EqualTo("Manager"));
    }

    [Test]
    [Category("Get")]
    public async Task UserGet_200_wTesterRegistered()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Tester");
        var response = await _request.GetAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(response.Status, Is.EqualTo(200));

        var json = await response.JsonAsync();
        Assert.That(json!.Value.GetProperty("role").ToString(), Is.EqualTo("Tester"));
    }

    [Test]
    [Category("Get")]
    public async Task UserGet_401_wNoToken()
    {
        var response = await _request.GetAsync($"{UserApi}/me");

        Assert.That(response.Status, Is.EqualTo(401));
    }

    [Test]
    [Category("Get")]
    public async Task UserGet_401_wInvalidToken()
    {

        var response = await _request.GetAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer tokentokentoken" }
        });

        Assert.That(response.Status, Is.EqualTo(401));

    }

    #endregion

    #region Put

    [Test]
    [Category("Put")]
    public async Task UserPut_200_wTokenAndValidData()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var newUsername = $"user{Guid.NewGuid()}";
        var newEmail = $"{Guid.NewGuid()}@test.com";

        var response = await _request.PutAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new
            {
                username = newUsername,
                email = newEmail
            }
        });

        Assert.That(response.Status, Is.EqualTo(200));

        var getResponse = await _request.GetAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        var json = await getResponse.JsonAsync();
        Assert.That(json!.Value.GetProperty("username").ToString(), Is.EqualTo(newUsername));
        Assert.That(json!.Value.GetProperty("email").ToString(), Is.EqualTo(newEmail));
    }

    [Test]
    [Category("Put")]
    public async Task UserPut_400_wTokenAndUsedUsername()
    {
        var email1 = $"{Guid.NewGuid()}@test.com";
        var username1 = $"testuser{Guid.NewGuid()}";

        var email2 = $"{Guid.NewGuid()}@test.com";
        var username2 = $"testuser{Guid.NewGuid()}";

        await GetToken(email1, username1, "Manager");
        var token = await GetToken(email2, username2, "Manager");

        var newEmail = $"{Guid.NewGuid()}@test.com";

        var response = await _request.PutAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new
            {
                username = username1,
                email = newEmail
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }

    [Test]
    [Category("Put")]
    public async Task UserPut_400_wTokenAndUsedEmail()
    {
        var email1 = $"{Guid.NewGuid()}@test.com";
        var username1 = $"testuser{Guid.NewGuid()}";

        var email2 = $"{Guid.NewGuid()}@test.com";
        var username2 = $"testuser{Guid.NewGuid()}";

        await GetToken(email1, username1, "Manager");
        var token = await GetToken(email2, username2, "Manager");

        var newUsername = $"testuser{Guid.NewGuid()}";

        var response = await _request.PutAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new
            {
                username = newUsername,
                email = email1
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }


    [Test]
    [Category("Put")]
    public async Task UserPut_400_wTokenAndInvalidEmail()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var newUsername = $"testuser{Guid.NewGuid()}";

        var response = await _request.PutAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new
            {
                username = newUsername,
                email = $"{Guid.NewGuid()}"
            }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }
    [Test]
    [Category("Put")]
    public async Task UserPut_401_wNoTokenAndValidData()
    {
        var newUsername = $"user{Guid.NewGuid()}";
        var newEmail = $"{Guid.NewGuid()}@test.com";

        var response = await _request.PutAsync($"{UserApi}/me", new()
        {
            DataObject = new
            {
                username = newUsername,
                email = newEmail
            }
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    #endregion

    #region Delete

    [Test]
    [Category("Delete")]
    public async Task UserDelete_200_wToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var response = await _request.DeleteAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },

        });

        Assert.That(response.Status, Is.EqualTo(200));

        var getResponse = await _request.GetAsync($"{UserApi}/me", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(getResponse.Status, Is.EqualTo(404));
    }

    [Test]
    [Category("Delete")]
    public async Task UserDelete_401_wNoToken()
    {
        var response = await _request.DeleteAsync($"{UserApi}/me", new()
        {});

        Assert.That(response.Status, Is.EqualTo(401));
    }

    #endregion
}

[TestFixture]
public class TestProjectApi : PlaywrightTest
{

}

[TestFixture]
public class TestBugApi : PlaywrightTest
{

}