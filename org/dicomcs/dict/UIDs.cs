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
#endregion

namespace org.dicomcs.dict
{
	using System;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// Provides UIDs constants.
	/// </summary>
	public class UIDs
	{		
		private static Hashtable s_dict = new Hashtable( 144 );

		static UIDs()
		{	
			FieldInfo[] fields = typeof(UIDs).GetFields( BindingFlags.Public | BindingFlags.Static ); 
			foreach ( FieldInfo field in fields )
			{
				String name = field.Name; 
				String uid = (String) field.GetValue( null );
				s_dict.Add( uid, name );
			}
		}

		/// <summary>
		/// Private constructor 
		/// </summary>
		private UIDs()
		{
		}

		/// <summary>
		/// Get the description name of this UID
		/// </summary>
		/// <param name="uid">the uid</param>
		/// <returns></returns>
		public static String GetName( String uid )
		{
			if( s_dict.ContainsKey( uid ) )
				return (String) s_dict[uid];
			else
				throw new ArgumentException("Unkown UID: " + uid);
		}
		
		/// <summary>
		/// Get the UID for the description name
		/// </summary>
		/// <param name="name">the description name</param>
		/// <returns></returns>
		public static String GetUID(String name)
		{
			try
			{
				return (String) typeof(UIDs).GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Unkown UID Name: " + name);
			}
		}

		public static String ToString( String uid )
		{
			return "[" + uid + "] SOP class: " + GetName( uid );
		}
		
		/// <summary>TransferSyntax: Implicit VR Little Endian 
		/// </summary>
		public const String ImplicitVRLittleEndian = "1.2.840.10008.1.2";
		
		/// <summary>TransferSyntax: Explicit VR Little Endian 
		/// </summary>
		public const String ExplicitVRLittleEndian = "1.2.840.10008.1.2.1";
		
		/// <summary>TransferSyntax: Deflated Explicit VR Little Endian 
		/// </summary>
		public const String DeflatedExplicitVRLittleEndian = "1.2.840.10008.1.2.1.99";
		
		/// <summary>TransferSyntax: Explicit VR Big Endian 
		/// </summary>
		public const String ExplicitVRBigEndian = "1.2.840.10008.1.2.2";
		
		/// <summary>TransferSyntax: JPEG Baseline (Process 1) 
		/// </summary>
		public const String JPEGBaseline = "1.2.840.10008.1.2.4.50";
		
		/// <summary>TransferSyntax: JPEG Extended (Process 2 & 4) 
		/// </summary>
		public const String JPEGExtended = "1.2.840.10008.1.2.4.51";
		
		/// <summary>TransferSyntax: JPEG Extended (Process 3 & 5) (Retired) 
		/// </summary>
		public const String JPEGExtended35Retired = "1.2.840.10008.1.2.4.52";
		
		/// <summary>TransferSyntax: JPEG Spectral Selection, Non- Hierarchical (Process 6 & 8) (Retired) 
		/// </summary>
		public const String JPEG68Retired = "1.2.840.10008.1.2.4.53";
		
		/// <summary>TransferSyntax: JPEG Spectral Selection, Non- Hierarchical (Process 7 & 9) (Retired) 
		/// </summary>
		public const String JPEG79Retired = "1.2.840.10008.1.2.4.54";
		
		/// <summary>TransferSyntax: JPEG Full Progression, Non- Hierarchical (Process 10 & 12) (Retired) 
		/// </summary>
		public const String JPEG1012Retired = "1.2.840.10008.1.2.4.55";
		
		/// <summary>TransferSyntax: JPEG Full Progression, Non- Hierarchical (Process 11 & 13) (Retired) 
		/// </summary>
		public const String JPEG1113Retired = "1.2.840.10008.1.2.4.56";
		
		/// <summary>TransferSyntax: JPEG Lossless, Non-Hierarchical (Process 14) 
		/// </summary>
		public const String JPEGLossless14 = "1.2.840.10008.1.2.4.57";
		
		/// <summary>TransferSyntax: JPEG Lossless, Non-Hierarchical (Process 15) (Retired) 
		/// </summary>
		public const String JPEGLossless15Retired = "1.2.840.10008.1.2.4.58";
		
		/// <summary>TransferSyntax: JPEG Extended, Hierarchical (Process 16 & 18) (Retired) 
		/// </summary>
		public const String JPEG1618Retired = "1.2.840.10008.1.2.4.59";
		
		/// <summary>TransferSyntax: JPEG Extended, Hierarchical (Process 17 & 19) (Retired) 
		/// </summary>
		public const String JPEG1719Retired = "1.2.840.10008.1.2.4.60";
		
		/// <summary>TransferSyntax: JPEG Spectral Selection, Hierarchical (Process 20 & 22) (Retired) 
		/// </summary>
		public const String JPEG2022Retired = "1.2.840.10008.1.2.4.61";
		
		/// <summary>TransferSyntax: JPEG Spectral Selection, Hierarchical (Process 21 & 23) (Retired) 
		/// </summary>
		public const String JPEG2123Retired = "1.2.840.10008.1.2.4.62";
		
		/// <summary>TransferSyntax: JPEG Full Progression, Hierarchical (Process 24 & 26) (Retired) 
		/// </summary>
		public const String JPEG2426Retired = "1.2.840.10008.1.2.4.63";
		
		/// <summary>TransferSyntax: JPEG Full Progression, Hierarchical (Process 25 & 27) (Retired) 
		/// </summary>
		public const String JPEG2527Retired = "1.2.840.10008.1.2.4.64";
		
		/// <summary>TransferSyntax: JPEG Lossless, Hierarchical (Process 28) (Retired) 
		/// </summary>
		public const String JPEGLoRetired = "1.2.840.10008.1.2.4.65";
		
		/// <summary>TransferSyntax: JPEG Lossless, Hierarchical (Process 29) (Retired) 
		/// </summary>
		public const String JPEG29Retired = "1.2.840.10008.1.2.4.66";
		
		/// <summary>TransferSyntax: JPEG Lossless, Non- Hierarchical, First-Order Prediction (Process 14 [Selection Value 1]) 
		/// </summary>
		public const String JPEGLossless = "1.2.840.10008.1.2.4.70";
		
		/// <summary>TransferSyntax: JPEG-LS Lossless Image Compression 
		/// </summary>
		public const String JPEGLSLossless = "1.2.840.10008.1.2.4.80";
		
		/// <summary>TransferSyntax: JPEG-LS Lossy (Near-Lossless) Image Compression 
		/// </summary>
		public const String JPEGLSLossy = "1.2.840.10008.1.2.4.81";
		
		/// <summary>TransferSyntax: JPEG 2000 Lossless Image Compression 
		/// </summary>
		public const String JPEG2000Lossless = "1.2.840.10008.1.2.4.90";
		
		/// <summary>TransferSyntax: JPEG 2000 Lossy Image Compression 
		/// </summary>
		public const String JPEG2000Lossy = "1.2.840.10008.1.2.4.91";
		
