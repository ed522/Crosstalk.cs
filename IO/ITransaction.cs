namespace Crosstalk.IO {
    interface ITransaction {

        /// <summary>
        ///     Start the specified transaction.
        ///     This method is called in an isolated thread.
        /// </summary>
        /// <param name="id"></param>
        void Start(ushort id, TransactionalTransport context);
        void Stop();

    }
    interface ITransactionTemplate<out T> where T : ITransaction {
        T CreateNew();
    }
}