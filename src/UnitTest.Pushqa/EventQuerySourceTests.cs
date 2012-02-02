using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa;

namespace UnitTest.Pushqa {
    [TestClass]
    public class EventQuerySourceTests {
        public EventQuerySourceTests() {}

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #endregion

        [TestMethod]
        public void CreateQueryConstructAnEventQuerySourceOfTheCorrectType() {
            Assert.IsInstanceOfType(new StubEventProvider().MockEvents, typeof(EventQuerySource<StubEventType>));
        }


        public class StubEventProvider : EventProvider {
            public StubEventProvider() : base(new Uri("http://www.foo.com/events")) {}

            public EventQuerySource<StubEventType> MockEvents {
                get { return CreateQuery<StubEventType>("MockEvents"); }
            }
        }

        public class StubEventType {
            public int EventId { get; set; }
        }
    }
}