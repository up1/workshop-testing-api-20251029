namespace api.Models;

public class ErrorResponse
{
    public ErrorDetail Error { get; set; } = new();
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public Dictionary<string, string>? Fields { get; set; }
    public string? Message { get; set; }
}