		/// <summary>TransferSyntax: JPEG 2000 Part 2 Lossless Image Compression 
		/// </summary>
		public const String JPEG2000Part2Lossless = "1.2.840.10008.1.2.4.92";

		/// <summary>TransferSyntax: JPEG 2000 Part 2 Lossy Image Compression 
		/// </summary>
		public const String JPEG2000Part2Lossy = "1.2.840.10008.1.2.4.93";

		/// <summary>TransferSyntax: JPIP Refrenced 
		/// </summary>
		public const String JPIPRefrenced = "1.2.840.10008.1.2.4.94";

		/// <summary>TransferSyntax: JPIP Refrenced Deflate
		/// </summary>
		public const String JPIPRefrencedDeflate = "1.2.840.10008.1.2.4.95";

		/// <summary>TransferSyntax: MPEG Main Profile @ Main Level 
		/// </summary>
		public const String MPEGMainProfileMainLevel = "1.2.840.10008.1.2.4.100";

		/// <summary>TransferSyntax: RLE Lossless 
		/// </summary>
		public const String RLELossless = "1.2.840.10008.1.2.5";
		
		/// <summary>TransferSyntax: RFC 2557 MIME encapsulation
		/// </summary>
		public const String RFC2557MIMEEncapsulation = "1.2.840.10008.1.2.6.1";

		/// <summary>TransferSyntax: XML Enconding
		/// </summary>
		public const String XMLEncoding = "1.2.840.10008.1.2.6.2";

		/// <summary>SOPClass: Verification SOP Class 
		/// </summary>
		public const String Verification = "1.2.840.10008.1.1";
		
		/// <summary>SOPClass: Media Storage Directory Storage 
		/// </summary>
		public const String MediaStorageDirectoryStorage = "1.2.840.10008.1.3.10";
		
		/// <summary>SOPClass: Basic Study Content Notification SOP Class (Retired) 
		/// </summary>
		public const String BasicStudyContentNotificationRetired = "1.2.840.10008.1.9";
		
		/// <summary>SOPClass: Storage Commitment Push Model SOP Class 
		/// </summary>
		public const String StorageCommitmentPushModel = "1.2.840.10008.1.20.1";
		
		/// <summary>SOPClass: Storage Commitment Pull Model SOP Class (Retired)
		/// </summary>
		public const String StorageCommitmentPullModelRetired = "1.2.840.10008.1.20.2";
		
		/// <summary>SOPClass: Procedural Event Logging SOP Class 
		/// </summary>
		public const String ProceduralEventLogging = "1.2.840.10008.1.40";
		
		/// <summary>SOPClass: Substance Administration Logging SOP Class
		/// </summary>
		public const String SubstanceAdministrationLogging = "1.2.840.10008.1.42";
		
		/// <summary>SOPClass: Detached Patient Management SOP Class (Retired) 
		/// </summary>
		public const String DetachedPatientManagementRetired = "1.2.840.10008.3.1.2.1.1";
		
		/// <summary>SOPClass: Detached Visit Management SOP Class (Retired)
		/// </summary>
		public const String DetachedVisitManagementRetired = "1.2.840.10008.3.1.2.2.1";
		
		/// <summary>SOPClass: Detached Study Management SOP Class (Retired) 
		/// </summary>
		public const String DetachedStudyManagementRetired = "1.2.840.10008.3.1.2.3.1";
		
		/// <summary>SOPClass: Study Component Management SOP Class (Retired) 
		/// </summary>
		public const String StudyComponentManagementRetired = "1.2.840.10008.3.1.2.3.2";
		
		/// <summary>SOPClass: Modality Performed Procedure Step SOP Class 
		/// </summary>
		public const String ModalityPerformedProcedureStep = "1.2.840.10008.3.1.2.3.3";
		
		/// <summary>SOPClass: Modality Performed Procedure Step Retrieve SOP Class 
		/// </summary>
		public const String ModalityPerformedProcedureStepRetrieve = "1.2.840.10008.3.1.2.3.4";
		
		/// <summary>SOPClass: Modality Performed Procedure Step Notification SOP Class 
		/// </summary>
		public const String ModalityPerformedProcedureStepNotification = "1.2.840.10008.3.1.2.3.5";
		
		/// <summary>SOPClass: Detached Results Management SOP Class (Retired)
		/// </summary>
		public const String DetachedResultsManagementRetired = "1.2.840.10008.3.1.2.5.1";
		
		/// <summary>SOPClass: Detached Interpretation Management SOP Class (Retired) 
		/// </summary>
		public const String DetachedInterpretationManagementRetired = "1.2.840.10008.3.1.2.6.1";
		
		/// <summary>SOPClass: Basic Film Session SOP Class 
		/// </summary>
		public const String BasicFilmSession = "1.2.840.10008.5.1.1.1";
		
		/// <summary>SOPClass: Basic Film Box SOP Class 
		/// </summary>
		public const String BasicFilmBoxSOP = "1.2.840.10008.5.1.1.2";
		
		/// <summary>SOPClass: Basic Grayscale Image Box SOP Class 
		/// </summary>
		public const String BasicGrayscaleImageBox = "1.2.840.10008.5.1.1.4";
		
		/// <summary>SOPClass: Basic Color Image Box SOP Class 
		/// </summary>
		public const String BasicColorImageBox = "1.2.840.10008.5.1.1.4.1";
		
		/// <summary>SOPClass: Referenced Image Box SOP Class (Retired) 
		/// </summary>
		public const String ReferencedImageBoxRetired = "1.2.840.10008.5.1.1.4.2";
		
		/// <summary>SOPClass: Print Job SOP Class 
		/// </summary>
		public const String PrintJob = "1.2.840.10008.5.1.1.14";
		
		/// <summary>SOPClass: Basic Annotation Box SOP Class 
		/// </summary>
		public const String BasicAnnotationBox = "1.2.840.10008.5.1.1.15";
		
		/// <summary>SOPClass: Printer SOP Class 
		/// </summary>
		public const String Printer = "1.2.840.10008.5.1.1.16";
		
		/// <summary>SOPClass: Printer Configuration Retrieval SOP Class 
		/// </summary>
		public const String PrinterConfigurationRetrieval = "1.2.840.10008.5.1.1.16.376";
		
		/// <summary>SOPClass: VOI LUT Box SOP Class 
		/// </summary>
		public const String VOILUTBox = "1.2.840.10008.5.1.1.22";
		
		/// <summary>SOPClass: Presentation LUT SOP Class 
		/// </summary>
		public const String PresentationLUT = "1.2.840.10008.5.1.1.23";
		
		/// <summary>SOPClass: Image Overlay Box SOP Class (Retired) 
		/// </summary>
		public const String ImageOverlayBoxRetired = "1.2.840.10008.5.1.1.24";
		
