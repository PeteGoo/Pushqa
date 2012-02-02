// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Linq2Rest.Provider
{
    internal class RestQueryable<T> : IOrderedQueryable<T>
	{
		private readonly IRestClient _client;

		public RestQueryable(IRestClient client, ISerializerFactory serializerFactory)
		{
			Contract.Requires<ArgumentNullException>(client != null);
			Contract.Requires<ArgumentNullException>(serializerFactory != null);

			_client = client;
			Provider = new RestQueryProvider<T>(_client, serializerFactory);
			Expression = Expression.Constant(this);
		}

		public RestQueryable(IRestClient client, ISerializerFactory serializerFactory, Expression expression)
			: this(client, serializerFactory)
		{
			Contract.Requires<ArgumentNullException>(client != null);
			Contract.Requires<ArgumentNullException>(serializerFactory != null);
			Contract.Requires<ArgumentNullException>(expression != null);

			Expression = expression;
		}

		/// <summary>
		/// 	<see cref="Type"/> of T in IQueryable of T.
		/// </summary>
		public Type ElementType
		{
			get { return typeof(T); }
		}

		/// <summary>
		/// 	The expression tree.
		/// </summary>
		public Expression Expression { get; private set; }

		/// <summary>
		/// 	IQueryProvider part of RestQueryable.
		/// </summary>
		public IQueryProvider Provider { get; private set; }

		public IEnumerator<T> GetEnumerator()
		{
			return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_client != null);
			Contract.Invariant(Expression != null);
		}
	}
}