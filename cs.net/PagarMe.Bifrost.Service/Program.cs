using PagarMe.Bifrost.CPP;
using PagarMe.Bifrost.Updates;
using PagarMe.Generic;
using System;
using System.ServiceProcess;

namespace PagarMe.Bifrost.Service
{
    internal static class Program
    {
        private static void Main(String[] args)
        {
            Log.TryLogOnException(() =>
            {
                var options = Options.Get(args);

                var runtime = Runtime4Windows.InstallIfNotFound();

                ServiceBase.Run(new BifrostService(options));

                runtime.Wait();
            });
        }
    }
}