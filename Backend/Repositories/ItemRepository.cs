using Backend.Models;
using Backend.Policies;
using Backend.Services;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class ItemRepository : GenericRepository<Item>
    {
        public ItemRepository(MongoDbService mongoDbService, ILogger<ItemRepository> logger, MongoDbResiliencePolicy resiliencePolicy)
            : base(mongoDbService, "items", logger, resiliencePolicy)
        {
        }

        public async Task<Item?> GetByNameAsync(string name)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    return await _collection.Find(u => u.Name == name).FirstOrDefaultAsync();
                });
            }
            catch (MongoException ex)
            {
                _logger.LogError("Error fetching item by name {Name}: {ErrorMessage}",
                    name, ex.Message);
                throw;
            }
        }
    }
}
