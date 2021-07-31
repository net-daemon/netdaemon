using System;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Interface for logging capabilities in NetDaemon Apps
    /// </summary>
    public interface INetDaemonAppLogging
    {
        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void Log(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogInformation(string message);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogInformation(Exception exception, string message);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogInformation(string message, params object[] param);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogInformation(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogDebug(string message);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogDebug(Exception exception, string message);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogDebug(string message, params object[] param);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogDebug(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogError(string message);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogError(Exception exception, string message);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogError(string message, params object[] param);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogError(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogTrace(string message);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogTrace(Exception exception, string message);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogTrace(string message, params object[] param);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogTrace(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogWarning(string message);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogWarning(Exception exception, string message);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogWarning(string message, params object[] param);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogWarning(Exception exception, string message, params object[] param);
    }
}