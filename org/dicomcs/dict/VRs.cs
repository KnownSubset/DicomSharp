#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2008 Fang Yang. All rights reserved.
// 
// This file is part of dicomcs, see http://www.sourceforge.net/projects/dicom-cs
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
// Fang Yang (yangfang@email.com)
//
// 7/22/08: Solved bug by Maarten JB van Ettinger. Missing UID added (added line 1930).
// VRs updated by Marcel de Wijs.
#endregion

namespace org.dicomcs.dict
{
	using System;
	
	
	/// <summary>
	/// Provides VR constants and VR related utility functions.
	/// <p>
	/// Further Information regarding Value Representation (DICOM data types)
	/// can be found at: <br>
	/// <code>PS 3.5 - 2000 Section 6.2 Page 15</code>
	/// </p>
	/// </summary>
	public class VRs
	{
		
		/// <summary> 
		/// Private constructor.
		/// </summary>
		private VRs()
		{
		}
		
		public static String ToString(int vr)
		{
			return (vr == NONE? "NONE" : new String( new char[]{(char)( vr >> 8 ), (char)(vr & 0x00ff)} ) );
		}
		
		public static int ValueOf(String str)
		{
			if ("NONE".Equals(str))
				return VRs.NONE;
			
			if (str.Length != 2)
				throw new System.ArgumentException(str);
			
			return ((str[0] & 0xff) << 8) | (str[1] & 0xff);
		}
		
		/// <summary> 
		/// NULL element for VRs. Use as VR value for Data Elements,
		/// Item (FFFE,E000), Item Delimitation Item (FFFE,E00D), and
		/// Sequence Delimitation Item (FFFE,E0DD).
		/// </summary>
		public const int NONE = 0x0000;
		public const int AE = 0x4145;
		public const int AS = 0x4153;
		public const int AT = 0x4154;
		public const int CS = 0x4353;
		public const int DA = 0x4441;
		public const int DS = 0x4453;
		public const int DT = 0x4454;
		public const int FL = 0x464C;
		public const int FD = 0x4644;
		public const int IS = 0x4953;
		public const int LO = 0x4C4F;
		public const int LT = 0x4C54;
		public const int OB = 0x4F42;
		public const int OW = 0x4F57;
		public const int PN = 0x504E;
		public const int SH = 0x5348;
		public const int SL = 0x534C;
		public const int SQ = 0x5351;
		public const int SS = 0x5353;
		public const int ST = 0x5354;
		public const int TM = 0x544D;
		public const int UI = 0x5549;
		public const int UL = 0x554C;
		public const int UN = 0x554E;
		public const int US = 0x5553;
		public const int UT = 0x5554;
		
		/// <summary>
		/// These VRs have 2 bytes fixed length
		/// </summary>
		/// <param name="vr"></param>
		/// <returns></returns>
		public static bool IsLengthField16Bit(int vr)
		{
			switch (vr)
			{
				case AE: 
				case AS: 
				case AT: 
				case CS: 
				case DA: 
				case DS:
				case DT:
				case FL:
				case FD:
				case IS:
				case LO:
				case LT:
				case PN:
				case SH:
				case SL:
				case SS:
				case ST:
				case TM:
				case UI:
				case UL:
				case US: 
					return true;
				
				default: 
					return false;
				
			}
		}
		
		public static int GetPadding(int vr)
		{
			switch (vr)
			{
				case AE:
				case AS:
				case CS:
				case DA:
				case DS:
				case DT:
				case IS:
				case LO:
				case LT:
				case PN:
				case SH:
				case SL:
				case ST:
				case TM:
				case UT: 
					return ' ';
				
				default: 
					return 0;
				
			}
		}
		
		/// <summary>
		/// These VRs are string values
		/// </summary>
		/// <param name="vr"></param>
		/// <returns></returns>
		public static bool IsStringValue(int vr)
		{
			switch (vr)
			{
				case AE:
				case AS:
				case CS:
				case DA:
				case DS:
				case DT:
				case IS:
				case LO:
				case LT:
				case PN:
				case SH:
				case ST:
				case TM:
				case UI:
				case UT: 
					return true;
				
			}
			return false;
		}

		/// <summary>
		/// Get the VR of this tag
		/// </summary>
		/// <param name="tag">the tag</param>
		/// <returns></returns>
		public static int GetVR(uint tag)
		{
			if ((tag & 0x0000ffff) == 0)
			{
				return VRs.UL;
			}
			switch (tag & 0xffff0000)
			{
				case 0x00000000: 
					return vrOfCommand(tag);
				
				case 0x00020000: 
					return vrOfFileMetaInfo(tag);
				
				case 0x00040000: 
					return vrOfDicomDir(tag);
				
			}
			if ((tag & 0x00010000) != 0)
			{
				return ((tag & 0x0000ff00) == 0?VRs.LO:VRs.UN);
			}
			return vrOfData(tag);
		}
		
		public static int vrOfCommand(uint tag)
		{
			switch (tag)
			{
				case Tags.MoveDestination:
				case Tags.MoveOriginatorAET: 
					return VRs.AE;
				
				case Tags.OffendingElement:
				case Tags.AttributeIdentifierList: 
					return VRs.AT;
				
				case Tags.ErrorComment: 
					return VRs.LO;
				
				case Tags.AffectedSOPClassUID:
				case Tags.RequestedSOPClassUID:
				case Tags.AffectedSOPInstanceUID:
				case Tags.RequestedSOPInstanceUID: 
					return VRs.UI;
				
				case Tags.CommandLengthToEndRetired:
				case Tags.CommandRecognitionCodeRetired:
				case Tags.InitiatorRetired:
				case Tags.ReceiverRetired:
				case Tags.FindLocationRetired:
				case Tags.NumberOfMatchesRetired:
				case Tags.ResponseSequenceNumberRetired:
				case Tags.DialogReceiverRetired:
				case Tags.TerminalTypeRetired:
				case Tags.MessageSetIDRetired:
				case Tags.EndMessageIDRetired:
				case Tags.DisplayFormatRetired:
				case Tags.PagePositionIDRetired:
				case Tags.TextFormatIDRetired:
				case Tags.NorRevRetired:
				case Tags.AddGrayScaleRetired:
				case Tags.BordersRetired:
				case Tags.CopiesRetired:
				case Tags.MagnificationTypeRetired:
				case Tags.EraseRetired:
				case Tags.PrintRetired: 
					return VRs.UN;
				
				case Tags.CommandField:
				case Tags.MessageID:
				case Tags.MessageIDToBeingRespondedTo:
				case Tags.Priority:
				case Tags.DataSetType:
				case Tags.Status:
				case Tags.ErrorID:
				case Tags.EventTypeID:
				case Tags.ActionTypeID:
				case Tags.NumberOfRemainingSubOperations:
				case Tags.NumberOfCompletedSubOperations:
				case Tags.NumberOfFailedSubOperations:
				case Tags.NumberOfWarningSubOperations:
				case Tags.MoveOriginatorMessageID: 
					return VRs.US;
				
			}
			throw new ArgumentException("Unrecognized Tag " + Tags.ToHexString(tag));
		}
		
		public static int vrOfFileMetaInfo(uint tag)
		{
			switch (tag)
			{
				case Tags.SourceApplicationEntityTitle: 
					return VRs.AE;
				
				case Tags.FileMetaInformationVersion:
				case Tags.PrivateInformation: 
					return VRs.OB;
				
				case Tags.ImplementationVersionName: 
					return VRs.SH;
				
				case Tags.MediaStorageSOPClassUID:
				case Tags.MediaStorageSOPInstanceUID:
				case Tags.TransferSyntaxUID:
				case Tags.ImplementationClassUID:
				case Tags.PrivateInformationCreatorUID: 
					return VRs.UI;
				
			}
			throw new ArgumentException("Unrecognized Tag 0x" + Convert.ToString(tag, 16));
		}
		
		public static int vrOfDicomDir(uint tag)
		{
			switch (tag)
			{
				case Tags.FileSetID:
				case Tags.FileSetDescriptorFileID:
				case Tags.SpecificCharacterSetOfFileSetDescriptorFile:
				case Tags.DirectoryRecordType:
				case Tags.RefFileID: 
					return VRs.CS;
				
				case Tags.DirectoryRecordSeq: 
					return VRs.SQ;
				
				case Tags.PrivateRecordUID:
				case Tags.RefSOPClassUIDInFile:
				case Tags.RefSOPInstanceUIDInFile:
				case Tags.RefSOPTransferSyntaxUIDInFile: 
					return VRs.UI;
				
				case Tags.OffsetOfFirstRootDirectoryRecord:
				case Tags.OffsetOfLastRootDirectoryRecord:
				case Tags.OffsetOfNextDirectoryRecord:
				case Tags.OffsetOfLowerLevelDirectoryEntity:
				case Tags.MRDRDirectoryRecordOffset:
				case Tags.NumberOfReferences: 
					return VRs.UL;
				
				case Tags.FileSetConsistencyFlag:
				case Tags.RecordInUseFlag: 
					return VRs.US;
				
			}
			throw new ArgumentException("Unrecognized Tag 0x" + Convert.ToString(tag, 16));
		}
		
