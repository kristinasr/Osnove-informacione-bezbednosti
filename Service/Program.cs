using Common;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using CertificateManager;
using SecurityManager;

namespace Zadatak26
{
    class Program
    {
        static void Main(string[] args)
        {
            string name = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
            //string name = "wcfservice";
            Console.WriteLine(name);

            NetTcpBinding bindingClient = new NetTcpBinding();
            string addressClient = "net.tcp://localhost:4001/DatabaseService";

            //Windows Authentification
            bindingClient.Security.Mode = SecurityMode.Transport;
            bindingClient.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            bindingClient.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost hostClient = new ServiceHost(typeof(DatabaseService));
            hostClient.AddServiceEndpoint(typeof(IDatabaseManagement), bindingClient, addressClient);

            //Defining our principal settings.
            hostClient.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            hostClient.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

            ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
            newAudit.AuditLogLocation = AuditLogLocation.Application;
            newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;

            hostClient.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostClient.Description.Behaviors.Add(newAudit);

            hostClient.Open();

            Console.WriteLine("Service endpoint is opened.");

            NetTcpBinding bindingBackup = new NetTcpBinding();
            string addressBackup = "net.tcp://localhost:4002/Backup";

            //Certificate Authentication
            bindingBackup.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            ServiceHost hostBackup = new ServiceHost(typeof(DatabaseService));
            hostBackup.AddServiceEndpoint(typeof(IBackupService), bindingBackup, addressBackup);

            hostBackup.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            hostBackup.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            hostBackup.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, name);

            ServiceSecurityAuditBehavior newAudit2 = new ServiceSecurityAuditBehavior();
            newAudit2.AuditLogLocation = AuditLogLocation.Application;
            newAudit2.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;

            hostBackup.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostBackup.Description.Behaviors.Add(newAudit2);

            hostBackup.Open();

            Console.WriteLine("BackupService endpoint is opened.");

            Console.ReadLine();

            hostBackup.Close();

            hostClient.Close();
        }
    }
}

