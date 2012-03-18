using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Web;
using Sample.Common;
using Sample.Web.Tracing;
using ProcessInfo = Sample.Common.ProcessInfo;

namespace Sample.Web {
    public class MyPushContext {
        public IQbservable<MyMessage> OneSecondTimer {
            get { 
                return Observable.Interval(TimeSpan.FromSeconds(1))
                    .Timestamp()
                    .Select(i => new MyMessage {
                        MessageId = i.Value, 
                        TimeStamp = i.Timestamp, 
                        Description = "Message"
                    })
                    .AsQbservable(); 
            }
        }

        public IQbservable<ProcessInfo> ProcessInformation {
            get {
                return (from i in Observable.Interval(TimeSpan.FromSeconds(1))
                       from processInfo in GetProcessInformation().ToObservable()
                       select processInfo).AsQbservable();
            }

        }

        private IEnumerable<IGrouping<DateTime, Stock>> stocks;

        public IQbservable<Stock> Stocks {
            get {
                if(stocks == null) {
                    
                    stocks = (from line in new StreamReader(typeof(MyPushContext).Assembly.GetManifestResourceStream("Sample.Web.AMZN_AAPL_GOOG_MSFT.txt")).ReadToEnd().Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                             let parts = line.Split(',')
                             select new Stock {
                                 Date = DateTime.ParseExact(parts.First(), "yyyyMMdd", CultureInfo.InvariantCulture).Date,
                                 Name = parts.ElementAt(1),
                                 Open = decimal.Parse(parts.ElementAt(2)),
                                 High = decimal.Parse(parts.ElementAt(3)),
                                 Low = decimal.Parse(parts.ElementAt(4)),
                                 Close = decimal.Parse(parts.ElementAt(5)),
                                 Volume = int.Parse(parts.ElementAt(6))
                             }).GroupBy(stock => stock.Date).OrderBy(group => group.Key).ToArray();
                }
                return (from i in Observable.Interval(TimeSpan.FromSeconds(1)).TakeWhile(j => j < stocks.Count())
                        from dateStock in stocks.ElementAt((int) i)
                        select dateStock).AsQbservable();
            }
        }

        private IEnumerable<ProcessInfo> GetProcessInformation() {
            Debug.WriteLine("Getting Process Information");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfProc_Process");
            var results=  searcher.Get().Cast<ManagementObject>().Select(proc => new ProcessInfo {
                ProcessId = int.Parse(proc["IDProcess"].ToString()),
                Name = proc["Name"].ToString(),
                CpuTime = int.Parse(proc["PercentProcessorTime"].ToString()),
                WorkingSet = long.Parse(proc["WorkingSet"].ToString())
            }).OrderBy(x => x.Name);

            Debug.WriteLine(string.Format("Matched {0} Processes", results.Count()));
            return results;
        }

        public IQbservable<TraceMessage> TraceMessages {
            get { return ObservableTraceListener.TraceMessages.AsQbservable(); }
        }
    }
}