		public static int vrOfData(uint tag)
		{
			switch (tag)
			{
				case Tags.RetrieveAET: 
				case Tags.ScheduledStudyLocationAET: 
				case Tags.ScheduledStationAET: 
				case Tags.PerformedStationAET: 
				case Tags.Originator: 
				case Tags.DestinationAE: 
					return VRs.AE;
				
				case Tags.PatientAge: 
					return VRs.AS;
				
				case Tags.DimensionIndexPointer: 
				case Tags.FunctionalGroupSequencePointer: 
				case Tags.FrameIncrementPointer: 
				case Tags.OverrideParameterPointer: 
					return VRs.AT;
				
				case Tags.SpecificCharacterSet: 
				case Tags.ImageType: 
				case Tags.NuclearMedicineSeriesTypeRetired: 
				case Tags.QueryRetrieveLevel: 
				case Tags.InstanceAvailability: 
				case Tags.Modality:
				case Tags.ModalitiesInStudy:
				case Tags.ConversionType:
				case Tags.PresentationIntentType:
				case Tags.MappingResource:
				case Tags.CodeSetExtensionFlag:
				case Tags.ContextIdentifier:
				case Tags.LossyImageCompressionRetired:
				case Tags.TransducerPositionRetired:
				case Tags.TransducerOrientationRetired:
				case Tags.AnatomicStructureRetired:
				case Tags.FrameType:
				case Tags.PixelPresentation:
				case Tags.VolumeBasedCalculationTechnique:
				case Tags.ComplexImageComponent:
				case Tags.AcquisitionContrast:
				case Tags.PatientSex:
				case Tags.SmokingStatus:
				case Tags.BodyPartExamined:
				case Tags.ScanningSeq:
				case Tags.SeqVariant:
				case Tags.ScanOptions:
				case Tags.MRAcquisitionType:
				case Tags.AngioFlag:
				case Tags.TherapyType:
				case Tags.InterventionalStatus:
				case Tags.TherapyDescription:
				case Tags.AcquisitionTerminationCondition: 
				case Tags.AcquisitionStartCondition:
				case Tags.ContrastBolusIngredient:
				case Tags.SynchronizationTrigger:
				case Tags.BeatRejectionFlag:
				case Tags.TableMotion:
				case Tags.TableType:
				case Tags.RotationDirection:
				case Tags.FieldOfViewShape:
				case Tags.RadiationSetting:
				case Tags.RectificationType:
				case Tags.RadiationMode:
				case Tags.Grid:
				case Tags.CollimatorType:
				case Tags.AnodeTargetMaterial:
				case Tags.WholeBodyTechnique:
				case Tags.PhaseEncodingDirection:
				case Tags.VariableFlipAngleFlag:
				case Tags.CassetteOrientation:
				case Tags.CassetteSize:
				case Tags.ColumnAngulation:
				case Tags.TomoType:
				case Tags.TomoClass:
				case Tags.PositionerMotion:
				case Tags.PositionerType:
				case Tags.ShutterShape:
				case Tags.CollimatorShape:
				case Tags.AcquisitionTimeSynchronized:
				case Tags.TimeDistributionProtocol:
				case Tags.PatientPosition:
				case Tags.ViewPosition: 
				case Tags.TransducerType:
				case Tags.DetectorConditionsNominalFlag:
				case Tags.DetectorType:
				case Tags.DetectorConfiguration:
				case Tags.DetectorActiveShape:
				case Tags.FieldOfViewHorizontalFlip: 
				case Tags.FilterMaterial: 
				case Tags.ExposureControlMode:
				case Tags.ExposureStatus:
				case Tags.ContentQualification:
				case Tags.EchoPulseSeq:
				case Tags.InversionRecovery:
				case Tags.FlowCompensation:
				case Tags.MultipleSpinEcho:
				case Tags.MultiPlanarExcitation:
				case Tags.PhaseContrast:
				case Tags.TimeOfFlightContrast:
				case Tags.Spoiling:
				case Tags.SteadyStatePulseSeq:
				case Tags.EchoPlanarPulseSeq:
				case Tags.MagnetizationTransfer:
				case Tags.T2Preparation:
				case Tags.BloodSignalNulling:
				case Tags.SaturationRecovery:
				case Tags.SpectrallySelectedSuppression:
				case Tags.SpectrallySelectedExcitation:
				case Tags.SpatialPreSaturation:
				case Tags.Tagging:
				case Tags.OversamplingPhase:
				case Tags.GeometryOfKSpaceTraversal:
				case Tags.SegmentedKSpaceTraversal:
				case Tags.RectilinearPhaseEncodeReordering:
				case Tags.PartialFourierDirection:
				case Tags.GatingSynchronizationTechnique:
				case Tags.ReceiveCoilType: 
				case Tags.QuadratureReceiveCoil:
				case Tags.MultiCoilElementUsed:
				case Tags.TransmitCoilType:
				case Tags.VolumeLocalizationTechnique: 
				case Tags.DeCoupling:
				case Tags.DeCoupledNucleus:
				case Tags.DeCouplingMethod:
				case Tags.KSpaceFiltering:
				case Tags.TimeDomainFiltering:
				case Tags.BaselineCorrection:
				case Tags.DiffusionDirectionality:
				case Tags.ParallelAcquisition:
				case Tags.ParallelAcquisitionTechnique:
				case Tags.PartialFourier:
				case Tags.CardiacSignalSource:
				case Tags.CoverageOfKSpace:
				case Tags.ResonantNucleus:
				case Tags.FrequencyCorrection:
				case Tags.DiffusionAnisotropyType:
				case Tags.BulkMotionStatus:
				case Tags.CardiacBeatRejectionTechnique:
				case Tags.RespiratoryMotionCompensation:
				case Tags.RespiratorySignalSource:
				case Tags.BulkMotionCompensationTechnique:
				case Tags.BulkMotionSignal:
				case Tags.ApplicableSafetyStandardAgency:
				case Tags.OperatingModeType:
				case Tags.OperationMode:
				case Tags.SpecificAbsorptionRateDefInition:
				case Tags.GradientOutputType:
				case Tags.FlowCompensationDirection:
				case Tags.FirstOrderPhaseCorrection: 
				case Tags.WaterReferencedPhaseCorrection:
				case Tags.MRSpectroscopyAcquisitionType: 
				case Tags.RespiratoryMotionStatus: 
				case Tags.CardiacMotionStatus:
				case Tags.PatientOrientation:
				case Tags.Laterality:
				case Tags.ImageLaterality:
				case Tags.ImageGeometryTypeRetired:
				case Tags.MaskingImageRetired:
				case Tags.FrameLaterality:
				case Tags.PhotometricInterpretation:
				case Tags.CorrectedImage:
				case Tags.CompressionCodeRetired:
				case Tags.QualityControlImage:
				case Tags.BurnedInAnnotation:
				case Tags.PixelIntensityRelationship:
				case Tags.RecommendedViewingMode:
				case Tags.ImplantPresent:
				case Tags.PartialView:
				case Tags.LossyImageCompression:
				case Tags.MaskOperation:
				case Tags.SignalDomain:
				case Tags.DataRepresentation:
				case Tags.SignalDomainRows:
				case Tags.StudyStatusID:
				case Tags.StudyPriorityID:
				case Tags.StudyComponentStatusID:
				case Tags.VisitStatusID:
				case Tags.WaveformOriginality:
				case Tags.ChannelStatus:
				case Tags.SPSStatus:
				case Tags.PPSStatus:
				case Tags.OrganExposed:
				case Tags.RelationshipType: 
				case Tags.ValueType:
				case Tags.ContinuityOfContent:
				case Tags.TemporalRangeType: 
				case Tags.CompletionFlag:
				case Tags.VerificationFlag:
				case Tags.TemplateIdentifier: 
				case Tags.TemplateExtensionFlag:
				case Tags.CalibrationImage:
				case Tags.DeviceDiameterUnits:
				case Tags.TypeOfDetectorMotion:
				case Tags.SeriesType:
				case Tags.Units:
				case Tags.CountsSource:
				case Tags.ReprojectionMethod:
				case Tags.RandomsCorrectionMethod:
				case Tags.DecayCorrection:
				case Tags.SecondaryCountsType:
				case Tags.CountsIncluded:
				case Tags.DeadTimeCorrectionFlag:
				case Tags.GraphicLayer:
				case Tags.BoundingBoxAnnotationUnits:
				case Tags.AnchorPointAnnotationUnits:
				case Tags.GraphicAnnotationUnits:
				case Tags.BoundingBoxTextHorizontalJustification:
				case Tags.AnchorPointVisibility:
				case Tags.GraphicType:
				case Tags.GraphicFilled:
				case Tags.ImageHorizontalFlip:
				case Tags.PresentationLabel:
				case Tags.PresentationSizeMode:
				case Tags.SOPInstanceStatus:
				case Tags.PrintPriority:
				case Tags.MediumType:
				case Tags.FilmDestination:
				case Tags.ColorImagePrintingFlag:
				case Tags.CollationFlag:
				case Tags.AnnotationFlag: 
				case Tags.ImageOverlayFlag:
				case Tags.PresentationLUTFlag: 
				case Tags.ImageBoxPresentationLUTFlag: 
				case Tags.AnnotationDisplayFormatID:
				case Tags.FilmOrientation:
				case Tags.FilmSizeID:
				case Tags.PrinterResolutionID:
				case Tags.DefaultPrinterResolutionID:
				case Tags.MagnificationType:
				case Tags.SmoothingType:
				case Tags.DefaultMagnificationType:
				case Tags.OtherMagnificationTypesAvailable:
				case Tags.DefaultSmoothingType:
				case Tags.OtherSmoothingTypesAvailable:
				case Tags.BorderDensity:
				case Tags.EmptyImageDensity:
				case Tags.Trim:
				case Tags.Polarity:
				case Tags.RequestedDecimateCropBehavior:
				case Tags.RequestedResolutionID:
				case Tags.RequestedImageSizeFlag:
				case Tags.DecimateCropResult:
				case Tags.OverlayMagnificationType:
				case Tags.OverlaySmoothingType:
				case Tags.OverlayOrImageMagnification:
				case Tags.OverlayForegroundDensity:
				case Tags.OverlayBackgroundDensity:
				case Tags.OverlayModeRetired:
				case Tags.ThresholdDensityRetired:
				case Tags.PresentationLUTShape:
				case Tags.ExecutionStatus: 
				case Tags.ExecutionStatusInfo:
				case Tags.PrinterStatus: 
				case Tags.PrinterStatusInfo:
				case Tags.QueueStatus: 
				case Tags.ReportedValuesOrigin: 
				case Tags.RTImagePlane: 
				case Tags.DVHType: 
				case Tags.DoseUnits: 
				case Tags.DoseType: 
				case Tags.DoseSummationType: 
				case Tags.DVHVolumeUnits: 
				case Tags.DVHROIContributionType: 
				case Tags.RTROIRelationship: 
				case Tags.ROIGenerationAlgorithm: 
				case Tags.ContourGeometricType: 
				case Tags.RTROIInterpretedType: 
				case Tags.ROIPhysicalProperty: 
				case Tags.FrameOfReferenceTransformationType: 
				case Tags.MeasuredDoseType: 
				case Tags.TreatmentTerminationStatus: 
				case Tags.TreatmentVerificationStatus: 
				case Tags.ApplicationSetupCheck: 
				case Tags.CurrentTreatmentStatus: 
				case Tags.FractionGroupType: 
				case Tags.BeamStopperPosition: 
				case Tags.TreatmentIntent: 
				case Tags.RTPlanGeometry: 
				case Tags.DoseReferenceStructureType: 
				case Tags.NominalBeamEnergyUnit: 
				case Tags.DoseReferenceType: 
				case Tags.RTPlanRelationship: 
				case Tags.PrimaryDosimeterUnit: 
				case Tags.RTBeamLimitingDeviceType: 
				case Tags.BeamType: 
				case Tags.RadiationType: 
				case Tags.TreatmentDeliveryType: 
				case Tags.WedgeType: 
				case Tags.CompensatorType: 
				case Tags.BlockType: 
				case Tags.BlockDivergence: 
				case Tags.ApplicatorType: 
				case Tags.WedgePosition: 
				case Tags.GantryRotationDirection: 
				case Tags.BeamLimitingDeviceRotationDirection: 
				case Tags.PatientSupportRotationDirection: 
				case Tags.TableTopEccentricRotationDirection: 
				case Tags.FixationDeviceType: 
				case Tags.ShieldingDeviceType: 
				case Tags.SetupTechnique: 
				case Tags.SetupDeviceType: 
				case Tags.BrachyTreatmentTechnique: 
				case Tags.BrachyTreatmentType: 
				case Tags.SourceType: 
				case Tags.ApplicationSetupType: 
				case Tags.BrachyAccessoryDeviceType: 
				case Tags.SourceMovementType: 
				case Tags.SourceApplicatorType: 
				case Tags.ApprovalStatus: 
				case Tags.InterpretationTypeID: 
				case Tags.InterpretationStatusID: 
				case Tags.WaveformSampleInterpretation:
                case Tags.CertificateType:
                case Tags.CertifiedTimestampType:
                case Tags.ReasonfortheAttributeModification:
					return VRs.CS;
				
				case Tags.InstanceCreationDate:
				case Tags.StudyDate:
				case Tags.SeriesDate:
				case Tags.AcquisitionDate:
				case Tags.ContentDate:
				case Tags.OverlayDate:
				case Tags.CurveDate:
				case Tags.PatientBirthDate:
				case Tags.LastMenstrualDate:
				case Tags.DateOfSecondaryCapture:
				case Tags.DateOfLastCalibration:
				case Tags.DateOfLastDetectorCalibration:
				case Tags.ModifiedImageDateRetired:
				case Tags.StudyVerifiedDate:
				case Tags.StudyReadDate:
				case Tags.ScheduledStudyStartDate:
				case Tags.ScheduledStudyStopDate:
				case Tags.StudyArrivalDate:
				case Tags.StudyCompletionDate:
				case Tags.ScheduledAdmissionDate:
				case Tags.ScheduledDischargeDate:
				case Tags.AdmittingDate:
				case Tags.DischargeDate:
				case Tags.SPSStartDate:
				case Tags.SPSEndDate:
				case Tags.PPSStartDate:
				case Tags.PPSEndDate:
				case Tags.IssueDateOfImagingServiceRequest:
				case Tags.Date:
				case Tags.PresentationCreationDate:
				case Tags.CreationDate:
				case Tags.StructureSetDate:
				case Tags.TreatmentControlPointDate: 
				case Tags.FirstTreatmentDate:
				case Tags.MostRecentTreatmentDate:
				case Tags.SafePositionExitDate:
				case Tags.SafePositionReturnDate:
				case Tags.TreatmentDate:
				case Tags.RTPlanDate:
				case Tags.AirKermaRateReferenceDate:
				case Tags.ReviewDate:
				case Tags.InterpretationRecordedDate:
				case Tags.InterpretationTranscriptionDate:
				case Tags.InterpretationApprovalDate: 
					return VRs.DA;
				
				case Tags.EventElapsedTime:
				case Tags.PatientSize:
				case Tags.PatientWeight:
				case Tags.InterventionDrugDose:
				case Tags.EnergyWindowCenterlineRetired:
				case Tags.EnergyWindowTotalWidthRetired:
				case Tags.SliceThickness:
				case Tags.KVP:
				case Tags.EffectiveSeriesDuration:
				case Tags.RepetitionTime:
				case Tags.EchoTime:
				case Tags.InversionTime:
				case Tags.NumberOfAverages:
				case Tags.ImagingFrequency:
				case Tags.MagneticFieldStrength:
				case Tags.SpacingBetweenSlices:
				case Tags.DataCollectionDiameter:
				case Tags.PercentSampling:
				case Tags.PercentPhaseFieldOfView:
				case Tags.PixelBandwidth:
				case Tags.ContrastBolusVolume:
				case Tags.ContrastBolusTotalDose:
				case Tags.ContrastFlowRate:
				case Tags.ContrastFlowDuration:
				case Tags.ContrastBolusIngredientConcentration:
				case Tags.SpatialResolution:
				case Tags.TriggerTime:
				case Tags.FrameTime:
				case Tags.FrameTimeVector:
				case Tags.FrameDelay:
				case Tags.ImageTriggerDelay:
				case Tags.MultiplexGroupTimeOffset: 
				case Tags.TriggerTimeOffset:
				case Tags.RadiopharmaceuticalVolume:
				case Tags.RadionuclideTotalDose:
				case Tags.RadionuclideHalfLife:
				case Tags.RadionuclidePositronFraction:
				case Tags.RadiopharmaceuticalSpecificActivity:
				case Tags.ReconstructionDiameter:
				case Tags.DistanceSourceToDetector:
				case Tags.DistanceSourceToPatient:
				case Tags.EstimatedRadiographicMagnificationFactor:
				case Tags.GantryDetectorTilt:
				case Tags.GantryDetectorSlew:
				case Tags.TableHeight:
				case Tags.TableTraverse:
				case Tags.TableVerticalIncrement:
				case Tags.TableLateralIncrement:
				case Tags.TableLongitudinalIncrement:
				case Tags.TableAngle:
				case Tags.AngularPosition:
				case Tags.RadialPosition:
				case Tags.ScanArc:
				case Tags.AngularStep:
				case Tags.CenterOfRotationOffset:
				case Tags.RotationOffsetRetired:
				case Tags.AveragePulseWidth:
				case Tags.ImageAreaDoseProduct:
				case Tags.IntensifierSize:
				case Tags.ImagerPixelSpacing:
				case Tags.XFocusCenter: 
				case Tags.YFocusCenter:
				case Tags.FocalSpot:
				case Tags.BodyPartThickness: 
				case Tags.CompressionForce:
				case Tags.ScanVelocity:
				case Tags.FlipAngle:
				case Tags.SAR: 
				case Tags.dBDt:
				case Tags.TomoLayerHeight:
				case Tags.TomoAngle:
				case Tags.TomoTime:
				case Tags.PositionerPrimaryAngle:
				case Tags.PositionerSecondaryAngle:
				case Tags.PositionerPrimaryAngleIncrement:
				case Tags.PositionerSecondaryAngleIncrement:
				case Tags.DetectorPrimaryAngle:
				case Tags.DetectorSecondaryAngle:
				case Tags.FocusDepth:
				case Tags.MechanicalIndex:
				case Tags.ThermalIndex:
				case Tags.CranialThermalIndex:
				case Tags.SoftTissueThermalIndex:
				case Tags.SoftTissueFocusThermalIndex:
				case Tags.SoftTissueSurfaceThermalIndex:
				case Tags.ImageTransformationMatrix:
				case Tags.ImageTranslationVector:
				case Tags.Sensitivity:
				case Tags.DetectorTemperature:
				case Tags.DetectorTimeSinceLastExposure:
				case Tags.DetectorActiveTime:
				case Tags.DetectorActivationOffsetFromExposure:
				case Tags.DetectorBinning:
				case Tags.DetectorElementPhysicalSize:
				case Tags.DetectorElementSpacing:
				case Tags.DetectorActiveDimension:
				case Tags.DetectorActiveOrigin:
				case Tags.FieldOfViewOrigin: 
				case Tags.FieldOfViewRotation:
				case Tags.GridThickness:
				case Tags.GridPitch: 
				case Tags.GridPeriod:
				case Tags.GridFocalDistance:
				case Tags.FilterThicknessMinimum:
				case Tags.FilterThicknessMaximum:
				case Tags.PhototimerSetting:
				case Tags.ExposureTimeInuS:
				case Tags.XRayTubeCurrentInuA:
				case Tags.ImagePositionRetired:
				case Tags.ImagePosition:
				case Tags.ImageOrientationRetired:
				case Tags.ImageOrientation:
				case Tags.TemporalResolution:
				case Tags.SliceLocation:
				case Tags.PixelSpacing:
				case Tags.ZoomFactor:
				case Tags.ZoomCenter:
				case Tags.WindowCenter:
				case Tags.WindowWidth:
				case Tags.RescaleIntercept:
				case Tags.RescaleSlope:
				case Tags.LossyImageCompressionRatio:
				case Tags.SamplingFrequency:
				case Tags.ChannelSensitivity:
				case Tags.ChannelSensitivityCorrectionFactor:
				case Tags.ChannelBaseline:
				case Tags.ChannelTimeSkew:
				case Tags.ChannelSampleSkew:
				case Tags.ChannelOffset:
				case Tags.FilterLowFrequency:
				case Tags.FilterHighFrequency:
				case Tags.NotchFilterFrequency:
				case Tags.NotchFilterBandwidth:
				case Tags.Quantity:
				case Tags.DistanceSourceToEntrance: 
				case Tags.DistanceSourceToSupport:
				case Tags.XRayOutput:
				case Tags.HalfValueLayer:
				case Tags.OrganDose:
				case Tags.XOffsetInSlideCoordinateSystem:
				case Tags.YOffsetInSlideCoordinateSystem:
				case Tags.ZOffsetInSlideCoordinateSystem:
				case Tags.EntranceDoseInmGy:
				case Tags.RefTimeOffsets:
				case Tags.NumericValue:
				case Tags.DeviceLength:
				case Tags.DeviceDiameter:
				case Tags.DeviceVolume:
				case Tags.InterMarkerDistance:
				case Tags.EnergyWindowLowerLimit:
				case Tags.EnergyWindowUpperLimit:
				case Tags.TimeSlotTime:
				case Tags.StartAngle:
				case Tags.AxialAcceptance:
				case Tags.DetectorElementSize:
				case Tags.CoincidenceWindowWidth:
				case Tags.FrameReferenceTime:
				case Tags.SliceSensitivityFactor:
				case Tags.DecayFactor:
				case Tags.DoseCalibrationFactor:
				case Tags.ScatterFractionFactor:
				case Tags.DeadTimeFactor:
				case Tags.PresentationPixelSpacing:
				case Tags.PrinterPixelSpacing:
				case Tags.RequestedImageSize:
				case Tags.XRayImageReceptorTranslation: 
				case Tags.XRayImageReceptorAngle:
				case Tags.RTImageOrientation: 
				case Tags.ImagePlanePixelSpacing:
				case Tags.RTImagePosition:
				case Tags.RadiationMachineSAD:
				case Tags.RadiationMachineSSD:
				case Tags.RTImageSID:
				case Tags.SourceToReferenceObjectDistance:
				case Tags.MetersetExposure:
				case Tags.NormalizationPoint:
				case Tags.GridFrameOffsetVector:
				case Tags.DoseGridScaling:
				case Tags.DoseValue:
				case Tags.DVHNormalizationPoint:
				case Tags.DVHNormalizationDoseValue:
				case Tags.DVHDoseScaling:
				case Tags.DVHData:
				case Tags.DVHMinimumDose:
				case Tags.DVHMaximumDose:
				case Tags.DVHMeanDose:
				case Tags.ROIVolume:
				case Tags.ContourSlabThickness:
				case Tags.ContourOffsetVector:
				case Tags.ContourData:
				case Tags.ROIPhysicalPropertyValue:
				case Tags.FrameOfReferenceTransformationMatrix:
				case Tags.MeasuredDoseValue:
				case Tags.SpecifiedPrimaryMeterset:
				case Tags.SpecifiedSecondaryMeterset:
				case Tags.DeliveredPrimaryMeterset:
				case Tags.DeliveredSecondaryMeterset:
				case Tags.SpecifiedTreatmentTime: 
				case Tags.DeliveredTreatmentTime:
				case Tags.SpecifiedMeterset:
				case Tags.DeliveredMeterset: 
				case Tags.DoseRateDelivered:
				case Tags.CumulativeDoseToDoseReference:
				case Tags.CalculatedDoseReferenceDoseValue:
				case Tags.StartMeterset:
				case Tags.EndMeterset:
				case Tags.SpecifiedChannelTotalTime:
				case Tags.DeliveredChannelTotalTime:
				case Tags.SpecifiedPulseRepetitionInterval:
				case Tags.DeliveredPulseRepetitionInterval:
				case Tags.DoseReferencePointCoordinates:
				case Tags.NominalPriorDose:
				case Tags.ConstraintWeight:
				case Tags.DeliveryWarningDose:
				case Tags.DeliveryMaximumDose:
				case Tags.TargetMinimumDose:
				case Tags.TargetPrescriptionDose:
				case Tags.TargetMaximumDose:
				case Tags.TargetUnderdoseVolumeFraction:
				case Tags.OrganAtRiskFullVolumeDose:
				case Tags.OrganAtRiskLimitDose:
				case Tags.OrganAtRiskMaximumDose:
				case Tags.OrganAtRiskOverdoseVolumeFraction:
				case Tags.GantryAngleTolerance:
				case Tags.BeamLimitingDeviceAngleTolerance:
				case Tags.BeamLimitingDevicePositionTolerance:
				case Tags.PatientSupportAngleTolerance:
				case Tags.TableTopEccentricAngleTolerance: 
				case Tags.TableTopVerticalPositionTolerance:
				case Tags.TableTopLongitudinalPositionTolerance:
				case Tags.TableTopLateralPositionTolerance:
				case Tags.BeamDoseSpecificationPoint:
				case Tags.BeamDose:
				case Tags.BeamMeterset:
				case Tags.BrachyApplicationSetupDoseSpecificationPoint:
				case Tags.BrachyApplicationSetupDose:
				case Tags.SourceAxisDistance:
				case Tags.SourceToBeamLimitingDeviceDistance:
				case Tags.LeafPositionBoundaries:
				case Tags.WedgeFactor:
				case Tags.WedgeOrientation:
				case Tags.SourceToWedgeTrayDistance:
				case Tags.TotalCompensatorTrayFactor:
				case Tags.SourceToCompensatorTrayDistance:
				case Tags.CompensatorPixelSpacing:
				case Tags.CompensatorPosition:
				case Tags.CompensatorTransmissionData:
				case Tags.CompensatorThicknessData:
				case Tags.TotalBlockTrayFactor:
				case Tags.SourceToBlockTrayDistance:
				case Tags.BlockThickness:
				case Tags.BlockTransmission:
				case Tags.BlockData:
				case Tags.CumulativeDoseReferenceCoefficient: 
				case Tags.FinalCumulativeMetersetWeight:
				case Tags.NominalBeamEnergy:
				case Tags.DoseRateSet:
				case Tags.LeafJawPositions: 
				case Tags.GantryAngle:
				case Tags.BeamLimitingDeviceAngle:
				case Tags.PatientSupportAngle:
				case Tags.TableTopEccentricAxisDistance:
				case Tags.TableTopEccentricAngle:
				case Tags.TableTopVerticalPosition:
				case Tags.TableTopLongitudinalPosition:
				case Tags.TableTopLateralPosition:
				case Tags.IsocenterPosition:
				case Tags.SurfaceEntryPoint:
				case Tags.SourceToSurfaceDistance:
				case Tags.CumulativeMetersetWeight:
				case Tags.SetupDeviceParameter:
				case Tags.TableTopVerticalSetupDisplacement:
				case Tags.TableTopLongitudinalSetupDisplacement:
				case Tags.TableTopLateralSetupDisplacement:
				case Tags.ActiveSourceDiameter:
				case Tags.ActiveSourceLength:
				case Tags.SourceEncapsulationNominalThickness:
				case Tags.SourceEncapsulationNominalTransmission:
				case Tags.SourceIsotopeHalfLife:
				case Tags.ReferenceAirKermaRate:
				case Tags.TotalReferenceAirKerma:
				case Tags.BrachyAccessoryDeviceNominalThickness:
				case Tags.BrachyAccessoryDeviceNominalTransmission:
				case Tags.ChannelLength: 
				case Tags.ChannelTotalTime:
				case Tags.PulseRepetitionInterval:
				case Tags.SourceApplicatorLength:
				case Tags.SourceApplicatorWallNominalThickness:
				case Tags.SourceApplicatorWallNominalTransmission:
				case Tags.SourceApplicatorStepSize:
				case Tags.TransferTubeLength:
				case Tags.ChannelShieldNominalThickness:
				case Tags.ChannelShieldNominalTransmission:
				case Tags.FinalCumulativeTimeWeight:
				case Tags.ControlPointRelativePosition:
				case Tags.ControlPoint3DPosition:
				case Tags.CumulativeTimeWeight:
				case Tags.StartCumulativeMetersetWeight:
				case Tags.EndCumulativeMetersetWeight: 
					return VRs.DS;
				
				case Tags.AcquisitionDatetime:
				case Tags.ContextGroupVersion:
				case Tags.ContextGroupLocalVersion:
				case Tags.FrameAcquisitionDatetime:
				case Tags.FrameReferenceDatetime:
				case Tags.VerificationDateTime:
				case Tags.ObservationDateTime:
				case Tags.DateTime:
				case Tags.RefDatetime:
				case Tags.TemplateVersion:
				case Tags.TemplateLocalVersion:
				case Tags.SOPAuthorizationDateAndTime:
                case Tags.ContributionDateTime:
                case Tags.DigitalSignatureDateTime:
                case Tags.AttributeModificationDatetime:
					return VRs.DT;
				
				case Tags.ReferencePixelPhysicalValueX:
				case Tags.ReferencePixelPhysicalValueY:
				case Tags.PhysicalDeltaX:
				case Tags.PhysicalDeltaY:
				case Tags.DopplerCorrectionAngle:
				case Tags.SteeringAngle:
				case Tags.TableOfYBreakPoints:
				case Tags.TagAngleFirstAxis:
				case Tags.TagSpacingFirstDimension:
				case Tags.TagThickness:
				case Tags.SpectralWidth:
				case Tags.ChemicalShiftReference:
				case Tags.DeCouplingFrequency:
				case Tags.DeCouplingChemicalShiftReference:
				case Tags.CardiacRRIntervalSpecified:
				case Tags.AcquisitionDuration:
				case Tags.InversionTimes:
				case Tags.EffectiveEchoTime:
				case Tags.DiffusionBValue:
				case Tags.DiffusionGradientOrientation:
				case Tags.VelocityEncodingDirection:
				case Tags.VelocityEncodingMinimumValue:
				case Tags.ParallelReductionFactorInPlane:
				case Tags.TransmitterFrequency:
				case Tags.SlabThickness:
				case Tags.SlabOrientation:
				case Tags.MidSlabPosition:
				case Tags.ParallelReductionFactorOutOfPlane:
				case Tags.ParallelReductionFactorSecondInPlane: 
				case Tags.SpecificAbsorptionRateValue:
				case Tags.GradientOutput:
				case Tags.TaggingDelay:
				case Tags.ChemicalShiftsMinimumIntegrationLimit:
				case Tags.ChemicalShiftsMaximumIntegrationLimit:
				case Tags.VelocityEncodingMaximumValue:
				case Tags.FrameAcquisitionDuration:
				case Tags.TriggerDelayTime:
				case Tags.RealWorldValueLUTData:
				case Tags.RealWorldValueIntercept:
				case Tags.RealWorldValueSlope: 
					return VRs.FD;
				
				case Tags.TableOfParameterValues:
				case Tags.MaskSubPixelShift:
				case Tags.BoundingBoxTopLeftHandCorner:
				case Tags.BoundingBoxBottomRightHandCorner:
				case Tags.AnchorPoint:
				case Tags.GraphicData:
				case Tags.PresentationPixelMagnificationRatio: 
					return VRs.FL;
				
				case Tags.RefFrameNumber:
				case Tags.StageNumber:
				case Tags.NumberOfStages:
				case Tags.ViewNumber:
				case Tags.NumberOfEventTimers:
				case Tags.NumberOfViewsInStage:
				case Tags.StartTrim:
				case Tags.StopTrim:
				case Tags.RecommendedDisplayFrameRate:
				case Tags.CineRate:
				case Tags.CountsAccumulated:
				case Tags.AcquisitionStartConditionData:
				case Tags.AcquisitionTerminationConditionData:
				case Tags.EchoNumber:
				case Tags.NumberOfPhaseEncodingSteps:
				case Tags.EchoTrainLength:
				case Tags.SyringeCounts:
				case Tags.NominalInterval:
				case Tags.LowRRValue:
				case Tags.HighRRValue:
				case Tags.IntervalsAcquired:
				case Tags.IntervalsRejected:
				case Tags.SkipBeats:
				case Tags.HeartRate:
				case Tags.CardiacNumberOfImages:
				case Tags.TriggerWindow:
				case Tags.FieldOfViewDimension:
				case Tags.ExposureTime:
				case Tags.XRayTubeCurrent:
				case Tags.Exposure:
				case Tags.ExposureInuAs:
				case Tags.GeneratorPower:
				case Tags.FocalDistance: 
				case Tags.ActualFrameDuration:
				case Tags.CountRate: 
				case Tags.ScanLength:
				case Tags.RelativeXRayExposure:
				case Tags.NumberofTomosynthesisSourceImages:
				case Tags.ShutterLeftVerticalEdge:
				case Tags.ShutterRightVerticalEdge:
				case Tags.ShutterUpperHorizontalEdge:
				case Tags.ShutterLowerHorizontalEdge:
				case Tags.CenterOfCircularShutter:
				case Tags.RadiusOfCircularShutter:
				case Tags.VerticesOfPolygonalShutter:
				case Tags.CollimatorLeftVerticalEdge:
				case Tags.CollimatorRightVerticalEdge:
				case Tags.CollimatorUpperHorizontalEdge:
				case Tags.CollimatorLowerHorizontalEdge:
				case Tags.CenterOfCircularCollimator:
				case Tags.RadiusOfCircularCollimator:
				case Tags.VerticesOfPolygonalCollimator:
				case Tags.DepthOfScanField:
				case Tags.ExposuresOnDetectorSinceLastCalibration:
				case Tags.ExposuresOnDetectorSinceManufactured:
				case Tags.GridAspectRatio:
				case Tags.SeriesNumber:
				case Tags.AcquisitionNumber:
				case Tags.InstanceNumber:
				case Tags.IsotopeNumberRetired:
				case Tags.PhaseNumberRetired:
				case Tags.IntervalNumberRetired: 
				case Tags.TimeSlotNumberRetired:
				case Tags.AngleNumberRetired:
				case Tags.ItemNumber:
				case Tags.OverlayNumber:
				case Tags.CurveNumber:
				case Tags.LUTNumber:
				case Tags.TemporalPositionIdentifier:
				case Tags.NumberOfTemporalPositions:
				case Tags.SeriesInStudy:
				case Tags.AcquisitionsInSeriesRetired:
				case Tags.ImagesInAcquisition:
				case Tags.AcquisitionsInStudy:
				case Tags.OtherStudyNumbers:
				case Tags.NumberOfPatientRelatedStudies:
				case Tags.NumberOfPatientRelatedSeries:
				case Tags.NumberOfPatientRelatedInstances:
				case Tags.NumberOfStudyRelatedSeries:
				case Tags.NumberOfStudyRelatedInstances:
				case Tags.NumberOfSeriesRelatedInstances:
				case Tags.NumberOfFrames:
				case Tags.PixelAspectRatio:
				case Tags.WaveformChannelNumber:
				case Tags.ResidualSyringeCounts:
				case Tags.PhaseDelay:
				case Tags.PauseBetweenFrames:
				case Tags.TriggerVector:
				case Tags.AxialMash:
				case Tags.TransverseMash:
				case Tags.PrimaryCountsAccumulated:
				case Tags.SecondaryCountsAccumulated:
				case Tags.GraphicLayerOrder:
				case Tags.PresentationPixelAspectRatio:
				case Tags.NumberOfCopies: 
				case Tags.MemoryAllocation:
				case Tags.MaximumMemoryAllocation:
				case Tags.MaximumCollatedFilms:
				case Tags.NumberOfFilms:
				case Tags.FractionNumber:
				case Tags.DVHNumberOfBins:
				case Tags.ROINumber:
				case Tags.ROIDisplayColor:
				case Tags.NumberOfContourPoints:
				case Tags.ContourNumber:
				case Tags.AttachedContours:
				case Tags.ObservationNumber:
				case Tags.RefROINumber:
				case Tags.CurrentFractionNumber:
				case Tags.NumberOfFractionsDelivered:
				case Tags.MeasuredDoseReferenceNumber:
				case Tags.CalculatedDoseReferenceNumber:
				case Tags.RefMeasuredDoseReferenceNumber:
				case Tags.RefCalculatedDoseReferenceNumber:
				case Tags.RefBrachyAccessoryDeviceNumber:
				case Tags.SpecifiedNumberOfPulses:
				case Tags.DeliveredNumberOfPulses:
				case Tags.RefSourceApplicatorNumber:
				case Tags.RefChannelShieldNumber:
				case Tags.RefFractionNumber:
				case Tags.DoseReferenceNumber:
				case Tags.ToleranceTableNumber:
				case Tags.FractionGroupNumber:
				case Tags.NumberOfFractionsPlanned:
				case Tags.NumberOfFractionsPerDay: 
				case Tags.RepeatFractionCycleLength:
				case Tags.NumberOfBeams:
				case Tags.NumberOfBrachyApplicationSetups:
				case Tags.NumberOfLeafJawPairs:
				case Tags.BeamNumber:
				case Tags.ReferenceImageNumber:
				case Tags.NumberOfWedges:
				case Tags.WedgeNumber:
				case Tags.WedgeAngle:
				case Tags.NumberOfCompensators:
				case Tags.CompensatorNumber:
				case Tags.CompensatorRows:
				case Tags.CompensatorColumns:
				case Tags.NumberOfBoli:
				case Tags.NumberOfBlocks:
				case Tags.BlockNumber:
				case Tags.BlockNumberOfPoints:
				case Tags.NumberOfControlPoints:
				case Tags.ControlPointIndex:
				case Tags.PatientSetupNumber:
				case Tags.SourceNumber:
				case Tags.ApplicationSetupNumber:
				case Tags.TemplateNumber:
				case Tags.BrachyAccessoryDeviceNumber:
				case Tags.ChannelNumber:
				case Tags.NumberOfPulses:
				case Tags.SourceApplicatorNumber:
				case Tags.TransferTubeNumber:
				case Tags.ChannelShieldNumber:
				case Tags.RefBeamNumber:
				case Tags.RefReferenceImageNumber:
				case Tags.RefBrachyApplicationSetupNumber:
				case Tags.RefSourceNumber:
				case Tags.RefFractionGroupNumber:
				case Tags.RefDoseReferenceNumber:
				case Tags.RefPatientSetupNumber: 
				case Tags.RefToleranceTableNumber:
				case Tags.RefWedgeNumber:
				case Tags.RefCompensatorNumber:
				case Tags.RefBlockNumber:
				case Tags.RefControlPointIndex: 
					return VRs.IS;
				
				case Tags.Manufacturer:
				case Tags.InstitutionName:
				case Tags.CodeMeaning:
				case Tags.StudyDescription:
				case Tags.SeriesDescription:
				case Tags.InstitutionalDepartmentName:
				case Tags.AdmittingDiagnosisDescription:
				case Tags.ManufacturerModelName:
				case Tags.EventTimerName:
				case Tags.PatientID:
				case Tags.IssuerOfPatientID:
				case Tags.OtherPatientIDs:
				case Tags.PatientAddress:
				case Tags.InsurancePlanIdentificationRetired:
				case Tags.MilitaryRank:
				case Tags.BranchOfService:
				case Tags.MedicalRecordLocator:
				case Tags.MedicalAlerts:
				case Tags.ContrastAllergies:
				case Tags.CountryOfResidence:
				case Tags.RegionOfResidence:
				case Tags.PatientReligiousPreference:
				case Tags.ContrastBolusAgent:
				case Tags.RadionuclideRetired:
				case Tags.Radiopharmaceutical:
				case Tags.InterventionDrugName:
				case Tags.DeviceSerialNumber:
				case Tags.PlateID:
				case Tags.SecondaryCaptureDeviceID:
				case Tags.HardcopyCreationDeviceID:
				case Tags.SecondaryCaptureDeviceManufacturer: 
				case Tags.HardcopyDeviceManufacturer:
				case Tags.SecondaryCaptureDeviceManufacturerModelName:
				case Tags.SecondaryCaptureDeviceSoftwareVersion:
				case Tags.HardcopyDeviceSoftwareVersion:
				case Tags.HardcopyDeviceManfuacturerModelName:
				case Tags.SoftwareVersion:
				case Tags.DigitalImageFormatAcquired:
				case Tags.ProtocolName:
				case Tags.ContrastBolusRoute:
				case Tags.TriggerSourceOrType:
				case Tags.FramingType:
				case Tags.RadiopharmaceuticalRoute:
				case Tags.PVCRejection:
				case Tags.TypeOfFilters:
				case Tags.PhosphorType:
				case Tags.AcquisitionDeviceProcessingDescription:
				case Tags.AcquisitionDeviceProcessingCode:
				case Tags.TransducerData:
				case Tags.ProcessingFunction:
				case Tags.PostprocessingFunction:
				case Tags.ReceiveCoilManufacturerName:
				case Tags.MultiCoilConfiguration:
				case Tags.TransmitCoilManufacturerName:
				case Tags.ApplicableSafetyStandardVersion:
				case Tags.PositionReferenceIndicator:
				case Tags.ModifyingDeviceManufacturerRetired:
				case Tags.ModifiedImageDescriptionRetired: 
				case Tags.OriginalImageIdentificationRetired:
				case Tags.OriginalImageIdentificationNomenclatureRetired: 
				case Tags.DimensionIndexPrivateCreator:
				case Tags.FunctionalGroupPrivateCreator:
				case Tags.RescaleType:
				case Tags.WindowCenterWidthExplanation:
				case Tags.LUTExplanation:
				case Tags.ModalityLUTType:
				case Tags.FrameOfInterestDescription:
				case Tags.StudyIDIssuer:
				case Tags.ScheduledStudyLocation:
				case Tags.ReasonforStudy:
				case Tags.RequestingService:
				case Tags.RequestedProcedureDescription:
				case Tags.RequestedContrastAgent:
				case Tags.AdmissionID:
				case Tags.IssuerOfAdmissionID:
				case Tags.RouteOfAdmissions:
				case Tags.ScheduledPatientInstitutionResidence:
				case Tags.DischargeDiagnosisDescription:
				case Tags.SpecialNeeds:
				case Tags.CurrentPatientLocation:
				case Tags.PatientInstitutionResidence:
				case Tags.PatientState:
				case Tags.ChannelDerivationDescription:
				case Tags.SPSDescription:
				case Tags.PreMedication:
				case Tags.PPSDescription:
				case Tags.PerformedProcedureTypeDescription:
				case Tags.SpecimenAccessionNumber:
				case Tags.SpecimenIdentifier:
				case Tags.SlideIdentifier: 
				case Tags.ReasonForTheRequestedProcedure:
				case Tags.PatientTransportArrangements:
				case Tags.RequestedProcedureLocation:
				case Tags.ConfidentialityCode:
				case Tags.ReasonForTheImagingServiceRequest:
				case Tags.PlacerOrderNumber:
				case Tags.FillerOrderNumber:
				case Tags.ConfidentialityPatientData:
				case Tags.VerifyingOrganization:
				case Tags.CompletionFlagDescription:
				case Tags.DeviceDescription:
				case Tags.AttenuationCorrectionMethod:
				case Tags.ReconstructionMethod:
				case Tags.DetectorLinesOfResponseUsed:
				case Tags.ScatterCorrectionMethod:
				case Tags.HistogramExplanation:
				case Tags.GraphicLayerDescription:
				case Tags.PresentationDescription:
				case Tags.TopicTitle:
				case Tags.TopicAuthor:
				case Tags.TopicKeyWords:
				case Tags.AuthorizationEquipmentCertificationNumber:
				case Tags.FilmSessionLabel:
				case Tags.TextString:
				case Tags.PrinterName:
				case Tags.RTImageName:
				case Tags.DoseComment:
				case Tags.StructureSetName:
				case Tags.ROIName: 
				case Tags.ROIGenerationDescription:
				case Tags.FrameOfReferenceTransformationComment:
				case Tags.SourceSerialNumber: 
				case Tags.RTPlanName:
				case Tags.TreatmentProtocols:
				case Tags.TreatmentSites:
				case Tags.DoseReferenceDescription:
				case Tags.BeamName:
				case Tags.ImagingDeviceSpecificAcquisitionParameters:
				case Tags.BlockName:
				case Tags.ApplicatorDescription:
				case Tags.PatientAdditionalPosition:
				case Tags.SourceManufacturer:
				case Tags.SourceIsotopeName:
				case Tags.ApplicationSetupName:
				case Tags.ApplicationSetupManufacturer:
				case Tags.TemplateName:
				case Tags.BrachyAccessoryDeviceName:
				case Tags.SourceApplicatorName:
				case Tags.SourceApplicatorManufacturer:
				case Tags.ChannelShieldName:
				case Tags.ResultsIDIssuer:
				case Tags.ReferenceToRecordedSound:
				case Tags.DistributionAddress:
                case Tags.InterpretationIDIssuer:
                case Tags.ModifyingSystem:
                case Tags.SourceofPreviousValues:
					return VRs.LO;
				
				case Tags.StudyCommentsRetired:
				case Tags.AdditionalPatientHistory:
				case Tags.PatientComments:
				case Tags.SeriesCommentsRetired:
				case Tags.DetectorDescription:
				case Tags.DetectorMode:
				case Tags.GridAbsorbingMaterial:
				case Tags.GridSpacingMaterial:
				case Tags.ExposureControlModeDescription:
				case Tags.ImageComments:
				case Tags.FrameComments:
				case Tags.PixelCommentsRetired:
				case Tags.StudyComments:
				case Tags.VisitComments:
				case Tags.SPSComments:
				case Tags.RequestedProcedureComments:
				case Tags.ImagingServiceRequestComments:
				case Tags.SOPAuthorizationComment:
				case Tags.ConfigurationInformationDescription:
				case Tags.FractionPattern:
				case Tags.ArbitraryRetired:
				case Tags.ArbitraryCommentsRetired:
				case Tags.InterpretationDiagnosisDescription: 
					return VRs.LT;
				
				case Tags.DataSetTrailingPadding:
                case Tags.CertificateOfSigner:
                case Tags.Signature:
                case Tags.CertifiedTimestamp:
                case Tags.MAC:
                case Tags.EncryptedContent:
					return VRs.OB;
								
				case Tags.RedPaletteColorLUTData:
				case Tags.GreenPaletteColorLUTData:
				case Tags.BluePaletteColorLUTData:
				case Tags.SegmentedRedPaletteColorLUTData:
				case Tags.SegmentedGreenPaletteColorLUTData:
				case Tags.SegmentedBluePaletteColorLUTData:
				case Tags.LUTData:
				case Tags.ChannelMinimumValue:
				case Tags.ChannelMaximumValue:
				case Tags.WaveformPaddingValue:
				case Tags.WaveformData:
				case Tags.PixelData: 
					return VRs.OW;
				
				case Tags.ReferringPhysicianName:
				case Tags.PhysicianOfRecord:
				case Tags.PerformingPhysicianName:
				case Tags.NameOfPhysicianReadingStudy:
				case Tags.OperatorName:
				case Tags.PatientName:
				case Tags.OtherPatientNames:
				case Tags.PatientBirthName:
				case Tags.PatientMotherBirthName:
				case Tags.RequestingPhysician:
				case Tags.ScheduledPerformingPhysicianName:
				case Tags.NamesOfIntendedRecipientsOfResults:
				case Tags.OrderEnteredBy:
				case Tags.VerifyingObserverName:
				case Tags.PersonName:
				case Tags.PresentationCreatorName:
				case Tags.ROIInterpreter:
				case Tags.ReviewerName:
				case Tags.InterpretationRecorder:
				case Tags.InterpretationTranscriber:
				case Tags.InterpretationAuthor:
				case Tags.PhysicianApprovingInterpretation:
				case Tags.DistributionName: 
					return VRs.PN;
				
				case Tags.AccessionNumber:
				case Tags.ReferringPhysicianPhoneNumbers:
				case Tags.CodeValue:
				case Tags.CodingSchemeDesignator:
				case Tags.CodingSchemeVersion:
				case Tags.TimezoneOffsetFromUTC:
				case Tags.StationName:
				case Tags.StageName:
				case Tags.PatientPhoneNumbers:
				case Tags.EthnicGroup:
				case Tags.Occupation:
				case Tags.SequenceName:
				case Tags.ImagedNucleus:
				case Tags.VideoImageFormatAcquired:
				case Tags.FilterType:
				case Tags.CollimatorGridName:
				case Tags.ConvolutionKernel:
				case Tags.ReceiveCoilName:
				case Tags.TransmitCoilName:
				case Tags.PlateType:
				case Tags.TimeSource:
				case Tags.OutputPower:
				case Tags.DetectorID:
				case Tags.PulseSequenceName:
				case Tags.MultiCoilElementName:
				case Tags.StudyID:
				case Tags.ModifyingDeviceIDRetired:
				case Tags.ModifiedImageIDRetired:
				case Tags.StackID:
				case Tags.MultiplexGroupLabel:
				case Tags.ChannelLabel:
				case Tags.SPSID:
				case Tags.ScheduledStationName:
				case Tags.SPSLocation:
				case Tags.PerformedStationName:
				case Tags.PerformedLocation:
				case Tags.PPSID:
				case Tags.RequestedProcedureID:
				case Tags.RequestedProcedurePriority:
				case Tags.PlacerOrderNumberProcedureRetired:
				case Tags.FillerOrderNumberProcedureRetired:
				case Tags.ReportingPriority:
				case Tags.PlacerOrderNumberImagingServiceRequestRetired:
				case Tags.FillerOrderNumberImagingServiceRequestRetired:
				case Tags.OrderEntererLocation:
				case Tags.OrderCallbackPhoneNumber:
				case Tags.EnergyWindowName:
				case Tags.ImageID:
				case Tags.StorageMediaFileSetID:
				case Tags.PrintJobID:
				case Tags.OwnerID:
				case Tags.PrintQueueID:
				case Tags.RTImageLabel:
				case Tags.RadiationMachineName:
				case Tags.StructureSetLabel:
				case Tags.ROIObservationLabel:
				case Tags.TreatmentTerminationCode:
				case Tags.RTPlanLabel:
				case Tags.ToleranceTableLabel:
				case Tags.TreatmentMachineName:
				case Tags.WedgeID:
				case Tags.MaterialID:
				case Tags.CompensatorID:
				case Tags.BlockTrayID:
				case Tags.ApplicatorID:
				case Tags.FixationDeviceLabel:
				case Tags.FixationDevicePosition:
				case Tags.ShieldingDeviceLabel:
				case Tags.ShieldingDevicePosition:
				case Tags.SetupDeviceLabel:
				case Tags.TemplateType:
				case Tags.BrachyAccessoryDeviceID:
				case Tags.SourceApplicatorID:
				case Tags.ChannelShieldID:
				case Tags.ResultsID:
				case Tags.InterpretationID: 
					return VRs.SH;
				
				case Tags.ReferencePixelX0:
				case Tags.ReferencePixelY0:
				case Tags.DisplayedAreaTopLeftHandCorner:
				case Tags.DisplayedAreaBottomRightHandCorner: 
					return VRs.SL;
				
				case Tags.InstitutionCodeSeq:
				case Tags.ProcedureCodeSeq:
				case Tags.AdmittingDiagnosisCodeSeq:
                case Tags.ReferringPhysicianIdentificationSequence:
				case Tags.RefResultsSeq:
				case Tags.RefStudySeq:
				case Tags.RefStudyComponentSeq:
				case Tags.RefSeriesSeq:
				case Tags.RefPatientSeq:
                case Tags.PersonIdentificationCodeSeq:
                case Tags.CodingSchemeIdentificationSeq:
                case Tags.PhysiciansofRecordIdentificationSeq:
                case Tags.PerformingPhysicianIdentificationSeq:
                case Tags.PhysiciansReadingStudyIdentificationSeq:
                case Tags.ContributingEquipmentSeq:
                case Tags.PurposeofReferenceCodeSeq: 
                case Tags.OperatorIdentificationSeq:
                case Tags.ReferencedWaveformSeq:
				case Tags.RefVisitSeq:
				case Tags.RefOverlaySeq:
				case Tags.RefImageSeq:
				case Tags.RefCurveSeq:
				case Tags.RefInstanceSeq:
				case Tags.FailedSOPSeq:
				case Tags.RefSOPSeq:
				case Tags.SourceImageSeq:
				case Tags.AnatomicRegionSeq:
				case Tags.AnatomicRegionModifierSeq:
				case Tags.PrimaryAnatomicStructureSeq:
				case Tags.AnatomicStructureSpaceRegionSeq:
				case Tags.PrimaryAnatomicStructureModifierSeq:
				case Tags.TransducerPositionSeq:
				case Tags.TransducerPositionModifierSeq:
				case Tags.TransducerOrientationSeq:
				case Tags.TransducerOrientationModifierSeq:
				case Tags.RefRawDataSeq:
				case Tags.DerivationImageSeq:
				case Tags.ReferringImageEvidenceSeq:
				case Tags.SourceImageEvidenceSeq:
				case Tags.DerivationCodeSeq:
				case Tags.RefGrayscalePresentationStateSeq:
				case Tags.PatientInsurancePlanCodeSeq:
				case Tags.ContrastBolusAgentSeq:
				case Tags.ContrastBolusAdministrationRouteSeq:
				case Tags.InterventionDrugInformationSeq:
				case Tags.InterventionDrugCodeSeq:
				case Tags.AdditionalDrugSeq:
				case Tags.InterventionalTherapySeq:
				case Tags.ProjectionEponymousNameCodeSeq:
				case Tags.SeqOfUltrasoundRegions:
				case Tags.MRImagingModifierSeq:
				case Tags.MRReceiveCoilSeq:
				case Tags.MultiCoilDefInitionSeq:
				case Tags.MRTransmitCoilSeq:
				case Tags.DiffusionGradientDirectionSeq:
				case Tags.ChemicalShiftSeq:
				case Tags.MRSpectroscopyFOVGeometrySeq:
				case Tags.MRSpatialSaturationSeq:
				case Tags.MRTimingAndRelatedParametersSeq:
				case Tags.MREchoSeq:
				case Tags.MRModifierSeq:
				case Tags.MRDiffusionSeq:
				case Tags.CardiacTriggerSeq:
				case Tags.MRAveragesSeq:
				case Tags.MRFOVGeometrySeq:
				case Tags.VolumeLocalizationSeq:
				case Tags.MetaboliteMapSeq:
				case Tags.OperationModeSeq:
				case Tags.MRVelocityEncodingSeq:
				case Tags.MRImageFrameTypeSeq:
				case Tags.MRSpectroscopyFrameTypeSeq:
				case Tags.SpecificAbsorptionRateSeq:
				case Tags.FrameAnatomySeq:
				case Tags.FrameContentSeq:
				case Tags.PlanePositionSeq:
				case Tags.PlaneOrientationSeq:
				case Tags.DimensionOrganizationSeq:
				case Tags.DimensionSeq:
				case Tags.ModalityLUTSeq:
				case Tags.VOILUTSeq:
				case Tags.SoftcopyVOILUTSeq:
				case Tags.BiPlaneAcquisitionSeq:
				case Tags.MaskSubtractionSeq:
				case Tags.PixelMatrixSeq:
				case Tags.FrameVOILUTSeq:
				case Tags.PixelValueTransformationSeq:
				case Tags.RequestedProcedureCodeSeq:
				case Tags.RefPatientAliasSeq:
				case Tags.DischargeDiagnosisCodeSeq:
				case Tags.ChannelDefInitionSeq:
				case Tags.ChannelSourceSeq:
				case Tags.ChannelSourceModifiersSeq:
				case Tags.SourceWaveformSeq:
				case Tags.ChannelSensitivityUnitsSeq:
				case Tags.ScheduledProtocolCodeSeq:
				case Tags.SPSSeq:
				case Tags.RefStandaloneSOPInstanceSeq:
				case Tags.PerformedProtocolCodeSeq:
				case Tags.ScheduledStepAttributesSeq:
				case Tags.RequestAttributesSeq:
				case Tags.QuantitySeq:
				case Tags.MeasuringUnitsSeq:
				case Tags.BillingItemSeq:
				case Tags.BillingProcedureStepSeq:
				case Tags.FilmConsumptionSeq:
				case Tags.BillingSuppliesAndDevicesSeq:
				case Tags.RefProcedureStepSeq:
				case Tags.PerformedSeriesSeq:
				case Tags.SpecimenSeq:
				case Tags.SpecimenTypeCodeSeq:
				case Tags.AcquisitionContextSeq:
				case Tags.ImageCenterPointCoordinatesSeq:
				case Tags.PixelSpacingSeq:
				case Tags.CoordinateSystemAxisCodeSeq:
				case Tags.MeasurementUnitsCodeSeq:
				case Tags.ConceptNameCodeSeq:
				case Tags.VerifyingObserverSeq:
				case Tags.VerifyingObserverIdentificationCodeSeq:
				case Tags.ConceptCodeSeq:
				case Tags.ModifierCodeSeq:
				case Tags.MeasuredValueSeq:
				case Tags.PredecessorDocumentsSeq:
				case Tags.RefRequestSeq:
				case Tags.PerformedProcedureCodeSeq:
				case Tags.CurrentRequestedProcedureEvidenceSeq:
				case Tags.PertinentOtherEvidenceSeq:
				case Tags.ContentTemplateSeq:
				case Tags.IdenticalDocumentsSeq:
				case Tags.ContentSeq:
				case Tags.AnnotationSeq:
				case Tags.RealWorldValueMappingSeq:
				case Tags.DeviceSeq:
				case Tags.EnergyWindowInformationSeq:
				case Tags.EnergyWindowRangeSeq:
				case Tags.RadiopharmaceuticalInformationSeq:
				case Tags.DetectorInformationSeq:
				case Tags.PhaseInformationSeq:
				case Tags.RotationInformationSeq:
				case Tags.GatedInformationSeq:
				case Tags.DataInformationSeq:
				case Tags.TimeSlotInformationSeq:
				case Tags.ViewCodeSeq:
				case Tags.ViewModifierCodeSeq:
				case Tags.RadionuclideCodeSeq:
				case Tags.AdministrationRouteCodeSeq:
				case Tags.RadiopharmaceuticalCodeSeq:
				case Tags.CalibrationDataSeq:
				case Tags.PatientOrientationCodeSeq:
				case Tags.PatientOrientationModifierCodeSeq:
				case Tags.PatientGantryRelationshipCodeSeq:
				case Tags.HistogramSeq:
				case Tags.GraphicAnnotationSeq:
				case Tags.TextObjectSeq:
				case Tags.GraphicObjectSeq:
				case Tags.DisplayedAreaSelectionSeq:
				case Tags.GraphicLayerSeq:
				case Tags.IconImageSeq:
				case Tags.PrinterConfigurationSeq:
				case Tags.MediaInstalledSeq:
				case Tags.OtherMediaAvailableSeq:
				case Tags.SupportedImageDisplayFormatsSeq:
				case Tags.RefFilmBoxSeq:
				case Tags.RefStoredPrintSeq:
				case Tags.RefFilmSessionSeq:
				case Tags.RefImageBoxSeq:
				case Tags.RefBasicAnnotationBoxSeq:
				case Tags.BasicGrayscaleImageSeq:
				case Tags.BasicColorImageSeq:
				case Tags.RefImageOverlayBoxSeqRetired:
				case Tags.RefVOILUTBoxSeqRetired:
				case Tags.RefOverlayPlaneSeq:
				case Tags.OverlayPixelDataSeq:
				case Tags.RefImageBoxSeqRetired:
				case Tags.PresentationLUTSeq:
				case Tags.RefPresentationLUTSeq:
				case Tags.RefPrintJobSeq:
				case Tags.PrintJobDescriptionSeq:
				case Tags.RefPrintJobSeqInQueue:
				case Tags.PrintManagementCapabilitiesSeq:
				case Tags.PrinterCharacteristicsSeq:
				case Tags.FilmBoxContentSeq:
				case Tags.ImageBoxContentSeq:
				case Tags.AnnotationContentSeq:
				case Tags.ImageOverlayBoxContentSeq:
				case Tags.PresentationLUTContentSeq:
				case Tags.ProposedStudySeq:
				case Tags.OriginalImageSeq:
				case Tags.ExposureSeq:
				case Tags.RTDoseROISeq:
				case Tags.DVHSeq:
				case Tags.DVHRefROISeq:
				case Tags.RefFrameOfReferenceSeq:
				case Tags.RTRefStudySeq:
				case Tags.RTRefSeriesSeq:
				case Tags.ContourImageSeq:
				case Tags.StructureSetROISeq:
				case Tags.RTRelatedROISeq:
				case Tags.ROIContourSeq:
				case Tags.ContourSeq:
				case Tags.RTROIObservationsSeq:
				case Tags.RTROIIdentificationCodeSeq:
				case Tags.RelatedRTROIObservationsSeq:
				case Tags.ROIPhysicalPropertiesSeq:
				case Tags.FrameOfReferenceRelationshipSeq:
				case Tags.MeasuredDoseReferenceSeq:
				case Tags.TreatmentSessionBeamSeq:
				case Tags.RefTreatmentRecordSeq:
				case Tags.ControlPointDeliverySeq:
				case Tags.TreatmentSummaryCalculatedDoseReferenceSeq:
				case Tags.OverrideSeq:
				case Tags.CalculatedDoseReferenceSeq:
				case Tags.RefMeasuredDoseReferenceSeq:
				case Tags.RefCalculatedDoseReferenceSeq:
				case Tags.BeamLimitingDeviceLeafPairsSeq:
				case Tags.RecordedWedgeSeq:
				case Tags.RecordedCompensatorSeq:
				case Tags.RecordedBlockSeq:
				case Tags.TreatmentSummaryMeasuredDoseReferenceSeq:
				case Tags.RecordedSourceSeq:
				case Tags.TreatmentSessionApplicationSetupSeq:
				case Tags.RecordedBrachyAccessoryDeviceSeq:
				case Tags.RecordedChannelSeq:
				case Tags.RecordedSourceApplicatorSeq:
				case Tags.RecordedChannelShieldSeq:
				case Tags.BrachyControlPointDeliveredSeq:
				case Tags.FractionGroupSummarySeq:
				case Tags.FractionStatusSummarySeq:
				case Tags.DoseReferenceSeq:
				case Tags.ToleranceTableSeq:
				case Tags.BeamLimitingDeviceToleranceSeq:
				case Tags.FractionGroupSeq:
				case Tags.BeamSeq:
				case Tags.BeamLimitingDeviceSeq:
				case Tags.PlannedVerificationImageSeq:
				case Tags.WedgeSeq:
				case Tags.CompensatorSeq:
				case Tags.BlockSeq:
				case Tags.ApplicatorSeq:
				case Tags.ControlPointSeq:
				case Tags.WedgePositionSeq:
				case Tags.BeamLimitingDevicePositionSeq:
				case Tags.PatientSetupSeq:
				case Tags.FixationDeviceSeq:
				case Tags.ShieldingDeviceSeq:
				case Tags.SetupDeviceSeq:
				case Tags.TreatmentMachineSeq:
				case Tags.SourceSeq:
				case Tags.ApplicationSetupSeq:
				case Tags.BrachyAccessoryDeviceSeq:
				case Tags.ChannelSeq:
				case Tags.ChannelShieldSeq:
				case Tags.BrachyControlPointSeq:
				case Tags.RefRTPlanSeq:
				case Tags.RefBeamSeq:
				case Tags.RefBrachyApplicationSetupSeq:
				case Tags.RefFractionGroupSeq:
				case Tags.RefVerificationImageSeq:
				case Tags.RefReferenceImageSeq:
				case Tags.RefDoseReferenceSeq:
				case Tags.BrachyRefDoseReferenceSeq:
				case Tags.RefStructureSetSeq:
				case Tags.RefDoseSeq:
				case Tags.RefBolusSeq:
				case Tags.RefInterpretationSeq:
				case Tags.InterpretationApproverSeq:
				case Tags.InterpretationDiagnosisCodeSeq:
				case Tags.ResultsDistributionListSeq:
				case Tags.SharedFunctionalGroupsSeq:
				case Tags.PerFrameFunctionalGroupsSeq:
				case Tags.WaveformSeq: 
                case Tags.DigitalSignaturePurposeCodeSeq:
                case Tags.ReferencedDigitalSignatureSeq:
                case Tags.ReferencedSOPInstanceMACSeq:
                case Tags.EncryptedAttributesSeq:
                case Tags.ModifiedAttributesSeq:
                case Tags.OriginalAttributesSeq:
                case Tags.MACParametersSeq:
					return VRs.SQ;
				
				case Tags.TagSpacingSecondDimension:
				case Tags.TagAngleSecondAxis:
				case Tags.PixelIntensityRelationshipSign:
				case Tags.TIDOffset:
				case Tags.LUTLabel: 
					return VRs.SS;
				
				case Tags.InstitutionAddress:
				case Tags.ReferringPhysicianAddress:
				case Tags.DerivationDescription:
				case Tags.MetaboliteMapDescription:
				case Tags.PartialViewDescription:
				case Tags.MaskOperationExplanation:
				case Tags.PPSComments:
				case Tags.CommentsOnRadiationDose:
				case Tags.AcquisitionContextDescription:
				case Tags.UnformattedTextValue:
				case Tags.TopicSubject:
				case Tags.ImageDisplayFormat:
				case Tags.ConfigurationInformation:
				case Tags.RTImageDescription:
				case Tags.StructureSetDescription:
				case Tags.ROIDescription:
				case Tags.ROIObservationDescription:
				case Tags.MeasuredDoseDescription:
				case Tags.OverrideReason:
				case Tags.CalculatedDoseReferenceDescription:
				case Tags.TreatmentStatusComment:
				case Tags.RTPlanDescription:
				case Tags.PrescriptionDescription:
				case Tags.BeamDescription:
				case Tags.FixationDeviceDescription:
				case Tags.ShieldingDeviceDescription:
				case Tags.SetupTechniqueDescription:
				case Tags.SetupDeviceDescription:
				case Tags.SetupReferenceDescription:
				case Tags.InterpretationText:
				case Tags.Impressions:
				case Tags.ResultsComments:
                case Tags.ContributionDescriptionST:
					return VRs.ST;
				
				case Tags.InstanceCreationTime:
				case Tags.StudyTime:
				case Tags.SeriesTime:
				case Tags.AcquisitionTime:
				case Tags.ContentTime:
				case Tags.OverlayTime:
				case Tags.CurveTime:
				case Tags.PatientBirthTime:
				case Tags.InterventionDrugStopTime:
				case Tags.InterventionDrugStartTime:
				case Tags.TimeOfSecondaryCapture:
				case Tags.ContrastBolusStartTime:
				case Tags.ContrastBolusStopTime:
				case Tags.RadiopharmaceuticalStartTime:
				case Tags.RadiopharmaceuticalStopTime:
				case Tags.TimeOfLastCalibration:
				case Tags.TimeOfLastDetectorCalibration:
				case Tags.ModifiedImageTimeRetired:
				case Tags.StudyVerifiedTime:
				case Tags.StudyReadTime:
				case Tags.ScheduledStudyStartTime:
				case Tags.ScheduledStudyStopTime:
				case Tags.StudyArrivalTime:
				case Tags.StudyCompletionTime:
				case Tags.ScheduledAdmissionTime:
				case Tags.ScheduledDischargeTime:
				case Tags.AdmittingTime:
				case Tags.DischargeTime:
				case Tags.SPSStartTime:
				case Tags.SPSEndTime:
				case Tags.PPSStartTime:
				case Tags.PPSEndTime:
				case Tags.IssueTimeOfImagingServiceRequest:
				case Tags.Time:
				case Tags.PresentationCreationTime:
				case Tags.CreationTime:
				case Tags.StructureSetTime:
				case Tags.TreatmentControlPointTime:
				case Tags.SafePositionExitTime:
				case Tags.SafePositionReturnTime:
				case Tags.TreatmentTime:
				case Tags.RTPlanTime:
				case Tags.AirKermaRateReferenceTime:
				case Tags.ReviewTime:
				case Tags.InterpretationRecordedTime:
				case Tags.InterpretationTranscriptionTime:
				case Tags.InterpretationApprovalTime: 
					return VRs.TM;
				
				case Tags.InstanceCreatorUID:
				case Tags.SOPClassUID:
				case Tags.SOPInstanceUID:
				case Tags.FailedSOPInstanceUIDList:
				case Tags.SOPClassesInStudy:
				case Tags.PrivateCodingSchemeCreatorUID:
				case Tags.CodeSetExtensionCreatorUID:
				case Tags.RefSOPClassUID:
				case Tags.RefSOPInstanceUID:
				case Tags.SOPClassesSupported:
				case Tags.TransactionUID:
				case Tags.CreatorVersionUID:
				case Tags.StudyInstanceUID:
				case Tags.SeriesInstanceUID:
				case Tags.FrameOfReferenceUID:
				case Tags.SynchronizationFrameOfReferenceUID:
				case Tags.ConcatenationUID:
				case Tags.DimensionOrganizationUID:
				case Tags.PaletteColorLUTUID:
				case Tags.UID:
				case Tags.TemplateExtensionOrganizationUID:
				case Tags.TemplateExtensionCreatorUID:
				case Tags.StorageMediaFileSetUID:
				case Tags.RefFrameOfReferenceUID:
				case Tags.RelatedFrameOfReferenceUID:
                case Tags.MACCalculationTransferSyntaxUID:
                case Tags.EncryptedContentTransferSyntaxUID:
                case Tags.DigitalSignatureUID:
					return VRs.UI;
				
				case Tags.TriggerSamplePosition:
				case Tags.RegionFlags:
				case Tags.RegionLocationMinX0:
				case Tags.RegionLocationMinY0:
				case Tags.RegionLocationMaxX1:
				case Tags.RegionLocationMaxY1:
				case Tags.TransducerFrequency:
				case Tags.PulseRepetitionFrequency:
				case Tags.DopplerSampleVolumeXPosition:
				case Tags.DopplerSampleVolumeYPosition:
				case Tags.TMLinePositionX0:
				case Tags.TMLinePositionY0:
				case Tags.TMLinePositionX1:
				case Tags.TMLinePositionY1:
				case Tags.PixelComponentMask:
				case Tags.PixelComponentRangeStart:
				case Tags.PixelComponentRangeStop:
				case Tags.NumberOfTableBreakPoints:
				case Tags.TableOfXBreakPoints:
				case Tags.NumberOfTableEntries:
				case Tags.TableOfPixelValues:
				case Tags.SpectroscopyAcquisitionPhaseRows:
				case Tags.SpectroscopyAcquisitionDataColumns:
				case Tags.SpectroscopyAcquisitionOutOfPlanePhaseSteps:
				case Tags.SpectroscopyAcquisitionPhaseColumns:
				case Tags.InStackPositionNumber:
				case Tags.TemporalPositionIndex:
				case Tags.DimensionIndexValues:
				case Tags.ConcatenationFrameOffsetNumber:
				case Tags.DataPointRows:
				case Tags.DataPointColumns:
				case Tags.NumberOfWaveformSamples:
				case Tags.RefSamplePositions:
				case Tags.RefContentItemIdentifier:
				case Tags.HistogramData: 
					return VRs.UL;
				
				case Tags.LengthToEndRetired:
				case Tags.RecognitionCodeRetired:
				case Tags.DataSetTypeRetired:
				case Tags.DataSetSubtypeRetired:
				case Tags.NetworkIDRetired:
				case Tags.VolumetricProperties:
				case Tags.UpperLowerPixelValuesRetired:
				case Tags.DynamicRangeRetired:
				case Tags.TotalGainRetired:
				case Tags.LocationRetired:
				case Tags.ReferenceRetired:
				case Tags.ImageDimensionsRetired:
				case Tags.ImageFormatRetired:
				case Tags.ManipulatedImageRetired:
				case Tags.ImageLocationRetired:
				case Tags.GrayScaleRetired: 
					return VRs.UN;
				
				case Tags.FailureReason: 
				case Tags.PregnancyStatus: 
				case Tags.SynchronizationChannel:
				case Tags.PreferredPlaybackSequencing:
				case Tags.AcquisitionMatrix:
				case Tags.ExposuresOnPlate:
				case Tags.ShutterPresentationValue:
				case Tags.ShutterOverlayGroup:
				case Tags.RegionSpatialFormat:
				case Tags.RegionDataType:
				case Tags.PhysicalUnitsXDirection:
				case Tags.PhysicalUnitsYDirection:
				case Tags.PixelComponentOrganization:
				case Tags.PixelComponentPhysicalUnits:
				case Tags.PixelComponentDataType:
				case Tags.MRAcquisitionFrequencyEncodingSteps:
				case Tags.NumberOfZeroFills:
				case Tags.NumberOfKSpaceTrajectories:
				case Tags.MRAcquisitionPhaseEncodingStepsInPlane:
				case Tags.MRAcquisitionPhaseEncodingStepsOutOfPlane:
				case Tags.FrameAcquisitionNumber:
				case Tags.InConcatenationNumber:
				case Tags.InConcatenationTotalNumber:
				case Tags.SamplesPerPixel:
				case Tags.PlanarConfiguration:
				case Tags.Rows:
				case Tags.Columns:
				case Tags.Planes:
				case Tags.UltrasoundColorDataPresent:
				case Tags.BitsAllocated:
				case Tags.BitsStored:
				case Tags.HighBit:
				case Tags.PixelRepresentation:
				case Tags.SmallestValidPixelValueRetired:
				case Tags.LargestValidPixelValueRetired:
				case Tags.SmallestImagePixelValue:
				case Tags.LargestImagePixelValue:
				case Tags.SmallestPixelValueInSeries:
				case Tags.LargestPixelValueInSeries:
				case Tags.SmallestImagePixelValueInPlane:
				case Tags.LargestImagePixelValueInPlane:
				case Tags.PixelPaddingValue:
				case Tags.GreyLUTDescriptorRetired:
				case Tags.RedPaletteColorLUTDescriptor:
				case Tags.GreenPaletteColorLUTDescriptor:
				case Tags.BluePaletteColorLUTDescriptor:
				case Tags.LUTDescriptor:
				case Tags.RepresentativeFrameNumber:
				case Tags.FrameNumbersOfInterest:
				case Tags.MaskPointer:
				case Tags.RWavePointer:
				case Tags.ApplicableFrameRange:
				case Tags.MaskFrameNumbers:
				case Tags.ContrastFrameAveraging:
				case Tags.LargestMonochromePixelValue:
				case Tags.NumberOfWaveformChannels:
				case Tags.WaveformBitsStored:
				case Tags.TotalTimeOfFluoroscopy:
				case Tags.TotalNumberOfExposures:
				case Tags.EntranceDose:
				case Tags.ExposedArea:
				case Tags.RefWaveformChannels:
				case Tags.RefFrameNumbers:
				case Tags.AnnotationGroupNumber:
				case Tags.RealWorldValueLUTLastValueMappedUS:
				case Tags.RealWorldValueLUTFirstValueMappedUS:
				case Tags.EnergyWindowVector:
				case Tags.NumberOfEnergyWindows:
				case Tags.DetectorVector:
				case Tags.NumberOfDetectors:
				case Tags.PhaseVector:
				case Tags.NumberOfPhases:
				case Tags.NumberOfFramesInPhase:
				case Tags.RotationVector:
				case Tags.NumberOfRotations:
				case Tags.NumberOfFramesInRotation:
				case Tags.RRIntervalVector:
				case Tags.NumberOfRRIntervals:
				case Tags.TimeSlotVector:
				case Tags.NumberOfTimeSlots:
				case Tags.SliceVector:
				case Tags.NumberOfSlices:
				case Tags.AngularViewVector:
				case Tags.TimeSliceVector:
				case Tags.NumberOfTimeSlices:
				case Tags.NumberOfTriggersInPhase:
				case Tags.EnergyWindowNumber:
				case Tags.ImageIndex:
				case Tags.HistogramNumberOfBins:
				case Tags.HistogramFirstBinValue:
				case Tags.HistogramLastBinValue:
				case Tags.HistogramBinWidth:
				case Tags.GraphicDimensions:
				case Tags.NumberOfGraphicPoints:
				case Tags.ImageRotation:
				case Tags.GraphicLayerRecommendedDisplayGrayscaleValue:
				case Tags.GraphicLayerRecommendedDisplayRGBValue:
				case Tags.MemoryBitDepth:
				case Tags.PrintingBitDepth:
				case Tags.MinDensity:
				case Tags.MaxDensity:
				case Tags.Illumination:
				case Tags.ReflectedAmbientLight:
				case Tags.ImagePositionOnFilm:
				case Tags.AnnotationPosition:
				case Tags.RefOverlayPlaneGroups:
				case Tags.MagnifyToNumberOfColumns:
				case Tags.WaveformBitsAllocated: 
                case Tags.MACIDNumber:
					return VRs.US;
				
				case Tags.TextValue: 
					return VRs.UT;
				
				case Tags.Item:
				case Tags.ItemDelimitationItem:
				case Tags.SeqDelimitationItem: 
					return VRs.NONE;
				
			}
			switch (tag & 0xFF00FFFF)
			{
				case Tags.TypeOfData:
				case Tags.CurveActivationLayer:
				case Tags.OverlayType:
				case Tags.OverlayCompressionCodeRetired:
				case Tags.OverlayFormatRetired:
				case Tags.OverlayActivationLayer: 
					return VRs.CS;
				
				case Tags.ROIMean:
				case Tags.ROIStandardDeviation: 
					return VRs.DS;
				
				case Tags.NumberOfFramesInOverlay:
				case Tags.ROIArea: 
					return VRs.IS;
				
				case Tags.CurveDescription:
				case Tags.CurveLabel:
				case Tags.OverlayDescription:
				case Tags.OverlaySubtype:
				case Tags.OverlayLabel: 
					return VRs.LO;
				
				case Tags.AudioComments:
				case Tags.OverlayCommentsRetired: 
					return VRs.LT;
				
				case Tags.CurveData: 
					return VRs.OB;
				
				case Tags.AudioSampleData:
				case Tags.OverlayData: 
					return VRs.OW;
				
				case Tags.AxisUnits:
				case Tags.AxisLabels:
				case Tags.CurveRange: 
					return VRs.SH;
				
				case Tags.RefOverlaySeqCurve: 
					return VRs.SQ;
				
				case Tags.OverlayOrigin: 
					return VRs.SS;
				
				case Tags.NumberOfSamples:
				case Tags.SampleRate:
				case Tags.TotalTime: 
					return VRs.UL;
				
				case Tags.OverlayLocationRetired: 
					return VRs.UN;
				
				case Tags.CurveDimensions:
				case Tags.NumberOfPoints:
				case Tags.DataValueRepresentation:
				case Tags.MinimumCoordinateValue:
				case Tags.MaximumCoordinateValue:
				case Tags.CurveDataDescriptor:
				case Tags.CoordinateStartValue:
				case Tags.CoordinateStepValue:
				case Tags.AudioType:
				case Tags.AudioSampleFormat:
				case Tags.NumberOfChannels:
				case Tags.RefOverlayGroup: 
				case Tags.OverlayRows:
				case Tags.OverlayColumns:
				case Tags.OverlayPlanes:
				case Tags.ImageFrameOrigin:
				case Tags.OverlayPlaneOrigin:
				case Tags.OverlayBitsAllocated:
				case Tags.OverlayBitPosition:
				case Tags.OverlayDescriptorGrayRetired:
				case Tags.OverlayDescriptorRedRetired:
				case Tags.OverlayDescriptorGreenRetired:
				case Tags.OverlayDescriptorBlueRetired:
				case Tags.OverlaysGrayRetired: 
				case Tags.OverlaysRedRetired:
				case Tags.OverlaysGreenRetired:
				case Tags.OverlaysBlueRetired: 
					return VRs.US;
				
			}
			switch (tag & 0xFFFFFF00)
			{
				case Tags.SourceImageIDRetired: 
					return VRs.SH;
				
			}
			return VRs.UN;
		}
	}
}