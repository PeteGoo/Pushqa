using Newtonsoft.Json;
using Pushqa.Infrastructure;

namespace Pushqa.Communication {
    /// <summary>
    /// A JSON implementation of an Pushqa <see cref="IMessageSerializer"/>
    /// </summary>
    public class JsonMessageSerializer : IMessageSerializer {
        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string Serialize<T>(T message) {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public T Deserialize<T>([NotNull]string message) {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.DeserializeObject<T>(message);
        }
    }
}