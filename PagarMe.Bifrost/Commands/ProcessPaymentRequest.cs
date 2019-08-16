using PagarMe.Mpos.Entities;

namespace PagarMe.Bifrost.Commands
{
    public class ProcessPaymentRequest
    {
        public int Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }
}
