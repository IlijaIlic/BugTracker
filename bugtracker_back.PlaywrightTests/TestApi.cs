using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace bugtracker_back.PlaywrightTests;

[TestFixture]
public class ApiAuthTest : PlaywrightTest
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
public class ApiUserTest : PlaywrightTest
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

        await _request.PostAsync($"{ApiAuthTest.AuthApi}/register", new()
        {
            DataObject = new
            {
                username,
                email,
                password,
                role
            }
        });

        var loginResponse = await _request.PostAsync($"{ApiAuthTest.AuthApi}/login", new()
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
        { });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    #endregion
}

[TestFixture]
public class ApiProjectTest : PlaywrightTest
{
    private IAPIRequestContext _request;
    public static string ProjectApi = "/api/project";

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

        await _request.PostAsync($"{ApiAuthTest.AuthApi}/register", new()
        {
            DataObject = new
            {
                username,
                email,
                password,
                role
            }
        });

        var loginResponse = await _request.PostAsync($"{ApiAuthTest.AuthApi}/login", new()
        {
            DataObject = new { email, password }
        });

        var jsonResponse = await loginResponse.JsonAsync();
        return jsonResponse!.Value.GetProperty("token").GetString()!;
    }

    #region GetAll

    [Test, Description("")]
    [Category("GetAll")]
    public async Task ProjectGetAll_200_wValidToken()
    {

        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var projectName = $"Project{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = projectName, status = "Planning" }
        });

        Assert.That(response.Status, Is.EqualTo(201));

        var getResponse = await _request.GetAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(getResponse.Status, Is.EqualTo(200));

        var json = await getResponse.JsonAsync();
        var projects = json!.Value.EnumerateArray().ToList();

        Assert.That(projects.Any(p => p.GetProperty("name").GetString() == projectName), Is.True);
    }

    [Test, Description("")]
    [Category("GetAll")]
    public async Task ProjectGetAll_200_wValidTokenAndSearching()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var matchingName = $"ABC{Guid.NewGuid().ToString("N")[..8]}";
        var nonMatchingName = $"DEF{Guid.NewGuid()}";

        await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = matchingName, status = "Planning" }
        });

        await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = nonMatchingName, status = "Planning" }
        });

        var getResponse = await _request.GetAsync($"{ProjectApi}?search={matchingName}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        var json = await getResponse.JsonAsync();
        var projects = json!.Value.EnumerateArray().ToList();

        Assert.That(projects.All(p => p.GetProperty("name").GetString()!.Contains(matchingName)), Is.True);
        Assert.That(projects.Any(p => p.GetProperty("name").GetString() == nonMatchingName), Is.False);
    }



    #endregion

    #region GetById

    [Test, Description("")]
    [Category("GetById")]
    public async Task ProjectGetById_200_wValidData()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var projectName = $"Test{Guid.NewGuid()}";
        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = projectName, status = "Active", description = "MyDescription" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var getResponse = await _request.GetAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        Assert.That(getResponse.Status, Is.EqualTo(200));

        var jsonGet = await getResponse.JsonAsync();
        Assert.That(jsonGet!.Value.GetProperty("name").GetString(), Is.EqualTo(projectName));
        Assert.That(jsonGet.Value.GetProperty("status").GetString(), Is.EqualTo("Active"));
        Assert.That(jsonGet.Value.GetProperty("description").GetString(), Is.EqualTo("MyDescription"));
    }

    [Test, Description("")]
    [Category("GetById")]
    public async Task ProjectGetById_404_NonExistentProject()
    {

        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var response = await _request.GetAsync($"{ProjectApi}/123123", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test, Description("")]
    [Category("GetById")]
    public async Task ProjectGetById_401_wNoToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var token = await GetToken(email, username, "Manager");

        var projectName = $"Test{Guid.NewGuid()}";
        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = projectName, status = "Active", description = "MyDescription" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var getResponse = await _request.GetAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer notoken" },
        });

        Assert.That(getResponse.Status, Is.EqualTo(401));
    }

    #endregion

    #region GetMyProjects

    [Test, Description("")]
    [Category("GetMyProjects")]
    public async Task ProjectGetMyProjects_200_wValidManagerToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var email2 = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var username2 = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");
        var token2 = await GetToken(email2, username2, "Manager");


        var projectName = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = projectName, status = "Planning" }
        });

        var responseUser2 = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            DataObject = new { name = projectName2, status = "Planning" }
        });


        var getResponse = await _request.GetAsync($"{ProjectApi}/my", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(getResponse.Status, Is.EqualTo(200));

        var json = await getResponse.JsonAsync();
        var projects = json!.Value.EnumerateArray().ToList();

        Assert.That(projects, Has.Count.EqualTo(1));
        Assert.That(projects[0].GetProperty("name").GetString(), Is.EqualTo(projectName));

    }

    [Test, Description("")]
    [Category("GetMyProjects")]
    public async Task ProjectGetMyProjects_200_wValidManagerTokenNoProjects()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var getResponse = await _request.GetAsync($"{ProjectApi}/my", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
        });

        Assert.That(getResponse.Status, Is.EqualTo(200));

        var json = await getResponse.JsonAsync();
        var projects = json!.Value.EnumerateArray().ToList();

        Assert.That(projects, Has.Count.EqualTo(0));
    }

    [Test, Description("")]
    [Category("GetMyProjects")]
    public async Task ProjectGetMyProjects_401_wNoToken()
    {
        var getResponse = await _request.GetAsync($"{ProjectApi}/my", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer notoken" }
        });

        Assert.That(getResponse.Status, Is.EqualTo(401));
    }


    #endregion

    #region Create

    [Test, Description("")]
    [Category("Create")]
    public async Task ProjectCreate_201_wValidManagerToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        Assert.That(response.Status, Is.EqualTo(201));

    }

    [Test, Description("")]
    [Category("Create")]
    public async Task ProjectCreate_403_wValidTesterToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Tester");

        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        Assert.That(response.Status, Is.EqualTo(403));
    }

    [Test, Description("")]
    [Category("Create")]
    public async Task ProjectCreate_401_wInvalidToken()
    {
        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer asdfasdf" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    [Test, Description("")]
    [Category("Create")]
    public async Task ProjectCreate_400_wValidManagerTokenAndInvalidStatus()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var response = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Project", status = "asdf" }
        });

        Assert.That(response.Status, Is.EqualTo(400));
    }

    #endregion

    #region Update

    [Test, Description("")]
    [Category("Update")]
    public async Task ProjectUpdate_204_SelfwValidToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");


        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var putResponse = await _request.PutAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Changed", status = "Active" }
        });

        Assert.That(putResponse.Status, Is.EqualTo(204));

        var getResponse = await _request.GetAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        var jsonGet = await getResponse.JsonAsync();
        var name = jsonGet!.Value.GetProperty("name").ToString();
        var status = jsonGet!.Value.GetProperty("status").ToString();

        Assert.That(name, Is.EqualTo("Changed"));
        Assert.That(status, Is.EqualTo("Active"));
    }

    [Test, Description("")]
    [Category("Update")]
    public async Task ProjectUpdate_403_AnotherwValidToken()
    {
        var email1 = $"{Guid.NewGuid()}@test.com";
        var email2 = $"{Guid.NewGuid()}@test.com";
        var username1 = $"testuser{Guid.NewGuid()}";
        var username2 = $"testuser{Guid.NewGuid()}";

        var tokenUser1 = await GetToken(email1, username1, "Manager");
        var tokenUser2 = await GetToken(email2, username2, "Manager");

        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {tokenUser1}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var putResponse = await _request.PutAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {tokenUser2}" },
            DataObject = new { name = "Changed", status = "Active" }
        });

        Assert.That(putResponse.Status, Is.EqualTo(403));
    }

    [Test, Description("")]
    [Category("Update")]
    public async Task ProjectUpdate_404_ValidTokenNoProject()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var putResponse = await _request.PutAsync($"{ProjectApi}/123123", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Changed", status = "Active" }
        });

        Assert.That(putResponse.Status, Is.EqualTo(404));

    }
    #endregion

    #region Delete


    [Test, Description("")]
    [Category("Delete")]
    public async Task ProjectDelete_204_ValidToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var deleteResponse = await _request.DeleteAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var getResponse = await _request.GetAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        Assert.That(getResponse.Status, Is.EqualTo(404));
    }

    [Test, Description("")]
    [Category("Delete")]
    public async Task ProjectDelete_403_AnotherwValidToken()
    {
        var email1 = $"{Guid.NewGuid()}@test.com";
        var email2 = $"{Guid.NewGuid()}@test.com";
        var username1 = $"testuser{Guid.NewGuid()}";
        var username2 = $"testuser{Guid.NewGuid()}";

        var tokenUser1 = await GetToken(email1, username1, "Manager");
        var tokenUser2 = await GetToken(email2, username2, "Manager");

        var postResponse = await _request.PostAsync(ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {tokenUser1}" },
            DataObject = new { name = "Project", status = "Planning" }
        });

        var jsonPost = await postResponse.JsonAsync();
        var id = jsonPost!.Value.GetProperty("id");

        var deleteResponse = await _request.DeleteAsync($"{ProjectApi}/{id}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {tokenUser2}" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(403));
    }

    [Test, Description("")]
    [Category("Delete")]
    public async Task ProjectDelete_404_ValidTokenNoProject()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";

        var token = await GetToken(email, username, "Manager");

        var deleteResponse = await _request.DeleteAsync($"{ProjectApi}/123123", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(404));
    }

    #endregion
}

