using System;

namespace Sample.Common {
    public class MyMessage {
        public long? MessageId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string Description { get; set; }
    }
}