using System;
using System.Reactive.Linq;
using Pushqa;
using Pushqa.Communication;
using Pushqa.Linq;
using Pushqa.Server;

namespace UnitTest.Pushqa {
    public class InMemoryViaUriEventQueryPipeline : IEventProviderPipeline {

        public Uri Uri { get; set; }

        public IObservable<TResult> GetEventStream<TSource, TResult>(EventQuery<TSource, TResult> query) {
            Uri = new QueryUriProvider().GetQueryUri(query);

            UriQueryDeserializer deserializer = new UriQueryDeserializer();


            return deserializer.Deserialize(((IObservable<TSource>)Source).AsQbservable(), Uri) as IQbservable<TResult>;
        }

        public object Source { get; set; }
    }
}