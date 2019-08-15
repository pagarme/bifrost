using System.Collections.Generic;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.v1;

namespace PagarMe.Bifrost.Commands
{
    public class ProcessPaymentRequest
    {
        public int Amount { get; set; }

        public IEnumerable<EmvApplication> Applications { get; set; }

        public PaymentMethod MagstripePaymentMethod { get; set; }
    }
}