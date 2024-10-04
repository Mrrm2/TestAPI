# TUTORIAL: SET UP MICROSOFT TEST FRAMEWORK ENVIRONMENT

## Note: If you get an error, you might have to delete the Data/Migrations Folder and Sample.db
## And try doing 

```sh
dotnet ef migrations add InitializeDb -o Data/Migrations
dotnet ef database update
```

1. Once Creating a project with controllers and the necessary data, use the attributes 
```[CheckValid<role>]``` and ```[InterceptOutbound]``` as the class's or methods attribute.

2. Then set up a dotnet Test Frame work

```sh
dotnet new mstest -o your_test_folder_here
```

3. In your csproj file, it should contain these packages, different versions but the modules should be there

```csharp
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
    <PackageReference Include="MSTest.TestFramework" Version="3.1.1" />
```

4. In your test class program, rename it whatever you like, but copy the contents to ensure the testing process. Comment + Uncomment the code in the registry async because duplication of regristry would break the code.

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace TestingIdempontency; // you may also change the namespace

// Data class used to mock data testing
struct MockUsers
{
    public string email { get; set; }
    public string password { get; set; }
}


/// <summary>
/// Tests the idempotency from the TestAPI Repo
/// Note: I dont actually modify the contents of the orders / products
/// I only check if the key is the same whenever a request is made
/// </summary>
[TestClass]
public class IdempotencyTest // YOU MAY CHANGE THE CLASS NAME
{
    // The HttpClient allows to make requests to the server based on the url
    private HttpClient? _client;

    // The url of the server
    private string? _url;

    // YOUR PORT HERE
    private const string port = "5241"; // CHANGE PORT HERE!
    
    // The urls to be tested, add as many testing urls if you want to extend the testing procedures
    private readonly string[] myUrls = 
    {
        "/register",
        "/login",
        "/api/Products"
    };

    // Mock Data servers to be registered and used
    private readonly MockUsers[] mockUsers = 
    {
        new MockUsers { email = "xaxid@gmail.com", password = "123456" },
        new MockUsers { email = "tvsdf@gmail.com", password = "P@ssw0rd" },
        new MockUsers { email = "wqeefq@gmail.com", password = "rwarwadawdas" },
        new MockUsers { email = "rwqfew@gmail.com", password = "bingchils" }
    };

    // Set up the client and url
    [TestInitialize]
    public void SetUp()
    {
        _client = new HttpClient(); // create static client
        _url = $"http://localhost:{port}";
    }

    // Called only if the mock data using has been registered
    // only for developers, just comment once it is done registering
    // TODO: COMMENT ONCE DONE REGISTERY, IF NOT THEN THIS WILL ALWAYS BREAK WHEN YOU RUN THE TEST
    [TestMethod]
    public async Task RegisterAsync()
    {
        // ARRANGE
        int i = 1; // pick a user
        var resource = new { email = mockUsers[i].email, password = mockUsers[i].password };
        var json = JsonConvert.SerializeObject(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // ACT
        var postResponse = await _client.PostAsync(_url + myUrls[0], content);
        postResponse.EnsureSuccessStatusCode();
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();
        // Get response context
        dynamic response = JsonConvert.DeserializeObject(postResponseBody);
        Assert.AreEqual(mockUsers[i].email, response!.username.ToString());
    
    }

    // Tests a failure scenario where the token is invalid
    [TestMethod]
    public async Task InvalidAuthToken()
    {
        // Arrange
        // Set a random Auth Access Token
        _client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Sample Token");

        // ACT
        var response = await _client.GetAsync(_url + myUrls[2]);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    // Tests the login scenario
    [TestMethod]
    public async Task loginAsync()
    {
        // Arrange
        var resource = new { email = mockUsers[0].email, password = mockUsers[0].password };
        var json = JsonConvert.SerializeObject(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // ACT
        // Attempt to login
        var postResponse = await _client!.PostAsync(_url + myUrls[1], content);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);       
    }


    // Tests the scenario where the access token is missing
    [TestMethod]
    public async Task MissingAccessToken()
    {
        // NO ARRANGE
        // Dont put any access token on header

        // ACT
        var response = await _client!.GetAsync(_url + myUrls[2]);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Tests the scenario where the idempotency key is missing
    [TestMethod]
    public async Task MissingIdempotencyKey()
    {
        // Arrange
        var resource = new { email = mockUsers[0].email, password = mockUsers[0].password };
        var json = JsonConvert.SerializeObject(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        // Login First to get the token
        var postResponse = await _client!.PostAsync(_url + myUrls[1], content);
        postResponse.EnsureSuccessStatusCode();
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();        
        dynamic? response = JsonConvert.DeserializeObject(postResponseBody);        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.token.ToString());
        
        // Now remove the Idempontency Key header
        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
        
        // ACT
        response = await _client.GetAsync(_url + myUrls[2]);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Tests the scenario where the idempotency key is duplicated
    [TestMethod]
    public async Task DuplicateIdempotencyKey()
    {
        // Arrange
        var resource = new { email = mockUsers[0].email, password = mockUsers[0].password };
        var json = JsonConvert.SerializeObject(resource);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        // Login First
        var postResponse = await _client!.PostAsync(_url + myUrls[1], content);
        postResponse.EnsureSuccessStatusCode();
        var postResponseBody = await postResponse.Content.ReadAsStringAsync();        
        dynamic? response = JsonConvert.DeserializeObject(postResponseBody);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.token.ToString());

        // Add a sample key
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "Key");
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // ACT
        response = await _client.GetAsync(_url + myUrls[2]); // call first and process
        response = await _client.GetAsync(_url + myUrls[2]); // call second and try to process with same key

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Tests the scenario where the idempotency key is the same but the user is different
    [TestMethod]
    public async Task SameIdempotencyKeyDifferentUsers()
    {
        // change upon processed already to prevent the same duplicate issue
        string IdempotencyKey = "Key1";
        {
            // User 1

            // Arrange --> Login --> Put Auth token then put key in the header
            var resource = new { email = mockUsers[0].email, password = mockUsers[0].password };
            var json = JsonConvert.SerializeObject(resource);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var postResponse = await _client!.PostAsync(_url + myUrls[1], content);
            postResponse.EnsureSuccessStatusCode();
            var postResponseBody = await postResponse.Content.ReadAsStringAsync();        
            dynamic? response = JsonConvert.DeserializeObject(postResponseBody);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.token.ToString());
            _client.DefaultRequestHeaders.Add("Idempotency-Key", IdempotencyKey);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // ACT
            await _client.GetAsync(_url + myUrls[2]);
        }

        {
            // User 2

            // Arrange --> Login --> Put Auth token then put key in the header
            var resource = new { email = mockUsers[1].email, password = mockUsers[1].password };
            var json = JsonConvert.SerializeObject(resource);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync(_url + myUrls[1], content);
            postResponse.EnsureSuccessStatusCode();
            var postResponseBody = await postResponse.Content.ReadAsStringAsync();        
            dynamic? response = JsonConvert.DeserializeObject(postResponseBody);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response!.token.ToString());

            // Put same key into this user
            _client.DefaultRequestHeaders.Add("Idempotency-Key", IdempotencyKey);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // ACT
            response = await _client.GetAsync(_url + myUrls[2]);

            // ASSERT
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);   
        }
    }
}
```

5. Run the test command

```sh
dotnet test
```
