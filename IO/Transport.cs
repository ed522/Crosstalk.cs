using System.Net.Sockets;

namespace Crosstalk.IO;

public abstract class Transport(Transport? basis)
{
    protected virtual Transport? TransportBase { get; init; } = basis;

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

    public abstract void Close();

}