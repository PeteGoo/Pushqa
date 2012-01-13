#Pushqa

##Overview
Pushqa is a .Net library that allows the remoting of queries over event streams so that events that are being emitted server side can be filtered and constrained by client side code which is executed server side rather than sending all the events to the client for client side filtering.

Pushqa uses Microsoft's Reactive Extensions (Rx) expressions over an HTTP connection using the oData URI specification. The current transport pipeline supported is SignalR though more pipelines may be added in the future. By default, messages are serialized using JSON. The message service is hosted in an ASP.Net Web project and can be hosted in IIS.

##Implementing a queryable event stream
You define your server context class with one or more event stream properties in terms of an Rx IQbservable.

Create an ASP.Net Web Application and add the following code to the Application_Start of the Global.asax file.

    RouteTable.Routes.MapConnection<QueryablePushService<MyPushContext>>("events", "events/{*operation}");

Now add the push context class that exposes the observable Rx event stream. 

    public class MyPushContext {
        public IQbservable<MyMessage> OneSecondTimer {
            get { 
                return Observable.Interval(TimeSpan.FromSeconds(1))
                    .Timestamp()
                    .Select(i => new MyMessage {
                        MessageId = i.Value, 
                        TimeStamp = i.Timestamp, 
                        Description = "Message"
                    })
                    .AsQbservable(); 
            }
        }
    }

Notice that the above Rx event stream is simply a 1 second timer projected into our custom message class. For more ways to easily create Rx event streams (e.g. from standard events) see the examples here.

##Consuming from Javascript
The above event stream will be exposed over SignalR so a javascript can use the standard SignalR API to listen to events.

    <script type="text/javascript">
        $(function () {
            var connection = $.connection('events/OneSecondTimer/?$filter=(MessageId mod 2) eq 0&$skip=2&$top=5');

            connection.received(function (data) {
                $('#messages').append('<li>' + data + '</li>');
            });

            connection.start();

            $("#connect").click(function () {
                connection.start();
            });

            $("#disconnect").click(function () {
                connection.stop();
            });
        });
    </script>
    <input type="button" id="connect" value="Connect" />
    <input type="button" id="disconnect" value="Disconnect" />
    <ul id="messages"></ul>

Notice that, because Pushqa uses oData's URI syntax, we can filter the event stream to every second event, we can skip the first 2 events after the subscription starts and we will only receive 5 messages.

##Consuming using the Client Linq API
Pushqa includes a linq provider to allow the event stream to be filtered using LINQ. Firstly we need to implement our client side EventProvider class.

    public class MyPushEventProvider : EventProvider {
        public MyPushEventProvider() : base(new Uri("http://localhost/Sample.Web/events")) { }

        public EventQuerySource<MyMessage> OneSecondTimer {
            get { return CreateQuery<MyMessage>("OneSecondTimer"); }
        }
    }

Then we simply use the EventQuerySource property to construct our query and subscribe.

    public class MainWindowViewModel {

        public MainWindowViewModel() {
            // Setup the event stream subscription
            MyPushEventProvider eventProvider = new MyPushEventProvider();
            (from myEvent in eventProvider.OneSecondTimer
             where myEvent.MessageId % 2 == 0 
             select myEvent)
             .Take(5)
             .AsObservable()
             .ObserveOnDispatcher()
             .Catch<MyMessage, Exception>(e => Observable.Return(new MyMessage {Description = e.Message}))
             .Subscribe(message => messages.Add(message), 
                () => Messages.Add(new MyMessage {Description = "Complete"}));

        }

        private readonly ObservableCollection<MyMessage> messages = new ObservableCollection<MyMessage>();

        public ObservableCollection<MyMessage> Messages {
            get { return messages; }
        }
    }

Notice how the standard LINQ query syntax works for filtering while Take and Skip are also implemented. AsObservable created our client side observable event stream and allows us to define further Rx operators that will execute on the client. In this case we monitor the events on the WPF dispatcher, handle errors and push messages onto the Messages collection. Complete is called when the server event stream has ended e.g. when the "Take" count is met.

&copy;Peter Goodman