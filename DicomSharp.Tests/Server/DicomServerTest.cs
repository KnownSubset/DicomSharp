#region imports

using System.Threading;
using DicomSharp.ServiceClassProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace DicomSharp.Tests.Server
{
    [TestClass]
    public class DicomServerTest
    {

        [TestMethod]
        public void ServerStart()
        {
            var dicomServer = new DicomServer();
            dicomServer.ArchiveDir = "c:\\";
            dicomServer.Port = 105;
            dicomServer.Policy = new AcceptorPolicyService().AcceptorPolicy;
            
            dicomServer.Start();
            while(!dicomServer.IsStarted)
            {
                Thread.Sleep(0);
            }
            dicomServer.Stop();
        }
        
    }
}