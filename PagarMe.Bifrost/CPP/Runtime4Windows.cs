using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using PagarMe.Generic;

namespace PagarMe.Bifrost.CPP
{
    public class Runtime4Windows
    {
        public static Task InstallIfNotFound()
        {
            return Task.Run(() =>
            {
                if (ProgramEnvironment.IsWindows)
                {
                    Log.TryLogOnException(installIfNotFound);
                }
            });
        }

        private static void installIfNotFound()
        {
            Log.Me.Info("Verify C++ runtime for windows");

            var needInstall = verify();

            if (needInstall)
            {
                Log.Me.Info("Installing C++ runtime");
                var arch = Environment.Is64BitProcess ? "64" : "86";
                var result = Terminal.Run($"vc_redist.x{arch}.exe", "/q");

                if (!result.Succedded)
                {
                    var message =
                        "Could not install Visual C++ Runtime:" + Environment.NewLine
                        + result.Output + Environment.NewLine
                        + result.Error + Environment.NewLine;

                    throw new Exception(message);
                }
            }

            Log.Me.Info("C++ runtime installed");
        }

        private static Boolean verify()
        {
            var mainKeyName = @"Installer\Dependencies";

            var value = Registry.ClassesRoot.OpenSubKey(mainKeyName);

            if (value == null)
                return true;

            var archCode = Environment.Is64BitProcess ? "amd64" : "x86";
            var archKeyNames = value.GetSubKeyNames()
                .Where(n => n.Contains(archCode))
                .ToList();

            if (!archKeyNames.Any())
                return true;

            foreach (var archKeyName in archKeyNames)
            {
                var archKey = value.OpenSubKey(archKeyName);
                var displayName = archKey?.GetValue("DisplayName");

                if (displayName != null && displayName.ToString().Contains("C++"))
                    return false;
            }

            return true;
        }
    }
}
