using System;

namespace KylesUnityLib.Pooling
{
    /// <summary>
    /// Exception for in the event of a <see cref="Pool{T}"/> not successfully initializing
    /// </summary>
    public class PoolInitializationException : Exception
    {
        /// <inheritdoc/>
        public PoolInitializationException(string message) : base(message) { }
        /// <inheritdoc/>
        public PoolInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception for the event of an attempt to overwrite an active <see cref="Pool{T}"/> while elements already exist in the pool
    /// </summary>
    public class ActivePoolOverwriteException : Exception
    {
        ///<inheritdoc/>
        public ActivePoolOverwriteException(string message) : base(message) { }
    }

    public class PoolIsInactiveException : Exception
    {
        ///<inheritdoc/>
        public PoolIsInactiveException(string message) : base(message) { }
    }

    public class PoolExhaustedException : Exception
    {
        ///<inheritdoc/>
        public PoolExhaustedException(string message) : base(message) { }
    }

}
