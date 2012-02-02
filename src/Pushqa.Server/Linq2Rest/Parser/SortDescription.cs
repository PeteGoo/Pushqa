// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Diagnostics.Contracts;
using System.Web.UI.WebControls;

namespace Linq2Rest.Parser
{
    /// <summary>
	/// Defines a sort description.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to sort.</typeparam>
	public class SortDescription<T>
	{
		private readonly Func<T, object> _keySelector;
		private readonly SortDirection _direction;

		/// <summary>
		/// Initializes a new instance of the <see cref="SortDescription{T}"/> class.
		/// </summary>
		/// <param name="keySelector">The function to select the sort key.</param>
		/// <param name="direction">The sort direction.</param>
		public SortDescription(Func<T, object> keySelector, SortDirection direction)
		{
			Contract.Requires<ArgumentNullException>(keySelector != null);

			_keySelector = keySelector;
			_direction = direction;
		}

		/// <summary>
		/// Gets the sort direction.
		/// </summary>
		public SortDirection Direction
		{
			get { return _direction; }
		}

		/// <summary>
		/// Gets the key to sort by.
		/// </summary>
		public Func<T, object> KeySelector
		{
			get { return _keySelector; }
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_keySelector != null);
		}
	}
}