using System.ServiceModel;

namespace WCFServiceWebRole1
{
    // UWAGA: możesz użyć polecenia „Zmień nazwę” w menu „Refaktoryzuj”, aby zmienić nazwę interfejsu „IService1” w kodzie i pliku konfiguracji.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        void Koduj(string nazwa, string tresc);

        [OperationContract]
        string Pobierz(string nazwa);

        [OperationContract]
        void AddUser(string login,string haslo);


        [OperationContract]
        bool CheckUser(string login);

    }
}
