using System;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using PagarMe.Mpos.Entities;

namespace PagarMe.Bifrost.Providers
{
    public interface IProvider : IDisposable
    {
        Task<MposResultCode> Open(InitializationOptions options);
        Task<MposResultCode> SynchronizeTables();
        Task<MposResultCode> DisplayMessage(String message);
        Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request);
        Task<MposResultCode> FinishPayment(FinishPaymentRequest request);
        Task<MposResultCode> Close();
        String GetMessage(MposResultCode code);
    }
}