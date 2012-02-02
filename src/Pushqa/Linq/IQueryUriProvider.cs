using System;
using System.Collections.Generic;

namespace Pushqa.Linq {
    /// <summary>
    /// Provides a uri for a given query
    /// </summary>
    public interface IQueryUriProvider {
        /// <summary>
        /// Gets the query URI.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        Uri GetQueryUri<TSource, TResult>(EventQuery<TSource, TResult> query);

    }
}