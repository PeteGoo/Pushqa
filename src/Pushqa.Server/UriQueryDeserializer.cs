using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Web;
using Linq2Rest.Parser;

namespace Pushqa.Server {
    /// <summary>
    /// Deserializes URI oData queries into IQbservables
    /// </summary>
    public class UriQueryDeserializer {
         /// <summary>
         /// Deserializes the specified query.
         /// </summary>
         /// <param name="query">The query.</param>
         /// <param name="uri">The URI.</param>
         /// <returns></returns>
         public IQbservable<T> Deserialize<T>(IQbservable<T> query, Uri uri) {
             Contract.Requires(query != null);
             Contract.Requires(uri != null);

             ServiceQuery serviceQuery = GetServiceQuery(uri);

             return Deserialize<T>(query, serviceQuery.QueryParts);
         }

         /// <summary>
         /// Deserializes the specified query.
         /// </summary>
         /// <param name="query">The query.</param>
         /// <param name="itemType">Type of the item.</param>
         /// <param name="uri">The URI.</param>
         /// <returns></returns>
         public IQbservable Deserialize(IQbservable query, Type itemType, Uri uri) {
             Contract.Requires(query != null);
             Contract.Requires(uri != null);

             ServiceQuery serviceQuery = GetServiceQuery(uri);

             return Deserialize(query, itemType, serviceQuery.QueryParts);
         }

         /// <summary>
         /// Gets the name of the resource.
         /// </summary>
         /// <param name="uri">The URI.</param>
         /// <returns></returns>
        public string GetResourceName(Uri uri) {
             return uri.Segments.Last().Trim('/');
         }


        internal static ServiceQuery GetServiceQuery(Uri uri) {
             Contract.Requires(uri != null);

             NameValueCollection queryPartCollection = HttpUtility.ParseQueryString(uri.Query);

             List<ServiceQueryPart> serviceQueryParts = new List<ServiceQueryPart>();
             foreach (string queryPart in queryPartCollection) {
                 if (queryPart == null || !queryPart.StartsWith("$", StringComparison.Ordinal)) {
                     // not a special query string
                     continue;
                 }

                 foreach (string value in queryPartCollection.GetValues(queryPart)) {
                     ServiceQueryPart serviceQueryPart = new ServiceQueryPart(queryPart.Substring(1), value);
                     serviceQueryParts.Add(serviceQueryPart);
                 }
             }

             // Query parts for OData need to be ordered $filter, $orderby, $skip, $top. For this
             // set of query operators, they are already in alphabetical order, so it suffices to
             // order by operator name. In the future if we support other operators, this may need
             // to be reexamined.
             serviceQueryParts = serviceQueryParts.OrderBy(p => p.QueryOperator).ToList();

             ServiceQuery serviceQuery = new ServiceQuery() {
                 QueryParts = serviceQueryParts,
             };

             return serviceQuery;
         }

         internal static IQbservable<T> Deserialize<T>(IQbservable<T> query, IEnumerable<ServiceQueryPart> queryParts) {
             Contract.Requires(query != null);
             Contract.Requires(queryParts != null);
             int? skip = null;
             int? top = null;
             foreach (ServiceQueryPart part in queryParts) {
                 switch (part.QueryOperator) {
                     case "filter":
                         FilterExpressionFactory filterExpressionFactory = new FilterExpressionFactory();
                         Expression<Func<T, bool>> expression = filterExpressionFactory.Create<T>(part.Expression);
                         query = query.Where(expression);
                         break;
                     case "orderby":
                         throw new NotSupportedException("The query operator 'orderby' is not supported for event streams");
                     case "skip":
                         int skipValue;
                         if (int.TryParse(part.Expression, out skipValue)) {
                             skip = skipValue;
                         }
                         break;
                     case "top":
                         int topValue;
                         if (int.TryParse(part.Expression, out topValue)) {
                             top = topValue;
                         }
                         break;
                 }
             }

             if(skip != null) {
                 query = query.Skip(skip.Value);
             }
             if (top != null) {
                 query = query.Take(top.Value);
             }
             return query;
         }

         internal static IQbservable Deserialize(IQbservable messageSource, Type itemType, IEnumerable<ServiceQueryPart> queryParts) {
             if (!typeof(IQbservable<>).MakeGenericType(new[] { itemType }).IsInstanceOfType(messageSource)) {
                 throw new NotImplementedException("CUstom exception handling");
             }
             Func<IQbservable<int>, IEnumerable<ServiceQueryPart>, IQbservable<int>> dummyAction = Deserialize<int>;
             MethodInfo methodInfo = dummyAction.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] {itemType});
             return methodInfo.Invoke(null, new object[] {messageSource, queryParts}) as IQbservable;
         }
    }

    /// <summary>
    /// Represents an <see cref="System.Linq.IQueryable"/>.
    /// </summary>
    internal class ServiceQuery
    {
        /// <summary>
        /// Gets or sets a list of query parts.
        /// </summary>
        public IEnumerable<ServiceQueryPart> QueryParts
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a single query operator to be applied to a query
    /// </summary>
    internal class ServiceQueryPart
    {
        private string _queryOperator;
        private string _expression;

        /// <summary>
        /// Public constructor
        /// </summary>
        public ServiceQueryPart()
        {
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="queryOperator">The query operator</param>
        /// <param name="expression">The query expression</param>
        public ServiceQueryPart(string queryOperator, string expression)
        {
            if (queryOperator == null)
            {
                throw new ArgumentNullException("queryOperator");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (queryOperator != "filter" && queryOperator != "orderby" &&
               queryOperator != "skip" && queryOperator != "top")
            {
                throw new ArgumentException("Invalid Query Operator '{0}'", "queryOperator");
            }

            this._queryOperator = queryOperator;
            this._expression = expression;
        }

        /// <summary>
        /// Gets or sets the query operator. Must be one of the supported operators : "where", "orderby", "skip", or "take".
        /// </summary>
        public string QueryOperator
        {
            get
            {
                return this._queryOperator;
            }
            set
            {
                this._queryOperator = value;
            }
        }

        /// <summary>
        /// Gets or sets the query expression.
        /// </summary>
        public string Expression
        {
            get
            {
                return this._expression;
            }
            set
            {
                this._expression = value;
            }
        }

        /// <summary>
        /// Returns a string representation of this <see cref="ServiceQueryPart"/>
        /// </summary>
        /// <returns>The string representation of this <see cref="ServiceQueryPart"/></returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}={1}", this.QueryOperator, this.Expression);
        }
    }
}