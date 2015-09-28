using System;
using System.Threading;
using System.Threading.Tasks;

using System.Reactive.Linq;
using System.Reactive.Disposables;

using OSIsoft.AF.Data;
using System.Reactive.Concurrency;

namespace AFSDK.Rx
{
    public static class AFDataPipeExtensions
    {
        //public static IObservable<AFDataPipeEvent> CreateObservable(this AFDataPipe dataPipe, double interval)
        //{
        //    return Observable.Create<AFDataPipeEvent>(observer =>
        //    {
        //        IDisposable dpToken = dataPipe.Subscribe(observer);

        //        Timer timer = new Timer();
        //        timer.Interval = interval;
        //        timer.Elapsed += (object sender, ElapsedEventArgs e) =>
        //        {
        //            bool hasMoreEvents = true;
        //            while (hasMoreEvents)
        //            {
        //                dataPipe.GetObserverEvents(out hasMoreEvents);
        //            }
        //        };

        //        timer.Start();

        //        return Disposable.Create(() =>
        //        {
        //            timer.Dispose();
        //            dpToken.Dispose();     
        //        });
        //    });
        //}

        public static IObservable<AFDataPipeEvent> CreateObservable(this AFDataPipe dataPipe, double seconds)
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

            }).Publish().RefCount();

        }

        public static IObservable<AFDataPipeEvent> CreateObservable(this AFDataPipe dataPipe, double interval, CancellationToken token)
        {
            return Observable.Create<AFDataPipeEvent>(observer =>
            {
                IDisposable dpToken = dataPipe.Subscribe(observer);

                Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        bool hasMoreEvents = true;
                        while (hasMoreEvents)
                        {
                            dataPipe.GetObserverEvents(out hasMoreEvents);
                        }
                        Thread.Sleep(Convert.ToInt32(interval));
                    }

                }, token);

                

                return Disposable.Create(() =>
                {
                    //timer.Dispose();
                    dpToken.Dispose();
                });
            });
        }
    }
}
