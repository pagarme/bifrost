using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class UnixStore : Store
    {
        private static readonly String certificatesPath = createIfNotExists(getStorePath());
        private static readonly String pfxPath = Path.Combine(certificatesPath, $"{TLSConfig.Address}.pfx");

        private static String createIfNotExists(String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return Log.TryLogOnException(() =>
            {
                if (!File.Exists(pfxPath)) return null;
                return new X509Certificate2(pfxPath);
            });
        }

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            Log.TryLogOnException(() =>
            {
                executeBash("unix-install-cert-tools.sh");

                using (var netCore = NetCoreCertificate.Export(tls, certificatesPath))
                {
                    executeBash("unix-export-cert.sh", netCore.FilePath, netCore.Filename);
                    executeBash("unix-store-os.sh", netCore.FilePath, netCore.Filename);
                    executeBash("unix-store-firefox.sh", netCore.FilePath, netCore.Filename);
                }

                Log.Me.Info("Finish generating");
            });
        }

        private static void executeBash(String scriptName, params String[] parameters)
        {
            var scriptPath = Path.Combine("CertificateScripts", scriptName);
            var result = Terminal.Run("sh", scriptPath.ArrayWith(parameters));
            if (!result.Succedded)
            {
                Log.Me.Error($"Output: {result.Output}");
                Log.Me.Error($"Error: {result.Error}");
                throw new Exception($"Could not install certificate: bash {scriptName} exited with code {result.Code}");
            }
        }

        private static String getStorePath()
        {
            if (File.Exists(@"/etc/redhat-release"))
            {
                return "/etc/pki/ca-trust/source/anchors";
            }

            if (File.Exists(@"/etc/debian_version"))
            {
                return "/usr/local/share/ca-certificates";
            }

            if (File.Exists("/etc/arch-release"))
            {
                return "/etc/ca-certificates/trust-source/anchors/";
            }

            throw new NotImplementedException("Certificates for this distro can not be installed");
        }
    }
}