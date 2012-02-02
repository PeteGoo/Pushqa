using System;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.Pushqa {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestBed {
        public TestBed() {}

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
        public void ExtractExpressionTreeIQbservable() {
            TestContext.WriteLine(new StubContext().Stream1.Where(x => x.MessageId % 2 == 0).GetType().AssemblyQualifiedName);
            new StubContext().Stream1.Where(x => x.MessageId % 2 == 0).Subscribe(x => TestContext.WriteLine(x.MessageId.ToString()));
        }

        //[TestMethod]
        //public void TranslateDataServicesQueryToUri() {
        //    FooContext fooContext = new FooContext(new Uri("http://foo.bar"));
        //    TestContext.WriteLine(fooContext.MyEntities.Where(x => x.EntityId == 2).GetType().FullName);


        //    QueryComponents queryComponents = new EventServiceQueryProvider(fooContext).Translate(
        //        fooContext.MyEntities.Where(x => x.EntityId > 1).Expression);

        //    TestContext.WriteLine("Uri: {0}", queryComponents.Uri.ToString());
        //}


        //[TestMethod]
        //public void TranslateQbservableQueryToUri() {
        //    FooObservableContext fooContext = new FooObservableContext(new Uri("http://foo.bar"));
        //    TestContext.WriteLine(fooContext.MyEntities.Where(x => x.EntityId == 2).GetType().FullName);

        //    QueryComponents queryComponents = new EventServiceQueryProvider(fooContext).Translate(
        //        fooContext.MyEntities.Where(x => x.EntityId > 1).Expression);

        //    TestContext.WriteLine("Uri: {0}", queryComponents.Uri.ToString());
        //}

        //public class FooContext : EventServiceContext {
        //    public FooContext(Uri serviceRoot) : base(serviceRoot) { }

        //    public EventServiceQuery<MyEntity> MyEntities {
        //        get { return CreateQuery<MyEntity>("MyEntities"); }
        //    }
        //}

        //public class FooObservableContext : EventServiceContext {
        //    public FooObservableContext(Uri serviceRoot) : base(serviceRoot) { }

        //    public IQbservable<MyEntity> MyEntities {
        //        get { return Observable.Range(1, 50).Select(x => new MyEntity { EntityId = x }).AsQbservable(); }
        //    }
        //}

        public class MyEntity {
            public int EntityId { get; set; }
        }
    }
}
