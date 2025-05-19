using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Sockets;

namespace Crosstalk.IO;

class TransactionalTransport : Transport {

    private static readonly Dictionary<string, ITransactionTemplate<ITransaction>> Transactions = [];
    private AutoResetEvent newMessages = new(false);
    private readonly Thread sender = new Thread(() => {
        while (true) {

        }
    });
    private readonly ConcurrentQueue<(short, byte[])> sendQueue = [];

    public static bool RegisterTransaction(string name, ITransactionTemplate<ITransaction> transaction) {
        if (Transactions.ContainsKey(name)) return false;
        Transactions.Add(name, transaction);
        return true;
    }

    private readonly Dictionary<short, ITransaction> ActiveTransactions = [];

    public TransactionalTransport(Transport? basis) : base(basis) {
        sender.Start();
    }

    public void StartTransaction(string name) {
        short id = 0;
        while (ActiveTransactions.ContainsKey(id)) id++;
        ActiveTransactions.Add(id, Transactions[name].CreateNew());
    }

    public override void Send(byte[] packet, short id) {
        if (!ActiveTransactions.ContainsKey(id)) throw new ArgumentOutOfRangeException(nameof(id));
        else sendQueue.Enqueue((id, packet));
    }
    public override Task SendAsync(byte[] packet, short id) {
        if (!ActiveTransactions.ContainsKey(id)) throw new ArgumentOutOfRangeException(nameof(id));
        else sendQueue.Enqueue((id, packet));
        return Task.CompletedTask;
    }
    public override byte[] Receive() {
        throw new NotImplementedException();
    }
    public override Task<byte[]> ReceiveAsync() {
        throw new NotImplementedException();
    }

}