using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace bugtracker_back.PlaywrightTests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        public const string TestUserEmailManager = "test.manager@test.com";
        public const string TestUserPasswordManager = "Password123!";
        public const string TestUserNameManager = "test-user";

        public const string TestUserNameTester = "demo_tester";
        public const string TestUserEmailTester = "demo.tester@test.com";
        public const string TestUserPasswordTester = "Password123!";


        public const string SharedProjectName = "SharedTestProject_DoNotDelete";
        public static int SharedProjectId { get; private set; }

        public const string FrontendUrl = "http://localhost:4200";
        public const string BackendUrl = "https://localhost:7236";


        [OneTimeSetUp]
        public async Task CreateTestUsersAndSharedProject()
        {
            using var playwright = await Playwright.CreateAsync();
            var request = await playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = BackendUrl,
                IgnoreHTTPSErrors = true
            });

            var managerResponse = await request.PostAsync("/api/auth/register", new()
            {
                DataObject = new
                {
                    username = TestUserNameManager,
                    email = TestUserEmailManager,
                    password = TestUserPasswordManager,
                    role = "Manager"
                }
            });
            Console.WriteLine($"Manager seed status: {managerResponse.Status}");

            var testerResponse = await request.PostAsync("/api/auth/register", new()
            {
                DataObject = new
                {
                    username = TestUserNameTester,
                    email = TestUserEmailTester,
                    password = TestUserPasswordTester,
                    role = "Tester"
                }
            });
            Console.WriteLine($"Tester seed status: {testerResponse.Status}");

            var loginResponse = await request.PostAsync("/api/auth/login", new()
            {
                DataObject = new { email = TestUserEmailManager, password = TestUserPasswordManager }
            });
            var loginBody = await loginResponse.JsonAsync();
            var token = loginBody!.Value.GetProperty("token").GetString();

            var allProjectsResponse = await request.GetAsync($"/api/project?search={SharedProjectName}", new()
            {
                Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
            });
            var projects = await allProjectsResponse.JsonAsync();
            var existing = projects!.Value.EnumerateArray()
                .FirstOrDefault(p => p.GetProperty("name").GetString() == SharedProjectName);

            if (existing.ValueKind != System.Text.Json.JsonValueKind.Undefined)
            {
                SharedProjectId = existing.GetProperty("id").GetInt32();
                Console.WriteLine($"Reusing existing shared project, Id={SharedProjectId}");
            }
            else
            {
                var createResponse = await request.PostAsync("/api/project", new()
                {
                    Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
                    DataObject = new { name = SharedProjectName, description = "Shared project for bug tests", status = "Active" }
                });
                var createBody = await createResponse.JsonAsync();
                SharedProjectId = createBody!.Value.GetProperty("id").GetInt32();
                Console.WriteLine($"Created new shared project, Id={SharedProjectId}");
            }

            await request.DisposeAsync();
        }
    }
}
