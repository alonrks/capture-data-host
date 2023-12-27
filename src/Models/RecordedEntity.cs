namespace CaptureDataHost.Models;

public record RecordedEntity
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string Image { get; set; }

    public int Timestamp { get; set; }
};