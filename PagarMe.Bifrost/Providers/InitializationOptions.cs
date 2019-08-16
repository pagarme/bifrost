using System;
using PagarMe.Bifrost.Devices;

namespace PagarMe.Bifrost.Providers
{
    public class InitializationOptions
    {
        public IDevice Device { get; set; }
        public String EncryptionKey { get; set; }
        public Int32 BaudRate { get; set; }
    }
}