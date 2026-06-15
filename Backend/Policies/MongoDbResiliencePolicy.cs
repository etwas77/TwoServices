using MongoDB.Driver;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Backend.Policies
{
    public class MongoDbResiliencePolicy
    {
        private readonly ILogger<MongoDbResiliencePolicy> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncTimeoutPolicy _timeoutPolicy;

        public MongoDbResiliencePolicy(ILogger<MongoDbResiliencePolicy> logger)
        {
            _logger = logger;

            // Retry policy: 3 attempts with exponential backoff
            _retryPolicy = Policy
                .Handle<MongoException>(ex => IsTransient(ex))
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "MongoDB transient error on attempt {RetryCount}. Waiting {TimeSpan} before retry",
                            retryCount,
                            timeSpan);
                    });

            // Timeout policy: 30 seconds max per operation
            _timeoutPolicy = Policy.TimeoutAsync(30, TimeoutStrategy.Pessimistic);
        }

        private bool IsTransient(MongoException exception)
        {
            // Transient errors that should be retried
            return exception is MongoConnectionException ||
                   exception is MongoExecutionTimeoutException ||
                   exception is MongoNodeIsRecoveringException ||
                   exception is MongoNotPrimaryException;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            return await _timeoutPolicy.WrapAsync(_retryPolicy).ExecuteAsync(operation);
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            await _timeoutPolicy.WrapAsync(_retryPolicy).ExecuteAsync(operation);
        }
    }


}
