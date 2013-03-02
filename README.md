#Pushqa

##Overview
Pushqa is a .Net library that allows the filtering of incoming push events from a server to be performed server-side.

It allows the consumer to define queries over event streams so that events that are being emitted server side can be filtered and constrained by client side code. The queries are serialized and executed server side rather than sending all the events to the client for client side filtering.

Pushqa uses Microsoft's Reactive Extensions (Rx) expressions over an HTTP connection with the queries serialized using the oData URI specification. The current transport pipeline supported is SignalR though more pipelines may be added in the future. By default, messages are serialized using JSON. The message service is hosted in an ASP.Net Web project running in IIS.

[Click Here](http://petegoo.github.com/Pushqa "Pushqa") For a video showing how to build a Pushqa sample project 

##Implementing a queryable event stream
Server side implementation is easy, we only need to define our server context class with one or more event stream properties in terms of an Rx IQbservable.

First though, lets create an ASP.Net Web Application and add the following code to the Application_Start of the Global.asax file so that incoming requests to the 'events' path of our app gets redirected to Pushqa.

```c#
    RouteTable.Routes.MapConnection<QueryablePushService<MyPushContext>>("events", "events");
```

Now add the push context class that exposes the observable Rx event stream. 

```c#
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
```

Notice that the above Rx event stream is simply a 1 second timer projected into our custom message class. For more ways to easily create Rx event streams (e.g. from standard events) see the examples [here](http://rxwiki.wikidot.com/101samples#toc5 "Rx 101 Samples").

##Consuming from Javascript
The above event stream will be exposed over SignalR so a javascript can use the standard SignalR API to listen to events.

```html
    <div>
        <input type="button" id="connect" value="Connect" />
        <input type="button" id="disconnect" value="Disconnect" />
        <ul id="messages"></ul>
    </div>

    <script src="~/Scripts/jquery-1.9.1.js"></script>
    <script src="~/Scripts/jquery-ui-1.9.2.js"></script>
    <script src="~/Scripts/jquery.signalR-1.0.1.js"></script>

    <script type="text/javascript">
        $(function () {
            var connection = $.connection('../events/OneSecondTimer/', { $filter: "(MessageId mod 2) eq 0", $skip: 2, $top: 5 });

            connection.received(function (data) {
                if (data.Type == 'Completed') {
                    connection.stop();
                    $('#messages').append('<li>Complete</li>');
                }
                else {
                    $('#messages').append('<li>' + JSON.stringify(data.Message) + '</li>');
                }
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
```

Notice that, because Pushqa uses oData's URI syntax, we can filter the event stream to every second event, we can skip the first 2 events after the subscription starts and we will only receive 5 messages.

Or you can now use Reactive Extensions for Javascript which makes it much easier to respond to the different type of events.

```html
    <div>
        <input type="button" id="connect" value="Connect" />
        <input type="button" id="disconnect" value="Disconnect" />
        <ul id="messages"></ul>
    </div>

    <script src="~/Scripts/jquery-1.9.1.js"></script>
    <script src="~/Scripts/jquery-ui-1.9.2.js"></script>
    <script src="~/Scripts/jquery.signalR-1.0.1.js"></script>
    <script src="~/Scripts/rx.min.js"></script>
    <script src="~/Scripts/Rx.Pushqa.js"></script>
    <script type="text/javascript">
        $(function () {
            var connection = $.connection('../events/OneSecondTimer/', { $filter: "(MessageId mod 2) eq 0", $skip: 2, $top: 5 });

            // Append each message received
            connection.asObservable().subscribe(
                function (data) {
                    // onNext
                    $('#messages').append('<li>' + JSON.stringify(data) + '</li>');
                },
                function (error) {
                    // onError
                    $('#messages').append('<li>Error: ' + error + '</li>');
                },
                function () {
                    // onCompleted
                    $('#messages').append('<li>Complete</li>');
                }
            );

            connection.start();

            $("#connect").click(function () {
                connection.start();
            });

            $("#disconnect").click(function () {
                connection.stop();
            });
        });
    </script>
```

##Consuming using the Client Linq API
Pushqa includes a linq provider to allow the event stream to be filtered using LINQ. Firstly we need to implement our client side EventProvider class.

```c#
    public class MyPushEventProvider : EventProvider {
        public MyPushEventProvider() : base(new Uri("http://localhost/Sample.Web/events")) { }

        public EventQuerySource<MyMessage> OneSecondTimer {
            get { return CreateQuery<MyMessage>("OneSecondTimer"); }
        }
    }
```

Then we simply use the EventQuerySource property to construct our query and subscribe.

```c#
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
```

Notice how the standard LINQ query syntax works for filtering while Take and Skip are also implemented. 

AsObservable() created our client side observable event stream and allows us to define further Rx operators that will execute on the client. In this case we monitor the events on the WPF dispatcher, handle errors and push messages onto the Messages collection. 

Complete is called when the server event stream has ended e.g. when the "Take" count is met.

&copy;Peter Goodman 