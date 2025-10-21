using backend_api_base_netcore8.Domain.Entities;

namespace backend_api_base_netcore8.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
