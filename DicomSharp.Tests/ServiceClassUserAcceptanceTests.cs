#region imports

using System;
using System.Collections.Generic;
using DicomSharp.Data;
using DicomSharp.Net;
using DicomSharp.ServiceClassProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace DicomSharp.Tests {
    [TestClass]
    public class ServiceClassUserAcceptanceTests {
        private Microsoft.Practices.Unity.IUnityContainer container;
        private readonly DicomServer dicomServer = new DicomServer();
        [TestInitialize]
        public void SetUp() {
            container = new Microsoft.Practices.Unity.UnityContainer();
            dicomServer.ArchiveDir = "c:\\temp";
            dicomServer.Port = 104;
            dicomServer.Policy = new AcceptorPolicyService().AcceptorPolicy;
        //    dicomServer.Start();
        }
        [TestCleanup]
        public void TearDown() {
        //    dicomServer.Stop();
        }

        [TestMethod]
        public void CEcho() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);

            var connected = scu.CEcho();

            Assert.IsTrue(connected);
        }


        [TestMethod]
        [ExpectedException(typeof (System.Net.Sockets.SocketException))]
        public void CEchoFailsBecauseOfBadPort() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 3333);

            scu.CEcho();

            Assert.Fail("Exception should be thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(DcmServiceException))]
        public void CEchoFailsBecauseOfBadAE_Title(){
            var scu = new ServiceClassUser(container, "NEWTON", "Non-Existant", "localhost", 11112);

            scu.CEcho();

            Assert.Fail("Exception should be thrown");
        }

        [TestMethod]
        public void CFindSeries() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            
            IList<DataSet> cFindSeries = scu.CFindSeriesForStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1");
            
            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CFindStudy() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            
            IList<DataSet> cFindSeries = scu.CFindStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1", "");
            
            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CFindStudyByStudyInstanceUID() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            
            IList<DataSet> cFindStudies = scu.CFindStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1");
            
            Assert.IsNotNull(cFindStudies);
        }
        
        [TestMethod]
        public void CFindInstance() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            
            IList<DataSet> cFindInstances = scu.CFindInstance(new List<string>(), new List<string> {"1.2.840.114165.8192.3.1.10.4005887757780752754.2"});
            
            Assert.IsNotNull(cFindInstances);
        }

        [TestMethod]
        public void CMove() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            var studyInstanceUIDs = new List<string> { "1.2.246.352.71.2.988760059.292338.20110216080258", "1.2.246.352.71.2.988760059.289629.20110214075640" };

            IList<DataSet> movedDatasets = scu.CMove(new List<string> { "1.2.840.114358.49.13.20100707164123.767653680600" }, studyInstanceUIDs, "FULLSTORAGE_SCP");
            
            Assert.IsNotNull(movedDatasets);
        }

        [TestMethod]
        public void AttemptCMoveForMultipleStudiesAndMultipleSeries() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            var studyInstanceUIDs = new List<string> { "1.3.6.1.4.1.22213.2.6289.2.1", "1.2.124.113532.10.45.57.43.20060404.125800.2907095", "2.16.840.1.113662.2.12.0.3173.1260902821.1270" };
            var seriesInstanceUIDs = new List<string> {};

            IList<DataSet> movedDatasets = scu.CMove(studyInstanceUIDs, seriesInstanceUIDs, "FULLSTORAGE_SCP");
            
            Assert.IsNotNull(movedDatasets);
        }

        [TestMethod]
        public void CancelOccursQuickly() {
            var scu = new ServiceClassUser(container, "FullAccess2", "DCM4CHEE", "localhost", 11112);
            var studyInstanceUIDs = new List<string> {
                                                         "1.3.12.2.1107.5.1.4.1031.30000009082111200307800000018",
                                                         "1.3.46.670589.11.5019.5.0.5844.2009082111084362050",
                                                         "1.2.124.113532.10.33.162.50.20090812.81504.24624529"
                                                     };
            var seriesInstanceUIDs = new List<string> {
                                                          "1.3.12.2.1107.5.1.4.1031.30000009082111213876500007458",
                                                          "1.3.46.670589.11.5019.5.0.4632.2009082111211140720",
                                                          "1.3.12.2.1107.5.1.4.1031.30000009082111223996800005593"
                                                      };

            var moveAction = new Action(() => scu.CMove(studyInstanceUIDs, seriesInstanceUIDs, "FULLSTORAGE_SCP"));
            moveAction.BeginInvoke(delegate { scu.Cancel(); }, null);
        }    
        
        [TestMethod]
        public void AttemptCMoveForZeroStudiesAndSeries() {
            var scu = new ServiceClassUser(container, "NEWTON", "DCM4CHEE", "localhost", 11112);
            var studyInstanceUIDs = new List<string>();
            var seriesInstanceUIDs = new List<string>();
            
            IList<DataSet> movedDatasets = scu.CMove(studyInstanceUIDs, seriesInstanceUIDs, "FULLSTORAGE_SCP");
            
            Assert.AreEqual(0, movedDatasets.Count);
        }
        
       
    }
}