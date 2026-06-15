using Backend.Models;
using Backend.Policies;
using Backend.Services;

namespace Backend.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>
    {
        public CustomerRepository(MongoDbService mongoDbService, ILogger<CustomerRepository> logger, MongoDbResiliencePolicy resiliencePolicy) 
            : base(mongoDbService, "customers", logger, resiliencePolicy)
        { 
        }
    }
}
