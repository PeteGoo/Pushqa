using System;
using Pushqa;
using Pushqa.Infrastructure;

namespace Pushqa {
    /// <summary>
    /// An Event Provider that allows remoting of queries on observable collection
    /// </summary>
    public abstract class EventProvider {
        private readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProvider"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI of the Queryable Push Service route.</param>
        protected EventProvider(Uri baseUri) {
            this.baseUri = baseUri;
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <param name="eventResourceName">Name of the event resource.</param>
        /// <returns></returns>
         protected virtual EventQuerySource<T> CreateQuery<T>([NotNull]string eventResourceName) {
             VerifyArgument.IsNotNullOrWhitespace("eventResourceName", eventResourceName);
             return new EventQuerySource<T>(eventResourceName, baseUri);
         }
    }
}