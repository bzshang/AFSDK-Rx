using System;

using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

using OSIsoft.AF.Data;

namespace AFSDK.Rx
{
    public static class AFDataPipeExtensions
    {
        /// <summary>
        /// Extension method on AFDataPipe to create an IObservable. A subscription to the underlying AFDataPipe is made.
        /// An EventLoopScheduler is set up to poll GetObserverEvents at a configurable interval.
        /// The Publish/RefCount construct is used to handle subscription side-effects and share the underlying AFDataPipe subscription
        /// with mulitple observers.
        /// See http://www.introtorx.com/content/v1.0.10621.0/14_HotAndColdObservables.html for Publish/RefCount documentation.
        /// See http://davesexton.com/blog/post/To-Use-Subject-Or-Not-To-Use-Subject.aspx for the Publish/RefCount use case.
        /// See http://stackoverflow.com/questions/14396449/why-are-subjects-not-recommended-in-net-reactive-extensions for EventLoopScheduler and recursion idea.
        /// See http://www.zerobugbuild.com/?p=259 for recursive scheduling idea.
        /// </summary>
        /// <param name="dataPipe"></param>
        /// <param name="seconds">Number of seconds to wait before calling GetObserverEvents</param>
        /// <returns></returns>
        public static IObservable<AFDataPipeEvent> CreateObservable(this AFDataPipe dataPipe, double seconds = 1)
        {
            TimeSpan interval = TimeSpan.FromSeconds(seconds);

            return Observable.Create<AFDataPipeEvent>(observer =>
            {
                IDisposable dpToken = dataPipe.Subscribe(observer);

                EventLoopScheduler scheduler = new EventLoopScheduler();

                IDisposable scheduleToken = scheduler.Schedule(dpToken, interval, (state, recurse) =>
                {
                    try
                    {
                        bool hasMoreEvents = true;
                        while (hasMoreEvents)
                        {
                            dataPipe.GetObserverEvents(out hasMoreEvents);
                        }

                        recurse(dpToken, interval);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });

                return Disposable.Create(() =>
                {
                    scheduleToken.Dispose();
                    scheduler.Dispose();
                    dpToken.Dispose();
                });

            })
            .Publish()
            .RefCount();

        }
    }
}