		/// <summary>SOPClass: Basic Print Image Overlay Box SOP Class (Retired) 
		/// </summary>
		public const String BasicPrintImageOverlayBoxRetired = "1.2.840.10008.5.1.1.24.1";
		
		/// <summary>SOPClass: Print Queue Management SOP Class (Retired) 
		/// </summary>
		public const String PrintQueueManagementRetired = "1.2.840.10008.5.1.1.26";
		
		/// <summary>SOPClass: Stored Print Storage SOP Class (Retired) 
		/// </summary>
		public const String StoredPrintStorageRetired = "1.2.840.10008.5.1.1.27";
		
		/// <summary>SOPClass: Hardcopy Grayscale Image Storage SOP Class (Retired)
		/// </summary>
		public const String HardcopyGrayscaleImageStorageRetired = "1.2.840.10008.5.1.1.29";
		
		/// <summary>SOPClass: Hardcopy Color Image Storage SOP Class (Retired)
		/// </summary>
		public const String HardcopyColorImageStorageRetired = "1.2.840.10008.5.1.1.30";
		
		/// <summary>SOPClass: Pull Print Request SOP Class (Retired)
		/// </summary>
		public const String PullPrintRequestRetired = "1.2.840.10008.5.1.1.31";

		/// <summary>SOPClass: Media Creation Management SOP Class
		/// </summary>
		public const String MediaCreationManagement = "1.2.840.10008.5.1.1.33";
		
		/// <summary>SOPClass: Computed Radiography Image Storage 
		/// </summary>
		public const String ComputedRadiographyImageStorage = "1.2.840.10008.5.1.4.1.1.1";
		
		/// <summary>SOPClass: Digital X-Ray Image Storage - For Presentation 
		/// </summary>
		public const String DigitalXRayImageStorageForPresentation = "1.2.840.10008.5.1.4.1.1.1.1";
		
		/// <summary>SOPClass: Digital X-Ray Image Storage - For Processing 
		/// </summary>
		public const String DigitalXRayImageStorageForProcessing = "1.2.840.10008.5.1.4.1.1.1.1.1";
		
		/// <summary>SOPClass: Digital Mammography X-Ray Image Storage - For Presentation 
		/// </summary>
		public const String DigitalMammographyXRayImageStorageForPresentation = "1.2.840.10008.5.1.4.1.1.1.2";
		
		/// <summary>SOPClass: Digital Mammography X-Ray Image Storage - For Processing 
		/// </summary>
		public const String DigitalMammographyXRayImageStorageForProcessing = "1.2.840.10008.5.1.4.1.1.1.2.1";
		
		/// <summary>SOPClass: Digital Intra-oral X-Ray Image Storage - For Presentation 
		/// </summary>
		public const String DigitalIntraoralXRayImageStorageForPresentation = "1.2.840.10008.5.1.4.1.1.1.3";
		
		/// <summary>SOPClass: Digital Intra-oral X-Ray Image Storage - For Processing 
		/// </summary>
		public const String DigitalIntraoralXRayImageStorageForProcessing = "1.2.840.10008.5.1.4.1.1.1.3.1";
		
		/// <summary>SOPClass: CT Image Storage 
		/// </summary>
		public const String CTImageStorage = "1.2.840.10008.5.1.4.1.1.2";

		/// <summary>SOPClass: Enhanced CT Image Storage 
		/// </summary>
		public const String EnhancedCTImageStorage = "1.2.840.10008.5.1.4.1.1.2.1";
		
		/// <summary>SOPClass: Ultrasound Multi-frame Image Storage (Retired) 
		/// </summary>
		public const String UltrasoundMultiframeImageStorageRetired = "1.2.840.10008.5.1.4.1.1.3";
		
		/// <summary>SOPClass: Ultrasound Multi-frame Image Storage 
		/// </summary>
		public const String UltrasoundMultiframeImageStorage = "1.2.840.10008.5.1.4.1.1.3.1";
		
		/// <summary>SOPClass: MR Image Storage 
		/// </summary>
		public const String MRImageStorage = "1.2.840.10008.5.1.4.1.1.4";
		
		/// <summary>SOPClass: Enhanced MR Image Storage 
		/// </summary>
		public const String EnhancedMRImageStorage = "1.2.840.10008.5.1.4.1.1.4.1";
		
		/// <summary>SOPClass: MR Spectroscopy Storage 
		/// </summary>
		public const String MRSpectroscopyStorage = "1.2.840.10008.5.1.4.1.1.4.2";
		
		/// <summary>SOPClass: Nuclear Medicine Image Storage (Retired) 
		/// </summary>
		public const String NuclearMedicineImageStorageRetired = "1.2.840.10008.5.1.4.1.1.5";
		
		/// <summary>SOPClass: Ultrasound Image Storage (Retired) 
		/// </summary>
		public const String UltrasoundImageStorageRetired = "1.2.840.10008.5.1.4.1.1.6";
		
		/// <summary>SOPClass: Ultrasound Image Storage 
		/// </summary>
		public const String UltrasoundImageStorage = "1.2.840.10008.5.1.4.1.1.6.1";
		
		/// <summary>SOPClass: Secondary Capture Image Storage 
		/// </summary>
		public const String SecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7";
		
		/// <summary>SOPClass: Multi-frame Single Bit Secondary Capture Image Storage 
		/// </summary>
		public const String MultiframeSingleBitSecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7.1";
		
		/// <summary>SOPClass: Multi-frame Grayscale Byte Secondary Capture Image Storage 
		/// </summary>
		public const String MultiframeGrayscaleByteSecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7.2";
		
		/// <summary>SOPClass: Multi-frame Grayscale Word Secondary Capture Image Storage 
		/// </summary>
		public const String MultiframeGrayscaleWordSecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7.3";
		
		/// <summary>SOPClass: Multi-frame True Color Secondary Capture Image Storage 
		/// </summary>
		public const String MultiframeTrueColorSecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7.4";
		
		/// <summary>SOPClass: Standalone Overlay Storage (Retired)
		/// </summary>
		public const String StandaloneOverlayStorageRetired = "1.2.840.10008.5.1.4.1.1.8";
		
		/// <summary>SOPClass: Standalone Curve Storage (Retired)
		/// </summary>
		public const String StandaloneCurveStorageRetired = "1.2.840.10008.5.1.4.1.1.9";

		/// <summary>SOPClass: Waveform Storage Trial (Retired)
		/// </summary>
		public const String WaveformStorageTrialRetired =  "1.2.840.10008.5.1.4.1.1.9.1";
		
		/// <summary>SOPClass: 12-lead ECG Waveform Storage 
		/// </summary>
		public const String TwelveLeadECGWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.1.1";
		
		/// <summary>SOPClass: General ECG Waveform Storage 
		/// </summary>
		public const String GeneralECGWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.1.2";
		
		/// <summary>SOPClass: Ambulatory ECG Waveform Storage 
		/// </summary>
		public const String AmbulatoryECGWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.1.3";
		
