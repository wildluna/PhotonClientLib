using System.Collections.Generic;

namespace PhotonGlobalLib
{
    public class CustomOperationRequest
    {
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters { get; set; } = new Dictionary<byte, object>();

        public CustomOperationRequest()
        { }

        public CustomOperationRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            OperationCode = operationCode;
            Parameters = parameters;
        }
    }

    public class CustomOperationResponse
    {
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters { get; set; } = new Dictionary<byte, object>();
        public string DebugMessage { get; set; }
        public short ReturnCode { get; set; }

        public CustomOperationResponse()
        { }

        public CustomOperationResponse(byte operationCode, Dictionary<byte, object> parameters)
        {
            OperationCode = operationCode;
            Parameters = parameters;
        }
    }
}
