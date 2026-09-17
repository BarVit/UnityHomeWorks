using System;
using System.Threading;

namespace FlappyBridge
{
    public class BridgeRequest
    {
        public BridgeRequest(Uri url)
        {
            Url = url;
            Completed = new ManualResetEventSlim(false);
        }

        public Uri Url { get; private set; }

        public ManualResetEventSlim Completed { get; private set; }

        public string Response { get; set; }
    }
}
