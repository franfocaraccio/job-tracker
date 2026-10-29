using Npgsql;
using Testcontainers.PostgreSql;

namespace JobTracker.Infrastructure.IntegrationTests;

/// <summary>
/// Starts a disposable PostgreSQL container with the application schema for the integration tests.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var schema = await File.ReadAllTextAsync("001_schema.sql");
        await _container.ExecScriptAsync(schema);

        DataSource = NpgsqlDataSource.Create(_container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync();
        await _container.DisposeAsync();
    }
}
