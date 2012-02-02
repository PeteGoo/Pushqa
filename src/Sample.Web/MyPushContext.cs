using System;
using System.Reactive.Linq;
using Sample.Common;
using Sample.Web.Tracing;

namespace Sample.Web {
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

        public IQbservable<TraceMessage> TraceMessages {
            get { return ObservableTraceListener.TraceMessages.AsQbservable(); }
        }
    }
}