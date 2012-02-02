// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Linq2Rest.Parser
{
    internal static class ExpressionTokenizer
	{
		public static IEnumerable<TokenSet> GetTokens(this string expression)
		{
			var cleanMatch = expression.EnclosedMatch();

			if (cleanMatch.Success)
			{
				var match = cleanMatch.Groups[1].Value;
				if (!HasOrphanedOpenParenthesis(match))
				{
					expression = match;
				}
			}

			if (expression.IsImpliedBoolean())
			{
				yield break;
			}

			var blocks = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var openGroups = 0;
			var startExpression = 0;
			var currentTokens = new TokenSet();

			for (int i = 0; i < blocks.Length; i++)
			{
				var netEnclosed = blocks[i].Count(c => c == '(') - blocks[i].Count(c => c == ')');
				openGroups += netEnclosed;

				if (openGroups == 0)
				{
					if (blocks[i].IsOperation())
					{
						var expression1 = startExpression;
						Func<string, int, bool> predicate = (x, j) => j >= expression1 && j < i;

						if (string.IsNullOrWhiteSpace(currentTokens.Left))
						{
							currentTokens.Left = string.Join(" ", blocks.Where(predicate));
							currentTokens.Operation = blocks[i];
							startExpression = i + 1;

							if (blocks[i].IsCombinationOperation())
							{
								currentTokens.Right = string.Join(" ", blocks.Where((x, j) => j > i));

								yield return currentTokens;
								yield break;
							}
						}
						else
						{
							currentTokens.Right = string.Join(" ", blocks.Where(predicate));

							yield return currentTokens;

							startExpression = i + 1;
							currentTokens = new TokenSet();

							if (blocks[i].IsCombinationOperation())
							{
								yield return new TokenSet { Operation = blocks[i].ToLowerInvariant() };
							}
						}
					}
				}
			}

			var remainingToken = string.Join(" ", blocks.Where((x, j) => j >= startExpression));

			if (!string.IsNullOrWhiteSpace(currentTokens.Left))
			{
				currentTokens.Right = remainingToken;
				yield return currentTokens;
			}
			else if (remainingToken.IsEnclosed())
			{
				currentTokens.Left = remainingToken;
				yield return currentTokens;
			}
		}

		private static bool HasOrphanedOpenParenthesis(string expression)
		{
			Contract.Requires(expression != null);

			var opens = new List<int>();
			var closes = new List<int>();
			var index = expression.IndexOf('(');
			while (index > -1)
			{
				opens.Add(index);
				index = expression.IndexOf('(', index + 1);
			}

			index = expression.IndexOf(')');
			while (index > -1)
			{
				closes.Add(index);
				index = expression.IndexOf(')', index + 1);
			}

			var pairs = opens.Zip(closes, (o, c) => new Tuple<int, int>(o, c));
			var hasOrphan = opens.Count == closes.Count && pairs.Any(x => x.Item2 < x.Item1);

			return hasOrphan;
		}
	}
}
