using Microsoft.Extensions.Logging;

namespace CaptureDataHost;

public class HostConsumer : IHostConsumer
{
    public HostMode Mode => HostMode.Consumer;
    private readonly ISqliteRepository _repository;
    private readonly ILogger<HostConsumer> _logger;

    public HostConsumer(ISqliteRepository repository, ILogger<HostConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task RunAsync(int threshold)
    {
        var numberOfConsumedRecords = 0;
        var numberOfRetries = 5;
        while (true)
        {
            var entities = await _repository?.GetAllAsync()!;
            var recordedEntities = entities.ToList();
            if (!recordedEntities.Any())
            {
                if (numberOfRetries-- >= 0)
                {
                    Task.Delay(1000).Wait();
                    continue;
                }
                _logger.LogInformation("No more records found.");
                break;
            }
            foreach (var entity in recordedEntities)
            {
                var createdAt = new DateTime(1970, 1, 1).AddSeconds(entity.Timestamp);
                _logger.LogInformation($"Entity {entity.Name} created at {createdAt}");
                await _repository?.DeleteAsync(entity.Id)!;
                numberOfConsumedRecords++;
            }

            Task.Delay(1000).Wait();
        }
        await _repository?.HousekeepingAsync()!;
        _logger.LogInformation($"Number of consumed records {numberOfConsumedRecords}, expected number of records {threshold}.");
    }
}