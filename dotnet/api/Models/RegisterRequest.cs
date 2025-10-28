using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "Username can only contain letters, numbers, dots and underscores")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Enter a valid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,64}$", 
        ErrorMessage = "Password must be 8–64 chars incl. upper/lower/digit/special")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public string Dob { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must accept the terms")]
    public bool AcceptTerms { get; set; }
}
