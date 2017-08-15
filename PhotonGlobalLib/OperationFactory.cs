using PhotonGlobalLib.Operation;
using System;
using System.Collections.Generic;

namespace PhotonGlobalLib
{
    public class OperationFactory
    {
        public static CustomOperationResponse GetResponse(byte operationCode, Dictionary<byte, object> parameters, short returnCode, string debugMessage)
        {
            switch((OperaionCodeEnum)operationCode)
            {
                case OperaionCodeEnum.Echo:
                    {
                        return new EchoResponse(operationCode, parameters);
                    }
                case OperaionCodeEnum.Delay:
                    {
                        return new DelayResponse(operationCode, parameters);
                    }
                default:
                    {
                        throw new Exception(string.Format("Unknow operationCode: {0}", operationCode));
                    }
            }
        }

        public static CustomOperationRequest GetRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            switch ((OperaionCodeEnum)operationCode)
            {
                case OperaionCodeEnum.Echo:
                    {
                        return new EchoRequest(operationCode, parameters);
                    }
                case OperaionCodeEnum.Delay:
                    {
                        return new DelayRequest(operationCode, parameters);
                    }
                default:
                    {
                        throw new Exception(string.Format("Unknow operationCode: {0}", operationCode));
                    }
            }
        }
    }
}
