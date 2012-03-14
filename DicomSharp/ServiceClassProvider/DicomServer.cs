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

#endregion

using System;
using System.IO;
using DicomSharp.Dictionary;
using DicomSharp.Net;
using DicomSharp.Server;

namespace DicomSharp.ServiceClassProvider {
    public class DicomServer {
        private static readonly ServerFactory ServerFactory = ServerFactory.Instance;
        private static readonly AssociationFactory AssociationFactory = AssociationFactory.Instance;
        private readonly String[] STORAGE_AS;
        private readonly DcmServiceRegistry services;
        private readonly StoreSCP storeSCP;
        private IDcmAssociationHandler _associationHandler;
        private int port = 104;
        private TcpServer _tcpServer;

        public DicomServer() {
            services = AssociationFactory.NewDcmServiceRegistry();
            storeSCP = new StoreSCP();
            STORAGE_AS = new[] {
                                   UIDs.Verification,
                                   UIDs.BasicStudyContentNotificationRetired,
                                   UIDs.StorageCommitmentPushModel,
                                   UIDs.ProceduralEventLogging,
                                   UIDs.SubstanceAdministrationLogging,
                                   UIDs.DetachedPatientManagementRetired,
                                   UIDs.DetachedVisitManagementRetired,
                                   UIDs.DetachedStudyManagementRetired,
                                   UIDs.StudyComponentManagementRetired,
                                   UIDs.ModalityPerformedProcedureStep,
                                   UIDs.ModalityPerformedProcedureStepRetrieve,
                                   UIDs.ModalityPerformedProcedureStepNotification,
                                   UIDs.DetachedResultsManagementRetired,
                                   UIDs.DetachedInterpretationManagementRetired,
                                   UIDs.BasicFilmSession,
                                   UIDs.BasicFilmBoxSOP,
                                   UIDs.BasicGrayscaleImageBox,
                                   UIDs.BasicColorImageBox,
                                   UIDs.ReferencedImageBoxRetired,
                                   UIDs.PrintJob,
                                   UIDs.BasicAnnotationBox,
                                   UIDs.Printer,
                                   UIDs.PrinterConfigurationRetrieval,
                                   UIDs.VOILUTBox,
                                   UIDs.PresentationLUT,
                                   UIDs.ImageOverlayBoxRetired,
                                   UIDs.BasicPrintImageOverlayBoxRetired,
                                   UIDs.PrintQueueManagementRetired,
                                   UIDs.StoredPrintStorageRetired,
                                   UIDs.HardcopyGrayscaleImageStorageRetired,
                                   UIDs.HardcopyColorImageStorageRetired,
                                   UIDs.PullPrintRequestRetired,
                                   UIDs.MediaCreationManagement,
                                   UIDs.ComputedRadiographyImageStorage,
                                   UIDs.DigitalXRayImageStorageForPresentation,
                                   UIDs.DigitalXRayImageStorageForProcessing,
                                   UIDs.DigitalMammographyXRayImageStorageForPresentation,
                                   UIDs.DigitalMammographyXRayImageStorageForProcessing,
                                   UIDs.DigitalIntraoralXRayImageStorageForPresentation,
                                   UIDs.DigitalIntraoralXRayImageStorageForProcessing,
                                   UIDs.CTImageStorage,
                                   UIDs.EnhancedCTImageStorage,
                                   UIDs.UltrasoundMultiframeImageStorageRetired,
                                   UIDs.UltrasoundMultiframeImageStorage,
                                   UIDs.MRImageStorage,
                                   UIDs.EnhancedMRImageStorage,
                                   UIDs.MRSpectroscopyStorage,
                                   UIDs.NuclearMedicineImageStorageRetired,
                                   UIDs.UltrasoundImageStorageRetired,
                                   UIDs.UltrasoundImageStorage,
                                   UIDs.SecondaryCaptureImageStorage,
                                   UIDs.MultiframeSingleBitSecondaryCaptureImageStorage,
                                   UIDs.MultiframeGrayscaleByteSecondaryCaptureImageStorage,
                                   UIDs.MultiframeGrayscaleWordSecondaryCaptureImageStorage,
                                   UIDs.MultiframeTrueColorSecondaryCaptureImageStorage,
                                   UIDs.StandaloneOverlayStorageRetired,
                                   UIDs.StandaloneCurveStorageRetired,
                                   UIDs.WaveformStorageTrialRetired,
                                   UIDs.TwelveLeadECGWaveformStorage,
                                   UIDs.GeneralECGWaveformStorage,
                                   UIDs.AmbulatoryECGWaveformStorage,
                                   UIDs.HemodynamicWaveformStorage,
                                   UIDs.CardiacElectrophysiologyWaveformStorage,
                                   UIDs.BasicVoiceAudioWaveformStorage,
                                   UIDs.StandaloneModalityLUTStorageRetired,
                                   UIDs.StandaloneVOILUTStorageRetired,
                                   UIDs.GrayscaleSoftcopyPresentationStateStorage,
                                   UIDs.ColorSoftcopyPresentationStateStorage,
                                   UIDs.PseudoColorSoftcopyPresentationStateStorage,
                                   UIDs.BlendingSoftcopyPresentationStateStorage,
                                   UIDs.XRayAngiographicImageStorage,
                                   UIDs.EnhancedXAImageStorage,
                                   UIDs.XRayRadiofluoroscopicImageStorage,
                                   UIDs.EnhancedXRFImageStorage,
                                   UIDs.XRay3DAngiographicImageStorage,
                                   UIDs.XRay3DCraniofacialImageStorage,
                                   UIDs.XRayAngiographicBiPlaneImageStorageRetired,
                                   UIDs.NuclearMedicineImageStorage,
                                   UIDs.RawDataStorage,
                                   UIDs.SpatialRegistrationStorage,
                                   UIDs.SpatialFiducialsStorage,
                                   UIDs.DeformableSpatialRegistrationStorage,
                                   UIDs.SegmentationStorage,
                                   UIDs.RealWorldValueMappingStorage,
                                   UIDs.VLImageStorageRetired,
                                   UIDs.VLMultiframeImageStorageRetired,
                                   UIDs.VLEndoscopicImageStorage,
                                   UIDs.VideoEndoscopicImageStorage,
                                   UIDs.VLMicroscopicImageStorage,
                                   UIDs.VideoMicroscopicImageStorage,
                                   UIDs.VLSlideCoordinatesMicroscopicImageStorage,
                                   UIDs.VLPhotographicImageStorage,
                                   UIDs.VideoPhotographicImageStorage,
                                   UIDs.OphthalmicPhotography8BitImageStorage,
                                   UIDs.OphthalmicPhotography16BitImageStorage,
                                   UIDs.StereometricRelationshipStorage,
                                   UIDs.OphthalmicTomographyImageStorage,
                                   UIDs.TextSRStorageTrialRetired,
                                   UIDs.AudioSRStorageTrialRetired,
                                   UIDs.DetailSRStorageTrialRetired,
                                   UIDs.ComprehensiveSRStorageRetired,
                                   UIDs.ProcedureLogStorage,
                                   UIDs.MammographyCADSRStorage,
                                   UIDs.KeyObjectSelectionDocumentStorage,
                                   UIDs.ChestCADSRStorage,
                                   UIDs.XRayRadiationDoseSR,
                                   UIDs.EncapsulatedPDFStorage,
                                   UIDs.EncapsulatedCDAStorage,
                                   UIDs.PositronEmissionTomographyImageStorage,
                                   UIDs.StandalonePETCurveStorageRetired,
                                   UIDs.RTImageStorage,
                                   UIDs.RTDoseStorage,
                                   UIDs.RTStructureSetStorage,
                                   UIDs.RTBeamsTreatmentRecordStorage,
                                   UIDs.RTPlanStorage,
                                   UIDs.RTBrachyTreatmentRecordStorage,
                                   UIDs.RTTreatmentSummaryRecordStorage,
                                   UIDs.RTIonPlanStorage,
                                   UIDs.RTIonBeamsTreatmentRecordStorage,
                                   UIDs.PatientRootQueryRetrieveInformationModelFIND,
                                   UIDs.PatientRootQueryRetrieveInformationModelMOVE,
                                   UIDs.PatientRootQueryRetrieveInformationModelGET,
                                   UIDs.StudyRootQueryRetrieveInformationModelFIND,
                                   UIDs.StudyRootQueryRetrieveInformationModelMOVE,
                                   UIDs.StudyRootQueryRetrieveInformationModelGET,
                                   UIDs.PatientStudyOnlyQueryRetrieveInformationModelFINDRetired,
                                   UIDs.PatientStudyOnlyQueryRetrieveInformationModelMOVERetired,
                                   UIDs.PatientStudyOnlyQueryRetrieveInformationModelGETRetired,
                                   UIDs.ModalityWorklistInformationModelFIND,
                                   UIDs.GeneralPurposeWorklistInformationModelFIND,
                                   UIDs.GeneralPurposeScheduledProcedureStep,
                                   UIDs.GeneralPurposePerformedProcedureStep,
                                   UIDs.InstanceAvailabilityNotification,
                                   UIDs.RTBeamsDeliveryInstructionDraft74,
                                   UIDs.RTConventionalMachineVerificationDraft74,
                                   UIDs.RTIonMachineVerificationDraft74,
                                   UIDs.UnifiedProcedureStepPush,
                                   UIDs.UnifiedProcedureStepWatch,
                                   UIDs.UnifiedProcedureStepPull,
                                   UIDs.UnifiedProcedureStepEvent,
                                   UIDs.GeneralReleventPatientInformationQuery,
                                   UIDs.BreastImagingReleventPatientInformationQuery,
                                   UIDs.CardiacReleventPatientInformationQuery,
                                   UIDs.HangingProtocolStorage,
                                   UIDs.HangingProtocolInformationModelFIND,
                                   UIDs.HangingProtocolInformationModelMOVE,
                                   UIDs.ProductCharacteristicsQuery,
                                   UIDs.SubstanceApprovalQuery
                               };

            InitServices();
        }

        public virtual AcceptorPolicy Policy {
            set {
                _associationHandler = ServerFactory.NewDcmHandler(value, services);
                _tcpServer = ServerFactory.NewServer(_associationHandler);
            }
        }

        public virtual int Port {
            get { return port; }
            set { port = value; }
        }

        public virtual String ArchiveDir {
            get { return storeSCP.ArchiveDirectory.FullName; }
            set { storeSCP.ArchiveDirectory = new FileInfo(value); }
        }

        public virtual void Start() {
            if (_tcpServer == null) {
                throw new SystemException();
            }
            _tcpServer.Start(port);
        }

        public virtual void Stop() {
            if (_tcpServer != null) {
                _tcpServer.Stop();
            }
        }


        private void InitServices(){
            foreach (string storageAssocationSyntax in STORAGE_AS) {
                services.Bind(storageAssocationSyntax, storeSCP);
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////

        [STAThread]
        public static void Main() {
            var srv = new DicomServer();
            var policy = new AcceptorPolicyService();
            srv.Policy = policy.AcceptorPolicy;
            srv.Start();
        }
    }
}