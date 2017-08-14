using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace PhotonGlobalLib.Operation
{
    public class DelayRequest : OperationRequest
    {
        /// <summary>
        /// 延遲的毫秒數
        /// </summary>
        public int DelayMilliSeconds { get => (int)Parameters[0]; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get => (string)Parameters[1]; }

        /// <summary>
        /// 建立伺服器延遲請求
        /// </summary>
        /// <param name="delayMilliSeconds">延遲的毫秒數</param>
        /// <param name="message">訊息</param>
        public DelayRequest(int delayMilliSeconds, string message)
        {
            OperationCode = (byte)OperaionCodeEnum.Delay;
            Parameters = new Dictionary<byte, object>()
            {
                {0, delayMilliSeconds },
                {1, message },
            };
        }

        public DelayRequest(OperationRequest operationRequest)
        {
            OperationCode = operationRequest.OperationCode;
            Parameters = operationRequest.Parameters;
        }
    }

    public class DelayResponse : OperationResponse
    {
        /// <summary>
        /// 延遲的毫秒數
        /// </summary>
        public int DelayMilliSeconds { get => (int)Parameters[0]; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get => (string)Parameters[1]; }
        /// <summary>
        /// 建立伺服器延遲回應
        /// </summary>
        /// <param name="delayMilliSeconds">延遲的毫秒數</param>
        /// <param name="message">訊息</param>
        public DelayResponse(int delayMilliSeconds, string message)
        {
            OperationCode = (byte)OperaionCodeEnum.Delay;
            Parameters = new Dictionary<byte, object>()
            {
                {0, delayMilliSeconds },
                {1, message },
            };
        }

        public DelayResponse(OperationResponse operationResponse)
        {
            OperationCode = operationResponse.OperationCode;
            ReturnCode = operationResponse.ReturnCode;
            DebugMessage = operationResponse.DebugMessage;
            Parameters = operationResponse.Parameters;
        }
    }
}
