namespace PagarMe.Bifrost.Data
{
    public class StatusResponse
    {
        public ContextStatus Code { get; set; }

        public string ConnectedDeviceId { get; set; }

        public int AvailableDevices { get; set; }
    }
}
