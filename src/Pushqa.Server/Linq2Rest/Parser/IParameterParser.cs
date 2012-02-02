// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections.Specialized;

namespace Linq2Rest.Parser
{
    /// <summary>
	/// Defines the public interface for a parameter parser.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IParameterParser<in T>
	{
		/// <summary>
		/// Parses the passes parameters into a <see cref="ModelFilter{T}"/>.
		/// </summary>
		/// <param name="queryParameters">The parameters to parse.</param>
		/// <returns>A <see cref="ModelFilter{T}"/> representing the restrictions in the parameters.</returns>
		IModelFilter<T> Parse(NameValueCollection queryParameters);
	}
}