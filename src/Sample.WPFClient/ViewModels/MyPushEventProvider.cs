using System;
using Pushqa;
using Sample.Common;

namespace Sample.WPFClient.ViewModels {
    /// <summary>
    /// Provides queryable notifications of server events
    /// </summary>
    public class MyPushEventProvider : EventProvider {
        public MyPushEventProvider() : base(new Uri("http://localhost:14844/events")) { }

        /// <summary>
        /// A stream of notifications that occur every second.
        /// </summary>
        /// <value>The one second timer.</value>
        public EventQuerySource<MyMessage> OneSecondTimer {
            get { return CreateQuery<MyMessage>("OneSecondTimer"); }
        }

        /// <summary>
        /// Gets notifications of changes to the processes running on the server.
        /// </summary>
        /// <value>The process information.</value>
        public EventQuerySource<ProcessInfo> ProcessInformation {
            get { return CreateQuery<ProcessInfo>("ProcessInformation"); }
        }

        /// <summary>
        /// Gets stock history for AMZN, AAPL, GOOG and MSFT.
        /// </summary>
        /// <value>The stocks.</value>
        public EventQuerySource<Stock> Stocks {
            get { return CreateQuery<Stock>("Stocks"); }
        }
    }
}