using System.Collections.Specialized;

namespace Sample.WPFClient.Controls.WeakEventing
{
    /// <summary>
    /// Default CollectionChanged weak-event wrapper for INotifyCollectionChanged event source.
    /// </summary>
    public class CollectionChangedWeakEventSource : WeakEventSourceBase<INotifyCollectionChanged>
    {
        protected override WeakEventListenerBase CreateWeakEventListener(INotifyCollectionChanged eventObject)
        {
            var weakListener = new WeakEventListener<CollectionChangedWeakEventSource,
                                                     INotifyCollectionChanged,
                                                     NotifyCollectionChangedEventArgs>(this, eventObject);
            weakListener.OnDetachAction = (listener, source) =>
            {
                source.CollectionChanged -= listener.OnEvent;
            };
            weakListener.OnEventAction = (instance, source, e) =>
            {
                // fire event
                if (instance.CollectionChanged != null)
                    instance.CollectionChanged(source, e);
            };
            eventObject.CollectionChanged += weakListener.OnEvent;

            return weakListener;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }

}
