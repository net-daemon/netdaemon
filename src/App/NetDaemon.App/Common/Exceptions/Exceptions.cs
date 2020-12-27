using System;

namespace NetDaemon.Common.Exceptions
{
    /// <inheritdoc/>
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
    }

    /// <inheritdoc/>
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
    }

    /// <inheritdoc/>
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
    }
}