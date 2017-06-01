using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using WCFServiceWebRole1;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            //http://localhost:11028/
            ServiceHost restHost = new ServiceHost(typeof(Service1), new Uri("http://localhost:11028"));
            restHost.AddServiceEndpoint(typeof(IService1), new WebHttpBinding(), "Usluga").Behaviors.Add(new WebHttpBehavior());

            //restHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            //restHost.AddServiceEndpoint(new UdpDiscoveryEndpoint("soap.udp://localhost:54321"));

            restHost.Open();
            Console.WriteLine("host już działa");
            Console.ReadKey();
        }
    }
}
