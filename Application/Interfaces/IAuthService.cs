using backend_api_base_netcore8.Application.DTOs;

namespace backend_api_base_netcore8.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken);
}
