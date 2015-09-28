using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

using System.Reactive.Linq;

using AFSDK.Rx;

namespace ConsoleExample
{
    public class AddStreamExample
    {

        public AddStreamExample()
        {

        }

        public void Run()
        {
            AFElement element = AFObject.FindObject(@"\\BSHANGE6430s\Sandbox\Reactive") as AFElement;
            AFAttributeList attrList = new AFAttributeList(element.Attributes);

            AFDataPipe dataPipe = new AFDataPipe();
            var errors = dataPipe.AddSignups(attrList);

            IObservable<AFDataPipeEvent> obsDataPipe = dataPipe.CreateObservable(); 

            IObservable<AFDataPipeEvent> sinusoidSnapshot = obsDataPipe
                .Where(evt => evt.Value.Attribute.Name == "SINUSOID" && evt.Action == AFDataPipeAction.Add);

            IObservable<AFDataPipeEvent> cdt158Snapshot = obsDataPipe
                .Where(evt => evt.Value.Attribute.Name == "CDT158" && evt.Action == AFDataPipeAction.Add);

            IObservable<AFDataPipeEvent> sumStream = sinusoidSnapshot.Add(cdt158Snapshot);

            IDisposable subscription = sumStream.Subscribe(evt =>
            {
                Console.WriteLine("Timestamp: {0}, SINUSOID + CDT158: {1}", evt.Value.Timestamp.ToString("HH:mm:ss.ffff"), evt.Value.Value);
            });

            Console.WriteLine("Press any key to unsubscribe");
            Console.ReadKey();

            subscription.Dispose();
            dataPipe.Dispose();
        }
    }
}
