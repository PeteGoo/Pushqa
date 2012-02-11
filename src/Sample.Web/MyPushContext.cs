using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using Sample.Common;
using Sample.Web.Tracing;

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