		/// <summary>SOPClass: Hemodynamic Waveform Storage 
		/// </summary>
		public const String HemodynamicWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.2.1";
		
		/// <summary>SOPClass: Cardiac Electrophysiology Waveform Storage 
		/// </summary>
		public const String CardiacElectrophysiologyWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.3.1";
		
		/// <summary>SOPClass: Basic Voice Audio Waveform Storage 
		/// </summary>
		public const String BasicVoiceAudioWaveformStorage = "1.2.840.10008.5.1.4.1.1.9.4.1";
		
		/// <summary>SOPClass: Standalone Modality LUT Storage (Retired)
		/// </summary>
		public const String StandaloneModalityLUTStorageRetired = "1.2.840.10008.5.1.4.1.1.10";
		
		/// <summary>SOPClass: Standalone VOI LUT Storage (Retired)
		/// </summary>
		public const String StandaloneVOILUTStorageRetired = "1.2.840.10008.5.1.4.1.1.11";
		
		/// <summary>SOPClass: Grayscale Softcopy Presentation State Storage SOP Class 
		/// </summary>
		public const String GrayscaleSoftcopyPresentationStateStorage = "1.2.840.10008.5.1.4.1.1.11.1";

		/// <summary>SOPClass: Color Softcopy Presentation State Storage SOP Class
		/// </summary>
		public const String ColorSoftcopyPresentationStateStorage = "1.2.840.10008.5.1.4.1.1.11.2";

		/// <summary>SOPClass: Pseudo-Color Softcopy Presentation State Storage SOP Class 
		/// </summary>
		public const String PseudoColorSoftcopyPresentationStateStorage = "1.2.840.10008.5.1.4.1.1.11.3";

		/// <summary>SOPClass: Blending Softcopy Presentation State Storage SOP Class 
		/// </summary>
		public const String BlendingSoftcopyPresentationStateStorage = "1.2.840.10008.5.1.4.1.1.11.4";
		
		/// <summary>SOPClass: X-Ray Angiographic Image Storage 
		/// </summary>
		public const String XRayAngiographicImageStorage = "1.2.840.10008.5.1.4.1.1.12.1";

		/// <summary>SOPClass: Enhanced XA Image Storage 
		/// </summary>
		public const String EnhancedXAImageStorage = "1.2.840.10008.5.1.4.1.1.12.1.1";
		
		/// <summary>SOPClass: X-Ray Radiofluoroscopic Image Storage 
		/// </summary>
		public const String XRayRadiofluoroscopicImageStorage = "1.2.840.10008.5.1.4.1.1.12.2";

		/// <summary>SOPClass: Enhanced XRF Image Storage
		/// </summary>
		public const String EnhancedXRFImageStorage = "1.2.840.10008.5.1.4.1.1.12.2.1";

		/// <summary>SOPClass: X-Ray 3D Angiographic Image Storage
		/// </summary>
		public const String XRay3DAngiographicImageStorage = "1.2.840.10008.5.1.4.1.1.13.1.1";

		/// <summary>SOPClass: X-Ray 3D Craniofacial Image Storage
		/// </summary>
		public const String XRay3DCraniofacialImageStorage = "1.2.840.10008.5.1.4.1.1.13.1.2";
		
		/// <summary>SOPClass: X-Ray Angiographic Bi-Plane Image Storage (Retired) 
		/// </summary>
		public const String XRayAngiographicBiPlaneImageStorageRetired = "1.2.840.10008.5.1.4.1.1.12.3";

		/// <summary>SOPClass: Nuclear Medicine Image Storage 
		/// </summary>
		public const String NuclearMedicineImageStorage = "1.2.840.10008.5.1.4.1.1.20";
		
		/// <summary>SOPClass: Raw Data Storage 
		/// </summary>
		public const String RawDataStorage = "1.2.840.10008.5.1.4.1.1.66";

		/// <summary>SOPClass: Spatial Registration Storage 
		/// </summary>
		public const String SpatialRegistrationStorage = "1.2.840.10008.5.1.4.1.1.66.1";

		/// <summary>SOPClass: Spatial Fiducials Storage
		/// </summary>
		public const String SpatialFiducialsStorage = "1.2.840.10008.5.1.4.1.1.66.2";

		/// <summary>SOPClass: Deformable Spatial Registration Storage
		/// </summary>
		public const String DeformableSpatialRegistrationStorage = "1.2.840.10008.5.1.4.1.1.66.3";

		/// <summary>SOPClass: Segmentation Storage
		/// </summary>
		public const String SegmentationStorage = "1.2.840.10008.5.1.4.1.1.66.4";

		/// <summary>SOPClass: Real World Value Mapping Storage 
		/// </summary>
		public const String RealWorldValueMappingStorage = "1.2.840.10008.5.1.4.1.1.67";
		
		/// <summary>SOPClass: VL Image Storage (Retired) 
		/// </summary>
		public const String VLImageStorageRetired = "1.2.840.10008.5.1.4.1.1.77.1";
		
		/// <summary>SOPClass: VL Multi-frame Image Storage (Retired) 
		/// </summary>
		public const String VLMultiframeImageStorageRetired = "1.2.840.10008.5.1.4.1.1.77.2";
		
		/// <summary>SOPClass: VL Endoscopic Image Storage 
		/// </summary>
		public const String VLEndoscopicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.1";

		/// <summary>SOPClass: Video Endoscopic Image Storage 
		/// </summary>
		public const String VideoEndoscopicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.1.1";
		
		/// <summary>SOPClass: VL Microscopic Image Storage 
		/// </summary>
		public const String VLMicroscopicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.2";

		/// <summary>SOPClass: Video Microscopic Image Storage 
		/// </summary>
		public const String VideoMicroscopicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.2.1";
		
		/// <summary>SOPClass: VL Slide-Coordinates Microscopic Image Storage 
		/// </summary>
		public const String VLSlideCoordinatesMicroscopicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.3";
		
		/// <summary>SOPClass: VL Photographic Image Storage 
		/// </summary>
		public const String VLPhotographicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.4";

		/// <summary>SOPClass: Video Photographic Image Storage 
		/// </summary>
		public const String VideoPhotographicImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.4.1";

		/// <summary>SOPClass: Ophthalmic Photography 8 Bit Image Storage
		/// </summary>
		public const String OphthalmicPhotography8BitImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.5.1";

		/// <summary>SOPClass: Ophthalmic Photography 16 Bit Image Storage
		/// </summary>
		public const String OphthalmicPhotography16BitImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.5.2";

		/// <summary>SOPClass: Stereometric Relationship Storage
		/// </summary>
		public const String StereometricRelationshipStorage = "1.2.840.10008.5.1.4.1.1.77.1.5.3";

		/// <summary>SOPClass: Ophthalmic Tomography Image Storage
		/// </summary>
		public const String OphthalmicTomographyImageStorage = "1.2.840.10008.5.1.4.1.1.77.1.5.4";

