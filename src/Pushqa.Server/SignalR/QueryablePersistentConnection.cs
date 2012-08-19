using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private Func<MessageContainer, bool> filter;
        


        /// <summary>
        /// Called when a new connection is made.
        /// </summary>
        /// <param name="request">The <see cref="T:SignalR.IRequest"/> for the current connection.</param><param name="connectionId">The id of the connecting client.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/> that completes when the connect operation is complete.
        /// </returns>
        protected override System.Threading.Tasks.Task OnConnectedAsync(IRequest request, string connectionId) {
            string filterString = request.QueryString.Get("$filter");
            if(!string.IsNullOrWhiteSpace(filterString)) {
                FilterExpressionFactory filterExpressionFactory = new FilterExpressionFactory();
                Expression<Func<MessageContainer, bool>> expression = filterExpressionFactory.Create<MessageContainer>(filterString);
                filter = expression.Compile();
                if(Connection != null && Connection is FilteredConnection) {
                    (Connection as FilteredConnection).Filter = filter;
                }
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
            return new FilteredConnection(filter, _messageBus,
                                  _jsonSerializer,
                                  DefaultSignal,
                                  connectionId,
                                  GetDefaultSignals(connectionId),
                                  groups,
                                  _trace);
        }

        internal class MessageContainer {
            public string Message { get; set; }
        }

        internal class FilteredConnection : Connection {
            private Func<MessageContainer, bool> filter;


            public FilteredConnection(Func<MessageContainer, bool> filter, IMessageBus messageBus, IJsonSerializer jsonSerializer, string baseSignal, string connectionId, IEnumerable<string> signals, IEnumerable<string> groups, ITraceManager traceManager) : base(messageBus, jsonSerializer, baseSignal, connectionId, signals, groups, traceManager) {
                this.Filter = filter;
            }

            /// <summary>
            /// The filter to apply to messages
            /// </summary>
            public Func<MessageContainer, bool> Filter {
                get { return filter; }
                set { filter = value; }
            }


            protected override List<object> ProcessResults(IList<Message> source) {
                return base.ProcessResults(source).Where(m => !(m is string) || Filter == null || Filter(new MessageContainer { Message = m.ToString()}) ).ToList();
            }
        }
    }
}