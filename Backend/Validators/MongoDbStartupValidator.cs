using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Validators
{
    public class MongoDbStartupValidator
    {
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<MongoDbStartupValidator> _logger;
        private readonly string _databaseName;

        public MongoDbStartupValidator(
            IMongoClient mongoClient, 
            ILogger<MongoDbStartupValidator> logger,
            IConfiguration configuration)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _databaseName = configuration.GetSection("MongoDbSettings:DatabaseName").Value
                ?? throw new InvalidOperationException("DatabaseName not configured");
        }

        public async Task ValidateConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Validating MongoDB connection at startup...");

                // Ping MongoDB to verify connection
                var database = _mongoClient.GetDatabase(_databaseName);
                var pingCommand = new BsonDocument("ping", 1);
                await database.RunCommandAsync<BsonDocument>(pingCommand);

                // Verify database exists or can be created
                var collections = await database.ListCollectionNamesAsync();
                await collections.MoveNextAsync();

                _logger.LogInformation("MongoDB connection validated successfully. Database: {DatabaseName}", _databaseName);
            }
            catch (MongoException ex)
            {
                _logger.LogCritical(ex, "Failed to connect to MongoDB at startup. Database: {DatabaseName}", _databaseName);
                throw new InvalidOperationException($"MongoDB connection failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error during MongoDB startup validation");
                throw;
            }
        }
    }
}
