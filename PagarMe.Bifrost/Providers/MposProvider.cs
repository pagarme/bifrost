using System;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using PagarMe.Bifrost.Devices;
using PagarMe.Mpos.Entities;
using mpos = PagarMe.Mpos.Mpos;

namespace PagarMe.Bifrost.Providers
{
    public class MposProvider : IProvider
    {
        private mpos mpos;
        private IDevice device;
        public async Task<MposResultCode> Open(InitializationOptions options)
        {
            device = options.Device;
            var stream = device.Open(options.BaudRate);

            mpos = new mpos(stream, options.EncryptionKey);

            return await mpos.Initialize();
        }

        public async Task<MposResultCode> SynchronizeTables()
        {
            return await mpos.SynchronizeTables();
        }

        public async Task<MposResultCode> DisplayMessage(string message)
        {
            return await mpos.Display(message);
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            var paymentMethod = request.PaymentMethod;

            if (paymentMethod == 0)
                paymentMethod = PaymentMethod.Credit;

            var response = await mpos.ProcessPayment(request.Amount, paymentMethod);

            return new ProcessPaymentResponse
            {
                Result = response
            };
        }

        public async Task<MposResultCode> FinishPayment(FinishPaymentRequest request)
        {
            return await mpos.FinishTransaction(
	            request.ResponseCode.ToString("0000"),
	            request.EmvData
	        );
        }

        public async Task<MposResultCode> Close()
        {
            return await mpos.Close();
        }

        public String GetMessage(MposResultCode code)
		{
			return mpos.GetMessage(code);
		}

        public void Dispose()
        {
            mpos?.Dispose();
            mpos = null;

            device?.Close();
        }
    }
}