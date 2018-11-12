namespace NewPlatform.Flexberry.ServiceBus.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class for creating locks with keys.
    /// </summary>
    /// <typeparam name="T1">The type of keys by which locks will be determined.</typeparam>
    /// <typeparam name="T2">The type of objects created for the lock.</typeparam>
    public class KeyLocker<T1, T2>
        where T2 : new()
    {
        /// <summary>
        /// Internal lock, for sharing access to the dictionary <see cref="locks"/>.
        /// </summary>
        private object internalLock = new object();

        /// <summary>
        /// Dictionary for storing locks.
        /// </summary>
        private Dictionary<T1, Lock> locks = new Dictionary<T1, Lock>();

        /// <summary>
        /// Returns the lock object for the given key.
        /// </summary>
        /// <param name="key">The key for which you want to create a lock.</param>
        /// <returns>The lock object for the given key.</returns>
        public T2 GetLock(T1 key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (internalLock)
            {
                Lock @lock;
                if (locks.TryGetValue(key, out @lock))
                {
                    @lock.LockCount++;
                }
                else
                {
                    @lock = new Lock(new T2());
                    locks.Add(key, @lock);
                }

                return @lock.LockObject;
            }
        }

        /// <summary>
        /// Releases the lock with the given key.
        /// </summary>
        /// <param name="key">The key for which the lock was created.</param>
        public void FreeLock(T1 key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (internalLock)
            {
                Lock @lock;
                if (locks.TryGetValue(key, out @lock) && --@lock.LockCount == 0)
                {
                    locks.Remove(key);
                }
            }
        }

        /// <summary>
        /// Lock structure.
        /// </summary>
        private struct Lock
        {
            /// <summary>
            /// The object on which the lock will be executed.
            /// </summary>
            public T2 LockObject;

            /// <summary>
            /// The count of locks created on this object.
            /// </summary>
            public int LockCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="Lock"/> struct.
            /// </summary>
            /// <param name="lockObject">The object on which the lock will be executed.</param>
            public Lock(T2 lockObject)
            {
                LockObject = lockObject;
                LockCount = 1;
            }
        }
    }
}
