namespace UnisaveCompiler
{
    /// <summary>
    /// Generates numbers sequentially, thread safe
    /// </summary>
    public class ThreadSafeCounter
    {
        private int nextNumber = 1;
        private readonly object synchronizationLock = new object();

        public int GetNext()
        {
            int ret;
            
            lock (synchronizationLock)
            {
                ret = nextNumber;
                nextNumber++;
            }

            return ret;
        }
    }
}