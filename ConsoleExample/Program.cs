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
            AddStreamExample addStreamExample = new AddStreamExample();

            addStreamExample.Run();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();       
        }

    }
}
