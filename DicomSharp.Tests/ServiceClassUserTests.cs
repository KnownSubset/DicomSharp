#region imports

using System.Collections.Generic;
using DicomSharp.Data;
using DicomSharp.Dictionary;
using DicomSharp.Net;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

#endregion

namespace DicomSharp.Tests
{
    [TestClass]
    public class ServiceClassUserTests
    {
        private const string HOSTNAME = "localhost";
        private const int PORT = 11112;
        private AssociationFactory associationFactory;
        private DcmObjectFactory dcmObjectFactory;
        private MockRepository mockRepository;
        private readonly string[] tranferSyntax = new[] { UIDs.ImplicitVRLittleEndian };
        private IUnityContainer unityContainer;
        private AAssociateRQ _aAssociateRq;

        [TestInitialize]
        public void SetUp()
        {
            mockRepository = new MockRepository();
            unityContainer = new UnityContainer();
            associationFactory = mockRepository.DynamicMock<AssociationFactory>();
            unityContainer.RegisterInstance(associationFactory);
            dcmObjectFactory = mockRepository.DynamicMock<DcmObjectFactory>();
            unityContainer.RegisterInstance(dcmObjectFactory);
            unityContainer.RegisterInstance(mockRepository.DynamicMock<DcmParserFactory>());
            _aAssociateRq = CreateAAssociateRq();

        }

        private IActiveAssociation CreateActiveAssociation(IAssociation association)
        {
            var activeAssociation = mockRepository.DynamicMock<IActiveAssociation>();
            Expect.Call(associationFactory.NewActiveAssociation(association, null)).Return(activeAssociation);
            Expect.Call(activeAssociation.Association).Return(association).Repeat.AtLeastOnce();
            return activeAssociation;
        }

        private IAssociation CreateAssociation()
        {
            var association = mockRepository.StrictMock<IAssociation>();
            Expect.Call(associationFactory.NewRequestor(HOSTNAME, PORT)).Return(association);
            var aAssociateAc = mockRepository.DynamicMock<IAAssociateAC>();
            Expect.Call(association.Connect(_aAssociateRq, 0)).Return(aAssociateAc);
            return association;
        }

        private AAssociateRQ CreateAAssociateRq()
        {
            var aAssociateRq = mockRepository.DynamicMock<AAssociateRQ>();
            Expect.Call(associationFactory.NewAAssociateRQ()).Return(aAssociateRq);
            return aAssociateRq;
        }

