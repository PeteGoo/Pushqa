namespace UnitTest.Pushqa {
    public class StubMessage {
        public int MessageId { get; set; }

        public ComplexType ComplexProperty { get; set; }

        public class ComplexType {
            public string Foo { get; set; }
        }
    }
}