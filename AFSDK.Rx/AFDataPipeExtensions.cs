using System;

using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

using OSIsoft.AF.Data;

namespace AFSDK.Rx
{
    public static class AFDataPipeExtensions
    {
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
