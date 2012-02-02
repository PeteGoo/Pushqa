using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa;

namespace UnitTest.Pushqa {

    [TestClass]
    public class EventProviderTests {
        public EventProviderTests() {}

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
            Assert.IsInstanceOfType(new StubEventProvider().Messages, typeof(EventQuerySource<StubMessage>));
        }


        public void SkipIgnoresFirstNEvents() {
            IQueryable<string> queryable = new[] {"Foo", "Bar"}.AsQueryable();
            var items = (from i in queryable
                        select i).Skip(5);

        }

        public class StubEventProvider : EventProvider {
            public StubEventProvider() : base(new Uri("http://www.foo.com/events")) { }

            public EventQuerySource<StubMessage> Messages {
                get { return CreateQuery<StubMessage>("Messages"); }
            }
        }

        public class StubEventType {
            public int EventId { get; set; }
        }
    }


}
