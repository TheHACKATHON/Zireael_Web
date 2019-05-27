using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wcf_CeadChat_ServiceLibrary;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(CeadChatService));
           
            try
            {
                host.Open(new TimeSpan(0, 3, 0));
                Console.WriteLine("Service is ready");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n\n");
                Thread.Sleep(2000);
                Console.ReadKey();
            }
        }
    }
}
