using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;


using System.Reactive.Linq;

namespace AFSDK.Rx
{
    public static class ObservableExtensions
    {

        /// <summary>
        /// Extension method on IObservable to print incoming items to the console.
        /// See http://www.introtorx.com/content/v1.0.10621.0/07_Aggregation.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="name"></param>
        public static void Dump<T>(this IObservable<T> source, string name)
        {
            source.Subscribe(
                i => Console.WriteLine("{0}-->{1}", name, i),
                ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
                () => Console.WriteLine("{0} completed", name));
        }

        /// <summary>
        /// Add the snapshot values of two streams and return an observable AFDataPipeEvent
        /// </summary>
        /// <param name="source1"></param>
        /// <param name="source2"></param>
        /// <returns></returns>
        public static IObservable<AFDataPipeEvent> Add(this IObservable<AFDataPipeEvent> source1, IObservable<AFDataPipeEvent> source2)
        {
            return source1
                .CombineLatest(source2, (s1, s2) => new[] { s1, s2 })
                .Select(evts =>
                {
                    //TODO: handle different types
                    double d1 = Convert.ToDouble(evts[0].Value.Value);
                    double d2 = Convert.ToDouble(evts[1].Value.Value);
                    double result = d1 + d2;

                    DateTime timestamp = evts[0].Value.Timestamp > evts[1].Value.Timestamp ? evts[0].Value.Timestamp : evts[1].Value.Timestamp;

                    AFValue afVal = new AFValue(result, timestamp);
                    AFDataPipeEvent dpEvent = new AFDataPipeEvent(AFDataPipeAction.Add, afVal);

                    return dpEvent;
                });
        }
    }
}
