using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonGlobalLib.Operation
{
    public class EchoRequest : OperationRequest
    {
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get => (string)Parameters[0]; }

        /// <summary>
        /// 建立伺服器延遲請求
        /// </summary>
        /// <param name="delayMilliSeconds">延遲的毫秒數</param>
        /// <param name="message">訊息</param>
        public EchoRequest(string message)
        {
            OperationCode = (byte)OperaionCodeEnum.Echo;
            Parameters = new Dictionary<byte, object>()
            {
                {0, message },
            };
        }

        public EchoRequest(OperationRequest operationRequest)
        {
            OperationCode = operationRequest.OperationCode;
            Parameters = operationRequest.Parameters;
        }
    }

    public class EchoResponse : OperationResponse
    {
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get => (string)Parameters[0]; }
        /// <summary>
        /// 建立伺服器延遲回應
        /// </summary>
        /// <param name="delayMilliSeconds">延遲的毫秒數</param>
        /// <param name="message">訊息</param>
        public EchoResponse(string message)
        {
            OperationCode = (byte)OperaionCodeEnum.Echo;
            Parameters = new Dictionary<byte, object>()
            {
                {0, message },
            };
        }

        public EchoResponse(OperationResponse operationResponse)
        {
            OperationCode = operationResponse.OperationCode;
            ReturnCode = operationResponse.ReturnCode;
            DebugMessage = operationResponse.DebugMessage;
            Parameters = operationResponse.Parameters;
        }
    }
}
