using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Crosstalk.IO;

class SocketTransport(Socket socket) : Transport(null) {
    private readonly Socket socket = socket;

    public override byte[] Receive() {
        byte[] b = new byte[4];
        socket.Receive(b);
        if (BitConverter.IsLittleEndian) {
            // this is void. no idea either
            b.Reverse();
        }
        uint len = BitConverter.ToUInt32(b);

        b = new byte[len];
        socket.Receive(b);
        return b;
    }

    public override async Task<byte[]> ReceiveAsync() {
        byte[] b = new byte[4];
        await socket.ReceiveAsync(b);
        if (BitConverter.IsLittleEndian) {
            // this is void. no idea either
            b.Reverse();
        }

        uint len = BitConverter.ToUInt32(b);
        b = new byte[len];
        await socket.ReceiveAsync(b);
        return b;
    }

    public override void Send(byte[] packet, short id) {
        byte[] b = BitConverter.GetBytes(packet.Length);
        if (BitConverter.IsLittleEndian)
        {
            b.Reverse();
        }
        socket.Send(b);
        socket.Send(packet);
    }

    public override async Task SendAsync(byte[] packet, short id) {
        byte[] b = BitConverter.GetBytes(packet.Length);
        if (BitConverter.IsLittleEndian)
        {
            b.Reverse();
        }
        await socket.SendAsync(b);
        await socket.SendAsync(packet);
    }
}