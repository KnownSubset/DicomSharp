#region Copyright

// 
// This library is based on Dicom# see http://sourceforge.net/projects/dicom-cs/
// Copyright (C) 2002 Fang Yang. All rights reserved.
// That library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2012 Nathan Dauber. All rights reserved.
// 
// This file is part of dicomSharp, see https://github.com/KnownSubset/DicomSharp
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.                                 
// 
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
// Nathan Dauber (nathan.dauber@gmail.com)
//
// 8/29/08: Added ServiceClassUser by Maarten JB van Ettinger based on TestSCU. Added a couple of SCU functions.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using DicomSharp.Data;
using DicomSharp.Dictionary;
using Microsoft.Practices.Unity;
using log4net;
using Timer = System.Timers.Timer;

namespace DicomSharp.Net
{
    /// <summary>
    /// Summary description for ServiceClassUser.
    /// </summary>
    public class ServiceClassUser : IServiceClassUser
    {
        private const string TRANSFER_SYNTAX_UID = UIDs.ImplicitVRLittleEndian;
        private const int ASSOCIATE_TIME_OUT = 0;
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (ServiceClassUser));
        private static readonly String[] DEF_TS = new[] {TRANSFER_SYNTAX_UID};
        private readonly AAssociateRQ aAssociateRequest;
        private readonly AssociationFactory associationFactory;
        private readonly Dictionary<string, IList<Dataset>> cacheManager = new Dictionary<string, IList<Dataset>>(50);
        private readonly IUnityContainer container;
        private readonly DcmObjectFactory dcmObjectFactory;
        private readonly DcmParserFactory dcmParserFactory;
        private IActiveAssociation activeAssociation;
        private String hostName = "localhost";
        private int pcidStart = 1;
        private int port = 104;

        public ServiceClassUser(IUnityContainer unityContainer, String name, String title, String hostName, int port)
        {
            container = unityContainer;
            associationFactory = container.Resolve<AssociationFactory>();
            dcmObjectFactory = container.Resolve<DcmObjectFactory>();
            dcmParserFactory = container.Resolve<DcmParserFactory>();
            aAssociateRequest = associationFactory.NewAAssociateRQ();
            SetUpForOperation(name, title, hostName, port);
            var timer = new Timer(5*60*1000);
            timer.Elapsed += ClearCache;
            timer.Start();
        }

        #region IServiceClassUser Members

