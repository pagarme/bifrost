﻿using NLog;
using PagarMe.Generic;
using System;
using System.IO;
using System.Net.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace PagarMe.Bifrost.Certificates.TLS
{
    public class TLSConfig
    {
        public static bool ClientValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static String algorithm => "SHA256WithRSA";
        static Int32 validYears => 100;
        static Int32 keyStrength => 2048;
        static String subjectTls => "CN=" + Address;
        static String subjectCa => "CN=Bifrost";

        public static String Address;

        static CertificateChain certificateChain = new CertificateChain(algorithm, validYears, keyStrength);

        public static X509Certificate2 Get()
        {
            return certificateChain.Get(subjectTls, subjectCa);
        }

        internal static X509Certificate2 GenerateIfNotExists()
        {
            return certificateChain.GenerateIfNotExists(subjectTls, subjectCa);
        }

        internal static void GrantLogAccess()
        {
            var logger = LogManager.GetCurrentClassLogger();
            var fullPath = logger.GetLogDirectoryPath();

            var info = new DirectoryInfo(fullPath);
            var security = info.GetAccessControl();

            security.AddAccessRule(new FileSystemAccessRule(
                GetServiceUser(), 
                FileSystemRights.Read | FileSystemRights.Write, 
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, 
                PropagationFlags.NoPropagateInherit, 
                AccessControlType.Allow
            ));

            info.SetAccessControl(security);
        }

        internal static IdentityReference GetServiceUser()
        {
            return new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
        }

    }
}