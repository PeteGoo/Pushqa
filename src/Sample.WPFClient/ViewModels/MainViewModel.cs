using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Sample.WPFClient.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {

        public MainViewModel() {
            SelectedSample = Samples.First();
        }

        public IEnumerable<SampleItem> Samples {
            get {
                return new[] {
                    new SampleItem {
                        Name="One Second Timer",
                        Key="OneSecondTimer",
                        ViewModel = new Lazy<ISampleViewModel>(() => new OneSecondTimerViewModel()),
                        Description = "Shows the use of skip and take on a server side event stream implemented as a one second timer"
                    },
                    new SampleItem {
                        Name="Process Viewer",
                        Key="ProcessViewer",
                        ViewModel = new Lazy<ISampleViewModel>(() => new ProcessViewerViewModel()),
                        Description = "Shows the processes currently running on the server, updating every second, filter by id possible"
                    },
                    new SampleItem {
                        Name="Stocks",
                        Key="Stocks",
                        ViewModel = new Lazy<ISampleViewModel>(() => new StockViewModel()),
                        Description = "Shows a mock stock graph updating live with filtering"
                    } 
                };
            }
        }

        private SampleItem selectedSample;
        public SampleItem SelectedSample {
            get { return selectedSample; }
            set {
                if (selectedSample != value) {
                    // Stop the previous sample
                    Stop();
                    selectedSample = value;
                    // Start the current sample
                    Start();
                    NotifyPropertyChanged("SelectedSample");
                }       
            }
        }

        private void Start() {
            if (selectedSample == null || selectedSample.ViewModel == null || selectedSample.ViewModel.Value == null) {
                return;
            }
            selectedSample.ViewModel.Value.Start();
        }

        private void Stop() {
            if (selectedSample == null || selectedSample.ViewModel == null || selectedSample.ViewModel.Value == null) {
                return;
            }
            selectedSample.ViewModel.Value.Stop();
        }

        protected virtual void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SampleItem {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public Lazy<ISampleViewModel> ViewModel { get; set; }
    }
}