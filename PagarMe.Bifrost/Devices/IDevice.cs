using System;
using System.IO;

namespace PagarMe.Bifrost.Devices
{
    public interface IDevice
    {
        String Id { get; }

        String Name { get; }

        String Manufacturer { get; }

        DeviceKind Kind { get; }

        Stream Open(Int32 baudRate);

        void Close();

        void Dispose();
    }
}
