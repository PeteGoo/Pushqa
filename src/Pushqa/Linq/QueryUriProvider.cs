using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Pushqa.Infrastructure;

namespace Pushqa.Linq {
    /// <summary>
    /// Implements the OData implementation of getting a query string from an EventQuery
    /// </summary>
    public class QueryUriProvider : IQueryUriProvider {
        /// <summary>
        /// Gets the query URI.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public Uri GetQueryUri<TSource, TResult>([NotNull]EventQuery<TSource, TResult> query) {
            UriBuilder uriBuilder = new UriBuilder(string.Format("{0}/{1}", query.Source.BaseUri.ToString().TrimEnd('/'), query.Source.EventResourceName));

            IDictionary<string, string> queryParts = GetQueryUriComponents(query);

            uriBuilder.Query = string.Join("&", queryParts.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)).ToArray());

            return uriBuilder.Uri;
        }

        private IDictionary<string, string> GetQueryUriComponents<TSource, TResult>(EventQuery<TSource, TResult> query) {
            
            VerifyArgument.IsNotNull("query", query);

            string filterQueryValue = string.Empty;
            if (query.Filter != null) {
                filterQueryValue = VisitFilter(query.Filter.Body, query.Filter.Parameters[0], false, false);
            }

            Dictionary<string, string> queryParts = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filterQueryValue)) {
                queryParts.Add("$filter", filterQueryValue);
            }
            if (query.Skip > 0) {
                queryParts.Add("$skip", query.Skip.ToString(CultureInfo.InvariantCulture));
            }
            if (query.Top > 0) {
                queryParts.Add("$top", query.Top.ToString(CultureInfo.InvariantCulture));
            }

            return queryParts;
        }


        /// <summary>
        /// Recursive oData filter translation function.
        /// </summary>
        /// <param name="expression">Expression to be translated.</param>
        /// <param name="parameter">Parameter expression referring to the event class entity being filtered.</param>
        /// <param name="isLeft">if set to <c>true</c> [is left].</param>
        /// <param name="bracketRequired">if set to <c>true</c> [bracket required].</param>
        /// <returns>oData filter for the given expression.</returns>
        private static string VisitFilter(Expression expression, ParameterExpression parameter, bool isLeft, bool bracketRequired) {
            Func<string, string> format = bracketRequired ? (x => string.Format("({0})", x)) : new Func<string, string>(x => x);
            switch (expression.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso: {
                    var be = (BinaryExpression)expression;
                    string left = VisitFilter(be.Left, parameter, true, false);
                    string right = VisitFilter(be.Right, parameter, false, false);
                    if (right == "false" || left == "false") {
                        return string.Empty;
                    }
                    if (left == "true" || string.IsNullOrWhiteSpace(left)) {
                        return right;
                    }
                    if (right == "true" || string.IsNullOrWhiteSpace(right)) {
                        return left;
                    }
                    return format(string.Format(CultureInfo.InvariantCulture, "({0}) and ({1})", left, right));
                }
                case ExpressionType.Or:
                case ExpressionType.OrElse: {
                    var be = (BinaryExpression)expression;
                    string left = VisitFilter(be.Left, parameter, true, false);
                    string right = VisitFilter(be.Right, parameter, false, false);
                    if (string.IsNullOrWhiteSpace(left)) {
                        return right;
                    }
                    if (string.IsNullOrWhiteSpace(right)) {
                        return left;
                    }
                    return format(string.Format(CultureInfo.InvariantCulture, "({0}) or ({1})", left, right));
                }
                case ExpressionType.Not: {
                    var ue = (UnaryExpression)expression;
                    return format(string.Format(CultureInfo.InvariantCulture, "not ({0})", VisitFilter(ue.Operand, parameter, false, false)));
                }
                case ExpressionType.LessThan:
                    return VisitFilter("lt", expression, parameter);
                case ExpressionType.LessThanOrEqual:
                    return VisitFilter("le", expression, parameter);
                case ExpressionType.GreaterThan:
                    return VisitFilter("gt", expression, parameter);
                case ExpressionType.GreaterThanOrEqual:
                    return VisitFilter("ge", expression, parameter);
                case ExpressionType.Equal:
                    return VisitFilter("eq", expression, parameter);
                case ExpressionType.NotEqual:
                    return VisitFilter("ne", expression, parameter);
                case ExpressionType.Add:
                    return format(VisitFilter("add", expression, parameter));
                case ExpressionType.Subtract:
                    return format(VisitFilter("sub", expression, parameter));
                case ExpressionType.Multiply:
                    return format(VisitFilter("mul", expression, parameter));
                case ExpressionType.Divide:
                    return format(VisitFilter("div", expression, parameter));
                case ExpressionType.Modulo:
                    return format(VisitFilter("mod", expression, parameter));
                case ExpressionType.Call:
                    return VisitFilterCall(expression, parameter, isLeft);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    if (Nullable.GetUnderlyingType(expression.Type) == ((UnaryExpression)expression).Operand.Type) {
                        return VisitFilterOperand(((UnaryExpression) expression).Operand, parameter, false);
                    }
                    throw new NotImplementedException("cast is not currently supported");
                case ExpressionType.MemberAccess:
                    return VisitFilterOperand(expression, parameter, true);
                default:
                    throw new InvalidOperationException(string.Format("Unsupported query expression encountered - {0}.", expression.NodeType));
            }
        }

        

        /// <summary>
        /// Helper function to visit a binary operator used in a filter and return its oData representation.
        /// </summary>
        /// <param name="op">Symbolic infix operator representation.</param>
        /// <param name="expression">Expression to be translated.</param>
        /// <param name="parameter">Parameter expression referring to the event class entity being filtered.</param>
        /// <returns>oData filter for the given expression.</returns>
        private static string VisitFilter(string op, Expression expression, ParameterExpression parameter) {
            var be = (BinaryExpression)expression;

            string left = VisitFilterOperand(be.Left, parameter, true);

            string right = VisitFilterOperand(be.Right, parameter, false);

            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", left, op, right);
        }

        /// <summary>
        /// Helper function to visit a filter operand.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="isLeft">if set to <c>true</c> [is left].</param>
        /// <returns></returns>
        private static string VisitFilterOperand(Expression expression, ParameterExpression parameter, bool isLeft) {
            return ExpressionIsMemberOrLiteral(expression)
                ? VisitFilterOperandValue(expression, parameter).ToString()
                : string.Format("{0}", VisitFilter(expression, parameter, isLeft, true));
        }

        /// <summary>
        /// Determines if an expression is either a member or a literal, if so returns true, otherwise;false
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>true if the expression is a member expression or a literal</returns>
        private static bool ExpressionIsMemberOrLiteral(Expression expression) {
            return expression is ConstantExpression || expression is MemberExpression;
        }

        /// <summary>
        /// Helper function to visit a filter operator's operands and return the containing member name or value.
        /// </summary>
        /// <param name="operand">Expression of the operand to be analyzed.</param>
        /// <param name="parameter">Parameter expression referring to the event class entity being filtered.</param>
        /// <returns>Member of value union type with information extracted from the operand expression.</returns>
        private static MemberOrValue VisitFilterOperandValue(Expression operand, ParameterExpression parameter) {

            var ce = operand as ConstantExpression;
            if (ce != null)
                return new MemberOrValue { Value = ce.Value, HasValue = true };

            var me = operand as MemberExpression;
            // If the root of the member expression is our parameter then we pass it as a query on an item
            if (me != null && GetRootParameterExpression(me) == parameter)
                return new MemberOrValue { MemberPath = GetParameterPath(me), HasValue = false };

            // Otherwise figure out the value and pass it on.
            var val = Expression.Lambda(operand).Compile().DynamicInvoke();
            return new MemberOrValue { Value = val, HasValue = true };
        }

        /// <summary>
        /// Visits the filter call.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="isLeft"> </param>
        /// <returns></returns>
        private static string VisitFilterCall(Expression expression, ParameterExpression parameter, bool isLeft) {
            MethodCallExpression mce = expression as MethodCallExpression;
            if (mce != null) {
                if (mce.Method.Name == "Contains" && mce.Method.DeclaringType == typeof(string) && mce.Arguments.Count == 1) {
                    string left = VisitFilterOperand(mce.Object, parameter, true);
                    string right = VisitFilterOperand(mce.Arguments[0], parameter, false);
                    return string.Format("substringof({0}, {1}){2}", right, left, isLeft ? "" : " eq true");
                }
                if (mce.Method.Name == "EndsWith" && mce.Method.DeclaringType == typeof(string) && mce.Arguments.Count >= 1) {
                    string left = VisitFilterOperand(mce.Object, parameter, true);
                    string right = VisitFilterOperand(mce.Arguments[0], parameter, false);
                    return string.Format("endswith({0}, {1}){2}", left, right, isLeft ? "" : " eq true");
                }
                if (mce.Method.Name == "StartsWith" && mce.Method.DeclaringType == typeof(string) && mce.Arguments.Count >= 1) {
                    string left = VisitFilterOperand(mce.Object, parameter, true);
                    string right = VisitFilterOperand(mce.Arguments[0], parameter, false);
                    return string.Format("startswith({0}, {1}){2}", left, right, isLeft ? "" : " eq true");
                }

                throw new InvalidOperationException(string.Format("Unsupported method call encountered in query expression. {0}.{1}", mce.Method.DeclaringType, mce.Method));
            }

            throw new InvalidOperationException("Unsupported query expression encountered.");
        }

        private static ParameterExpression GetRootParameterExpression(MemberExpression memberExpression) {
            Expression currentExpression = memberExpression;
            while (currentExpression is MemberExpression) {
                currentExpression = ((MemberExpression) currentExpression).Expression;
            }
            return currentExpression as ParameterExpression;
        }

        private static string GetParameterPath(MemberExpression memberExpression) {
            List<string> paths = new List<string>();
            Expression currentExpression = memberExpression;
            
            while (currentExpression is MemberExpression) {
                paths.Add(((MemberExpression)currentExpression).Member.Name);
                currentExpression = ((MemberExpression)currentExpression).Expression;
            }

            paths.Reverse();
            return string.Join("/", paths);
        }

        /// <summary>
        /// Union type representing a member reference or a value.
        /// </summary>
        class MemberOrValue {
            /// <summary>
            /// Gets or sets a member info.
            /// </summary>
            public string MemberPath { get; set; }

            /// <summary>
            /// Gets or sets whether the object contains a value.
            /// If set to false, Member should be supplied. If set to true, Value should be supplied.
            /// </summary>
            public bool HasValue { get; set; }

            /// <summary>
            /// Gets or sets a value.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Returns a oData string representation of the member (column) reference or the value.
            /// </summary>
            /// <returns></returns>
            public override string ToString() {
                if (HasValue) {
                    if (Value == null)
                        return "null";
                    if (Value is bool)
                        return (bool)Value ? "true" : "false";
                    if (Value is string)
                        return "\'" + Value + "\'";
                    if (Value is int || Value is uint || Value is short || Value is ushort || Value is decimal || Value is double || Value is Single || Value is long)
                        return Value.ToString();

                    if (Value is DateTime) {
                        return string.Format("datetime'{0}'", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.fffffff"));
                    }
                    if (Value is Guid) {
                        return string.Format("guid'{0}'", Value);
                    }
                    throw new InvalidOperationException("Unsupported data type detected."); // TODO: support more types
                }
                return MemberPath;
            }
        }
    }
}