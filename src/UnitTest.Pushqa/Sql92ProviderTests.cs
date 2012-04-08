using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pushqa;
using Pushqa.Communication;
using Pushqa.Linq;

namespace UnitTest.Pushqa {
    /// <summary>
    /// Summary description for Sql92ProviderTests
    /// </summary>
    [TestClass]
    public class Sql92ProviderTests {
        public Sql92ProviderTests() { }

        


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
        public void TakeOnEventQuerySourceDoesNotProduceFilterCriteria() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Take(5);
            Assert.AreEqual(string.Empty, GetFilterString(query));
        }

        [TestMethod]
        public void SkipOnEventQuerySourceDoesNotProduceFilterCriteria() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => source.Skip(5);
            Assert.AreEqual(string.Empty, GetFilterString(query));
        }


        [TestMethod]
        public void PropertyEqualsLiteralProducesCorrectFilterSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId == 5 select ev;
            Assert.AreEqual("MessageId = 5", GetFilterString(query));
        }

        [ExpectedException(typeof(Sql92DoesNotSupportPropertyNavigationException))]
        [TestMethod]
        public void SecondLevelPropertyAccessProducesException() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo == "foo" select ev;
            GetFilterString(query);
        }

        [TestMethod]
        public void PropertyNotEqualsLiteralProducesCorrectFilterSyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId != 5 select ev;
            Assert.AreEqual("MessageId != 5", GetFilterString(query));
        }

        [TestMethod]
        public void PropertyEqualsValueProducesCorrectQuerySyntax() {
            int x = 5;
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId == x select ev;
            Assert.AreEqual("MessageId = 5", GetFilterString(query));
        }

        [TestMethod]
        public void PropertyGreatThanLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId > 5 select ev;
            Assert.AreEqual("MessageId > 5", GetFilterString(query));
        }

        [TestMethod]
        public void PropertyGreatThanOrEqualLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId >= 5 select ev;
            Assert.AreEqual("MessageId >= 5", GetFilterString(query));
        }

        [TestMethod]
        public void PropertyLessThanLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId < 5 select ev;
            Assert.AreEqual("MessageId < 5", GetFilterString(query));
        }

        [TestMethod]
        public void PropertyLessThanOrEqualLiteralProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId <= 5 select ev;
            Assert.AreEqual("MessageId <= 5", GetFilterString(query));
        }

        [TestMethod]
        public void LogicalAndProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId > 1 && ev.MessageId < 5 select ev;
            Assert.AreEqual("(MessageId > 1) AND (MessageId < 5)", GetFilterString(query));
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
            Assert.AreEqual("(MessageId > 1) AND (MessageId < 5)", GetFilterString(query));
        }

        [TestMethod]
        public void LogicalOrProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId < 1 || ev.MessageId > 5 select ev;
            Assert.AreEqual("(MessageId < 1) OR (MessageId > 5)", GetFilterString(query));
        }

        [TestMethod]
        public void LogicalNotProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where !(ev.MessageId < 1 || ev.MessageId > 5) select ev;
            Assert.AreEqual("NOT ((MessageId < 1) OR (MessageId > 5))", GetFilterString(query));
        }

        [TestMethod]
        public void AddProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId + 5 > 10 select ev;
            Assert.AreEqual("(MessageId + 5) > 10", GetFilterString(query));
        }

        [TestMethod]
        public void SubtractProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId - 5 > 10 select ev;
            Assert.AreEqual("(MessageId - 5) > 10", GetFilterString(query));
        }

        [TestMethod]
        public void MultiplyProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId * 5 > 10 select ev;
            Assert.AreEqual("(MessageId * 5) > 10", GetFilterString(query));
        }

        [TestMethod]
        public void DivideProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId / 2 > 10 select ev;
            Assert.AreEqual("(MessageId / 2) > 10", GetFilterString(query));
        }

        [TestMethod]
        public void ModuloProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.MessageId % 2 > 10 select ev;
            Assert.AreEqual("(MOD(MessageId, 2)) > 10", GetFilterString(query));
        }

        [TestMethod]
        public void StringContainsProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.StringValue.Contains("f") select ev;
            Assert.AreEqual("StringValue LIKE '%f%'", GetFilterString(query));
        }

        [TestMethod]
        public void StringContainsWithBooleanEqualsComparisonProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.StringValue.Contains("f") == false select ev;
            Assert.AreEqual("StringValue NOT LIKE '%f%'", GetFilterString(query));
        }

        [TestMethod]
        public void StringEndsWithProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.StringValue.EndsWith("oo") select ev;
            Assert.AreEqual("StringValue LIKE '%oo'", GetFilterString(query));
        }

        [TestMethod]
        public void StringStartsWithProducesCorrectQuerySyntax() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.StringValue.StartsWith("fo") select ev;
            Assert.AreEqual("StringValue LIKE 'fo%'", GetFilterString(query));
        }

        [TestMethod]
        public void BooleanPropertyReferenceAloneConstitutesExpression() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.BooleanValue select ev;
            Assert.AreEqual("BooleanValue = TRUE", GetFilterString(query));
        }

        
        [TestMethod]
        public void LocalVariableReferenceInFilterProducesLiteralInQueryString() {
            string variableReference = "Foo";
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.StringValue == variableReference select ev;
            Assert.AreEqual("StringValue = 'Foo'", GetFilterString(query));
        }

        [TestMethod]
        public void LocalVariableReferenceAsBooleanLiteralInFilterProducesLiteralInQueryString() {
            bool variableReference = true;
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where variableReference && ev.StringValue == "Foo" select ev;
            Assert.AreEqual("StringValue = 'Foo'", GetFilterString(query));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void UnsupportedMethodCallThrowsInvalidOperationException() {
            Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query = source => from ev in source where ev.ComplexProperty.Foo.IsNormalized() select ev;
            GetFilterString(query);
        }

        private string GetFilterString(Func<EventQuerySource<StubMessage>, IEventQuery<StubMessage>> query) {
            IEventQuery<StubMessage> eventProjectedQuery = query(new StubSql92EventProvider().Messages);
            Sql92LoggerEventQueryPipeline pipeline = new Sql92LoggerEventQueryPipeline();
            eventProjectedQuery.AsObservable(pipeline);
            return pipeline.Filter;
        }

        public class StubSql92EventProvider : EventProvider {
            public StubSql92EventProvider() : base(new Uri("http://www.foo.com")) { }

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

    public class Sql92LoggerEventQueryPipeline : IEventProviderPipeline {

        public string Filter { get; set; }

        public IObservable<TResult> GetEventStream<TSource, TResult>(EventQuery<TSource, TResult> query) {
            Filter = new Sql92Provider().GetFilter(query);
            return new TResult[0].ToObservable();
        }
    }
}