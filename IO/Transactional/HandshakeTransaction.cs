namespace Crosstalk.IO.Transactional;

using Crosstalk.IO;

class HandshakeTransaction : ITransaction
{

    public class HandshakeTransactionTemplate : ITransactionTemplate<HandshakeTransaction>
    {
        HandshakeTransaction ITransactionTemplate<HandshakeTransaction>.CreateNew()
        {
            return new HandshakeTransaction();
        }
    }
    
    private static void CancelIfRequested(ushort id, TransactionalTransport context, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            // send a cancellation notification
        }
    }

    public void Start(ushort id, TransactionalTransport context, CancellationToken token)
    {

        token.ThrowIfCancellationRequested();
        

    }

    public void Stop()
    {

    }

}