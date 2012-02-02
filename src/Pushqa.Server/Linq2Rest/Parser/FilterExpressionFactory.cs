// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Linq2Rest.Parser
{
    internal class FilterExpressionFactory : IFilterExpressionFactory
	{
		private static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");
		private static readonly Regex StringRx = new Regex(@"^[""']([^""']*?)[""']$", RegexOptions.Compiled);
		private static readonly Regex FunctionRx = new Regex(@"^([^\(\)]+)\((.+)\)$", RegexOptions.Compiled);
		private static readonly Regex FunctionContentRx = new Regex(@"^(.*\((?>[^()]+|\((?<Depth>.*)|\)(?<-Depth>.*))*(?(Depth)(?!))\)|.*?)\s*,\s*(.+)$", RegexOptions.Compiled);
		private static readonly Regex NewRx = new Regex(@"^new (?<type>[^\(\)]+)\((?<parameters>.*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly ConcurrentDictionary<Type, MethodInfo> ParseMethods = new ConcurrentDictionary<Type, MethodInfo>();

		public Expression<Func<T, bool>> Create<T>(string filter)
		{
			return Create<T>(filter, DefaultCulture);
		}

		public Expression<Func<T, bool>> Create<T>(string filter, IFormatProvider formatProvider)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				return x => true;
			}

			var parameter = Expression.Parameter(typeof(T), "x");

			var expression = CreateExpression<T>(filter, parameter, null, formatProvider);

			return expression == null ? x => true : Expression.Lambda<Func<T, bool>>(expression, parameter);
		}

		private static TokenSet GetFunctionTokens(string filter)
		{
			Contract.Requires(filter != null);

			var functionMatch = FunctionRx.Match(filter);
			if (!functionMatch.Success)
			{
				return null;
			}

			var functionName = functionMatch.Groups[1].Value;
			var functionContent = functionMatch.Groups[2].Value;
			var functionContentMatch = FunctionContentRx.Match(functionContent);
			if (!functionContentMatch.Success)
			{
				return new FunctionTokenSet
				{
					Operation = functionName,
					Left = functionContent
				};
			}

			return new FunctionTokenSet
			{
				Operation = functionName,
				Left = functionContentMatch.Groups[1].Value,
				Right = functionContentMatch.Groups[2].Value
			};
		}

		private static string[] GetConstructorTokens(string filter)
		{
			Contract.Requires(filter != null);

			var constructorMatch = NewRx.Match(filter);
			if (!constructorMatch.Success)
			{
				return null;
			}

			var matchGroup = constructorMatch.Groups["parameters"];

			Contract.Assume(matchGroup != null, "Otherwise match is not success.");

			var constructorContent = matchGroup.Value;
			return constructorContent.Split(',').Select(x => x.Trim().Trim(')', '(')).ToArray();
		}

		private static Type GetFunctionParameterType(string operation)
		{
			Contract.Requires(operation != null);

			switch (operation.ToLowerInvariant())
			{
				case "substring":
					return typeof(int);
				default:
					return null;
			}
		}

		private static Expression GetPropertyExpression<T>(string propertyToken, ParameterExpression parameter)
		{
			Contract.Requires(propertyToken != null);

			if (!propertyToken.IsImpliedBoolean())
			{
				var token = propertyToken.GetTokens().FirstOrDefault();
				if (token != null)
				{
					return GetPropertyExpression<T>(token.Left, parameter) ?? GetPropertyExpression<T>(token.Right, parameter);
				}
			}

			var parentType = typeof(T);
			Expression propertyExpression = null;

			var propertyChain = propertyToken.Split('/');
			foreach (var propertyName in propertyChain)
			{
				var property = parentType.GetProperty(propertyName);
				if (property != null)
				{
					parentType = property.PropertyType;
					propertyExpression = propertyExpression == null
					? Expression.Property(parameter, property)
					: Expression.Property(propertyExpression, property);
				}
			}

			return propertyExpression;
		}

		private static Type GetExpressionType<T>(TokenSet set, ParameterExpression parameter)
		{
			Contract.Requires(set != null);

			if (Regex.IsMatch(set.Left, @"^\(.*\)$") && set.Operation.IsCombinationOperation())
			{
				return null;
			}

			var property = GetPropertyExpression<T>(set.Left, parameter) ?? GetPropertyExpression<T>(set.Right, parameter);

			return property == null ? null : property.Type;
		}

		private static Expression GetOperation(string token, Expression left, Expression right)
		{
			Contract.Requires(token != null);
			Contract.Requires(right != null);

			token = token.ToLowerInvariant();

			if (string.Equals("not", token, StringComparison.OrdinalIgnoreCase))
			{
				return GetRightOperation(token, right);
			}

			Contract.Assume(left != null);

			return GetLeftRightOperation(token, left, right);
		}

		private static Expression GetLeftRightOperation(string token, Expression left, Expression right)
		{
			Contract.Requires(token != null);
			Contract.Requires(left != null);
			Contract.Requires(right != null);

			switch (token.ToLowerInvariant())
			{
				case "eq":
					if (left.Type.IsEnum && left.Type.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
					{
						var underlyingType = Enum.GetUnderlyingType(left.Type);
						var leftValue = Expression.Convert(left, underlyingType);
						var rightValue = Expression.Convert(right, underlyingType);
						var andExpression = Expression.And(leftValue, rightValue);
						return Expression.Equal(andExpression, rightValue);
					}

					return Expression.Equal(left, right);
				case "ne":
					return Expression.NotEqual(left, right);
				case "gt":
					return Expression.GreaterThan(left, right);
				case "ge":
					return Expression.GreaterThanOrEqual(left, right);
				case "lt":
					return Expression.LessThan(left, right);
				case "le":
					return Expression.LessThanOrEqual(left, right);
				case "and":
					return Expression.AndAlso(left, right);
				case "or":
					return Expression.OrElse(left, right);
				case "add":
					return Expression.Add(left, right);
				case "sub":
					return Expression.Subtract(left, right);
				case "mul":
					return Expression.Multiply(left, right);
				case "div":
					return Expression.Divide(left, right);
				case "mod":
					return Expression.Modulo(left, right);
			}

			throw new InvalidOperationException("Unsupported operation");
		}

		private static Expression GetRightOperation(string token, Expression right)
		{
			Contract.Requires(token != null);
			Contract.Requires(right != null);

			switch (token.ToLowerInvariant())
			{
				case "not":
					return Expression.Not(right);
			}

			throw new InvalidOperationException("Unsupported operation");
		}

		private static Expression GetFunction(string function, Expression left, Expression right)
		{
			Contract.Requires(function != null);

			switch (function.ToLowerInvariant())
			{
				case "substringof":
					return Expression.Call(right, MethodProvider.ContainsMethod, new[] { left });
				case "endswith":
					return Expression.Call(left, MethodProvider.EndsWithMethod, new[] { right, MethodProvider.IgnoreCaseExpression });
				case "startswith":
					return Expression.Call(left, MethodProvider.StartsWithMethod, new[] { right, MethodProvider.IgnoreCaseExpression });
				case "length":
					return Expression.Property(left, MethodProvider.LengthProperty);
				case "indexof":
					return Expression.Call(left, MethodProvider.IndexOfMethod, new[] { right, MethodProvider.IgnoreCaseExpression });
				case "substring":
					return Expression.Call(left, MethodProvider.SubstringMethod, new[] { right });
				case "tolower":
					return Expression.Call(left, MethodProvider.ToLowerMethod);
				case "toupper":
					return Expression.Call(left, MethodProvider.ToUpperMethod);
				case "trim":
					return Expression.Call(left, MethodProvider.TrimMethod);
				case "hour":
					return Expression.Property(left, MethodProvider.HourProperty);
				case "minute":
					return Expression.Property(left, MethodProvider.MinuteProperty);
				case "second":
					return Expression.Property(left, MethodProvider.SecondProperty);
				case "day":
					return Expression.Property(left, MethodProvider.DayProperty);
				case "month":
					return Expression.Property(left, MethodProvider.MonthProperty);
				case "year":
					return Expression.Property(left, MethodProvider.YearProperty);
				case "round":
					Contract.Assume(left != null);

					return Expression.Call(left.Type == typeof(double) ? MethodProvider.DoubleRoundMethod : MethodProvider.DecimalRoundMethod, left);
				case "floor":
					Contract.Assume(left != null);

					return Expression.Call(left.Type == typeof(double) ? MethodProvider.DoubleFloorMethod : MethodProvider.DecimalFloorMethod, left);
				case "ceiling":
					Contract.Assume(left != null);

					return Expression.Call(left.Type == typeof(double) ? MethodProvider.DoubleCeilingMethod : MethodProvider.DecimalCeilingMethod, left);
				default:
					return null;
			}
		}

		private static Expression GetKnownConstant(string token, Type type, IFormatProvider formatProvider)
		{
			Contract.Requires(token != null);

			if (string.Equals(token, "null", StringComparison.OrdinalIgnoreCase))
			{
				return Expression.Constant(null);
			}

			if (type == typeof(Guid) || type == typeof(Guid?))
			{
				return
					string.Equals(token, "newguid()", StringComparison.OrdinalIgnoreCase)
						? Expression.Convert(Expression.Constant(Guid.NewGuid()), type)
						: string.Equals(token, "empty", StringComparison.OrdinalIgnoreCase)
							? Expression.Convert(Expression.Constant(Guid.Empty), type)
							: Expression.Convert(Expression.Constant(Guid.Parse(token)), type);
			}

			if (type != null)
			{
				var parseMethod = ParseMethods.GetOrAdd(type, ResolveParseMethod);
				if (parseMethod != null)
				{
					var parseResult = parseMethod.Invoke(null, new object[] { token, formatProvider });
					return Expression.Constant(parseResult);
				}

				if (type.IsEnum)
				{
					var enumValue = Enum.Parse(type, token, true);
					return Expression.Constant(enumValue);
				}

				if (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
				{
					var genericTypeArgument = type.GetGenericArguments().First();
					var value = GetKnownConstant(token, genericTypeArgument, formatProvider);
					if (value != null)
					{
						return Expression.Convert(value, type);
					}
				}
			}

			return null;
		}

		private static MethodInfo ResolveParseMethod(Type type)
		{
			Contract.Requires(type != null);

			return type.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(x => x.Name == "Parse" && x.GetParameters().Length == 2)
				.Where(x => x.GetParameters().First().ParameterType == typeof(string) && x.GetParameters().ElementAt(1).ParameterType == typeof(IFormatProvider))
				.FirstOrDefault();
		}

		private Expression CreateExpression<T>(string filter, ParameterExpression parameter, Type type, IFormatProvider formatProvider)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				return null;
			}

			var tokens = filter.GetTokens().ToArray();

			Expression existing = null;
			string combiner = null;

			if (tokens.Any())
			{
				foreach (var tokenSet in tokens)
				{
					if (string.IsNullOrWhiteSpace(tokenSet.Left))
					{
						if (string.Equals(tokenSet.Operation, "not", StringComparison.OrdinalIgnoreCase))
						{
							var right = CreateExpression<T>(tokenSet.Right, parameter, type ?? GetExpressionType<T>(tokenSet, parameter), formatProvider);

							if (right == null)
							{
								return null;
							}

							return GetOperation(tokenSet.Operation, null, right);
						}

						combiner = tokenSet.Operation;
					}
					else
					{
						var left = CreateExpression<T>(tokenSet.Left, parameter, type ?? GetExpressionType<T>(tokenSet, parameter), formatProvider);
						var right = CreateExpression<T>(tokenSet.Right, parameter, left.Type, formatProvider);

						if (existing != null && !string.IsNullOrWhiteSpace(combiner))
						{
							var current = right == null ? null : GetOperation(tokenSet.Operation, left, right);
							existing = GetOperation(combiner, existing, current ?? left);
						}
						else if (right != null)
						{
							existing = GetOperation(tokenSet.Operation, left, right);
						}
					}
				}

				return existing;
			}

			Expression expression = null;
			var stringMatch = StringRx.Match(filter);

			if (stringMatch.Success)
			{
				expression = Expression.Constant(stringMatch.Groups[1].Value, typeof(string));
			}

			if (expression == null)
			{
				expression = GetConstructorExpression<T>(filter, parameter, type, formatProvider);
			}

			if (expression == null)
			{
				expression = GetFunctionExpression<T>(filter, parameter, type, formatProvider);
			}

			if (expression == null)
			{
				expression = GetPropertyExpression<T>(filter, parameter);
			}

			if (expression == null)
			{
				expression = GetKnownConstant(filter, type, formatProvider);
			}

			if (expression == null)
			{
				Contract.Assume(type != null);

				expression = Expression.Constant(Convert.ChangeType(filter, type, formatProvider), type);
			}

			return expression;
		}

		private Expression GetConstructorExpression<T>(string filter, ParameterExpression parameter, Type resultType, IFormatProvider formatProvider)
		{
			Contract.Requires(filter != null);

			var newMatch = NewRx.Match(filter);
			if (newMatch.Success)
			{
				var matchGroup = newMatch.Groups["type"];

				Contract.Assume(matchGroup != null, "Otherwise match is not success.");

				var typeName = matchGroup.Value;
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				var type = assemblies
					.SelectMany(x => x.GetTypes().Where(t => t.Name == typeName))
					.FirstOrDefault();

				if (type == null)
				{
					return null;
				}

				var constructorTokens = GetConstructorTokens(filter);

				var constructorInfos = type.GetConstructors().Where(x => x.GetParameters().Length == constructorTokens.Length);
				foreach (var constructorInfo in constructorInfos)
				{
					try
					{
						var parameterExpressions = constructorInfo
							.GetParameters()
							.Select((p, i) => CreateExpression<T>(constructorTokens[i], parameter, p.ParameterType, formatProvider))
							.ToArray();

						if (resultType == null)
						{
							throw new ArgumentNullException("resultType");
						}

						return Expression.Convert(Expression.New(constructorInfo, parameterExpressions), resultType);
					}
					catch
					{
					}
				}
			}

			return null;
		}

		private Expression GetFunctionExpression<T>(string filter, ParameterExpression parameter, Type type, IFormatProvider formatProvider)
		{
			Contract.Requires(filter != null);

			var functionTokens = GetFunctionTokens(filter);
			if (functionTokens == null)
			{
				return null;
			}

			var left = CreateExpression<T>(
				functionTokens.Left,
				parameter,
				type ?? GetExpressionType<T>(functionTokens, parameter),
				formatProvider);

			var right = CreateExpression<T>(functionTokens.Right, parameter, GetFunctionParameterType(functionTokens.Operation) ?? left.Type, formatProvider);

			return GetFunction(functionTokens.Operation, left, right);
		}
	}
}