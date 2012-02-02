namespace Pushqa.Communication {
    /// <summary>
    /// Describes a ODta Push message serializer
    /// </summary>
    public interface IMessageSerializer {
        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        string Serialize<T>(T message);

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        T Deserialize<T>(string message);
    }
}