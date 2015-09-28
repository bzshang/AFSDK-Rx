using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

namespace AFSDK.Rx
{
    public class AFObservableDataPipe
    {
        private AFDataPipe _dataPipe;

        private readonly IScheduler _scheduler;

        public AFObservableDataPipe()
        {
            _dataPipe = new AFDataPipe();
            _scheduler = new EventLoopScheduler();
        }

        public AFErrors<AFAttribute> AddSignups(IList<AFAttribute> attributes)
        {
            return _dataPipe.AddSignups(attributes);
        }

        public IObservable<AFDataPipeEvent> CreateObservable(double seconds)
        {
            TimeSpan interval = TimeSpan.FromSeconds(seconds);

            return Observable.Create<AFDataPipeEvent>(observer =>
            {
                IDisposable dpToken = _dataPipe.Subscribe(observer);

                IDisposable scheduleToken = _scheduler.Schedule(dpToken, interval, (state, recurse) =>
                {
                    try
                    {

                        bool hasMoreEvents = true;
                        while (hasMoreEvents)
                        {
                            _dataPipe.GetObserverEvents(out hasMoreEvents);
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
                    dpToken.Dispose();
                });

            }).Do(x => Console.WriteLine("emitting")).Publish().RefCount();

        }

    }
}
