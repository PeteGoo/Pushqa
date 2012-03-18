using System;

namespace Sample.Common {
    public class Stock {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string DateString {
            get { return Date.ToLongDateString(); }
        }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public int Volume { get; set; }
    }
}