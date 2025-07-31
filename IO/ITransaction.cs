namespace Crosstalk.IO
{
    interface ITransaction
    {

        /// <summary>
        ///     Start the specified transaction.
        ///     This method is called in an isolated thread.
        /// </summary>
        /// <param name="id"></param>
        void Start(ushort id, TransactionalTransport context, CancellationToken token);

        /// <summary>
        ///     Called when the thread is interrupted.
        ///     Called in the same thread as the Start method, after it has exited/been terminated.
        /// </summary>
        /// <param name="id"></param>
        void Stop();

    }
    interface ITransactionTemplate<out T> where T : ITransaction
    {
        T CreateNew();
    }
    
}