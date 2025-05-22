using System.Numerics;

namespace Crosstalk.IO;

record SocketData(string Address, ushort Port);

interface INetworkRuntime {

    public void Initialize(Span<byte> dataBuffer);
    public void Serialize(object data);
    public T Deserialize<T>();

}