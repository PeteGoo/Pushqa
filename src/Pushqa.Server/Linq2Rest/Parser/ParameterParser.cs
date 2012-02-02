// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Linq2Rest.Parser
{
    /// <summary>
	/// Defines the default implementation of a parameter parser.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> of item to create parser for.</typeparam>
	public class ParameterParser<T> : IParameterParser<T>
	{
		private readonly IFilterExpressionFactory _filterExpressionFactory;
		private readonly ISortExpressionFactory _sortExpressionFactory;
		private readonly ISelectExpressionFactory<T> _selectExpressionFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterParser{T}"/> class.
		/// </summary>
		public ParameterParser()
		{
			_filterExpressionFactory = new FilterExpressionFactory();
			_sortExpressionFactory = new SortExpressionFactory();
			_selectExpressionFactory = new SelectExpressionFactory<T>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterParser{T}"/> class.
		/// </summary>
		/// <param name="filterExpressionFactory">The <see cref="IFilterExpressionFactory"/> to use.</param>
		/// <param name="sortExpressionFactory">The <see cref="ISortExpressionFactory"/> to use.</param>
		/// <param name="selectExpressionFactory">The <see cref="ISelectExpressionFactory{T}"/> to use.</param>
		public ParameterParser(
			IFilterExpressionFactory filterExpressionFactory,
			ISortExpressionFactory sortExpressionFactory,
			ISelectExpressionFactory<T> selectExpressionFactory)
		{
			Contract.Requires<ArgumentNullException>(filterExpressionFactory != null);
			Contract.Requires<ArgumentNullException>(sortExpressionFactory != null);
			Contract.Requires<ArgumentNullException>(selectExpressionFactory != null);

			_filterExpressionFactory = filterExpressionFactory;
			_sortExpressionFactory = sortExpressionFactory;
			_selectExpressionFactory = selectExpressionFactory;
		}

		/// <summary>
		/// Parses the passes query parameters to a <see cref="ModelFilter{T}"/>.
		/// </summary>
		/// <param name="queryParameters"></param>
		/// <returns></returns>
		public IModelFilter<T> Parse(NameValueCollection queryParameters)
		{
			var orderbyField = queryParameters["$orderby"];
			var selects = queryParameters["$select"];
			var filter = queryParameters["$filter"];
			var skip = queryParameters["$skip"];
			var top = queryParameters["$top"];

			var filterExpression = _filterExpressionFactory.Create<T>(filter);
			var sortDescriptions = _sortExpressionFactory.Create<T>(orderbyField);
			var selectFunction = _selectExpressionFactory.Create(selects);

			var modelFilter = new ModelFilter<T>(
				filterExpression,
				selectFunction,
				sortDescriptions,
				string.IsNullOrWhiteSpace(skip) ? -1 : Convert.ToInt32(skip),
				string.IsNullOrWhiteSpace(top) ? -1 : Convert.ToInt32(top));
			return modelFilter;
		}
	}
}