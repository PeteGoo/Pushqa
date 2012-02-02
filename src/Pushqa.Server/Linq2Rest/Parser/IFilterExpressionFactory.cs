// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Linq.Expressions;

namespace Linq2Rest.Parser
{
    /// <summary>
	/// Defines the public interface for a FilterExpressionFactory.
	/// </summary>
	public interface IFilterExpressionFactory
	{
		/// <summary>
		/// Creates a filter expression from its string representation.
		/// </summary>
		/// <param name="filter">The string representation of the filter.</param>
		/// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
		/// <returns>An <see cref="Expression{TDelegate}"/> if the passed filter is valid, otherwise null.</returns>
		Expression<Func<T, bool>> Create<T>(string filter);

		/// <summary>
		/// Creates a filter expression from its string representation.
		/// </summary>
		/// <param name="filter">The string representation of the filter.</param>
		/// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when reading the filter.</param>
		/// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
		/// <returns>An <see cref="Expression{TDelegate}"/> if the passed filter is valid, otherwise null.</returns>
		Expression<Func<T, bool>> Create<T>(string filter, IFormatProvider formatProvider);
	}
}