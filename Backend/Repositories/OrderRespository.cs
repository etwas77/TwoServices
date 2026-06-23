using Backend.Models;
using Backend.Policies;
using Backend.Services;

namespace Backend.Repositories
{
    public class OrderRepository : GenericRepository<Order>
    {
        public OrderRepository(MongoDbService mongoDbService, ILogger<OrderRepository> logger, MongoDbResiliencePolicy resiliencePolicy)
            : base(mongoDbService, "orders", logger, resiliencePolicy)
        {
        }
    }
}
