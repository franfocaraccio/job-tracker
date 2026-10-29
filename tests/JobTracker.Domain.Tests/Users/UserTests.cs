using System.ComponentModel.DataAnnotations;
using JobTracker.Domain.Users;

namespace JobTracker.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Demo User", user.Name);
        Assert.Equal("demo@jobtracker.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var user = User.Create("  Demo User ", "demo@jobtracker.com", "hashed-password");

        Assert.Equal("Demo User", user.Name);
    }

    [Fact]
    public void Create_TrimsAndLowercasesEmail()
    {
        var user = User.Create("Demo User", "  Demo@JobTracker.com ", "hashed-password");

        Assert.Equal("demo@jobtracker.com", user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ThrowsValidationError(string name)
    {
        var exception = Assert.Throws<ValidationException>(
            () => User.Create(name, "demo@jobtracker.com", "hashed-password"));

        Assert.Equal("Name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNameLongerThanMaxLength_ThrowsValidationError()
    {
        var name = new string('a', User.MaxNameLength + 1);

        var exception = Assert.Throws<ValidationException>(
            () => User.Create(name, "demo@jobtracker.com", "hashed-password"));

        Assert.Equal("Name must be 100 characters or fewer.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankEmail_ThrowsValidationError(string email)
    {
        var exception = Assert.Throws<ValidationException>(
            () => User.Create("Demo User", email, "hashed-password"));

        Assert.Equal("Email is required.", exception.Message);
    }

    [Theory]
    [InlineData("demo")]
    [InlineData("demo@")]
    [InlineData("@jobtracker.com")]
    public void Create_WithInvalidEmail_ThrowsValidationError(string email)
    {
        var exception = Assert.Throws<ValidationException>(
            () => User.Create("Demo User", email, "hashed-password"));

        Assert.Equal("Email is not a valid email address.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void ValidatePassword_ShorterThanMinLength_ThrowsValidationError(string password)
    {
        var exception = Assert.Throws<ValidationException>(() => User.ValidatePassword(password));

        Assert.Equal("Password must be at least 6 characters long.", exception.Message);
    }

    [Fact]
    public void ValidatePassword_WithMinLength_DoesNotThrow()
    {
        User.ValidatePassword("123456");
    }
}
