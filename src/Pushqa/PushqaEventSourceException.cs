using System;

namespace Pushqa {
    /// <summary>
    /// An exception that occurs when a OnError is called on the event source
    /// </summary>
    public class PushqaEventSourceException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushqaEventSourceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PushqaEventSourceException(string message) : base(message) {}
    }
}