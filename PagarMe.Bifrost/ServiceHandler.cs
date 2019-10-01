using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp.Server;
using PagarMe.Generic;
using PagarMe.Bifrost.Certificates.Generation;
using System.Linq;
using System.Threading.Tasks;
using PagarMe.Bifrost.Data;

namespace PagarMe.Bifrost
{
    public class ServiceHandler : IDisposable
    {
        private static readonly Dictionary<string, Context> contexts
            = new Dictionary<string, Context>();

        private static Boolean contextsLocked = false;

        private WebSocketServer server;
        private MessagesHandler messagesHandler;

        public Options Options { get; }
        internal DeviceManager DeviceManager { get; }

        public ServiceHandler(Options options)
        {
            Options = options;
            DeviceManager = new DeviceManager(Log.TryLogOnException);
        }


        public void Start(Boolean ssl = true)
        {
            var addresses = Dns.GetHostAddresses(Options.BindAddress);
            server = new WebSocketServer(addresses[0], Options.BindPort, ssl);

            TLSConfig.Address = Options.BindAddress;

            if (ssl)
            {
                server.SslConfiguration.ServerCertificate = TLSConfig.Get();

                server.SslConfiguration.CheckCertificateRevocation = false;
                server.SslConfiguration.ClientCertificateRequired = false;
                server.SslConfiguration.ClientCertificateValidationCallback = TLSConfig.ClientValidate;

                server.KeepClean = false;
            }

            server.Log.File = Log.GetLogFilePath();

            messagesHandler = new MessagesHandler(this);
            server.AddWebSocketService("/mpos", () => messagesHandler);
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Dispose()
        {
            DeviceManager.Dispose();
        }

        public String GetDeviceContextName(String deviceId)
        {
            lock (contexts)
            {
                return contexts
                    .Where(c => c.Value != null && c.Value.DeviceId == deviceId)
                    .Select(c => c.Key)
                    .SingleOrDefault();
            }
        }

        internal Context GetContext(PaymentRequest request)
        {
            if (contextsLocked) return null;

            var name = normalize(request.ContextId);

            lock (contexts)
            {
                if (contexts.ContainsKey(name))
                    return contexts[name];

                var allowed = new[]
                {
                    PaymentRequest.Type.Initialize,
                    PaymentRequest.Type.ListDevices,
                };

                if (allowed.Contains(request.RequestType))
                {
                    var context = new Context(this, messagesHandler.OnError);
                    contexts[name] = context;

                    return context;
                }
            }

            return null;
        }

        internal async Task<PaymentResponse.Type> KillContext(PaymentRequest request)
        {
            var context = GetContext(request);

            if (context != null &&
                context.GetStatus().Code != ContextStatus.Closed)
            {
                var result = await context.Close();
                context.Dispose();
                return result;
            }

            return PaymentResponse.Type.ContextClosed;
        }

        private string normalize(string name)
        {
            return string.IsNullOrEmpty(name) ? "<default>" : name;
        }
    }
}
