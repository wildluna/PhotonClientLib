using PhotonGlobalLib.Operation;
using PhotonLib;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestClientLib
{
    public class PhotonUtil
    {
        public static readonly PhotonUtil Instance = new PhotonUtil();

        private static Dictionary<string, PhotonClient> clients = new Dictionary<string, PhotonClient>();
        private static ReaderWriterLockSlim rwls = new ReaderWriterLockSlim();

        public PhotonClient GetClient(string applicationName)
        {
            try
            {
                rwls.EnterReadLock();

                if (clients.ContainsKey(applicationName))
                    return clients[applicationName];

                return null;
            }
            finally
            {
                rwls.ExitReadLock();
            }
        }
        public void CreateAndConnect(string applicationName, string serverAddress)
        {
            try
            {
                PhotonClient client = null;

                rwls.EnterWriteLock();
                if (clients.ContainsKey(applicationName))
                {
                    client = clients[applicationName];
                    if (client.PeerState == ExitGames.Client.Photon.PeerStateValue.Disconnected)
                        client.Connect();
                }
                else
                {
                    client = new PhotonClient(applicationName, serverAddress);
                    clients.Add(applicationName, client);
                    client.Connect();
                }
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }
        public void RemoveClient(string applicationName)
        {
            try
            {
                PhotonClient client = null;
                rwls.EnterWriteLock();
                if (clients.ContainsKey(applicationName))
                {
                    client = clients[applicationName];
                    client.Close();
                    client.Dispose();
                    clients.Remove(applicationName);
                }
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        public async Task<EchoResponse> RequestEchoAsync(string message)
        {
            PhotonClient client = GetClient("Test");

            EchoRequest request = new EchoRequest(message);
            var response = await client.RequestAsync(request);
            return OperationFactory.GetResponse(response) as EchoResponse;
        }
        public async Task<DelayResponse> RequestDelayAsync(int milliSeconds, string message)
        {
            PhotonClient client = GetClient("Test");
            DelayRequest request = new DelayRequest(milliSeconds, message);
            var response = await client.RequestAsync(request);
            return OperationFactory.GetResponse(response) as DelayResponse;
        }
    }
}
