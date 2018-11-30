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
            var updatesPath = getUpdatesPath();

            makeLinuxZip(Version, updatesPath);

            var jsonPath = Path.Combine(updatesPath, "update-linux.json");
            var json = $@"{{ ""last_version_name"": ""{Version}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static void makeLinuxZip(String currentVersion, String updatesPath)
        {
            var originFiles = Path.Combine(MainDir, "bin", "Debug", "Linux");

            var msi = Path.Combine(originFiles, "windows.msi");
            File.Delete(msi);
            var exe = Path.Combine(originFiles, "setup.exe");
            File.Delete(exe);

            var zipDestination = Path.Combine(updatesPath, $"bifrost-installer-{currentVersion}.zip");
            FileExtension.DeleteIfExists(zipDestination);
            ZipFile.CreateFromDirectory(originFiles, zipDestination, CompressionLevel.Optimal, false);
        }

        public static void PublishWindows()
        {
            var updatesPath = getUpdatesPath();

            var originMsi = Path.Combine(MainDir, "bin", "Debug", "Windows", "BifrostInstaller.msi");
            var msiDestination = Path.Combine(updatesPath, $"bifrost-installer-{Version}.msi");
            File.Copy(originMsi, msiDestination, true);

            var jsonPath = Path.Combine(updatesPath, "update-windows.json");
            var json = $@"{{ ""last_version_name"": ""{Version}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static string getUpdatesPath()
        {
            var updatesPath = Path.Combine(MainDir, "PagarMe.Bifrost.Updates");

            if (!Directory.Exists(updatesPath))
                Directory.CreateDirectory(updatesPath);

            return updatesPath;
        }
    }
}