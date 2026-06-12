using Backend.Models;
using Backend.Services;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>
    {
        public CustomerRepository(MongoDbService mongoDbService) 
            : base(mongoDbService, "customers")
        { 
        }
    }
}
