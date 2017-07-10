using Org.BouncyCastle.Utilities.IO.Pem;
using PagarMe.Generic;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    public class NetCoreCertificate : IDisposable
    {
        private X509Certificate2 certificate;
        public String FilePath { get; private set; }
        public String Filename { get; private set; }
        private String crtPath;
        private String keyPath;

        public static NetCoreCertificate Export(X509Certificate2 certificate, String path)
        {
            var newCoreCert = new NetCoreCertificate(certificate, path);

            newCoreCert.createCrt();
            newCoreCert.createKey();

            return newCoreCert;
        }

        private NetCoreCertificate(X509Certificate2 certificate, String path)
        {
            this.certificate = certificate;
            FilePath = path;
            Filename = certificate.Subject.CleanSubject();
        }

        public delegate void ExecuteBash(String scriptName, params String[] parameters);

        private void createCrt()
        {
            crtPath = createPemFile("CERTIFICATE", certificate.RawData, "crt");
        }

        public void createKey()
        {
            keyPath = createPemFile("RSA PRIVATE KEY", certificate.GetPrivateKeyRawData(), "key");
        }

        private String createPemFile(String title, Byte[] content, String extension)
        {
            if (content == null) return null;

            var certPath = Path.Combine(FilePath, $"{Filename}.{extension}");

            using (var stream = new FileStream(certPath, FileMode.Create))
            using (var textWriter = new StreamWriter(stream))
            {
                var pemWriter = new PemWriter(textWriter);

                var pemObj = new PemObject(title, content);
                pemWriter.WriteObject(pemObj);

                pemWriter.Writer.Flush();
                pemWriter.Writer.Close();
            }

            return certPath;
        }

        public void Dispose()
        {
            FileExtension.DeleteIfExists(crtPath);
            FileExtension.DeleteIfExists(keyPath);
        }
    }
}