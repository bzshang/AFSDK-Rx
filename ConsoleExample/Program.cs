using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Reactive.Linq;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

using AFSDK.Rx;
using System.Diagnostics;

namespace ConsoleExample
{
    class Program
    {
         

        static void Main(string[] args)
        {
            

            AFElement element = AFObject.FindObject(@"\\BSHANGE6430s\Sandbox\Reactive") as AFElement;

            AFAttributeList attrList = new AFAttributeList(element.Attributes);

            AFDataPipe dataPipe = new AFDataPipe();
            var errors = dataPipe.AddSignups(attrList);
            IObservable<AFDataPipeEvent> _observableDataPipe = dataPipe.CreateObservable(1);

            //AFObservableDataPipe obsDataPipe = new AFObservableDataPipe();
            //var errors = obsDataPipe.AddSignups(attrList);
            //IObservable<AFDataPipeEvent> _observableDataPipe = obsDataPipe.CreateObservable(1);        

            IObservable<AFDataPipeEvent> sinusoidSnapshot = _observableDataPipe
                .Where(evt => evt.Value.Attribute.Name == "SINUSOID" && evt.Action == AFDataPipeAction.Add);

            IObservable<AFDataPipeEvent> cdt158Snapshot = _observableDataPipe
                .Where(evt => evt.Value.Attribute.Name == "CDT158" && evt.Action == AFDataPipeAction.Add);

            IObservable<AFValue> sumStream = sinusoidSnapshot
                .CombineLatest(cdt158Snapshot, (sin, cdt) => new[] { sin, cdt })
                .Select(evts =>
                {
                    double sinusoid = Convert.ToDouble(evts[0].Value.Value);
                    double cdt158 = Convert.ToDouble(evts[1].Value.Value);
                    double result = sinusoid + cdt158;

                    DateTime timestamp = evts[0].Value.Timestamp > evts[1].Value.Timestamp ? evts[0].Value.Timestamp : evts[1].Value.Timestamp;
                    return new AFValue(result, timestamp);
                });

            //Console.WriteLine("Press any key to subscribe");
            //Console.ReadKey();

            IDisposable subscription = sumStream.Subscribe(val =>
            {
                Console.WriteLine("Timestamp: {0}, SUM_SINSUOID_CDT158: {1}", val.Timestamp.ToString("HH:mm:ss.ffff"), val.Value);            
            });

            //Console.WriteLine("Press any key to subscribe");
            //Console.ReadKey();

            //IDisposable subscription2 = sinusoidSnapshot.Subscribe(evt =>
            //{
            //    Console.WriteLine("Timestamp: {0}, SINUSOID: {1}", evt.Value.Timestamp.ToString("HH:mm:ss.ffff"), evt.Value.Value);
            //});

            Console.WriteLine("Press any key to unsubscribe");
            Console.ReadKey();

            subscription.Dispose();

            //Console.WriteLine("Press any key to unsubscribe");
            //Console.ReadKey();

            //subscription2.Dispose();


            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static void UpdateConsole()
        {


        }
    }
}
