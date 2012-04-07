using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa;
using Pushqa.Linq;

namespace UnitTest.Pushqa {
    /// <summary>
    /// Tests that queries can me serialized and deserialized correctly into their corresponding observables
    /// </summary>
    [TestClass]
    public class QuerySerializeDeserializeTests {

        #region TestContext

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
        public void EmptyEventSourceCompletedImmediately() {
            IObservable<StubMessage> clientEvents = DoQuery(Observable.Empty<StubMessage>(), source => source.Take(5));
            Assert.AreEqual(0, clientEvents.ToEnumerable().Count());
        }

        [TestMethod]
        public void EmptyFilterProducesUnfilteredObservable() {
            RunOneToTenTest(s => s, expectedFirst:1, expectedLast:10, expectedCount:10);
        }

        [TestMethod]
        public void TakeLimitsResultsReturned() {
            RunOneToTenTest(s => s.Take(5), expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void SkipIgnoresFirstNEvents() {
            RunOneToTenTest(s => s.Skip(5), expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void IntegerEquals() {
            RunOneToTenTest(s => from i in s where i.MessageId == 3 select i, expectedFirst: 3, expectedLast: 3, expectedCount: 1);
        }

        [TestMethod]
        public void IntegerNotEquals() {
            RunOneToTenTest(s => from i in s where i.MessageId != 1 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 9);
        }

        [TestMethod]
        public void IntegerGreaterThan() {
            RunOneToTenTest(s => from i in s where i.MessageId > 6 select i, expectedFirst: 7, expectedLast: 10, expectedCount: 4);
        }

        [TestMethod]
        public void IntegerGreaterThanOrEqual() {
            RunOneToTenTest(s => from i in s where i.MessageId >= 6 select i, expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void IntegerLessThan() {
            RunOneToTenTest(s => from i in s where i.MessageId < 6 select i, expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void IntegerLessThanOrEqual() {
            RunOneToTenTest(s => from i in s where i.MessageId <= 6 select i, expectedFirst: 1, expectedLast: 6, expectedCount: 6);
        }

        [Ignore] // Arithmetic operators currently unsupported
        [TestMethod]
        public void IntegerModulo() {
            RunOneToTenTest(s => from i in s where i.MessageId % 2 == 0 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 5);
        }


        [TestMethod]
        public void SecondLevelProperty() {
            RunOneToTenTest(s => from i in s where i.ComplexProperty.Foo == "2" select i, expectedFirst: 2, expectedLast: 2, expectedCount: 1);
        }

        private void RunOneToTenTest(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query, int? expectedFirst, int? expectedLast, int? expectedCount) {
            IObservable<StubMessage> source = Observable.Range(1, 10).Select(i => new StubMessage { MessageId = i, ComplexProperty = new StubMessage.ComplexType {Foo = i.ToString(CultureInfo.InvariantCulture)}});
            IObservable<StubMessage> result = DoQuery(source, query);

            IEnumerable<StubMessage> messages = result.ToEnumerable();
            
            if(expectedFirst != null) {
                Assert.AreEqual(expectedFirst, messages.First().MessageId);
            }
            if(expectedLast != null) {
                Assert.AreEqual(expectedLast, messages.Last().MessageId);
            }
            if(expectedCount != null) {
                Assert.AreEqual(expectedCount, messages.Count());
            }
        }

        private IObservable<StubMessage> DoQuery(IObservable<StubMessage> source, Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query) {
            return ((IEventQuery<StubMessage>) query(new EventQuerySource<StubMessage>("Messages", new Uri("http://www.foo.com")))).
                AsObservable(new InMemoryViaUriEventQueryPipeline {Source = source});
        }
    }
}
