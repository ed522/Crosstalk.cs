using System.Net.Sockets;

namespace Crosstalk.IO;

public abstract class Transport(Transport? basis) : IDisposable
{
    protected virtual Transport? TransportBase { get; init; } = basis;
    protected bool _disposed = false;

    public abstract void Send(byte[] packet, short id);
    public abstract byte[] Receive();
    public abstract Task SendAsync(byte[] packet, short id);
    public abstract Task<byte[]> ReceiveAsync();
    public void Send(byte[] packet)
    {
        this.Send(packet, 0);
    }
    public async Task SendAsync(byte[] packet)
    {
        await this.SendAsync(packet, 0);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    ~Transport() => Dispose(false);

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange<bool>(ref _disposed, true)) return;
        if (disposing)
        {
            // nothing to do here
        }

    }

}