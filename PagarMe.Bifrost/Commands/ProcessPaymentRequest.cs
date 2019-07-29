using PagarMe.Mpos;
using System.Collections.Generic;
using PagarMe.Mpos.Entities;

namespace PagarMe.Bifrost.Commands
{
    public class ProcessPaymentRequest
    {
        public int Amount { get; set; }

        public IEnumerable<EmvApplication> Applications { get; set; }

        public PaymentMethod MagstripePaymentMethod { get; set; }
    }
}