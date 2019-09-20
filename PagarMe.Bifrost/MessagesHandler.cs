using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PagarMe.Bifrost.Data;
using PagarMe.Bifrost.Util;
using PagarMe.Mpos.Entities;
using WebSocketSharp;
using WebSocketSharp.Server;
using log = PagarMe.Generic.Log;
using mposFunc = System.Func<
    PagarMe.Bifrost.Context,
    PagarMe.Bifrost.Data.PaymentRequest,
    PagarMe.Bifrost.Data.PaymentResponse,
    System.Threading.Tasks.Task
>;

namespace PagarMe.Bifrost
{
    internal class MessagesHandler : WebSocketBehavior
    {
        private readonly ServiceHandler serviceHandler;

        public MessagesHandler(ServiceHandler serviceHandler)
        {
            this.serviceHandler = serviceHandler;
            IgnoreExtensions = true;
        }


        protected override void OnOpen()
        {
            log.Me.Info("Socket Opened");
        }

        protected override void OnClose(CloseEventArgs args)
        {
            log.Me.Info($"Socket Closed: [{args.Code}] {args.Reason}");
        }


        protected override async void OnMessage(MessageEventArgs args)
        {
            var request = new PaymentRequest();

            await log.TryLogOnExceptionAsync(() =>
            {
                request = JsonConvert.DeserializeObject<PaymentRequest>(args.Data, SnakeCase.Settings);
                return handleMessage(request);
            },
            (e) => processError(e, request));
        }

        private void processError(Exception exception, PaymentRequest request)
        {
            log.TryLogOnException(() =>
            {
                var response = request.GenerateResponse();

                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = $"An error has occured with the [{request.RequestType}] request. See the log and contact the support.";

                send(response);
            });
        }

        private async Task handleMessage(PaymentRequest request)
        {
            var context = serviceHandler.GetContext(request);
            var response = request.GenerateResponse();

            if (context == null)
            {
                var message = "Error on creating context";
                log.Me.Info(message);
                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = message;
            }
            else
            {
                log.Me.Info(request.RequestType);
                await handleRequest(context, request, response);
            }

            send(response);
        }

        private async Task handleRequest(Context context, PaymentRequest request, PaymentResponse response)
        {
            var keepProcessing = verifyContextSequence(context, request, response);

            if (!keepProcessing)
                return;

            var type = request.RequestType;

            var actions = new Dictionary<PaymentRequest.Type, mposFunc>
            {
                {PaymentRequest.Type.ListDevices, getDeviceList},
                {PaymentRequest.Type.Initialize, initialize},
                {PaymentRequest.Type.Process, process},
                {PaymentRequest.Type.Finish, finish},
                {PaymentRequest.Type.DisplayMessage, displayMessage},
                {PaymentRequest.Type.Status, setStatus},
                {PaymentRequest.Type.CloseContext, close},
            };

            if (actions.ContainsKey(type))
            {
                await actions[type](context, request, response);
            }
            else
            {
                response.ResponseType = PaymentResponse.Type.UnknownCommand;
            }
        }

        private Boolean verifyContextSequence(Context context, PaymentRequest request, PaymentResponse response)
        {
            lock (context)
            {
                var canCallAnytime = new[]
                {
                    PaymentRequest.Type.ListDevices,
                    PaymentRequest.Type.DisplayMessage,
                    PaymentRequest.Type.Status,
                    PaymentRequest.Type.UnknownCommand,
                    PaymentRequest.Type.CloseContext,
                };

                if (!canCallAnytime.Contains(request.RequestType))
                {
                    var allowed = context.CurrentOperation.GetNextAllowed();

                    if (!allowed.Contains(request.RequestType))
                    {
                        var allowedText = String.Join(", ", allowed);
                        response.Error = $"Just follow operations allowed: {allowedText}";
                        response.ResponseType = PaymentResponse.Type.Error;
                        return false;
                    }

                    context.CurrentOperation = request.RequestType;
                }

                return true;
            }
        }

        private async Task getDeviceList(Context context, PaymentRequest request, PaymentResponse response)
        {
            response.DeviceList = await context.ListDevices();
            response.ResponseType = PaymentResponse.Type.DevicesListed;
        }

        private async Task initialize(Context context, PaymentRequest request, PaymentResponse response)
        {
            var initialize = request.Initialize;

            var deviceContextName = serviceHandler.GetDeviceContextName(initialize.DeviceId);

            if (deviceContextName != null && deviceContextName != request.ContextId)
            {
                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = $"Device already in use by context {deviceContextName}";
                return;
            }

            var initialized = await context.Initialize(initialize);

            if (initialized.HasValue)
            {
                response.ResponseType = initialized.Value;
            }
            else
            {
                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = "Could not initialize device. Perhaps it's not the right port.";
            }
        }

        private Task setStatus(Context context, PaymentRequest request, PaymentResponse response)
        {
            return Task.Run(() =>
            {
                var result = context.GetStatus();
                response.Status = result;
                response.ResponseType = PaymentResponse.Type.Status;
            });
        }

        private async Task process(Context context, PaymentRequest request, PaymentResponse response)
        {
            var process = await context.ProcessPayment(request.Process);
            response.Process = process.Result;

            if (process.Result.ResultCode == MposResultCode.Ok)
            {
                response.ResponseType = PaymentResponse.Type.Processed;
            }
            else
            {
                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = $"Transaction {process.Result.ResultCode}";
            }
        }

        private async Task finish(Context context, PaymentRequest request, PaymentResponse response)
        {
            response.ResponseType =
                await context.FinishPayment(request.Finish);
        }

        private async Task displayMessage(Context context, PaymentRequest request, PaymentResponse response)
        {
            response.ResponseType =
                await context.DisplayMessage(request.DisplayMessage);
        }

        private async Task close(Context context, PaymentRequest request, PaymentResponse response)
        {
            response.ResponseType =
                await serviceHandler.KillContext(request);
        }


        protected override void OnError(ErrorEventArgs e)
        {
            log.Me.Error(e.Message);
            log.Me.Error(e.Exception);
            OnError(e.Message);
        }

        internal void OnError(String message)
        {
            var response = new PaymentResponse
            {
                ResponseType = PaymentResponse.Type.Error,
                Error = message
            };

            log.Me.Error(message);

            send(response);
        }

        private void send(PaymentResponse response)
        {
            log.Me.Info(response.ResponseType);

            if (State == WebSocketState.Open)
            {
                Send(JsonConvert.SerializeObject(response, SnakeCase.Settings));
            }
            else
            {
                log.Me.Warn($"Could not send response of {response.ResponseType}, websocket connection not opened.");
            }
        }
    }
}
