using System.ComponentModel;

namespace Sample.Common {
    public class ProcessInfo : INotifyPropertyChanged {
        private int processId;
        public int ProcessId {
            get { return processId; }
            set {
                if (processId != value) {
                    processId = value;    
                    NotifyPropertyChanged("ProcessId");
                }
            }
        }

        private string name;
        public string Name {
            get { return name; }
            set {
                if (name != value) {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private int cpuTime;
        public int CpuTime {
            get { return cpuTime; }
            set {
                if (cpuTime != value) {
                    cpuTime = value;
                    NotifyPropertyChanged("CpuTime");
                }
            }
        }

        private long workingSet;
        public long WorkingSet {
            get { return workingSet; }
            set {
                if (workingSet != value) {
                    workingSet = value;
                    NotifyPropertyChanged("WorkingSet");
                }
            }
        }

        public void Update(ProcessInfo processInfo) {
            ProcessId = processInfo.ProcessId;
            Name = processInfo.Name;
            CpuTime = processInfo.CpuTime;
            WorkingSet = processInfo.WorkingSet;
        }

        protected virtual void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}