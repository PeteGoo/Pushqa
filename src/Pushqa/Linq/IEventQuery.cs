using System;
using Pushqa.Communication;

namespace Pushqa.Linq {
    internal interface IEventQuery<out TResult> {
        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        IObservable<TResult> AsObservable();

        /// <summary>
        /// Obtains an observable sequence object to receive the WQL query results.
        /// </summary>
        /// <returns>Observable sequence for query results.</returns>
        IObservable<TResult> AsObservable(IEventProviderPipeline eventProviderPipeline);
    }
}