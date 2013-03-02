using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Pushqa.Communication;
using Pushqa.Infrastructure;
using Pushqa.Linq;
using Microsoft.AspNet.SignalR.Client;

namespace Pushqa.SignalR {
    /// <summary>
    /// Provides a signalr implementation of the oData query pipeline
    /// </summary>
    public class SignalrQueryPipeline : IEventProviderPipeline {
        private static readonly Logger logger = new Logger();
        
        private readonly IQueryUriProvider uriProvider;
        private readonly IMessageSerializer messageSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalrQueryPipeline"/> class.
        /// </summary>
        public SignalrQueryPipeline() : this(new QueryUriProvider(), new JsonMessageSerializer()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalrQueryPipeline"/> class.
        /// </summary>
        /// <param name="uriProvider">The URI provider.</param>
        /// <param name="messageSerializer">The message serializer. The default is the <see cref="JsonMessageSerializer"/></param>
        public SignalrQueryPipeline(IQueryUriProvider uriProvider, IMessageSerializer messageSerializer) {
            VerifyArgument.IsNotNull("uriProvider", uriProvider);
            VerifyArgument.IsNotNull("messageSerializer", messageSerializer);
            this.uriProvider = uriProvider;
            this.messageSerializer = messageSerializer;
        }

        /// <summary>
        /// Gets the event stream.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public IObservable<TResult> GetEventStream<TSource, TResult>(EventQuery<TSource, TResult> query) {
            Uri uri = uriProvider.GetQueryUri(query);
            logger.Log(Logger.LogLevel.Debug, "Opening connection");
            Connection connection = new Connection(uri.GetComponents(UriComponents.Scheme | UriComponents.HostAndPort | UriComponents.Path, UriFormat.SafeUnescaped), uri.Query.Trim('?').Split('&').Where(x => !string.IsNullOrWhiteSpace(x)).Select(pair => new KeyValuePair<string, string>(pair.Substring(0, pair.IndexOf('=')), pair.Substring(pair.IndexOf('=') + 1))).ToDictionary(x => x.Key, x => x.Value));
            connection.Start();

            connection.Closed += () => logger.Log(Logger.LogLevel.Debug, string.Format("Connection {0} closed", connection.ConnectionId));

            return connection.AsObservable()
                .Select(messageSerializer.Deserialize<EventWrapper<TResult>>)
                .TakeWhile(message => message.Type != EventWrapper<TResult>.EventType.Completed)
                .Select(message => {
                    if (message.Type == EventWrapper<TResult>.EventType.Error) {
                        throw new PushqaEventSourceException(message.ErrorMessage);
                    }
                            return message.Message;
                        })
                .Finally(connection.Stop);
        }
    }
}