using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Sample.WPFClient.ViewModels {
    /// <summary>
    /// Takes a stream of stock change events and at the end of each "day" (which happens every second) and add the day's results to a collection
    /// </summary>
    public class StockViewModel : ISampleViewModel, INotifyPropertyChanged {
        private IDisposable subscription;
        private readonly ObservableCollection<TradingDayClosingSummary> stocks = new ObservableCollection<TradingDayClosingSummary>();
        private bool showAmazon = true;
        private bool showApple = true;
        private bool showGoogle = true;
        private bool showMicrosoft = true;
        private bool isVisible = false;

        public StockViewModel() {
            stocks.Add(new TradingDayClosingSummary() {
                Date = new DateTime(2009, 8, 20)
            });
        }

        public bool IsVisible {
            get { return isVisible; }
            set {
                if (isVisible != value) {
                    isVisible = value;
                    NotifyPropertyChanged("IsVisible");
                }
            }
        }

        public bool ShowAmazon {
            get { return showAmazon; }
            set {
                if (showAmazon != value) {
                    showAmazon = value;
                    NotifyPropertyChanged("ShowAmazon");
                    Start();
                }
            }
        }

        public bool ShowApple {
            get { return showApple; }
            set {
                if (showApple != value) {
                    showApple = value;
                    NotifyPropertyChanged("ShowApple");
                    Start();
                }
            }
        }

        public bool ShowGoogle {
            get { return showGoogle; }
            set {
                if (showGoogle != value) {
                    showGoogle = value;
                    NotifyPropertyChanged("ShowGoogle");
                    Start();
                }
            }
        }

        public bool ShowMicrosoft {
            get { return showGoogle; }
            set {
                if (showMicrosoft != value) {
                    showMicrosoft = value;
                    NotifyPropertyChanged("ShowMicrosoft");
                    Start();
                }
            }
        }

        public ObservableCollection<TradingDayClosingSummary> Stocks {
            get { return stocks; }
        }


        public void Start() {
            Stop();

            IsVisible = true;

            MyPushEventProvider eventProvider = new MyPushEventProvider();
            int selectedStockCount = Convert.ToInt32(showAmazon) + Convert.ToInt32(showApple) + Convert.ToInt32(showGoogle) + Convert.ToInt32(showMicrosoft);

            subscription = eventProvider.Stocks.Where(s => 
                (showAmazon && s.Name == "AMZN") || 
                (showApple && s.Name == "AAPL") || 
                (showGoogle && s.Name == "GOOG") || 
                (showMicrosoft && s.Name == "MSFT"))
                .AsObservable()
                .Buffer(selectedStockCount) // Wait till we have a value for all stocks for the day and collate them into a list of trading summaries
                .ObserveOnDispatcher().Subscribe(group => {
                if (group.Count == 0) {
                    return;
                }

                var daySummary = new TradingDayClosingSummary {
                    Date = group.First().Date,
                    Amazon = group.Where(s => s.Name == "AMZN").Select(s => s.Close).FirstOrDefault(),
                    Apple = group.Where(s => s.Name == "AAPL").Select(s => s.Close).FirstOrDefault(),
                    Google = group.Where(s => s.Name == "GOOG").Select(s => s.Close).FirstOrDefault(),
                    Microsoft = group.Where(s => s.Name == "MSFT").Select(s => s.Close).FirstOrDefault()
                };


                stocks.Add(daySummary);
            });

        }

        public void Stop() {
            IsVisible = false;
            stocks.Clear();

            stocks.Add(new TradingDayClosingSummary() {
                Date = new DateTime(2009, 8, 20)
            });
            if (subscription != null) {
                subscription.Dispose();
                subscription = null;
            }
        }

        public class TradingDayClosingSummary {
            public DateTime Date { get; set; }
            public string DateString {
                get { return Date.ToShortDateString(); }
            }
            public decimal Apple { get; set; }
            public decimal Amazon { get; set; }
            public decimal Google { get; set; }
            public decimal Microsoft { get; set; }
        }

        protected virtual void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}