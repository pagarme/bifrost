using System;
using PagarMe.Bifrost.Devices;
using PagarMe.Mpos.Entities;

namespace PagarMe.Bifrost.Commands
{
    public class PaymentResponse
    {
        public IDevice[] DeviceList { get; internal set; }
        public PaymentResult Process { get; internal set; }
        public StatusResponse Status { get; internal set; }

        public String ContextId { get; internal set; }
        public Type ResponseType { get; internal set; }

        public String Error { get; internal set; }

        public enum Type
        {
            UnknownCommand = 0,
            DevicesListed = 1,
            Initialized = 2,
            AlreadyInitialized = 3,
            Processed = 4,
            Finished = 5,
            MessageDisplayed = 6,
            Status = 7,
            ContextClosed = 8,
            Error = 9,
        }
    }
}