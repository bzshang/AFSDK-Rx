# AFSDK-Rx
Implementation of Rx.NET for AF SDK

## AFDataPipeExtensions

This class contains extension methods for creating Observables from an AFDataPipe. Upon subscription to the observable, a dedicated event loop thread is created which checks for new events at a configurable interval period on the datapipe via GetObserverEvents.

The Observable is "reference counted" in the sense that upon first subscription to the observable, the event loop and underlying AFDataPipe subscription is created. These are the subscription side-effects. Further observers share the single subscription. When all the observers subscriptions are disposed, the side-effects are also disposed of. See [Publish() and RefCount()] (http://www.introtorx.com/content/v1.0.10621.0/14_HotAndColdObservables.html#PublishAndConnect)

## ObservableExtensions

This class contains the following extension methods.

### Dump

Dumps out the sequence of events, helpful for debugging. See [Dump()](http://www.introtorx.com/content/v1.0.10621.0/07_Aggregation.html).

### Add

Adds two observable AFDataPipeEvent streams and returns an observable stream representing the sum.
