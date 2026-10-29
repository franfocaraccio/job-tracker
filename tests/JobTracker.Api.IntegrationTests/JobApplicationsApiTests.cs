using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using JobTracker.Api.Contracts;
using JobTracker.Application.JobApplications;
using JobTracker.Application.Users;
using JobTracker.Domain.JobApplications;
using JobTracker.Domain.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace JobTracker.Api.IntegrationTests;

public sealed class JobApplicationsApiTests : IDisposable
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IJobApplicationRepository _repository = Substitute.For<IJobApplicationRepository>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly User _user = User.Create("Demo User", "demo@jobtracker.com", "hashed-password");

    public JobApplicationsApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped(_ => _repository));
        });
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<ITokenService>().CreateToken(_user);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Returns200WithTheUserApplications()
    {
        var application = NewApplication();
        _repository.GetByUserAsync(_user.Id, Arg.Any<CancellationToken>()).Returns([application]);

        var response = await _client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var applications = await response.Content.ReadFromJsonAsync<List<JobApplicationResponse>>(JsonOptions);
        Assert.NotNull(applications);
        Assert.Single(applications);
        Assert.Equal(application.Id, applications[0].Id);
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/applications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidData_Returns201WithTheApplication()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/applications", new CreateJobApplicationRequest("Ballast Lane", ".NET Developer", Today), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var application = await response.Content.ReadFromJsonAsync<JobApplicationResponse>(JsonOptions);
        Assert.NotNull(application);
        Assert.Equal("Ballast Lane", application.Company);
        Assert.Equal(ApplicationStatus.Applied, application.Status);
        Assert.EndsWith($"/api/applications/{application.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Create_WithFutureDate_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/applications", new CreateJobApplicationRequest("Ballast Lane", ".NET Developer", Today.AddDays(2)), JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidStatusChange_Returns200WithTheChanges()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, _user.Id, Arg.Any<CancellationToken>()).Returns(application);

        var response = await _client.PutAsJsonAsync(
            $"/api/applications/{application.Id}",
            new UpdateJobApplicationRequest("Ballast Lane", ".NET Developer", Today, ApplicationStatus.Interviewing),
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JobApplicationResponse>(JsonOptions);
        Assert.Equal(ApplicationStatus.Interviewing, updated?.Status);
    }

    [Fact]
    public async Task Update_WithInvalidStatusChange_Returns400()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, _user.Id, Arg.Any<CancellationToken>()).Returns(application);

        var response = await _client.PutAsJsonAsync(
            $"/api/applications/{application.Id}",
            new UpdateJobApplicationRequest("Ballast Lane", ".NET Developer", Today, ApplicationStatus.Accepted),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/applications/{Guid.NewGuid()}",
            new UpdateJobApplicationRequest("Ballast Lane", ".NET Developer", Today, ApplicationStatus.Applied),
            JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenFound_Returns204()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, _user.Id, Arg.Any<CancellationToken>()).Returns(application);

        var response = await _client.DeleteAsync($"/api/applications/{application.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/applications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private JobApplication NewApplication()
    {
        return JobApplication.Create(_user.Id, "Ballast Lane", ".NET Developer", Today);
    }
}
