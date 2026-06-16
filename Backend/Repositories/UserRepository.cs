using Backend.Models;
using Backend.Policies;
using Backend.Services;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(MongoDbService mongoDbService, ILogger<UserRepository> logger, MongoDbResiliencePolicy resiliencePolicy)
            : base(mongoDbService, "users", logger, resiliencePolicy)
        {

        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
                });
            }
            catch (MongoException ex)
            {
                _logger.LogError("Error fetching user by username {Username}: {ErrorMessage}",
                    username, ex.Message);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    return await _collection.Find(u => u.Username == username).AnyAsync();
                });
            }
            catch (MongoException ex)
            {
                _logger.LogError("Error checking if username exists {Username}: {ErrorMessage}",
                    username, ex.Message);
                throw;
            }
        }
    }
}
