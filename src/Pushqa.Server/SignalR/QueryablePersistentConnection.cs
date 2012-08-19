using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Linq2Rest.Parser;
using SignalR;
using SignalR.Infrastructure;

namespace Pushqa.Server.SignalR {
    /// <summary>
    /// A Persistent Connection that stores the initial connection query and filters any outbound messages accordingly
    /// </summary>
    public abstract class QueryablePersistentConnection : PersistentConnection {

        private string filterString;
        


        /// <summary>
        /// Called when a new connection is made.
        /// </summary>
        /// <param name="request">The <see cref="T:SignalR.IRequest"/> for the current connection.</param><param name="connectionId">The id of the connecting client.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/> that completes when the connect operation is complete.
        /// </returns>
        protected override System.Threading.Tasks.Task OnConnectedAsync(IRequest request, string connectionId) {
            filterString = request.QueryString.Get("$filter");

            if(Connection != null && Connection is FilteredConnection) {
                (Connection as FilteredConnection).FilterString = filterString;
            }

            return base.OnConnectedAsync(request, connectionId);
        }

        

        /// <summary>
        /// Creates a connection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="groups"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override global::SignalR.Connection CreateConnection(string connectionId, System.Collections.Generic.IEnumerable<string> groups, IRequest request) {
            return new FilteredConnection(filterString, _messageBus,
                                  _jsonSerializer,
                                  DefaultSignal,
                                  connectionId,
                                  GetDefaultSignals(connectionId),
                                  groups,
                                  _trace);
        }

        internal class MessageContainer<T> {
            public MessageContainer(T message) {
                Message = message;
            }
            public T Message { get; private set; }
        }
         

        internal class FilteredConnection : Connection {
            private string filterString;
            private ConcurrentDictionary<Type,Func<object, bool>> typedFilters = new ConcurrentDictionary<Type, Func<object, bool>>();

            static FilteredConnection() {
                createFilterGenericMethodDefn =
                    new Func<string, Func<object, bool>>(CreateFilter<MessageContainer<int>>).Method.GetGenericMethodDefinition();
            }

            public FilteredConnection(string filterString, IMessageBus messageBus, IJsonSerializer jsonSerializer, string baseSignal, string connectionId, IEnumerable<string> signals, IEnumerable<string> groups, ITraceManager traceManager) : base(messageBus, jsonSerializer, baseSignal, connectionId, signals, groups, traceManager) {
                this.filterString = filterString;
            }

            /// <summary>
            /// The filter to apply to messages
            /// </summary>
            public string FilterString {
                get { return filterString; }
                set { 
                    if(filterString == value) {
                        return;
                    }
                    filterString = value;
                    typedFilters.Clear();
                }
            }

            protected override List<object> ProcessResults(IList<Message> source) {
                // override the default message processing to filter based on our query
                List<object> processedResults = base.ProcessResults(source);
                
                return processedResults.Where(m => GetFilter(m.GetType())(m) ).ToList();
            }

            private Func<object, bool> GetFilter(Type messageType) {
                return typedFilters.GetOrAdd(messageType, type => createFilterGenericMethodDefn.MakeGenericMethod(new Type[] { messageType }).Invoke(null, new object[] { FilterString }) as Func<object, bool>);
            }

            private static readonly MethodInfo createFilterGenericMethodDefn;

            private static Func<object, bool> CreateFilter<T>(string filterString)  {
                Func<MessageContainer<T>, bool> filter;
                if (!string.IsNullOrWhiteSpace(filterString)) {
                    try {
                        // Construct the filter expression
                        FilterExpressionFactory filterExpressionFactory = new FilterExpressionFactory();
                        Expression<Func<MessageContainer<T>, bool>> expression = filterExpressionFactory.Create<MessageContainer<T>>(filterString);
                        filter = expression.Compile();
                    } catch (Exception) {
                        // Could not create a valid expression for this type
                        filter = arg => true;
                    }
                }
                else {
                    // No filter was given
                    filter = arg => true;
                }
                return new Func<object, bool>(message => !(message is T) || filter(new MessageContainer<T>((T)message)));
            }
        }
    }
}