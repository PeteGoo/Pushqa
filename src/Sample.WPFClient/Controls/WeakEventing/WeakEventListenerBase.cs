namespace Sample.WPFClient.Controls.WeakEventing
{
    /// <summary>
    /// Provides a non-generic base class for the WeakEventListener implementation.
    /// The main reason is to get easier reading code in the WeakEventSourceBase implementation. 
    /// </summary>
    public abstract class WeakEventListenerBase
    {
        public abstract void Detach();
    }
}
