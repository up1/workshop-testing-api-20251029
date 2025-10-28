using api.Models;
using api.Repositories;
using api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests.Unit;

public class RegisterServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<RegisterService>> _mockLogger;
    private readonly RegisterService _registerService;

    public RegisterServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<RegisterService>>();
        _registerService = new RegisterService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _registerService.RegisterAsync(request, "test-idempotency-key");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.UserId);
        Assert.Equal("pending_verification", result.Status);
        Assert.Equal("email", result.Verification.Channel);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        var existingUser = new User { Id = "usr_123", Username = "somkiat.p" };
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("username", exception.Errors.Keys);
        Assert.Equal("Username is already taken", exception.Errors["username"]);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var existingUser = new User { Id = "usr_123", Email = "somkiat.p@example.com" };
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("email", exception.Errors.Keys);
        Assert.Equal("Email is already registered", exception.Errors["email"]);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingPhone_ShouldThrowValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var existingUser = new User { Id = "usr_123", Phone = "+66812345678" };
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(request.Phone)).ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("phone", exception.Errors.Keys);
        Assert.Equal("Phone number is already registered", exception.Errors["phone"]);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidDateFormat_ShouldThrowValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "invalid-date",
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("dob", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithAgeLessThan13_ShouldThrowValidationException()
    {
        // Arrange
        var tenYearsAgo = DateTime.UtcNow.AddYears(-10);
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = tenYearsAgo.ToString("yyyy-MM-dd"),
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("dob", exception.Errors.Keys);
        Assert.Equal("You must be at least 13 years old to register", exception.Errors["dob"]);
    }

    [Fact]
    public async Task RegisterAsync_WithFutureDateOfBirth_ShouldThrowValidationException()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = tomorrow.ToString("yyyy-MM-dd"),
            AcceptTerms = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _registerService.RegisterAsync(request, "test-idempotency-key"));
        
        Assert.Contains("dob", exception.Errors.Keys);
        Assert.Equal("Date of birth cannot be in the future", exception.Errors["dob"]);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        User? capturedUser = null;
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync((User user) => user);

        // Act
        await _registerService.RegisterAsync(request, "test-idempotency-key");

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotEqual(request.Password, capturedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash));
    }
}
