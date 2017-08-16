using ExitGames.Client.Photon;
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
    /// Photon客戶端
    /// </summary>
    public class PhotonClient : IPhotonPeerListener, IDisposable
    {
        /// <summary>
        /// 連線子狀態
        /// </summary>
        public PeerStateValue PeerState { get => peer.PeerState; }
        /// <summary>
        /// 取得伺服器地址
        /// </summary>
        public string ServerAddress { get; private set; }
        /// <summary>
        /// 取得伺服器ApplicationName
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// Peer服務的取消符號
        /// </summary>
        private CancellationTokenSource serviceCancel;
        /// <summary>
        /// 請求包隊列的取消符號
        /// </summary>
        private CancellationTokenSource requestCancel;

        /// <summary>
        /// 請求包隊列的等待器
        /// </summary>
        private ManualResetEvent requestAwaitor = new ManualResetEvent(true);
        /// <summary>
        /// 新增請求包的等待器
        /// </summary>
        private ManualResetEvent addPackAwaitor = new ManualResetEvent(true);

        /// <summary>
        /// 請求包隊列
        /// </summary>
        private BlockingCollection<RequestPackage> requestQueue;
        /// <summary>
        /// 目前的請求包
        /// </summary>
        private RequestPackage currentRequest = null;

        /// <summary>
        /// 連線等待器
        /// </summary>
        private ManualResetEvent connectAwaitor = new ManualResetEvent(false);
        /// <summary>
        /// Photon連線子
        /// </summary>
        private PhotonPeer peer;

        /// <summary>
        /// 建立新的Photon客戶端
        /// </summary>
        /// <param name="applicationName">ApplicationName</param>
        /// <param name="serverAddress">伺服器地址:連接阜</param>
        /// <param name="protocalType">連線方式</param>
        public PhotonClient(string applicationName, string serverAddress, ConnectionProtocol protocalType = ConnectionProtocol.Tcp)
        {
            peer = new PhotonPeer(this, protocalType);
            ApplicationName = applicationName;
            ServerAddress = serverAddress;
        }

        /// <summary>
        /// 連線
        /// </summary>
        public void Connect()
        {
            StartService();
            peer.Connect(ServerAddress, ApplicationName);
            connectAwaitor.WaitOne();
        }
        /// <summary>
        /// 關閉連線
        /// </summary>
        public void Close()
        {
            StopService();
            peer.Disconnect();
        }
        /// <summary>
        /// 異步新增請求
        /// </summary>
        /// <param name="operationCode">請求代碼</param>
        /// <param name="parameters">參數</param>
        /// <returns>伺服器回應</returns>
        public Task<OperationResponse> RequestAsync(byte operationCode, Dictionary<byte, object> parameters)
        {
            RequestPackage packet = new RequestPackage()
            {
                Request = new OperationRequest
                {
                    OperationCode = operationCode,
                    Parameters = parameters
                }
            };

            // 將請求加入請求包隊列
            // 如果失敗就丟出例外
            if (PushRequest(packet))
                return packet.WaitResponseAsync();
            else
                throw new Exception("PushRequest Failed.");
        }

        /// <summary>
        /// 統一加入請求
        /// 並通知請求執行者有封包加入
        /// </summary>
        /// <param name="requestPackage">請求包</param>
        /// <returns>是否成功加入</returns>
        private bool PushRequest(RequestPackage requestPackage)
        {
            bool add = requestQueue.TryAdd(requestPackage);
            if (add) addPackAwaitor.Set();
            return add;
        }
        /// <summary>
        /// 偵錯回應
        /// </summary>
        /// <param name="level">偵錯等級</param>
        /// <param name="message">訊息</param>
        public virtual void DebugReturn(DebugLevel level, string message)
        {
        }
        /// <summary>
        /// 作業回應處理
        /// </summary>
        /// <param name="operationResponse">作業回應</param>
        public virtual void OnOperationResponse(OperationResponse operationResponse)
        {
            // 檢查內容, 防指timeout掉的訊息後續回應
            if(operationResponse.OperationCode != currentRequest.Request.OperationCode)
            {
                // 回應跟請求的OpCode不相符 忽略訊息
                return;
            }

            // 驗證正確, 是目前request等待中的response
            // 設置結果
            currentRequest.OnOperationResponse(operationResponse);
            // 通知請求下一個
            requestAwaitor.Set(); 
        }
        /// <summary>
        /// 連線狀態改變
        /// </summary>
        /// <param name="statusCode">連線狀態</param>
        public virtual void OnStatusChanged(StatusCode statusCode)
        {
            // 連線
            if(statusCode == StatusCode.Connect)
            {
                OnConnect();
            }
            // 斷線
            if(statusCode == StatusCode.Disconnect)
            {
                OnDisconnect();
            }
        }
        /// <summary>
        /// 事件處理
        /// </summary>
        /// <param name="eventData">事件資料</param>
        public virtual void OnEvent(EventData eventData)
        {
        }
        /// <summary>
        /// 訊息處理
        /// </summary>
        /// <param name="messages">訊息</param>
        public virtual void OnMessage(object messages)
        {
        }
        /// <summary>
        /// 開始Peer服務
        /// </summary>
        private void StartService()
        {
            requestQueue = new BlockingCollection<RequestPackage>();

            serviceCancel = new CancellationTokenSource();
            Task.Factory.StartNew(() => 
            {
                try
                {
                    while (!serviceCancel.IsCancellationRequested)
                    {
                        peer.Service();
                    }
                }
                catch (Exception)
                { }

            }, serviceCancel.Token);

            requestCancel = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (!requestCancel.IsCancellationRequested)
                    {
                        addPackAwaitor.WaitOne();   // 等後封包加入
                        if (requestQueue.TryTake(out currentRequest, -1, requestCancel.Token))
                        {
                            requestAwaitor.Reset();
                            peer.OpCustom(currentRequest.Request, true, 0, false);
                            if (!requestAwaitor.WaitOne(3000))
                            {
                                currentRequest = null;

                                // 等候3秒回應, 沒回應視同失敗, 2種方式則一
                                // 第一種統一response 傳一個timeout訊息
                                
                                currentRequest.OnOperationResponse(new OperationResponse
                                {
                                    ReturnCode = -1,
                                    DebugMessage = "Response timeout."
                                });

                                // 第二種方式拋例外的方式, 等待方會拋出TimeoutException
                                currentRequest.Timeout();
                            }
                        }
                    }
                }
                catch (Exception)
                { }
            });
        }
        /// <summary>
        /// 停止Peer服務
        /// </summary>
        private void StopService()
        {
            serviceCancel.Cancel();
            serviceCancel.Dispose();
            serviceCancel = null;
            requestCancel.Cancel();
            requestCancel.Dispose();
            requestCancel = null;
            DisposeRequestQueue();
            currentRequest = null;
        }
        /// <summary>
        /// 釋放請求包隊列
        /// </summary>
        private void DisposeRequestQueue()
        {
            while (requestQueue.Any())
            {
                requestQueue.TryTake(out RequestPackage request);
                request.OnOperationResponse(null);
            }
        }
        /// <summary>
        /// 連線處理
        /// </summary>
        protected void OnConnect()
        {
            connectAwaitor.Set();
        }
        /// <summary>
        /// 斷線處理
        /// </summary>
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
                    serviceCancel?.Dispose();
                    requestCancel?.Dispose();
                    DisposeRequestQueue();
                    requestAwaitor.Dispose();
                    requestQueue.Dispose();
                    connectAwaitor.Dispose();
                }

                requestAwaitor = null;
                requestQueue = null;
                connectAwaitor = null;
                currentRequest = null;
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
