using System;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace Sample.Web.Tracing {
    public class ObservableTraceListener : TraceListener {
        private static readonly Subject<TraceMessage> traceMessages = new Subject<TraceMessage>();

        public static IObservable<TraceMessage> TraceMessages {
            get { return traceMessages; }
        }

        public override void Write(string message) {
            traceMessages.OnNext(new TraceMessage(message));
        }

        public override void WriteLine(string message) {
            traceMessages.OnNext(new TraceMessage(message));
        }
    }

    public class TraceMessage {
        public TraceMessage(string message) {
            Message = message;
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}