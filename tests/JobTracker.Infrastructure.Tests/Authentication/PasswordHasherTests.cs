using JobTracker.Infrastructure.Authentication;

namespace JobTracker.Infrastructure.Tests.Authentication;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void Hash_DoesNotReturnThePlainPassword()
    {
        var hash = _hasher.Hash("Demo123!");

        Assert.NotEqual("Demo123!", hash);
    }

    [Fact]
    public void Verify_WithTheSamePassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Demo123!");

        Assert.True(_hasher.Verify("Demo123!", hash));
    }

    [Fact]
    public void Verify_WithAnotherPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Demo123!");

        Assert.False(_hasher.Verify("Wrong123!", hash));
    }

    [Fact]
    public void Verify_WithTheSeededDemoHash_AcceptsTheDemoPassword()
    {
        var seededHash = "AQAAAAIAAYagAAAAEAN7/Z2pot+dfUhAnaU2rIcZgssgjd+B55pZQ9kRXQjF/3gEjbbs0B9GZoI+mYiP6Q==";

        Assert.True(_hasher.Verify("Demo123!", seededHash));
    }
}
