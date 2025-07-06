using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Sockets;

namespace Crosstalk.IO;

class TransactionalTransport : Transport
{

    private readonly struct ThreadArguments
    {
        public readonly AutoResetEvent messageTrigger;
        public readonly CancellationToken token;
        public ThreadArguments(AutoResetEvent messageTrigger, CancellationToken token)
        {
            this.messageTrigger = messageTrigger;
            this.token = token;
        }
    }

    private static readonly Dictionary<string, ITransactionTemplate<ITransaction>> Transactions = [];
    private readonly AutoResetEvent newMessageTrigger = new(false);
    private readonly CancellationTokenSource tokenSource;
    private readonly Thread sender;
    private readonly ConcurrentQueue<(short, byte[])> sendQueue = [];

    public static bool RegisterTransaction(string name, ITransactionTemplate<ITransaction> transaction)
    {
        if (Transactions.ContainsKey(name)) return false;
        Transactions.Add(name, transaction);
        return true;
    }

    private readonly Dictionary<short, ITransaction> ActiveTransactions = [];

    public TransactionalTransport(Transport? basis) : base(basis)
    {

        tokenSource = new();
        ThreadArguments args = new(this.newMessageTrigger, this.tokenSource.Token);

        sender = new Thread((object? rawArgs) =>
        {
            ThreadArguments args = (ThreadArguments)rawArgs!;
            while (true)
            {
                if (args.token.IsCancellationRequested) return;
                args.messageTrigger.WaitOne();
                // TODO send the message
                throw new NotImplementedException();
            }
        });
        sender.Start(args);
    }

    public void StartTransaction(string name)
    {
        short id = 0;
        while (ActiveTransactions.ContainsKey(id)) id++;
        ActiveTransactions.Add(id, Transactions[name].CreateNew());
    }

    public override void Send(byte[] packet, short id)
    {
        if (!ActiveTransactions.ContainsKey(id)) throw new ArgumentOutOfRangeException(nameof(id));
        else sendQueue.Enqueue((id, packet));
    }
    public override Task SendAsync(byte[] packet, short id)
    {
        if (!ActiveTransactions.ContainsKey(id)) throw new ArgumentOutOfRangeException(nameof(id));
        else sendQueue.Enqueue((id, packet));
        return Task.CompletedTask;
    }
    public override byte[] Receive()
    {
        throw new NotImplementedException();
    }
    public override Task<byte[]> ReceiveAsync()
    {
        throw new NotImplementedException();
    }

    public override void Close()
    {
        this.tokenSource.Cancel();
        this.tokenSource.Dispose();
    }

    ~TransactionalTransport()
    {
        this.tokenSource.Dispose();
    }

}