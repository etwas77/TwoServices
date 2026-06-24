using Backend.Models;
using Backend.Policies;
using Backend.Services;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>
    {
        public CustomerRepository(MongoDbService mongoDbService, ILogger<CustomerRepository> logger, MongoDbResiliencePolicy resiliencePolicy) 
            : base(mongoDbService, "customers", logger, resiliencePolicy)
        { 
        }

        public async Task<Customer> GetByNameAsync(string name)
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
                _logger.LogError("Error fetching customer by name {Name}: {ErrorMessage}",
                    name, ex.Message);
                throw;
            }
        }
    }
}