        [TestCleanup]
        public void After()
        {
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CEcho()
        {
            var association = CreateAssociation();
            var activeAssociation = CreateActiveAssociation(association);
            Expect.Call(association.GetAcceptedTransferSyntaxUID(1)).Return("");
            var dicomCommand = mockRepository.DynamicMock<IDicomCommand>();
            Expect.Call(dcmObjectFactory.NewCommand()).Return(dicomCommand);
            Expect.Call(dicomCommand.InitCEchoRQ(0)).Return(dicomCommand);
            var dimse = mockRepository.DynamicMock<IDimse>();
            Expect.Call(associationFactory.NewDimse(1, dicomCommand)).Return(dimse);
            var presContext = mockRepository.DynamicMock<PresentationContext>(0x20, 2, 3, "", tranferSyntax);
            Expect.Call(associationFactory.NewPresContext(1, UIDs.Verification, tranferSyntax)).Return(presContext);
            //Expect.Call(() => _aAssociateRq.AddPresContext(presContext));
            Expect.Call(activeAssociation.Invoke(dimse)).Return(null);
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            var succeed = scu.CEcho();

            Assert.IsTrue(succeed);
        }

        [TestMethod]
        public void CEchoEndpointDoesNotSupportTheTransferSyntax()
        {
            var association = CreateAssociation();
            CreateActiveAssociation(association);
            var presContext = mockRepository.DynamicMock<PresentationContext>(0x20, 2, 3, "", tranferSyntax);
            Expect.Call(association.GetAcceptedTransferSyntaxUID(1)).Return(null);
            Expect.Call(associationFactory.NewPresContext(1, UIDs.Verification, tranferSyntax)).Return(presContext);
            //Expect.Call(() => aAssociateRq.AddPresContext(presContext));
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            var succeed = scu.CEcho();

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void CFindSeries()
        {
            SetUpCFind();
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            IList<DataSet> cFindSeries = scu.CFindSeriesForStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1");

            Assert.IsNotNull(cFindSeries);
        }


        [TestMethod]
        public void CFindStudy()
        {
            SetUpCFind();
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            IList<DataSet> cFindSeries = scu.CFindStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1", "");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CFindStudyByStudyInstanceUID()
        {
            SetUpCFind();
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            IList<DataSet> cFindSeries = scu.CFindStudy("1.2.840.114165.8192.3.1.10.8047921150017449681.1");

            Assert.IsNotNull(cFindSeries);
        }


        [TestMethod]
        public void CFindInstance()
        {
            SetUpCFind();
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            IList<DataSet> cFindSeries = scu.CFindInstance(new List<string>(), new List<string> { "1.2.840.114165.8192.3.1.10.4005887757780752754.2" });

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CMoveForStudies()
        {
            SetUpCFind();
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);
            var studyInstanceUIDs = new List<string> { "1.2.840.114165.8192.3.1.10.8047921150017449681.1" };

            IList<DataSet> cFindSeries = scu.CMove(studyInstanceUIDs, new List<string>(), "FULLSTORAGE_SCP");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CMoveForSeries()
        {
            SetUpCFind();
            SetUpCMove(1);
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);
            var seriesInstanceUIDs = new List<string> { "1.2.840.114165.8192.3.1.10.4005887757780752754.2" };

            IList<DataSet> cFindSeries = scu.CMove(new List<string>(), seriesInstanceUIDs, "FULLSTORAGE_SCP");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void CMove()
        {
            SetUpCFind();
            SetUpCFind();
            SetUpCMove(1);
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);
            var studyInstanceUIDs = new List<string> { "1.2.840.114165.8192.3.1.10.8047921150017449681.1" };
            var seriesInstanceUIDs = new List<string> { "1.2.840.114165.8192.3.1.10.4005887757780752754.2" };

            IList<DataSet> cFindSeries = scu.CMove(studyInstanceUIDs, seriesInstanceUIDs, "FULLSTORAGE_SCP");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void WhenCMoveIsCalledWithNoStudiesOrSeriesInstanceUIDsPassedToItNoCallToTheSCP_IsMade()
        {
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            IList<DataSet> cFindSeries = scu.CMove(new List<string>(), new List<string>(), "FULLSTORAGE_SCP");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void AttemptCMoveForMultipleStudiesAndMultipleSeries()
        {
            SetUpCFind();
            SetUpCFind();
            SetUpCMove(1);
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);
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

            IList<DataSet> cFindSeries = scu.CMove(studyInstanceUIDs, seriesInstanceUIDs, "FULLSTORAGE_SCP");

            Assert.IsNotNull(cFindSeries);
        }

        [TestMethod]
        public void IssuingCancelOperationRequestToSCP()
        {
            var association = CreateAssociation();
            var activeAssociation = CreateActiveAssociation(association);
            var dicomCommand = mockRepository.DynamicMock<IDicomCommand>();
            Expect.Call(association.CurrentMessageId()).Return(1);
            Expect.Call(dcmObjectFactory.NewCommand()).Return(dicomCommand);
            var dimse = mockRepository.DynamicMock<IDimse>();
            var cancelAssociation = mockRepository.DynamicMock<IActiveAssociation>();
            Expect.Call(associationFactory.NewActiveAssociation(association, null)).Return(cancelAssociation);
            Expect.Call(associationFactory.NewDimse(1, dicomCommand)).Return(dimse);
            Expect.Call(dicomCommand.InitCCancelRQ(1)).Return(dicomCommand);
            Expect.Call(cancelAssociation.Start);
            Expect.Call(cancelAssociation.Invoke(dimse)).Return(null);
            mockRepository.ReplayAll();
            var scu = new ServiceClassUser(unityContainer, "NEWTON", "DCM4CHEE", HOSTNAME, PORT);

            var cancelSuccess = scu.Cancel();

            Assert.IsTrue(cancelSuccess);
        }

        private void SetUpCFind()
        {
            var association = CreateAssociation();
            var activeAssociation = CreateActiveAssociation(association);
            Expect.Call(association.GetAcceptedPresContext("1.2.840.10008.5.1.4.1.2.2.1", UIDs.ImplicitVRLittleEndian)).Return(new PresentationContext(0, 0, 0, "", tranferSyntax)).Repeat.AtLeastOnce();
            Expect.Call(association.NextMsgID()).Return(1);
            var dicomCommand = mockRepository.DynamicMock<IDicomCommand>();
            Expect.Call(dcmObjectFactory.NewCommand()).Return(dicomCommand);
            Expect.Call(dicomCommand.InitCFindRQ(1, UIDs.StudyRootQueryRetrieveInformationModelFIND, Priority.HIGH)).Return(dicomCommand);
            var dimse = mockRepository.DynamicMock<IDimse>();
            Expect.Call(associationFactory.NewDimse(1, dicomCommand, new DataSet())).IgnoreArguments().Return(dimse);
            var futureDimseResponse = mockRepository.DynamicMock<FutureDimseResponse>();
            Expect.Call(activeAssociation.Invoke(dimse)).Return(futureDimseResponse);
            Expect.Call(futureDimseResponse.IsReady()).Return(true);
            var dimses = new List<IDimse> { dimse };
            Expect.Call(dimse.DataSet).Return(new DataSet());
            Expect.Call(futureDimseResponse.ListPending()).Return(dimses);
        }

        private void SetUpCMove(int numberOfTimes)
        {
            var association = CreateAssociation();
            var dicomCommand = mockRepository.DynamicMock<IDicomCommand>();
            var dimse = mockRepository.DynamicMock<IDimse>();
            var futureDimseResponse = mockRepository.DynamicMock<FutureDimseResponse>();
            var cMoveActiveAssociation = SetUpAssociation(association);
            Expect.Call(association.GetAcceptedPresContext("1.2.840.10008.5.1.4.1.2.2.2", UIDs.ImplicitVRLittleEndian)).Return(new PresentationContext(0, 0, 0, "", tranferSyntax));
            Expect.Call(association.NextMsgID()).Return(1).Repeat.Times(numberOfTimes);
            Expect.Call(dicomCommand.InitCMoveRQ(1, UIDs.StudyRootQueryRetrieveInformationModelMOVE, Priority.HIGH, "FULLSTORAGE_SCP")).Return(dicomCommand).Repeat.Times(numberOfTimes);
            Expect.Call(dcmObjectFactory.NewCommand()).Return(dicomCommand).Repeat.Times(numberOfTimes);
            Expect.Call(associationFactory.NewDimse(1, dicomCommand, new DataSet())).IgnoreArguments().Return(dimse).Repeat.Times(numberOfTimes);
            Expect.Call(cMoveActiveAssociation.Invoke(dimse)).Return(futureDimseResponse).Repeat.Times(numberOfTimes);
            Expect.Call(futureDimseResponse.IsReady()).Return(true).Repeat.Times(numberOfTimes);
        }

        private IActiveAssociation SetUpAssociation(IAssociation association)
        {
            //Expect.Call(associationFactory.NewRequestor(HOSTNAME, PORT)).Return(association);
            var mockActiveAssociation = mockRepository.DynamicMock<IActiveAssociation>();
            Expect.Call(associationFactory.NewActiveAssociation(association, null)).Return(mockActiveAssociation);
            Expect.Call(mockActiveAssociation.Association).Return(association);
            return mockActiveAssociation;
        }
    }
}