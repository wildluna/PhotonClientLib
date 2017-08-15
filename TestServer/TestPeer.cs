using Photon.SocketServer;
using PhotonGlobalLib;
using PhotonGlobalLib.Operation;
using PhotonHostRuntimeInterfaces;
using System.Collections.Generic;
using System.Threading;

namespace TestServer
{
    class TestPeer : ClientPeer
    {
        public TestPeer(InitRequest initRequest) : base(initRequest)
        {
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch(operationRequest.OperationCode)
            {
                // Echo
                case 0:
                    {
                        var request = OperationFactory.GetRequest(operationRequest.OperationCode, operationRequest.Parameters) as EchoRequest;
                        EchoResponse response = new EchoResponse(request.Message);
                        SendOperationResponse(new OperationResponse(response.OperationCode, response.Parameters), sendParameters);
                    }
                    break;
                // Delay Echo
                case 1:
                    {
                        var request = OperationFactory.GetRequest(operationRequest.OperationCode, operationRequest.Parameters) as DelayRequest;
                        int sleepTime = request.DelayMilliSeconds;
                        Thread.Sleep(sleepTime);

                        DelayResponse response = new DelayResponse(request.DelayMilliSeconds, request.Message);

                        SendOperationResponse(new OperationResponse(response.OperationCode, response.Parameters), sendParameters);
                    }
                    break;
            }
        }
    }
}
