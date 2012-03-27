#region imports

using System;
using System.Collections.Generic;
using System.Threading;
using DicomSharp.Data;
using DicomSharp.Net;
using DicomSharp.ServiceClassProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace DicomCS.Tests
{
    [TestClass]
    public class DicomServerTest
    {

        [TestMethod]
        public void CEcho()
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