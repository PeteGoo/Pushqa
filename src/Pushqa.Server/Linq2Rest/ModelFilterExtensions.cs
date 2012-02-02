// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using Linq2Rest.Parser;

namespace Linq2Rest
{
    /// <summary>
	/// Defines extension methods for model filters.
	/// </summary>
	public static class ModelFilterExtensions
	{
		/// <summary>
		/// Filters the source collection using the passed query parameters.
		/// </summary>
		/// <param name="source">The source items to filter.</param>
		/// <param name="query">The query parameters defining the filter.</param>
		/// <typeparam name="T">The <see cref="Type"/> of items in the source collection.</typeparam>
		/// <returns>A filtered and projected enumeration of the source collection.</returns>
		public static IEnumerable<object> Filter<T>(this IEnumerable<T> source, NameValueCollection query)
		{
			var parser = new ParameterParser<T>();

			return Filter(source, parser.Parse(query));
		}

		/// <summary>
		/// Filters the source collection using the passed query parameters.
		/// </summary>
		/// <param name="source">The source items to filter.</param>
		/// <param name="filter">The filter to apply.</param>
		/// <typeparam name="T">The <see cref="Type"/> of items in the source collection.</typeparam>
		/// <returns>A filtered and projected enumeration of the source collection.</returns>
		public static IEnumerable<object> Filter<T>(this IEnumerable<T> source, IModelFilter<T> filter)
		{
			Contract.Requires<ArgumentNullException>(source != null);

			return filter == null ? source.OfType<object>() : filter.Filter(source);
		}
	}
}