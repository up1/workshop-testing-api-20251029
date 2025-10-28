using api.Models;
using api.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace tests.Integration;

public class RegisterApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RegisterApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove any existing DbContext
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(AppDbContext));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add DbContext using in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });
            });

            builder.UseEnvironment("Testing");
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnOk()
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

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.UserId);
        Assert.Equal("pending_verification", result.Status);
        Assert.Equal("email", result.Verification.Channel);
    }

    [Fact]
    public async Task Register_WithMissingIdempotencyKey_ShouldReturnBadRequest()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("MISSING_IDEMPOTENCY_KEY", result.Error.Code);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "invalid-email",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
        Assert.NotNull(result.Error.Fields);
        Assert.Contains("Email", result.Error.Fields.Keys);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "weak",
            ConfirmPassword = "weak",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
        Assert.NotNull(result.Error.Fields);
        Assert.Contains("Password", result.Error.Fields.Keys);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Somkiat Pui",
            Username = "somkiat.p",
            Email = "somkiat.p@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "DifferentPassword123!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var firstRequest = new RegisterRequest
        {
            FullName = "First User",
            Username = "somkiat.p",
            Email = "first@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        var secondRequest = new RegisterRequest
        {
            FullName = "Second User",
            Username = "somkiat.p", // Same username
            Email = "second@example.com",
            Phone = "+66812345679",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-11",
            AcceptTerms = true
        };

        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();
        
        client1.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        client2.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        await client1.PostAsJsonAsync("/api/v1/register", firstRequest);
        var response = await client2.PostAsJsonAsync("/api/v1/register", secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
        Assert.NotNull(result.Error.Fields);
        Assert.Contains("username", result.Error.Fields.Keys);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var firstRequest = new RegisterRequest
        {
            FullName = "First User",
            Username = "first.user",
            Email = "same@example.com",
            Phone = "+66812345678",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-10",
            AcceptTerms = true
        };

        var secondRequest = new RegisterRequest
        {
            FullName = "Second User",
            Username = "second.user",
            Email = "same@example.com", // Same email
            Phone = "+66812345679",
            Password = "Pa$$w0rd2025!",
            ConfirmPassword = "Pa$$w0rd2025!",
            Dob = "1995-05-11",
            AcceptTerms = true
        };

        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();
        
        client1.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        client2.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        await client1.PostAsJsonAsync("/api/v1/register", firstRequest);
        var response = await client2.PostAsJsonAsync("/api/v1/register", secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
        Assert.NotNull(result.Error.Fields);
        Assert.Contains("email", result.Error.Fields.Keys);
    }

    [Fact]
    public async Task Register_WithoutAcceptingTerms_ShouldReturnBadRequest()
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
            AcceptTerms = false
        };

        _client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(result);
        Assert.Equal("VALIDATION_FAILED", result.Error.Code);
    }
}
