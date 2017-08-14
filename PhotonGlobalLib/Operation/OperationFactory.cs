using ExitGames.Client.Photon;

namespace PhotonGlobalLib.Operation
{
    public class OperationFactory
    {
        public static OperationResponse GetResponse(OperationResponse operationResponse)
        {
            switch((OperaionCodeEnum)operationResponse.OperationCode)
            {
                case OperaionCodeEnum.Echo:
                    {
                        return new EchoResponse(operationResponse);
                    }
                    break;
                case OperaionCodeEnum.Delay:
                    {
                        return new DelayResponse(operationResponse);
                    }
                default:
                    {
                        return new OperationResponse()
                        {
                            DebugMessage = string.Format("Error OperationCode {0}", operationResponse.OperationCode),
                        };
                    }
            }
        }
    }
}
