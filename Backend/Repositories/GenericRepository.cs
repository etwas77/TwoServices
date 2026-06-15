using Backend.Policies;
using Backend.Services;
using MongoDB.Driver;

namespace Backend.Repositories
{
    public class GenericRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly ILogger _logger;
        private readonly MongoDbResiliencePolicy _resiliencePolicy;

        public GenericRepository(MongoDbService mongoDbService, string collectionName, 
            ILogger logger, 
            MongoDbResiliencePolicy resiliencePolicy)
        {
            _collection = mongoDbService.GetCollection<T>(collectionName);
            _logger = logger;
            _resiliencePolicy = resiliencePolicy;
        }

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    return await _collection.Find(_ => true).ToListAsync();
                });
            }
            catch(MongoException ex)
            {
                _logger.LogError("Error fetching all documents from collection {CollectionName}: {ErrorMessage}",
                    _collection.CollectionNamespace.CollectionName, ex.Message);
                throw;
            }
           
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.DeleteOneAsync(filter);
        }
    }
}
