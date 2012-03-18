using System;
using System.Linq.Expressions;
using Pushqa;
using Pushqa.Communication;
using Pushqa.SignalR;

namespace Pushqa.Linq {
    /// <summary>
    /// A projected event type
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class EventProjectedQuery<TSource, TResult> : IEventQuery<TResult> {
        private readonly EventQuerySource<TSource> source;
        private readonly Expression<Func<TSource, bool>> filter;
        private readonly Expression<Func<TSource, TResult>> selector;
        private readonly int skip;
        private readonly int top;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProjectedQuery&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="selector">The selector.</param>
        public EventProjectedQuery(EventQuerySource<TSource> source, Expression<Func<TSource, bool>> filter, Expression<Func<TSource, TResult>> selector) {
            this.source = source;
            this.filter = filter;
            this.selector = selector;
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProjectedQuery&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="filter">The "where" filter.</param>
        /// <param name="selector">The "select" projection.</param>
        /// <param name="skip">The amount of events to skip at the start of the event stream.</param>
        /// <param name="top">The number of events to observe before completing.</param>
        public EventProjectedQuery(EventQuerySource<TSource> source, Expression<Func<TSource, bool>> filter, Expression<Func<TSource, TResult>> selector, int skip, int top) {
            this.source = source;
            this.filter = filter;
            this.selector = selector;
            this.skip = skip;
            this.top = top;
        }

        /// <summary>
        /// Skips the specified number of events at the start of the event stream.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public EventProjectedQuery<TSource, TResult> Skip(int count) {
            return new EventProjectedQuery<TSource, TResult>(source, filter, selector, count, top);
        }

        /// <summary>
        /// Takes the specified number of events and then completes.
        /// </summary>
        /// <param name="count">The number of items to observe.</param>
        /// <returns></returns>
        public EventProjectedQuery<TSource, TResult> Take(int count) {
            return new EventProjectedQuery<TSource, TResult>(source, filter, selector, skip, count);
        }


        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        public IObservable<TResult> AsObservable() {
            return ((IEventQuery<TResult>)this).AsObservable(new SignalrQueryPipeline());
        }

        IObservable<TResult> IEventQuery<TResult>.AsObservable(IEventProviderPipeline eventProviderPipeline) {
            EventQuery<TSource, TResult> query = new EventQuery<TSource, TResult> {
                Source = source,
                Filter = filter,
                Selector = selector,
                Skip = skip,
                Top = top
            };
            return eventProviderPipeline.GetEventStream(query);
        }

    }
}