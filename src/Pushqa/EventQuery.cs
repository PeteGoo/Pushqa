using System;
using System.Linq.Expressions;
using Pushqa;

namespace Pushqa {
    /// <summary>
    /// Represents the Query options specified on an Event Query Source
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class EventQuery<TSource, TResult> {
        /// <summary>
        /// Gets or sets the source to filter on.
        /// </summary>
        /// <value>The source.</value>
        public EventQuerySource<TSource> Source { get; set; }
        /// <summary>
        /// Gets or sets the "where" filter.
        /// </summary>
        /// <value>The filter.</value>
        public Expression<Func<TSource, bool>> Filter { get; set; }
        /// <summary>
        /// Gets or sets the "select" projection.
        /// </summary>
        /// <value>The selector.</value>
        public Expression<Func<TSource, TResult>> Selector { get; set; }
        /// <summary>
        /// Gets or sets the number of events to skip at the start of the event stream.
        /// </summary>
        /// <value>The skip.</value>
        public int Skip { get; set; }
        /// <summary>
        /// Gets or sets the number of events to return before completing the event stream.
        /// </summary>
        /// <value>The top.</value>
        public int Top { get; set; }
    }
}