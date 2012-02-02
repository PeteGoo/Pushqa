using System;
using System.Linq.Expressions;
using Pushqa;
using Pushqa.Communication;

namespace Pushqa.Linq {
    /// <summary>
    /// A filtered event source
    /// </summary>
    /// <typeparam name="TSource">The type of the source event.</typeparam>
    public class EventFilteredQuerySource<TSource> : IEventQuery<TSource> {
        private readonly EventQuerySource<TSource> source;
        private readonly Expression<Func<TSource, bool>> filter;
        private readonly int top;
        private readonly int skip;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventFilteredQuerySource&lt;TSource&gt;"/> class.
        /// </summary>
        /// <param name="eventQuerySource">The event query source.</param>
        /// <param name="filter">The filter.</param>
        public EventFilteredQuerySource(EventQuerySource<TSource> eventQuerySource, Expression<Func<TSource, bool>> filter) : this(eventQuerySource, filter, 0, 0){
            source = eventQuerySource;
            this.filter = filter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventFilteredQuerySource&lt;TSource&gt;"/> class.
        /// </summary>
        /// <param name="eventQuerySource">The event query source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="skip">The number of events to skip at the beginning of the event stream.</param>
        /// <param name="top">The number of events to observe before completing the event stream.</param>
        public EventFilteredQuerySource(EventQuerySource<TSource> eventQuerySource, Expression<Func<TSource, bool>> filter, int skip, int top) {
            source = eventQuerySource;
            this.filter = filter;
            this.top = top;
            this.skip = skip;
        }

        /// <summary>
        /// Applies a selector clause to the query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">Selector function.</param>
        /// <returns>
        /// Representation of the query with a selector clause applied.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EventProjectedQuery<TSource, TResult> Select<TResult>(Expression<Func<TSource, TResult>> selector) {
            if (typeof(TSource) != typeof(TResult)) {
                throw new NotSupportedException("Projection is not currently supported");
            }
            return new EventProjectedQuery<TSource, TResult>(source, filter, selector);
        }

        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        public IObservable<TSource> AsObservable() {
            return new EventProjectedQuery<TSource, TSource>(source, filter, null, skip, top).AsObservable();
        }

        /// <summary>
        /// Skips the specified number of events at the start of the event stream.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public EventFilteredQuerySource<TSource> Skip(int count) {
            return new EventFilteredQuerySource<TSource>(source, filter, count, top);
        }
        
        /// <summary>
        /// Takes the specified number of events and then completes.
        /// </summary>
        /// <param name="count">The number of items to observe.</param>
        /// <returns></returns>
        public EventFilteredQuerySource<TSource> Take(int count) {
            return new EventFilteredQuerySource<TSource>(source, filter, 0, count);
        }

        IObservable<TSource> IEventQuery<TSource>.AsObservable(IEventProviderPipeline eventProviderPipeline) {
            return ((IEventQuery<TSource>)new EventProjectedQuery<TSource, TSource>(source, filter, null, skip, top)).AsObservable(eventProviderPipeline);
        }
    }
}