        public string Name
        {
            get { return aAssociateRequest.Name; }
            set { aAssociateRequest.Name = value; }
        }

        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }

        public string Title
        {
            get { return aAssociateRequest.ApplicationEntityTitle; }
            set { aAssociateRequest.ApplicationEntityTitle = value; }
        }

        public uint Port
        {
            get { return (uint) port; }
            set { port = (int) value; }
        }

        /// <summary>
        /// This method configures the Service Class User to operate against a specific Service Class Provider
        /// <param name="name">The name of the SCU that will be sent to the SCP</param>
        /// <param name="title">The AE Title of the SCP</param>
        /// <param name="newHostName">The hostname of the SCP</param>
        /// <param name="newPort">The newPort of the SCP</param>
        /// </summary>
        public void SetUpForOperation(string name, string title, string newHostName, int newPort)
        {
            aAssociateRequest.ApplicationEntityTitle = title;
            aAssociateRequest.Name = name;
            aAssociateRequest.AsyncOpsWindow = associationFactory.NewAsyncOpsWindow(0, 1);
            aAssociateRequest.MaxPduLength = 16352;
            hostName = newHostName;
            port = newPort;
            pcidStart = 1;
        }

        /// <summary>
        /// Send C-ECHO, <see cref="SetUpForOperation"/> to specify the endpoint for the echo
        /// </summary>
        public bool CEcho()
        {
            int pcid = pcidStart;
            pcidStart += 2;
            bool success = false;
            try
            {
                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, UIDs.Verification, DEF_TS));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    if (active.Association.GetAcceptedTransferSyntaxUID(pcid) == null)
                    {
                        LOGGER.Error("Verification SOP class is not supported");
                    } else
                    {
                        IDicomCommand cEchoDicomCommand = dcmObjectFactory.NewCommand().InitCEchoRQ(0);
                        IDimse dimse = associationFactory.NewDimse(pcid, cEchoDicomCommand);
                        LOGGER.Info(String.Format("Echoing as {0} @ {1} {2}:{3}", aAssociateRequest.Name, aAssociateRequest.ApplicationEntityTitle,
                                                  hostName, port));
                        active.Invoke(dimse);
                        success = true;
                    }
                    active.Release(true);
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }
            return success;
        }

        public bool Cancel()
        {
            int pcid = pcidStart;
            pcidStart += 2;
            bool success = false;
            try
            {
                activeAssociation = activeAssociation ?? OpenAssociation();
                IDicomCommand cCancelRequest = dcmObjectFactory.NewCommand().InitCCancelRQ(activeAssociation.Association.CurrentMessageId());
                IDimse dimse = associationFactory.NewDimse(pcid, cCancelRequest);
                IActiveAssociation active = associationFactory.NewActiveAssociation(activeAssociation.Association, null);
                active.Start();
                active.Invoke(dimse);
                success = true;
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }
            return success;
        }

        #endregion

        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUID">A series instance UID</param>
        /// </summary>
        public IList<Dataset> CFindSeries(string seriesInstanceUID)
        {
            if (cacheManager.ContainsKey(seriesInstanceUID))
            {
                return cacheManager[seriesInstanceUID];
            }
            IList<Dataset> datasets = CFindSeries(new List<string> {seriesInstanceUID});
            if (datasets.Count > 0)
            {
                cacheManager.Add(seriesInstanceUID, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUIDs">The series' instance UIDs</param>
        /// </summary>
        public IList<Dataset> CFindSeries(IEnumerable<string> seriesInstanceUIDs)
        {
            var dataset = new Dataset();
            const string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            dataset.SetFileMetaInfo(GenerateFileMetaInfo(sopClassUID));
            dataset.PutCS(Tags.QueryRetrieveLevel, "SERIES");
            dataset.PutCS(Tags.Modality);
            dataset.PutUI(Tags.StudyInstanceUID, seriesInstanceUIDs.ToArray());
            dataset.PutUI(Tags.SeriesInstanceUID);
            dataset.PutIS(Tags.SeriesNumber);
            dataset.PutDA(Tags.SeriesDate);
            dataset.PutTM(Tags.SeriesTime);
            dataset.PutLO(Tags.SeriesDescription);
            return seriesInstanceUIDs.Any() ? RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUID) : new List<Dataset>();
        }

        /// <summary>
        /// Find all the studies for a specific patient
        /// <param name="patientId">The id of the patient</param>
        /// <param name="patientName">The name of the patient</param>
        /// </summary>
        public IList<Dataset> CFindStudy(string patientId, string patientName)
        {
            string queryKey = aAssociateRequest.ApplicationEntityTitle + port + hostName + patientId + patientName;
            if (cacheManager.ContainsKey(queryKey))
            {
                return cacheManager[queryKey];
            }
            const string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var dataset = new Dataset();
            dataset.SetFileMetaInfo(GenerateFileMetaInfo(sopClassUID));
            dataset.PutDA(Tags.StudyDate);
            dataset.PutTM(Tags.StudyTime);
            dataset.PutSH(Tags.AccessionNumber);
            dataset.PutCS(Tags.QueryRetrieveLevel, "STUDY");
            dataset.PutCS(Tags.ModalitiesInStudy);
            dataset.PutLO(Tags.InstitutionName);
            dataset.PutPN(Tags.PerformingPhysicianName);
            dataset.PutPN(Tags.ReferringPhysicianName);
            dataset.PutLO(Tags.StudyDescription);
            dataset.PutPN(Tags.PatientName, patientName);
            dataset.PutLO(Tags.PatientID, patientId);
            dataset.PutDA(Tags.PatientBirthDate);
            dataset.PutCS(Tags.PatientSex);
            dataset.PutAS(Tags.PatientAge);
            dataset.PutUI(Tags.StudyInstanceUID);
            dataset.PutSH(Tags.StudyID);
            List<Dataset> datasets = RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUID);
            if (datasets.Any())
            {
                cacheManager.Add(queryKey, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Find a the study for the study Instance UIDs
        /// <param name="studyInstanceUID">The studu instance UIDs</param>
        /// </summary>
        public IList<Dataset> CFindStudy(string studyInstanceUID)
        {
            if (cacheManager.ContainsKey(studyInstanceUID))
            {
                return cacheManager[studyInstanceUID];
            }
            IList<Dataset> datasets = CFindStudies(new List<string> {studyInstanceUID});
            if (datasets.Count > 0)
            {
                cacheManager.Add(studyInstanceUID, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Find all the studies for the studies Instance UIDs
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// </summary>
        public IList<Dataset> CFindStudies(IEnumerable<string> studyInstanceUIDs)
        {
            const string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var dataset = new Dataset();
            dataset.SetFileMetaInfo(GenerateFileMetaInfo(sopClassUID));
            dataset.PutDA(Tags.StudyDate);
            dataset.PutTM(Tags.StudyTime);
            dataset.PutSH(Tags.AccessionNumber);
            dataset.PutCS(Tags.QueryRetrieveLevel, "STUDY");
            dataset.PutCS(Tags.ModalitiesInStudy);
            dataset.PutLO(Tags.InstitutionName);
            dataset.PutPN(Tags.ReferringPhysicianName);
            dataset.PutLO(Tags.StudyDescription);
            dataset.PutPN(Tags.PatientName);
            dataset.PutLO(Tags.PatientID);
            dataset.PutDA(Tags.PatientBirthDate);
            dataset.PutCS(Tags.PatientSex);
            dataset.PutAS(Tags.PatientAge);
            dataset.PutUI(Tags.StudyInstanceUID, studyInstanceUIDs.ToArray());
            dataset.PutSH(Tags.StudyID);
            return studyInstanceUIDs.Any() ? RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUID) : new List<Dataset>();
        }

        /// <summary>
        /// Send C-FIND for instance
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// <param name="seriesInstanceUIDs">The series' instance UIDs</param>
        /// </summary>
        public IList<Dataset> CFindInstance(IEnumerable<string> studyInstanceUIDs, IEnumerable<string> seriesInstanceUIDs)
        {
            const string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var datasets = new List<Dataset>();
            List<string> seriesNotCached = RetrieveItemsFromTheCache(seriesInstanceUIDs, datasets);
            List<string> studiesNotCached = RetrieveItemsFromTheCache(studyInstanceUIDs, datasets);
            var dataset = new Dataset();
            dataset.SetFileMetaInfo(GenerateFileMetaInfo(sopClassUID));
            dataset.PutUI(Tags.SOPInstanceUID);
            dataset.PutCS(Tags.QueryRetrieveLevel, "IMAGE");
            dataset.PutUI(Tags.SeriesInstanceUID, seriesNotCached.ToArray());
            dataset.PutUI(Tags.StudyInstanceUID, studiesNotCached.ToArray());
            datasets.AddRange(RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUID));
            return datasets;
        }

        /// <summary>
        /// Send C-Move for studies and series to be stored in the specified newAETDestination
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// <param name="seriesInstanceUIDs">The series' instance UIDs</param>
        /// <param name="newAETDestination">The SCP that will store the files</param>
        /// </summary>
        public IList<Dataset> CMove(IEnumerable<string> studyInstanceUIDs, IEnumerable<string> seriesInstanceUIDs,
                                    string newAETDestination)
        {
            int pcid = pcidStart;
            pcidStart += 2;

            var foundDatasets = new List<Dataset>();
            var dataSetsToMove = new List<Dataset>();
            List<string> studies = RetrieveItemsFromTheCache(studyInstanceUIDs, dataSetsToMove);
            List<string> series = RetrieveItemsFromTheCache(seriesInstanceUIDs, dataSetsToMove);
            dataSetsToMove.AddRange(CFindSeries(series));
            dataSetsToMove.AddRange(CFindStudies(studies));
            try
            {
                const string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelMOVE;
                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, sopClassUID, DEF_TS));
                IActiveAssociation activeAssociation = OpenAssociation();
                if (activeAssociation != null)
                {
                    IAssociation association = activeAssociation.Association;
                    if (association.GetAcceptedPresContext(sopClassUID, TRANSFER_SYNTAX_UID) == null)
                    {
                        LOGGER.Error("SOP class UID not supported");
                    } else
                    {
                        foreach (Dataset dataset in dataSetsToMove)
                        {
                            DicomCommand dicomCommand = dcmObjectFactory.NewCommand();
                            IDicomCommand cMoveRequest = dicomCommand.InitCMoveRQ(association.NextMsgID(), sopClassUID, 1,
                                                                                  newAETDestination);
                            IDimse dimseRequest = associationFactory.NewDimse(pcid, cMoveRequest, dataset);
                            LOGGER.Info(String.Format("CMove from {0} @ {1} {2}:{3} to {4}", aAssociateRequest.Name,
                                                      aAssociateRequest.ApplicationEntityTitle, hostName, port, newAETDestination));
                            FutureDimseResponse dimseResponse = activeAssociation.Invoke(dimseRequest);
                            while (!dimseResponse.IsReady())
                            {
                                Thread.Sleep(0);
                            }
                            foundDatasets.AddRange(dimseResponse.ListPending().Select(dimse => dimse.Dataset));
                        }
                        activeAssociation.Release(true);
                    }
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }

            return foundDatasets;
        }

        /// <summary>
        /// Send C-GET
        /// <param name="studyInstanceUID">A study instance UID</param>
        /// <param name="seriesInstanceUID">A series instance UID</param>
        /// <param name="sopInstanceUID">A sop instance instance UID</param>
        /// </summary>
        public IList<Dataset> CGet(string studyInstanceUID, string seriesInstanceUID, string sopInstanceUID)
        {
            if ((studyInstanceUID == null) && (seriesInstanceUID == null) && (sopInstanceUID == null))
            {
                return null;
            }
            int pcid = pcidStart;
            pcidStart += 2;
            var datasets = new List<Dataset>();
            try
            {
                string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelGET;
                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, sopClassUID, DEF_TS));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    var dataset = new Dataset();
                    dataset.SetFileMetaInfo(GenerateFileMetaInfo(sopClassUID));
                    dataset.PutUI(Tags.StudyInstanceUID, studyInstanceUID);
                    dataset.PutUI(Tags.SeriesInstanceUID, seriesInstanceUID);
                    dataset.PutUI(Tags.SOPInstanceUID, sopInstanceUID);
                    IAssociation association = active.Association;
                    if ((association.GetAcceptedPresContext(sopClassUID, TRANSFER_SYNTAX_UID)) == null)
                    {
                        LOGGER.Error("SOP class UID not supported");
                        return null;
                    }
                    DicomCommand cGetDicomCommand = dcmObjectFactory.NewCommand().InitCGetRQ(association.NextMsgID(), sopClassUID,
                                                                                        (int) DicomCommandMessage.HIGH);
                    IDimse dimseRequest = associationFactory.NewDimse(pcid, cGetDicomCommand, dataset);
                    FutureDimseResponse dimseResponse = active.Invoke(dimseRequest);
                    active.Release(true);
                    while (!dimseResponse.IsReady())
                    {
                        Thread.Sleep(0);
                    }
                    datasets.AddRange(dimseResponse.ListPending().Select(dimse => dimse.Dataset));
                    aAssociateRequest.RemovePresentationContext(pcid);
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }
            return datasets;
        }

        /// <summary>
        /// Send C-STORE
        /// </summary>
        /// <param name="fileName"></param>
        public bool CStore(String fileName)
        {
            int pcid = pcidStart;
            pcidStart += 2;

            Stream ins = null;
            DcmParser dcmParser = null;
            Dataset dataset = null;

            try
            {
                // Load DICOM file
                try
                {
                    ins = new BufferedStream(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                    dcmParser = dcmParserFactory.NewDcmParser(ins);
                    FileFormat format = dcmParser.DetectFileFormat();
                    if (format != null)
                    {
                        dataset = dcmObjectFactory.NewDataset();
                        dcmParser.DcmHandler = dataset.DcmHandler;
                        dcmParser.ParseDcmFile(format, Tags.PixelData);
                        LOGGER.Debug("Reading done");
                    } else
                    {
                        LOGGER.Error("Unknown format!");
                    }
                } catch (IOException e)
                {
                    LOGGER.Error(e);
                }

                //
                // Prepare association
                //
                string classUID = dataset.GetString(Tags.SOPClassUID);
                string tsUID = dataset.GetString(Tags.TransferSyntaxUID);

                if ((tsUID == null || tsUID.Equals("")) && (dataset.GetFileMetaInfo() != null))
                {
                    tsUID = dataset.GetFileMetaInfo().GetString(Tags.TransferSyntaxUID);
                }

                if (tsUID == null || tsUID.Equals(""))
                {
                    tsUID = UIDs.ImplicitVRLittleEndian;
                }

                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, classUID, new[] {tsUID}));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    bool bResponse = false;

                    FutureDimseResponse frsp = SendDataset(active, dcmParser, dataset);
                    if (frsp != null)
                    {
                        active.WaitOnRSP();
                        bResponse = true;
                    }
                    active.Release(true);
                    return bResponse;
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
                if (ins != null)
                {
                    try
                    {
                        ins.Close();
                    } catch (IOException) {}
                }
            }

            return false;
        }

        /// <summary>
        /// Send C-STORE
        /// </summary>
        /// <param name="ds"></param>
        public bool CStore(Dataset ds)
        {
            int pcid = pcidStart;
            pcidStart += 2;

            try
            {
                //
                // Prepare association
                //
                String classUID = ds.GetString(Tags.SOPClassUID);
                String tsUID = ds.GetString(Tags.TransferSyntaxUID);

                if ((tsUID == null || tsUID.Equals("")) && (ds.GetFileMetaInfo() != null))
                {
                    tsUID = ds.GetFileMetaInfo().GetString(Tags.TransferSyntaxUID);
                }

                if (tsUID == null || tsUID.Equals(""))
                {
                    tsUID = UIDs.ImplicitVRLittleEndian;
                }

                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, classUID, new[] {tsUID}));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    bool bResponse = false;
                    FutureDimseResponse frsp = SendDataset(active, null, ds);
                    if (frsp != null)
                    {
                        active.WaitOnRSP();
                        bResponse = true;
                    }
                    active.Release(true);
                    return bResponse;
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }

            return false;
        }

        private void ClearCache(object sender, ElapsedEventArgs e)
        {
            LOGGER.Info("Clearing Cache");
            cacheManager.Clear();
        }

        private List<string> RetrieveItemsFromTheCache(IEnumerable<string> uids, List<Dataset> datasets)
        {
            var uidsNotCached = new List<string>();
            foreach (string uid in uids)
            {
                if (cacheManager.ContainsKey(uid))
                {
                    datasets.AddRange(cacheManager[uid]);
                } else
                {
                    uidsNotCached.Add(uid);
                }
            }
            return uidsNotCached;
        }

        private IActiveAssociation OpenAssociation()
        {
            IAssociation association = associationFactory.NewRequestor(hostName, port);
            IPdu assocAC = association.Connect(aAssociateRequest, ASSOCIATE_TIME_OUT);
            if (assocAC is AAssociateRJ)
            {
                var aAssociateRj = ((AAssociateRJ) assocAC);
                throw new DcmServiceException(aAssociateRj.Reason(), aAssociateRj.ReasonAsString());
            }
            activeAssociation = associationFactory.NewActiveAssociation(association, null);
            activeAssociation.Timeout = ASSOCIATE_TIME_OUT;
            activeAssociation.Start();
            return activeAssociation;
        }

        private List<Dataset> RetrieveDatasetsFromServiceClassProvider(Dataset dataset, string sopClassUID)
        {
            int pcid = pcidStart;
            pcidStart += 2;
            var datasets = new List<Dataset>();
            try
            {
                aAssociateRequest.AddPresContext(associationFactory.NewPresContext(pcid, sopClassUID, DEF_TS));
                IActiveAssociation activeAssociation = OpenAssociation();
                if (activeAssociation != null)
                {
                    datasets = ExecuteCFindDicomCommand(activeAssociation, dataset, pcid);
                }
            } finally
            {
                aAssociateRequest.RemovePresentationContext(pcid);
            }
            return datasets;
        }

        private List<Dataset> ExecuteCFindDicomCommand(IActiveAssociation active, Dataset dataset, int pcid)
        {
            IAssociation association = active.Association;
            if (association.GetAcceptedPresContext(UIDs.StudyRootQueryRetrieveInformationModelFIND, TRANSFER_SYNTAX_UID) == null)
            {
                LOGGER.Error("SOP class UID not supported");
                return null;
            }
            IDicomCommand cFindDicomCommand = dcmObjectFactory.NewCommand().InitCFindRQ(association.NextMsgID(),
                                                                                  UIDs.StudyRootQueryRetrieveInformationModelFIND,
                                                                                  (int) DicomCommandMessage.HIGH);
            IDimse dimseRequest = associationFactory.NewDimse(pcid, cFindDicomCommand, dataset);
            LOGGER.Info(String.Format("CFind as {0} @ {1} {2}:{3}", aAssociateRequest.Name, aAssociateRequest.ApplicationEntityTitle, hostName, port));
            FutureDimseResponse dimseResponse = active.Invoke(dimseRequest);
            active.Release(true);
            while (!dimseResponse.IsReady())
            {
                Thread.Sleep(0);
            }
            return dimseResponse.ListPending().Select(dimse => dimse.Dataset).ToList();
        }

        private FutureDimseResponse SendDataset(IActiveAssociation activeAssociation, DcmParser parser, Dataset dataset)
        {
            String sopInstUID = dataset.GetString(Tags.SOPInstanceUID);
            if (sopInstUID == null)
            {
                LOGGER.Error("SOP instance UID is null");
                return null;
            }
            String sopClassUID = dataset.GetString(Tags.SOPClassUID);
            if (sopClassUID == null)
            {
                LOGGER.Error("SOP class UID is null");
                return null;
            }
            PresContext pc = null;
            IAssociation association = activeAssociation.Association;

            if (parser != null)
            {
                if (parser.DcmDecodeParam.encapsulated)
                {
                    String tsuid = dataset.GetFileMetaInfo().TransferSyntaxUID;
                    if ((pc = association.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
                    {
                        LOGGER.Error("SOP class UID not supported");
                        return null;
                    }
                } else if (IsSopClassUIDNotSupported(association, sopClassUID, out pc))
                {
                    LOGGER.Error("SOP class UID not supported");
                    return null;
                }

                DicomCommand cStoreRequest = dcmObjectFactory.NewCommand().InitCStoreRQ(association.NextMsgID(), sopClassUID,
                                                                                        sopInstUID, (int) DicomCommandMessage.HIGH);
                return
                    activeAssociation.Invoke(associationFactory.NewDimse(pc.pcid(), cStoreRequest,
                                                                         new FileDataSource(parser, dataset, new byte[2048])));
            }
            if ((dataset.GetFileMetaInfo() != null) && (dataset.GetFileMetaInfo().TransferSyntaxUID != null))
            {
                String tsuid = dataset.GetFileMetaInfo().TransferSyntaxUID;
                if ((pc = association.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
                {
                    LOGGER.Error("SOP class UID not supported");
                    return null;
                }
            } else if (IsSopClassUIDNotSupported(association, sopClassUID, out pc))
            {
                LOGGER.Error("SOP class UID not supported");
                return null;
            }

            DicomCommand cStoreRq = dcmObjectFactory.NewCommand().InitCStoreRQ(association.NextMsgID(), sopClassUID, sopInstUID,
                                                                               (int) DicomCommandMessage.HIGH);
            IDimse dimse = associationFactory.NewDimse(pc.pcid(), cStoreRq, dataset);
            return activeAssociation.Invoke(dimse);
        }

        private static bool IsSopClassUIDNotSupported(IAssociation association, string sopClassUID, out PresContext pc)
        {
            return (pc = association.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null;
        }

        private FileMetaInfo GenerateFileMetaInfo(string sopClassUID)
        {
            var fileMetaInfo = new FileMetaInfo();
            fileMetaInfo.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
            fileMetaInfo.PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
            fileMetaInfo.PutUI(Tags.TransferSyntaxUID, TRANSFER_SYNTAX_UID);
            fileMetaInfo.PutSH(Tags.ImplementationVersionName, "dicomSharp-SCU");
            return fileMetaInfo;
        }

        private static void copy(Stream ins, Stream outs, int len, bool swap, byte[] buffer)
        {
            if (swap && (len & 1) != 0)
            {
                throw new DcmParseException("Illegal length of OW Pixel Data: " + len);
            }
            if (buffer == null)
            {
                if (swap)
                {
                    int tmp;
                    for (int i = 0; i < len; ++i, ++i)
                    {
                        tmp = ins.ReadByte();
                        outs.WriteByte((Byte) ins.ReadByte());
                        outs.WriteByte((Byte) tmp);
                    }
                } else
                {
                    for (int i = 0; i < len; ++i)
                    {
                        outs.WriteByte((Byte) ins.ReadByte());
                    }
                }
            } else
            {
                byte tmp;
                int c, remain = len;
                while (remain > 0)
                {
                    c = ins.Read(buffer, 0, Math.Min(buffer.Length, remain));
                    if (swap)
                    {
                        if ((c & 1) != 0)
                        {
                            buffer[c++] = (byte) ins.ReadByte();
                        }
                        for (int i = 0; i < c; ++i, ++i)
                        {
                            tmp = buffer[i];
                            buffer[i] = buffer[i + 1];
                            buffer[i + 1] = tmp;
                        }
                    }
                    outs.Write(buffer, 0, c);
                    remain -= c;
                }
            }
        }

        #region Nested type: FileDataSource

        /// <summary>
        /// File Data source
        /// </summary>
        public sealed class FileDataSource : IDataSource
        {
            private readonly byte[] buffer;
            private readonly Dataset ds;
            private readonly DcmParser parser;

            public FileDataSource(DcmParser parser, Dataset ds, byte[] buffer)
            {
                this.parser = parser;
                this.ds = ds;
                this.buffer = buffer;
            }

            public void WriteTo(Stream outs, String transferSyntaxUniqueId)
            {
                DcmEncodeParam netParam = DcmDecodeParam.ValueOf(transferSyntaxUniqueId);
                ds.WriteDataset(outs, netParam);
                if (parser.ReadTag == Tags.PixelData)
                {
                    DcmDecodeParam fileParam = parser.DcmDecodeParam;
                    ds.WriteHeader(outs, netParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
                    if (netParam.encapsulated)
                    {
                        parser.ParseHeader();
                        while (parser.ReadTag == Tags.Item)
                        {
                            ds.WriteHeader(outs, netParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
                            copy(parser.InputStream, outs, parser.ReadLength, false, buffer);
                        }
                        if (parser.ReadTag != Tags.SeqDelimitationItem)
                        {
                            throw new DcmParseException("Unexpected Tag:" + Tags.ToHexString(parser.ReadTag));
                        }
                        if (parser.ReadLength != 0)
                        {
                            throw new DcmParseException("(fffe,e0dd), Length:" + parser.ReadLength);
                        }
                        ds.WriteHeader(outs, netParam, Tags.SeqDelimitationItem, VRs.NONE, 0);
                    } else
                    {
                        bool swap = fileParam.byteOrder != netParam.byteOrder && parser.ReadVR == VRs.OW;
                        copy(parser.InputStream, outs, parser.ReadLength, swap, buffer);
                    }
                    ds.Clear();
                    parser.ParseDataset(fileParam, 0);
                    ds.WriteDataset(outs, netParam);
                }
            }
        }

        #endregion
    }
}