using JobTracker.Application.Users;
using JobTracker.Domain.Users;
using Npgsql;

namespace JobTracker.Infrastructure.Users;

/// <summary>
/// Stores users in PostgreSQL using the Npgsql ADO.NET provider.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("SELECT id, name, email, password_hash FROM users WHERE id = @id");
        command.Parameters.AddWithValue("id", id);

        return await ReadSingleAsync(command, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("SELECT id, name, email, password_hash FROM users WHERE email = @email");
        command.Parameters.AddWithValue("email", email);

        return await ReadSingleAsync(command, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("INSERT INTO users (id, name, email, password_hash) VALUES (@id, @name, @email, @passwordHash)");
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("passwordHash", user.PasswordHash);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<User?> ReadSingleAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return User.FromDatabase(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3));
    }
}
