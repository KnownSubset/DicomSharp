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
using System.Collections;
using System.Text;
using Dicom.Dictionary;
using Dicom.Net;
using Dicom.Utility;

namespace Dicom.ServiceClassProvider {
    public class AcceptorPolicyService : ExtNegotiatorI {
        // Constants -----------------------------------------------------
        private static readonly String[] TS_UIDS =
            {
                UIDs.ExplicitVRLittleEndian,
                UIDs.ImplicitVRLittleEndian,
                UIDs.JPEGLossless
            };

        private static readonly String[] AS_UIDS =
            {
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

        private static readonly AssociationFactory assocFact = AssociationFactory.Instance;
        private readonly AcceptorPolicy policy;

        /// <summary>
        /// Constructor
        /// </summary>
        public AcceptorPolicyService() {
            policy = assocFact.NewAcceptorPolicy();
            for (int i = 1; i < AS_UIDS.Length; ++i) {
                policy.PutPresContext(AS_UIDS[i], TS_UIDS);
            }
            PatientRootRelationalQuery = true;
            StudyRootRelationalQuery = true;
            PatientRootRelationalRetrieve = true;
            StudyRootRelationalRetrieve = true;
            MaxNumOpsInvoked = 0;
        }

        public virtual String Name {
            get { return "AcceptorPolicy"; }
        }

        public virtual int MaxPduLength {
            get { return policy.MaxPduLength; }
            set { policy.MaxPduLength = value; }
        }

        public virtual String CalledAETs {
            get { return aetsToString(policy.CalledAETs); }
            set { policy.CalledAETs = "<any>".ToUpper().Equals(value.Trim().ToUpper()) ? null : toStringArray(value); }
        }

        public virtual String CallingAETs {
            get { return aetsToString(policy.CallingAETs); }
            set { policy.CallingAETs = "<any>".ToUpper().Equals(value.Trim().ToUpper()) ? null : toStringArray(value); }
        }

        public virtual int MaxNumOpsInvoked {
            get {
                AsyncOpsWindow aow = policy.AsyncOpsWindow;
                return aow != null ? aow.MaxOpsInvoked : 1;
            }
            set { policy.setAsyncOpsWindow(value, 1); }
        }

        public virtual String[] PresContext {
            get {
                ArrayList list = policy.ListPresContext();
                var retval = new String[list.Count];
                for (int i = 0; i < retval.Length; ++i) {
                    retval[i] = toString((PresContext) list[i]);
                }
                return retval;
            }
        }

        public virtual bool PatientRootRelationalQuery {
            set { SetRelational(UIDs.PatientRootQueryRetrieveInformationModelFIND, value); }
        }

        public virtual bool StudyRootRelationalQuery {
            set { SetRelational(UIDs.StudyRootQueryRetrieveInformationModelFIND, value); }
        }

        public virtual bool PatientRootRelationalRetrieve {
            set { SetRelational(UIDs.PatientRootQueryRetrieveInformationModelMOVE, value); }
        }

        public virtual bool StudyRootRelationalRetrieve {
            set { SetRelational(UIDs.StudyRootQueryRetrieveInformationModelMOVE, value); }
        }

        public virtual AcceptorPolicy AcceptorPolicy {
            get { return policy; }
        }

        #region ExtNegotiatorI Members

        public virtual byte[] Negotiate(byte[] offered) {
            return offered;
        }

        #endregion

        private String aetsToString(String[] array) {
            return array != null ? array.ToString() : "<any>";
        }

        private String[] toStringArray(String s) {
            var stk = new Tokenizer(s, " ,;[]\t\r\n");
            if (!stk.HasMoreTokens()) {
                return null;
            }
            var array = new String[stk.Count];
            for (int i = 0; i < array.Length; ++i) {
                array[i] = stk.NextToken();
            }
            return array;
        }


        private String toString(PresContext pc) {
            var sb = new StringBuilder(64);
            sb.Append(UIDs.GetName(pc.AbstractSyntaxUID)).Append(pc.TransferSyntaxUIDs);
            return sb.ToString();
        }

        public virtual void addPresContext(String pc) {
            String[] tsuids = TS_UIDS;
            String[] a = toStringArray(pc);
            if (a.Length == 0) {
                throw new NullReferenceException();
            }
            if (a.Length > 1) {
                tsuids = new String[a.Length - 1];
                Array.Copy(a, 1, tsuids, 0, tsuids.Length);
            }
            policy.PutPresContext(a[0], tsuids);
        }

        public virtual void removePresContext(String asuid) {
            policy.PutPresContext(asuid, null);
        }

        private bool IsRelational(String uid) {
            return policy.GetExtNegPolicy(uid) != null;
        }

        public bool IsPatientRootRelationalQuery() {
            return IsRelational(UIDs.PatientRootQueryRetrieveInformationModelFIND);
        }

        public bool IsStudyRootRelationalQuery() {
            return IsRelational(UIDs.StudyRootQueryRetrieveInformationModelFIND);
        }

        public bool IsPatientRootRelationalRetrieve() {
            return IsRelational(UIDs.PatientRootQueryRetrieveInformationModelMOVE);
        }

        public bool IsStudyRootRelationalRetrieve() {
            return IsRelational(UIDs.StudyRootQueryRetrieveInformationModelMOVE);
        }

        private void SetRelational(String uid, bool rel) {
            policy.PutExtNegPolicy(uid, rel ? this : null);
        }

        // ExtNegotiator
    }
}