using System;
using PagarMe.Bifrost.Devices;

namespace PagarMe.Bifrost.Providers
{
    internal class InitializationOptions
    {
        public SerialDevice Device { get; set; }
        public String EncryptionKey { get; set; }
        public Int32 BaudRate { get; set; }
    }
}