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
            RunOneToTenIntegerTest(s => s, expectedFirst:1, expectedLast:10, expectedCount:10);
        }

        [TestMethod]
        public void TakeLimitsResultsReturned() {
            RunOneToTenIntegerTest(s => s.Take(5), expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void SkipIgnoresFirstNEvents() {
            RunOneToTenIntegerTest(s => s.Skip(5), expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        // *** Decimal Tests ***
        [TestMethod]
        public void IntegerEquals() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId == 3 select i, expectedFirst: 3, expectedLast: 3, expectedCount: 1);
        }

        [TestMethod]
        public void IntegerNotEquals() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId != 1 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 9);
        }

        [TestMethod]
        public void IntegerGreaterThan() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId > 6 select i, expectedFirst: 7, expectedLast: 10, expectedCount: 4);
        }

        [TestMethod]
        public void IntegerGreaterThanOrEqual() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId >= 6 select i, expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void IntegerLessThan() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId < 6 select i, expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void IntegerLessThanOrEqual() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId <= 6 select i, expectedFirst: 1, expectedLast: 6, expectedCount: 6);
        }

        [TestMethod]
        public void IntegerModulo() {
            RunOneToTenIntegerTest(s => from i in s where i.MessageId % 2 == 0 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 5);
        }

        // *** Decimal Tests ***
        [TestMethod]
        public void DecimalEquals() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId == 3 select i, expectedFirst: 3, expectedLast: 3, expectedCount: 1);
        }

        [TestMethod]
        public void DecimalNotEquals() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId != 1 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 9);
        }

        [TestMethod]
        public void DecimalGreaterThan() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId > 6 select i, expectedFirst: 7, expectedLast: 10, expectedCount: 4);
        }

        [TestMethod]
        public void DecimalGreaterThanOrEqual() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId >= 6 select i, expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void DecimalLessThan() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId < 6 select i, expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void DecimalLessThanOrEqual() {
            RunOneToTenDecimalTest(s => from i in s where i.MessageId <= 6 select i, expectedFirst: 1, expectedLast: 6, expectedCount: 6);
        }

        // *** Double Tests ***
        [TestMethod]
        public void DoubleEquals() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId == 3 select i, expectedFirst: 3, expectedLast: 3, expectedCount: 1);
        }

        [TestMethod]
        public void DoubleNotEquals() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId != 1 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 9);
        }

        [TestMethod]
        public void DoubleGreaterThan() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId > 6 select i, expectedFirst: 7, expectedLast: 10, expectedCount: 4);
        }

        [TestMethod]
        public void DoubleGreaterThanOrEqual() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId >= 6 select i, expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void DoubleLessThan() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId < 6 select i, expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void DoubleLessThanOrEqual() {
            RunOneToTenDoubleTest(s => from i in s where i.MessageId <= 6 select i, expectedFirst: 1, expectedLast: 6, expectedCount: 6);
        }

        // *** DateTime Tests ***
        [TestMethod]
        public void DateTimeEquals() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId == 3 select i, expectedFirst: 3, expectedLast: 3, expectedCount: 1);
        }

        [TestMethod]
        public void DateTimeNotEquals() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId != 1 select i, expectedFirst: 2, expectedLast: 10, expectedCount: 9);
        }

        [TestMethod]
        public void DateTimeGreaterThan() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId > 6 select i, expectedFirst: 7, expectedLast: 10, expectedCount: 4);
        }

        [TestMethod]
        public void DateTimeGreaterThanOrEqual() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId >= 6 select i, expectedFirst: 6, expectedLast: 10, expectedCount: 5);
        }

        [TestMethod]
        public void DateTimeLessThan() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId < 6 select i, expectedFirst: 1, expectedLast: 5, expectedCount: 5);
        }

        [TestMethod]
        public void DateTimeLessThanOrEqual() {
            RunOneToTenDateTimeTest(s => from i in s where i.MessageId <= 6 select i, expectedFirst: 1, expectedLast: 6, expectedCount: 6);
        }


        
        [TestMethod]
        public void SecondLevelProperty() {
            RunOneToTenIntegerTest(s => from i in s where i.ComplexProperty.Foo == "2" select i, expectedFirst: 2, expectedLast: 2, expectedCount: 1);
        }

        private void RunOneToTenIntegerTest(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query, int? expectedFirst, int? expectedLast, int? expectedCount) {
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

        private void RunOneToTenDecimalTest(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query, decimal? expectedFirst, decimal? expectedLast, decimal? expectedCount) {
            IObservable<StubMessage> source = Observable.Range(1, 10).Select(i => new StubMessage { MessageId = i, DecimalValue = Convert.ToDecimal(0.1*i), ComplexProperty = new StubMessage.ComplexType { Foo = i.ToString(CultureInfo.InvariantCulture) } });
            IObservable<StubMessage> result = DoQuery(source, query);

            IEnumerable<StubMessage> messages = result.ToEnumerable();

            if (expectedFirst != null) {
                Assert.AreEqual(expectedFirst.Value * Convert.ToDecimal(0.1), messages.First().DecimalValue);
            }
            if (expectedLast != null) {
                Assert.AreEqual(expectedLast * Convert.ToDecimal(0.1), messages.Last().DecimalValue);
            }
            if (expectedCount != null) {
                Assert.AreEqual(expectedCount, messages.Count());
            }
        }

        private void RunOneToTenDoubleTest(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query, double? expectedFirst, double? expectedLast, int? expectedCount) {
            IObservable<StubMessage> source = Observable.Range(1, 10).Select(i => new StubMessage { MessageId = i, DoubleValue = 0.1 * i, ComplexProperty = new StubMessage.ComplexType { Foo = i.ToString(CultureInfo.InvariantCulture) } });
            IObservable<StubMessage> result = DoQuery(source, query);

            IEnumerable<StubMessage> messages = result.ToEnumerable();

            if (expectedFirst != null) {
                Assert.AreEqual(expectedFirst.Value * 0.1, messages.First().DoubleValue);
            }
            if (expectedLast != null) {
                Assert.AreEqual(expectedLast * 0.1, messages.Last().DoubleValue);
            }
            if (expectedCount != null) {
                Assert.AreEqual(expectedCount, messages.Count());
            }
        }

        private void RunOneToTenDateTimeTest(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query, int? expectedFirst, int? expectedLast, int? expectedCount) {
            IObservable<StubMessage> source = Observable.Range(1, 10).Select(i => new StubMessage { MessageId = i, DateTimeValue = new DateTime(2001, 1, i), ComplexProperty = new StubMessage.ComplexType { Foo = i.ToString(CultureInfo.InvariantCulture) } });
            IObservable<StubMessage> result = DoQuery(source, query);

            IEnumerable<StubMessage> messages = result.ToEnumerable();

            if (expectedFirst != null) {
                Assert.AreEqual(new DateTime(2001, 1, expectedFirst.Value), messages.First().DateTimeValue);
            }
            if (expectedLast != null) {
                Assert.AreEqual(new DateTime(2001, 1, expectedLast.Value), messages.Last().DateTimeValue);
            }
            if (expectedCount != null) {
                Assert.AreEqual(expectedCount, messages.Count());
            }
        }

        private IObservable<StubMessage> DoQuery(IObservable<StubMessage> source, Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query) {
            return ((IEventQuery<StubMessage>) query(new EventQuerySource<StubMessage>("Messages", new Uri("http://www.foo.com")))).
                AsObservable(new InMemoryViaUriEventQueryPipeline {Source = source});
        }
    }
}
