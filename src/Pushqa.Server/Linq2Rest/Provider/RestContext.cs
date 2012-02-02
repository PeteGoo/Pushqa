// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Linq2Rest.Provider
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class RestContext<T>
	{
		private readonly RestQueryable<T> _queryable;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestContext&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="serializerFactory">The serializer factory.</param>
		public RestContext(IRestClient client, ISerializerFactory serializerFactory)
		{
			Contract.Requires<ArgumentNullException>(client != null);
			Contract.Requires<ArgumentNullException>(serializerFactory != null);

			_queryable = new RestQueryable<T>(client, serializerFactory);
		}

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>The query.</value>
		public IQueryable<T> Query
		{
			get
			{
				Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

				return _queryable;
			}
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_queryable != null);
		}
	}
}