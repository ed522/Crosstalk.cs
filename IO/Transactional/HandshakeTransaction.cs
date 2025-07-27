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


}