		/// <summary>SOPClass: Text SR Storage – Trial (Retired)
		/// </summary>
		public const String TextSRStorageTrialRetired = "1.2.840.10008.5.1.4.1.1.88.1";

		/// <summary>SOPClass: Audio SR Storage – Trial (Retired)
		/// </summary>
		public const String AudioSRStorageTrialRetired = "1.2.840.10008.5.1.4.1.1.88.2";

		/// <summary>SOPClass: Detail SR Storage – Trial (Retired)
		/// </summary>
		public const String DetailSRStorageTrialRetired = "1.2.840.10008.5.1.4.1.1.88.3";

		/// <summary>SOPClass: Comprehensive SR Storage – Trial (Retired)
		/// </summary>
		public const String ComprehensiveSRStorageTrialRetired = "1.2.840.10008.5.1.4.1.1.88.4";
		
		/// <summary>SOPClass: Basic Text SR Storage
		/// </summary>
		public const String BasicTextSR = "1.2.840.10008.5.1.4.1.1.88.11";
		
		/// <summary>SOPClass: Enhanced SR Storage
		/// </summary>
		public const String EnhancedSR = "1.2.840.10008.5.1.4.1.1.88.22";
		
		/// <summary>SOPClass: Comprehensive SR Storage (Retired)
		/// </summary>
		public const String ComprehensiveSRStorageRetired = "1.2.840.10008.5.1.4.1.1.88.33";

		/// <summary>SOPClass: Procedure Log Storage 
		/// </summary>
		public const String ProcedureLogStorage = "1.2.840.10008.5.1.4.1.1.88.40";
		
		/// <summary>SOPClass: Mammography CAD SR Storage
		/// </summary>
		public const String MammographyCADSRStorage = "1.2.840.10008.5.1.4.1.1.88.50";
		
		/// <summary>SOPClass: Key Object Selection Document Storage
		/// </summary>
		public const String KeyObjectSelectionDocumentStorage = "1.2.840.10008.5.1.4.1.1.88.59";

		/// <summary>SOPClass: Chest CAD SR Storage
		/// </summary>
		public const String ChestCADSRStorage = "1.2.840.10008.5.1.4.1.1.88.65";

		/// <summary>SOPClass: X-Ray Radiation Dose SR Storage
		/// </summary>
		public const String XRayRadiationDoseSR = "1.2.840.10008.5.1.4.1.1.88.67";

		/// <summary>SOPClass: Encapsulated PDF Storage
		/// </summary>
		public const String EncapsulatedPDFStorage = "1.2.840.10008.5.1.4.1.1.104.1";

		/// <summary>SOPClass: Encapsulated CDA Storage
		/// </summary>
		public const String EncapsulatedCDAStorage = "1.2.840.10008.5.1.4.1.1.104.2";
		
		/// <summary>SOPClass: Positron Emission Tomography Image Storage 
		/// </summary>
		public const String PositronEmissionTomographyImageStorage = "1.2.840.10008.5.1.4.1.1.128";
		
		/// <summary>SOPClass: Standalone PET Curve Storage (Retired)
		/// </summary>
		public const String StandalonePETCurveStorageRetired = "1.2.840.10008.5.1.4.1.1.129";
		
		/// <summary>SOPClass: RT Image Storage 
		/// </summary>
		public const String RTImageStorage = "1.2.840.10008.5.1.4.1.1.481.1";
		
		/// <summary>SOPClass: RT Dose Storage 
		/// </summary>
		public const String RTDoseStorage = "1.2.840.10008.5.1.4.1.1.481.2";
		
		/// <summary>SOPClass: RT Structure Set Storage 
		/// </summary>
		public const String RTStructureSetStorage = "1.2.840.10008.5.1.4.1.1.481.3";
		
		/// <summary>SOPClass: RT Beams Treatment Record Storage 
		/// </summary>
		public const String RTBeamsTreatmentRecordStorage = "1.2.840.10008.5.1.4.1.1.481.4";
		
		/// <summary>SOPClass: RT Plan Storage 
		/// </summary>
		public const String RTPlanStorage = "1.2.840.10008.5.1.4.1.1.481.5";
		
		/// <summary>SOPClass: RT Brachy Treatment Record Storage 
		/// </summary>
		public const String RTBrachyTreatmentRecordStorage = "1.2.840.10008.5.1.4.1.1.481.6";
		
		/// <summary>SOPClass: RT Treatment Summary Record Storage 
		/// </summary>
		public const String RTTreatmentSummaryRecordStorage = "1.2.840.10008.5.1.4.1.1.481.7";

		/// <summary>SOPClass: RT Ion Plan Storage
		/// </summary>
		public const String RTIonPlanStorage = "1.2.840.10008.5.1.4.1.1.481.8";

		/// <summary>SOPClass: RT Ion Beams Treatment Record Storage 
		/// </summary>
		public const String RTIonBeamsTreatmentRecordStorage = "1.2.840.10008.5.1.4.1.1.481.9";
		
		/// <summary>SOPClass: Patient Root Query/Retrieve Information Model - FIND 
		/// </summary>
		public const String PatientRootQueryRetrieveInformationModelFIND = "1.2.840.10008.5.1.4.1.2.1.1";
		
		/// <summary>SOPClass: Patient Root Query/Retrieve Information Model - MOVE 
		/// </summary>
		public const String PatientRootQueryRetrieveInformationModelMOVE = "1.2.840.10008.5.1.4.1.2.1.2";
		
		/// <summary>SOPClass: Patient Root Query/Retrieve Information Model - GET 
		/// </summary>
		public const String PatientRootQueryRetrieveInformationModelGET = "1.2.840.10008.5.1.4.1.2.1.3";
		
		/// <summary>SOPClass: Study Root Query/Retrieve Information Model - FIND 
		/// </summary>
		public const String StudyRootQueryRetrieveInformationModelFIND = "1.2.840.10008.5.1.4.1.2.2.1";
		
		/// <summary>SOPClass: Study Root Query/Retrieve Information Model - MOVE 
		/// </summary>
		public const String StudyRootQueryRetrieveInformationModelMOVE = "1.2.840.10008.5.1.4.1.2.2.2";
		
		/// <summary>SOPClass: Study Root Query/Retrieve Information Model - GET 
		/// </summary>
		public const String StudyRootQueryRetrieveInformationModelGET = "1.2.840.10008.5.1.4.1.2.2.3";
		
		/// <summary>SOPClass: Patient/Study Only Query/Retrieve Information Model - FIND (Retired) 
		/// </summary>
		public const String PatientStudyOnlyQueryRetrieveInformationModelFINDRetired = "1.2.840.10008.5.1.4.1.2.3.1";
		
		/// <summary>SOPClass: Patient/Study Only Query/Retrieve Information Model - MOVE (Retired)
		/// </summary>
		public const String PatientStudyOnlyQueryRetrieveInformationModelMOVERetired = "1.2.840.10008.5.1.4.1.2.3.2";
		
