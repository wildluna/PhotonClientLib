using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestServer
{
    public class TestApplication : ApplicationBase
    {
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new TestPeer(initRequest);
        }

        protected override void Setup()
        {
        }

        protected override void TearDown()
        {
        }
    }
}
