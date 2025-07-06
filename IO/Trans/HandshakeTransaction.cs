namespace Crosstalk.IO.Trans;

class HandshakeTransaction : ITransaction
{

    public class HandshakeTransactionTemplate : ITransactionTemplate<HandshakeTransaction>
    {
        HandshakeTransaction ITransactionTemplate<HandshakeTransaction>.CreateNew()
        {
            return new HandshakeTransaction();
        }
    }


}