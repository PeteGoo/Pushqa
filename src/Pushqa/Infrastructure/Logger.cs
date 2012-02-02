using System;
using System.Diagnostics;

namespace Pushqa.Infrastructure {
    /// <summary>
    /// Logger
    /// </summary>
    public class Logger {

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        public void Log(LogLevel level, string message, params object[] parameters) {
#if (!DEBUG)
{  
            if(level == LogLevel.Debug){
                return;
            }
}
#endif
            Debug.WriteLine(string.Format(message, parameters), "Pushqa");
        }

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        public void Log(LogLevel level, Exception exception, string message, params object[] parameters) {
#if (!DEBUG)
{  
            if(level == LogLevel.Debug){
                return;
            }
}
#endif
            Debug.WriteLine(string.Format(message, parameters) + Environment.NewLine + exception, "Pushqa");
        }

        /// <summary>
        /// The Log Level
        /// </summary>
        public enum LogLevel {
            /// <summary>
            /// Debug for development
            /// </summary>
            Debug,
            /// <summary>
            /// Warning for possible problems
            /// </summary>
            Warning,
            /// <summary>
            /// Error for exceptions and runtime failures
            /// </summary>
            Error
        }
    }
}