// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections.Generic;

namespace Linq2Rest.Provider
{
    /// <summary>
	/// Defines the public interface for an object serializer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISerializer<T>
	{
		/// <summary>
		/// Deserializes a single item.
		/// </summary>
		/// <param name="input">The serialized item.</param>
		/// <returns>An instance of the serialized item.</returns>
		T Deserialize(string input);

		/// <summary>
		/// Deserializes a list of items.
		/// </summary>
		/// <param name="input">The serialized items.</param>
		/// <returns>An list of the serialized items.</returns>
		IList<T> DeserializeList(string input);
	}
}