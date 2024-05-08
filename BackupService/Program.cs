using CertificateManager;
using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using System.Xml.Serialization;

namespace BackupService
{
    class Program
    {
        public static SecretMasks sm = new SecretMasks();
        public static AES aes = new AES();

        static void Main(string[] args)
        {
            string srvCertCN = "wcfservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:4002/Backup"),
                                      new X509CertificateEndpointIdentity(srvCert));

            using (Backup proxy = new Backup(binding, address))
            {
                List<DatabaseEntry> entryList = new List<DatabaseEntry>();
                while (true)
                {
                    Thread.Sleep(10000);

                    (List<byte[]>, byte[], byte[]) results = proxy.PullDatabase();
                    //List<DatabaseEntry> entryList = new List<DatabaseEntry>();
                    List<byte[]> templList = new List<byte[]>();

                    templList = results.Item1;  // Kriptovana lista
                    byte[] yourKey = results.Item2; // Kriptovan kljuc
                    byte[] yourIV = results.Item3;  // Kriptovan IV vektor

                    // Dekriptovanj kljuca i IV
                    (byte[], byte[]) retV = sm.KeyAndIVEncryption(yourKey, yourIV);
                    byte[] decryptedyourKey = retV.Item1;
                    byte[] decrypteyourIV = retV.Item2;

                    DatabaseEntry entryPOM = new DatabaseEntry();

                    List<DatabaseEntry> listOfEntities = new List<DatabaseEntry>();

                    // Dekripotavnje podataka
                   
                    string decryptedString = "";


                    foreach (var item in templList)
                    {

                        decryptedString += aes.DecryptBytesToString_Aes(item, decryptedyourKey, decrypteyourIV);
                        decryptedString += ",";


                    }
                    string[] parts = decryptedString.Split(',');

                    foreach (var p in parts)
                    {
                        if (p.Length != 0)
                        {
                            //Console.Write(p);
                            listOfEntities.Add(CreateFromString(p));
                            //Console.ReadLine();
                        }

                    }


                    SaveDatabase(listOfEntities);
                }
            }
        }




        private static double ParseDouble(string[] pom, int index)
        {
            if (index >= 0 && index < pom.Length)
            {
                string value = pom[index].Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    if (double.TryParse(value, out double result))
                    {
                        return result;
                    }
                    else
                    {

                        return 0.0;
                    }
                }
                else
                {

                    return 0.0;
                }
            }
            else
            {

                return 0.0;
            }
        }


        private static void SaveDatabase(List<DatabaseEntry> entryList)
        {
            string databasePath = @"..\..\BackupDatabase.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(List<DatabaseEntry>));

            using (var stream = File.Create(databasePath))
            {
                serializer.Serialize(stream, entryList);
                Console.WriteLine("Backup saved.");
            }
        }

        public static DatabaseEntry CreateFromString(string input)
        {

            input = input.Replace("Id: ", "").Replace("Region: ", "").Replace("City: ", "").Replace("Year: ", "").Replace("Consumption: [", "");


            string[] parts = input.Split(new[] { ' ', ']' }, StringSplitOptions.RemoveEmptyEntries);


            if (parts.Length < 13)
            {
                throw new ArgumentException("Invalid input format");
            }

            DatabaseEntry newData = new DatabaseEntry
            {
                Id = int.Parse(parts[0]),
                Region = parts[1].Trim(),
                City = parts[2].Trim(),
                Year = parts[3],
                Consumption = new double[12]
            };

            for (int i = 0; i < 12; i++)
            {
                newData.Consumption[i] = double.Parse(parts[i + 4]);
            }



            return newData;
        }




    }
}


