using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using JobTracker.Api.Contracts;
using JobTracker.Application.Users;
using JobTracker.Domain.Users;
using JobTracker.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace JobTracker.Api.IntegrationTests;

public sealed class UsersApiTests : IDisposable
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped(_ => _repository));
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201WithTheUser()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Demo User", "demo@jobtracker.com", "Demo123!"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal("Demo User", user.Name);
        Assert.Equal("demo@jobtracker.com", user.Email);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Demo User", "demo@jobtracker.com", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        AddExistingUser("Demo123!");

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("demo@jobtracker.com", "Demo123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(login?.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        AddExistingUser("Demo123!");

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("demo@jobtracker.com", "Wrong123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithToken_Returns200WithTheUser()
    {
        var existingUser = AddExistingUser("Demo123!");
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("demo@jobtracker.com", "Demo123!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal(existingUser.Id, user.Id);
        Assert.Equal("Demo User", user.Name);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private User AddExistingUser(string password)
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", new PasswordHasher().Hash(password));
        _repository.GetByEmailAsync("demo@jobtracker.com", Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
