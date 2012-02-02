// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

namespace Linq2Rest.Parser
{
	internal class FunctionTokenSet : TokenSet
	{
		public override string ToString()
		{
			return string.Format("{0} {1} {2}", Operation, Left, Right);
		}
	}
}