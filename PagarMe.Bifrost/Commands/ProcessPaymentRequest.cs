using PagarMe.Mpos;
using PagarMe.Mpos.Entities;
using System.Collections.Generic;

namespace PagarMe.Bifrost.Commands
{
    public class ProcessPaymentRequest
    {
        public int Amount { get; set; }

        public IEnumerable<EmvApplication> Applications { get; set; }

        public PaymentMethod MagstripePaymentMethod { get; set; }
    }
}