namespace Crosstalk.IO {
    interface ITransaction {

        void Initialize(TransactionalTransport context);
        void Start(short id);
        void Stop(short id);

    }
    interface ITransactionTemplate<out T> where T : ITransaction {
        T CreateNew();
    }
}