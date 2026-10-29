using JobTracker.Domain.Users;

namespace JobTracker.Application.Users;

public interface ITokenService
{
    string CreateToken(User user);
}
