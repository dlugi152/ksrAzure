using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Tabela
{
    public class TestEntity : TableEntity
    {
        public TestEntity(string rk, string pk)
        {
            this.PartitionKey = pk; // ustawiamy klucz partycji
            this.RowKey = rk; // ustawiamy klucz główny
            // this.Timestamp; jest tylko do odczytu
        }
        public TestEntity() { }
        public string s { get; set; }
        public int i { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // w przypadku emulatora
            //tworzenie
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient cl = account.CreateCloudTableClient();
            
            var table = cl.GetTableReference("abcde");
            table.DeleteIfExists();//usuniecie

            table = cl.GetTableReference("abcde");
            table.CreateIfNotExists(); // utworzenie tabeli jeżeli nie istnieje
            //wstawianie pojedynczych 
            var e = new TestEntity("1", "1");
            TableOperation op1 = TableOperation.Insert(e);
            table.Execute(op1);
            var f = new TestEntity("2", "2");
            TableOperation op2 = TableOperation.Insert(f);
            table.Execute(op2);
            var g = new TestEntity("3", "3");
            TableOperation op3 = TableOperation.Insert(g);
            table.Execute(op3);
            var h = new TestEntity("4", "4");
            TableOperation op4 = TableOperation.Insert(h);
            table.Execute(op4); 
            var i = new TestEntity("5", "5");
            TableOperation op5 = TableOperation.Insert(i);
            table.Execute(op5);
            Console.WriteLine("wpisano 5 dane");
            //wstawianie wielu - ten sam partition key
            TableBatchOperation batchOperation = new TableBatchOperation();
            batchOperation.Insert(new TestEntity("6", "6"));
            batchOperation.Insert(new TestEntity("7", "6"));
            batchOperation.Insert(new TestEntity("8", "6"));
            batchOperation.Insert(new TestEntity("9", "6"));
            batchOperation.Insert(new TestEntity("10", "6"));
            batchOperation.Insert(new TestEntity("11", "6"));
            table.ExecuteBatch(batchOperation);
            //wczytywanie
            TableOperation op = TableOperation.Retrieve<TestEntity>("5", "5");
            var res = table.Execute(op);
            TestEntity a = (TestEntity)res.Result;
            Console.WriteLine(a.RowKey.ToString());
            //wczytywanie 2 - wszystkie co maja partition key 6 lub row key 3
            string pkey = "6";
            string rkey = "3";
            TableQuery<TestEntity> query = new TableQuery<TestEntity>()
            .Where(TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,pkey),
            TableOperators.Or, 
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,rkey)));
            foreach (TestEntity x in table.ExecuteQuery(query))
                Console.WriteLine("{0}, {1}", x.PartitionKey.ToString(), x.RowKey.ToString());

            // account utworzone jak w przypadku tabel
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("mojkontener");
            container.CreateIfNotExists();
            // pobranie referencji do blokowego BLOBa
            var blob = container.GetBlockBlobReference("mojblob");
            // null gdy nie istnieje
            // zapisanie strumienia do BLOBa
            var bytes = new System.Text.ASCIIEncoding().GetBytes("wartosc");
            var s = new System.IO.MemoryStream(bytes);
            blob.UploadFromStream(s);

            var s2 = new System.IO.MemoryStream();
            blob.DownloadToStream(s2);
            string content = System.Text.Encoding.UTF8.GetString(s2.ToArray());
            // usunięcie
            //blob.Delete();
            //blob.DeleteIfExists();
            // lista BLOBów
            string lst = "";
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob2 = (CloudBlockBlob)item;
                    lst += String.Format("Block blob of length {0}: {1}",
                               blob2.Properties.Length, blob.Uri) + "\n";
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;
                    lst += String.Format("Page blob of length {0}: {1}",
                               pageBlob.Properties.Length, pageBlob.Uri) + "\n";
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    lst += String.Format("Directory: {0}", directory.Uri) + "\n";
                }
            }
            Console.WriteLine(lst);
        }
    }
}