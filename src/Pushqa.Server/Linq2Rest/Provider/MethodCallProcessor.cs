// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Linq2Rest.Provider
{
    internal static class MethodCallProcessor
	{
		public static object ProcessMethodCall<T>(this MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IList<T>> resultLoader)
		{
			Contract.Requires(builder != null);
			Contract.Requires(resultLoader != null);

			if (methodCall == null)
			{
				return null;
			}

			var method = methodCall.Method.Name;

			switch (method)
			{
				case "Where":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.FilterParameter = methodCall.Arguments[1].ProcessExpression();
					break;
				case "Select":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					var unaryExpression = methodCall.Arguments[1] as UnaryExpression;
					if (unaryExpression != null)
					{
						var lambdaExpression = unaryExpression.Operand as LambdaExpression;
						if (lambdaExpression != null)
						{
							var selectFunction = lambdaExpression.Body as NewExpression;

							if (selectFunction != null)
							{
								var members = selectFunction.Members.Select(x => x.Name).ToArray();
								var args = selectFunction.Arguments.OfType<MemberExpression>().Select(x => x.Member.Name).ToArray();
								if (members.Intersect(args).Count() != members.Length)
								{
									throw new InvalidOperationException("Projection into new member names is not supported.");
								}

								builder.SelectParameter = string.Join(",", args);
							}
						}
					}

					break;
				case "OrderBy":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.OrderByParameter.Add(methodCall.Arguments[1].ProcessExpression());
					break;
				case "OrderByDescending":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.OrderByParameter.Add(methodCall.Arguments[1].ProcessExpression() + " desc");
					break;
				case "ThenBy":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.OrderByParameter.Add(methodCall.Arguments[1].ProcessExpression());
					break;
				case "ThenByDescending":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.OrderByParameter.Add(methodCall.Arguments[1].ProcessExpression() + " desc");
					break;
				case "Take":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.TakeParameter = methodCall.Arguments[1].ProcessExpression();
					break;
				case "Skip":
					Contract.Assume(methodCall.Arguments.Count >= 2);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					builder.SkipParameter = methodCall.Arguments[1].ProcessExpression();
					break;
				default:
					Contract.Assume(methodCall.Arguments.Count >= 1);

					ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader);
					var results = resultLoader(builder);

					Contract.Assume(results != null);

					var parameters = new object[] { results.AsQueryable() }
						.Concat(methodCall.Arguments.Where((x, i) => i > 0).Select(x => x.GetExpressionValue()))
						.ToArray();

					var final = methodCall.Method.Invoke(null, parameters);
					return final;
			}

			return null;
		}
	}
}