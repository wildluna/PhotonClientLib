using ExitGames.Client.Photon;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhotonLib
{
    public class RequestPackage
    {
        public ManualResetEvent Awaitor { get; protected set; } = new ManualResetEvent(false);
        public Task<OperationResponse> Task { get; set; }
        public OperationRequest Request { get; set; }
        public OperationResponse Response { get; set; }

        public void OnOprationResponse(OperationResponse response)
        {
            foreach(var parameter in response.Parameters)
            {
                if(!Request.Parameters.ContainsKey(parameter.Key))
                    throw new Exception("回應與請求不相等.");
                if(!Request.Parameters[parameter.Key].Equals(parameter.Value))
                    throw new Exception("回應與請求不相等.");
            }
                
            Response = response;
            Awaitor.Set();
        }
    }
}
