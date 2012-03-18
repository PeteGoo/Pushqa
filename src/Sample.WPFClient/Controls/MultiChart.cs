using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.Specialized;
using Sample.WPFClient.Controls.WeakEventing;

namespace Sample.WPFClient.Controls {
    public class MultiChart : System.Windows.Controls.DataVisualization.Charting.Chart {
        private CollectionChangedWeakEventSource _sourceChanged;

        public MultiChart() {
            // setup wrapper for CollectionChanged
            _sourceChanged = new CollectionChangedWeakEventSource();
            _sourceChanged.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(sourceCollectionChanged_CollectionChanged);
        }

        #region SeriesSource (DependencyProperty)

        public IEnumerable SeriesSource {
            get { return (IEnumerable)GetValue(SeriesSourceProperty); }
            set { SetValue(SeriesSourceProperty, value); }
        }

        public static readonly DependencyProperty SeriesSourceProperty = DependencyProperty.Register("SeriesSource", typeof(IEnumerable), typeof(MultiChart), new PropertyMetadata(default(IEnumerable), new PropertyChangedCallback(OnSeriesSourceChanged)));

        private static void OnSeriesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            IEnumerable oldValue = (IEnumerable)e.OldValue;
            IEnumerable newValue = (IEnumerable)e.NewValue;
            MultiChart source = (MultiChart)d;
            source.OnSeriesSourceChanged(oldValue, newValue);
        }

        protected virtual void OnSeriesSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            //  in order to prevent memory leak, we use a weak-eventing pattern.
            // for more information about the weak eventing used in this example, please visit the follwoing two urls:
            //    http://blog.thekieners.com/2010/02/11/simple-weak-event-listener-for-silverlight/
            //    http://blog.thekieners.com/2010/02/17/weakeventsource-implementation-2/
            // ...just set the event source to the wrapper when the source list changes
            _sourceChanged.SetEventSource(newValue);

            this.Series.Clear();

            if (newValue != null) {
                foreach (object item in newValue) {
                    CreateSeries(item);
                }
            }

        }


        #endregion

        #region SeriesTemplate (DependencyProperty)

        public DataTemplate SeriesTemplate {
            get { return (DataTemplate)GetValue(SeriesTemplateProperty); }
            set { SetValue(SeriesTemplateProperty, value); }
        }
        public static readonly DependencyProperty SeriesTemplateProperty = DependencyProperty.Register("SeriesTemplate", typeof(DataTemplate), typeof(MultiChart), new PropertyMetadata(default(DataTemplate)));

        #endregion

        #region SeriesTemplateSelector (DependencyProperty)

        public DataTemplateSelector SeriesTemplateSelector {
            get { return (DataTemplateSelector)GetValue(SeriesTemplateSelectorProperty); }
            set { SetValue(SeriesTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty SeriesTemplateSelectorProperty = DependencyProperty.Register("SeriesTemplateSelector", typeof(DataTemplateSelector), typeof(MultiChart), new PropertyMetadata(default(DataTemplateSelector)));

        #endregion

        #region Private methods

        private void CreateSeries(object item) {
            CreateSeries(item, this.Series.Count);
        }

        private void CreateSeries(object item, int index) {
            DataTemplate dataTemplate = null;

            // get data template
            if (this.SeriesTemplateSelector != null) {
                dataTemplate = this.SeriesTemplateSelector.SelectTemplate(item, this);
            }
            if (dataTemplate == null && this.SeriesTemplate != null) {
                dataTemplate = this.SeriesTemplate;
            }

            // load data template content
            if (dataTemplate != null) {
                Series series = dataTemplate.LoadContent() as Series;

                if (series != null) {
                    // set data context
                    series.DataContext = item;

                    this.Series.Insert(index, series);
                }
            }
        }

        void sourceCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:

                    if (e.NewItems != null) {
                        for (int i = 0; i < e.NewItems.Count; i++) {
                            CreateSeries(e.NewItems[0], e.NewStartingIndex + i);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:

                    if (e.OldItems != null) {
                        for (int i = e.OldItems.Count; i > 0; i--) {
                            this.Series.RemoveAt(i + e.OldStartingIndex - 1);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("NotifyCollectionChangedAction.Replace is not implemented by MultiChart control.");

                case NotifyCollectionChangedAction.Reset:

                    this.Series.Clear();

                    if (SeriesSource != null) {
                        foreach (object item in SeriesSource) {
                            CreateSeries(item);
                        }
                    }

                    break;
                default:
                    break;
            }

        }

        #endregion

    }
}
