using CaptureDataHost.Models;
using Microsoft.Extensions.Logging;

namespace CaptureDataHost;
public class HostProducer : IHostProducer
{
    public HostMode Mode => HostMode.Producer;
    private readonly ISqliteRepository _repository;
    private readonly ILogger<HostProducer> _logger;

    public HostProducer(ISqliteRepository repository, ILogger<HostProducer> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task RunAsync(int threshold)
    {
        _logger.LogInformation($"Number of pending records {threshold}");
        while (true)
        {
            if (threshold == 0)
            {
                break;
            }

            var id = Guid.NewGuid().ToString("D");
            var now = DateTime.UtcNow;
            var entity = new RecordedEntity
            {
                Id = Guid.NewGuid().ToString("D"),
                Name = $"{id} Entity",
                Timestamp = (int)now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                Image = "<some image>"
            };
            await _repository?.AddAsync(entity)!;
            _logger.LogInformation($"Entity {entity.Name} created at {now}");
            Task.Delay(500).Wait();
            threshold--;
        }

        await _repository?.HousekeepingAsync()!;

    }
}