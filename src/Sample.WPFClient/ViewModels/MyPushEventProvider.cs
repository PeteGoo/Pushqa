using System;
using Pushqa;
using Sample.Common;

namespace Sample.WPFClient.ViewModels {
    public class MyPushEventProvider : EventProvider {
        public MyPushEventProvider() : base(new Uri("http://localhost:14844/events")) { }
        //public MyPushEventProvider() : base(new Uri("http://localhost/Sample.Web/events")) { }

        public EventQuerySource<MyMessage> OneSecondTimer {
            get { return CreateQuery<MyMessage>("OneSecondTimer"); }
        }

        public EventQuerySource<ProcessInfo> ProcessInformation {
            get { return CreateQuery<ProcessInfo>("ProcessInformation"); }
        }

        public EventQuerySource<Stock> Stocks {
            get { return CreateQuery<Stock>("Stocks"); }
        }
    }
}