[TestFixture]
public class ApiBugTest : PlaywrightTest
{

    private IAPIRequestContext _request;
    public static string BugApi = "/api/bug";

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

    public async Task<string> GetToken(string role)
    {

        var email = $"{Guid.NewGuid()}@test.com";
        var username = $"testuser{Guid.NewGuid()}";
        var password = "Password123!";

        await _request.PostAsync($"{ApiAuthTest.AuthApi}/register", new()
        {
            DataObject = new { username, email, password, role }
        });

        var loginResponse = await _request.PostAsync($"{ApiAuthTest.AuthApi}/login", new()
        {
            DataObject = new { email, password }
        });

        var jsonResponse = await loginResponse.JsonAsync();
        return jsonResponse!.Value.GetProperty("token").GetString()!;
    }

    public async Task<int> CreateProject(string name, string token)
    {
        var response = await _request.PostAsync(ApiProjectTest.ProjectApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
            DataObject = new { name, status = "Planning" }
        });

        var json = await response.JsonAsync();
        return json!.Value.GetProperty("id").GetInt32();
    }

    private IFormData CreateBugFormData(string name, string platform, string priority, string severity, int projectId, string? dateFixed = null)
    {
        var form = _request.CreateFormData();
        form.Set("Name", name);
        form.Set("Platform", platform);
        form.Set("Priority", priority);
        form.Set("Severity", severity);
        form.Set("ProjectId", projectId.ToString());
        if (dateFixed != null) form.Set("DateFixed", dateFixed);
        return form;
    }

    #region GetAll

    [Test, Description("")]
    [Category("GetAll")]
    public async Task BugGetAll_200_ManagerWValidTokenAndDifferentUsers()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Tester");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var response = await _request.GetAsync($"{BugApi}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.GreaterThanOrEqualTo(2));
    }

    [Test, Description("")]
    [Category("GetAll")]
    public async Task BugGetAll_200_TesterWValidTokenAndDifferentUsers()
    {
        var token1 = await GetToken("Tester");
        var token2 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token2);

        var bugName = $"Bug{Guid.NewGuid()}";

        var createResponse = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData("BugName", "Web", "Low", "Low", project1)
        });

        var response = await _request.GetAsync($"{BugApi}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(bugs.Any(b => b.GetProperty("name").GetString() == "BugName"), Is.True);

    }


    [Test, Description("")]
    [Category("GetAll")]
    public async Task BugGetAll_401_wInvalidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var response = await _request.GetAsync($"{BugApi}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer asdf" },
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    #endregion

    #region GetById

    [Test, Description("")]
    [Category("GetById")]
    public async Task BugGetById_200_wValidTokenManager()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Tester");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var response = await _request.GetAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });


        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugName = json!.Value.GetProperty("name").ToString();

        Assert.That(bugName, Is.EqualTo(bugName1));
    }

    [Test, Description("")]
    [Category("GetById")]
    public async Task BugGetById_200_wValidTokenTester()
    {
        var token1 = await GetToken("Tester");
        var token2 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token2);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var response = await _request.GetAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
        });



        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugName = json!.Value.GetProperty("name").ToString();

        Assert.That(bugName, Is.EqualTo(bugName1));
    }

    [Test, Description("")]
    [Category("GetById")]
    public async Task BugGetById_401_wInvalidToken()
    {
        var token1 = await GetToken("Manager");

        var projectName1 = $"Project{Guid.NewGuid()}";
        var project1 = await CreateProject(projectName1, token1);
        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var response = await _request.GetAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer asdf" },
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    #endregion

    #region GetByProject

    [Test, Description("")]
    [Category("GetByProject")]
    public async Task BugGetByProject_401_wNoValidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var response = await _request.GetAsync($"{BugApi}/project/{project1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer asdf" },
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    [Test, Description("")]
    [Category("GetByProject")]
    public async Task BugGetByProject_200_wValidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var response = await _request.GetAsync($"{BugApi}/project/{project1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.EqualTo(1));
        Assert.That(bugs[0].GetProperty("name").ToString(), Is.EqualTo(bugName1));
    }

    [Test, Description("")]
    [Category("GetByProject")]
    public async Task BugGetByProject_200_wValidTokenAndSearch()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createResponse3 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var response = await _request.GetAsync($"{BugApi}/project/{project1}?search={bugName1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.EqualTo(1));
        Assert.That(bugs[0].GetProperty("name").ToString(), Is.EqualTo(bugName1));
    }

    #endregion

    #region GetMyBugsByProject

    [Test, Description("")]
    [Category("GetMyBugsByProject")]
    public async Task BugGetMyBugsByProject_200_wValidToken()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createResponse3 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var response = await _request.GetAsync($"{BugApi}/project/my/{project1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.EqualTo(1));
        Assert.That(bugs[0].GetProperty("name").ToString(), Is.EqualTo(bugName1));
    }

    [Test, Description("")]
    [Category("GetMyBugsByProject")]
    public async Task BugGetMyBugsByProject_401_wInvalidToken()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createResponse3 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var response = await _request.GetAsync($"{BugApi}/project/my/{project1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer asdf" },
        });

        Assert.That(response.Status, Is.EqualTo(401));
    }

    [Test, Description("")]
    [Category("GetMyBugsByProject")]
    public async Task BugGetMyBugsByProject_200_wValidTokenNoBugsSubmitted()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";
        var projectName2 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);
        var project2 = await CreateProject(projectName2, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createResponse2 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        var createResponse3 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project2)
        });

        var response = await _request.GetAsync($"{BugApi}/project/my/{project1}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        var bugs = json!.Value.EnumerateArray().ToList();

        Assert.That(bugs, Has.Count.EqualTo(0));
    }

    #endregion

    #region Create

    [Test, Description("")]
    [Category("Create")]
    public async Task BugCreate_201_WValidManagerToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        Assert.That(createResponse1.Status, Is.EqualTo(201));
    }

    [Test, Description("")]
    [Category("Create")]
    public async Task BugCreate_201_WValidTesterToken()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Tester");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });


        Assert.That(createResponse1.Status, Is.EqualTo(201));
    }


    [Test, Description("")]
    [Category("Create")]
    public async Task BugCreate_401_wNoValidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer asdf" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        Assert.That(createResponse1.Status, Is.EqualTo(401));
    }

    #endregion

    #region Update

    [Test, Description("")]
    [Category("Update")]
    public async Task BugUpdate_204_wValidTokenSelf()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var updateResponse = await _request.PutAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        Assert.That(updateResponse.Status, Is.EqualTo(204));

        var response = await _request.GetAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        var json = await response.JsonAsync();
        var bugName = json!.Value.GetProperty("name").ToString();

        Assert.That(bugName, Is.EqualTo(bugName2));
    }

    [Test, Description("")]
    [Category("Update")]
    public async Task BugUpdate_204_wValidTokenOther()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Tester");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var updateResponse = await _request.PutAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
            Multipart = CreateBugFormData(bugName2, "Web", "Low", "Low", project1)
        });

        Assert.That(updateResponse.Status, Is.EqualTo(204));

        var response = await _request.GetAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token2}" },
        });

        var json = await response.JsonAsync();
        var bugName = json!.Value.GetProperty("name").ToString();

        Assert.That(bugName, Is.EqualTo(bugName2));
    }

    [Test, Description("")]
    [Category("Update")]
    public async Task BugUpdate_401_wNoValidToken()
    {
        var token1 = await GetToken("Manager");
        var token2 = await GetToken("Tester");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";
        var bugName2 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var updateResponse = await _request.PutAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer asdf" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        Assert.That(updateResponse.Status, Is.EqualTo(401));
    }

    #endregion

    #region Delete

    [Test, Description("")]
    [Category("Delete")]
    public async Task BugDelete_204_wValidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var deleteResponse = await _request.DeleteAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(204));
    }

    [Test, Description("")]
    [Category("Delete")]
    public async Task BugDelete_401_wInvalidToken()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var deleteResponse = await _request.DeleteAsync($"{BugApi}/{bugId}", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer asdf" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(401));
    }

    [Test, Description("")]
    [Category("Delete")]
    public async Task BugDelete_404_wValidTokenBadBugId()
    {
        var token1 = await GetToken("Manager");
        var projectName1 = $"Project{Guid.NewGuid()}";

        var project1 = await CreateProject(projectName1, token1);

        var bugName1 = $"Bug{Guid.NewGuid()}";

        var createResponse1 = await _request.PostAsync(BugApi, new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
            Multipart = CreateBugFormData(bugName1, "Web", "Low", "Low", project1)
        });

        var createJson = await createResponse1.JsonAsync();
        var bugId = createJson!.Value.GetProperty("id").GetInt32();

        var deleteResponse = await _request.DeleteAsync($"{BugApi}/123123", new()
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token1}" },
        });

        Assert.That(deleteResponse.Status, Is.EqualTo(404));
    }

    #endregion
}