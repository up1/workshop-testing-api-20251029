using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/v1")]
public class RegisterController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly ILogger<RegisterController> _logger;

    public RegisterController(IRegisterService registerService, ILogger<RegisterController> logger)
    {
        _registerService = registerService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "MISSING_IDEMPOTENCY_KEY",
                        Message = "Idempotency-Key header is required"
                    }
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.First().ErrorMessage ?? "Invalid value"
                    );

                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_FAILED",
                        Fields = errors
                    }
                });
            }

            var response = await _registerService.RegisterAsync(request, idempotencyKey);
            
            _logger.LogInformation("Registration completed successfully for user: {UserId}", response.UserId);
            
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "VALIDATION_FAILED",
                    Fields = ex.Errors
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred"
                }
            });
        }
    }
}
