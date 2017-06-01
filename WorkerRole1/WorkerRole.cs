using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        const string KONTENER = "bloby";
        const string KONTENER_KODOWANY = "bloby_kodowane";
        const string KOLEJKA = "kolejka";

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            // ustawienie kolejki
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(KOLEJKA);
            queue.CreateIfNotExists();

            // ustawienie kontenera blobow
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(KONTENER);

            // ustawienie kontenera szyfrowanych blobow
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(KONTENER_KODOWANY);

            while (true)
            {
                // odebranie wiadomosci
                var msg = queue.GetMessage();
                if (msg == null) continue;
                queue.DeleteMessage(msg);

                // odczytanie tresci bloba
                var blob = blobContainer.GetBlockBlobReference(msg.AsString);
                var s2 = new System.IO.MemoryStream();
                blob.DownloadToStream(s2);
                string content = System.Text.Encoding.UTF8.GetString(s2.ToArray());

                // kodowanie tresci ROT13
                string new_content = Szyfruj(content);

                // zapisanie bloba
                container.CreateIfNotExists();
                var blob2 = blobContainer.GetBlockBlobReference(msg.AsString);
                var bytes = new System.Text.ASCIIEncoding().GetBytes(new_content);
                var s = new System.IO.MemoryStream(bytes);
                blob2.UploadFromStream(s);
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();
            Trace.TraceInformation("WorkerRole1 has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");
            base.OnStop();
            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private string Szyfruj(string tekst)
        {
            var data = tekst.ToCharArray();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] >= 'a' && data[i] <= 'z')
                {
                    // rozbite na duzo krokow bo byly problemy z typami
                    var c = data[i] - 'a';
                    c += (char) 13;
                    c %= 26;
                    c += 'a';

                    data[i] = (char) c;
                }
                else if (data[i] >= 'A' && data[i] <= 'Z')
                {
                    var c = data[i] - 'A';
                    c += (char) 13;
                    c %= 26;
                    c += 'A';

                    data[i] = (char) c;
                }
            }

            return data.ToString();
        }
    }
}