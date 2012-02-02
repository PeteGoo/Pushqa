// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Diagnostics.Contracts;

namespace Linq2Rest.Provider
{
    /// <summary>
	/// Defines the public interface for a REST client.
	/// </summary>
	[ContractClass(typeof(RestClientContracts))]
	public interface IRestClient
	{
		/// <summary>
		/// Gets the base <see cref="Uri"/> for the REST service.
		/// </summary>
		Uri ServiceBase { get; }

		/// <summary>
		/// Gets a service response.
		/// </summary>
		/// <param name="uri">The <see cref="Uri"/> to load the resource from.</param>
		/// <returns>A string representation of the resource.</returns>
		string Get(Uri uri);
	}

	[ContractClassFor(typeof(IRestClient))]
	internal abstract class RestClientContracts : IRestClient
	{
		public Uri ServiceBase
		{
			get
			{
				Contract.Ensures(Contract.Result<Uri>() != null);
				throw new NotImplementedException();
			}
		}

		public string Get(Uri uri)
		{
			Contract.Requires<ArgumentNullException>(uri != null);

			throw new NotImplementedException();
		}
	}
}