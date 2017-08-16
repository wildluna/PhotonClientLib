using ExitGames.Client.Photon;
using System;
using System.Threading.Tasks;

namespace PhotonLib
{
    /// <summary>
    /// 請求包
    /// </summary>
    public class RequestPackage
    {
        /// <summary>
        /// 
        /// </summary>
        private TaskCompletionSource<OperationResponse> completeSource = new TaskCompletionSource<OperationResponse>();
        /// <summary>
        /// 作業請求
        /// </summary>
        public OperationRequest Request { get; set; }

        /// <summary>
        /// 作業回應處理
        /// </summary>
        /// <param name="response">作業回應</param>
        public void OnOperationResponse(OperationResponse response)
        {
            completeSource.TrySetResult(response);
        }
        /// <summary>
        /// 逾時
        /// </summary>
        public void Timeout()
        {
            completeSource.TrySetException(new TimeoutException("Timeout"));
        }
        /// <summary>
        /// 等待回應
        /// </summary>
        /// <returns>作業回應</returns>
        public Task<OperationResponse> WaitResponseAsync()
        {
            return completeSource.Task;
        }
    }
}
