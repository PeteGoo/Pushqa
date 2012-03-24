using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using Sample.Common;

namespace Sample.WPFClient.ViewModels {
    public class ProcessViewerViewModel : ISampleViewModel {
        private readonly ObservableCollection<ProcessInfo> processes = new ObservableCollection<ProcessInfo>();
        private ICollectionView collectionView;
        private IDisposable subscription;

        public ICollectionView Processes {
            get { return collectionView; }
        }

        public void Start() {
            // Setup the event stream subscription
            MyPushEventProvider eventProvider = new MyPushEventProvider();

            collectionView = CollectionViewSource.GetDefaultView(processes);
            collectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            subscription = (from processInfo in eventProvider.ProcessInformation
                            select processInfo)
                            .AsObservable()
                            .ObserveOnDispatcher()
                            .Subscribe(message => {
                                if (processes.Any(p => p.ProcessId == message.ProcessId)) {
                                    processes.Where(p => p.ProcessId == message.ProcessId).ToList().ForEach(
                                        x => x.Update(message));
                                }
                                else {
                                    processes.Add(message);
                                }
                            },
                            () => processes.Clear());

        }

        public void Stop() {
            processes.Clear();
            if (subscription != null) {
                subscription.Dispose();
                subscription = null;
            }
        }
    }
}