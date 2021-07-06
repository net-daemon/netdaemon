using System;

namespace NetDaemon.Common.Exceptions
{
    /// <inheritdoc/>
    [Serializable]
    public class NetDaemonNullReferenceException : NullReferenceException
    {
        /// <inheritdoc/>
        public NetDaemonNullReferenceException()
        {
        }

        /// <inheritdoc/>
        public NetDaemonNullReferenceException(string? message) : base(message)
        {
        }

        /// <inheritdoc/>
        public NetDaemonNullReferenceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected NetDaemonNullReferenceException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)        {
        }
    }

    /// <inheritdoc/>
    [Serializable]
    public class NetDaemonException : Exception
    {
        /// <inheritdoc/>
        public NetDaemonException()
        {
        }

        /// <inheritdoc/>
        public NetDaemonException(string? message) : base(message)
        {
        }

        /// <inheritdoc/>
        public NetDaemonException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected NetDaemonException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)        {
        }
    }

    /// <inheritdoc/>
    [Serializable]
    public class NetDaemonArgumentNullException : ArgumentNullException
    {
        /// <inheritdoc/>
        public NetDaemonArgumentNullException()
        {
        }

        /// <inheritdoc/>
        public NetDaemonArgumentNullException(string? paramName) : base(paramName)
        {
        }

        /// <inheritdoc/>
        public NetDaemonArgumentNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public NetDaemonArgumentNullException(string? paramName, string? message) : base(paramName, message)
        {
        }

        /// <inheritdoc/>
        protected NetDaemonArgumentNullException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)        {
        }
    }
}