		/// <summary>SOPClass: Patient/Study Only Query/Retrieve Information Model - GET (Retired)
		/// </summary>
		public const String PatientStudyOnlyQueryRetrieveInformationModelGETRetired = "1.2.840.10008.5.1.4.1.2.3.3";
		
		/// <summary>SOPClass: Modality Worklist Information Model - FIND 
		/// </summary>
		public const String ModalityWorklistInformationModelFIND = "1.2.840.10008.5.1.4.31";
		
		/// <summary>SOPClass: General Purpose Worklist Information Model - FIND 
		/// </summary>
		public const String GeneralPurposeWorklistInformationModelFIND = "1.2.840.10008.5.1.4.32.1";
		
		/// <summary>SOPClass: General Purpose Scheduled Procedure Step SOP Class 
		/// </summary>
		public const String GeneralPurposeScheduledProcedureStep = "1.2.840.10008.5.1.4.32.2";
		
		/// <summary>SOPClass: General Purpose Performed Procedure Step SOP Class 
		/// </summary>
		public const String GeneralPurposePerformedProcedureStep = "1.2.840.10008.5.1.4.32.3";
		
		/// <summary>SOPClass: Instance Availability Notification SOP Class
		/// </summary>
		public const String InstanceAvailabilityNotification = "1.2.840.10008.5.1.4.33";
		
		/// <summary>SOPClass: RT Beams Delivery Instruction Storage (Supplement 74 Frozen Draft)
		/// </summary>
		public const String RTBeamsDeliveryInstructionDraft74 = "1.2.840.10008.5.1.4.34.1";
		
		/// <summary>SOPClass: RT Conventional Machine Verification (Supplement 74 Frozen Draft)
		/// </summary>
		public const String RTConventionalMachineVerificationDraft74 = "1.2.840.10008.5.1.4.34.2";

		/// <summary>SOPClass: RT Ion Machine Verification (Supplement 74 Frozen Draft)
		/// </summary>
		public const String RTIonMachineVerificationDraft74 = "1.2.840.10008.5.1.4.34.3";

		/// <summary>SOPClass: Unified Procedure Step - Push
		/// </summary>
		public const String UnifiedProcedureStepPush = "1.2.840.10008.5.1.4.34.4.1";

		/// <summary>SOPClass: Unified Procedure Step - Watch
		/// </summary>
		public const String UnifiedProcedureStepWatch = "1.2.840.10008.5.1.4.34.4.2";

		/// <summary>SOPClass: Unified Procedure Step - Pull
		/// </summary>
		public const String UnifiedProcedureStepPull = "1.2.840.10008.5.1.4.34.4.3";

		/// <summary>SOPClass: Unified Procedure Step - Event
		/// </summary>
		public const String UnifiedProcedureStepEvent = "1.2.840.10008.5.1.4.34.4.4";

		/// <summary>SOPClass: General Relevent Patient Information Query 
		/// </summary>
		public const String GeneralReleventPatientInformationQuery = "1.2.840.10008.5.1.4.37.1";

		/// <summary>SOPClass: Breast Imaging Relevent Patient Information Query 
		/// </summary>
		public const String BreastImagingReleventPatientInformationQuery = "1.2.840.10008.5.1.4.37.2";

		/// <summary>SOPClass: Cardiac Patient Information Query 
		/// </summary>
		public const String CardiacReleventPatientInformationQuery = "1.2.840.10008.5.1.4.37.3";

		/// <summary>SOPClass: Hanging Protocol Storage
		/// </summary>
		public const String HangingProtocolStorage = "1.2.840.10008.5.1.4.38.1";

		/// <summary>SOPClass: Hanging Protocol Information Model - FIND
		/// </summary>
		public const String HangingProtocolInformationModelFIND = "1.2.840.10008.5.1.4.38.2";

		/// <summary>SOPClass: Hanging Protocol Information Model - MOVE
		/// </summary>
		public const String HangingProtocolInformationModelMOVE = "1.2.840.10008.5.1.4.38.3";

		/// <summary>SOPClass: Product Characteristics Query
		/// </summary>
		public const String ProductCharacteristicsQuery = "1.2.840.10008.5.1.4.41";

		/// <summary>SOPClass: Substance Approval Query
		/// </summary>
		public const String SubstanceApprovalQuery = "1.2.840.10008.5.1.4.42";

		/// <summary>MetaSOPClass: Detached Patient Management Meta SOP Class (Retired)
		/// </summary>
		public const String DetachedPatientManagementMetaSOPClassRetired = "1.2.840.10008.3.1.2.1.4";
		
		/// <summary>MetaSOPClass: Detached Results Management Meta SOP Class (Retired)
		/// </summary>
		public const String DetachedResultsManagementMetaSOPClassRetired = "1.2.840.10008.3.1.2.5.4";
		
		/// <summary>MetaSOPClass: Detached Study Management Meta SOP Class (Retired)
		/// </summary>
		public const String DetachedStudyManagementMetaSOPClassRetired = "1.2.840.10008.3.1.2.5.5";
		
		/// <summary>MetaSOPClass: Basic Grayscale Print Management Meta SOP Class 
		/// </summary>
		public const String BasicGrayscalePrintManagement = "1.2.840.10008.5.1.1.9";
		
		/// <summary>MetaSOPClass: Referenced Grayscale Print Management Meta SOP Class (Retired) 
		/// </summary>
		public const String ReferencedGrayscalePrintManagementRetired = "1.2.840.10008.5.1.1.9.1";
		
		/// <summary>MetaSOPClass: Basic Color Print Management Meta SOP Class 
		/// </summary>
		public const String BasicColorPrintManagement = "1.2.840.10008.5.1.1.18";
		
		/// <summary>MetaSOPClass: Referenced Color Print Management Meta SOP Class (Retired) 
		/// </summary>
		public const String ReferencedColorPrintManagementRetired = "1.2.840.10008.5.1.1.18.1";
		
		/// <summary>MetaSOPClass: Pull Stored Print Management Meta SOP Class (Retired)
		/// </summary>
		public const String PullStoredPrintManagementMetaSOPClassRetired = "1.2.840.10008.5.1.1.32";
		
		/// <summary>MetaSOPClass: General Purpose Worklist Management Meta SOP Class 
		/// </summary>
		public const String GeneralPurposeWorklistManagementMetaSOPClass = "1.2.840.10008.5.1.4.32";
		
		/// <summary>SOPInstance: Storage Commitment Push Model SOP Instance 
		/// </summary>
		public const String StorageCommitmentPushModelSOPInstance = "1.2.840.10008.1.20.1.1";
		
		/// <summary>SOPInstance: Storage Commitment Pull Model SOP Instance (Retired)
		/// </summary>
		public const String StorageCommitmentPullModelSOPInstanceRetired = "1.2.840.10008.1.20.2.1";
		
