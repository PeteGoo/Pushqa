using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Pushqa;
using Sample.Common;

namespace Sample.WPFClient {
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

    public class MyPushEventProvider : EventProvider {
        public MyPushEventProvider() : base(new Uri("http://localhost:14844/events")) { }
        //public MyPushEventProvider() : base(new Uri("http://localhost/Sample.Web/events")) { }

        public EventQuerySource<MyMessage> OneSecondTimer {
            get { return CreateQuery<MyMessage>("OneSecondTimer"); }
        }
    }
}