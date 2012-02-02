using System.Reactive.Linq;

namespace UnitTest.Pushqa {
    public class StubContext {
        private IQbservable<StubMessage> stream1;
        public IQbservable<StubMessage> Stream1 {
            get {
                return stream1 ??
                       (stream1 = Observable.Range(1, 50).Select(x => new StubMessage {MessageId = x}).AsQbservable());
            }
        }
    }
}