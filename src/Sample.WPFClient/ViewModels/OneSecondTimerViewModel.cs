using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Sample.Common;

namespace Sample.WPFClient.ViewModels {
    public class OneSecondTimerViewModel : ISampleViewModel {
        private readonly ObservableCollection<MyMessage> messages = new ObservableCollection<MyMessage>();

        public ObservableCollection<MyMessage> Messages {
            get { return messages; }
        }

        private IDisposable subscription;

        public void Start() {
            Stop();
            // Setup the event stream subscription
            MyPushEventProvider eventProvider = new MyPushEventProvider();
            subscription = (from myEvent in eventProvider.OneSecondTimer
                            where myEvent.MessageId % 2 == 0    // Only log every 2nd message
                            select myEvent)
                            .Take(5)   // Only take 5 messages in total, then complete
                            .AsObservable()
                            .ObserveOnDispatcher()
                            .Catch<MyMessage, Exception>(e => Observable.Return(new MyMessage { Description = e.Message }))
                            .Subscribe(
                                message => messages.Add(message), // Append each new message
                                () => Messages.Add(new MyMessage { Description = "Complete" }) // Write out "Complete" when there are no more messages
                            );
        }

        public void Stop() {
            messages.Clear();
            if (subscription != null) {
                subscription.Dispose();
                subscription = null;
            }
        }
    }
}