using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonGlobalLib.Operation
{
    public class EchoRequest : CustomOperationRequest
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

        public EchoRequest(byte operationCode, Dictionary<byte, object> parameters)
            : base(operationCode, parameters)
        { }
    }

    public class EchoResponse : CustomOperationResponse
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
        public EchoResponse(byte operationCode, Dictionary<byte, object> parameters)
            : base(operationCode, parameters)
        { }
    }
}
