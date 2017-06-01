using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WCFServiceWebRole1
{
    // UWAGA: możesz użyć polecenia „Zmień nazwę” w menu „Refaktoryzuj”, aby zmienić nazwę klasy „Service1” w kodzie, usłudze i pliku konfiguracji.
    // UWAGA: aby uruchomić klienta testowego WCF w celu przetestowania tej usługi, wybierz plik Service1.svc lub Service1.svc.cs w eksploratorze rozwiązań i rozpocznij debugowanie.
    public class Service1 : IService1
    {
        const string KONTENER = "bloby";
        const string KONTENER_KODOWANY = "blobykodowane";
        const string KOLEJKA = "kolejka";

        public void AddUser(string login, string haslo)
        {
            SqlConnection c = new SqlConnection();
            //c.ConnectionString = "Driver={SQL Server Native Client 11.0};"+
            //"Server=(LocalDB)\\v11.0;Database=nazwa;Trusted_Connection=yes;";
            c.ConnectionString =
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=nazwa;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText = "drop table if exists users;create table users(login varchar(100) unique not null, haslo varchar(100) not null)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "insert into users values('" + login + "', '" + haslo + "')";
            cmd.ExecuteNonQuery();
            c.Close();

        }

        public bool CheckUser(string login)
        {
            SqlConnection c = new SqlConnection();
            //c.ConnectionString = "Driver={SQL Server Native Client 11.0};"+
            //"Server=(LocalDB)\\v11.0;Database=nazwa;Trusted_Connection=yes;";
            c.ConnectionString =
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=nazwa;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText = "select * from users where login='"+login+"';";
            if (cmd.ExecuteScalar() == null)
            {
                c.Close();
                return false;
            }
            c.Close();
            return true;
        }

        public void Koduj(string nazwa, string tresc)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(KONTENER);
            container.CreateIfNotExists();

            // pobranie referencji do blokowego BLOBa
            var blob = container.GetBlockBlobReference(nazwa);

            // zapisanie strumienia do BLOBa
            var bytes = new ASCIIEncoding().GetBytes(tresc);
            var s = new MemoryStream(bytes);
            blob.UploadFromStream(s);

            // dodanie nazwy bloba do kolejki
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(KOLEJKA);
            queue.CreateIfNotExists();
            var msg = new CloudQueueMessage(nazwa);
            queue.AddMessage(msg);
        }

        public string Pobierz(string nazwa)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(KONTENER);
            container.CreateIfNotExists();

            // pobranie referencji do blokowego BLOBa
            var blob = container.GetBlockBlobReference(nazwa);
            if (blob == null) return null;

            var s2 = new MemoryStream();
            blob.DownloadToStream(s2);
            string content = Encoding.UTF8.GetString(s2.ToArray());

            return content;
        }
    }
}