		/// <summary>SOPInstance: Substance Administration Logging SOP Instance 
		/// </summary>
		public const String SubstanceAdministrationLoggingSOPInstance = "1.2.840.10008.1.40.1";

		/// <summary>Service Class: Unified Worklist and Procedure Step Well-known SOP Instance
		/// </summary>
		public const String UnifiedWorklistAndProcedureStepSOPInstance = "1.2.840.10008.5.1.4.34.5";
		
		/// <summary>SOPInstance: Printer SOP Instance 
		/// </summary>
		public const String PrinterSOPInstance = "1.2.840.10008.5.1.1.17";
		
		/// <summary>SOPInstance: Printer Configuration Retrieval SOP Instance 
		/// </summary>
		public const String PrinterConfigurationRetrievalSOPInstance = "1.2.840.10008.5.1.1.17.376";
		
		/// <summary>SOPInstance: Print Queue SOP Instance (Retired)
		/// </summary>
		public const String PrintQueueSOPInstanceRetired = "1.2.840.10008.5.1.1.25";

		/// <summary>Well-known frame of reference: Talairach Brain Atlas Frame of Reference 
		/// </summary>
		public const String TalairachBrainAtlasFrameOfReference = "1.2.840.10008.1.4.1.1";

		/// <summary>Well-known frame of reference:  SPM2 T1 Frame of Reference
		/// </summary>
		public const String SPM2T1FrameOfReference = "1.2.840.10008.1.4.1.2";

		/// <summary>Well-known frame of reference: SPM2 T2 Frame of Reference 
		/// </summary>
		public const String SPM2T2FrameOfReference = "1.2.840.10008.1.4.1.3";

		/// <summary>Well-known frame of reference: SPM2 PD Frame of Reference
		/// </summary>
		public const String SPM2PDFrameOfReference = "1.2.840.10008.1.4.1.4";

		/// <summary>Well-known frame of reference: SPM2 EPI Frame of Reference
		/// </summary>
		public const String SPM2EPIFrameOfReference = "1.2.840.10008.1.4.1.5";

		/// <summary>Well-known frame of reference: SPM2 FIL T1 Frame of Reference
		/// </summary>
		public const String SPM2FILT1FrameOfReference = "1.2.840.10008.1.4.1.6";

		/// <summary>Well-known frame of reference: SPM2 PET Frame of Reference
		/// </summary>
		public const String SPM2PETFrameOfReference = "1.2.840.10008.1.4.1.7";

		/// <summary>Well-known frame of reference: SPM2 TRANSM Frame of Reference
		/// </summary>
		public const String SPM2TRANSMFrameOfReference = "1.2.840.10008.1.4.1.8";

		/// <summary>Well-known frame of reference: SPM2 SPECT Frame of Reference
		/// </summary>
		public const String SPM2SPECTFrameOfReference = "1.2.840.10008.1.4.1.9";

		/// <summary>Well-known frame of reference: SPM2 GRAY Frame of Reference
		/// </summary>
		public const String SPM2GRAYFrameOfReference = "1.2.840.10008.1.4.1.10";

		/// <summary>Well-known frame of reference: SPM2 WHITE Frame of Reference
		/// </summary>
		public const String SPM2WHITEFrameOfReference = "1.2.840.10008.1.4.1.11";

		/// <summary>Well-known frame of reference: SPM2 CSF Frame of Reference
		/// </summary>
		public const String SPM2CSFFrameOfReference = "1.2.840.10008.1.4.1.12";

		/// <summary>Well-known frame of reference: SPM2 BRAINMASK Frame of Reference
		/// </summary>
		public const String SPM2BRAINMASKFrameOfReference = "1.2.840.10008.1.4.1.13";

		/// <summary>Well-known frame of reference: SPM2 AVG305T1 Frame of Reference
		/// </summary>
		public const String SPM2AVG305T1FrameOfReference = "1.2.840.10008.1.4.1.14";

		/// <summary>Well-known frame of reference: SPM2 AVG152T1 Frame of Reference
		/// </summary>
		public const String SPM2AVG152T1FrameOfReference = "1.2.840.10008.1.4.1.15";

		/// <summary>Well-known frame of reference: SPM2 AVG152T2 Frame of Reference
		/// </summary>
		public const String SPM2AVG125T2FrameOfReference = "1.2.840.10008.1.4.1.16";

		/// <summary>Well-known frame of reference: SPM2 AVG152PD Frame of Reference
		/// </summary>
		public const String SPM2AVG152PDFrameOfReference = "1.2.840.10008.1.4.1.17";

		/// <summary>Well-known frame of reference: SPM2 SINGLESUBJT1 Frame of Reference
		/// </summary>
		public const String SPM2SINGLESUBJT1FrameOfReference = "1.2.840.10008.1.4.1.18";

		/// <summary>Well-known frame of reference: ICBM 452 T1 Frame of Reference 
		/// </summary>
		public const String ICBM452T1FrameOfReference = "1.2.840.10008.1.4.2.1";

		/// <summary>Well-known frame of reference: ICBM Single Subject MRI Frame of Reference 
		/// </summary>
		public const String ICBMSingleSubjectMRIFrameOfReference = "1.2.840.10008.1.4.2.2";

		/// <summary>Service Class: Storage Service Class 
		/// </summary>
		public const String StorageServiceClass = "1.2.840.10008.4.2";

		/// <summary>Service Class: Unified Worklist and Procedure Step Service Class 
		/// </summary>
		public const String UnifiedWorklistAndProcedureStepServiceClass = "1.2.840.10008.5.1.4.34.4";
		
		/// <summary>ApplicationContextName: DICOM Application Context Name 
		/// </summary>
		public const String DICOMApplicationContextName = "1.2.840.10008.3.1.1.1";

		/// <summary>ApplicationContextName: DICOM UID Registry Coding Scheme
		/// </summary>
		public const String DICOMUIDRegistryCodingScheme = "1.2.840.100082.6.1";
		
		/// <summary>ApplicationContextName: DICOM Controlled Terminology Coding Scheme
		/// </summary>
		public const String DICOMControlledTerminologyCodingScheme = "1.2.840.10008.2.16.4";

		/// <summary>LDAP OID: dicomDeviceName
		/// </summary>
		public const String dicomDeviceNameLDAPOID = "1.2.840.10008.15.0.3.1";

		/// <summary>LDAP OID: dicomDescription
		/// </summary>
		public const String dicomDescriptionLDAPOID = "1.2.840.10008.15.0.3.2";

		/// <summary>LDAP OID: dicomManufacturer
		/// </summary>
		public const String dicomManufacturerLDAPOID = "1.2.840.10008.15.0.3.3";

		/// <summary>LDAP OID: dicomManufacturerModelName
		/// </summary>
		public const String dicomManufacturerModelNameLDAPOID = "1.2.840.10008.15.0.3.4";

		/// <summary>LDAP OID: dicomSoftwareVersion
		/// </summary>
		public const String dicomSoftwareVersionLDAPOID = "1.2.840.10008.15.0.3.5";

