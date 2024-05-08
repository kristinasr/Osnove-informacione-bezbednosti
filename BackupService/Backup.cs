using SecurityManager;
using Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using CertificateManager;

namespace BackupService
{
    public class Backup : ChannelFactory<IBackupService>, IBackupService
    {
        IBackupService factory;

        public Backup(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
            Console.WriteLine(cltCertCN);

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

            factory = this.CreateChannel();
        }

        public (List<byte[]>, byte[], byte[]) PullDatabase()
        {
            (List<byte[]>, byte[], byte[]) entryList = (new List<byte[]>(), null, null);
            try
            {
                entryList = factory.PullDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            return entryList;
        }
    }
}