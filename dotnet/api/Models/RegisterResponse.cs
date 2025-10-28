namespace api.Models;

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public VerificationInfo Verification { get; set; } = new();
}

public class VerificationInfo
{
    public string Channel { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
