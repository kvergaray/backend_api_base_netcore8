using backend_api_base_netcore8.Application.Interfaces;
using backend_api_base_netcore8.Domain.Entities;
using backend_api_base_netcore8.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_api_base_netcore8.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