		/// <summary>LDAP OID: dicomVendorData
		/// </summary>
		public const String dicomVendorDataLDAPOID = "1.2.840.10008.15.0.3.6";

		/// <summary>LDAP OID: dicomAETitle
		/// </summary>
		public const String dicomAETitleLDAPOID = "1.2.840.10008.15.0.3.7";

		/// <summary>LDAP OID: dicomNetworkConnectionReference
		/// </summary>
		public const String dicomNetworkConnectionReferenceLDAPOID = "1.2.840.10008.15.0.3.8";

		/// <summary>LDAP OID: dicomApplicationCluster
		/// </summary>
		public const String dicomApplicationClusterLDAPOID = "1.2.840.10008.15.0.3.9";

		/// <summary>LDAP OID: dicomAssociationInitiator
		/// </summary>
		public const String dicomAssociationInitiatorLDAPOID = "1.2.840.10008.15.0.3.10";

		/// <summary>LDAP OID: dicomAssociationAcceptor
		/// </summary>
		public const String dicomAssociationAcceptorLDAPOID = "1.2.840.10008.15.0.3.11";

		/// <summary>LDAP OID: dicomHostname
		/// </summary>
		public const String dicomHostnameLDAPOID = "1.2.840.10008.15.0.3.12";

		/// <summary>LDAP OID: dicomPort
		/// </summary>
		public const String dicomPortLDAPOID = "1.2.840.10008.15.0.3.13";

		/// <summary>LDAP OID: dicomSOPClass
		/// </summary>
		public const String dicomSOPClassLDAPOID = "1.2.840.10008.15.0.3.14";

		/// <summary>LDAP OID: dicomTransferRole
		/// </summary>
		public const String dicomTransferRoleLDAPOID = "1.2.840.10008.15.0.3.15";

		/// <summary>LDAP OID: dicomTransferSyntax
		/// </summary>
		public const String dicomTransferSyntaxLDAPOID = "1.2.840.10008.15.0.3.16";

		/// <summary>LDAP OID: dicomPrimairyDeviceType
		/// </summary>
		public const String dicomPrimairyDeviceTypeLDAPOID = "1.2.840.10008.15.0.3.17";

		/// <summary>LDAP OID: dicomRelatedDeviceReference
		/// </summary>
		public const String dicomRelatedDeviceReferenceLDAPOID = "1.2.840.10008.15.0.3.18";

		/// <summary>LDAP OID: dicomPreferredCalledAETitle
		/// </summary>
		public const String dicomPreferredCalledAETitleLDAPOID = "1.2.840.10008.15.0.3.19";

		/// <summary>LDAP OID: dicomTLSCyphersuite
		/// </summary>
		public const String dicomTLSCyphersuiteLDAPOID = "1.2.840.10008.15.0.3.20";

		/// <summary>LDAP OID: dicomAuthorizedNodeCertificateReference
		/// </summary>
		public const String dicomAuthorizedNodeCertifcateReferenceLDAPOID = "1.2.840.10008.15.0.3.21";

		/// <summary>LDAP OID: dicomThisNodeCertificateReference
		/// </summary>
		public const String dicomThisNodeCertificateReferenceLDAPOID = "1.2.840.10008.15.0.3.22";

		/// <summary>LDAP OID: dicomInstalled
		/// </summary>
		public const String dicomInstalledLDAPOID = "1.2.840.10008.15.0.3.23";

		/// <summary>LDAP OID: dicomStationName
		/// </summary>
		public const String dicomStationNameLDAPOID = "1.2.840.10008.15.0.3.24";

		/// <summary>LDAP OID: dicomDeviceSerialNumber
		/// </summary>
		public const String dicomDeviceSerialNumberLDAPOID = "1.2.840.10008.15.0.3.25";

		/// <summary>LDAP OID: dicomInstitutionName
		/// </summary>
		public const String dicomInstitutionNameLDAPOID = "1.2.840.10008.15.0.3.26";

		/// <summary>LDAP OID: dicomInstitutionAddress
		/// </summary>
		public const String dicomInstitutionAddressLDAPOID = "1.2.840.10008.15.0.3.27";

		/// <summary>LDAP OID: dicomInstitutionDepartmentName
		/// </summary>
		public const String dicomInstitutionDepartmentNameLDAPOID = "1.2.840.10008.15.0.3.28";

		/// <summary>LDAP OID: dicomIssuerOfPatientID
		/// </summary>
		public const String dicomIssuerOfPatientIDLDAPOID = "1.2.840.10008.15.0.3.29";

		/// <summary>LDAP OID: dicomPreferredCallingAETitle
		/// </summary>
		public const String dicomPreferredCallingAETitleLDAPOID = "1.2.840.10008.15.0.3.30";

		/// <summary>LDAP OID: dicomSupportedCharacterSet
		/// </summary>
		public const String dicomSupportedCharacterSetLDAPOID = "1.2.840.10008.15.0.3.31";

		/// <summary>LDAP OID: dicomConfigurationRoot
		/// </summary>
		public const String dicomConfigurationRootLDAPOID = "1.2.840.10008.15.0.4.1";

		/// <summary>LDAP OID: dicomDevicesRoot
		/// </summary>
		public const String dicomDevicesRootLDAPOID = "1.2.840.10008.15.0.4.2";

		/// <summary>LDAP OID: dicomUniqueAETitlesRegistryRoot
		/// </summary>
		public const String dicomUniqueAETitleRegistryRootLDAPOID = "1.2.840.10008.15.0.4.3";

		/// <summary>LDAP OID: dicomDevice
		/// </summary>
		public const String dicomDeviceLDAPOID = "1.2.840.10008.15.0.4.4";

		/// <summary>LDAP OID: dicomNetworkAE
		/// </summary>
		public const String dicomNetworkAELDAPOID = "1.2.840.10008.15.0.4.5";

		/// <summary>LDAP OID: dicomNetworkConnection
		/// </summary>
		public const String dicomNetworkConnectionLDAPOID = "1.2.840.10008.15.0.4.6";

		/// <summary>LDAP OID: dicomUniqueAETitle
		/// </summary>
		public const String dicomUniqueAETitleLDAPOID = "1.2.840.10008.15.0.4.7";

		/// <summary>LDAP OID: dicomTransferCapability
		/// </summary>
		public const String dicomTransferCapabilityLDAPOID = "1.2.840.10008.15.0.4.8";
		
		public static void Main()
		{
			try
			{
				//Console.WriteLine( "PrintQueueSOPInstance=" + UIDs.GetUID( "PrintQueueSOPInstance1" ) );
				Console.WriteLine( "1.2.840.10008.1.1=" + UIDs.GetName( "1.2.840.10008.1.1" ) );
			}
			catch( Exception e )
			{
				Console.WriteLine( e );
				Console.WriteLine( "Error!!!!!!!!!!!!!" );
			}
		}
	}
}