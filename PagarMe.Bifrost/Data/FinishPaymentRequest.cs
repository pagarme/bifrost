namespace PagarMe.Bifrost.Data
{
    public class FinishPaymentRequest
    {
        public string EmvData { get; set; }
        public int ResponseCode { get; set; }
    }
}
