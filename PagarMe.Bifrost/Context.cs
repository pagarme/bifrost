using System;
using System.Threading;
using System.Threading.Tasks;
using PagarMe.Bifrost.Data;
using PagarMe.Generic;
using PagarMe.Mpos.Entities;
using mpos = PagarMe.Mpos.Mpos;

namespace PagarMe.Bifrost
{
    internal class Context : IDisposable
    {
        private readonly ServiceHandler service;
        private readonly SemaphoreSlim locker;
        private SerialDevice device;

        private ContextStatus status;

        private mpos mpos;

        private Action<String> sendErrorMessage { get; }
        public void OnError(MposResultCode errorCode)
        {
            var code = (Int32)errorCode;
            var message = mpos.GetMessage(errorCode);
            sendErrorMessage($"Error: [{code}] {message}");
        }

        internal String DeviceId => device?.Id;

        public Context(ServiceHandler service, Action<String> sendErrorMessage)
        {
            this.service = service;
            this.sendErrorMessage = sendErrorMessage;
            locker = new SemaphoreSlim(1, 1);
            status = ContextStatus.Uninitialized;
        }

        public Task<SerialDevice[]> ListDevices()
        {
            var devices = service.DeviceManager.FindAvailableDevices();
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
                device = service.DeviceManager.GetById(request.DeviceId);
                var stream = device.Open(request.BaudRate);

                mpos = new mpos(stream, request.EncryptionKey);

                var initTask = mpos.Initialize();

                var completedInit = await initTask
                    .SetTimeout(request.TimeoutMilliseconds);

                if (!completedInit)
                {
                    return null;
                }

                await mpos.SynchronizeTables();
                if (initTask.Result != MposResultCode.Ok)
                {
                    OnError(initTask.Result);
                    return PaymentResponse.Type.Error;
                }

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
            var devices = service.DeviceManager.FindAvailableDevices();

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
                var result = await mpos.Display(message);

                if (result == MposResultCode.Ok)
                    return PaymentResponse.Type.MessageDisplayed;

                OnError(result);
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

                var paymentMethod = request.PaymentMethod;

                if (paymentMethod == 0)
                    paymentMethod = PaymentMethod.Credit;

                var response = await mpos.ProcessPayment(request.Amount, paymentMethod);

                return new ProcessPaymentResponse
                {
                    Result = response
                };
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

                var result = await mpos.FinishTransaction(
                    request.ResponseCode.ToString("0000"),
                    request.EmvData
                );

                if (result == MposResultCode.Ok)
                    return PaymentResponse.Type.Finished;

                OnError(result);
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
            var result = await mpos.Close();

            if (result != MposResultCode.Ok)
            {
                return PaymentResponse.Type.Error;
            }

            status = ContextStatus.Closed;

            return PaymentResponse.Type.ContextClosed;
        }

        public void Dispose()
        {
            mpos?.Dispose();
            mpos = null;

            device?.Close();
        }
    }
}
