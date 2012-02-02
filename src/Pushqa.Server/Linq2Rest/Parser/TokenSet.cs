// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Diagnostics.Contracts;

namespace Linq2Rest.Parser
{
    internal class TokenSet
	{
		private string _left;
		private string _right;
		private string _operation;

		public TokenSet()
		{
			_left = string.Empty;
			_right = string.Empty;
			_operation = string.Empty;
		}

		public string Left
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return _left;
			}

			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				_left = value;
			}
		}

		public string Operation
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return _operation;
			}

			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				_operation = value;
			}
		}

		public string Right
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return _right;
			}

			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				_right = value;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", Left, Operation, Right);
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(_left != null);
			Contract.Invariant(_right != null);
			Contract.Invariant(_operation != null);
		}
	}
}