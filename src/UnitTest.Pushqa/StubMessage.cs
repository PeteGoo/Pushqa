using System;

namespace UnitTest.Pushqa {
    public class StubMessage {
        public int MessageId { get; set; }

        public decimal DecimalValue { get; set; }
        public double DoubleValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public bool BooleanValue { get; set; }
        public ComplexType ComplexProperty { get; set; }
        
        public class ComplexType {
            public string Foo { get; set; }
            public bool BooleanValue { get; set; }
        }
    }
}