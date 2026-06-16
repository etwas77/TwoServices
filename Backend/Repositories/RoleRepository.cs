using Backend.Models;
using Backend.Policies;
using Backend.Services;
using Contracts.DTO;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class RoleRepository : GenericRepository<Role>
    {
        public RoleRepository(MongoDbService mongoDbService, ILogger<RoleRepository> logger, MongoDbResiliencePolicy resiliencePolicy)
            : base(mongoDbService, "roles", logger, resiliencePolicy)
        {
        }

        public async Task<Role?> GetByTypeAsync(RoleType type)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    return await _collection.Find(r => r.Type == type).FirstOrDefaultAsync();
                });
            }
            catch (MongoException ex)
            {
                _logger.LogError("Error fetching role by type {RoleType}: {ErrorMessage}",
                    type, ex.Message);
                throw;
            }
        }
    }
}
