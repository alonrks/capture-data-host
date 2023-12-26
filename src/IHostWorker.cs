namespace CaptureDataHost;

public interface IHostWorker
{
    HostMode Mode { get;}
    Task RunAsync(int threshold);
}