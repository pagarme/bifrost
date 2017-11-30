using System.ServiceProcess;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class BifrostService : ServiceBase
    {
        private readonly Options options;
        private MposBridge bridge;

        public BifrostService(Options options)
        {
            this.options = options;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Me.Info("Bifrost Service Bridge");

            base.OnStart(args);

            Log.TryLogOnException(() =>
            {
                bridge = new MposBridge(options);

                Log.Me.Info("Starting server");
                bridge.Start();
            });
        }

        protected override void OnShutdown()
        {
            Log.TryLogOnException(() =>
            {
                Log.Me.Info("Shuting down server");
                bridge?.Stop();
            });

            base.OnShutdown();
        }

        protected override void OnStop()
        {
            Log.TryLogOnException(() =>
            {
                Log.Me.Info("Stopping server");
                bridge?.Stop();
            });

            base.OnStop();
        }
    }
}