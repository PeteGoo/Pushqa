using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa;
using Pushqa.Communication;
using Pushqa.Linq;

namespace UnitTest.Pushqa {
    /// <summary>
    /// Summary description for QueryUriProviderTests
    /// </summary>
    [TestClass]
    public class QueryUriProviderTests {
        public QueryUriProviderTests() { }

        private string uriBaseAddress = "http://www.foo.com/events/";


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
        public void TakeOnEventQuerySourceProducesTopUriSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Take(5);
            Assert.AreEqual("?$top=5", GetQueryString(query));
        }

        [TestMethod]
        public void TakeOnFilteredEventQuerySourceProducesTopUriSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Where(message => message.MessageId > 3).Take(5);
            Assert.IsTrue(GetQueryString(query).Contains("$top=5"));
        }

        [TestMethod]
        public void SkipOnEventQuerySourceProducesTopUriSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Skip(5);
            Assert.AreEqual("?$skip=5", GetQueryString(query));
        }

        [TestMethod]
        public void SkipOnFilteredEventQuerySourceProducesTopUriSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Where(message => message.MessageId > 3).Skip(5);
            Assert.IsTrue(GetQueryString(query).Contains("$skip=5"));
        }

        [TestMethod]
        public void PropertyEqualsLiteralProducesCorrectFilterSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId == 5 select ev;
            Assert.AreEqual("?$filter=MessageId eq 5", GetQueryString(query));
        }

        [TestMethod]
        public void SecondLevelPropertyEqualsLiteralProducesCorrectFilterSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo == "foo" select ev;
            Assert.AreEqual("?$filter=ComplexProperty/Foo eq 'foo'", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyNotEqualsLiteralProducesCorrectFilterSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId != 5 select ev;
            Assert.AreEqual("?$filter=MessageId ne 5", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyEqualsValueProducesCorrectQuerySyntax() {
            int x = 5;
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId == x select ev;
            Assert.AreEqual("?$filter=MessageId eq 5", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyGreatThanLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId > 5 select ev;
            Assert.AreEqual("?$filter=MessageId gt 5", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyGreatThanOrEqualLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId >= 5 select ev;
            Assert.AreEqual("?$filter=MessageId ge 5", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyLessThanLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId < 5 select ev;
            Assert.AreEqual("?$filter=MessageId lt 5", GetQueryString(query));
        }

        [TestMethod]
        public void PropertyLessThanOrEqualLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId <= 5 select ev;
            Assert.AreEqual("?$filter=MessageId le 5", GetQueryString(query));
        }

        [TestMethod]
        public void LogicalAndProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId > 1 && ev.MessageId < 5 select ev;
            Assert.AreEqual("?$filter=(MessageId gt 1) and (MessageId lt 5)", GetQueryString(query));
        }

        [TestMethod]
        public void ComposedWhereClausesProduceLogicalAnds() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => {
                var temp =
                    source.Where(
                        x => x.MessageId > 1);
                return
                    temp.Where(
                        x => x.MessageId < 5);
            };
            Assert.AreEqual("?$filter=(MessageId gt 1) and (MessageId lt 5)", GetQueryString(query));
        }

        [TestMethod]
        public void LogicalOrProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId < 1 || ev.MessageId > 5 select ev;
            Assert.AreEqual("?$filter=(MessageId lt 1) or (MessageId gt 5)", GetQueryString(query));
        }

        [TestMethod]
        public void LogicalNotProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where !(ev.MessageId < 1 || ev.MessageId > 5) select ev;
            Assert.AreEqual("?$filter=not ((MessageId lt 1) or (MessageId gt 5))", GetQueryString(query));
        }

        [TestMethod]
        public void AddProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId + 5 > 10 select ev;
            Assert.AreEqual("?$filter=(MessageId add 5) gt 10", GetQueryString(query));
        }

        [TestMethod]
        public void SubtractProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId - 5 > 10 select ev;
            Assert.AreEqual("?$filter=(MessageId sub 5) gt 10", GetQueryString(query));
        }

        [TestMethod]
        public void MultiplyProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId * 5 > 10 select ev;
            Assert.AreEqual("?$filter=(MessageId mul 5) gt 10", GetQueryString(query));
        }

        [TestMethod]
        public void DivideProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId / 2 > 10 select ev;
            Assert.AreEqual("?$filter=(MessageId div 2) gt 10", GetQueryString(query));
        }

        [TestMethod]
        public void ModuloProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId % 2 > 10 select ev;
            Assert.AreEqual("?$filter=(MessageId mod 2) gt 10", GetQueryString(query));
        }

        [TestMethod]
        public void StringContainsProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.Contains("f") select ev;
            Assert.AreEqual("?$filter=substringof('f', ComplexProperty/Foo) eq true", GetQueryString(query));
        }

        [TestMethod]
        public void StringContainsWithBooleanEqualsComparisonProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.Contains("f") == false select ev;
            Assert.AreEqual("?$filter=substringof('f', ComplexProperty/Foo) eq false", GetQueryString(query));
        }

        [TestMethod]
        public void StringEndsWithProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.EndsWith("oo") select ev;
            Assert.AreEqual("?$filter=endswith(ComplexProperty/Foo, 'oo') eq true", GetQueryString(query));
        }

        [TestMethod]
        public void StringStartsWithProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.StartsWith("fo") select ev;
            Assert.AreEqual("?$filter=startswith(ComplexProperty/Foo, 'fo') eq true", GetQueryString(query));
        }

        [TestMethod]
        public void BooleanPropertyReferenceAloneConstitutesExpression() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.BooleanValue select ev;
            Assert.AreEqual("?$filter=BooleanValue", GetQueryString(query));
        }

        [TestMethod]
        public void BooleanPropertyOnComplexNavigationPropertyReferenceAloneConstitutesExpression() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.BooleanValue select ev;
            Assert.AreEqual("?$filter=ComplexProperty/BooleanValue", GetQueryString(query));
        }

        [TestMethod]
        public void LocalVariableReferenceInFilterProducesLiteralInQueryString() {
            string variableReference = "Foo";
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo == variableReference select ev;
            Assert.AreEqual("?$filter=ComplexProperty/Foo eq 'Foo'", GetQueryString(query));
        }

        [TestMethod]
        public void LocalVariableReferenceAsBooleanLiteralInFilterProducesLiteralInQueryString() {
            bool variableReference = true;
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where variableReference  && ev.ComplexProperty.Foo == "Foo" select ev;
            Assert.AreEqual("?$filter=ComplexProperty/Foo eq 'Foo'", GetQueryString(query));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void UnsupportedMethodCallThrowsInvalidOperationException() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.IsNormalized() select ev;
            GetQueryString(query);
        }

        private string GetQueryString(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query) {
            IEventQuery<StubMessage> eventProjectedQuery = query(new StubEventProvider(new Uri(uriBaseAddress)).Messages);
            UriLoggerEventQueryPipeline pipeline = new UriLoggerEventQueryPipeline();
            eventProjectedQuery.AsObservable(pipeline);
            return Unescape(pipeline.Uri.Query);
        }

        private string Unescape(string stringToEscape) {
            return stringToEscape.Replace("%20", " ");
        }

        public class StubEventProvider : EventProvider {
            public StubEventProvider(Uri baseUri) : base(baseUri) { }

            public EventQuerySource<StubMessage> Messages {
                get { return CreateQuery<StubMessage>("Messages"); }
            }
        }

        [TestMethod]
        public void TestBed() {
            Expression<Func<StubMessage, bool>> expression = message => message.ComplexProperty.Foo.Contains("x");

            Expression<Func<StubMessage, bool>> expression2 = message => message.ComplexProperty.Foo + "x" == "";

            Expression<Func<StubMessage, bool>> expression3 = message => message.ComplexProperty.Foo.ToLower() == "";

            Expression<Func<StubMessage, bool>> expression4 = message => message.ComplexProperty.Foo.ToUpper() == "";
        }
    }

    public class UriLoggerEventQueryPipeline : IEventProviderPipeline {

        public Uri Uri { get; set; }

        public IObservable<TResult> GetEventStream<TSource, TResult>(EventQuery<TSource, TResult> query) {
            Uri = new QueryUriProvider().GetQueryUri(query);
            return new TResult[0].ToObservable();
        }
    }
}
