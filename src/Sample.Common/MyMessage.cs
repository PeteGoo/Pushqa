using System;

namespace Sample.Common {
    /// <summary>
    /// Example Message Type
    /// </summary>
    public class MyMessage {
        /// <summary>
        /// Gets or sets the message id. Auto-incrementing for each unique message
        /// </summary>
        /// <value>The message id.</value>
        public long? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the time stamp of the current notification from the start of the event stream.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets a description of the message.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
    }
}