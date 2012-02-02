using System;
using Pushqa;

namespace Pushqa.Communication {
    /// <summary>
    /// 
    /// </summary>
    public interface IEventProviderPipeline {
        
        /// <summary>
        /// Gets the event stream.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IObservable<TResult> GetEventStream<TSource, TResult>(EventQuery<TSource, TResult> query);
    }
}