using api.Models;

namespace api.Services;

public interface IRegisterService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, string idempotencyKey);
}
