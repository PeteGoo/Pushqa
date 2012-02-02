// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Net;

namespace Linq2Rest.Provider
{
    /// <summary>
	/// Defines a REST client implementation.
	/// </summary>
	public class RestClient : IRestClient
	{
		private readonly WebClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="RestClient"/> class.
		/// </summary>
		/// <param name="uri">The base <see cref="Uri"/> for the REST service.</param>
		public RestClient(Uri uri)
		{
			_client = new WebClient();
			
			ServiceBase = uri;
		}

		/// <summary>
		/// Gets the base <see cref="Uri"/> for the REST service.
		/// </summary>
		public Uri ServiceBase { get; private set; }

		/// <summary>
		/// Gets a service response.
		/// </summary>
		/// <param name="uri">The <see cref="Uri"/> to load the resource from.</param>
		/// <returns>A string representation of the resource.</returns>
		public string Get(Uri uri)
		{
			_client.Headers["Accept"] = "application/json";

			return _client.DownloadString(uri);
		}
	}
}