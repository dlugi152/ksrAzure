using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO;
//using System.ServiceModel.Discovery;
using System.Xml;

[ServiceContract]
public interface IUsluga
{
    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "Add2/{a}/{b}")]
    int Add2(string a, string b);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "Add/{a}/{b}")]
    int Add(string a, string b);

    [OperationContract]
    [WebInvoke(ResponseFormat = WebMessageFormat.Json, UriTemplate = "testformularza")]
    int Formularz(Stream s);

}

namespace Klient
{

    class Program
    {

        [ServiceContract]
        public interface IService1
        {
            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "Add/{a}/{b}")]
            int Add(string a, string b);

            [OperationContract]
            [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "Add2/{a}/{b}")]
            int Add2(string a, string b);

            [OperationContract]
            [WebInvoke(ResponseFormat = WebMessageFormat.Json, UriTemplate = "testformularza")]
            int Formularz(Stream s);

            [OperationContract,
             WebGet(UriTemplate = "index.html"), XmlSerializerFormat]
            XmlDocument Index();
        }

        static void Main(string[] args)
        {
            var f = new ChannelFactory<IUsluga>(new WebHttpBinding(), new EndpointAddress("http://localhost:11028/Usluga"));
            f.Endpoint.Behaviors.Add(new WebHttpBehavior());
            var c = f.CreateChannel();
            Console.WriteLine(c.Add2("1", "1"));
            Console.WriteLine(c.Add("8", "9"));

            string str = "Pole=3&asdsa=54";

            Stream x;
            x = new MemoryStream();
            foreach (char t in str)
                x.WriteByte((byte)t);
            x.Position = 0;
            int s = c.Formularz(x);

            Console.WriteLine(s.ToString());
            Console.ReadKey();
            ((IDisposable)c).Dispose();


            //wyszukiwanie
            //Console.WriteLine("Test discovery. Szukam usług...");
            //DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint("soap.udp://localhost:54321"));
            //System.Collections.ObjectModel.Collection<EndpointDiscoveryMetadata> lst = discoveryClient.Find(new FindCriteria(typeof(IService1))).Endpoints;
            //discoveryClient.Close();
            //if (lst.Count > 0)
            //{
            //    var addr = lst[0].Address;
            //    var proxy = new ChannelFactory<IService1>(new WebHttpBinding(), addr);
            //    proxy.Endpoint.Behaviors.Add(new WebHttpBehavior());
            //    var d = proxy.CreateChannel();
            //    Console.WriteLine(d.Add("3", "6"));
            //    ((IDisposable)d).Dispose();
            //}

            Console.ReadLine();
        }
    }
}
