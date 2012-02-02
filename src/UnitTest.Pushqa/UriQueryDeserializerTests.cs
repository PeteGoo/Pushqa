using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa.Server;

namespace UnitTest.Pushqa {
    /// <summary>
    /// Query Filter tests validating against http://www.odata.org/developers/protocols/uri-conventions
    /// </summary>
    [TestClass]
    public class UriQueryDeserializerTests {
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
        public void EmptyFilterProducesUnfilteredObservable() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 10).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc"));
            Assert.AreEqual(1, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerEqualsFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 10).Select(i => new StubMessage{MessageId = i}).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId eq 3"));
            Assert.AreEqual(3, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerNotEqualsFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 2).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId ne 1"));
            Assert.AreEqual(2, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerGreaterThanFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(18, 20).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId gt 23"));
            Assert.AreEqual(24, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerGreaterOrEqualThanFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(18, 20).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId ge 23"));
            Assert.AreEqual(23, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerLessThanFilter() {
            IQbservable<StubMessage> qbservable = new[]{20,19,18,17,16}.ToObservable().Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId lt 18"));
            Assert.AreEqual(17, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerLessOrEqualThanFilter() {
            IQbservable<StubMessage> qbservable = new[] { 20, 19, 18, 17, 16 }.ToObservable().Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId le 18"));
            Assert.AreEqual(18, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerModuloEqualsFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(15, 3).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=(MessageId mod 2) eq 0"));
            Assert.AreEqual(16, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void IntegerDivisionEqualsFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(15, 3).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=(MessageId div 2) eq 8"));
            Assert.AreEqual(16, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void SubstringOfFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 3).Select(i => new StubMessage { MessageId = i, ComplexProperty = new StubMessage.ComplexType() {Foo = i.ToString(CultureInfo.InvariantCulture)}}).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=substringof('2', ComplexProperty/Foo) eq true"));
            Assert.AreEqual(2, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [Ignore] // Currently Unsupported
        [TestMethod]
        public void CastFilter() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 3).Select(i => new StubMessage { MessageId = i, ComplexProperty = new StubMessage.ComplexType() { Foo = i.ToString(CultureInfo.InvariantCulture) } }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$filter=MessageId gt cast(3,'Edm.Int32')"));
            Assert.AreEqual(2, filteredQbservable.ToEnumerable().First().MessageId);
        }

        [TestMethod]
        public void TopQueryParameterLimitsResultsReturned() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 15).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$top=5"));
            Assert.AreEqual(5, filteredQbservable.ToEnumerable().Last().MessageId);
        }

        [TestMethod]
        public void SkipQueryParameterSkipsFirstNResultsReturned() {
            IQbservable<StubMessage> qbservable = Observable.Range(1, 15).Select(i => new StubMessage { MessageId = i }).AsQbservable();
            UriQueryDeserializer uriQueryDeserializer = new UriQueryDeserializer();
            IQbservable<StubMessage> filteredQbservable = uriQueryDeserializer.Deserialize(qbservable, new Uri("http://foo.bar/qux.svc?$skip=5"));
            Assert.AreEqual(6, filteredQbservable.ToEnumerable().First().MessageId);
        }
    }
}
