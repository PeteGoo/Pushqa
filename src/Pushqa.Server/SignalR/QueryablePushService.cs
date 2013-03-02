using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pushqa.Communication;
using Pushqa.Infrastructure;
using Microsoft.AspNet.SignalR;

namespace Pushqa.Server.SignalR {
    /// <summary>
    /// A SignalR connection that implements query semantics for oData
    /// </summary>
    public class QueryablePushService<T> : PersistentConnection where T : new() {
        private static readonly Logger logger = new Logger();
        private readonly T context;
        private UriQueryDeserializer queryDeserializer = new UriQueryDeserializer();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryablePushService&lt;T&gt;"/> class.
        /// </summary>
        public QueryablePushService() : this(new JsonMessageSerializer()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryablePushService&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="messageSerializer">The message serializer to use. The default is the <see cref="JsonMessageSerializer"/></param>
        public QueryablePushService(IMessageSerializer messageSerializer) {
            VerifyArgument.IsNotNull("messageSerializer", messageSerializer);
            context = new T();
        }

        private static ConcurrentDictionary<string, IDisposable> subscriptions = new ConcurrentDictionary<string, IDisposable>();

        /// <summary>
        /// Called when [connected async].
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="connectionId">The connection id.</param>
        /// <returns></returns>
        protected override Task OnConnected(IRequest request, string connectionId) {
            return Task.Factory.StartNew(() => {
                
                logger.Log(Logger.LogLevel.Debug, "Request URI {0}", request.Url);
                IDisposable currentSubscription;
                if (subscriptions.ContainsKey(connectionId) && subscriptions.TryGetValue(connectionId, out currentSubscription)) {
                    return;
                }
                if (request.Url == null) {
                    return;
                }
                string resourceName = queryDeserializer.GetResourceName(new Uri(request.Url.ToString().Replace("/connect", ""))); 

                PropertyInfo propertyInfo = typeof(T).GetProperty(resourceName);
                if (propertyInfo == null) {
                    throw new NotImplementedException("Need exception type");
                }

                Type messageType = propertyInfo.PropertyType.GetInterfaces().Concat(new[] { propertyInfo.PropertyType }).Where(
                    iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IQbservable<>)).Select(iface => iface.GetGenericArguments()[0]).FirstOrDefault();

                if (messageType == null) {
                    throw new NotImplementedException("Need exception type");
                }

                IQbservable qbservable = queryDeserializer.Deserialize(propertyInfo.GetValue(context, null) as IQbservable, messageType, request.Url);

                Func<IQbservable<int>, string, IDisposable> dummyCreateSubscription = CreateSubscription;

                IDisposable subscription = dummyCreateSubscription.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] { messageType }).Invoke(
                    this, new object[] { qbservable, connectionId }) as IDisposable;

                subscriptions.AddOrUpdate(connectionId, subscription, (x, y) => subscription);

                logger.Log(Logger.LogLevel.Debug, string.Format("New client {0} connected to server. Total connected clients {1}", connectionId, subscriptions.Count));
            });
        }

        /// <summary>
        /// Creates the subscription.
        /// </summary>
        /// <typeparam name="TItemType">The type of the item type.</typeparam>
        /// <param name="qbservable">The qbservable.</param>
        /// <param name="clientId">The client id.</param>
        /// <returns></returns>
        protected virtual IDisposable CreateSubscription<TItemType>(IQbservable<TItemType> qbservable, string clientId) {
            return qbservable.Subscribe(x => {

                try {
                    Connection.Send(clientId, new EventWrapper<TItemType> { Message = x, Type = EventWrapper<TItemType>.EventType.Message });
                }
                catch (Exception exception) {
                    // How should we handle send exceptions like serialization etc? Error the stream of ignore the poison message?
                    logger.Log(Logger.LogLevel.Error, exception, "Error sending message");
                }
            },
            ex => {

                try {
                    Connection.Send(clientId, new EventWrapper<TItemType> { ErrorMessage = ex.Message, Type = EventWrapper<TItemType>.EventType.Error });
                }
                catch (Exception exception) {
                    // How should we handle send exceptions like serialization etc? Error the stream of ignore the poison message?
                    logger.Log(Logger.LogLevel.Error, exception, "Error sending message");
                }
            }
            , () => {
                try {
                    Connection.Send(clientId, new EventWrapper<TItemType> { Type = EventWrapper<TItemType>.EventType.Completed});
                }
                catch (Exception exception) {
                    logger.Log(Logger.LogLevel.Error, exception, "Error sending complete message");
                }
            });
        }

        /// <summary>
        /// Called when disconnected
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="connectionId">The client id.</param>
        /// <returns></returns>
        protected override Task OnDisconnected(IRequest request, string connectionId) {
            IDisposable success;
            subscriptions.TryRemove(connectionId, out success);
            if (success != null) {
                success.Dispose();
            }
            logger.Log(Logger.LogLevel.Debug, string.Format("Client {0} has disconnected from server. Number of remaining connected clients {1}", connectionId, subscriptions.Count));
            return base.OnDisconnected(request, connectionId);
        }
    }
}

