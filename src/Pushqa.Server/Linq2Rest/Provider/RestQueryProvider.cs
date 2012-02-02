// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Linq2Rest.Provider
{
    internal class RestQueryProvider<T> : IQueryProvider
	{
		private readonly IRestClient _client;
		private readonly ISerializerFactory _serializerFactory;
		private readonly ParameterBuilder _parameterBuilder;

		public RestQueryProvider(IRestClient client, ISerializerFactory serializerFactory)
		{
			Contract.Requires<ArgumentNullException>(client != null);
			Contract.Requires<ArgumentNullException>(serializerFactory != null);

			_client = client;
			_serializerFactory = serializerFactory;
			_parameterBuilder = new ParameterBuilder(client.ServiceBase);
		}

		public IQueryable CreateQuery(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			return new RestQueryable<T>(_client, _serializerFactory, expression);
		}

		public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			return new RestQueryable<TResult>(_client, _serializerFactory, expression);
		}

		public object Execute(Expression expression)
		{
			Contract.Assume(expression != null);

			var methodCallExpression = expression as MethodCallExpression;

			return (methodCallExpression != null
					? methodCallExpression.ProcessMethodCall(_parameterBuilder, GetResults)
					: expression.ProcessExpression())
					?? GetResults(_parameterBuilder);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			Contract.Assume(expression != null);
			return (TResult)Execute(expression);
		}

		private IList<T> GetResults(ParameterBuilder builder)
		{
			Contract.Requires(builder != null);
			Contract.Ensures(Contract.Result<IList<T>>() != null);

			var response = _client.Get(builder.GetFullUri());

			var serializer = _serializerFactory.Create<T>();

			var resultSet = serializer.DeserializeList(response);

			Contract.Assume(resultSet != null);

			return resultSet;
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_client != null);
			Contract.Invariant(_serializerFactory != null);
		}
	}
}