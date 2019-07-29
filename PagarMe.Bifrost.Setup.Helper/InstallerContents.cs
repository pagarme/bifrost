using PagarMe.Generic;
using System;
using System.IO;
using System.IO.Compression;
using Version = PagarMe.Generic.Version;

namespace PagarMe.Bifrost.Setup.Helper
{
    internal class InstallerContents : ProgramVersion
    {
        public static void PublishLinux()
        {
            var installersPath = getInstallersPath();

            makeLinuxZip(installersPath);

            var jsonPath = Path.Combine(installersPath, "update-linux.json");
            var json = $@"{{ ""last_version_name"": ""{Version}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static void makeLinuxZip(String installersPath)
        {
            var originFiles = Path.Combine(MainDir, "bin", "Debug", "Linux");

            var msi = Path.Combine(originFiles, "windows.msi");
            File.Delete(msi);
            var exe = Path.Combine(originFiles, "setup.exe");
            File.Delete(exe);

            FileExtension.Delete(installersPath, "*.zip");

            var zip = Path.Combine(installersPath, $"bifrost-linux-{Version}.zip");
            ZipFile.CreateFromDirectory(originFiles, zip, CompressionLevel.Optimal, false);
        }

        public static void PublishWindows()
        {
            var installersPath = getInstallersPath();

            FileExtension.Delete(installersPath, "*.msi");

            var originMsi = Path.Combine(MainDir, "bin", "Debug", "Windows", "BifrostInstaller.msi");
            var msi = Path.Combine(installersPath, $"bifrost-windows-{Version}.msi");
            File.Copy(originMsi, msi, true);

            var jsonPath = Path.Combine(installersPath, "update-windows.json");
            var json = $@"{{ ""last_version_name"": ""{Version}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static string getInstallersPath()
        {
            var installersPath = Path.Combine(MainDir, "installers");

            if (!Directory.Exists(installersPath))
                Directory.CreateDirectory(installersPath);

            return installersPath;
        }
    }
}
