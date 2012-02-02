// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Linq2Rest
{
    /// <summary>
	/// Defines the public interface for a model filter.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
	[ContractClass(typeof(ModelFilterContracts<>))]
	public interface IModelFilter<in T>
	{
		/// <summary>
		/// Filters the passed collection with the defined filter.
		/// </summary>
		/// <param name="source">The source items to filter.</param>
		/// <returns>A filtered enumeration and projected of the source items.</returns>
		IEnumerable<object> Filter(IEnumerable<T> source);
	}

	[ContractClassFor(typeof(IModelFilter<>))]
	internal abstract class ModelFilterContracts<T> : IModelFilter<T>
	{
		public IEnumerable<object> Filter(IEnumerable<T> source)
		{
			Contract.Requires<ArgumentNullException>(source != null);

			throw new NotImplementedException();
		}
	}
}