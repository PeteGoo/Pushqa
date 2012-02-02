// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace Linq2Rest.Provider
{
    internal class ParameterBuilder
	{
		private readonly Uri _serviceBase;

		public ParameterBuilder(Uri serviceBase)
		{
			Contract.Requires(serviceBase != null);

			_serviceBase = serviceBase;
			OrderByParameter = new List<string>();
		}

		public string FilterParameter { get; set; }

		public IList<string> OrderByParameter { get; private set; }

		public string SelectParameter { get; set; }

		public string SkipParameter { get; set; }

		public string TakeParameter { get; set; }

		public Uri GetFullUri()
		{
			var parameters = new List<string>();
			if (!string.IsNullOrWhiteSpace(FilterParameter))
			{
				parameters.Add("$filter=" + HttpUtility.UrlEncode(FilterParameter));
			}

			if (!string.IsNullOrWhiteSpace(SelectParameter))
			{
				parameters.Add("$select=" + SelectParameter);
			}

			if (!string.IsNullOrWhiteSpace(SkipParameter))
			{
				parameters.Add("$skip=" + SkipParameter);
			}

			if (!string.IsNullOrWhiteSpace(TakeParameter))
			{
				parameters.Add("$top=" + TakeParameter);
			}

			if (OrderByParameter.Any())
			{
				parameters.Add("$orderby=" + string.Join(",", OrderByParameter));
			}

			var builder = new UriBuilder(_serviceBase);
			builder.Query = (string.IsNullOrEmpty(builder.Query) ? string.Empty : "&") + string.Join("&", parameters);

			return builder.Uri;
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_serviceBase != null);
			Contract.Invariant(OrderByParameter != null);
		}
	}
}