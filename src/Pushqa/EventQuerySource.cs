using System;
using System.Linq.Expressions;
using Pushqa.Communication;
using Pushqa.Infrastructure;
using Pushqa.Linq;

namespace Pushqa {
    /// <summary>
    /// A source of oData Events
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    public class EventQuerySource<T> : IEventQuery<T> {
        private readonly string eventResourceName;
        private readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventQuerySource&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="eventResourceName">Name of the event resource.</param>
        /// <param name="baseUri"> </param>
        public EventQuerySource([NotNull]string eventResourceName, Uri baseUri) {
            VerifyArgument.IsNotNull("eventResourceName", eventResourceName);
            this.eventResourceName = eventResourceName;
            this.baseUri = baseUri;
        }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public Uri BaseUri {
            get { return baseUri; }
        }

        /// <summary>
        /// Gets the name of the event resource.
        /// </summary>
        /// <value>The name of the event resource.</value>
        public string EventResourceName {
            get { return eventResourceName; }
        }

        /// <summary>
        /// Applies a filter to the query.
        /// </summary>
        /// <param name="filter">Filter predicate.</param>
        /// <returns>Representation of the query with a filter applied.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EventFilteredQuerySource<T> Where([NotNull]Expression<Func<T, bool>> filter) {
            VerifyArgument.IsNotNull("filter", filter);
            return new EventFilteredQuerySource<T>(this, filter);
        }

        /// <summary>
        /// Applies a projection clause to the query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">Selector function.</param>
        /// <returns>
        /// Representation of the query with a projection clause applied.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EventProjectedQuery<T, TResult> Select<TResult>([NotNull]Expression<Func<T, TResult>> selector) {
            VerifyArgument.IsNotNull("selected", selector);
            if (typeof(T) != typeof(TResult)) {
                throw new NotSupportedException("Projection is not currently supported");
            }
            return new EventProjectedQuery<T, TResult>(this, null, selector);
        }

        /// <summary>
        /// Observes the current event source and completes after the specified number of events.
        /// </summary>
        /// <param name="i">The number of events to take.</param>
        /// <returns></returns>
        public EventProjectedQuery<T, T> Take(int i) {
            return new EventProjectedQuery<T, T>(this, null, null, 0, i);
        }

        /// <summary>
        /// Skips the specified number of events at the start of the event stream.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public EventProjectedQuery<T, T> Skip(int count) {
            return new EventProjectedQuery<T, T>(this, null, null, count, 0);
        }

        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        public IObservable<T> AsObservable() {
            return new EventProjectedQuery<T, T>(this, null, null, 0, 0).AsObservable();
        }

        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        IObservable<T> IEventQuery<T>.AsObservable(IEventProviderPipeline pipeline) {
            return ((IEventQuery<T>)new EventProjectedQuery<T, T>(this, null, null, 0, 0)).AsObservable(pipeline);
        }

    }
}