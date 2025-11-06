using FluentAssertions;
using Mangalith.Domain.Entities;

namespace Mangalith.Tests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "John Doe";
        var username = "johndoe";

        // Act
        var user = new User(email, passwordHash, fullName, username);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FullName.Should().Be(fullName);
        user.Username.Should().Be(username);
        user.Role.Should().Be(UserRole.Reader);
        user.IsActive.Should().BeTrue();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastLoginAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutUsername_ShouldGenerateUsernameFromEmail()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "John Doe";

        // Act
        var user = new User(email, passwordHash, fullName);

        // Assert
        user.Username.Should().Be("test");
    }

    [Fact]
    public void UpdateLastLogin_ShouldUpdateTimestamps()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");
        var loginTime = DateTime.UtcNow.AddMinutes(-5);
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.UpdateLastLogin(loginTime);

        // Assert
        user.LastLoginAtUtc.Should().Be(loginTime);
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordHashAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "oldHash", "John Doe");
        var newPasswordHash = "newHashedPassword";
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateProfileFieldsAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");
        var newFullName = "Jane Smith";
        var newUsername = "janesmith";
        var newBio = "Software developer";
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.UpdateProfile(newFullName, newUsername, newBio);

        // Assert
        user.FullName.Should().Be(newFullName);
        user.Username.Should().Be(newUsername);
        user.Bio.Should().Be(newBio);
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateAvatar_ShouldUpdateAvatarPathAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");
        var avatarPath = "/uploads/avatars/user123.jpg";
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.UpdateAvatar(avatarPath);

        // Assert
        user.Avatar.Should().Be(avatarPath);
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateRole_ShouldUpdateRoleAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.UpdateRole(UserRole.Moderator);

        // Assert
        user.Role.Should().Be(UserRole.Moderator);
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SetActive_ShouldUpdateActiveStatusAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.SetActive(false);

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(UserRole.Reader)]
    [InlineData(UserRole.Uploader)]
    [InlineData(UserRole.Moderator)]
    [InlineData(UserRole.Administrator)]
    public void UpdateRole_WithValidRoles_ShouldUpdateSuccessfully(UserRole role)
    {
        // Arrange
        var user = new User("test@example.com", "hash", "John Doe");

        // Act
        user.UpdateRole(role);

        // Assert
        user.Role.Should().Be(role);
    }
}