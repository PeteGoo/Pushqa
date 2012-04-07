using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pushqa.Communication {
    /// <summary>
    /// A wrapper for a Pushqa event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventWrapper<T> {

        /// <summary>
        /// Defines the possible event types in a Pushqa server event
        /// </summary>
        public enum EventType {
            /// <summary>
            /// A message
            /// </summary>
            Message,
            /// <summary>
            /// An error occured in the event source
            /// </summary>
            Error,
            /// <summary>
            /// The event source has completed
            /// </summary>
            Completed
        }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventType Type { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        /// <value>The message content.</value>
        public T Message { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }
    }


}