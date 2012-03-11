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
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace DicomSharp.Net
{
    /// <summary>
    /// Summary description for ServiceClassUser.
    /// </summary>
    public class ServiceClassUser : IServiceClassUser
    {
        private const string TransferSyntaxUniqueId = UIDs.ImplicitVRLittleEndian;
        private const int AssociateTimeOut = 0;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ServiceClassUser));
        private static readonly String[] DefinedTransferSyntaxes = new[] {TransferSyntaxUniqueId};
        private readonly AAssociateRQ _aAssociateRequest;
        private readonly AssociationFactory _associationFactory;
        private readonly Dictionary<string, IList<DataSet>> _cacheManager = new Dictionary<string, IList<DataSet>>(50);
        private readonly IUnityContainer _container;
        private readonly DcmObjectFactory _dcmObjectFactory;
        private readonly DcmParserFactory _dcmParserFactory;
        private IActiveAssociation _activeAssociation;
        private string _hostName = "localhost";
        private int _port = 104;
        private int _presentationContextIdStart = 1;

        public ServiceClassUser(IUnityContainer unityContainer, String name, String title, String hostName, int port)
        {
            _container = unityContainer;
            _associationFactory = _container.Resolve<AssociationFactory>();
            _dcmObjectFactory = _container.Resolve<DcmObjectFactory>();
            _dcmParserFactory = _container.Resolve<DcmParserFactory>();
            _aAssociateRequest = _associationFactory.NewAAssociateRQ();
            SetUpForOperation(name, title, hostName, port);
            var timer = new Timer(5*60*1000);
            timer.Elapsed += ClearCache;
            timer.Start();
        }

        #region IServiceClassUser Members

        public string Name
        {
            get { return _aAssociateRequest.Name; }
            set { _aAssociateRequest.Name = value; }
        }

        public string HostName
        {
            get { return _hostName; }
            set { _hostName = value; }
        }

        public string Title
        {
            get { return _aAssociateRequest.ApplicationEntityTitle; }
            set { _aAssociateRequest.ApplicationEntityTitle = value; }
        }

        public uint Port
        {
            get { return (uint) _port; }
            set { _port = (int) value; }
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
            _aAssociateRequest.ApplicationEntityTitle = title;
            _aAssociateRequest.Name = name;
            _aAssociateRequest.AsyncOpsWindow = _associationFactory.NewAsyncOpsWindow(0, 1);
            _aAssociateRequest.MaxPduLength = 16352;
            _hostName = newHostName;
            _port = newPort;
            _presentationContextIdStart = 1;
        }

        /// <summary>
        /// Send C-ECHO, <see cref="SetUpForOperation"/> to specify the endpoint for the echo
        /// </summary>
        public bool CEcho()
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;
            bool success = false;
            try
            {
                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, UIDs.Verification, DefinedTransferSyntaxes));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    if (active.Association.GetAcceptedTransferSyntaxUID(pcid) == null)
                    {
                        Logger.Error("Verification SOP class is not supported");
                    } else
                    {
                        IDicomCommand cEchoDicomCommand = _dcmObjectFactory.NewCommand().InitCEchoRQ(0);
                        IDimse dimse = _associationFactory.NewDimse(pcid, cEchoDicomCommand);
                        Console.Out.WriteLine("{0} {1}", Logger.Logger.Name, Logger.Logger.Repository);    
                        Logger.Info(String.Format("Echoing as {0} @ {1} {2}:{3}", _aAssociateRequest.Name, _aAssociateRequest.ApplicationEntityTitle, _hostName, _port));
                        active.Invoke(dimse);
                        success = true;
                    }
                    active.Release(true);
                }
            } finally
            {
                _aAssociateRequest.RemovePresentationContext(pcid);
            }
            return success;
        }

        public bool Cancel()
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;
            bool success = false;
            try
            {
                _activeAssociation = _activeAssociation ?? OpenAssociation();
                IDicomCommand cCancelRequest =
                    _dcmObjectFactory.NewCommand().InitCCancelRQ(_activeAssociation.Association.CurrentMessageId());
                IDimse dimse = _associationFactory.NewDimse(pcid, cCancelRequest);
                IActiveAssociation active = _associationFactory.NewActiveAssociation(_activeAssociation.Association, null);
                active.Start();
                active.Invoke(dimse);
                success = true;
            } finally
            {
                _aAssociateRequest.RemovePresentationContext(pcid);
            }
            return success;
        }

        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUniqueId">A series instance UniqueId</param>
        /// </summary>
        public IList<DataSet> CFindSeries(string seriesInstanceUniqueId)
        {
            if (_cacheManager.ContainsKey(seriesInstanceUniqueId))
            {
                return _cacheManager[seriesInstanceUniqueId];
            }
            IList<DataSet> datasets = CFindSeries(new List<string> {seriesInstanceUniqueId});
            if (datasets.Count > 0)
            {
                _cacheManager.Add(seriesInstanceUniqueId, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUniqueIds">The series' instance UniqueIds</param>
        /// </summary>
        public IList<DataSet> CFindSeries(IEnumerable<string> seriesInstanceUniqueIds)
        {
            var dataset = new DataSet();
            const string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            dataset.FileMetaInfo = GenerateFileMetaInfo(sopClassUniqueId);
            dataset.PutCS(Tags.QueryRetrieveLevel, "SERIES");
            dataset.PutCS(Tags.Modality);
            dataset.PutUI(Tags.StudyInstanceUniqueId, seriesInstanceUniqueIds.ToArray());
            dataset.PutUI(Tags.SeriesInstanceUniqueId);
            dataset.PutIS(Tags.SeriesNumber);
            dataset.PutDA(Tags.SeriesDate);
            dataset.PutTM(Tags.SeriesTime);
            dataset.PutLO(Tags.SeriesDescription);
            return seriesInstanceUniqueIds.Any()
                       ? RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUniqueId)
                       : new List<DataSet>();
        }

        /// <summary>
        /// Find all the studies for a specific patient
        /// <param name="patientId">The id of the patient</param>
        /// <param name="patientName">The name of the patient</param>
        /// </summary>
        public IList<DataSet> CFindStudy(string patientId, string patientName)
        {
            string queryKey = _aAssociateRequest.ApplicationEntityTitle + _port + _hostName + patientId + patientName;
            if (_cacheManager.ContainsKey(queryKey))
            {
                return _cacheManager[queryKey];
            }
            const string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var dataset = new DataSet();
            dataset.FileMetaInfo = GenerateFileMetaInfo(sopClassUniqueId);
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
            dataset.PutUI(Tags.StudyInstanceUniqueId);
            dataset.PutSH(Tags.StudyID);
            List<DataSet> datasets = RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUniqueId);
            if (datasets.Any())
            {
                _cacheManager.Add(queryKey, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Find a the study for the study Instance UniqueIds
        /// <param name="studyInstanceUniqueId">The studu instance UniqueIds</param>
        /// </summary>
        public IList<DataSet> CFindStudy(string studyInstanceUniqueId)
        {
            if (_cacheManager.ContainsKey(studyInstanceUniqueId))
            {
                return _cacheManager[studyInstanceUniqueId];
            }
            IList<DataSet> datasets = CFindStudies(new List<string> {studyInstanceUniqueId});
            if (datasets.Count > 0)
            {
                _cacheManager.Add(studyInstanceUniqueId, datasets);
            }
            return datasets;
        }

        /// <summary>
        /// Find all the studies for the studies Instance UniqueIds
        /// <param name="studyInstanceUniqueIds">The studies' instance UniqueIds</param>
        /// </summary>
        public IList<DataSet> CFindStudies(IEnumerable<string> studyInstanceUniqueIds)
        {
            const string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var dataset = new DataSet();
            dataset.FileMetaInfo = GenerateFileMetaInfo(sopClassUniqueId);
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
            dataset.PutUI(Tags.StudyInstanceUniqueId, studyInstanceUniqueIds.ToArray());
            dataset.PutSH(Tags.StudyID);
            return studyInstanceUniqueIds.Any()
                       ? RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUniqueId)
                       : new List<DataSet>();
        }

        /// <summary>
        /// Send C-FIND for instance
        /// <param name="studyInstanceUniqueIds">The studies' instance UniqueIds</param>
        /// <param name="seriesInstanceUniqueIds">The series' instance UniqueIds</param>
        /// </summary>
        public IList<DataSet> CFindInstance(IEnumerable<string> studyInstanceUniqueIds, IEnumerable<string> seriesInstanceUniqueIds)
        {
            const string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelFIND;
            var datasets = new List<DataSet>();
            List<string> seriesNotCached = RetrieveItemsFromTheCache(seriesInstanceUniqueIds, datasets);
            List<string> studiesNotCached = RetrieveItemsFromTheCache(studyInstanceUniqueIds, datasets);
            var dataset = new DataSet();
            dataset.FileMetaInfo = GenerateFileMetaInfo(sopClassUniqueId);
            dataset.PutUI(Tags.SOPInstanceUniqueId);
            dataset.PutCS(Tags.QueryRetrieveLevel, "IMAGE");
            dataset.PutUI(Tags.SeriesInstanceUniqueId, seriesNotCached.ToArray());
            dataset.PutUI(Tags.StudyInstanceUniqueId, studiesNotCached.ToArray());
            datasets.AddRange(RetrieveDatasetsFromServiceClassProvider(dataset, sopClassUniqueId));
            return datasets;
        }

        /// <summary>
        /// Send C-Move for studies and series to be stored in the specified applicationEntityDestination
        /// <param name="studyInstanceUniqueIds">The studies' instance UniqueIds</param>
        /// <param name="seriesInstanceUniqueIds">The series' instance UniqueIds</param>
        /// <param name="applicationEntityDestination">The SCP that will store the files</param>
        /// </summary>
        public IList<DataSet> CMove(IEnumerable<string> studyInstanceUniqueIds, IEnumerable<string> seriesInstanceUniqueIds,
                                    string applicationEntityDestination)
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;

            var foundDatasets = new List<DataSet>();
            var dataSetsToMove = new List<DataSet>();
            List<string> studies = RetrieveItemsFromTheCache(studyInstanceUniqueIds, dataSetsToMove);
            List<string> series = RetrieveItemsFromTheCache(seriesInstanceUniqueIds, dataSetsToMove);
            dataSetsToMove.AddRange(CFindSeries(series));
            dataSetsToMove.AddRange(CFindStudies(studies));
            try
            {
                const string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelMOVE;
                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, sopClassUniqueId, DefinedTransferSyntaxes));
                IActiveAssociation activeAssociation = OpenAssociation();
                if (activeAssociation != null)
                {
                    IAssociation association = activeAssociation.Association;
                    if (association.GetAcceptedPresContext(sopClassUniqueId, TransferSyntaxUniqueId) == null)
                    {
                        Logger.Error("SOP class UniqueId not supported");
                    } else
                    {
                        foreach (DataSet dataset in dataSetsToMove)
                        {
                            IDicomCommand dicomCommand = _dcmObjectFactory.NewCommand();
                            IDicomCommand cMoveRequest = dicomCommand.InitCMoveRQ(association.NextMsgID(), sopClassUniqueId, 1,
                                                                                  applicationEntityDestination);
                            IDimse dimseRequest = _associationFactory.NewDimse(pcid, cMoveRequest, dataset);
                            Logger.Info(String.Format("CMove from {0} @ {1} {2}:{3} to {4}", _aAssociateRequest.Name,
                                                      _aAssociateRequest.ApplicationEntityTitle, _hostName, _port,
                                                      applicationEntityDestination));
                            FutureDimseResponse dimseResponse = activeAssociation.Invoke(dimseRequest);
                            while (!dimseResponse.IsReady())
                            {
                                Thread.Sleep(0);
                            }
                            foundDatasets.AddRange(dimseResponse.ListPending().Select(dimse => dimse.DataSet));
                        }
                        activeAssociation.Release(true);
                    }
                }
            } finally
            {
                _aAssociateRequest.RemovePresentationContext(pcid);
            }

            return foundDatasets;
        }

        /// <summary>
        /// Send C-GET
        /// <param name="studyInstanceUniqueId">A study instance UniqueId</param>
        /// <param name="seriesInstanceUniqueId">A series instance UniqueId</param>
        /// <param name="sopInstanceUniqueId">A sop instance instance UniqueId</param>
        /// </summary>
        public IList<DataSet> CGet(string studyInstanceUniqueId, string seriesInstanceUniqueId, string sopInstanceUniqueId)
        {
            if ((studyInstanceUniqueId == null) && (seriesInstanceUniqueId == null) && (sopInstanceUniqueId == null))
            {
                return null;
            }
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;
            var datasets = new List<DataSet>();
            try
            {
                string sopClassUniqueId = UIDs.StudyRootQueryRetrieveInformationModelGET;
                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, sopClassUniqueId, DefinedTransferSyntaxes));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    var dataset = new DataSet();
                    dataset.FileMetaInfo = GenerateFileMetaInfo(sopClassUniqueId);
                    dataset.PutUI(Tags.StudyInstanceUniqueId, studyInstanceUniqueId);
                    dataset.PutUI(Tags.SeriesInstanceUniqueId, seriesInstanceUniqueId);
                    dataset.PutUI(Tags.SOPInstanceUniqueId, sopInstanceUniqueId);
                    IAssociation association = active.Association;
                    if ((association.GetAcceptedPresContext(sopClassUniqueId, TransferSyntaxUniqueId)) == null)
                    {
                        Logger.Error("SOP class UniqueId not supported");
                        return null;
                    }
                    DicomCommand cGetDicomCommand = _dcmObjectFactory.NewCommand().InitCGetRQ(association.NextMsgID(),sopClassUniqueId,(int) DicomCommandMessage.HIGH);
                    IDimse dimseRequest = _associationFactory.NewDimse(pcid, cGetDicomCommand, dataset);
                    FutureDimseResponse dimseResponse = active.Invoke(dimseRequest);
                    active.Release(true);
                    while (!dimseResponse.IsReady())
                    {
                        Thread.Sleep(0);
                    }
                    datasets.AddRange(dimseResponse.ListPending().Select(dimse => dimse.DataSet));
                    _aAssociateRequest.RemovePresentationContext(pcid);
                }
            } finally
            {
                _aAssociateRequest.RemovePresentationContext(pcid);
            }
            return datasets;
        }

        #endregion

        /// <summary>
        /// Send C-STORE
        /// </summary>
        /// <param name="fileName"></param>
        public bool CStore(String fileName)
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;

            Stream ins = null;
            DcmParser dcmParser = null;
            DataSet dataSet = null;

            try
            {
                // Load DICOM file
                try
                {
                    ins = new BufferedStream(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                    dcmParser = _dcmParserFactory.NewDcmParser(ins);
                    FileFormat format = dcmParser.DetectFileFormat();
                    if (format != null)
                    {
                        dataSet = _dcmObjectFactory.NewDataset();
                        dcmParser.DcmHandler = dataSet.DcmHandler;
                        dcmParser.ParseDcmFile(format, Tags.PixelData);
                        Logger.Debug("Reading done");
                    } else
                    {
                        Logger.Error("Unknown format!");
                    }
                } catch (IOException e)
                {
                    Logger.Error(e);
                }

                //
                // Prepare association
                //
                string classUniqueId = dataSet.GetString(Tags.SOPClassUniqueId);
                string tsUniqueId = dataSet.GetString(Tags.TransferSyntaxUniqueId);

                if ((tsUniqueId == null || tsUniqueId.Equals("")) && (dataSet.FileMetaInfo != null))
                {
                    tsUniqueId = dataSet.FileMetaInfo.GetString(Tags.TransferSyntaxUniqueId);
                }

                if (tsUniqueId == null || tsUniqueId.Equals(""))
                {
                    tsUniqueId = UIDs.ImplicitVRLittleEndian;
                }

                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, classUniqueId, new[] {tsUniqueId}));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    bool bResponse = false;

                    FutureDimseResponse frsp = SendDataset(active, dcmParser, dataSet);
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
                _aAssociateRequest.RemovePresentationContext(pcid);
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
        /// <param name="dataSet"></param>
        public bool CStore(DataSet dataSet)
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;

            try
            {
                //
                // Prepare association
                //
                String classUniqueId = dataSet.GetString(Tags.SOPClassUniqueId);
                String tsUniqueId = dataSet.GetString(Tags.TransferSyntaxUniqueId);

                if (string.IsNullOrEmpty(tsUniqueId) && (dataSet.FileMetaInfo != null))
                {
                    tsUniqueId = dataSet.FileMetaInfo.GetString(Tags.TransferSyntaxUniqueId);
                }

                if (string.IsNullOrEmpty(tsUniqueId))
                {
                    tsUniqueId = UIDs.ImplicitVRLittleEndian;
                }

                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, classUniqueId, new[] {tsUniqueId}));
                IActiveAssociation active = OpenAssociation();
                if (active != null)
                {
                    bool bResponse = false;
                    FutureDimseResponse frsp = SendDataset(active, null, dataSet);
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
                _aAssociateRequest.RemovePresentationContext(pcid);
            }

            return false;
        }

        private void ClearCache(object sender, ElapsedEventArgs e)
        {
            Logger.Info("Clearing Cache");
            _cacheManager.Clear();
        }

        private List<string> RetrieveItemsFromTheCache(IEnumerable<string> UniqueIds, List<DataSet> datasets)
        {
            var UniqueIdsNotCached = new List<string>();
            foreach (string UniqueId in UniqueIds)
            {
                if (_cacheManager.ContainsKey(UniqueId))
                {
                    datasets.AddRange(_cacheManager[UniqueId]);
                } else
                {
                    UniqueIdsNotCached.Add(UniqueId);
                }
            }
            return UniqueIdsNotCached;
        }

        private IActiveAssociation OpenAssociation()
        {
            IAssociation association = _associationFactory.NewRequestor(_hostName, _port);
            IPdu assocAC = association.Connect(_aAssociateRequest, AssociateTimeOut);
            if (assocAC is AAssociateRJ)
            {
                var aAssociateRj = ((AAssociateRJ) assocAC);
                throw new DcmServiceException(aAssociateRj.Reason(), aAssociateRj.ReasonAsString());
            }
            _activeAssociation = _associationFactory.NewActiveAssociation(association, null);
            _activeAssociation.Timeout = AssociateTimeOut;
            _activeAssociation.Start();
            return _activeAssociation;
        }

        private List<DataSet> RetrieveDatasetsFromServiceClassProvider(DataSet dataSet, string sopClassUniqueId)
        {
            int pcid = _presentationContextIdStart;
            _presentationContextIdStart += 2;
            var datasets = new List<DataSet>();
            try
            {
                _aAssociateRequest.AddPresContext(_associationFactory.NewPresContext(pcid, sopClassUniqueId, DefinedTransferSyntaxes));
                IActiveAssociation activeAssociation = OpenAssociation();
                if (activeAssociation != null)
                {
                    datasets = ExecuteCFindDicomCommand(activeAssociation, dataSet, pcid);
                }
            } finally
            {
                _aAssociateRequest.RemovePresentationContext(pcid);
            }
            return datasets;
        }

        private List<DataSet> ExecuteCFindDicomCommand(IActiveAssociation active, DataSet dataSet, int pcid)
        {
            IAssociation association = active.Association;
            if (association.GetAcceptedPresContext(UIDs.StudyRootQueryRetrieveInformationModelFIND, TransferSyntaxUniqueId) == null)
            {
                Logger.Error("SOP class UniqueId not supported");
                return null;
            }
            IDicomCommand cFindDicomCommand = _dcmObjectFactory.NewCommand().InitCFindRQ(association.NextMsgID(),
                                                                                         UIDs.
                                                                                             StudyRootQueryRetrieveInformationModelFIND,
                                                                                         (int) DicomCommandMessage.HIGH);
            IDimse dimseRequest = _associationFactory.NewDimse(pcid, cFindDicomCommand, dataSet);
            Logger.Info(String.Format("CFind as {0} @ {1} {2}:{3}", _aAssociateRequest.Name,
                                      _aAssociateRequest.ApplicationEntityTitle, _hostName, _port));
            FutureDimseResponse dimseResponse = active.Invoke(dimseRequest);
            active.Release(true);
            while (!dimseResponse.IsReady())
            {
                Thread.Sleep(0);
            }
            return dimseResponse.ListPending().Select(dimse => dimse.DataSet).ToList();
        }

        private FutureDimseResponse SendDataset(IActiveAssociation activeAssociation, DcmParser parser, DataSet dataSet)
        {
            String sopInstUniqueId = dataSet.GetString(Tags.SOPInstanceUniqueId);
            if (sopInstUniqueId == null)
            {
                Logger.Error("SOP instance UniqueId is null");
                return null;
            }
            String sopClassUniqueId = dataSet.GetString(Tags.SOPClassUniqueId);
            if (sopClassUniqueId == null)
            {
                Logger.Error("SOP class UniqueId is null");
                return null;
            }
            PresentationContext pc = null;
            IAssociation association = activeAssociation.Association;

            if (parser != null)
            {
                if (parser.DcmDecodeParam.encapsulated)
                {
                    String tsUniqueId = dataSet.FileMetaInfo.TransferSyntaxUniqueId;
                    if ((pc = association.GetAcceptedPresContext(sopClassUniqueId, tsUniqueId)) == null)
                    {
                        Logger.Error("SOP class UniqueId not supported");
                        return null;
                    }
                } else if (IsSopClassUniqueIdNotSupported(association, sopClassUniqueId, out pc))
                {
                    Logger.Error("SOP class UniqueId not supported");
                    return null;
                }

                DicomCommand cStoreRequest = _dcmObjectFactory.NewCommand().InitCStoreRQ(association.NextMsgID(), sopClassUniqueId,
                                                                                         sopInstUniqueId,
                                                                                         (int) DicomCommandMessage.HIGH);
                return
                    activeAssociation.Invoke(_associationFactory.NewDimse(pc.pcid(), cStoreRequest,
                                                                          new FileDataSource(parser, dataSet, new byte[2048])));
            }
            if ((dataSet.FileMetaInfo != null) && (dataSet.FileMetaInfo.TransferSyntaxUniqueId != null))
            {
                String tsUniqueId = dataSet.FileMetaInfo.TransferSyntaxUniqueId;
                if ((pc = association.GetAcceptedPresContext(sopClassUniqueId, tsUniqueId)) == null)
                {
                    Logger.Error("SOP class UniqueId not supported");
                    return null;
                }
            } else if (IsSopClassUniqueIdNotSupported(association, sopClassUniqueId, out pc))
            {
                Logger.Error("SOP class UniqueId not supported");
                return null;
            }

            DicomCommand cStoreRq = _dcmObjectFactory.NewCommand().InitCStoreRQ(association.NextMsgID(), sopClassUniqueId,
                                                                                sopInstUniqueId, (int) DicomCommandMessage.HIGH);
            IDimse dimse = _associationFactory.NewDimse(pc.pcid(), cStoreRq, dataSet);
            return activeAssociation.Invoke(dimse);
        }

        private static bool IsSopClassUniqueIdNotSupported(IAssociation association, string sopClassUniqueId,
                                                           out PresentationContext pc)
        {
            return (pc = association.GetAcceptedPresContext(sopClassUniqueId, UIDs.ImplicitVRLittleEndian)) == null;
        }

        private FileMetaInfo GenerateFileMetaInfo(string sopClassUniqueId)
        {
            var fileMetaInfo = new FileMetaInfo();
            fileMetaInfo.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
            fileMetaInfo.PutUI(Tags.MediaStorageSOPClassUniqueId, sopClassUniqueId);
            fileMetaInfo.PutUI(Tags.TransferSyntaxUniqueId, TransferSyntaxUniqueId);
            fileMetaInfo.PutSH(Tags.ImplementationVersionName, "dicomSharp-SCU");
            return fileMetaInfo;
        }

        private static void Copy(Stream ins, Stream outs, int len, bool swap, byte[] buffer)
        {
            if (swap && (len & 1) != 0)
            {
                throw new DcmParseException("Illegal Length of OW Pixel Data: " + len);
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
            private readonly byte[] _buffer;
            private readonly DataSet _dataSet;
            private readonly DcmParser _parser;

            public FileDataSource(DcmParser parser, DataSet dataSet, byte[] buffer)
            {
                _parser = parser;
                _dataSet = dataSet;
                _buffer = buffer;
            }

            #region IDataSource Members

            public void WriteTo(Stream outs, String transferSyntaxUniqueId)
            {
                DcmEncodeParam netParam = DcmDecodeParam.ValueOf(transferSyntaxUniqueId);
                _dataSet.WriteDataSet(outs, netParam);
                if (_parser.ReadTag == Tags.PixelData)
                {
                    DcmDecodeParam fileParam = _parser.DcmDecodeParam;
                    _dataSet.WriteHeader(outs, netParam, _parser.ReadTag, _parser.ReadVR, _parser.ReadLength);
                    if (netParam.encapsulated)
                    {
                        _parser.ParseHeader();
                        while (_parser.ReadTag == Tags.Item)
                        {
                            _dataSet.WriteHeader(outs, netParam, _parser.ReadTag, _parser.ReadVR, _parser.ReadLength);
                            Copy(_parser.InputStream, outs, _parser.ReadLength, false, _buffer);
                        }
                        if (_parser.ReadTag != Tags.SeqDelimitationItem)
                        {
                            throw new DcmParseException("Unexpected Tag:" + Tags.ToHexString(_parser.ReadTag));
                        }
                        if (_parser.ReadLength != 0)
                        {
                            throw new DcmParseException("(fffe,e0dd), Length:" + _parser.ReadLength);
                        }
                        _dataSet.WriteHeader(outs, netParam, Tags.SeqDelimitationItem, VRs.NONE, 0);
                    } else
                    {
                        bool swap = fileParam.byteOrder != netParam.byteOrder && _parser.ReadVR == VRs.OW;
                        Copy(_parser.InputStream, outs, _parser.ReadLength, swap, _buffer);
                    }
                    _dataSet.Clear();
                    _parser.ParseDataset(fileParam, 0);
                    _dataSet.WriteDataSet(outs, netParam);
                }
            }

            #endregion
        }

        #endregion
    }
}