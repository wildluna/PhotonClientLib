﻿using ExitGames.Client.Photon;
using ExitGames.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotonLib
{
    /// <summary>
    /// 
    /// </summary>
    public class PhotonClient : IPhotonPeerListener, IDisposable
    {
        public TimeSpan ResponseTimeout = new TimeSpan(0, 0, 5);
        public PeerStateValue PeerState { get => peer.PeerState; }

        public string ServerAddress { get; private set; }
        public string ApplicationName { get; private set; }

        private Task serviceTask;
        private CancellationTokenSource serviceCancelToken;

        private Task requestTask;
        private CancellationTokenSource requestCancelToken;

        private ManualResetEvent requestAwaitor = new ManualResetEvent(true);
        private BlockingCollection<RequestPackage> requestQueue = new BlockingCollection<RequestPackage>();
        private ReaderWriterLockSlim requestLock = new ReaderWriterLockSlim();

        private RequestPackage currentRequest = null;

        private ManualResetEvent connectAwaitor = new ManualResetEvent(false);
        private PhotonPeer peer;

        public PhotonClient(string applicationName, string serverAddress)
        {
            peer = new PhotonPeer(this, ConnectionProtocol.Tcp);
            ApplicationName = applicationName;
            ServerAddress = serverAddress;
        }

        public void Connect()
        {
            StartService();
            peer.Connect(ServerAddress, ApplicationName);
            connectAwaitor.WaitOne();
        }

        public void Close()
        {
            StopService();
            peer.Disconnect();  
        }
        public Task<OperationResponse> RequestAsync(OperationRequest operationRequest)
        {

            var request = new RequestPackage();
            request.Request = operationRequest;
            request.Task = Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!request.Awaitor.WaitOne())
                    {
                        return new OperationResponse() { ReturnCode = -1, DebugMessage = "Response timeout." };
                    }
                    else
                    {
                        return request.Response;
                    }
                }
                catch (Exception e)
                {
                    return new OperationResponse() { ReturnCode = -1, DebugMessage = e.Message };
                }
                finally
                {
                    currentRequest.Awaitor.Dispose();
                    currentRequest = null;
                    requestAwaitor.Set();
                }
            });
            requestQueue.TryAdd(request);
            return request.Task;
        }

        public virtual void DebugReturn(DebugLevel level, string message)
        {
            Console.WriteLine("Debug return: Level: {0} Message: {1}", level, message);
        }

        public virtual void OnOperationResponse(OperationResponse operationResponse)
        {
                if(currentRequest != null)
                {
                    currentRequest.OnOprationResponse(operationResponse);
                }
        }

        public virtual void OnStatusChanged(StatusCode statusCode)
        {
            Console.WriteLine("Current status: {0}", statusCode);
            if(statusCode == StatusCode.Connect)
            {
                OnConnect();
            }
            if(statusCode == StatusCode.Disconnect)
            {
                OnDisconnect();
            }
        }

        public virtual void OnEvent(EventData eventData)
        {
        }

        public virtual void OnMessage(object messages)
        {
        }

        private void StartService()
        {
            serviceCancelToken = new CancellationTokenSource();
            serviceTask = Task.Factory.StartNew(() => 
            {
                while (!serviceCancelToken.Token.IsCancellationRequested)
                {
                    peer.Service();
                }
            }, serviceCancelToken.Token);

            requestCancelToken = new CancellationTokenSource();
            requestTask = Task.Factory.StartNew(() =>
            {
                while(!requestCancelToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (requestAwaitor.WaitOne())
                        {
                            if (requestQueue.TryTake(out currentRequest, Timeout.Infinite, requestCancelToken.Token))
                            {
                                requestAwaitor.Reset();
                                peer.OpCustom(currentRequest.Request, true, 0, false);
                            }
                        }
                    }
                    catch(OperationCanceledException e)
                    {

                    }
                };
            });
        }

        private void StopService()
        {
            serviceCancelToken.Cancel();
            try
            {
                serviceTask.Wait();
            }
            finally
            {
                serviceTask.Dispose();
                serviceCancelToken.Dispose();
            }

            requestCancelToken.Cancel();
            try
            {
                requestTask.Wait();
            }
            finally
            {
                requestTask.Dispose();
                requestCancelToken.Dispose();
            }
        }

        protected void OnConnect()
        {
            connectAwaitor.Set();
        }

        protected void OnDisconnect()
        {
            connectAwaitor.Reset();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                    
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~PhotonClient() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}