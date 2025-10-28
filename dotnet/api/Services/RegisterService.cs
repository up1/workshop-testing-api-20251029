using api.Models;
using api.Repositories;
using Microsoft.Extensions.Logging;

namespace api.Services;

public class RegisterService : IRegisterService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterService> _logger;

    public RegisterService(IUserRepository userRepository, ILogger<RegisterService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, string idempotencyKey)
    {
        _logger.LogInformation("Starting registration process for username: {Username}", request.Username);

        // Validate uniqueness
        await ValidateUniquenessAsync(request);

        // Validate date of birth
        var dob = ValidateDateOfBirth(request.Dob);

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = $"usr_{Guid.NewGuid():N}",
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = passwordHash,
            Dob = dob,
            Status = "pending_verification",
            AcceptTerms = request.AcceptTerms,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);

        _logger.LogInformation("User registered successfully with ID: {UserId}", createdUser.Id);

        // TODO: Send verification email/SMS
        var response = new RegisterResponse
        {
            UserId = createdUser.Id,
            Status = createdUser.Status,
            Verification = new VerificationInfo
            {
                Channel = "email",
                SentAt = DateTime.UtcNow
            }
        };

        return response;
    }

    private async Task ValidateUniquenessAsync(RegisterRequest request)
    {
        var errors = new Dictionary<string, string>();

        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            errors["username"] = "Username is already taken";
        }

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            errors["email"] = "Email is already registered";
        }

        var existingPhone = await _userRepository.GetByPhoneAsync(request.Phone);
        if (existingPhone != null)
        {
            errors["phone"] = "Phone number is already registered";
        }

        if (errors.Any())
        {
            _logger.LogWarning("Registration validation failed: {Errors}", string.Join(", ", errors.Keys));
            throw new ValidationException(errors);
        }
    }

    private DateOnly ValidateDateOfBirth(string dobString)
    {
        if (!DateOnly.TryParse(dobString, out var dob))
        {
            throw new ValidationException(new Dictionary<string, string>
            {
                ["dob"] = "Invalid date format. Use YYYY-MM-DD"
            });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        if (dob > today)
        {
            throw new ValidationException(new Dictionary<string, string>
            {
                ["dob"] = "Date of birth cannot be in the future"
            });
        }

        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age))
        {
            age--;
        }

        if (age < 13)
        {
            throw new ValidationException(new Dictionary<string, string>
            {
                ["dob"] = "You must be at least 13 years old to register"
            });
        }

        return dob;
    }
}

public class ValidationException : Exception
{
    public Dictionary<string, string> Errors { get; }

    public ValidationException(Dictionary<string, string> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}
