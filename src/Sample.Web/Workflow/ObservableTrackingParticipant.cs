using System;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Sample.Web.Workflow {
    public class ObservableTrackingParticipant : TrackingParticipant {

        private Subject<TrackingMessage> trackingMessages = new Subject<TrackingMessage>();

        public Subject<TrackingMessage> TrackingMessages {
            get { return trackingMessages; }
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout) {
            TrackingMessages.OnNext(new TrackingMessage(record));
        }


        public class TrackingMessage {

            public TrackingMessage() { }

            public TrackingMessage(TrackingRecord trackingRecord) {
                RecordType = trackingRecord.GetType().Name;
                InstanceId = trackingRecord.InstanceId.ToString();
                RecordNumber = trackingRecord.RecordNumber;
                EventTime = trackingRecord.EventTime;
                Content = trackingRecord.ToString().Replace("<null>", "null");
                if (trackingRecord is WorkflowInstanceRecord) {
                    ActivityDefinitionId = ((WorkflowInstanceRecord)trackingRecord).ActivityDefinitionId;
                    State = ((WorkflowInstanceRecord)trackingRecord).State;
                }
                if (trackingRecord is ActivityScheduledRecord) {
                    Activity = ((ActivityScheduledRecord)trackingRecord).Activity.Name;
                    ChildActivity = ((ActivityScheduledRecord)trackingRecord).Child.Name;
                }
                if (trackingRecord is ActivityStateRecord) {
                    Activity = ((ActivityStateRecord)trackingRecord).Activity.Name;
                    State = ((ActivityStateRecord)trackingRecord).State;
                    Variables = ((ActivityStateRecord)trackingRecord).Variables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value == null ? null : kvp.Value.ToString());
                    Arguments = ((ActivityStateRecord)trackingRecord).Arguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value == null ? null : kvp.Value.ToString());
                }
                if (trackingRecord is CustomTrackingRecord) {
                    Activity = ((CustomTrackingRecord)trackingRecord).Activity.Name;
                    Name = ((CustomTrackingRecord)trackingRecord).Name;
                    Data = string.Join(", ", ((CustomTrackingRecord)trackingRecord).Data.Select(kvp => string.Format("{0} = {1}", kvp.Key, kvp.Value)));
                }
                if (trackingRecord is WorkflowInstanceUnhandledExceptionRecord) {
                    Activity = ((WorkflowInstanceUnhandledExceptionRecord)trackingRecord).FaultSource.Name;
                    Data = ((WorkflowInstanceUnhandledExceptionRecord)trackingRecord).UnhandledException.ToString();
                }
            } 

            public string Name { get; set; }

            public string RecordType { get; set; }

            public string InstanceId { get; set; }

            public long RecordNumber { get; set; }

            public DateTime EventTime { get; set; }

            public string ActivityDefinitionId { get; set; }

            public string State { get; set; }

            public string Activity { get; set; }

            public string ChildActivity { get; set; }

            public IDictionary<string, string> Arguments { get; set; }

            public IDictionary<string, string> Variables { get; set; }

            public string Data { get; set; }

            public string Content { get; set; }


        }
    }
}