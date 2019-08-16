using System;
using System.Threading;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using PagarMe.Bifrost.Devices;
using PagarMe.Bifrost.Providers;
using PagarMe.Generic;
using PagarMe.Mpos.Entities;

namespace PagarMe.Bifrost
{
    internal class Context : IDisposable
    {
        private readonly MposBridge bridge;
        private readonly SemaphoreSlim locker;
        private MposProvider provider;
        private SerialDevice device;

        private ContextStatus status;

        private Action<String> sendErrorMessage { get; }
        public void onError(MposResultCode errorCode)
        {
	        var code = (Int32) errorCode;
	        var message = provider.GetMessage(errorCode);
	        sendErrorMessage($"Error: [{code}] {message}");
        }

		internal String DeviceId => device?.Id;

        public Context(MposBridge bridge, MposProvider provider, Action<String> sendErrorMessage)
        {
            this.bridge = bridge;
            this.provider = provider;
            this.sendErrorMessage = sendErrorMessage;
            locker = new SemaphoreSlim(1, 1);
            status = ContextStatus.Uninitialized;
        }

        public Task<SerialDevice[]> ListDevices()
        {
            var devices = bridge.DeviceManager.FindAvailableDevices();
            return Task.FromResult(devices);
        }

        internal PaymentRequest.Type CurrentOperation { get; set; }

        public async Task<PaymentResponse.Type?> Initialize(InitializeRequest request)
        {
            if (status == ContextStatus.Ready)
                return PaymentResponse.Type.AlreadyInitialized;

            await locker.WaitAsync();

            try
            {
	            var options = new InitializationOptions
	            {
		            Device = bridge.DeviceManager.GetById(request.DeviceId),
		            EncryptionKey = request.EncryptionKey,
		            BaudRate = request.BaudRate,
	            };

	            var initTask = provider.Open(options);

				var completedInit = await initTask
					.SetTimeout(request.TimeoutMilliseconds);

				if (!completedInit)
				{
					return null;
				}

				if (initTask.Result != MposResultCode.Ok)
				{
					onError(initTask.Result);
					return PaymentResponse.Type.Error;
				}

                await provider.SynchronizeTables();

                device = options.Device;
                status = ContextStatus.Ready;
            }
            finally
            {
                locker.Release(1);
            }

            return PaymentResponse.Type.Initialized;
        }

        public StatusResponse GetStatus()
        {
            var devices = bridge.DeviceManager.FindAvailableDevices();

            var response = new StatusResponse
            {
                Code = status,
                AvailableDevices = devices.Length
            };

            if (device != null)
            {
                response.ConnectedDeviceId = device.Id;
            }

            return response;
        }

        public async Task<PaymentResponse.Type> DisplayMessage(DisplayMessageRequest request)
        {
            await locker.WaitAsync();

            try
            {
	            var message = request?.Message ?? String.Empty;
				var result = await provider.DisplayMessage(message);

				if (result == MposResultCode.Ok)
					return PaymentResponse.Type.MessageDisplayed;

				onError(result);
				return PaymentResponse.Type.Error;
            }
            finally
            {
                locker.Release(1);
            }
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            await locker.WaitAsync();

            if (status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                status = ContextStatus.InUse;

                return await provider.ProcessPayment(request);
            }
            finally
            {
                status = ContextStatus.Ready;
                locker.Release(1);
            }
        }

        public async Task<PaymentResponse.Type> FinishPayment(FinishPaymentRequest request)
        {
            await locker.WaitAsync();

            if (status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                status = ContextStatus.InUse;

                var result = await provider.FinishPayment(request);

                if (result == MposResultCode.Ok)
	                return PaymentResponse.Type.Finished;

                onError(result);
                return PaymentResponse.Type.Error;
            }
            finally
            {
                status = ContextStatus.Ready;
                locker.Release(1);
            }
        }

        public async Task<PaymentResponse.Type> Close()
        {
            var result = await provider.Close();

            if (result != MposResultCode.Ok)
            {
	            return PaymentResponse.Type.Error;
            }

            status = ContextStatus.Closed;

            return PaymentResponse.Type.ContextClosed;
        }

        public void Dispose()
        {
            provider.Dispose();
            provider = null;
        }
    }
}