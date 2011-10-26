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
// List of Tags update by Marcel de Wijs.
#endregion

namespace org.dicomcs.dict
{
	using System;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	
	/// <summary>
	/// Provides tag constants.
	/// </summary>
	public class Tags
	{		
		private static Hashtable s_dict = new Hashtable( 1853 );

		static Tags()
		{	
			FieldInfo[] fields = typeof(Tags).GetFields( BindingFlags.Public | BindingFlags.Static ); 
			foreach ( FieldInfo field in fields )
			{
				String name = field.Name; 
				uint tag = (uint)field.GetValue( null );
				s_dict.Add( tag, name );
			}
		}

		/// <summary>
		/// Private constructor 
		/// </summary>
		private Tags()
		{
		}
		
		public static bool IsPrivate(uint tag)
		{
			return (tag & 0x00010000) != 0;
		}
		
		public static uint ValueOf(short grTag, short elTag)
		{
			return (uint) ((grTag << 16) | (elTag & 0xffff));
		}
		
		/// <summary>
		/// Get the description name of this Tag
		/// </summary>
		/// <param name="tag">the tag</param>
		/// <returns></returns>
		public static String GetName( uint tag )
		{
			if( IsPrivate(tag) )
				return "UnknownTag";			// Private Tag

			if( (tag & 0x0000FFFF) == 0 )
				tag = 0x00000000;				// Group Length			

			if( s_dict.Contains( tag ) )
				return (String) s_dict[tag];	// DICOM 3.0 Tags
			else
				throw new ArgumentException("Unkown Tag: " + ToHexStringWithoutTagName(tag));
		}

		/// <summary>
		/// Get the tag for this description name
		/// </summary>
		/// <param name="name">the description name</param>
		/// <returns></returns>
		public static uint GetTag( String name )
		{
			try
			{
				return (uint) typeof(Tags).GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);
			}
			catch ( Exception e)
			{
				throw new ArgumentException("Unkown Tag Name: " + name);
			}
		}

		public static String ToHexString(uint tag)
		{
			return String.Format("({0:x4},{1:x4})", tag >> 16, tag & 0xFFFF ) + " " + GetName(tag);
		}

        public static String ToHexStringWithoutTagName(uint tag)
        {
            return String.Format("({0:x4},{1:x4})", tag >> 16, tag & 0xFFFF);
        }
        
		/*
		public static String ToHexString( short v, short l)
		{
			return ToHexString( ValueOf( v, l ) );
		}*/
				

		
		/// <summary>(0000,0001) VR=UN (Command) Length to End (Retired) 
		/// </summary>
		public const uint CommandLengthToEndRetired = 0x00000001;
		
		/// <summary>(0000,0010) VR=UN (Command) Recognition Code (Retired) 
		/// </summary>
		public const uint CommandRecognitionCodeRetired = 0x00000010;
		
		/// <summary>(0000,0002) VR=UI Affected SOP Class uid 
		/// </summary>
		public const uint AffectedSOPClassUID = 0x00000002;
		
		/// <summary>(0000,0003) VR=UI Requested SOP Class uid 
		/// </summary>
		public const uint RequestedSOPClassUID = 0x00000003;
		
		/// <summary>(0000,0100) VR=US Command Field 
		/// </summary>
		public const uint CommandField = 0x00000100;
		
		/// <summary>(0000,0110) VR=US Message ID 
		/// </summary>
		public const uint MessageID = 0x00000110;
		
		/// <summary>(0000,0120) VR=US Message ID Being Responded To 
		/// </summary>
		public const uint MessageIDToBeingRespondedTo = 0x00000120;
		
		/// <summary>(0000,0200) VR=UN Initiator (Retired) 
		/// </summary>
		public const uint InitiatorRetired = 0x00000200;
		
		/// <summary>(0000,0300) VR=UN Receiver (Retired) 
		/// </summary>
		public const uint ReceiverRetired = 0x00000300;
		
		/// <summary>
		/// (0000,0400) VR=UN Find Location (Retired) 
		/// </summary>
		public const uint FindLocationRetired = 0x00000400;
		
		/// <summary>(0000,0600) VR=AE Move Destination 
		/// </summary>
		public const uint MoveDestination = 0x00000600;
		
		/// <summary>(0000,0700) VR=US Priority 
		/// </summary>
		public const uint Priority = 0x00000700;
		
		/// <summary>(0000,0800) VR=US Data Set type 
		/// </summary>
		public const uint DataSetType = 0x00000800;
		
		/// <summary>(0000,0850) VR=UN Number of Matches (Retired) 
		/// </summary>
		public const uint NumberOfMatchesRetired = 0x00000850;
		
		/// <summary>(0000,0860) VR=UN Response Sequence Number (Retired) 
		/// </summary>
		public const uint ResponseSequenceNumberRetired = 0x00000860;
		
		/// <summary>(0000,0900) VR=US Status 
		/// </summary>
		public const uint Status = 0x00000900;
		
		/// <summary>(0000,0901) VR=AT Offending element 
		/// </summary>
		public const uint OffendingElement = 0x00000901;
		
		/// <summary>(0000,0902) VR=LO Error Comment 
		/// </summary>
		public const uint ErrorComment = 0x00000902;
		
		/// <summary>(0000,0903) VR=US Error ID 
		/// </summary>
		public const uint ErrorID = 0x00000903;
		
		/// <summary>(0000,1000) VR=UI Affected SOP Instance uid 
		/// </summary>
		public const uint AffectedSOPInstanceUID = 0x00001000;
		
		/// <summary>(0000,1001) VR=UI Requested SOP Instance uid 
		/// </summary>
		public const uint RequestedSOPInstanceUID = 0x00001001;
		
		/// <summary>(0000,1002) VR=US Event type ID 
		/// </summary>
		public const uint EventTypeID = 0x00001002;
		
		/// <summary>(0000,1005) VR=AT Attribute Identifier List 
		/// </summary>
		public const uint AttributeIdentifierList = 0x00001005;
		
		/// <summary>(0000,1008) VR=US Action type ID 
		/// </summary>
		public const uint ActionTypeID = 0x00001008;
		
		/// <summary>(0000,1020) VR=US Number of Remaining Sub-operations 
		/// </summary>
		public const uint NumberOfRemainingSubOperations = 0x00001020;
		
		/// <summary>(0000,1021) VR=US Number of Completed Sub-operations 
		/// </summary>
		public const uint NumberOfCompletedSubOperations = 0x00001021;
		
		/// <summary>(0000,1022) VR=US Number of Failed Sub-operations 
		/// </summary>
		public const uint NumberOfFailedSubOperations = 0x00001022;
		
		/// <summary>(0000,1023) VR=US Number of Warning Sub-operations 
		/// </summary>
		public const uint NumberOfWarningSubOperations = 0x00001023;
		
		/// <summary>(0000,1030) VR=AE Move Originator Application Entity Title 
		/// </summary>
		public const uint MoveOriginatorAET = 0x00001030;
		
		/// <summary>(0000,1031) VR=US Move Originator Message ID 
		/// </summary>
		public const uint MoveOriginatorMessageID = 0x00001031;
		
		/// <summary>(0000,4000) VR=UN Dialog Receiver (Retired) 
		/// </summary>
		public const uint DialogReceiverRetired = 0x00004000;
		
		/// <summary>(0000,4010) VR=UN Terminal Type (Retired) 
		/// </summary>
		public const uint TerminalTypeRetired = 0x00004010;
		
		/// <summary>(0000,5010) VR=UN Message Set ID (Retired) 
		/// </summary>
		public const uint MessageSetIDRetired = 0x00005010;
		
		/// <summary>(0000,5020) VR=UN End Message ID (Retired) 
		/// </summary>
		public const uint EndMessageIDRetired = 0x00005020;
		
		/// <summary>(0000,5110) VR=UN Display Format (Retired) 
		/// </summary>
		public const uint DisplayFormatRetired = 0x00005110;
		
		/// <summary>(0000,5120) VR=UN Page Position ID (Retired) 
		/// </summary>
		public const uint PagePositionIDRetired = 0x00005120;
		
		/// <summary>(0000,5130) VR=UN Text Format ID (Retired) 
		/// </summary>
		public const uint TextFormatIDRetired = 0x00005130;
		
		/// <summary>(0000,5140) VR=UN Nor/Rev (Retired) 
		/// </summary>
		public const uint NorRevRetired = 0x00005140;
		
		/// <summary>(0000,5150) VR=UN Add Gray Scale (Retired) 
		/// </summary>
		public const uint AddGrayScaleRetired = 0x00005150;
		
		/// <summary>(0000,5160) VR=UN Borders (Retired) 
		/// </summary>
		public const uint BordersRetired = 0x00005160;
		
		/// <summary>(0000,5170) VR=UN Copies (Retired) 
		/// </summary>
		public const uint CopiesRetired = 0x00005170;
		
		/// <summary>(0000,5180) VR=UN Magnification Type (Retired) 
		/// </summary>
		public const uint MagnificationTypeRetired = 0x00005180;
		
		/// <summary>(0000,5190) VR=UN Erase (Retired) 
		/// </summary>
		public const uint EraseRetired = 0x00005190;
		
		/// <summary>(0000,51A0) VR=UN Print (Retired) 
		/// </summary>
		public const uint PrintRetired = 0x000051A0;
		
		/// <summary>(0002,0001) VR=OB File Meta Information Version 
		/// </summary>
		public const uint FileMetaInformationVersion = 0x00020001;
		
		/// <summary>(0002,0002) VR=UI Media Storage SOP Class UID 
		/// </summary>
		public const uint MediaStorageSOPClassUID = 0x00020002;
		
		/// <summary>(0002,0003) VR=UI Media Storage SOP Instance UID 
		/// </summary>
		public const uint MediaStorageSOPInstanceUID = 0x00020003;
		
		/// <summary>(0002,0010) VR=UI Transfer Syntax UID 
		/// </summary>
		public const uint TransferSyntaxUID = 0x00020010;
		
		/// <summary>(0002,0012) VR=UI Implementation Class UID 
		/// </summary>
		public const uint ImplementationClassUID = 0x00020012;
		
		/// <summary>(0002,0013) VR=SH Implementation Version Name 
		/// </summary>
		public const uint ImplementationVersionName = 0x00020013;
		
		/// <summary>(0002,0016) VR=AE Source Application Entity Title 
		/// </summary>
		public const uint SourceApplicationEntityTitle = 0x00020016;
		
		/// <summary>(0002,0100) VR=UI Private Information Creator UID 
		/// </summary>
		public const uint PrivateInformationCreatorUID = 0x00020100;
		
		/// <summary>(0002,0102) VR=OB Private Information 
		/// </summary>
		public const uint PrivateInformation = 0x00020102;
		
		/// <summary>(0004,1130) VR=CS File-set ID 
		/// </summary>
		public const uint FileSetID = 0x00041130;
		
		/// <summary>(0004,1141) VR=CS File-set Descriptor File ID 
		/// </summary>
		public const uint FileSetDescriptorFileID = 0x00041141;
		
		/// <summary>(0004,1142) VR=CS Specific Character Set of File-set Descriptor File 
		/// </summary>
		public const uint SpecificCharacterSetOfFileSetDescriptorFile = 0x00041142;
		
		/// <summary>(0004,1200) VR=UL Offset of the First Directory Record of the Root Directory Entity 
		/// </summary>
		public const uint OffsetOfFirstRootDirectoryRecord = 0x00041200;
		
		/// <summary>(0004,1202) VR=UL Offset of the Last Directory Record of the Root Directory Entity 
		/// </summary>
		public const uint OffsetOfLastRootDirectoryRecord = 0x00041202;
		
		/// <summary>(0004,1212) VR=US File-set Consistency Flag 
		/// </summary>
		public const uint FileSetConsistencyFlag = 0x00041212;
		
		/// <summary>(0004,1220) VR=SQ Directory Record Sequence 
		/// </summary>
		public const uint DirectoryRecordSeq = 0x00041220;
		
		/// <summary>(0004,1400) VR=UL Offset of the Next Directory Record 
		/// </summary>
		public const uint OffsetOfNextDirectoryRecord = 0x00041400;
		
		/// <summary>(0004,1410) VR=US Record In-use Flag 
		/// </summary>
		public const uint RecordInUseFlag = 0x00041410;
		
		/// <summary>(0004,1420) VR=UL Offset of Referenced Lower-Level Directory Entity 
		/// </summary>
		public const uint OffsetOfLowerLevelDirectoryEntity = 0x00041420;
		
		/// <summary>(0004,1430) VR=CS Directory Record Type 
		/// </summary>
		public const uint DirectoryRecordType = 0x00041430;
		
		/// <summary>(0004,1432) VR=UI Private Record UID 
		/// </summary>
		public const uint PrivateRecordUID = 0x00041432;
		
		/// <summary>(0004,1500) VR=CS Referenced File ID 
		/// </summary>
		public const uint RefFileID = 0x00041500;
		
		/// <summary>(0004,1504) VR=UL MRDR Directory Record Offset 
		/// </summary>
		public const uint MRDRDirectoryRecordOffset = 0x00041504;
		
		/// <summary>(0004,1510) VR=UI Referenced SOP Class UID in File 
		/// </summary>
		public const uint RefSOPClassUIDInFile = 0x00041510;
		
		/// <summary>(0004,1511) VR=UI Referenced SOP Instance UID in File 
		/// </summary>
		public const uint RefSOPInstanceUIDInFile = 0x00041511;
		
		/// <summary>(0004,1512) VR=UI Referenced SOP Transfer Syntax UID in File 
		/// </summary>
		public const uint RefSOPTransferSyntaxUIDInFile = 0x00041512;
		
		/// <summary>(0004,1600) VR=UL Number of References 
		/// </summary>
		public const uint NumberOfReferences = 0x00041600;
		
		/// <summary>(xxxx,0000) VR=UL Group Length 
		/// </summary>
		public const uint GroupLength = 0x00000000;
		
		/// <summary>(0008,0001) VR=UN Length to End (Retired) 
		/// </summary>
		public const uint LengthToEndRetired = 0x00080001;
		
		/// <summary>(0008,0005) VR=CS Specific Character Set 
		/// </summary>
		public const uint SpecificCharacterSet = 0x00080005;
		
		/// <summary>(0008,0008) VR=CS Image Type 
		/// </summary>
		public const uint ImageType = 0x00080008;
		
		/// <summary>(0008,0010) VR=UN Recognition Code (Retired) 
		/// </summary>
		public const uint RecognitionCodeRetired = 0x00080010;
		
		/// <summary>(0008,0012) VR=DA Instance Creation Date 
		/// </summary>
		public const uint InstanceCreationDate = 0x00080012;
		
		/// <summary>(0008,0013) VR=TM Instance Creation Time 
		/// </summary>
		public const uint InstanceCreationTime = 0x00080013;
		
		/// <summary>(0008,0014) VR=UI Instance Creator UID 
		/// </summary>
		public const uint InstanceCreatorUID = 0x00080014;
		
		/// <summary>(0008,0016) VR=UI SOP Class UID 
		/// </summary>
		public const uint SOPClassUID = 0x00080016;
		
		/// <summary>(0008,0018) VR=UI SOP Instance UID 
		/// </summary>
		public const uint SOPInstanceUID = 0x00080018;
		
		/// <summary>(0008,0020) VR=DA Study Date 
		/// </summary>
		public const uint StudyDate = 0x00080020;
		
		/// <summary>(0008,0021) VR=DA Series Date 
		/// </summary>
		public const uint SeriesDate = 0x00080021;
		
		/// <summary>(0008,0022) VR=DA Acquisition Date 
		/// </summary>
		public const uint AcquisitionDate = 0x00080022;
		
		/// <summary>(0008,0023) VR=DA Content Date 
		/// </summary>
		public const uint ContentDate = 0x00080023;
		
		/// <summary>(0008,0024) VR=DA Overlay Date 
		/// </summary>
		public const uint OverlayDate = 0x00080024;
		
		/// <summary>(0008,0025) VR=DA Curve Date 
		/// </summary>
		public const uint CurveDate = 0x00080025;
		
		/// <summary>(0008,002A) VR=DT Acquisition Datetime 
		/// </summary>
		public const uint AcquisitionDatetime = 0x0008002A;
		
		/// <summary>(0008,0030) VR=TM Study Time 
		/// </summary>
		public const uint StudyTime = 0x00080030;
		
		/// <summary>(0008,0031) VR=TM Series Time 
		/// </summary>
		public const uint SeriesTime = 0x00080031;
		
		/// <summary>(0008,0032) VR=TM Acquisition Time 
		/// </summary>
		public const uint AcquisitionTime = 0x00080032;
		
		/// <summary>(0008,0033) VR=TM Content Time 
		/// </summary>
		public const uint ContentTime = 0x00080033;
		
		/// <summary>(0008,0034) VR=TM Overlay Time 
		/// </summary>
		public const uint OverlayTime = 0x00080034;
		
		/// <summary>(0008,0035) VR=TM Curve Time 
		/// </summary>
		public const uint CurveTime = 0x00080035;
		
		/// <summary>(0008,0040) VR=UN Data Set Type (Retired) 
		/// </summary>
		public const uint DataSetTypeRetired = 0x00080040;
		
		/// <summary>(0008,0041) VR=UN Data Set Subtype (Retired) 
		/// </summary>
		public const uint DataSetSubtypeRetired = 0x00080041;
		
		/// <summary>(0008,0042) VR=CS Nuclear Medicine Series Type (Retired) 
		/// </summary>
		public const uint NuclearMedicineSeriesTypeRetired = 0x00080042;
		
		/// <summary>(0008,0050) VR=SH Accession Number 
		/// </summary>
		public const uint AccessionNumber = 0x00080050;
		
		/// <summary>(0008,0052) VR=CS Query/Retrieve Level 
		/// </summary>
		public const uint QueryRetrieveLevel = 0x00080052;
		
		/// <summary>(0008,0054) VR=AE Retrieve AE Title 
		/// </summary>
		public const uint RetrieveAET = 0x00080054;
		
		/// <summary>(0008,0056) VR=CS Instance Availability 
		/// </summary>
		public const uint InstanceAvailability = 0x00080056;
		
		/// <summary>(0008,0058) VR=UI Failed SOP Instance UID List 
		/// </summary>
		public const uint FailedSOPInstanceUIDList = 0x00080058;
		
		/// <summary>(0008,0060) VR=CS Modality 
		/// </summary>
		public const uint Modality = 0x00080060;
		
		/// <summary>(0008,0061) VR=CS Modalities in Study 
		/// </summary>
		public const uint ModalitiesInStudy = 0x00080061;

		/// <summary>(0008,0062) VR=UI SOP Classes in Study 
		/// </summary>
		public const uint SOPClassesInStudy = 0x00080062;
		
		/// <summary>(0008,0064) VR=CS Conversion Type 
		/// </summary>
		public const uint ConversionType = 0x00080064;
		
		/// <summary>(0008,0068) VR=CS Presentation Intent Type 
		/// </summary>
		public const uint PresentationIntentType = 0x00080068;
		
		/// <summary>(0008,0070) VR=LO Manufacturer 
		/// </summary>
		public const uint Manufacturer = 0x00080070;
		
		/// <summary>(0008,0080) VR=LO Institution Name 
		/// </summary>
		public const uint InstitutionName = 0x00080080;
		
		/// <summary>(0008,0081) VR=ST Institution Address 
		/// </summary>
		public const uint InstitutionAddress = 0x00080081;
		
		/// <summary>(0008,0082) VR=SQ Institution Code Sequence 
		/// </summary>
		public const uint InstitutionCodeSeq = 0x00080082;
		
		/// <summary>(0008,0090) VR=PN Referring Physician's Name 
		/// </summary>
		public const uint ReferringPhysicianName = 0x00080090;
		
		/// <summary>(0008,0092) VR=ST Referring Physician's Address 
		/// </summary>
		public const uint ReferringPhysicianAddress = 0x00080092;
		
		/// <summary>(0008,0094) VR=SH Referring Physician's Telephone Numbers 
		/// </summary>
		public const uint ReferringPhysicianPhoneNumbers = 0x00080094;
		
		/// <summary>(0008,0096) VR=SQ Referring Physician's Indentifcation Sequence
		/// </summary>
        public const uint ReferringPhysicianIdentificationSequence = 0x00080096;

		/// <summary>(0008,0100) VR=SH Code Value 
		/// </summary>
		public const uint CodeValue = 0x00080100;
		
		/// <summary>(0008,0102) VR=SH Coding Scheme Designator 
		/// </summary>
		public const uint CodingSchemeDesignator = 0x00080102;
		
		/// <summary>(0008,0103) VR=SH Coding Scheme Version 
		/// </summary>
		public const uint CodingSchemeVersion = 0x00080103;
		
		/// <summary>(0008,0104) VR=LO Code Meaning 
		/// </summary>
		public const uint CodeMeaning = 0x00080104;
		
		/// <summary>(0008,0105) VR=CS Mapping Resource 
		/// </summary>
		public const uint MappingResource = 0x00080105;
		
		/// <summary>(0008,0106) VR=DT Context Group Version 
		/// </summary>
		public const uint ContextGroupVersion = 0x00080106;
		
		/// <summary>(0008,0107) VR=DT Context Group Local Version 
		/// </summary>
		public const uint ContextGroupLocalVersion = 0x00080107;
		
		/// <summary>(0008,010B) VR=CS Code Set Extension Flag 
		/// </summary>
		public const uint CodeSetExtensionFlag = 0x0008010B;
		
		/// <summary>(0008,010C) VR=UI Private Coding Scheme Creator UID 
		/// </summary>
		public const uint PrivateCodingSchemeCreatorUID = 0x0008010C;
		
		/// <summary>(0008,010D) VR=UI Code Set Extension Creator UID 
		/// </summary>
		public const uint CodeSetExtensionCreatorUID = 0x0008010D;
		
		/// <summary>(0008,010F) VR=CS Context Identifier 
		/// </summary>
		public const uint ContextIdentifier = 0x0008010F;

        /// <summary>(0008,0110) VR=SQ Coding Scheme Identification Sequence
		/// </summary>
        public const uint CodingSchemeIdentificationSeq = 0x00080110;
		
		/// <summary>(0008,0201) VR=SH Timezone Offset From UTC 
		/// </summary>
		public const uint TimezoneOffsetFromUTC = 0x00080201;
		
		/// <summary>(0008,1000) VR=UN Network ID (Retired) 
		/// </summary>
		public const uint NetworkIDRetired = 0x00081000;
		
		/// <summary>(0008,1010) VR=SH Station Name 
		/// </summary>
		public const uint StationName = 0x00081010;
		
		/// <summary>(0008,1030) VR=LO Study Description 
		/// </summary>
		public const uint StudyDescription = 0x00081030;
		
		/// <summary>(0008,1032) VR=SQ Procedure Code Sequence 
		/// </summary>
		public const uint ProcedureCodeSeq = 0x00081032;
		
		/// <summary>(0008,103E) VR=LO Series Description 
		/// </summary>
		public const uint SeriesDescription = 0x0008103E;
		
		/// <summary>(0008,1040) VR=LO Institutional Department Name 
		/// </summary>
		public const uint InstitutionalDepartmentName = 0x00081040;
		
		/// <summary>(0008,1048) VR=PN Physician(s) of Record 
		/// </summary>
		public const uint PhysicianOfRecord = 0x00081048;
		
		/// <summary>(0008,1049) VR=SQ Physician(s) of Record Identification Sequence
		/// </summary>
        public const uint PhysiciansofRecordIdentificationSeq=0x00081049;

		/// <summary>(0008,1050) VR=PN Performing Physician's Name 
		/// </summary>
		public const uint PerformingPhysicianName = 0x00081050;

		/// <summary>(0008,1052) VR=SQ Performing Physician Identification Sequence 
		/// </summary>
        public const uint PerformingPhysicianIdentificationSeq = 0x00081052;
		
		/// <summary>(0008,1060) VR=PN Name of Physician(s) Reading Study 
		/// </summary>
		public const uint NameOfPhysicianReadingStudy = 0x00081060;
		
		/// <summary>(0008,1062) VR=SQ Physician(s) Reading Study Identification Sequence 
		/// </summary>
        public const uint PhysiciansReadingStudyIdentificationSeq = 0x00081062;

		/// <summary>(0008,1070) VR=PN Operator's Name 
		/// </summary>
		public const uint OperatorName = 0x00081070;
		
		/// <summary>(0008,1072) VR=SQ Operator Identification Sequence
		/// </summary>
        public const uint OperatorIdentificationSeq = 0x00081072;
		
         /// <summary>(0008,1080) VR=LO Admitting Diagnosis Description 
		/// </summary>
		public const uint AdmittingDiagnosisDescription = 0x00081080;
		
		/// <summary>(0008,1084) VR=SQ Admitting Diagnosis Code Sequence 
		/// </summary>
		public const uint AdmittingDiagnosisCodeSeq = 0x00081084;
		
		/// <summary>(0008,1090) VR=LO Manufacturer's Model Name 
		/// </summary>
		public const uint ManufacturerModelName = 0x00081090;
		
		/// <summary>(0008,1100) VR=SQ Referenced Results Sequence 
		/// </summary>
		public const uint RefResultsSeq = 0x00081100;
		
		/// <summary>(0008,1110) VR=SQ Referenced Study Sequence 
		/// </summary>
		public const uint RefStudySeq = 0x00081110;
		
		/// <summary>(0008,1111) VR=SQ Referenced Study Component Sequence 
		/// </summary>
		public const uint RefStudyComponentSeq = 0x00081111;
		
		/// <summary>(0008,1115) VR=SQ Referenced Series Sequence 
		/// </summary>
		public const uint RefSeriesSeq = 0x00081115;
		
		/// <summary>(0008,1120) VR=SQ Referenced Patient Sequence 
		/// </summary>
		public const uint RefPatientSeq = 0x00081120;
		
		/// <summary>(0008,1125) VR=SQ Referenced Visit Sequence 
		/// </summary>
		public const uint RefVisitSeq = 0x00081125;
		
		/// <summary>(0008,1130) VR=SQ Referenced Overlay Sequence 
		/// </summary>
		public const uint RefOverlaySeq = 0x00081130;

		/// <summary>(0008,113A) VR=SQ Referenced Waveform Sequence
		/// </summary>
        public const uint ReferencedWaveformSeq=0x0008113A;
		
		/// <summary>(0008,1140) VR=SQ Referenced Image Sequence 
		/// </summary>
		public const uint RefImageSeq = 0x00081140;
		
		/// <summary>(0008,1145) VR=SQ Referenced Curve Sequence 
		/// </summary>
		public const uint RefCurveSeq = 0x00081145;
		
		/// <summary>(0008,114A) VR=SQ Referenced Instance Sequence 
		/// </summary>
		public const uint RefInstanceSeq = 0x0008114A;
		
		/// <summary>(0008,1150) VR=UI Referenced SOP Class UID 
		/// </summary>
		public const uint RefSOPClassUID = 0x00081150;
		
		/// <summary>(0008,1155) VR=UI Referenced SOP Instance UID 
		/// </summary>
		public const uint RefSOPInstanceUID = 0x00081155;
		
		/// <summary>(0008,115A) VR=UI SOP Classes Supported 
		/// </summary>
		public const uint SOPClassesSupported = 0x0008115A;
		
		/// <summary>(0008,1160) VR=IS Referenced Frame Number 
		/// </summary>
		public const uint RefFrameNumber = 0x00081160;
		
		/// <summary>(0008,1195) VR=UI Transaction UID 
		/// </summary>
		public const uint TransactionUID = 0x00081195;
		
		/// <summary>(0008,1197) VR=US Failure Reason 
		/// </summary>
		public const uint FailureReason = 0x00081197;
		
		/// <summary>(0008,1198) VR=SQ Failed SOP Sequence 
		/// </summary>
		public const uint FailedSOPSeq = 0x00081198;
		
		/// <summary>(0008,1199) VR=SQ Referenced SOP Sequence 
		/// </summary>
		public const uint RefSOPSeq = 0x00081199;
		
		/// <summary>(0008,2110) VR=CS Lossy Image Compression (Retired) 
		/// </summary>
		public const uint LossyImageCompressionRetired = 0x00082110;
		
		/// <summary>(0008,2111) VR=ST Derivation Description 
		/// </summary>
		public const uint DerivationDescription = 0x00082111;
		
		/// <summary>(0008,2112) VR=SQ Source Image Sequence 
		/// </summary>
		public const uint SourceImageSeq = 0x00082112;
		
		/// <summary>(0008,2120) VR=SH Stage Name 
		/// </summary>
		public const uint StageName = 0x00082120;
		
		/// <summary>(0008,2122) VR=IS Stage Number 
		/// </summary>
		public const uint StageNumber = 0x00082122;
		
		/// <summary>(0008,2124) VR=IS Number of Stages 
		/// </summary>
		public const uint NumberOfStages = 0x00082124;
		
		/// <summary>(0008,2128) VR=IS View Number 
		/// </summary>
		public const uint ViewNumber = 0x00082128;
		
		/// <summary>(0008,2129) VR=IS Number of Event Timers 
		/// </summary>
		public const uint NumberOfEventTimers = 0x00082129;
		
		/// <summary>(0008,212A) VR=IS Number of Views in Stage 
		/// </summary>
		public const uint NumberOfViewsInStage = 0x0008212A;
		
		/// <summary>(0008,2130) VR=DS Event Elapsed Time(s) 
		/// </summary>
		public const uint EventElapsedTime = 0x00082130;
		
		/// <summary>(0008,2132) VR=LO Event Timer Name(s) 
		/// </summary>
		public const uint EventTimerName = 0x00082132;
		
		/// <summary>(0008,2142) VR=IS Start Trim 
		/// </summary>
		public const uint StartTrim = 0x00082142;
		
		/// <summary>(0008,2143) VR=IS Stop Trim 
		/// </summary>
		public const uint StopTrim = 0x00082143;
		
		/// <summary>(0008,2144) VR=IS Recommended Display Frame Rate 
		/// </summary>
		public const uint RecommendedDisplayFrameRate = 0x00082144;
		
		/// <summary>(0008,2200) VR=CS Transducer Position (Retired) 
		/// </summary>
		public const uint TransducerPositionRetired = 0x00082200;
		
		/// <summary>(0008,2204) VR=CS Transducer Orientation (Retired) 
		/// </summary>
		public const uint TransducerOrientationRetired = 0x00082204;
		
		/// <summary>(0008,2208) VR=CS Anatomic Structure (Retired) 
		/// </summary>
		public const uint AnatomicStructureRetired = 0x00082208;
		
		/// <summary>(0008,2218) VR=SQ Anatomic Region Sequence 
		/// </summary>
		public const uint AnatomicRegionSeq = 0x00082218;
		
		/// <summary>(0008,2220) VR=SQ Anatomic Region Modifier Sequence 
		/// </summary>
		public const uint AnatomicRegionModifierSeq = 0x00082220;
		
		/// <summary>(0008,2228) VR=SQ Primary Anatomic Structure Sequence 
		/// </summary>
		public const uint PrimaryAnatomicStructureSeq = 0x00082228;
		
		/// <summary>(0008,2229) VR=SQ Anatomic Structure, Space or Region Sequence 
		/// </summary>
		public const uint AnatomicStructureSpaceRegionSeq = 0x00082229;
		
		/// <summary>(0008,2230) VR=SQ Primary Anatomic Structure Modifier Sequence 
		/// </summary>
		public const uint PrimaryAnatomicStructureModifierSeq = 0x00082230;
		
		/// <summary>(0008,2240) VR=SQ Transducer Position Sequence 
		/// </summary>
		public const uint TransducerPositionSeq = 0x00082240;
		
		/// <summary>(0008,2242) VR=SQ Transducer Position Modifier Sequence 
		/// </summary>
		public const uint TransducerPositionModifierSeq = 0x00082242;
		
		/// <summary>(0008,2244) VR=SQ Transducer Orientation Sequence 
		/// </summary>
		public const uint TransducerOrientationSeq = 0x00082244;
		
		/// <summary>(0008,2246) VR=SQ Transducer Orientation Modifier Sequence 
		/// </summary>
		public const uint TransducerOrientationModifierSeq = 0x00082246;
		
		/// <summary>(0008,4000) VR=LT (Study) Comments (Retired) 
		/// </summary>
		public const uint StudyCommentsRetired = 0x00084000;
		
		/// <summary>(0008,9007) VR=CS Frame Type 
		/// </summary>
		public const uint FrameType = 0x00089007;
		
		/// <summary>(0008,9121) VR=SQ Referenced Raw Data Sequence 
		/// </summary>
		public const uint RefRawDataSeq = 0x00089121;
		
		/// <summary>(0008,9123) VR=UI Creator-Version UID 
		/// </summary>
		public const uint CreatorVersionUID = 0x00089123;
		
		/// <summary>(0008,9124) VR=SQ Derivation Image Sequence 
		/// </summary>
		public const uint DerivationImageSeq = 0x00089124;
		
		/// <summary>(0008,9092) VR=SQ Referring Image Evidence Sequence 
		/// </summary>
		public const uint ReferringImageEvidenceSeq = 0x00089092;
		
		/// <summary>(0008,9154) VR=SQ Source Image Evidence Sequence 
		/// </summary>
		public const uint SourceImageEvidenceSeq = 0x00089154;
		
		/// <summary>(0008,9205) VR=CS Pixel Presentation 
		/// </summary>
		public const uint PixelPresentation = 0x00089205;
		
		/// <summary>(0008,9206) VR=UN Volumetric Properties CS 
		/// </summary>
		public const uint VolumetricProperties = 0x00089206;
		
		/// <summary>(0008,9207) VR=CS Volume Based Calculation Technique 
		/// </summary>
		public const uint VolumeBasedCalculationTechnique = 0x00089207;
		
		/// <summary>(0008,9208) VR=CS Complex Image Component 
		/// </summary>
		public const uint ComplexImageComponent = 0x00089208;
		
		/// <summary>(0008,9209) VR=CS Acquisition Contrast 
		/// </summary>
		public const uint AcquisitionContrast = 0x00089209;
		
		/// <summary>(0008,9215) VR=SQ Derivation Code Sequence 
		/// </summary>
		public const uint DerivationCodeSeq = 0x00089215;
		
		/// <summary>(0008,9237) VR=SQ Referenced Grayscale Presentation State Sequence 
		/// </summary>
		public const uint RefGrayscalePresentationStateSeq = 0x00089237;
		
		/// <summary>(0010,0010) VR=PN Patient's Name 
		/// </summary>
		public const uint PatientName = 0x00100010;
		
		/// <summary>(0010,0020) VR=LO Patient ID 
		/// </summary>
		public const uint PatientID = 0x00100020;
		
		/// <summary>(0010,0021) VR=LO Issuer of Patient ID 
		/// </summary>
		public const uint IssuerOfPatientID = 0x00100021;
		
		/// <summary>(0010,0030) VR=DA Patient's Birth Date 
		/// </summary>
		public const uint PatientBirthDate = 0x00100030;
		
		/// <summary>(0010,0032) VR=TM Patient's Birth Time 
		/// </summary>
		public const uint PatientBirthTime = 0x00100032;
		
		/// <summary>(0010,0040) VR=CS Patient's Sex 
		/// </summary>
		public const uint PatientSex = 0x00100040;
		
		/// <summary>(0010,0050) VR=SQ Patient's Insurance Plan Code Sequence 
		/// </summary>
		public const uint PatientInsurancePlanCodeSeq = 0x00100050;
		
		/// <summary>(0010,1000) VR=LO Other Patient IDs 
		/// </summary>
		public const uint OtherPatientIDs = 0x00101000;
		
		/// <summary>(0010,1001) VR=PN Other Patient Names 
		/// </summary>
		public const uint OtherPatientNames = 0x00101001;
		
		/// <summary>(0010,1005) VR=PN Patient's Birth Name 
		/// </summary>
		public const uint PatientBirthName = 0x00101005;
		
		/// <summary>(0010,1010) VR=AS Patient's Age 
		/// </summary>
		public const uint PatientAge = 0x00101010;
		
		/// <summary>(0010,1020) VR=DS Patient's Size 
		/// </summary>
		public const uint PatientSize = 0x00101020;
		
		/// <summary>(0010,1030) VR=DS Patient's Weight 
		/// </summary>
		public const uint PatientWeight = 0x00101030;
		
		/// <summary>(0010,1040) VR=LO Patient's Address 
		/// </summary>
		public const uint PatientAddress = 0x00101040;
		
		/// <summary>(0010,1050) VR=LO Insurance Plan Identification (Retired) 
		/// </summary>
		public const uint InsurancePlanIdentificationRetired = 0x00101050;
		
		/// <summary>(0010,1060) VR=PN Patient's Mother's Birth Name 
		/// </summary>
		public const uint PatientMotherBirthName = 0x00101060;
		
		/// <summary>(0010,1080) VR=LO Military Rank 
		/// </summary>
		public const uint MilitaryRank = 0x00101080;
		
		/// <summary>(0010,1081) VR=LO Branch of Service 
		/// </summary>
		public const uint BranchOfService = 0x00101081;
		
		/// <summary>(0010,1090) VR=LO Medical Record Locator 
		/// </summary>
		public const uint MedicalRecordLocator = 0x00101090;
		
		/// <summary>(0010,2000) VR=LO Medical Alerts 
		/// </summary>
		public const uint MedicalAlerts = 0x00102000;
		
		/// <summary>(0010,2110) VR=LO Contrast Allergies 
		/// </summary>
		public const uint ContrastAllergies = 0x00102110;
		
		/// <summary>(0010,2150) VR=LO Country of Residence 
		/// </summary>
		public const uint CountryOfResidence = 0x00102150;
		
		/// <summary>(0010,2152) VR=LO Region of Residence 
		/// </summary>
		public const uint RegionOfResidence = 0x00102152;
		
		/// <summary>(0010,2154) VR=SH Patient's Telephone Numbers 
		/// </summary>
		public const uint PatientPhoneNumbers = 0x00102154;
		
		/// <summary>(0010,2160) VR=SH Ethnic Group 
		/// </summary>
		public const uint EthnicGroup = 0x00102160;
		
		/// <summary>(0010,2180) VR=SH Occupation 
		/// </summary>
		public const uint Occupation = 0x00102180;
		
		/// <summary>(0010,21A0) VR=CS Smoking Status 
		/// </summary>
		public const uint SmokingStatus = 0x001021A0;
		
		/// <summary>(0010,21B0) VR=LT Additional Patient History 
		/// </summary>
		public const uint AdditionalPatientHistory = 0x001021B0;
		
		/// <summary>(0010,21C0) VR=US Pregnancy Status 
		/// </summary>
		public const uint PregnancyStatus = 0x001021C0;
		
		/// <summary>(0010,21D0) VR=DA Last Menstrual Date 
		/// </summary>
		public const uint LastMenstrualDate = 0x001021D0;
		
		/// <summary>(0010,21F0) VR=LO Patient's Religious Preference 
		/// </summary>
		public const uint PatientReligiousPreference = 0x001021F0;
		
		/// <summary>(0010,4000) VR=LT Patient Comments 
		/// </summary>
		public const uint PatientComments = 0x00104000;
		
		/// <summary>(0018,0010) VR=LO Contrast/Bolus Agent 
		/// </summary>
		public const uint ContrastBolusAgent = 0x00180010;
		
		/// <summary>(0018,0012) VR=SQ Contrast/Bolus Agent Sequence 
		/// </summary>
		public const uint ContrastBolusAgentSeq = 0x00180012;
		
		/// <summary>(0018,0014) VR=SQ Contrast/Bolus Administration Route Sequence 
		/// </summary>
		public const uint ContrastBolusAdministrationRouteSeq = 0x00180014;
		
		/// <summary>(0018,0015) VR=CS Body Part Examined 
		/// </summary>
		public const uint BodyPartExamined = 0x00180015;
		
		/// <summary>(0018,0020) VR=CS Scanning Sequence 
		/// </summary>
		public const uint ScanningSeq = 0x00180020;
		
		/// <summary>(0018,0021) VR=CS Seq Variant 
		/// </summary>
		public const uint SeqVariant = 0x00180021;
		
		/// <summary>(0018,0022) VR=CS Scan Options 
		/// </summary>
		public const uint ScanOptions = 0x00180022;
		
		/// <summary>(0018,0023) VR=CS MR Acquisition Type 
		/// </summary>
		public const uint MRAcquisitionType = 0x00180023;
		
		/// <summary>(0018,0024) VR=SH Sequence Name 
		/// </summary>
		public const uint SequenceName = 0x00180024;
		
		/// <summary>(0018,0025) VR=CS Angio Flag 
		/// </summary>
		public const uint AngioFlag = 0x00180025;
		
		/// <summary>(0018,0026) VR=SQ Intervention Drug Information Sequence 
		/// </summary>
		public const uint InterventionDrugInformationSeq = 0x00180026;
		
		/// <summary>(0018,0027) VR=TM Intervention Drug Stop Time 
		/// </summary>
		public const uint InterventionDrugStopTime = 0x00180027;
		
		/// <summary>(0018,0028) VR=DS Intervention Drug Dose 
		/// </summary>
		public const uint InterventionDrugDose = 0x00180028;
		
		/// <summary>(0018,0029) VR=SQ Intervention Drug Code Sequence 
		/// </summary>
		public const uint InterventionDrugCodeSeq = 0x00180029;
		
		/// <summary>(0018,002A) VR=SQ Additional Drug Sequence 
		/// </summary>
		public const uint AdditionalDrugSeq = 0x0018002A;
		
		/// <summary>(0018,0030) VR=LO Radionuclide (Retired) 
		/// </summary>
		public const uint RadionuclideRetired = 0x00180030;
		
		/// <summary>(0018,0031) VR=LO Radiopharmaceutical 
		/// </summary>
		public const uint Radiopharmaceutical = 0x00180031;
		
		/// <summary>(0018,0032) VR=DS Energy Window Centerline (Retired) 
		/// </summary>
		public const uint EnergyWindowCenterlineRetired = 0x00180032;
		
		/// <summary>(0018,0033) VR=DS Energy Window Total Width (Retired) 
		/// </summary>
		public const uint EnergyWindowTotalWidthRetired = 0x00180033;
		
		/// <summary>(0018,0034) VR=LO Intervention Drug Name 
		/// </summary>
		public const uint InterventionDrugName = 0x00180034;
		
		/// <summary>(0018,0035) VR=TM Intervention Drug Start Time 
		/// </summary>
		public const uint InterventionDrugStartTime = 0x00180035;
		
		/// <summary>(0018,0036) VR=SQ Interventional Therapy Sequence 
		/// </summary>
		public const uint InterventionalTherapySeq = 0x00180036;
		
		/// <summary>(0018,0037) VR=CS Therapy Type 
		/// </summary>
		public const uint TherapyType = 0x00180037;
		
		/// <summary>(0018,0038) VR=CS Interventional Status 
		/// </summary>
		public const uint InterventionalStatus = 0x00180038;
		
		/// <summary>(0018,0039) VR=CS Therapy Description 
		/// </summary>
		public const uint TherapyDescription = 0x00180039;
		
		/// <summary>(0018,0040) VR=IS Cine Rate 
		/// </summary>
		public const uint CineRate = 0x00180040;
		
		/// <summary>(0018,0050) VR=DS Slice Thickness 
		/// </summary>
		public const uint SliceThickness = 0x00180050;
		
		/// <summary>(0018,0060) VR=DS KVP 
		/// </summary>
		public const uint KVP = 0x00180060;
		
		/// <summary>(0018,0070) VR=IS Counts Accumulated 
		/// </summary>
		public const uint CountsAccumulated = 0x00180070;
		
		/// <summary>(0018,0071) VR=CS Acquisition Termination Condition 
		/// </summary>
		public const uint AcquisitionTerminationCondition = 0x00180071;
		
		/// <summary>(0018,0072) VR=DS Effective Series Duration 
		/// </summary>
		public const uint EffectiveSeriesDuration = 0x00180072;
		
		/// <summary>(0018,0073) VR=CS Acquisition Start Condition 
		/// </summary>
		public const uint AcquisitionStartCondition = 0x00180073;
		
		/// <summary>(0018,0074) VR=IS Acquisition Start Condition Data 
		/// </summary>
		public const uint AcquisitionStartConditionData = 0x00180074;
		
		/// <summary>(0018,0075) VR=IS Acquisition Termination Condition Data 
		/// </summary>
		public const uint AcquisitionTerminationConditionData = 0x00180075;
		
		/// <summary>(0018,0080) VR=DS Repetition Time 
		/// </summary>
		public const uint RepetitionTime = 0x00180080;
		
		/// <summary>(0018,0081) VR=DS Echo Time 
		/// </summary>
		public const uint EchoTime = 0x00180081;
		
		/// <summary>(0018,0082) VR=DS Inversion Time 
		/// </summary>
		public const uint InversionTime = 0x00180082;
		
		/// <summary>(0018,0083) VR=DS Number of Averages 
		/// </summary>
		public const uint NumberOfAverages = 0x00180083;
		
		/// <summary>(0018,0084) VR=DS Imaging Frequency 
		/// </summary>
		public const uint ImagingFrequency = 0x00180084;
		
		/// <summary>(0018,0085) VR=SH Imaged Nucleus 
		/// </summary>
		public const uint ImagedNucleus = 0x00180085;
		
		/// <summary>(0018,0086) VR=IS Echo Number(s) 
		/// </summary>
		public const uint EchoNumber = 0x00180086;
		
		/// <summary>(0018,0087) VR=DS Magnetic Field Strength 
		/// </summary>
		public const uint MagneticFieldStrength = 0x00180087;
		
		/// <summary>(0018,0088) VR=DS Spacing Between Slices 
		/// </summary>
		public const uint SpacingBetweenSlices = 0x00180088;
		
		/// <summary>(0018,0089) VR=IS Number of Phase Encoding Steps 
		/// </summary>
		public const uint NumberOfPhaseEncodingSteps = 0x00180089;
		
		/// <summary>(0018,0090) VR=DS Data Collection Diameter 
		/// </summary>
		public const uint DataCollectionDiameter = 0x00180090;
		
		/// <summary>(0018,0091) VR=IS Echo Train Length 
		/// </summary>
		public const uint EchoTrainLength = 0x00180091;
		
		/// <summary>(0018,0093) VR=DS Percent Sampling 
		/// </summary>
		public const uint PercentSampling = 0x00180093;
		
		/// <summary>(0018,0094) VR=DS Percent Phase Field of View 
		/// </summary>
		public const uint PercentPhaseFieldOfView = 0x00180094;
		
		/// <summary>(0018,0095) VR=DS Pixel Bandwidth 
		/// </summary>
		public const uint PixelBandwidth = 0x00180095;
		
		/// <summary>(0018,1000) VR=LO Device Serial Number 
		/// </summary>
		public const uint DeviceSerialNumber = 0x00181000;
		
		/// <summary>(0018,1004) VR=LO Plate ID 
		/// </summary>
		public const uint PlateID = 0x00181004;
		
		/// <summary>(0018,1010) VR=LO Secondary Capture Device ID 
		/// </summary>
		public const uint SecondaryCaptureDeviceID = 0x00181010;
		
		/// <summary>(0018,1011) VR=LO Hardcopy Creation Device ID 
		/// </summary>
		public const uint HardcopyCreationDeviceID = 0x00181011;
		
		/// <summary>(0018,1012) VR=DA Date of Secondary Capture 
		/// </summary>
		public const uint DateOfSecondaryCapture = 0x00181012;
		
		/// <summary>(0018,1014) VR=TM Time of Secondary Capture 
		/// </summary>
		public const uint TimeOfSecondaryCapture = 0x00181014;
		
		/// <summary>(0018,1016) VR=LO Secondary Capture Device Manufacturer 
		/// </summary>
		public const uint SecondaryCaptureDeviceManufacturer = 0x00181016;
		
		/// <summary>(0018,1017) VR=LO Hardcopy Device Manufacturer 
		/// </summary>
		public const uint HardcopyDeviceManufacturer = 0x00181017;
		
		/// <summary>(0018,1018) VR=LO Secondary Capture Device Manufacturer's Model Name 
		/// </summary>
		public const uint SecondaryCaptureDeviceManufacturerModelName = 0x00181018;
		
		/// <summary>(0018,1019) VR=LO Secondary Capture Device Software Version(s) 
		/// </summary>
		public const uint SecondaryCaptureDeviceSoftwareVersion = 0x00181019;
		
		/// <summary>(0018,101A) VR=LO Hardcopy Device Software Version 
		/// </summary>
		public const uint HardcopyDeviceSoftwareVersion = 0x0018101A;
		
		/// <summary>(0018,101B) VR=LO Hardcopy Device Manfuacturer's Model Name 
		/// </summary>
		public const uint HardcopyDeviceManfuacturerModelName = 0x0018101B;
		
		/// <summary>(0018,1020) VR=LO Software Version(s) 
		/// </summary>
		public const uint SoftwareVersion = 0x00181020;
		
		/// <summary>(0018,1022) VR=SH Video Image Format Acquired 
		/// </summary>
		public const uint VideoImageFormatAcquired = 0x00181022;
		
		/// <summary>(0018,1023) VR=LO Digital Image Format Acquired 
		/// </summary>
		public const uint DigitalImageFormatAcquired = 0x00181023;
		
		/// <summary>(0018,1030) VR=LO Protocol Name 
		/// </summary>
		public const uint ProtocolName = 0x00181030;
		
		/// <summary>(0018,1040) VR=LO Contrast/Bolus Route 
		/// </summary>
		public const uint ContrastBolusRoute = 0x00181040;
		
		/// <summary>(0018,1041) VR=DS Contrast/Bolus Volume 
		/// </summary>
		public const uint ContrastBolusVolume = 0x00181041;
		
		/// <summary>(0018,1042) VR=TM Contrast/Bolus Start Time 
		/// </summary>
		public const uint ContrastBolusStartTime = 0x00181042;
		
		/// <summary>(0018,1043) VR=TM Contrast/Bolus Stop Time 
		/// </summary>
		public const uint ContrastBolusStopTime = 0x00181043;
		
		/// <summary>(0018,1044) VR=DS Contrast/Bolus Total Dose 
		/// </summary>
		public const uint ContrastBolusTotalDose = 0x00181044;
		
		/// <summary>(0018,1045) VR=IS Syringe Counts 
		/// </summary>
		public const uint SyringeCounts = 0x00181045;
		
		/// <summary>(0018,1046) VR=DS Contrast Flow Rate(s) 
		/// </summary>
		public const uint ContrastFlowRate = 0x00181046;
		
		/// <summary>(0018,1047) VR=DS Contrast Flow Duration(s) 
		/// </summary>
		public const uint ContrastFlowDuration = 0x00181047;
		
		/// <summary>(0018,1048) VR=CS Contrast/Bolus Ingredient 
		/// </summary>
		public const uint ContrastBolusIngredient = 0x00181048;
		
		/// <summary>(0018,1049) VR=DS Contrast/Bolus Ingredient Concentration 
		/// </summary>
		public const uint ContrastBolusIngredientConcentration = 0x00181049;
		
		/// <summary>(0018,1050) VR=DS Spatial Resolution 
		/// </summary>
		public const uint SpatialResolution = 0x00181050;
		
		/// <summary>(0018,1060) VR=DS Trigger Time 
		/// </summary>
		public const uint TriggerTime = 0x00181060;
		
		/// <summary>(0018,1061) VR=LO Trigger Source or Type 
		/// </summary>
		public const uint TriggerSourceOrType = 0x00181061;
		
		/// <summary>(0018,1062) VR=IS Nominal Interval 
		/// </summary>
		public const uint NominalInterval = 0x00181062;
		
		/// <summary>(0018,1063) VR=DS Frame Time 
		/// </summary>
		public const uint FrameTime = 0x00181063;
		
		/// <summary>(0018,1064) VR=LO Framing Type 
		/// </summary>
		public const uint FramingType = 0x00181064;
		
		/// <summary>(0018,1065) VR=DS Frame Time Vector 
		/// </summary>
		public const uint FrameTimeVector = 0x00181065;
		
		/// <summary>(0018,1066) VR=DS Frame Delay 
		/// </summary>
		public const uint FrameDelay = 0x00181066;
		
		/// <summary>(0018,1067) VR=DS Image Trigger Delay 
		/// </summary>
		public const uint ImageTriggerDelay = 0x00181067;
		
		/// <summary>(0018,1068) VR=DS Multiplex Group Time Offset 
		/// </summary>
		public const uint MultiplexGroupTimeOffset = 0x00181068;
		
		/// <summary>(0018,1069) VR=DS Trigger Time Offset 
		/// </summary>
		public const uint TriggerTimeOffset = 0x00181069;
		
		/// <summary>(0018,106A) VR=CS Synchronization Trigger 
		/// </summary>
		public const uint SynchronizationTrigger = 0x0018106A;
		
		/// <summary>(0018,106C) VR=US Synchronization Channel 
		/// </summary>
		public const uint SynchronizationChannel = 0x0018106C;
		
		/// <summary>(0018,106E) VR=UL Trigger Sample Position 
		/// </summary>
		public const uint TriggerSamplePosition = 0x0018106E;
		
		/// <summary>(0018,1070) VR=LO Radiopharmaceutical Route 
		/// </summary>
		public const uint RadiopharmaceuticalRoute = 0x00181070;
		
		/// <summary>(0018,1071) VR=DS Radiopharmaceutical Volume 
		/// </summary>
		public const uint RadiopharmaceuticalVolume = 0x00181071;
		
		/// <summary>(0018,1072) VR=TM Radiopharmaceutical Start Time 
		/// </summary>
		public const uint RadiopharmaceuticalStartTime = 0x00181072;
		
		/// <summary>(0018,1073) VR=TM Radiopharmaceutical Stop Time 
		/// </summary>
		public const uint RadiopharmaceuticalStopTime = 0x00181073;
		
		/// <summary>(0018,1074) VR=DS Radionuclide Total Dose 
		/// </summary>
		public const uint RadionuclideTotalDose = 0x00181074;
		
		/// <summary>(0018,1075) VR=DS Radionuclide Half Life 
		/// </summary>
		public const uint RadionuclideHalfLife = 0x00181075;
		
		/// <summary>(0018,1076) VR=DS Radionuclide Positron Fraction 
		/// </summary>
		public const uint RadionuclidePositronFraction = 0x00181076;
		
		/// <summary>(0018,1077) VR=DS Radiopharmaceutical Specific Activity 
		/// </summary>
		public const uint RadiopharmaceuticalSpecificActivity = 0x00181077;
		
		/// <summary>(0018,1080) VR=CS Beat Rejection Flag 
		/// </summary>
		public const uint BeatRejectionFlag = 0x00181080;
		
		/// <summary>(0018,1081) VR=IS Low R-R Value 
		/// </summary>
		public const uint LowRRValue = 0x00181081;
		
		/// <summary>(0018,1082) VR=IS High R-R Value 
		/// </summary>
		public const uint HighRRValue = 0x00181082;
		
		/// <summary>(0018,1083) VR=IS Intervals Acquired 
		/// </summary>
		public const uint IntervalsAcquired = 0x00181083;
		
		/// <summary>(0018,1084) VR=IS Intervals Rejected 
		/// </summary>
		public const uint IntervalsRejected = 0x00181084;
		
		/// <summary>(0018,1085) VR=LO PVC Rejection 
		/// </summary>
		public const uint PVCRejection = 0x00181085;
		
		/// <summary>(0018,1086) VR=IS Skip Beats 
		/// </summary>
		public const uint SkipBeats = 0x00181086;
		
		/// <summary>(0018,1088) VR=IS Heart Rate 
		/// </summary>
		public const uint HeartRate = 0x00181088;
		
		/// <summary>(0018,1090) VR=IS Cardiac Number of Images 
		/// </summary>
		public const uint CardiacNumberOfImages = 0x00181090;
		
		/// <summary>(0018,1094) VR=IS Trigger Window 
		/// </summary>
		public const uint TriggerWindow = 0x00181094;
		
		/// <summary>(0018,1100) VR=DS Reconstruction Diameter 
		/// </summary>
		public const uint ReconstructionDiameter = 0x00181100;
		
		/// <summary>(0018,1110) VR=DS Distance Source to Detector 
		/// </summary>
		public const uint DistanceSourceToDetector = 0x00181110;
		
		/// <summary>(0018,1111) VR=DS Distance Source to Patient 
		/// </summary>
		public const uint DistanceSourceToPatient = 0x00181111;
		
		/// <summary>(0018,1114) VR=DS Estimated Radiographic Magnification Factor 
		/// </summary>
		public const uint EstimatedRadiographicMagnificationFactor = 0x00181114;
		
		/// <summary>(0018,1120) VR=DS Gantry/Detector Tilt 
		/// </summary>
		public const uint GantryDetectorTilt = 0x00181120;
		
		/// <summary>(0018,1121) VR=DS Gantry/Detector Slew 
		/// </summary>
		public const uint GantryDetectorSlew = 0x00181121;
		
		/// <summary>(0018,1130) VR=DS Table Height 
		/// </summary>
		public const uint TableHeight = 0x00181130;
		
		/// <summary>(0018,1131) VR=DS Table Traverse 
		/// </summary>
		public const uint TableTraverse = 0x00181131;
		
		/// <summary>(0018,1134) VR=CS Table Motion 
		/// </summary>
		public const uint TableMotion = 0x00181134;
		
		/// <summary>(0018,1135) VR=DS Table Vertical Increment 
		/// </summary>
		public const uint TableVerticalIncrement = 0x00181135;
		
		/// <summary>(0018,1136) VR=DS Table Lateral Increment 
		/// </summary>
		public const uint TableLateralIncrement = 0x00181136;
		
		/// <summary>(0018,1137) VR=DS Table Longitudinal Increment 
		/// </summary>
		public const uint TableLongitudinalIncrement = 0x00181137;
		
		/// <summary>(0018,1138) VR=DS Table Angle 
		/// </summary>
		public const uint TableAngle = 0x00181138;
		
		/// <summary>(0018,113A) VR=CS Table Type 
		/// </summary>
		public const uint TableType = 0x0018113A;
		
		/// <summary>(0018,1140) VR=CS Rotation Direction 
		/// </summary>
		public const uint RotationDirection = 0x00181140;
		
		/// <summary>(0018,1141) VR=DS Angular Position 
		/// </summary>
		public const uint AngularPosition = 0x00181141;
		
		/// <summary>(0018,1142) VR=DS Radial Position 
		/// </summary>
		public const uint RadialPosition = 0x00181142;
		
		/// <summary>(0018,1143) VR=DS Scan Arc 
		/// </summary>
		public const uint ScanArc = 0x00181143;
		
		/// <summary>(0018,1144) VR=DS Angular Step 
		/// </summary>
		public const uint AngularStep = 0x00181144;
		
		/// <summary>(0018,1145) VR=DS Center of Rotation Offset 
		/// </summary>
		public const uint CenterOfRotationOffset = 0x00181145;
		
		/// <summary>(0018,1146) VR=DS Rotation Offset (Retired) 
		/// </summary>
		public const uint RotationOffsetRetired = 0x00181146;
		
		/// <summary>(0018,1147) VR=CS Field of View Shape 
		/// </summary>
		public const uint FieldOfViewShape = 0x00181147;
		
		/// <summary>(0018,1149) VR=IS Field of View Dimension(s) 
		/// </summary>
		public const uint FieldOfViewDimension = 0x00181149;
		
		/// <summary>(0018,1150) VR=IS Exposure Time 
		/// </summary>
		public const uint ExposureTime = 0x00181150;
		
		/// <summary>(0018,1151) VR=IS X-ray Tube Current 
		/// </summary>
		public const uint XRayTubeCurrent = 0x00181151;
		
		/// <summary>(0018,1152) VR=IS Exposure 
		/// </summary>
		public const uint Exposure = 0x00181152;
		
		/// <summary>(0018,1153) VR=IS Exposure in uAs 
		/// </summary>
		public const uint ExposureInuAs = 0x00181153;
		
		/// <summary>(0018,1154) VR=DS Average Pulse Width 
		/// </summary>
		public const uint AveragePulseWidth = 0x00181154;
		
		/// <summary>(0018,1155) VR=CS Radiation Setting 
		/// </summary>
		public const uint RadiationSetting = 0x00181155;
		
		/// <summary>(0018,1156) VR=CS Rectification Type 
		/// </summary>
		public const uint RectificationType = 0x00181156;
		
		/// <summary>(0018,115A) VR=CS Radiation Mode 
		/// </summary>
		public const uint RadiationMode = 0x0018115A;
		
		/// <summary>(0018,115E) VR=DS Image Area Dose Product 
		/// </summary>
		public const uint ImageAreaDoseProduct = 0x0018115E;
		
		/// <summary>(0018,1160) VR=SH Filter Type 
		/// </summary>
		public const uint FilterType = 0x00181160;
		
		/// <summary>(0018,1161) VR=LO Type of Filters 
		/// </summary>
		public const uint TypeOfFilters = 0x00181161;
		
		/// <summary>(0018,1162) VR=DS Intensifier Size 
		/// </summary>
		public const uint IntensifierSize = 0x00181162;
		
		/// <summary>(0018,1164) VR=DS Imager Pixel Spacing 
		/// </summary>
		public const uint ImagerPixelSpacing = 0x00181164;
		
		/// <summary>(0018,1166) VR=CS Grid 
		/// </summary>
		public const uint Grid = 0x00181166;
		
		/// <summary>(0018,1170) VR=IS Generator Power 
		/// </summary>
		public const uint GeneratorPower = 0x00181170;
		
		/// <summary>(0018,1180) VR=SH Collimator/grid Name 
		/// </summary>
		public const uint CollimatorGridName = 0x00181180;
		
		/// <summary>(0018,1181) VR=CS Collimator Type 
		/// </summary>
		public const uint CollimatorType = 0x00181181;
		
		/// <summary>(0018,1182) VR=IS Focal Distance 
		/// </summary>
		public const uint FocalDistance = 0x00181182;
		
		/// <summary>(0018,1183) VR=DS X Focus Center 
		/// </summary>
		public const uint XFocusCenter = 0x00181183;
		
		/// <summary>(0018,1184) VR=DS Y Focus Center 
		/// </summary>
		public const uint YFocusCenter = 0x00181184;
		
		/// <summary>(0018,1190) VR=DS Focal Spot(s) 
		/// </summary>
		public const uint FocalSpot = 0x00181190;
		
		/// <summary>(0018,1191) VR=CS Anode Target Material 
		/// </summary>
		public const uint AnodeTargetMaterial = 0x00181191;
		
		/// <summary>(0018,11A0) VR=DS Body Part Thickness 
		/// </summary>
		public const uint BodyPartThickness = 0x001811A0;
		
		/// <summary>(0018,11A2) VR=DS Compression Force 
		/// </summary>
		public const uint CompressionForce = 0x001811A2;
		
		/// <summary>(0018,1200) VR=DA Date of Last Calibration 
		/// </summary>
		public const uint DateOfLastCalibration = 0x00181200;
		
		/// <summary>(0018,1201) VR=TM Time of Last Calibration 
		/// </summary>
		public const uint TimeOfLastCalibration = 0x00181201;
		
		/// <summary>(0018,1210) VR=SH Convolution Kernel 
		/// </summary>
		public const uint ConvolutionKernel = 0x00181210;
		
		/// <summary>(0018,1240) VR=UN Upper/Lower Pixel Values (Retired) 
		/// </summary>
		public const uint UpperLowerPixelValuesRetired = 0x00181240;
		
		/// <summary>(0018,1242) VR=IS Actual Frame Duration 
		/// </summary>
		public const uint ActualFrameDuration = 0x00181242;
		
		/// <summary>(0018,1243) VR=IS Count Rate 
		/// </summary>
		public const uint CountRate = 0x00181243;
		
		/// <summary>(0018,1244) VR=US Preferred Playback Sequencing 
		/// </summary>
		public const uint PreferredPlaybackSequencing = 0x00181244;
		
		/// <summary>(0018,1250) VR=SH Receive Coil Name 
		/// </summary>
		public const uint ReceiveCoilName = 0x00181250;
		
		/// <summary>(0018,1251) VR=SH Transmit Coil Name 
		/// </summary>
		public const uint TransmitCoilName = 0x00181251;
		
		/// <summary>(0018,1260) VR=SH Plate Type 
		/// </summary>
		public const uint PlateType = 0x00181260;
		
		/// <summary>(0018,1261) VR=LO Phosphor Type 
		/// </summary>
		public const uint PhosphorType = 0x00181261;
		
		/// <summary>(0018,1300) VR=DS Scan Velocity 
		/// </summary>
		public const uint ScanVelocity = 0x00181300;
		
		/// <summary>(0018,1301) VR=CS Whole Body Technique 
		/// </summary>
		public const uint WholeBodyTechnique = 0x00181301;
		
		/// <summary>(0018,1302) VR=IS Scan Length 
		/// </summary>
		public const uint ScanLength = 0x00181302;
		
		/// <summary>(0018,1310) VR=US Acquisition Matrix 
		/// </summary>
		public const uint AcquisitionMatrix = 0x00181310;
		
		/// <summary>(0018,1312) VR=CS Phase Encoding Direction 
		/// </summary>
		public const uint PhaseEncodingDirection = 0x00181312;
		
		/// <summary>(0018,1314) VR=DS Flip Angle 
		/// </summary>
		public const uint FlipAngle = 0x00181314;
		
		/// <summary>(0018,1315) VR=CS Variable Flip Angle Flag 
		/// </summary>
		public const uint VariableFlipAngleFlag = 0x00181315;
		
		/// <summary>(0018,1316) VR=DS SAR 
		/// </summary>
		public const uint SAR = 0x00181316;
		
		/// <summary>(0018,1318) VR=DS dB/dt 
		/// </summary>
		public const uint dBDt = 0x00181318;
		
		/// <summary>(0018,1400) VR=LO Acquisition Device Processing Description 
		/// </summary>
		public const uint AcquisitionDeviceProcessingDescription = 0x00181400;
		
		/// <summary>(0018,1401) VR=LO Acquisition Device Processing Code 
		/// </summary>
		public const uint AcquisitionDeviceProcessingCode = 0x00181401;
		
		/// <summary>(0018,1402) VR=CS Cassette Orientation 
		/// </summary>
		public const uint CassetteOrientation = 0x00181402;
		
		/// <summary>(0018,1403) VR=CS Cassette Size 
		/// </summary>
		public const uint CassetteSize = 0x00181403;
		
		/// <summary>(0018,1404) VR=US Exposures on Plate 
		/// </summary>
		public const uint ExposuresOnPlate = 0x00181404;
		
		/// <summary>(0018,1405) VR=IS Relative X-ray Exposure 
		/// </summary>
		public const uint RelativeXRayExposure = 0x00181405;
		
		/// <summary>(0018,1450) VR=CS Column Angulation 
		/// </summary>
		public const uint ColumnAngulation = 0x00181450;
		
		/// <summary>(0018,1460) VR=DS Tomo Layer Height 
		/// </summary>
		public const uint TomoLayerHeight = 0x00181460;
		
		/// <summary>(0018,1470) VR=DS Tomo Angle 
		/// </summary>
		public const uint TomoAngle = 0x00181470;
		
		/// <summary>(0018,1480) VR=DS Tomo Time 
		/// </summary>
		public const uint TomoTime = 0x00181480;
		
		/// <summary>(0018,1490) VR=CS Tomo Type 
		/// </summary>
		public const uint TomoType = 0x00181490;
		
		/// <summary>(0018,1491) VR=CS Tomo Class 
		/// </summary>
		public const uint TomoClass = 0x00181491;
		
		/// <summary>(0018,1495) VR=IS Number of Tomosynthesis Source Images 
		/// </summary>
		public const uint NumberofTomosynthesisSourceImages = 0x00181495;
		
		/// <summary>(0018,1500) VR=CS Positioner Motion 
		/// </summary>
		public const uint PositionerMotion = 0x00181500;
		
		/// <summary>(0018,1508) VR=CS Positioner Type 
		/// </summary>
		public const uint PositionerType = 0x00181508;
		
		/// <summary>(0018,1510) VR=DS Positioner Primary Angle 
		/// </summary>
		public const uint PositionerPrimaryAngle = 0x00181510;
		
		/// <summary>(0018,1511) VR=DS Positioner Secondary Angle 
		/// </summary>
		public const uint PositionerSecondaryAngle = 0x00181511;
		
		/// <summary>(0018,1520) VR=DS Positioner Primary Angle Increment 
		/// </summary>
		public const uint PositionerPrimaryAngleIncrement = 0x00181520;
		
		/// <summary>(0018,1521) VR=DS Positioner Secondary Angle Increment 
		/// </summary>
		public const uint PositionerSecondaryAngleIncrement = 0x00181521;
		
		/// <summary>(0018,1530) VR=DS Detector Primary Angle 
		/// </summary>
		public const uint DetectorPrimaryAngle = 0x00181530;
		
		/// <summary>(0018,1531) VR=DS Detector Secondary Angle 
		/// </summary>
		public const uint DetectorSecondaryAngle = 0x00181531;
		
		/// <summary>(0018,1600) VR=CS Shutter Shape 
		/// </summary>
		public const uint ShutterShape = 0x00181600;
		
		/// <summary>(0018,1602) VR=IS Shutter Left Vertical Edge 
		/// </summary>
		public const uint ShutterLeftVerticalEdge = 0x00181602;
		
		/// <summary>(0018,1604) VR=IS Shutter Right Vertical Edge 
		/// </summary>
		public const uint ShutterRightVerticalEdge = 0x00181604;
		
		/// <summary>(0018,1606) VR=IS Shutter Upper Horizontal Edge 
		/// </summary>
		public const uint ShutterUpperHorizontalEdge = 0x00181606;
		
		/// <summary>(0018,1608) VR=IS Shutter Lower Horizontal Edge 
		/// </summary>
		public const uint ShutterLowerHorizontalEdge = 0x00181608;
		
		/// <summary>(0018,1610) VR=IS Center of Circular Shutter 
		/// </summary>
		public const uint CenterOfCircularShutter = 0x00181610;
		
		/// <summary>(0018,1612) VR=IS Radius of Circular Shutter 
		/// </summary>
		public const uint RadiusOfCircularShutter = 0x00181612;
		
		/// <summary>(0018,1620) VR=IS Vertices of the Polygonal Shutter 
		/// </summary>
		public const uint VerticesOfPolygonalShutter = 0x00181620;
		
		/// <summary>(0018,1622) VR=US Shutter Presentation Value 
		/// </summary>
		public const uint ShutterPresentationValue = 0x00181622;
		
		/// <summary>(0018,1623) VR=US Shutter Overlay Group 
		/// </summary>
		public const uint ShutterOverlayGroup = 0x00181623;
		
		/// <summary>(0018,1700) VR=CS Collimator Shape 
		/// </summary>
		public const uint CollimatorShape = 0x00181700;
		
		/// <summary>(0018,1702) VR=IS Collimator Left Vertical Edge 
		/// </summary>
		public const uint CollimatorLeftVerticalEdge = 0x00181702;
		
		/// <summary>(0018,1704) VR=IS Collimator Right Vertical Edge 
		/// </summary>
		public const uint CollimatorRightVerticalEdge = 0x00181704;
		
		/// <summary>(0018,1706) VR=IS Collimator Upper Horizontal Edge 
		/// </summary>
		public const uint CollimatorUpperHorizontalEdge = 0x00181706;
		
		/// <summary>(0018,1708) VR=IS Collimator Lower Horizontal Edge 
		/// </summary>
		public const uint CollimatorLowerHorizontalEdge = 0x00181708;
		
		/// <summary>(0018,1710) VR=IS Center of Circular Collimator 
		/// </summary>
		public const uint CenterOfCircularCollimator = 0x00181710;
		
		/// <summary>(0018,1712) VR=IS Radius of Circular Collimator 
		/// </summary>
		public const uint RadiusOfCircularCollimator = 0x00181712;
		
		/// <summary>(0018,1720) VR=IS Vertices of the Polygonal Collimator 
		/// </summary>
		public const uint VerticesOfPolygonalCollimator = 0x00181720;
		
		/// <summary>(0018,1800) VR=CS Acquisition Time Synchronized 
		/// </summary>
		public const uint AcquisitionTimeSynchronized = 0x00181800;
		
		/// <summary>(0018,1802) VR=CS Time Distribution Protocol 
		/// </summary>
		public const uint TimeDistributionProtocol = 0x00181802;
		
		/// <summary>(0018,1801) VR=SH Time Source 
		/// </summary>
		public const uint TimeSource = 0x00181801;
		
		/// <summary>(0018,4000) VR=LT (Series) Comments (Retired) 
		/// </summary>
		public const uint SeriesCommentsRetired = 0x00184000;
		
		/// <summary>(0018,5000) VR=SH Output Power 
		/// </summary>
		public const uint OutputPower = 0x00185000;
		
		/// <summary>(0018,5010) VR=LO Transducer Data 
		/// </summary>
		public const uint TransducerData = 0x00185010;
		
		/// <summary>(0018,5012) VR=DS Focus Depth 
		/// </summary>
		public const uint FocusDepth = 0x00185012;
		
		/// <summary>(0018,5020) VR=LO Processing Function 
		/// </summary>
		public const uint ProcessingFunction = 0x00185020;
		
		/// <summary>(0018,5021) VR=LO Postprocessing Function 
		/// </summary>
		public const uint PostprocessingFunction = 0x00185021;
		
		/// <summary>(0018,5022) VR=DS Mechanical Index 
		/// </summary>
		public const uint MechanicalIndex = 0x00185022;
		
		/// <summary>(0018,5024) VR=DS Thermal Index 
		/// </summary>
		public const uint ThermalIndex = 0x00185024;
		
		/// <summary>(0018,5026) VR=DS Cranial Thermal Index 
		/// </summary>
		public const uint CranialThermalIndex = 0x00185026;
		
		/// <summary>(0018,5027) VR=DS Soft Tissue Thermal Index 
		/// </summary>
		public const uint SoftTissueThermalIndex = 0x00185027;
		
		/// <summary>(0018,5028) VR=DS Soft Tissue-focus Thermal Index 
		/// </summary>
		public const uint SoftTissueFocusThermalIndex = 0x00185028;
		
		/// <summary>(0018,5029) VR=DS Soft Tissue-surface Thermal Index 
		/// </summary>
		public const uint SoftTissueSurfaceThermalIndex = 0x00185029;
		
		/// <summary>(0018,5030) VR=UN Dynamic Range (Retired) 
		/// </summary>
		public const uint DynamicRangeRetired = 0x00185030;
		
		/// <summary>(0018,5040) VR=UN Total Gain (Retired) 
		/// </summary>
		public const uint TotalGainRetired = 0x00185040;
		
		/// <summary>(0018,5050) VR=IS Depth of Scan Field 
		/// </summary>
		public const uint DepthOfScanField = 0x00185050;
		
		/// <summary>(0018,5100) VR=CS Patient Position 
		/// </summary>
		public const uint PatientPosition = 0x00185100;
		
		/// <summary>(0018,5101) VR=CS View Position 
		/// </summary>
		public const uint ViewPosition = 0x00185101;
		
		/// <summary>(0018,5104) VR=SQ Projection Eponymous Name Code Sequence 
		/// </summary>
		public const uint ProjectionEponymousNameCodeSeq = 0x00185104;
		
		/// <summary>(0018,5210) VR=DS Image Transformation Matrix 
		/// </summary>
		public const uint ImageTransformationMatrix = 0x00185210;
		
		/// <summary>(0018,5212) VR=DS Image Translation Vector 
		/// </summary>
		public const uint ImageTranslationVector = 0x00185212;
		
		/// <summary>(0018,6000) VR=DS Sensitivity 
		/// </summary>
		public const uint Sensitivity = 0x00186000;
		
		/// <summary>(0018,6011) VR=SQ Seq of Ultrasound Regions 
		/// </summary>
		public const uint SeqOfUltrasoundRegions = 0x00186011;
		
		/// <summary>(0018,6012) VR=US Region Spatial Format 
		/// </summary>
		public const uint RegionSpatialFormat = 0x00186012;
		
		/// <summary>(0018,6014) VR=US Region Data Type 
		/// </summary>
		public const uint RegionDataType = 0x00186014;
		
		/// <summary>(0018,6016) VR=UL Region Flags 
		/// </summary>
		public const uint RegionFlags = 0x00186016;
		
		/// <summary>(0018,6018) VR=UL Region Location Min X0 
		/// </summary>
		public const uint RegionLocationMinX0 = 0x00186018;
		
		/// <summary>(0018,601A) VR=UL Region Location Min Y0 
		/// </summary>
		public const uint RegionLocationMinY0 = 0x0018601A;
		
		/// <summary>(0018,601C) VR=UL Region Location Max X1 
		/// </summary>
		public const uint RegionLocationMaxX1 = 0x0018601C;
		
		/// <summary>(0018,601E) VR=UL Region Location Max Y1 
		/// </summary>
		public const uint RegionLocationMaxY1 = 0x0018601E;
		
		/// <summary>(0018,6020) VR=SL Reference Pixel X0 
		/// </summary>
		public const uint ReferencePixelX0 = 0x00186020;
		
		/// <summary>(0018,6022) VR=SL Reference Pixel Y0 
		/// </summary>
		public const uint ReferencePixelY0 = 0x00186022;
		
		/// <summary>(0018,6024) VR=US Physical Units X Direction 
		/// </summary>
		public const uint PhysicalUnitsXDirection = 0x00186024;
		
		/// <summary>(0018,6026) VR=US Physical Units Y Direction 
		/// </summary>
		public const uint PhysicalUnitsYDirection = 0x00186026;
		
		/// <summary>(0018,6028) VR=FD Reference Pixel Physical Value X 
		/// </summary>
		public const uint ReferencePixelPhysicalValueX = 0x00186028;
		
		/// <summary>(0018,602A) VR=FD Reference Pixel Physical Value Y 
		/// </summary>
		public const uint ReferencePixelPhysicalValueY = 0x0018602A;
		
		/// <summary>(0018,602C) VR=FD Physical Delta X 
		/// </summary>
		public const uint PhysicalDeltaX = 0x0018602C;
		
		/// <summary>(0018,602E) VR=FD Physical Delta Y 
		/// </summary>
		public const uint PhysicalDeltaY = 0x0018602E;
		
		/// <summary>(0018,6030) VR=UL Transducer Frequency 
		/// </summary>
		public const uint TransducerFrequency = 0x00186030;
		
		/// <summary>(0018,6031) VR=CS Transducer Type 
		/// </summary>
		public const uint TransducerType = 0x00186031;
		
		/// <summary>(0018,6032) VR=UL Pulse Repetition Frequency 
		/// </summary>
		public const uint PulseRepetitionFrequency = 0x00186032;
		
		/// <summary>(0018,6034) VR=FD Doppler Correction Angle 
		/// </summary>
		public const uint DopplerCorrectionAngle = 0x00186034;
		
		/// <summary>(0018,6036) VR=FD Steering Angle 
		/// </summary>
		public const uint SteeringAngle = 0x00186036;
		
		/// <summary>(0018,6038) VR=UL Doppler Sample Volume X Position 
		/// </summary>
		public const uint DopplerSampleVolumeXPosition = 0x00186038;
		
		/// <summary>(0018,603A) VR=UL Doppler Sample Volume Y Position 
		/// </summary>
		public const uint DopplerSampleVolumeYPosition = 0x0018603A;
		
		/// <summary>(0018,603C) VR=UL TM-Line Position X0 
		/// </summary>
		public const uint TMLinePositionX0 = 0x0018603C;
		
		/// <summary>(0018,603E) VR=UL TM-Line Position Y0 
		/// </summary>
		public const uint TMLinePositionY0 = 0x0018603E;
		
		/// <summary>(0018,6040) VR=UL TM-Line Position X1 
		/// </summary>
		public const uint TMLinePositionX1 = 0x00186040;
		
		/// <summary>(0018,6042) VR=UL TM-Line Position Y1 
		/// </summary>
		public const uint TMLinePositionY1 = 0x00186042;
		
		/// <summary>(0018,6044) VR=US Pixel Component Organization 
		/// </summary>
		public const uint PixelComponentOrganization = 0x00186044;
		
		/// <summary>(0018,6046) VR=UL Pixel Component Mask 
		/// </summary>
		public const uint PixelComponentMask = 0x00186046;
		
		/// <summary>(0018,6048) VR=UL Pixel Component Range Start 
		/// </summary>
		public const uint PixelComponentRangeStart = 0x00186048;
		
		/// <summary>(0018,604A) VR=UL Pixel Component Range Stop 
		/// </summary>
		public const uint PixelComponentRangeStop = 0x0018604A;
		
		/// <summary>(0018,604C) VR=US Pixel Component Physical Units 
		/// </summary>
		public const uint PixelComponentPhysicalUnits = 0x0018604C;
		
		/// <summary>(0018,604E) VR=US Pixel Component Data Type 
		/// </summary>
		public const uint PixelComponentDataType = 0x0018604E;
		
		/// <summary>(0018,6050) VR=UL Number of Table Break Points 
		/// </summary>
		public const uint NumberOfTableBreakPoints = 0x00186050;
		
		/// <summary>(0018,6052) VR=UL Table of X Break Points 
		/// </summary>
		public const uint TableOfXBreakPoints = 0x00186052;
		
		/// <summary>(0018,6054) VR=FD Table of Y Break Points 
		/// </summary>
		public const uint TableOfYBreakPoints = 0x00186054;
		
		/// <summary>(0018,6056) VR=UL Number of Table Entries 
		/// </summary>
		public const uint NumberOfTableEntries = 0x00186056;
		
		/// <summary>(0018,6058) VR=UL Table of Pixel Values 
		/// </summary>
		public const uint TableOfPixelValues = 0x00186058;
		
		/// <summary>(0018,605A) VR=FL Table of Parameter Values 
		/// </summary>
		public const uint TableOfParameterValues = 0x0018605A;
		
		/// <summary>(0018,7000) VR=CS Detector Conditions Nominal Flag 
		/// </summary>
		public const uint DetectorConditionsNominalFlag = 0x00187000;
		
		/// <summary>(0018,7001) VR=DS Detector Temperature 
		/// </summary>
		public const uint DetectorTemperature = 0x00187001;
		
		/// <summary>(0018,7004) VR=CS Detector Type 
		/// </summary>
		public const uint DetectorType = 0x00187004;
		
		/// <summary>(0018,7005) VR=CS Detector Configuration 
		/// </summary>
		public const uint DetectorConfiguration = 0x00187005;
		
		/// <summary>(0018,7006) VR=LT Detector Description 
		/// </summary>
		public const uint DetectorDescription = 0x00187006;
		
		/// <summary>(0018,7008) VR=LT Detector Mode 
		/// </summary>
		public const uint DetectorMode = 0x00187008;
		
		/// <summary>(0018,700A) VR=SH Detector ID 
		/// </summary>
		public const uint DetectorID = 0x0018700A;
		
		/// <summary>(0018,700C) VR=DA Date of Last Detector Calibration 
		/// </summary>
		public const uint DateOfLastDetectorCalibration = 0x0018700C;
		
		/// <summary>(0018,700E) VR=TM Time of Last Detector Calibration 
		/// </summary>
		public const uint TimeOfLastDetectorCalibration = 0x0018700E;
		
		/// <summary>(0018,7010) VR=IS Exposures on Detector Since Last Calibration 
		/// </summary>
		public const uint ExposuresOnDetectorSinceLastCalibration = 0x00187010;
		
		/// <summary>(0018,7011) VR=IS Exposures on Detector Since Manufactured 
		/// </summary>
		public const uint ExposuresOnDetectorSinceManufactured = 0x00187011;
		
		/// <summary>(0018,7012) VR=DS Detector Time Since Last Exposure 
		/// </summary>
		public const uint DetectorTimeSinceLastExposure = 0x00187012;
		
		/// <summary>(0018,7014) VR=DS Detector Active Time 
		/// </summary>
		public const uint DetectorActiveTime = 0x00187014;
		
		/// <summary>(0018,7016) VR=DS Detector Activation Offset From Exposure 
		/// </summary>
		public const uint DetectorActivationOffsetFromExposure = 0x00187016;
		
		/// <summary>(0018,701A) VR=DS Detector Binning 
		/// </summary>
		public const uint DetectorBinning = 0x0018701A;
		
		/// <summary>(0018,7020) VR=DS Detector Element Physical Size 
		/// </summary>
		public const uint DetectorElementPhysicalSize = 0x00187020;
		
		/// <summary>(0018,7022) VR=DS Detector Element Spacing 
		/// </summary>
		public const uint DetectorElementSpacing = 0x00187022;
		
		/// <summary>(0018,7024) VR=CS Detector Active Shape 
		/// </summary>
		public const uint DetectorActiveShape = 0x00187024;
		
		/// <summary>(0018,7026) VR=DS Detector Active Dimension(s) 
		/// </summary>
		public const uint DetectorActiveDimension = 0x00187026;
		
		/// <summary>(0018,7028) VR=DS Detector Active Origin 
		/// </summary>
		public const uint DetectorActiveOrigin = 0x00187028;
		
		/// <summary>(0018,7030) VR=DS Field of View Origin 
		/// </summary>
		public const uint FieldOfViewOrigin = 0x00187030;
		
		/// <summary>(0018,7032) VR=DS Field of View Rotation 
		/// </summary>
		public const uint FieldOfViewRotation = 0x00187032;
		
		/// <summary>(0018,7034) VR=CS Field of View Horizontal Flip 
		/// </summary>
		public const uint FieldOfViewHorizontalFlip = 0x00187034;
		
		/// <summary>(0018,7040) VR=LT Grid Absorbing Material 
		/// </summary>
		public const uint GridAbsorbingMaterial = 0x00187040;
		
		/// <summary>(0018,7041) VR=LT Grid Spacing Material 
		/// </summary>
		public const uint GridSpacingMaterial = 0x00187041;
		
		/// <summary>(0018,7042) VR=DS Grid Thickness 
		/// </summary>
		public const uint GridThickness = 0x00187042;
		
		/// <summary>(0018,7044) VR=DS Grid Pitch 
		/// </summary>
		public const uint GridPitch = 0x00187044;
		
		/// <summary>(0018,7046) VR=IS Grid Aspect Ratio 
		/// </summary>
		public const uint GridAspectRatio = 0x00187046;
		
		/// <summary>(0018,7048) VR=DS Grid Period 
		/// </summary>
		public const uint GridPeriod = 0x00187048;
		
		/// <summary>(0018,704C) VR=DS Grid Focal Distance 
		/// </summary>
		public const uint GridFocalDistance = 0x0018704C;
		
		/// <summary>(0018,7050) VR=CS Filter Material 
		/// </summary>
		public const uint FilterMaterial = 0x00187050;
		
		/// <summary>(0018,7052) VR=DS Filter Thickness Minimum 
		/// </summary>
		public const uint FilterThicknessMinimum = 0x00187052;
		
		/// <summary>(0018,7054) VR=DS Filter Thickness Maximum 
		/// </summary>
		public const uint FilterThicknessMaximum = 0x00187054;
		
		/// <summary>(0018,7060) VR=CS Exposure Control Mode 
		/// </summary>
		public const uint ExposureControlMode = 0x00187060;
		
		/// <summary>(0018,7062) VR=LT Exposure Control Mode Description 
		/// </summary>
		public const uint ExposureControlModeDescription = 0x00187062;
		
		/// <summary>(0018,7064) VR=CS Exposure Status 
		/// </summary>
		public const uint ExposureStatus = 0x00187064;
		
		/// <summary>(0018,7065) VR=DS Phototimer Setting 
		/// </summary>
		public const uint PhototimerSetting = 0x00187065;
		
		/// <summary>(0018,8150) VR=DS Exposure Time in uS 
		/// </summary>
		public const uint ExposureTimeInuS = 0x00188150;
		
		/// <summary>(0018,8151) VR=DS X-Ray Tube Current in uA 
		/// </summary>
		public const uint XRayTubeCurrentInuA = 0x00188151;
		
		/// <summary>(0018,9004) VR=CS Content Qualification 
		/// </summary>
		public const uint ContentQualification = 0x00189004;
		
		/// <summary>(0018,9005) VR=SH Pulse Sequence Name 
		/// </summary>
		public const uint PulseSequenceName = 0x00189005;
		
		/// <summary>(0018,9006) VR=SQ MR Imaging Modifier Sequence 
		/// </summary>
		public const uint MRImagingModifierSeq = 0x00189006;
		
		/// <summary>(0018,9008) VR=CS Echo Pulse Sequence 
		/// </summary>
		public const uint EchoPulseSeq = 0x00189008;
		
		/// <summary>(0018,9009) VR=CS Inversion Recovery 
		/// </summary>
		public const uint InversionRecovery = 0x00189009;
		
		/// <summary>(0018,9010) VR=CS Flow Compensation 
		/// </summary>
		public const uint FlowCompensation = 0x00189010;
		
		/// <summary>(0018,9011) VR=CS Multiple Spin Echo 
		/// </summary>
		public const uint MultipleSpinEcho = 0x00189011;
		
		/// <summary>(0018,9012) VR=CS Multi-planar Excitation 
		/// </summary>
		public const uint MultiPlanarExcitation = 0x00189012;
		
		/// <summary>(0018,9014) VR=CS Phase Contrast 
		/// </summary>
		public const uint PhaseContrast = 0x00189014;
		
		/// <summary>(0018,9015) VR=CS Time of Flight Contrast 
		/// </summary>
		public const uint TimeOfFlightContrast = 0x00189015;
		
		/// <summary>(0018,9016) VR=CS Spoiling 
		/// </summary>
		public const uint Spoiling = 0x00189016;
		
		/// <summary>(0018,9017) VR=CS Steady State Pulse Sequence 
		/// </summary>
		public const uint SteadyStatePulseSeq = 0x00189017;
		
		/// <summary>(0018,9018) VR=CS Echo Planar Pulse Sequence 
		/// </summary>
		public const uint EchoPlanarPulseSeq = 0x00189018;
		
		/// <summary>(0018,9019) VR=FD Tag Angle First Axis 
		/// </summary>
		public const uint TagAngleFirstAxis = 0x00189019;
		
		/// <summary>(0018,9020) VR=CS Magnetization Transfer 
		/// </summary>
		public const uint MagnetizationTransfer = 0x00189020;
		
		/// <summary>(0018,9021) VR=CS T2 Preparation 
		/// </summary>
		public const uint T2Preparation = 0x00189021;
		
		/// <summary>(0018,9022) VR=CS Blood Signal Nulling 
		/// </summary>
		public const uint BloodSignalNulling = 0x00189022;
		
		/// <summary>(0018,9024) VR=CS Saturation Recovery 
		/// </summary>
		public const uint SaturationRecovery = 0x00189024;
		
		/// <summary>(0018,9025) VR=CS Spectrally Selected Suppression 
		/// </summary>
		public const uint SpectrallySelectedSuppression = 0x00189025;
		
		/// <summary>(0018,9026) VR=CS Spectrally Selected Excitation 
		/// </summary>
		public const uint SpectrallySelectedExcitation = 0x00189026;
		
		/// <summary>(0018,9027) VR=CS Spatial Pre-saturation 
		/// </summary>
		public const uint SpatialPreSaturation = 0x00189027;
		
		/// <summary>(0018,9028) VR=CS Tagging 
		/// </summary>
		public const uint Tagging = 0x00189028;
		
		/// <summary>(0018,9029) VR=CS Oversampling Phase 
		/// </summary>
		public const uint OversamplingPhase = 0x00189029;
		
		/// <summary>(0018,9030) VR=FD Tag Spacing First Dimension 
		/// </summary>
		public const uint TagSpacingFirstDimension = 0x00189030;
		
		/// <summary>(0018,9032) VR=CS Geometry of k-Space Traversal 
		/// </summary>
		public const uint GeometryOfKSpaceTraversal = 0x00189032;
		
		/// <summary>(0018,9033) VR=CS Segmented k-Space Traversal 
		/// </summary>
		public const uint SegmentedKSpaceTraversal = 0x00189033;
		
		/// <summary>(0018,9034) VR=CS Rectilinear Phase Encode Reordering 
		/// </summary>
		public const uint RectilinearPhaseEncodeReordering = 0x00189034;
		
		/// <summary>(0018,9035) VR=FD Tag Thickness 
		/// </summary>
		public const uint TagThickness = 0x00189035;
		
		/// <summary>(0018,9036) VR=CS Partial Fourier Direction 
		/// </summary>
		public const uint PartialFourierDirection = 0x00189036;
		
		/// <summary>(0018,9037) VR=CS Gating Synchronization Technique 
		/// </summary>
		public const uint GatingSynchronizationTechnique = 0x00189037;
		
		/// <summary>(0018,9041) VR=LO Receive Coil Manufacturer Name 
		/// </summary>
		public const uint ReceiveCoilManufacturerName = 0x00189041;
		
		/// <summary>(0018,9042) VR=SQ MR Receive Coil Sequence 
		/// </summary>
		public const uint MRReceiveCoilSeq = 0x00189042;
		
		/// <summary>(0018,9043) VR=CS Receive Coil Type 
		/// </summary>
		public const uint ReceiveCoilType = 0x00189043;
		
		/// <summary>(0018,9044) VR=CS Quadrature Receive Coil 
		/// </summary>
		public const uint QuadratureReceiveCoil = 0x00189044;
		
		/// <summary>(0018,9045) VR=SQ Multi-Coil DefInition Sequence 
		/// </summary>
		public const uint MultiCoilDefInitionSeq = 0x00189045;
		
		/// <summary>(0018,9046) VR=LO Multi-Coil Configuration 
		/// </summary>
		public const uint MultiCoilConfiguration = 0x00189046;
		
		/// <summary>(0018,9047) VR=SH Multi-Coil Element Name 
		/// </summary>
		public const uint MultiCoilElementName = 0x00189047;
		
		/// <summary>(0018,9048) VR=CS Multi-Coil Element Used 
		/// </summary>
		public const uint MultiCoilElementUsed = 0x00189048;
		
		/// <summary>(0018,9049) VR=SQ MR Transmit Coil Sequence 
		/// </summary>
		public const uint MRTransmitCoilSeq = 0x00189049;
		
		/// <summary>(0018,9050) VR=LO Transmit Coil Manufacturer Name 
		/// </summary>
		public const uint TransmitCoilManufacturerName = 0x00189050;
		
		/// <summary>(0018,9051) VR=CS Transmit Coil Type 
		/// </summary>
		public const uint TransmitCoilType = 0x00189051;
		
		/// <summary>(0018,9052) VR=FD Spectral Width 
		/// </summary>
		public const uint SpectralWidth = 0x00189052;
		
		/// <summary>(0018,9053) VR=FD Chemical Shift Reference 
		/// </summary>
		public const uint ChemicalShiftReference = 0x00189053;
		
		/// <summary>(0018,9054) VR=CS Volume Localization Technique 
		/// </summary>
		public const uint VolumeLocalizationTechnique = 0x00189054;
		
		/// <summary>(0018,9058) VR=US MR Acquisition Frequency Encoding Steps 
		/// </summary>
		public const uint MRAcquisitionFrequencyEncodingSteps = 0x00189058;
		
		/// <summary>(0018,9059) VR=CS De-coupling 
		/// </summary>
		public const uint DeCoupling = 0x00189059;
		
		/// <summary>(0018,9060) VR=CS De-coupled Nucleus 
		/// </summary>
		public const uint DeCoupledNucleus = 0x00189060;
		
		/// <summary>(0018,9061) VR=FD De-coupling Frequency 
		/// </summary>
		public const uint DeCouplingFrequency = 0x00189061;
		
		/// <summary>(0018,9062) VR=CS De-coupling Method 
		/// </summary>
		public const uint DeCouplingMethod = 0x00189062;
		
		/// <summary>(0018,9063) VR=FD De-coupling Chemical Shift Reference 
		/// </summary>
		public const uint DeCouplingChemicalShiftReference = 0x00189063;
		
		/// <summary>(0018,9064) VR=CS k-space Filtering 
		/// </summary>
		public const uint KSpaceFiltering = 0x00189064;
		
		/// <summary>(0018,9065) VR=CS Time Domain Filtering 
		/// </summary>
		public const uint TimeDomainFiltering = 0x00189065;
		
		/// <summary>(0018,9066) VR=US Number of Zero fills 
		/// </summary>
		public const uint NumberOfZeroFills = 0x00189066;
		
		/// <summary>(0018,9067) VR=CS Baseline Correction 
		/// </summary>
		public const uint BaselineCorrection = 0x00189067;
		
		/// <summary>(0018,9070) VR=FD Cardiac R-R Interval Specified 
		/// </summary>
		public const uint CardiacRRIntervalSpecified = 0x00189070;
		
		/// <summary>(0018,9073) VR=FD Acquisition Duration 
		/// </summary>
		public const uint AcquisitionDuration = 0x00189073;
		
		/// <summary>(0018,9074) VR=DT Frame Acquisition Datetime 
		/// </summary>
		public const uint FrameAcquisitionDatetime = 0x00189074;
		
		/// <summary>(0018,9075) VR=CS Diffusion Directionality 
		/// </summary>
		public const uint DiffusionDirectionality = 0x00189075;
		
		/// <summary>(0018,9076) VR=SQ Diffusion Gradient Direction Sequence 
		/// </summary>
		public const uint DiffusionGradientDirectionSeq = 0x00189076;
		
		/// <summary>(0018,9077) VR=CS Parallel Acquisition 
		/// </summary>
		public const uint ParallelAcquisition = 0x00189077;
		
		/// <summary>(0018,9078) VR=CS Parallel Acquisition Technique 
		/// </summary>
		public const uint ParallelAcquisitionTechnique = 0x00189078;
		
		/// <summary>(0018,9079) VR=FD Inversion Times 
		/// </summary>
		public const uint InversionTimes = 0x00189079;
		
		/// <summary>(0018,9080) VR=ST Metabolite Map Description 
		/// </summary>
		public const uint MetaboliteMapDescription = 0x00189080;
		
		/// <summary>(0018,9081) VR=CS Partial Fourier 
		/// </summary>
		public const uint PartialFourier = 0x00189081;
		
		/// <summary>(0018,9082) VR=FD Effective Echo Time 
		/// </summary>
		public const uint EffectiveEchoTime = 0x00189082;
		
		/// <summary>(0018,9084) VR=SQ Chemical Shift Sequence 
		/// </summary>
		public const uint ChemicalShiftSeq = 0x00189084;
		
		/// <summary>(0018,9085) VR=CS Cardiac Signal Source 
		/// </summary>
		public const uint CardiacSignalSource = 0x00189085;
		
		/// <summary>(0018,9087) VR=FD Diffusion b-value 
		/// </summary>
		public const uint DiffusionBValue = 0x00189087;
		
		/// <summary>(0018,9089) VR=FD Diffusion Gradient Orientation 
		/// </summary>
		public const uint DiffusionGradientOrientation = 0x00189089;
		
		/// <summary>(0018,9090) VR=FD Velocity Encoding Direction 
		/// </summary>
		public const uint VelocityEncodingDirection = 0x00189090;
		
		/// <summary>(0018,9091) VR=FD Velocity Encoding Minimum Value 
		/// </summary>
		public const uint VelocityEncodingMinimumValue = 0x00189091;
		
		/// <summary>(0018,9093) VR=US Number of k-Space Trajectories 
		/// </summary>
		public const uint NumberOfKSpaceTrajectories = 0x00189093;
		
		/// <summary>(0018,9094) VR=CS Coverage of k-Space 
		/// </summary>
		public const uint CoverageOfKSpace = 0x00189094;
		
		/// <summary>(0018,9095) VR=UL Spectroscopy Acquisition Phase Rows 
		/// </summary>
		public const uint SpectroscopyAcquisitionPhaseRows = 0x00189095;
		
		/// <summary>(0018,9096) VR=FD Parallel Reduction Factor In-plane 
		/// </summary>
		public const uint ParallelReductionFactorInPlane = 0x00189096;
		
		/// <summary>(0018,9098) VR=FD Transmitter Frequency 
		/// </summary>
		public const uint TransmitterFrequency = 0x00189098;
		
		/// <summary>(0018,9100) VR=CS Resonant Nucleus 
		/// </summary>
		public const uint ResonantNucleus = 0x00189100;
		
		/// <summary>(0018,9101) VR=CS Frequency Correction 
		/// </summary>
		public const uint FrequencyCorrection = 0x00189101;
		
		/// <summary>(0018,9103) VR=SQ MR Spectroscopy FOV/Geometry Sequence 
		/// </summary>
		public const uint MRSpectroscopyFOVGeometrySeq = 0x00189103;
		
		/// <summary>(0018,9104) VR=FD Slab Thickness 
		/// </summary>
		public const uint SlabThickness = 0x00189104;
		
		/// <summary>(0018,9105) VR=FD Slab Orientation 
		/// </summary>
		public const uint SlabOrientation = 0x00189105;
		
		/// <summary>(0018,9106) VR=FD Mid Slab Position 
		/// </summary>
		public const uint MidSlabPosition = 0x00189106;
		
		/// <summary>(0018,9107) VR=SQ MR Spatial Saturation Sequence 
		/// </summary>
		public const uint MRSpatialSaturationSeq = 0x00189107;
		
		/// <summary>(0018,9112) VR=SQ MR Timing and Related Parameters Sequence 
		/// </summary>
		public const uint MRTimingAndRelatedParametersSeq = 0x00189112;
		
		/// <summary>(0018,9114) VR=SQ MR Echo Sequence 
		/// </summary>
		public const uint MREchoSeq = 0x00189114;
		
		/// <summary>(0018,9115) VR=SQ MR Modifier Sequence 
		/// </summary>
		public const uint MRModifierSeq = 0x00189115;
		
		/// <summary>(0018,9117) VR=SQ MR Diffusion Sequence 
		/// </summary>
		public const uint MRDiffusionSeq = 0x00189117;
		
		/// <summary>(0018,9118) VR=SQ Cardiac Trigger Sequence 
		/// </summary>
		public const uint CardiacTriggerSeq = 0x00189118;
		
		/// <summary>(0018,9119) VR=SQ MR Averages Sequence 
		/// </summary>
		public const uint MRAveragesSeq = 0x00189119;
		
		/// <summary>(0018,9125) VR=SQ MR FOV/Geometry Sequence 
		/// </summary>
		public const uint MRFOVGeometrySeq = 0x00189125;
		
		/// <summary>(0018,9127) VR=UL Spectroscopy Acquisition Data Columns 
		/// </summary>
		public const uint SpectroscopyAcquisitionDataColumns = 0x00189127;
		
		/// <summary>(0018,9126) VR=SQ Volume Localization Sequence 
		/// </summary>
		public const uint VolumeLocalizationSeq = 0x00189126;
		
		/// <summary>(0018,9147) VR=CS Diffusion Anisotropy Type 
		/// </summary>
		public const uint DiffusionAnisotropyType = 0x00189147;
		
		/// <summary>(0018,9151) VR=DT Frame Reference Datetime 
		/// </summary>
		public const uint FrameReferenceDatetime = 0x00189151;
		
		/// <summary>(0018,9152) VR=SQ Metabolite Map Sequence 
		/// </summary>
		public const uint MetaboliteMapSeq = 0x00189152;
		
		/// <summary>(0018,9155) VR=FD Parallel Reduction Factor out-of-plane 
		/// </summary>
		public const uint ParallelReductionFactorOutOfPlane = 0x00189155;
		
		/// <summary>(0018,9159) VR=UL Spectroscopy Acquisition Out-of-plane Phase Steps 
		/// </summary>
		public const uint SpectroscopyAcquisitionOutOfPlanePhaseSteps = 0x00189159;
		
		/// <summary>(0018,9166) VR=CS Bulk Motion Status 
		/// </summary>
		public const uint BulkMotionStatus = 0x00189166;
		
		/// <summary>(0018,9168) VR=FD Parallel Reduction Factor Second In-plane 
		/// </summary>
		public const uint ParallelReductionFactorSecondInPlane = 0x00189168;
		
		/// <summary>(0018,9169) VR=CS Cardiac Beat Rejection Technique 
		/// </summary>
		public const uint CardiacBeatRejectionTechnique = 0x00189169;
		
		/// <summary>(0018,9170) VR=CS Respiratory Motion Compensation 
		/// </summary>
		public const uint RespiratoryMotionCompensation = 0x00189170;
		
		/// <summary>(0018,9171) VR=CS Respiratory Signal Source 
		/// </summary>
		public const uint RespiratorySignalSource = 0x00189171;
		
		/// <summary>(0018,9172) VR=CS Bulk Motion Compensation Technique 
		/// </summary>
		public const uint BulkMotionCompensationTechnique = 0x00189172;
		
		/// <summary>(0018,9173) VR=CS Bulk Motion Signal 
		/// </summary>
		public const uint BulkMotionSignal = 0x00189173;
		
		/// <summary>(0018,9174) VR=CS Applicable Safety Standard Agency 
		/// </summary>
		public const uint ApplicableSafetyStandardAgency = 0x00189174;
		
		/// <summary>(0018,9175) VR=LO Applicable Safety Standard Version 
		/// </summary>
		public const uint ApplicableSafetyStandardVersion = 0x00189175;
		
		/// <summary>(0018,9176) VR=SQ Operation Mode Sequence 
		/// </summary>
		public const uint OperationModeSeq = 0x00189176;
		
		/// <summary>(0018,9177) VR=CS Operating Mode Type 
		/// </summary>
		public const uint OperatingModeType = 0x00189177;
		
		/// <summary>(0018,9178) VR=CS Operation Mode 
		/// </summary>
		public const uint OperationMode = 0x00189178;
		
		/// <summary>(0018,9179) VR=CS Specific Absorption Rate DefInition 
		/// </summary>
		public const uint SpecificAbsorptionRateDefInition = 0x00189179;
		
		/// <summary>(0018,9180) VR=CS Gradient Output Type 
		/// </summary>
		public const uint GradientOutputType = 0x00189180;
		
		/// <summary>(0018,9181) VR=FD Specific Absorption Rate Value 
		/// </summary>
		public const uint SpecificAbsorptionRateValue = 0x00189181;
		
		/// <summary>(0018,9182) VR=FD Gradient Output 
		/// </summary>
		public const uint GradientOutput = 0x00189182;
		
		/// <summary>(0018,9183) VR=CS Flow Compensation Direction 
		/// </summary>
		public const uint FlowCompensationDirection = 0x00189183;
		
		/// <summary>(0018,9184) VR=FD Tagging Delay 
		/// </summary>
		public const uint TaggingDelay = 0x00189184;
		
		/// <summary>(0018,9195) VR=FD Chemical Shifts Minimum Integration Limit 
		/// </summary>
		public const uint ChemicalShiftsMinimumIntegrationLimit = 0x00189195;
		
		/// <summary>(0018,9196) VR=FD Chemical Shifts Maximum Integration Limit 
		/// </summary>
		public const uint ChemicalShiftsMaximumIntegrationLimit = 0x00189196;
		
		/// <summary>(0018,9197) VR=SQ MR Velocity Encoding Sequence 
		/// </summary>
		public const uint MRVelocityEncodingSeq = 0x00189197;
		
		/// <summary>(0018,9198) VR=CS First Order Phase Correction 
		/// </summary>
		public const uint FirstOrderPhaseCorrection = 0x00189198;
		
		/// <summary>(0018,9199) VR=CS Water Referenced Phase Correction 
		/// </summary>
		public const uint WaterReferencedPhaseCorrection = 0x00189199;
		
		/// <summary>(0018,9200) VR=CS MR Spectroscopy Acquisition Type 
		/// </summary>
		public const uint MRSpectroscopyAcquisitionType = 0x00189200;
		
		/// <summary>(0018,9214) VR=CS Respiratory Motion Status 
		/// </summary>
		public const uint RespiratoryMotionStatus = 0x00189214;
		
		/// <summary>(0018,9217) VR=FD Velocity Encoding Maximum Value 
		/// </summary>
		public const uint VelocityEncodingMaximumValue = 0x00189217;
		
		/// <summary>(0018,9218) VR=SS Tag Spacing Second Dimension 
		/// </summary>
		public const uint TagSpacingSecondDimension = 0x00189218;
		
		/// <summary>(0018,9219) VR=SS Tag Angle Second Axis 
		/// </summary>
		public const uint TagAngleSecondAxis = 0x00189219;
		
		/// <summary>(0018,9220) VR=FD Frame Acquisition Duration 
		/// </summary>
		public const uint FrameAcquisitionDuration = 0x00189220;
		
		/// <summary>(0018,9226) VR=SQ MR Image Frame Type Sequence 
		/// </summary>
		public const uint MRImageFrameTypeSeq = 0x00189226;
		
		/// <summary>(0018,9227) VR=SQ MR Spectroscopy Frame Type Sequence 
		/// </summary>
		public const uint MRSpectroscopyFrameTypeSeq = 0x00189227;
		
		/// <summary>(0018,9231) VR=US MR Acquisition Phase Encoding Steps in-plane 
		/// </summary>
		public const uint MRAcquisitionPhaseEncodingStepsInPlane = 0x00189231;
		
		/// <summary>(0018,9232) VR=US MR Acquisition Phase Encoding Steps out-of-plane 
		/// </summary>
		public const uint MRAcquisitionPhaseEncodingStepsOutOfPlane = 0x00189232;
		
		/// <summary>(0018,9234) VR=UL Spectroscopy Acquisition Phase Columns 
		/// </summary>
		public const uint SpectroscopyAcquisitionPhaseColumns = 0x00189234;
		
		/// <summary>(0018,9236) VR=CS Cardiac Motion Status 
		/// </summary>
		public const uint CardiacMotionStatus = 0x00189236;
		
		/// <summary>(0018,9239) VR=SQ Specific Absorption Rate Sequence 
		/// </summary>
		public const uint SpecificAbsorptionRateSeq = 0x00189239;

		/// <summary>(0018,A001) VR=SQ ContributingEquipmentSeq 
		/// </summary>
        public const uint ContributingEquipmentSeq = 0x0018a001;
        
        /// <summary>(0018,A002) VR=DT Contribution Date Time DT 
        /// /// </summary>
 		public const uint ContributionDateTime = 0x0018a0002;

        /// <summary>(0018,A003) VR=ST Contribution Description ST
		/// </summary>
		public const uint  ContributionDescriptionST = 0x0018a0003;
		
		/// <summary>(0020,000D) VR=UI Study Instance UID 
		/// </summary>
		public const uint StudyInstanceUID = 0x0020000D;
		
		/// <summary>(0020,000E) VR=UI Series Instance UID 
		/// </summary>
		public const uint SeriesInstanceUID = 0x0020000E;
		
		/// <summary>(0020,0010) VR=SH Study ID 
		/// </summary>
		public const uint StudyID = 0x00200010;
		
		/// <summary>(0020,0011) VR=IS Series Number 
		/// </summary>
		public const uint SeriesNumber = 0x00200011;
		
		/// <summary>(0020,0012) VR=IS Acquisition Number 
		/// </summary>
		public const uint AcquisitionNumber = 0x00200012;
		
		/// <summary>(0020,0013) VR=IS Instance Number 
		/// </summary>
		public const uint InstanceNumber = 0x00200013;
		
		/// <summary>(0020,0014) VR=IS Isotope Number (Retired) 
		/// </summary>
		public const uint IsotopeNumberRetired = 0x00200014;
		
		/// <summary>(0020,0015) VR=IS Phase Number (Retired) 
		/// </summary>
		public const uint PhaseNumberRetired = 0x00200015;
		
		/// <summary>(0020,0016) VR=IS Interval Number (Retired) 
		/// </summary>
		public const uint IntervalNumberRetired = 0x00200016;
		
		/// <summary>(0020,0017) VR=IS Time Slot Number (Retired) 
		/// </summary>
		public const uint TimeSlotNumberRetired = 0x00200017;
		
		/// <summary>(0020,0018) VR=IS Angle Number (Retired) 
		/// </summary>
		public const uint AngleNumberRetired = 0x00200018;
		
		/// <summary>(0020,0019) VR=IS Item Number 
		/// </summary>
		public const uint ItemNumber = 0x00200019;
		
		/// <summary>(0020,0020) VR=CS Patient Orientation 
		/// </summary>
		public const uint PatientOrientation = 0x00200020;
		
		/// <summary>(0020,0022) VR=IS Overlay Number 
		/// </summary>
		public const uint OverlayNumber = 0x00200022;
		
		/// <summary>(0020,0024) VR=IS Curve Number 
		/// </summary>
		public const uint CurveNumber = 0x00200024;
		
		/// <summary>(0020,0026) VR=IS Lookup Table Number 
		/// </summary>
		public const uint LUTNumber = 0x00200026;
		
		/// <summary>(0020,0030) VR=DS Image Position (Retired) 
		/// </summary>
		public const uint ImagePositionRetired = 0x00200030;
		
		/// <summary>(0020,0032) VR=DS Image Position (Patient) 
		/// </summary>
		public const uint ImagePosition = 0x00200032;
		
		/// <summary>(0020,0035) VR=DS Image Orientation (Retired) 
		/// </summary>
		public const uint ImageOrientationRetired = 0x00200035;
		
		/// <summary>(0020,0037) VR=DS Image Orientation (Patient) 
		/// </summary>
		public const uint ImageOrientation = 0x00200037;
		
		/// <summary>(0020,0050) VR=UN Location (Retired) 
		/// </summary>
		public const uint LocationRetired = 0x00200050;
		
		/// <summary>(0020,0052) VR=UI Frame of Reference UID 
		/// </summary>
		public const uint FrameOfReferenceUID = 0x00200052;
		
		/// <summary>(0020,0060) VR=CS Laterality 
		/// </summary>
		public const uint Laterality = 0x00200060;
		
		/// <summary>(0020,0062) VR=CS Image Laterality 
		/// </summary>
		public const uint ImageLaterality = 0x00200062;
		
		/// <summary>(0020,0070) VR=CS Image Geometry Type (Retired) 
		/// </summary>
		public const uint ImageGeometryTypeRetired = 0x00200070;
		
		/// <summary>(0020,0080) VR=CS Masking Image (Retired) 
		/// </summary>
		public const uint MaskingImageRetired = 0x00200080;
		
		/// <summary>(0020,0100) VR=IS Temporal Position Identifier 
		/// </summary>
		public const uint TemporalPositionIdentifier = 0x00200100;
		
		/// <summary>(0020,0105) VR=IS Number of Temporal Positions 
		/// </summary>
		public const uint NumberOfTemporalPositions = 0x00200105;
		
		/// <summary>(0020,0110) VR=DS Temporal Resolution 
		/// </summary>
		public const uint TemporalResolution = 0x00200110;
		
		/// <summary>(0020,0200) VR=UI Synchronization Frame of Reference UID 
		/// </summary>
		public const uint SynchronizationFrameOfReferenceUID = 0x00200200;
		
		/// <summary>(0020,1000) VR=IS Series in Study 
		/// </summary>
		public const uint SeriesInStudy = 0x00201000;
		
		/// <summary>(0020,1001) VR=IS Acquisitions in Series (Retired) 
		/// </summary>
		public const uint AcquisitionsInSeriesRetired = 0x00201001;
		
		/// <summary>(0020,1002) VR=IS Images in Acquisition 
		/// </summary>
		public const uint ImagesInAcquisition = 0x00201002;
		
		/// <summary>(0020,1004) VR=IS Acquisitions in Study 
		/// </summary>
		public const uint AcquisitionsInStudy = 0x00201004;
		
		/// <summary>(0020,1020) VR=UN Reference (Retired) 
		/// </summary>
		public const uint ReferenceRetired = 0x00201020;
		
		/// <summary>(0020,1040) VR=LO Position Reference Indicator 
		/// </summary>
		public const uint PositionReferenceIndicator = 0x00201040;
		
		/// <summary>(0020,1041) VR=DS Slice Location 
		/// </summary>
		public const uint SliceLocation = 0x00201041;
		
		/// <summary>(0020,1070) VR=IS Other Study Numbers 
		/// </summary>
		public const uint OtherStudyNumbers = 0x00201070;
		
		/// <summary>(0020,1200) VR=IS Number of Patient Related Studies 
		/// </summary>
		public const uint NumberOfPatientRelatedStudies = 0x00201200;
		
		/// <summary>(0020,1202) VR=IS Number of Patient Related Series 
		/// </summary>
		public const uint NumberOfPatientRelatedSeries = 0x00201202;
		
		/// <summary>(0020,1204) VR=IS Number of Patient Related Instances 
		/// </summary>
		public const uint NumberOfPatientRelatedInstances = 0x00201204;
		
		/// <summary>(0020,1206) VR=IS Number of Study Related Series 
		/// </summary>
		public const uint NumberOfStudyRelatedSeries = 0x00201206;
		
		/// <summary>(0020,1208) VR=IS Number of Study Related Instances 
		/// </summary>
		public const uint NumberOfStudyRelatedInstances = 0x00201208;
		
		/// <summary>(0020,1209) VR=IS Number of Series Related Instances 
		/// </summary>
		public const uint NumberOfSeriesRelatedInstances = 0x00201209;
		
		/// <summary>(0020,31xx) VR=SH Source Image ID (Retired) 
		/// </summary>
		public const uint SourceImageIDRetired = 0x00203100;
		
		/// <summary>(0020,3401) VR=SH Modifying Device ID (Retired) 
		/// </summary>
		public const uint ModifyingDeviceIDRetired = 0x00203401;
		
		/// <summary>(0020,3402) VR=SH Modified Image ID (Retired) 
		/// </summary>
		public const uint ModifiedImageIDRetired = 0x00203402;
		
		/// <summary>(0020,3403) VR=DA Modified Image Date (Retired) 
		/// </summary>
		public const uint ModifiedImageDateRetired = 0x00203403;
		
		/// <summary>(0020,3404) VR=LO Modifying Device Manufacturer (Retired) 
		/// </summary>
		public const uint ModifyingDeviceManufacturerRetired = 0x00203404;
		
		/// <summary>(0020,3405) VR=TM Modified Image Time (Retired) 
		/// </summary>
		public const uint ModifiedImageTimeRetired = 0x00203405;
		
		/// <summary>(0020,3406) VR=LO Modified Image Description (Retired) 
		/// </summary>
		public const uint ModifiedImageDescriptionRetired = 0x00203406;
		
		/// <summary>(0020,4000) VR=LT Image Comments 
		/// </summary>
		public const uint ImageComments = 0x00204000;
		
		/// <summary>(0020,5000) VR=LO Original Image Identification (Retired) 
		/// </summary>
		public const uint OriginalImageIdentificationRetired = 0x00205000;
		
		/// <summary>(0020,5002) VR=LO Original Image Identification Nomenclature (Retired) 
		/// </summary>
		public const uint OriginalImageIdentificationNomenclatureRetired = 0x00205002;
		
		/// <summary>(0020,9056) VR=SH Stack ID 
		/// </summary>
		public const uint StackID = 0x00209056;
		
		/// <summary>(0020,9057) VR=UL In-Stack Position Number 
		/// </summary>
		public const uint InStackPositionNumber = 0x00209057;
		
		/// <summary>(0020,9071) VR=SQ Frame Anatomy Sequence 
		/// </summary>
		public const uint FrameAnatomySeq = 0x00209071;
		
		/// <summary>(0020,9072) VR=CS Frame Laterality 
		/// </summary>
		public const uint FrameLaterality = 0x00209072;
		
		/// <summary>(0020,9111) VR=SQ Frame Content Sequence 
		/// </summary>
		public const uint FrameContentSeq = 0x00209111;
		
		/// <summary>(0020,9113) VR=SQ Plane Position Sequence 
		/// </summary>
		public const uint PlanePositionSeq = 0x00209113;
		
		/// <summary>(0020,9116) VR=SQ Plane Orientation Sequence 
		/// </summary>
		public const uint PlaneOrientationSeq = 0x00209116;
		
		/// <summary>(0020,9128) VR=UL Temporal Position Index 
		/// </summary>
		public const uint TemporalPositionIndex = 0x00209128;
		
		/// <summary>(0020,9153) VR=FD Trigger Delay Time 
		/// </summary>
		public const uint TriggerDelayTime = 0x00209153;
		
		/// <summary>(0020,9156) VR=US Frame Acquisition Number 
		/// </summary>
		public const uint FrameAcquisitionNumber = 0x00209156;
		
		/// <summary>(0020,9157) VR=UL Dimension Index Values 
		/// </summary>
		public const uint DimensionIndexValues = 0x00209157;
		
		/// <summary>(0020,9158) VR=LT Frame Comments 
		/// </summary>
		public const uint FrameComments = 0x00209158;
		
		/// <summary>(0020,9161) VR=UI Concatenation UID 
		/// </summary>
		public const uint ConcatenationUID = 0x00209161;
		
		/// <summary>(0020,9162) VR=US In-concatenation Number 
		/// </summary>
		public const uint InConcatenationNumber = 0x00209162;
		
		/// <summary>(0020,9163) VR=US In-concatenation Total Number 
		/// </summary>
		public const uint InConcatenationTotalNumber = 0x00209163;
		
		/// <summary>(0020,9164) VR=UI Dimension Organization UID 
		/// </summary>
		public const uint DimensionOrganizationUID = 0x00209164;
		
		/// <summary>(0020,9165) VR=AT Dimension Index Pointer 
		/// </summary>
		public const uint DimensionIndexPointer = 0x00209165;
		
		/// <summary>(0020,9167) VR=AT Functional Group Sequence Pointer 
		/// </summary>
		public const uint FunctionalGroupSequencePointer = 0x00209167;
		
		/// <summary>(0020,9213) VR=LO Dimension Index Private Creator 
		/// </summary>
		public const uint DimensionIndexPrivateCreator = 0x00209213;
		
		/// <summary>(0020,9221) VR=SQ Dimension Organization Sequence 
		/// </summary>
		public const uint DimensionOrganizationSeq = 0x00209221;
		
		/// <summary>(0020,9222) VR=SQ Dimension Sequence 
		/// </summary>
		public const uint DimensionSeq = 0x00209222;
		
		/// <summary>(0020,9228) VR=UL Concatenation Frame Offset Number 
		/// </summary>
		public const uint ConcatenationFrameOffsetNumber = 0x00209228;
		
		/// <summary>(0020,9238) VR=LO Functional Group Private Creator 
		/// </summary>
		public const uint FunctionalGroupPrivateCreator = 0x00209238;
		
		/// <summary>(0028,0002) VR=US Samples per Pixel 
		/// </summary>
		public const uint SamplesPerPixel = 0x00280002;
		
		/// <summary>(0028,0005) VR=UN Image Dimensions (Retired) 
		/// </summary>
		public const uint ImageDimensionsRetired = 0x00280005;
		
		/// <summary>(0028,0004) VR=CS Photometric Interpretation 
		/// </summary>
		public const uint PhotometricInterpretation = 0x00280004;
		
		/// <summary>(0028,0006) VR=US Planar Configuration 
		/// </summary>
		public const uint PlanarConfiguration = 0x00280006;
		
		/// <summary>(0028,0008) VR=IS Number of Frames 
		/// </summary>
		public const uint NumberOfFrames = 0x00280008;
		
		/// <summary>(0028,0009) VR=AT Frame Increment Pointer 
		/// </summary>
		public const uint FrameIncrementPointer = 0x00280009;
		
		/// <summary>(0028,0010) VR=US Rows 
		/// </summary>
		public const uint Rows = 0x00280010;
		
		/// <summary>(0028,0011) VR=US Columns 
		/// </summary>
		public const uint Columns = 0x00280011;
		
		/// <summary>(0028,0012) VR=US Planes 
		/// </summary>
		public const uint Planes = 0x00280012;
		
		/// <summary>(0028,0014) VR=US Ultrasound Color Data Present 
		/// </summary>
		public const uint UltrasoundColorDataPresent = 0x00280014;
		
		/// <summary>(0028,0030) VR=DS Pixel Spacing 
		/// </summary>
		public const uint PixelSpacing = 0x00280030;
		
		/// <summary>(0028,0031) VR=DS Zoom Factor 
		/// </summary>
		public const uint ZoomFactor = 0x00280031;
		
		/// <summary>(0028,0032) VR=DS Zoom Center 
		/// </summary>
		public const uint ZoomCenter = 0x00280032;
		
		/// <summary>(0028,0034) VR=IS Pixel Aspect Ratio 
		/// </summary>
		public const uint PixelAspectRatio = 0x00280034;
		
		/// <summary>(0028,0040) VR=UN Image Format (Retired) 
		/// </summary>
		public const uint ImageFormatRetired = 0x00280040;
		
		/// <summary>(0028,0050) VR=UN Manipulated Image (Retired) 
		/// </summary>
		public const uint ManipulatedImageRetired = 0x00280050;
		
		/// <summary>(0028,0051) VR=CS Corrected Image 
		/// </summary>
		public const uint CorrectedImage = 0x00280051;
		
		/// <summary>(0028,0060) VR=CS Compression Code (Retired) 
		/// </summary>
		public const uint CompressionCodeRetired = 0x00280060;
		
		/// <summary>(0028,0100) VR=US Bits Allocated 
		/// </summary>
		public const uint BitsAllocated = 0x00280100;
		
		/// <summary>(0028,0101) VR=US Bits Stored 
		/// </summary>
		public const uint BitsStored = 0x00280101;
		
		/// <summary>(0028,0102) VR=US High Bit 
		/// </summary>
		public const uint HighBit = 0x00280102;
		
		/// <summary>(0028,0103) VR=US Pixel Representation 
		/// </summary>
		public const uint PixelRepresentation = 0x00280103;
		
		/// <summary>(0028,0104) VR=US,SS Smallest Valid Pixel Value (Retired) 
		/// </summary>
		public const uint SmallestValidPixelValueRetired = 0x00280104;
		
		/// <summary>(0028,0105) VR=US,SS Largest Valid Pixel Value (Retired) 
		/// </summary>
		public const uint LargestValidPixelValueRetired = 0x00280105;
		
		/// <summary>(0028,0106) VR=US,SS Smallest Image Pixel Value 
		/// </summary>
		public const uint SmallestImagePixelValue = 0x00280106;
		
		/// <summary>(0028,0107) VR=US,SS Largest Image Pixel Value 
		/// </summary>
		public const uint LargestImagePixelValue = 0x00280107;
		
		/// <summary>(0028,0108) VR=US,SS Smallest Pixel Value in Series 
		/// </summary>
		public const uint SmallestPixelValueInSeries = 0x00280108;
		
		/// <summary>(0028,0109) VR=US,SS Largest Pixel Value in Series 
		/// </summary>
		public const uint LargestPixelValueInSeries = 0x00280109;
		
		/// <summary>(0028,0110) VR=US,SS Smallest Image Pixel Value in Plane 
		/// </summary>
		public const uint SmallestImagePixelValueInPlane = 0x00280110;
		
		/// <summary>(0028,0111) VR=US,SS Largest Image Pixel Value in Plane 
		/// </summary>
		public const uint LargestImagePixelValueInPlane = 0x00280111;
		
		/// <summary>(0028,0120) VR=US,SS Pixel Padding Value 
		/// </summary>
		public const uint PixelPaddingValue = 0x00280120;
		
		/// <summary>(0028,0200) VR=UN Image Location (Retired) 
		/// </summary>
		public const uint ImageLocationRetired = 0x00280200;
		
		/// <summary>(0028,0300) VR=CS Quality Control Image 
		/// </summary>
		public const uint QualityControlImage = 0x00280300;
		
		/// <summary>(0028,0301) VR=CS Burned In Annotation 
		/// </summary>
		public const uint BurnedInAnnotation = 0x00280301;
		
		/// <summary>(0028,1040) VR=CS Pixel Intensity Relationship 
		/// </summary>
		public const uint PixelIntensityRelationship = 0x00281040;
		
		/// <summary>(0028,1041) VR=SS Pixel Intensity Relationship Sign 
		/// </summary>
		public const uint PixelIntensityRelationshipSign = 0x00281041;
		
		/// <summary>(0028,1050) VR=DS Window Center 
		/// </summary>
		public const uint WindowCenter = 0x00281050;
		
		/// <summary>(0028,1051) VR=DS Window Width 
		/// </summary>
		public const uint WindowWidth = 0x00281051;
		
		/// <summary>(0028,1052) VR=DS Rescale Intercept 
		/// </summary>
		public const uint RescaleIntercept = 0x00281052;
		
		/// <summary>(0028,1053) VR=DS Rescale Slope 
		/// </summary>
		public const uint RescaleSlope = 0x00281053;
		
		/// <summary>(0028,1054) VR=LO Rescale Type 
		/// </summary>
		public const uint RescaleType = 0x00281054;
		
		/// <summary>(0028,1055) VR=LO Window Center & Width Explanation 
		/// </summary>
		public const uint WindowCenterWidthExplanation = 0x00281055;
		
		/// <summary>(0028,1080) VR=UN Gray Scale (Retired) 
		/// </summary>
		public const uint GrayScaleRetired = 0x00281080;
		
		/// <summary>(0028,1090) VR=CS Recommended Viewing Mode 
		/// </summary>
		public const uint RecommendedViewingMode = 0x00281090;
		
		/// <summary>(0028,1100) VR=US,SS Gray Lookup Table Descriptor (Retired) 
		/// </summary>
		public const uint GreyLUTDescriptorRetired = 0x00281100;
		
		/// <summary>(0028,1101) VR=US,SS Red Palette Color Lookup Table Descriptor 
		/// </summary>
		public const uint RedPaletteColorLUTDescriptor = 0x00281101;
		
		/// <summary>(0028,1102) VR=US,SS Green Palette Color Lookup Table Descriptor 
		/// </summary>
		public const uint GreenPaletteColorLUTDescriptor = 0x00281102;
		
		/// <summary>(0028,1103) VR=US,SS Blue Palette Color Lookup Table Descriptor 
		/// </summary>
		public const uint BluePaletteColorLUTDescriptor = 0x00281103;
		
		/// <summary>(0028,1199) VR=UI Palette Color Lookup Table UID 
		/// </summary>
		public const uint PaletteColorLUTUID = 0x00281199;
		
		/// <summary>(0028,1201) VR=OW Red Palette Color Lookup Table Data 
		/// </summary>
		public const uint RedPaletteColorLUTData = 0x00281201;
		
		/// <summary>(0028,1202) VR=OW Green Palette Color Lookup Table Data 
		/// </summary>
		public const uint GreenPaletteColorLUTData = 0x00281202;
		
		/// <summary>(0028,1203) VR=OW Blue Palette Color Lookup Table Data 
		/// </summary>
		public const uint BluePaletteColorLUTData = 0x00281203;
		
		/// <summary>(0028,1221) VR=OW Segmented Red Palette Color Lookup Table Data 
		/// </summary>
		public const uint SegmentedRedPaletteColorLUTData = 0x00281221;
		
		/// <summary>(0028,1222) VR=OW Segmented Green Palette Color Lookup Table Data 
		/// </summary>
		public const uint SegmentedGreenPaletteColorLUTData = 0x00281222;
		
		/// <summary>(0028,1223) VR=OW Segmented Blue Palette Color Lookup Table Data 
		/// </summary>
		public const uint SegmentedBluePaletteColorLUTData = 0x00281223;
		
		/// <summary>(0028,1300) VR=CS Implant Present 
		/// </summary>
		public const uint ImplantPresent = 0x00281300;
		
		/// <summary>(0028,1350) VR=CS Partial View 
		/// </summary>
		public const uint PartialView = 0x00281350;
		
		/// <summary>(0028,1351) VR=ST Partial View Description 
		/// </summary>
		public const uint PartialViewDescription = 0x00281351;
		
		/// <summary>(0028,2110) VR=CS Lossy Image Compression 
		/// </summary>
		public const uint LossyImageCompression = 0x00282110;
		
		/// <summary>(0028,2112) VR=DS Lossy Image Compression Ratio 
		/// </summary>
		public const uint LossyImageCompressionRatio = 0x00282112;
		
		/// <summary>(0028,3000) VR=SQ Modality LUT Sequence 
		/// </summary>
		public const uint ModalityLUTSeq = 0x00283000;
		
		/// <summary>(0028,3002) VR=US,SS LUT Descriptor 
		/// </summary>
		public const uint LUTDescriptor = 0x00283002;
		
		/// <summary>(0028,3003) VR=LO LUT Explanation 
		/// </summary>
		public const uint LUTExplanation = 0x00283003;
		
		/// <summary>(0028,3004) VR=LO Modality LUT Type 
		/// </summary>
		public const uint ModalityLUTType = 0x00283004;
		
		/// <summary>(0028,3006) VR=OW,US,SS LUT Data 
		/// </summary>
		public const uint LUTData = 0x00283006;
		
		/// <summary>(0028,3010) VR=SQ VOI LUT Sequence 
		/// </summary>
		public const uint VOILUTSeq = 0x00283010;
		
		/// <summary>(0028,3110) VR=SQ Softcopy VOI LUT Sequence 
		/// </summary>
		public const uint SoftcopyVOILUTSeq = 0x00283110;
		
		/// <summary>(0028,4000) VR=LT (Pixel) Comments (Retired) 
		/// </summary>
		public const uint PixelCommentsRetired = 0x00284000;
		
		/// <summary>(0028,5000) VR=SQ Bi-Plane Acquisition Sequence 
		/// </summary>
		public const uint BiPlaneAcquisitionSeq = 0x00285000;
		
		/// <summary>(0028,6010) VR=US Representative Frame Number 
		/// </summary>
		public const uint RepresentativeFrameNumber = 0x00286010;
		
		/// <summary>(0028,6020) VR=US Frame Numbers of Interest (FOI) 
		/// </summary>
		public const uint FrameNumbersOfInterest = 0x00286020;
		
		/// <summary>(0028,6022) VR=LO Frame(s) of Interest Description 
		/// </summary>
		public const uint FrameOfInterestDescription = 0x00286022;
		
		/// <summary>(0028,6030) VR=US Mask Pointer(s) 
		/// </summary>
		public const uint MaskPointer = 0x00286030;
		
		/// <summary>(0028,6040) VR=US R Wave Pointer 
		/// </summary>
		public const uint RWavePointer = 0x00286040;
		
		/// <summary>(0028,6100) VR=SQ Mask Subtraction Sequence 
		/// </summary>
		public const uint MaskSubtractionSeq = 0x00286100;
		
		/// <summary>(0028,6101) VR=CS Mask Operation 
		/// </summary>
		public const uint MaskOperation = 0x00286101;
		
		/// <summary>(0028,6102) VR=US Applicable Frame Range 
		/// </summary>
		public const uint ApplicableFrameRange = 0x00286102;
		
		/// <summary>(0028,6110) VR=US Mask Frame Numbers 
		/// </summary>
		public const uint MaskFrameNumbers = 0x00286110;
		
		/// <summary>(0028,6112) VR=US Contrast Frame Averaging 
		/// </summary>
		public const uint ContrastFrameAveraging = 0x00286112;
		
		/// <summary>(0028,6114) VR=FL Mask Sub-pixel Shift 
		/// </summary>
		public const uint MaskSubPixelShift = 0x00286114;
		
		/// <summary>(0028,6120) VR=SS TID Offset 
		/// </summary>
		public const uint TIDOffset = 0x00286120;
		
		/// <summary>(0028,6190) VR=ST Mask Operation Explanation 
		/// </summary>
		public const uint MaskOperationExplanation = 0x00286190;
		
		/// <summary>(0028,9001) VR=UL Data Point Rows 
		/// </summary>
		public const uint DataPointRows = 0x00289001;
		
		/// <summary>(0028,9002) VR=UL Data Point Columns 
		/// </summary>
		public const uint DataPointColumns = 0x00289002;
		
		/// <summary>(0028,9003) VR=CS Signal Domain 
		/// </summary>
		public const uint SignalDomain = 0x00289003;
		
		/// <summary>(0028,9099) VR=US Largest Monochrome Pixel Value 
		/// </summary>
		public const uint LargestMonochromePixelValue = 0x00289099;
		
		/// <summary>(0028,9108) VR=CS Data Representation 
		/// </summary>
		public const uint DataRepresentation = 0x00289108;
		
		/// <summary>(0028,9110) VR=SQ Pixel Matrix Sequence 
		/// </summary>
		public const uint PixelMatrixSeq = 0x00289110;
		
		/// <summary>(0028,9132) VR=SQ Frame VOI LUT Sequence 
		/// </summary>
		public const uint FrameVOILUTSeq = 0x00289132;
		
		/// <summary>(0028,9145) VR=SQ Pixel Value Transformation Sequence 
		/// </summary>
		public const uint PixelValueTransformationSeq = 0x00289145;
		
		/// <summary>(0028,9235) VR=CS Signal Domain Rows 
		/// </summary>
		public const uint SignalDomainRows = 0x00289235;
		
		/// <summary>(0032,000A) VR=CS Study Status ID 
		/// </summary>
		public const uint StudyStatusID = 0x0032000A;
		
		/// <summary>(0032,000C) VR=CS Study Priority ID 
		/// </summary>
		public const uint StudyPriorityID = 0x0032000C;
		
		/// <summary>(0032,0012) VR=LO Study ID Issuer 
		/// </summary>
		public const uint StudyIDIssuer = 0x00320012;
		
		/// <summary>(0032,0032) VR=DA Study Verified Date 
		/// </summary>
		public const uint StudyVerifiedDate = 0x00320032;
		
		/// <summary>(0032,0033) VR=TM Study Verified Time 
		/// </summary>
		public const uint StudyVerifiedTime = 0x00320033;
		
		/// <summary>(0032,0034) VR=DA Study Read Date 
		/// </summary>
		public const uint StudyReadDate = 0x00320034;
		
		/// <summary>(0032,0035) VR=TM Study Read Time 
		/// </summary>
		public const uint StudyReadTime = 0x00320035;
		
		/// <summary>(0032,1000) VR=DA Scheduled Study Start Date 
		/// </summary>
		public const uint ScheduledStudyStartDate = 0x00321000;
		
		/// <summary>(0032,1001) VR=TM Scheduled Study Start Time 
		/// </summary>
		public const uint ScheduledStudyStartTime = 0x00321001;
		
		/// <summary>(0032,1010) VR=DA Scheduled Study Stop Date 
		/// </summary>
		public const uint ScheduledStudyStopDate = 0x00321010;
		
		/// <summary>(0032,1011) VR=TM Scheduled Study Stop Time 
		/// </summary>
		public const uint ScheduledStudyStopTime = 0x00321011;
		
		/// <summary>(0032,1020) VR=LO Scheduled Study Location 
		/// </summary>
		public const uint ScheduledStudyLocation = 0x00321020;
		
		/// <summary>(0032,1021) VR=AE Scheduled Study Location AE Title(s) 
		/// </summary>
		public const uint ScheduledStudyLocationAET = 0x00321021;
		
		/// <summary>(0032,1030) VR=LO Reason for Study 
		/// </summary>
		public const uint ReasonforStudy = 0x00321030;
		
		/// <summary>(0032,1032) VR=PN Requesting Physician 
		/// </summary>
		public const uint RequestingPhysician = 0x00321032;
		
		/// <summary>(0032,1033) VR=LO Requesting Service 
		/// </summary>
		public const uint RequestingService = 0x00321033;
		
		/// <summary>(0032,1040) VR=DA Study Arrival Date 
		/// </summary>
		public const uint StudyArrivalDate = 0x00321040;
		
		/// <summary>(0032,1041) VR=TM Study Arrival Time 
		/// </summary>
		public const uint StudyArrivalTime = 0x00321041;
		
		/// <summary>(0032,1050) VR=DA Study Completion Date 
		/// </summary>
		public const uint StudyCompletionDate = 0x00321050;
		
		/// <summary>(0032,1051) VR=TM Study Completion Time 
		/// </summary>
		public const uint StudyCompletionTime = 0x00321051;
		
		/// <summary>(0032,1055) VR=CS Study Component Status ID 
		/// </summary>
		public const uint StudyComponentStatusID = 0x00321055;
		
		/// <summary>(0032,1060) VR=LO Requested Procedure Description 
		/// </summary>
		public const uint RequestedProcedureDescription = 0x00321060;
		
		/// <summary>(0032,1064) VR=SQ Requested Procedure Code Sequence 
		/// </summary>
		public const uint RequestedProcedureCodeSeq = 0x00321064;
		
		/// <summary>(0032,1070) VR=LO Requested Contrast Agent 
		/// </summary>
		public const uint RequestedContrastAgent = 0x00321070;
		
		/// <summary>(0032,4000) VR=LT Study Comments 
		/// </summary>
		public const uint StudyComments = 0x00324000;
		
		/// <summary>(0038,0004) VR=SQ Referenced Patient Alias Sequence 
		/// </summary>
		public const uint RefPatientAliasSeq = 0x00380004;
		
		/// <summary>(0038,0008) VR=CS Visit Status ID 
		/// </summary>
		public const uint VisitStatusID = 0x00380008;
		
		/// <summary>(0038,0010) VR=LO Admission ID 
		/// </summary>
		public const uint AdmissionID = 0x00380010;
		
		/// <summary>(0038,0011) VR=LO Issuer of Admission ID 
		/// </summary>
		public const uint IssuerOfAdmissionID = 0x00380011;
		
		/// <summary>(0038,0016) VR=LO Route of Admissions 
		/// </summary>
		public const uint RouteOfAdmissions = 0x00380016;
		
		/// <summary>(0038,001A) VR=DA Scheduled Admission Date 
		/// </summary>
		public const uint ScheduledAdmissionDate = 0x0038001A;
		
		/// <summary>(0038,001B) VR=TM Scheduled Admission Time 
		/// </summary>
		public const uint ScheduledAdmissionTime = 0x0038001B;
		
		/// <summary>(0038,001C) VR=DA Scheduled Discharge Date 
		/// </summary>
		public const uint ScheduledDischargeDate = 0x0038001C;
		
		/// <summary>(0038,001D) VR=TM Scheduled Discharge Time 
		/// </summary>
		public const uint ScheduledDischargeTime = 0x0038001D;
		
		/// <summary>(0038,001E) VR=LO Scheduled Patient Institution Residence 
		/// </summary>
		public const uint ScheduledPatientInstitutionResidence = 0x0038001E;
		
		/// <summary>(0038,0020) VR=DA Admitting Date 
		/// </summary>
		public const uint AdmittingDate = 0x00380020;
		
		/// <summary>(0038,0021) VR=TM Admitting Time 
		/// </summary>
		public const uint AdmittingTime = 0x00380021;
		
		/// <summary>(0038,0030) VR=DA Discharge Date 
		/// </summary>
		public const uint DischargeDate = 0x00380030;
		
		/// <summary>(0038,0032) VR=TM Discharge Time 
		/// </summary>
		public const uint DischargeTime = 0x00380032;
		
		/// <summary>(0038,0040) VR=LO Discharge Diagnosis Description 
		/// </summary>
		public const uint DischargeDiagnosisDescription = 0x00380040;
		
		/// <summary>(0038,0044) VR=SQ Discharge Diagnosis Code Sequence 
		/// </summary>
		public const uint DischargeDiagnosisCodeSeq = 0x00380044;
		
		/// <summary>(0038,0050) VR=LO Special Needs 
		/// </summary>
		public const uint SpecialNeeds = 0x00380050;
		
		/// <summary>(0038,0300) VR=LO Current Patient Location 
		/// </summary>
		public const uint CurrentPatientLocation = 0x00380300;
		
		/// <summary>(0038,0400) VR=LO Patient's Institution Residence 
		/// </summary>
		public const uint PatientInstitutionResidence = 0x00380400;
		
		/// <summary>(0038,0500) VR=LO Patient State 
		/// </summary>
		public const uint PatientState = 0x00380500;
		
		/// <summary>(0038,4000) VR=LT Visit Comments 
		/// </summary>
		public const uint VisitComments = 0x00384000;
		
		/// <summary>(003A,0004) VR=CS Waveform Originality 
		/// </summary>
		public const uint WaveformOriginality = 0x003A0004;
		
		/// <summary>(003A,0005) VR=US Number of Waveform Channels 
		/// </summary>
		public const uint NumberOfWaveformChannels = 0x003A0005;
		
		/// <summary>(003A,0010) VR=UL Number of Waveform Samples 
		/// </summary>
		public const uint NumberOfWaveformSamples = 0x003A0010;
		
		/// <summary>(003A,001A) VR=DS Sampling Frequency 
		/// </summary>
		public const uint SamplingFrequency = 0x003A001A;
		
		/// <summary>(003A,0020) VR=SH Multiplex Group Label 
		/// </summary>
		public const uint MultiplexGroupLabel = 0x003A0020;
		
		/// <summary>(003A,0200) VR=SQ Channel DefInition Sequence 
		/// </summary>
		public const uint ChannelDefInitionSeq = 0x003A0200;
		
		/// <summary>(003A,0202) VR=IS Waveform Channel Number 
		/// </summary>
		public const uint WaveformChannelNumber = 0x003A0202;
		
		/// <summary>(003A,0203) VR=SH Channel Label 
		/// </summary>
		public const uint ChannelLabel = 0x003A0203;
		
		/// <summary>(003A,0205) VR=CS Channel Status 
		/// </summary>
		public const uint ChannelStatus = 0x003A0205;
		
		/// <summary>(003A,0208) VR=SQ Channel Source Sequence 
		/// </summary>
		public const uint ChannelSourceSeq = 0x003A0208;
		
		/// <summary>(003A,0209) VR=SQ Channel Source Modifiers Sequence 
		/// </summary>
		public const uint ChannelSourceModifiersSeq = 0x003A0209;
		
		/// <summary>(003A,020A) VR=SQ Source Waveform Sequence 
		/// </summary>
		public const uint SourceWaveformSeq = 0x003A020A;
		
		/// <summary>(003A,020C) VR=LO Channel Derivation Description 
		/// </summary>
		public const uint ChannelDerivationDescription = 0x003A020C;
		
		/// <summary>(003A,0210) VR=DS Channel Sensitivity 
		/// </summary>
		public const uint ChannelSensitivity = 0x003A0210;
		
		/// <summary>(003A,0211) VR=SQ Channel Sensitivity Units Sequence 
		/// </summary>
		public const uint ChannelSensitivityUnitsSeq = 0x003A0211;
		
		/// <summary>(003A,0212) VR=DS Channel Sensitivity Correction Factor 
		/// </summary>
		public const uint ChannelSensitivityCorrectionFactor = 0x003A0212;
		
		/// <summary>(003A,0213) VR=DS Channel Baseline 
		/// </summary>
		public const uint ChannelBaseline = 0x003A0213;
		
		/// <summary>(003A,0214) VR=DS Channel Time Skew 
		/// </summary>
		public const uint ChannelTimeSkew = 0x003A0214;
		
		/// <summary>(003A,0215) VR=DS Channel Sample Skew 
		/// </summary>
		public const uint ChannelSampleSkew = 0x003A0215;
		
		/// <summary>(003A,0218) VR=DS Channel Offset 
		/// </summary>
		public const uint ChannelOffset = 0x003A0218;
		
		/// <summary>(003A,021A) VR=US Waveform Bits Stored 
		/// </summary>
		public const uint WaveformBitsStored = 0x003A021A;
		
		/// <summary>(003A,0220) VR=DS Filter Low Frequency 
		/// </summary>
		public const uint FilterLowFrequency = 0x003A0220;
		
		/// <summary>(003A,0221) VR=DS Filter High Frequency 
		/// </summary>
		public const uint FilterHighFrequency = 0x003A0221;
		
		/// <summary>(003A,0222) VR=DS Notch Filter Frequency 
		/// </summary>
		public const uint NotchFilterFrequency = 0x003A0222;
		
		/// <summary>(003A,0223) VR=DS Notch Filter Bandwidth 
		/// </summary>
		public const uint NotchFilterBandwidth = 0x003A0223;
		
		/// <summary>(0040,0001) VR=AE Scheduled Station AE Title 
		/// </summary>
		public const uint ScheduledStationAET = 0x00400001;
		
		/// <summary>(0040,0002) VR=DA Scheduled Procedure Step Start Date 
		/// </summary>
		public const uint SPSStartDate = 0x00400002;
		
		/// <summary>(0040,0003) VR=TM Scheduled Procedure Step Start Time 
		/// </summary>
		public const uint SPSStartTime = 0x00400003;
		
		/// <summary>(0040,0004) VR=DA Scheduled Procedure Step End Date 
		/// </summary>
		public const uint SPSEndDate = 0x00400004;
		
		/// <summary>(0040,0005) VR=TM Scheduled Procedure Step End Time 
		/// </summary>
		public const uint SPSEndTime = 0x00400005;
		
		/// <summary>(0040,0006) VR=PN Scheduled Performing Physician's Name 
		/// </summary>
		public const uint ScheduledPerformingPhysicianName = 0x00400006;
		
		/// <summary>(0040,0007) VR=LO Scheduled Procedure Step Description 
		/// </summary>
		public const uint SPSDescription = 0x00400007;
		
		/// <summary>(0040,0008) VR=SQ Scheduled Protocol Code Sequence 
		/// </summary>
		public const uint ScheduledProtocolCodeSeq = 0x00400008;
		
		/// <summary>(0040,0009) VR=SH Scheduled Procedure Step ID 
		/// </summary>
		public const uint SPSID = 0x00400009;
		
		/// <summary>(0040,0010) VR=SH Scheduled Station Name 
		/// </summary>
		public const uint ScheduledStationName = 0x00400010;
		
		/// <summary>(0040,0011) VR=SH Scheduled Procedure Step Location 
		/// </summary>
		public const uint SPSLocation = 0x00400011;
		
		/// <summary>(0040,0012) VR=LO Pre-Medication 
		/// </summary>
		public const uint PreMedication = 0x00400012;
		
		/// <summary>(0040,0020) VR=CS Scheduled Procedure Step Status 
		/// </summary>
		public const uint SPSStatus = 0x00400020;
		
		/// <summary>(0040,0100) VR=SQ Scheduled Procedure Step Sequence 
		/// </summary>
		public const uint SPSSeq = 0x00400100;
		
		/// <summary>(0040,0220) VR=SQ Referenced Standalone SOP Instance Sequence 
		/// </summary>
		public const uint RefStandaloneSOPInstanceSeq = 0x00400220;
		
		/// <summary>(0040,0241) VR=AE Performed Station AE Title 
		/// </summary>
		public const uint PerformedStationAET = 0x00400241;
		
		/// <summary>(0040,0242) VR=SH Performed Station Name 
		/// </summary>
		public const uint PerformedStationName = 0x00400242;
		
		/// <summary>(0040,0243) VR=SH Performed Location 
		/// </summary>
		public const uint PerformedLocation = 0x00400243;
		
		/// <summary>(0040,0244) VR=DA Performed Procedure Step Start Date 
		/// </summary>
		public const uint PPSStartDate = 0x00400244;
		
		/// <summary>(0040,0245) VR=TM Performed Procedure Step Start Time 
		/// </summary>
		public const uint PPSStartTime = 0x00400245;
		
		/// <summary>(0040,0250) VR=DA Performed Procedure Step End Date 
		/// </summary>
		public const uint PPSEndDate = 0x00400250;
		
		/// <summary>(0040,0251) VR=TM Performed Procedure Step End Time 
		/// </summary>
		public const uint PPSEndTime = 0x00400251;
		
		/// <summary>(0040,0252) VR=CS Performed Procedure Step Status 
		/// </summary>
		public const uint PPSStatus = 0x00400252;
		
		/// <summary>(0040,0253) VR=SH Performed Procedure Step ID 
		/// </summary>
		public const uint PPSID = 0x00400253;
		
		/// <summary>(0040,0254) VR=LO Performed Procedure Step Description 
		/// </summary>
		public const uint PPSDescription = 0x00400254;
		
		/// <summary>(0040,0255) VR=LO Performed Procedure Type Description 
		/// </summary>
		public const uint PerformedProcedureTypeDescription = 0x00400255;
		
		/// <summary>(0040,0260) VR=SQ Performed Protocol Code Sequence 
		/// </summary>
		public const uint PerformedProtocolCodeSeq = 0x00400260;
		
		/// <summary>(0040,0270) VR=SQ Scheduled Step Attributes Sequence 
		/// </summary>
		public const uint ScheduledStepAttributesSeq = 0x00400270;
		
		/// <summary>(0040,0275) VR=SQ Request Attributes Sequence 
		/// </summary>
		public const uint RequestAttributesSeq = 0x00400275;
		
		/// <summary>(0040,0280) VR=ST Comments on the Performed Procedure Steps 
		/// </summary>
		public const uint PPSComments = 0x00400280;
		
		/// <summary>(0040,0293) VR=SQ Quantity Sequence 
		/// </summary>
		public const uint QuantitySeq = 0x00400293;
		
		/// <summary>(0040,0294) VR=DS Quantity 
		/// </summary>
		public const uint Quantity = 0x00400294;
		
		/// <summary>(0040,0295) VR=SQ Measuring Units Sequence 
		/// </summary>
		public const uint MeasuringUnitsSeq = 0x00400295;
		
		/// <summary>(0040,0296) VR=SQ Billing Item Sequence 
		/// </summary>
		public const uint BillingItemSeq = 0x00400296;
		
		/// <summary>(0040,0300) VR=US Total Time of Fluoroscopy 
		/// </summary>
		public const uint TotalTimeOfFluoroscopy = 0x00400300;
		
		/// <summary>(0040,0301) VR=US Total Number of Exposures 
		/// </summary>
		public const uint TotalNumberOfExposures = 0x00400301;
		
		/// <summary>(0040,0302) VR=US Entrance Dose 
		/// </summary>
		public const uint EntranceDose = 0x00400302;
		
		/// <summary>(0040,0303) VR=US Exposed Area 
		/// </summary>
		public const uint ExposedArea = 0x00400303;
		
		/// <summary>(0040,0306) VR=DS Distance Source to Entrance 
		/// </summary>
		public const uint DistanceSourceToEntrance = 0x00400306;
		
		/// <summary>(0040,0307) VR=DS Distance Source to Support 
		/// </summary>
		public const uint DistanceSourceToSupport = 0x00400307;
		
		/// <summary>(0040,0310) VR=ST Comments on Radiation Dose 
		/// </summary>
		public const uint CommentsOnRadiationDose = 0x00400310;
		
		/// <summary>(0040,0312) VR=DS X-Ray Output 
		/// </summary>
		public const uint XRayOutput = 0x00400312;
		
		/// <summary>(0040,0314) VR=DS Half Value Layer 
		/// </summary>
		public const uint HalfValueLayer = 0x00400314;
		
		/// <summary>(0040,0316) VR=DS Organ Dose 
		/// </summary>
		public const uint OrganDose = 0x00400316;
		
		/// <summary>(0040,0318) VR=CS Organ Exposed 
		/// </summary>
		public const uint OrganExposed = 0x00400318;
		
		/// <summary>(0040,0320) VR=SQ Billing Procedure Step Sequence 
		/// </summary>
		public const uint BillingProcedureStepSeq = 0x00400320;
		
		/// <summary>(0040,0321) VR=SQ Film Consumption Sequence 
		/// </summary>
		public const uint FilmConsumptionSeq = 0x00400321;
		
		/// <summary>(0040,0324) VR=SQ Billing Supplies and Devices Sequence 
		/// </summary>
		public const uint BillingSuppliesAndDevicesSeq = 0x00400324;
		
		/// <summary>(0040,0330) VR=SQ Referenced Procedure Step Sequence 
		/// </summary>
		public const uint RefProcedureStepSeq = 0x00400330;
		
		/// <summary>(0040,0340) VR=SQ Performed Series Sequence 
		/// </summary>
		public const uint PerformedSeriesSeq = 0x00400340;
		
		/// <summary>(0040,0400) VR=LT Comments on the Scheduled Procedure Step 
		/// </summary>
		public const uint SPSComments = 0x00400400;
		
		/// <summary>(0040,050A) VR=LO Specimen Accession Number 
		/// </summary>
		public const uint SpecimenAccessionNumber = 0x0040050A;
		
		/// <summary>(0040,0550) VR=SQ Specimen Sequence 
		/// </summary>
		public const uint SpecimenSeq = 0x00400550;
		
		/// <summary>(0040,0551) VR=LO Specimen Identifier 
		/// </summary>
		public const uint SpecimenIdentifier = 0x00400551;
		
		/// <summary>(0040,059A) VR=SQ Specimen Type Code Sequence 
		/// </summary>
		public const uint SpecimenTypeCodeSeq = 0x0040059A;
		
		/// <summary>(0040,0555) VR=SQ Acquisition Context Sequence 
		/// </summary>
		public const uint AcquisitionContextSeq = 0x00400555;
		
		/// <summary>(0040,0556) VR=ST Acquisition Context Description 
		/// </summary>
		public const uint AcquisitionContextDescription = 0x00400556;
		
		/// <summary>(0040,06FA) VR=LO Slide Identifier 
		/// </summary>
		public const uint SlideIdentifier = 0x004006FA;
		
		/// <summary>(0040,071A) VR=SQ Image Center Point Coordinates Sequence 
		/// </summary>
		public const uint ImageCenterPointCoordinatesSeq = 0x0040071A;
		
		/// <summary>(0040,072A) VR=DS X offset in Slide Coordinate System 
		/// </summary>
		public const uint XOffsetInSlideCoordinateSystem = 0x0040072A;
		
		/// <summary>(0040,073A) VR=DS Y offset in Slide Coordinate System 
		/// </summary>
		public const uint YOffsetInSlideCoordinateSystem = 0x0040073A;
		
		/// <summary>(0040,074A) VR=DS Z offset in Slide Coordinate System 
		/// </summary>
		public const uint ZOffsetInSlideCoordinateSystem = 0x0040074A;
		
		/// <summary>(0040,08D8) VR=SQ Pixel Spacing Sequence 
		/// </summary>
		public const uint PixelSpacingSeq = 0x004008D8;
		
		/// <summary>(0040,08DA) VR=SQ Coordinate System Axis Code Sequence 
		/// </summary>
		public const uint CoordinateSystemAxisCodeSeq = 0x004008DA;
		
		/// <summary>(0040,08EA) VR=SQ Measurement Units Code Sequence 
		/// </summary>
		public const uint MeasurementUnitsCodeSeq = 0x004008EA;
		
		/// <summary>(0040,1001) VR=SH Requested Procedure ID 
		/// </summary>
		public const uint RequestedProcedureID = 0x00401001;
		
		/// <summary>(0040,1002) VR=LO Reason for the Requested Procedure 
		/// </summary>
		public const uint ReasonForTheRequestedProcedure = 0x00401002;
		
		/// <summary>(0040,1003) VR=SH Requested Procedure Priority 
		/// </summary>
		public const uint RequestedProcedurePriority = 0x00401003;
		
		/// <summary>(0040,1004) VR=LO Patient Transport Arrangements 
		/// </summary>
		public const uint PatientTransportArrangements = 0x00401004;
		
		/// <summary>(0040,1005) VR=LO Requested Procedure Location 
		/// </summary>
		public const uint RequestedProcedureLocation = 0x00401005;
		
		/// <summary>(0040,1006) VR=SH Placer Order Number / Procedure (Retired) 
		/// </summary>
		public const uint PlacerOrderNumberProcedureRetired = 0x00401006;
		
		/// <summary>(0040,1007) VR=SH Filler Order Number / Procedure (Retired) 
		/// </summary>
		public const uint FillerOrderNumberProcedureRetired = 0x00401007;
		
		/// <summary>(0040,1008) VR=LO Confidentiality Code 
		/// </summary>
		public const uint ConfidentialityCode = 0x00401008;
		
		/// <summary>(0040,1009) VR=SH Reporting Priority 
		/// </summary>
		public const uint ReportingPriority = 0x00401009;
		
		/// <summary>(0040,1010) VR=PN Names of Intended Recipients of Results 
		/// </summary>
		public const uint NamesOfIntendedRecipientsOfResults = 0x00401010;

        /// <summary>(0040,1101) VR=SQ Person Identification Sequence
        /// </summary>
        public const uint PersonIdentificationCodeSeq = 0x00401101;

		/// <summary>(0040,1400) VR=LT Requested Procedure Comments 
		/// </summary>
		public const uint RequestedProcedureComments = 0x00401400;
		
		/// <summary>(0040,2001) VR=LO Reason for the Imaging Service Request 
		/// </summary>
		public const uint ReasonForTheImagingServiceRequest = 0x00402001;
		
		/// <summary>(0040,2004) VR=DA Issue Date of Imaging Service Request 
		/// </summary>
		public const uint IssueDateOfImagingServiceRequest = 0x00402004;
		
		/// <summary>(0040,2005) VR=TM Issue Time of Imaging Service Request 
		/// </summary>
		public const uint IssueTimeOfImagingServiceRequest = 0x00402005;
		
		/// <summary>(0040,2006) VR=SH Placer Order Number / Image Service Request (Retired) 
		/// </summary>
		public const uint PlacerOrderNumberImagingServiceRequestRetired = 0x00402006;
		
		/// <summary>(0040,2007) VR=SH Filler Order Number / Image Service Request (Retired) 
		/// </summary>
		public const uint FillerOrderNumberImagingServiceRequestRetired = 0x00402007;
		
		/// <summary>(0040,2008) VR=PN Order Entered By 
		/// </summary>
		public const uint OrderEnteredBy = 0x00402008;
		
		/// <summary>(0040,2009) VR=SH Order Enterer s Location 
		/// </summary>
		public const uint OrderEntererLocation = 0x00402009;
		
		/// <summary>(0040,2010) VR=SH Order Callback Phone Number 
		/// </summary>
		public const uint OrderCallbackPhoneNumber = 0x00402010;
		
		/// <summary>(0040,2016) VR=LO Placer Order Number / Imaging Service Request 
		/// </summary>
		public const uint PlacerOrderNumber = 0x00402016;
		
		/// <summary>(0040,2017) VR=LO Filler Order Number / Imaging Service Request 
		/// </summary>
		public const uint FillerOrderNumber = 0x00402017;
		
		/// <summary>(0040,2400) VR=LT Imaging Service Request Comments 
		/// </summary>
		public const uint ImagingServiceRequestComments = 0x00402400;
		
		/// <summary>(0040,3001) VR=LO Confidentiality Constraint on Patient Data Description 
		/// </summary>
		public const uint ConfidentialityPatientData = 0x00403001;
		
		/// <summary>(0040,8302) VR=DS Entrance Dose in mGy 
		/// </summary>
		public const uint EntranceDoseInmGy = 0x00408302;
		
		/// <summary>(0040,A010) VR=CS Relationship Type 
		/// </summary>
		public const uint RelationshipType = 0x0040A010;
		
		/// <summary>(0040,A027) VR=LO Verifying Organization 
		/// </summary>
		public const uint VerifyingOrganization = 0x0040A027;
		
		/// <summary>(0040,A030) VR=DT Verification DateTime 
		/// </summary>
		public const uint VerificationDateTime = 0x0040A030;
		
		/// <summary>(0040,A032) VR=DT Observation DateTime 
		/// </summary>
		public const uint ObservationDateTime = 0x0040A032;
		
		/// <summary>(0040,A040) VR=CS Value Type 
		/// </summary>
		public const uint ValueType = 0x0040A040;
		
		/// <summary>(0040,A043) VR=SQ Concept-name Code Sequence 
		/// </summary>
		public const uint ConceptNameCodeSeq = 0x0040A043;
		
		/// <summary>(0040,A050) VR=CS Continuity Of Content 
		/// </summary>
		public const uint ContinuityOfContent = 0x0040A050;
		
		/// <summary>(0040,A073) VR=SQ Verifying Observer Sequence 
		/// </summary>
		public const uint VerifyingObserverSeq = 0x0040A073;
		
		/// <summary>(0040,A075) VR=PN Verifying Observer Name 
		/// </summary>
		public const uint VerifyingObserverName = 0x0040A075;
		
		/// <summary>(0040,A088) VR=SQ Verifying Observer Identification Code Sequence 
		/// </summary>
		public const uint VerifyingObserverIdentificationCodeSeq = 0x0040A088;
		
		/// <summary>(0040,A0B0) VR=US Referenced Waveform Channels 
		/// </summary>
		public const uint RefWaveformChannels = 0x0040A0B0;
		
		/// <summary>(0040,A120) VR=DT DateTime 
		/// </summary>
		public const uint DateTime = 0x0040A120;
		
		/// <summary>(0040,A121) VR=DA Date 
		/// </summary>
		public const uint Date = 0x0040A121;
		
		/// <summary>(0040,A122) VR=TM Time 
		/// </summary>
		public const uint Time = 0x0040A122;
		
		/// <summary>(0040,A123) VR=PN Person Name 
		/// </summary>
		public const uint PersonName = 0x0040A123;
		
		/// <summary>(0040,A124) VR=UI UID 
		/// </summary>
		public const uint UID = 0x0040A124;
		
		/// <summary>(0040,A130) VR=CS Temporal Range Type 
		/// </summary>
		public const uint TemporalRangeType = 0x0040A130;
		
		/// <summary>(0040,A132) VR=UL Referenced Sample Positions 
		/// </summary>
		public const uint RefSamplePositions = 0x0040A132;
		
		/// <summary>(0040,A136) VR=US Referenced Frame Numbers 
		/// </summary>
		public const uint RefFrameNumbers = 0x0040A136;
		
		/// <summary>(0040,A138) VR=DS Referenced Time Offsets 
		/// </summary>
		public const uint RefTimeOffsets = 0x0040A138;
		
		/// <summary>(0040,A13A) VR=DT Referenced Datetime 
		/// </summary>
		public const uint RefDatetime = 0x0040A13A;
		
		/// <summary>(0040,A160) VR=UT Text Value 
		/// </summary>
		public const uint TextValue = 0x0040A160;
		
		/// <summary>(0040,A168) VR=SQ Concept Code Sequence 
		/// </summary>
		public const uint ConceptCodeSeq = 0x0040A168;
		
		/// <summary>(0040,A170) VR=SQ Purpose of Reference Code Sequence 
		/// </summary>
		public const uint PurposeofReferenceCodeSeq = 0x0040A170;

        /// <summary>(0040,A180) VR=US Annotation Group Number 
		/// </summary>
		public const uint AnnotationGroupNumber = 0x0040A180;
		
		/// <summary>(0040,A195) VR=SQ Modifier Code Sequence 
		/// </summary>
		public const uint ModifierCodeSeq = 0x0040A195;
		
		/// <summary>(0040,A300) VR=SQ Measured Value Sequence 
		/// </summary>
		public const uint MeasuredValueSeq = 0x0040A300;
		
		/// <summary>(0040,A30A) VR=DS Numeric Value 
		/// </summary>
		public const uint NumericValue = 0x0040A30A;
		
		/// <summary>(0040,A360) VR=SQ Predecessor Documents Sequence 
		/// </summary>
		public const uint PredecessorDocumentsSeq = 0x0040A360;
		
		/// <summary>(0040,A370) VR=SQ Referenced Request Sequence 
		/// </summary>
		public const uint RefRequestSeq = 0x0040A370;
		
		/// <summary>(0040,A372) VR=SQ Performed Procedure Code Sequence 
		/// </summary>
		public const uint PerformedProcedureCodeSeq = 0x0040A372;
		
		/// <summary>(0040,A375) VR=SQ Current Requested Procedure Evidence Sequence 
		/// </summary>
		public const uint CurrentRequestedProcedureEvidenceSeq = 0x0040A375;
		
		/// <summary>(0040,A385) VR=SQ Pertinent Other Evidence Sequence 
		/// </summary>
		public const uint PertinentOtherEvidenceSeq = 0x0040A385;
		
		/// <summary>(0040,A491) VR=CS Completion Flag 
		/// </summary>
		public const uint CompletionFlag = 0x0040A491;
		
		/// <summary>(0040,A492) VR=LO Completion Flag Description 
		/// </summary>
		public const uint CompletionFlagDescription = 0x0040A492;
		
		/// <summary>(0040,A493) VR=CS Verification Flag 
		/// </summary>
		public const uint VerificationFlag = 0x0040A493;
		
		/// <summary>(0040,A504) VR=SQ Content Template Sequence 
		/// </summary>
		public const uint ContentTemplateSeq = 0x0040A504;
		
		/// <summary>(0040,A525) VR=SQ Identical Documents Sequence 
		/// </summary>
		public const uint IdenticalDocumentsSeq = 0x0040A525;
		
		/// <summary>(0040,A730) VR=SQ Content Sequence 
		/// </summary>
		public const uint ContentSeq = 0x0040A730;
		
		/// <summary>(0040,B020) VR=SQ Annotation Sequence 
		/// </summary>
		public const uint AnnotationSeq = 0x0040B020;
		
		/// <summary>(0040,DB00) VR=CS Template Identifier 
		/// </summary>
		public const uint TemplateIdentifier = 0x0040DB00;
		
		/// <summary>(0040,DB06) VR=DT Template Version 
		/// </summary>
		public const uint TemplateVersion = 0x0040DB06;
		
		/// <summary>(0040,DB07) VR=DT Template Local Version 
		/// </summary>
		public const uint TemplateLocalVersion = 0x0040DB07;
		
		/// <summary>(0040,DB0B) VR=CS Template Extension Flag 
		/// </summary>
		public const uint TemplateExtensionFlag = 0x0040DB0B;
		
		/// <summary>(0040,DB0C) VR=UI Template Extension Organization UID 
		/// </summary>
		public const uint TemplateExtensionOrganizationUID = 0x0040DB0C;
		
		/// <summary>(0040,DB0D) VR=UI Template Extension Creator UID 
		/// </summary>
		public const uint TemplateExtensionCreatorUID = 0x0040DB0D;
		
		/// <summary>(0040,DB73) VR=UL Referenced Content Item Identifier 
		/// </summary>
		public const uint RefContentItemIdentifier = 0x0040DB73;
		
		/// <summary>(0040,9096) VR=SQ Real World Value Mapping Sequence 
		/// </summary>
		public const uint RealWorldValueMappingSeq = 0x00409096;
		
		/// <summary>(0040,9210) VR=SS LUT Label 
		/// </summary>
		public const uint LUTLabel = 0x00409210;
		
		/// <summary>(0040,9211) VR=US/SS Real World Value LUT Last Value Mapped 
		/// </summary>
		public const uint RealWorldValueLUTLastValueMappedUS = 0x00409211;
		
		/// <summary>(0040,9212) VR=FD Real World Value LUT Data 
		/// </summary>
		public const uint RealWorldValueLUTData = 0x00409212;
		
		/// <summary>(0040,9216) VR=US/SS Real World Value LUT First Value Mapped 
		/// </summary>
		public const uint RealWorldValueLUTFirstValueMappedUS = 0x00409216;
		
		/// <summary>(0040,9224) VR=FD Real World Value Intercept 
		/// </summary>
		public const uint RealWorldValueIntercept = 0x00409224;
		
		/// <summary>(0040,9225) VR=FD Real World Value Slope 
		/// </summary>
		public const uint RealWorldValueSlope = 0x00409225;
		
		/// <summary>(0050,0004) VR=CS Calibration Image 
		/// </summary>
		public const uint CalibrationImage = 0x00500004;
		
		/// <summary>(0050,0010) VR=SQ Device Sequence 
		/// </summary>
		public const uint DeviceSeq = 0x00500010;
		
		/// <summary>(0050,0014) VR=DS Device Length 
		/// </summary>
		public const uint DeviceLength = 0x00500014;
		
		/// <summary>(0050,0016) VR=DS Device Diameter 
		/// </summary>
		public const uint DeviceDiameter = 0x00500016;
		
		/// <summary>(0050,0017) VR=CS Device Diameter Units 
		/// </summary>
		public const uint DeviceDiameterUnits = 0x00500017;
		
		/// <summary>(0050,0018) VR=DS Device Volume 
		/// </summary>
		public const uint DeviceVolume = 0x00500018;
		
		/// <summary>(0050,0019) VR=DS Inter-marker Distance 
		/// </summary>
		public const uint InterMarkerDistance = 0x00500019;
		
		/// <summary>(0050,0020) VR=LO Device Description 
		/// </summary>
		public const uint DeviceDescription = 0x00500020;
		
		/// <summary>(0054,0010) VR=US Energy Window Vector 
		/// </summary>
		public const uint EnergyWindowVector = 0x00540010;
		
		/// <summary>(0054,0011) VR=US Number of Energy Windows 
		/// </summary>
		public const uint NumberOfEnergyWindows = 0x00540011;
		
		/// <summary>(0054,0012) VR=SQ Energy Window Information Sequence 
		/// </summary>
		public const uint EnergyWindowInformationSeq = 0x00540012;
		
		/// <summary>(0054,0013) VR=SQ Energy Window Range Sequence 
		/// </summary>
		public const uint EnergyWindowRangeSeq = 0x00540013;
		
		/// <summary>(0054,0014) VR=DS Energy Window Lower Limit 
		/// </summary>
		public const uint EnergyWindowLowerLimit = 0x00540014;
		
		/// <summary>(0054,0015) VR=DS Energy Window Upper Limit 
		/// </summary>
		public const uint EnergyWindowUpperLimit = 0x00540015;
		
		/// <summary>(0054,0016) VR=SQ Radiopharmaceutical Information Sequence 
		/// </summary>
		public const uint RadiopharmaceuticalInformationSeq = 0x00540016;
		
		/// <summary>(0054,0017) VR=IS Residual Syringe Counts 
		/// </summary>
		public const uint ResidualSyringeCounts = 0x00540017;
		
		/// <summary>(0054,0018) VR=SH Energy Window Name 
		/// </summary>
		public const uint EnergyWindowName = 0x00540018;
		
		/// <summary>(0054,0020) VR=US Detector Vector 
		/// </summary>
		public const uint DetectorVector = 0x00540020;
		
		/// <summary>(0054,0021) VR=US Number of Detectors 
		/// </summary>
		public const uint NumberOfDetectors = 0x00540021;
		
		/// <summary>(0054,0022) VR=SQ Detector Information Sequence 
		/// </summary>
		public const uint DetectorInformationSeq = 0x00540022;
		
		/// <summary>(0054,0030) VR=US Phase Vector 
		/// </summary>
		public const uint PhaseVector = 0x00540030;
		
		/// <summary>(0054,0031) VR=US Number of Phases 
		/// </summary>
		public const uint NumberOfPhases = 0x00540031;
		
		/// <summary>(0054,0032) VR=SQ Phase Information Sequence 
		/// </summary>
		public const uint PhaseInformationSeq = 0x00540032;
		
		/// <summary>(0054,0033) VR=US Number of Frames in Phase 
		/// </summary>
		public const uint NumberOfFramesInPhase = 0x00540033;
		
		/// <summary>(0054,0036) VR=IS Phase Delay 
		/// </summary>
		public const uint PhaseDelay = 0x00540036;
		
		/// <summary>(0054,0038) VR=IS Pause Between Frames 
		/// </summary>
		public const uint PauseBetweenFrames = 0x00540038;
		
		/// <summary>(0054,0050) VR=US Rotation Vector 
		/// </summary>
		public const uint RotationVector = 0x00540050;
		
		/// <summary>(0054,0051) VR=US Number of Rotations 
		/// </summary>
		public const uint NumberOfRotations = 0x00540051;
		
		/// <summary>(0054,0052) VR=SQ Rotation Information Sequence 
		/// </summary>
		public const uint RotationInformationSeq = 0x00540052;
		
		/// <summary>(0054,0053) VR=US Number of Frames in Rotation 
		/// </summary>
		public const uint NumberOfFramesInRotation = 0x00540053;
		
		/// <summary>(0054,0060) VR=US R-R Interval Vector 
		/// </summary>
		public const uint RRIntervalVector = 0x00540060;
		
		/// <summary>(0054,0061) VR=US Number of R-R Intervals 
		/// </summary>
		public const uint NumberOfRRIntervals = 0x00540061;
		
		/// <summary>(0054,0062) VR=SQ Gated Information Sequence 
		/// </summary>
		public const uint GatedInformationSeq = 0x00540062;
		
		/// <summary>(0054,0063) VR=SQ Data Information Sequence 
		/// </summary>
		public const uint DataInformationSeq = 0x00540063;
		
		/// <summary>(0054,0070) VR=US Time Slot Vector 
		/// </summary>
		public const uint TimeSlotVector = 0x00540070;
		
		/// <summary>(0054,0071) VR=US Number of Time Slots 
		/// </summary>
		public const uint NumberOfTimeSlots = 0x00540071;
		
		/// <summary>(0054,0072) VR=SQ Time Slot Information Sequence 
		/// </summary>
		public const uint TimeSlotInformationSeq = 0x00540072;
		
		/// <summary>(0054,0073) VR=DS Time Slot Time 
		/// </summary>
		public const uint TimeSlotTime = 0x00540073;
		
		/// <summary>(0054,0080) VR=US Slice Vector 
		/// </summary>
		public const uint SliceVector = 0x00540080;
		
		/// <summary>(0054,0081) VR=US Number of Slices 
		/// </summary>
		public const uint NumberOfSlices = 0x00540081;
		
		/// <summary>(0054,0090) VR=US Angular View Vector 
		/// </summary>
		public const uint AngularViewVector = 0x00540090;
		
		/// <summary>(0054,0100) VR=US Time Slice Vector 
		/// </summary>
		public const uint TimeSliceVector = 0x00540100;
		
		/// <summary>(0054,0101) VR=US Number of Time Slices 
		/// </summary>
		public const uint NumberOfTimeSlices = 0x00540101;
		
		/// <summary>(0054,0200) VR=DS Start Angle 
		/// </summary>
		public const uint StartAngle = 0x00540200;
		
		/// <summary>(0054,0202) VR=CS Type of Detector Motion 
		/// </summary>
		public const uint TypeOfDetectorMotion = 0x00540202;
		
		/// <summary>(0054,0210) VR=IS Trigger Vector 
		/// </summary>
		public const uint TriggerVector = 0x00540210;
		
		/// <summary>(0054,0211) VR=US Number of Triggers in Phase 
		/// </summary>
		public const uint NumberOfTriggersInPhase = 0x00540211;
		
		/// <summary>(0054,0220) VR=SQ View Code Sequence 
		/// </summary>
		public const uint ViewCodeSeq = 0x00540220;
		
		/// <summary>(0054,0222) VR=SQ View Modifier Code Sequence 
		/// </summary>
		public const uint ViewModifierCodeSeq = 0x00540222;
		
		/// <summary>(0054,0300) VR=SQ Radionuclide Code Sequence 
		/// </summary>
		public const uint RadionuclideCodeSeq = 0x00540300;
		
		/// <summary>(0054,0302) VR=SQ Administration Route Code Sequence 
		/// </summary>
		public const uint AdministrationRouteCodeSeq = 0x00540302;
		
		/// <summary>(0054,0304) VR=SQ Radiopharmaceutical Code Sequence 
		/// </summary>
		public const uint RadiopharmaceuticalCodeSeq = 0x00540304;
		
		/// <summary>(0054,0306) VR=SQ Calibration Data Sequence 
		/// </summary>
		public const uint CalibrationDataSeq = 0x00540306;
		
		/// <summary>(0054,0308) VR=US Energy Window Number 
		/// </summary>
		public const uint EnergyWindowNumber = 0x00540308;
		
		/// <summary>(0054,0400) VR=SH Image ID 
		/// </summary>
		public const uint ImageID = 0x00540400;
		
		/// <summary>(0054,0410) VR=SQ Patient Orientation Code Sequence 
		/// </summary>
		public const uint PatientOrientationCodeSeq = 0x00540410;
		
		/// <summary>(0054,0412) VR=SQ Patient Orientation Modifier Code Sequence 
		/// </summary>
		public const uint PatientOrientationModifierCodeSeq = 0x00540412;
		
		/// <summary>(0054,0414) VR=SQ Patient Gantry Relationship Code Sequence 
		/// </summary>
		public const uint PatientGantryRelationshipCodeSeq = 0x00540414;
		
		/// <summary>(0054,1000) VR=CS Series Type 
		/// </summary>
		public const uint SeriesType = 0x00541000;
		
		/// <summary>(0054,1001) VR=CS Units 
		/// </summary>
		public const uint Units = 0x00541001;
		
		/// <summary>(0054,1002) VR=CS Counts Source 
		/// </summary>
		public const uint CountsSource = 0x00541002;
		
		/// <summary>(0054,1004) VR=CS Reprojection Method 
		/// </summary>
		public const uint ReprojectionMethod = 0x00541004;
		
		/// <summary>(0054,1100) VR=CS Randoms Correction Method 
		/// </summary>
		public const uint RandomsCorrectionMethod = 0x00541100;
		
		/// <summary>(0054,1101) VR=LO Attenuation Correction Method 
		/// </summary>
		public const uint AttenuationCorrectionMethod = 0x00541101;
		
		/// <summary>(0054,1102) VR=CS Decay Correction 
		/// </summary>
		public const uint DecayCorrection = 0x00541102;
		
		/// <summary>(0054,1103) VR=LO Reconstruction Method 
		/// </summary>
		public const uint ReconstructionMethod = 0x00541103;
		
		/// <summary>(0054,1104) VR=LO Detector Lines of Response Used 
		/// </summary>
		public const uint DetectorLinesOfResponseUsed = 0x00541104;
		
		/// <summary>(0054,1105) VR=LO Scatter Correction Method 
		/// </summary>
		public const uint ScatterCorrectionMethod = 0x00541105;
		
		/// <summary>(0054,1200) VR=DS Axial Acceptance 
		/// </summary>
		public const uint AxialAcceptance = 0x00541200;
		
		/// <summary>(0054,1201) VR=IS Axial Mash 
		/// </summary>
		public const uint AxialMash = 0x00541201;
		
		/// <summary>(0054,1202) VR=IS Transverse Mash 
		/// </summary>
		public const uint TransverseMash = 0x00541202;
		
		/// <summary>(0054,1203) VR=DS Detector Element Size 
		/// </summary>
		public const uint DetectorElementSize = 0x00541203;
		
		/// <summary>(0054,1210) VR=DS Coincidence Window Width 
		/// </summary>
		public const uint CoincidenceWindowWidth = 0x00541210;
		
		/// <summary>(0054,1220) VR=CS Secondary Counts Type 
		/// </summary>
		public const uint SecondaryCountsType = 0x00541220;
		
		/// <summary>(0054,1300) VR=DS Frame Reference Time 
		/// </summary>
		public const uint FrameReferenceTime = 0x00541300;
		
		/// <summary>(0054,1310) VR=IS Primary (Prompts) Counts Accumulated 
		/// </summary>
		public const uint PrimaryCountsAccumulated = 0x00541310;
		
		/// <summary>(0054,1311) VR=IS Secondary Counts Accumulated 
		/// </summary>
		public const uint SecondaryCountsAccumulated = 0x00541311;
		
		/// <summary>(0054,1320) VR=DS Slice Sensitivity Factor 
		/// </summary>
		public const uint SliceSensitivityFactor = 0x00541320;
		
		/// <summary>(0054,1321) VR=DS Decay Factor 
		/// </summary>
		public const uint DecayFactor = 0x00541321;
		
		/// <summary>(0054,1322) VR=DS Dose Calibration Factor 
		/// </summary>
		public const uint DoseCalibrationFactor = 0x00541322;
		
		/// <summary>(0054,1323) VR=DS Scatter Fraction Factor 
		/// </summary>
		public const uint ScatterFractionFactor = 0x00541323;
		
		/// <summary>(0054,1324) VR=DS Dead Time Factor 
		/// </summary>
		public const uint DeadTimeFactor = 0x00541324;
		
		/// <summary>(0054,1330) VR=US Image Index 
		/// </summary>
		public const uint ImageIndex = 0x00541330;
		
		/// <summary>(0054,1400) VR=CS Counts Included 
		/// </summary>
		public const uint CountsIncluded = 0x00541400;
		
		/// <summary>(0054,1401) VR=CS Dead Time Correction Flag 
		/// </summary>
		public const uint DeadTimeCorrectionFlag = 0x00541401;
		
		/// <summary>(0060,3000) VR=SQ Histogram Sequence 
		/// </summary>
		public const uint HistogramSeq = 0x00603000;
		
		/// <summary>(0060,3002) VR=US Histogram Number of Bins 
		/// </summary>
		public const uint HistogramNumberOfBins = 0x00603002;
		
		/// <summary>(0060,3004) VR=US,SS Histogram First Bin Value 
		/// </summary>
		public const uint HistogramFirstBinValue = 0x00603004;
		
		/// <summary>(0060,3006) VR=US,SS Histogram Last Bin Value 
		/// </summary>
		public const uint HistogramLastBinValue = 0x00603006;
		
		/// <summary>(0060,3008) VR=US Histogram Bin Width 
		/// </summary>
		public const uint HistogramBinWidth = 0x00603008;
		
		/// <summary>(0060,3010) VR=LO Histogram Explanation 
		/// </summary>
		public const uint HistogramExplanation = 0x00603010;
		
		/// <summary>(0060,3020) VR=UL Histogram Data 
		/// </summary>
		public const uint HistogramData = 0x00603020;
		
		/// <summary>(0070,0001) VR=SQ Graphic Annotation Sequence 
		/// </summary>
		public const uint GraphicAnnotationSeq = 0x00700001;
		
		/// <summary>(0070,0002) VR=CS Graphic Layer 
		/// </summary>
		public const uint GraphicLayer = 0x00700002;
		
		/// <summary>(0070,0003) VR=CS Bounding Box Annotation Units 
		/// </summary>
		public const uint BoundingBoxAnnotationUnits = 0x00700003;
		
		/// <summary>(0070,0004) VR=CS Anchor Point Annotation Units 
		/// </summary>
		public const uint AnchorPointAnnotationUnits = 0x00700004;
		
		/// <summary>(0070,0005) VR=CS Graphic Annotation Units 
		/// </summary>
		public const uint GraphicAnnotationUnits = 0x00700005;
		
		/// <summary>(0070,0006) VR=ST Unformatted Text Value 
		/// </summary>
		public const uint UnformattedTextValue = 0x00700006;
		
		/// <summary>(0070,0008) VR=SQ Text Object Sequence 
		/// </summary>
		public const uint TextObjectSeq = 0x00700008;
		
		/// <summary>(0070,0009) VR=SQ Graphic Object Sequence 
		/// </summary>
		public const uint GraphicObjectSeq = 0x00700009;
		
		/// <summary>(0070,0010) VR=FL Bounding Box Top Left Hand Corner 
		/// </summary>
		public const uint BoundingBoxTopLeftHandCorner = 0x00700010;
		
		/// <summary>(0070,0011) VR=FL Bounding Box Bottom Right Hand Corner 
		/// </summary>
		public const uint BoundingBoxBottomRightHandCorner = 0x00700011;
		
		/// <summary>(0070,0012) VR=CS Bounding Box Text Horizontal Justification 
		/// </summary>
		public const uint BoundingBoxTextHorizontalJustification = 0x00700012;
		
		/// <summary>(0070,0014) VR=FL Anchor Point 
		/// </summary>
		public const uint AnchorPoint = 0x00700014;
		
		/// <summary>(0070,0015) VR=CS Anchor Point Visibility 
		/// </summary>
		public const uint AnchorPointVisibility = 0x00700015;
		
		/// <summary>(0070,0020) VR=US Graphic Dimensions 
		/// </summary>
		public const uint GraphicDimensions = 0x00700020;
		
		/// <summary>(0070,0021) VR=US Number of Graphic Points 
		/// </summary>
		public const uint NumberOfGraphicPoints = 0x00700021;
		
		/// <summary>(0070,0022) VR=FL Graphic Data 
		/// </summary>
		public const uint GraphicData = 0x00700022;
		
		/// <summary>(0070,0023) VR=CS Graphic Type 
		/// </summary>
		public const uint GraphicType = 0x00700023;
		
		/// <summary>(0070,0024) VR=CS Graphic Filled 
		/// </summary>
		public const uint GraphicFilled = 0x00700024;
		
		/// <summary>(0070,0041) VR=CS Image Horizontal Flip 
		/// </summary>
		public const uint ImageHorizontalFlip = 0x00700041;
		
		/// <summary>(0070,0042) VR=US Image Rotation 
		/// </summary>
		public const uint ImageRotation = 0x00700042;
		
		/// <summary>(0070,0052) VR=SL Displayed Area Top Left Hand Corner 
		/// </summary>
		public const uint DisplayedAreaTopLeftHandCorner = 0x00700052;
		
		/// <summary>(0070,0053) VR=SL Displayed Area Bottom Right Hand Corner 
		/// </summary>
		public const uint DisplayedAreaBottomRightHandCorner = 0x00700053;
		
		/// <summary>(0070,005A) VR=SQ Displayed Area Selection Sequence 
		/// </summary>
		public const uint DisplayedAreaSelectionSeq = 0x0070005A;
		
		/// <summary>(0070,0060) VR=SQ Graphic Layer Sequence 
		/// </summary>
		public const uint GraphicLayerSeq = 0x00700060;
		
		/// <summary>(0070,0062) VR=IS Graphic Layer Order 
		/// </summary>
		public const uint GraphicLayerOrder = 0x00700062;
		
		/// <summary>(0070,0066) VR=US Graphic Layer Recommended Display Grayscale Value 
		/// </summary>
		public const uint GraphicLayerRecommendedDisplayGrayscaleValue = 0x00700066;
		
		/// <summary>(0070,0067) VR=US Graphic Layer Recommended Display RGB Value 
		/// </summary>
		public const uint GraphicLayerRecommendedDisplayRGBValue = 0x00700067;
		
		/// <summary>(0070,0068) VR=LO Graphic Layer Description 
		/// </summary>
		public const uint GraphicLayerDescription = 0x00700068;
		
		/// <summary>(0070,0080) VR=CS Presentation Label 
		/// </summary>
		public const uint PresentationLabel = 0x00700080;
		
		/// <summary>(0070,0081) VR=LO Presentation Description 
		/// </summary>
		public const uint PresentationDescription = 0x00700081;
		
		/// <summary>(0070,0082) VR=DA Presentation Creation Date 
		/// </summary>
		public const uint PresentationCreationDate = 0x00700082;
		
		/// <summary>(0070,0083) VR=TM Presentation Creation Time 
		/// </summary>
		public const uint PresentationCreationTime = 0x00700083;
		
		/// <summary>(0070,0084) VR=PN Presentation Creator's Name 
		/// </summary>
		public const uint PresentationCreatorName = 0x00700084;
		
		/// <summary>(0070,0100) VR=CS Presentation Size Mode 
		/// </summary>
		public const uint PresentationSizeMode = 0x00700100;
		
		/// <summary>(0070,0101) VR=DS Presentation Pixel Spacing 
		/// </summary>
		public const uint PresentationPixelSpacing = 0x00700101;
		
		/// <summary>(0070,0102) VR=IS Presentation Pixel Aspect Ratio 
		/// </summary>
		public const uint PresentationPixelAspectRatio = 0x00700102;
		
		/// <summary>(0070,0103) VR=FL Presentation Pixel Magnification Ratio 
		/// </summary>
		public const uint PresentationPixelMagnificationRatio = 0x00700103;
		
		/// <summary>(0088,0130) VR=SH Storage Media File-set ID 
		/// </summary>
		public const uint StorageMediaFileSetID = 0x00880130;
		
		/// <summary>(0088,0140) VR=UI Storage Media File-set UID 
		/// </summary>
		public const uint StorageMediaFileSetUID = 0x00880140;
		
		/// <summary>(0088,0200) VR=SQ Icon Image Sequence 
		/// </summary>
		public const uint IconImageSeq = 0x00880200;
		
		/// <summary>(0088,0904) VR=LO Topic Title 
		/// </summary>
		public const uint TopicTitle = 0x00880904;
		
		/// <summary>(0088,0906) VR=ST Topic Subject 
		/// </summary>
		public const uint TopicSubject = 0x00880906;
		
		/// <summary>(0088,0910) VR=LO Topic Author 
		/// </summary>
		public const uint TopicAuthor = 0x00880910;
		
		/// <summary>(0088,0912) VR=LO Topic Key Words 
		/// </summary>
		public const uint TopicKeyWords = 0x00880912;
		
		/// <summary>(0100,0410) VR=CS SOP Instance Status 
		/// </summary>
		public const uint SOPInstanceStatus = 0x01000410;
		
		/// <summary>(0100,0420) VR=DT SOP Authorization Date and Time 
		/// </summary>
		public const uint SOPAuthorizationDateAndTime = 0x01000420;
		
		/// <summary>(0100,0424) VR=LT SOP Authorization Comment 
		/// </summary>
		public const uint SOPAuthorizationComment = 0x01000424;
		
		/// <summary>(0100,0426) VR=LO Authorization Equipment Certification Number 
		/// </summary>
		public const uint AuthorizationEquipmentCertificationNumber = 0x01000426;
		


		/// <summary>(0400,0005)	MAC ID Number	US
        /// </summary>
        public const uint MACIDNumber = 0x04000005;

        /// <summary>(0400,0010)	MAC Calculation Transfer Syntax UID	UI
        /// </summary>
        public const uint MACCalculationTransferSyntaxUID = 0x04000010;
        
        /// <summary>(0400,0015)	MAC Algorithm	CS
        /// </summary>
        public const uint MACAlgorithm = 0x04000015;

        /// <summary>(0400,0020)	Data Elements Signed	AT
        /// </summary>
        public const uint DataElementsSigned = 0x04000020;
        
        /// <summary>(0400,0100)	Digital Signature UID	UI
        /// </summary>
        public const uint DigitalSignatureUID = 0x04000100;
        
        /// <summary>(0400,0105)	Digital Signature DateTime	DT
        /// </summary>
        public const uint DigitalSignatureDateTime = 0x04000105;
        
        /// <summary>(0400,0110)	Certificate Type	CS
        /// </summary>
        public const uint CertificateType = 0x04000110;

        /// <summary>(0400,0115)	Certificate of Signer	OB
        /// </summary>
        public const uint CertificateOfSigner = 0x04000115;

        /// <summary>(0400,0120)	Signature	OB
        /// </summary>
        public const uint Signature = 0x04000120;
        
        /// <summary>(0400,0305)	Certified Timestamp Type	CS
        /// </summary>
        public const uint CertifiedTimestampType = 0x04000305;
        
        /// <summary>(0400,0310)	Certified Timestamp	OB
        /// </summary>
        public const uint CertifiedTimestamp = 0x04000310;

        /// <summary>(0400,0401)	Digital Signature Purpose Code Sequence	SQ
        /// </summary>
        public const uint DigitalSignaturePurposeCodeSeq = 0x04000401;

        /// <summary>(0400,0402)	Referenced Digital Signature Sequence	SQ
        /// </summary>
        public const uint ReferencedDigitalSignatureSeq = 0x04000402;

        /// <summary>(0400,0403)	Referenced SOP Instance MAC Sequence	SQ
        /// </summary>
        public const uint ReferencedSOPInstanceMACSeq = 0x04000403;

        /// <summary>(0400,0404)	MAC	OB
        /// </summary>
        public const uint MAC = 0x04000404;
        
        /// <summary>(0400,0500)    Encrypted Attributes Sequence	SQ
        /// </summary>
        public const uint EncryptedAttributesSeq = 0x04000500;
        
        /// <summary>(0400,0510)    Encrypted Content Transfer Syntax UID	UI
        /// </summary>
        public const uint EncryptedContentTransferSyntaxUID = 0x04000510;

        /// <summary>(0400,0520)    Encrypted Content	OB
        /// </summary>
        public const uint EncryptedContent = 0x04000520;

        /// <summary>(0400,0550)    Modified Attributes Sequence	SQ
        /// </summary>
        public const uint ModifiedAttributesSeq= 0x04000550;

        /// <summary>(0400,0561)	Original Attributes Sequence	SQ
        /// </summary>
        public const uint OriginalAttributesSeq = 0x04000561;

        /// <summary>(0400,0562)	Attribute Modification Datetime	DT
        /// </summary>
        public const uint AttributeModificationDatetime = 0x04000562;

        /// <summary>(0400,0563)	Modifying System	LO
        /// </summary>
        public const uint ModifyingSystem = 0x04000563;

        /// <summary>(0400,0564)	Source of Previous Values	LO
        /// </summary>
        public const uint SourceofPreviousValues = 0x04000564;

        /// <summary>(0400,0565)	Reason for the Attribute Modification	CS
        /// </summary>
        public const uint ReasonfortheAttributeModification = 0x04000565;
        

        /// <summary>(2000,0010) VR=IS Number of Copies 
		/// </summary>
		public const uint NumberOfCopies = 0x20000010;
		
		/// <summary>(2000,001E) VR=SQ Printer Configuration Sequence 
		/// </summary>
		public const uint PrinterConfigurationSeq = 0x2000001E;
		
		/// <summary>(2000,0020) VR=CS Print Priority 
		/// </summary>
		public const uint PrintPriority = 0x20000020;
		
		/// <summary>(2000,0030) VR=CS Medium Type 
		/// </summary>
		public const uint MediumType = 0x20000030;
		
		/// <summary>(2000,0040) VR=CS Film Destination 
		/// </summary>
		public const uint FilmDestination = 0x20000040;
		
		/// <summary>(2000,0050) VR=LO Film Session Label 
		/// </summary>
		public const uint FilmSessionLabel = 0x20000050;
		
		/// <summary>(2000,0060) VR=IS Memory Allocation 
		/// </summary>
		public const uint MemoryAllocation = 0x20000060;
		
		/// <summary>(2000,0061) VR=IS Maximum Memory Allocation 
		/// </summary>
		public const uint MaximumMemoryAllocation = 0x20000061;
		
		/// <summary>(2000,0062) VR=CS Color Image Printing Flag 
		/// </summary>
		public const uint ColorImagePrintingFlag = 0x20000062;
		
		/// <summary>(2000,0063) VR=CS Collation Flag 
		/// </summary>
		public const uint CollationFlag = 0x20000063;
		
		/// <summary>(2000,0065) VR=CS Annotation Flag 
		/// </summary>
		public const uint AnnotationFlag = 0x20000065;
		
		/// <summary>(2000,0067) VR=CS Image Overlay Flag 
		/// </summary>
		public const uint ImageOverlayFlag = 0x20000067;
		
		/// <summary>(2000,0069) VR=CS Presentation LUT Flag 
		/// </summary>
		public const uint PresentationLUTFlag = 0x20000069;
		
		/// <summary>(2000,006A) VR=CS Image Box Presentation LUT Flag 
		/// </summary>
		public const uint ImageBoxPresentationLUTFlag = 0x2000006A;
		
		/// <summary>(2000,00A0) VR=US Memory Bit Depth 
		/// </summary>
		public const uint MemoryBitDepth = 0x200000A0;
		
		/// <summary>(2000,00A1) VR=US Printing Bit Depth 
		/// </summary>
		public const uint PrintingBitDepth = 0x200000A1;
		
		/// <summary>(2000,00A2) VR=SQ Media Installed Sequence 
		/// </summary>
		public const uint MediaInstalledSeq = 0x200000A2;
		
		/// <summary>(2000,00A4) VR=SQ Other Media Available Sequence 
		/// </summary>
		public const uint OtherMediaAvailableSeq = 0x200000A4;
		
		/// <summary>(2000,00A8) VR=SQ Supported Image Display Formats Sequence 
		/// </summary>
		public const uint SupportedImageDisplayFormatsSeq = 0x200000A8;
		
		/// <summary>(2000,0500) VR=SQ Referenced Film Box Sequence 
		/// </summary>
		public const uint RefFilmBoxSeq = 0x20000500;
		
		/// <summary>(2000,0510) VR=SQ Referenced Stored Print Sequence 
		/// </summary>
		public const uint RefStoredPrintSeq = 0x20000510;
		
		/// <summary>(2010,0010) VR=ST Image Display Format 
		/// </summary>
		public const uint ImageDisplayFormat = 0x20100010;
		
		/// <summary>(2010,0030) VR=CS Annotation Display Format ID 
		/// </summary>
		public const uint AnnotationDisplayFormatID = 0x20100030;
		
		/// <summary>(2010,0040) VR=CS Film Orientation 
		/// </summary>
		public const uint FilmOrientation = 0x20100040;
		
		/// <summary>(2010,0050) VR=CS Film Size ID 
		/// </summary>
		public const uint FilmSizeID = 0x20100050;
		
		/// <summary>(2010,0052) VR=CS Printer Resolution ID 
		/// </summary>
		public const uint PrinterResolutionID = 0x20100052;
		
		/// <summary>(2010,0054) VR=CS Default Printer Resolution ID 
		/// </summary>
		public const uint DefaultPrinterResolutionID = 0x20100054;
		
		/// <summary>(2010,0060) VR=CS Magnification Type 
		/// </summary>
		public const uint MagnificationType = 0x20100060;
		
		/// <summary>(2010,0080) VR=CS Smoothing Type 
		/// </summary>
		public const uint SmoothingType = 0x20100080;
		
		/// <summary>(2010,00A6) VR=CS Default Magnification Type 
		/// </summary>
		public const uint DefaultMagnificationType = 0x201000A6;
		
		/// <summary>(2010,00A7) VR=CS Other Magnification Types Available 
		/// </summary>
		public const uint OtherMagnificationTypesAvailable = 0x201000A7;
		
		/// <summary>(2010,00A8) VR=CS Default Smoothing Type 
		/// </summary>
		public const uint DefaultSmoothingType = 0x201000A8;
		
		/// <summary>(2010,00A9) VR=CS Other Smoothing Types Available 
		/// </summary>
		public const uint OtherSmoothingTypesAvailable = 0x201000A9;
		
		/// <summary>(2010,0100) VR=CS Border Density 
		/// </summary>
		public const uint BorderDensity = 0x20100100;
		
		/// <summary>(2010,0110) VR=CS Empty Image Density 
		/// </summary>
		public const uint EmptyImageDensity = 0x20100110;
		
		/// <summary>(2010,0120) VR=US Min Density 
		/// </summary>
		public const uint MinDensity = 0x20100120;
		
		/// <summary>(2010,0130) VR=US Max Density 
		/// </summary>
		public const uint MaxDensity = 0x20100130;
		
		/// <summary>(2010,0140) VR=CS Trim 
		/// </summary>
		public const uint Trim = 0x20100140;
		
		/// <summary>(2010,0150) VR=ST Configuration Information 
		/// </summary>
		public const uint ConfigurationInformation = 0x20100150;
		
		/// <summary>(2010,0152) VR=LT Configuration Information Description 
		/// </summary>
		public const uint ConfigurationInformationDescription = 0x20100152;
		
		/// <summary>(2010,0154) VR=IS Maximum Collated Films 
		/// </summary>
		public const uint MaximumCollatedFilms = 0x20100154;
		
		/// <summary>(2010,015E) VR=US Illumination 
		/// </summary>
		public const uint Illumination = 0x2010015E;
		
		/// <summary>(2010,0160) VR=US Reflected Ambient Light 
		/// </summary>
		public const uint ReflectedAmbientLight = 0x20100160;
		
		/// <summary>(2010,0376) VR=DS Printer Pixel Spacing 
		/// </summary>
		public const uint PrinterPixelSpacing = 0x20100376;
		
		/// <summary>(2010,0500) VR=SQ Referenced Film Session Sequence 
		/// </summary>
		public const uint RefFilmSessionSeq = 0x20100500;
		
		/// <summary>(2010,0510) VR=SQ Referenced Image Box Sequence 
		/// </summary>
		public const uint RefImageBoxSeq = 0x20100510;
		
		/// <summary>(2010,0520) VR=SQ Referenced Basic Annotation Box Sequence 
		/// </summary>
		public const uint RefBasicAnnotationBoxSeq = 0x20100520;
		
		/// <summary>(2020,0010) VR=US Image Position 
		/// </summary>
		public const uint ImagePositionOnFilm = 0x20200010;
		
		/// <summary>(2020,0020) VR=CS Polarity 
		/// </summary>
		public const uint Polarity = 0x20200020;
		
		/// <summary>(2020,0030) VR=DS Requested Image Size 
		/// </summary>
		public const uint RequestedImageSize = 0x20200030;
		
		/// <summary>(2020,0040) VR=CS Requested Decimate/Crop Behavior 
		/// </summary>
		public const uint RequestedDecimateCropBehavior = 0x20200040;
		
		/// <summary>(2020,0050) VR=CS Requested Resolution ID 
		/// </summary>
		public const uint RequestedResolutionID = 0x20200050;
		
		/// <summary>(2020,00A0) VR=CS Requested Image Size Flag 
		/// </summary>
		public const uint RequestedImageSizeFlag = 0x202000A0;
		
		/// <summary>(2020,00A2) VR=CS Decimate/Crop Result 
		/// </summary>
		public const uint DecimateCropResult = 0x202000A2;
		
		/// <summary>(2020,0110) VR=SQ Basic Grayscale Image Sequence 
		/// </summary>
		public const uint BasicGrayscaleImageSeq = 0x20200110;
		
		/// <summary>(2020,0111) VR=SQ Basic Color Image Sequence 
		/// </summary>
		public const uint BasicColorImageSeq = 0x20200111;
		
		/// <summary>(2020,0130) VR=SQ Referenced Image Overlay Box Sequence (Retired) 
		/// </summary>
		public const uint RefImageOverlayBoxSeqRetired = 0x20200130;
		
		/// <summary>(2020,0140) VR=SQ Referenced VOI LUT Box Sequence (Retired) 
		/// </summary>
		public const uint RefVOILUTBoxSeqRetired = 0x20200140;
		
		/// <summary>(2030,0010) VR=US Annotation Position 
		/// </summary>
		public const uint AnnotationPosition = 0x20300010;
		
		/// <summary>(2030,0020) VR=LO Text String 
		/// </summary>
		public const uint TextString = 0x20300020;
		
		/// <summary>(2040,0010) VR=SQ Referenced Overlay Plane Sequence 
		/// </summary>
		public const uint RefOverlayPlaneSeq = 0x20400010;
		
		/// <summary>(2040,0011) VR=US Referenced Overlay Plane Groups 
		/// </summary>
		public const uint RefOverlayPlaneGroups = 0x20400011;
		
		/// <summary>(2040,0020) VR=SQ Overlay Pixel Data Sequence 
		/// </summary>
		public const uint OverlayPixelDataSeq = 0x20400020;
		
		/// <summary>(2040,0060) VR=CS Overlay Magnification Type 
		/// </summary>
		public const uint OverlayMagnificationType = 0x20400060;
		
		/// <summary>(2040,0070) VR=CS Overlay Smoothing Type 
		/// </summary>
		public const uint OverlaySmoothingType = 0x20400070;
		
		/// <summary>(2040,0072) VR=CS Overlay or Image Magnification 
		/// </summary>
		public const uint OverlayOrImageMagnification = 0x20400072;
		
		/// <summary>(2040,0074) VR=US Magnify to Number of Columns 
		/// </summary>
		public const uint MagnifyToNumberOfColumns = 0x20400074;
		
		/// <summary>(2040,0080) VR=CS Overlay Foreground Density 
		/// </summary>
		public const uint OverlayForegroundDensity = 0x20400080;
		
		/// <summary>(2040,0082) VR=CS Overlay Background Density 
		/// </summary>
		public const uint OverlayBackgroundDensity = 0x20400082;
		
		/// <summary>(2040,0090) VR=CS Overlay Mode (Retired) 
		/// </summary>
		public const uint OverlayModeRetired = 0x20400090;
		
		/// <summary>(2040,0100) VR=CS Threshold Density (Retired) 
		/// </summary>
		public const uint ThresholdDensityRetired = 0x20400100;
		
		/// <summary>(2040,0500) VR=SQ Referenced Image Box Sequence (Retired) 
		/// </summary>
		public const uint RefImageBoxSeqRetired = 0x20400500;
		
		/// <summary>(2050,0010) VR=SQ Presentation LUT Sequence 
		/// </summary>
		public const uint PresentationLUTSeq = 0x20500010;
		
		/// <summary>(2050,0020) VR=CS Presentation LUT Shape 
		/// </summary>
		public const uint PresentationLUTShape = 0x20500020;
		
		/// <summary>(2050,0500) VR=SQ Referenced Presentation LUT Sequence 
		/// </summary>
		public const uint RefPresentationLUTSeq = 0x20500500;
		
		/// <summary>(2100,0010) VR=SH Print Job ID 
		/// </summary>
		public const uint PrintJobID = 0x21000010;
		
		/// <summary>(2100,0020) VR=CS Execution Status 
		/// </summary>
		public const uint ExecutionStatus = 0x21000020;
		
		/// <summary>(2100,0030) VR=CS Execution Status Info 
		/// </summary>
		public const uint ExecutionStatusInfo = 0x21000030;
		
		/// <summary>(2100,0040) VR=DA Creation Date 
		/// </summary>
		public const uint CreationDate = 0x21000040;
		
		/// <summary>(2100,0050) VR=TM Creation Time 
		/// </summary>
		public const uint CreationTime = 0x21000050;
		
		/// <summary>(2100,0070) VR=AE Originator 
		/// </summary>
		public const uint Originator = 0x21000070;
		
		/// <summary>(2100,0140) VR=AE Destination AE 
		/// </summary>
		public const uint DestinationAE = 0x21000140;
		
		/// <summary>(2100,0160) VR=SH Owner ID 
		/// </summary>
		public const uint OwnerID = 0x21000160;
		
		/// <summary>(2100,0170) VR=IS Number of Films 
		/// </summary>
		public const uint NumberOfFilms = 0x21000170;
		
		/// <summary>(2100,0500) VR=SQ Referenced Print Job Sequence 
		/// </summary>
		public const uint RefPrintJobSeq = 0x21000500;
		
		/// <summary>(2110,0010) VR=CS Printer Status 
		/// </summary>
		public const uint PrinterStatus = 0x21100010;
		
		/// <summary>(2110,0020) VR=CS Printer Status Info 
		/// </summary>
		public const uint PrinterStatusInfo = 0x21100020;
		
		/// <summary>(2110,0030) VR=LO Printer Name 
		/// </summary>
		public const uint PrinterName = 0x21100030;
		
		/// <summary>(2110,0099) VR=SH Print Queue ID 
		/// </summary>
		public const uint PrintQueueID = 0x21100099;
		
		/// <summary>(2120,0010) VR=CS Queue Status 
		/// </summary>
		public const uint QueueStatus = 0x21200010;
		
		/// <summary>(2120,0050) VR=SQ Print Job Description Sequence 
		/// </summary>
		public const uint PrintJobDescriptionSeq = 0x21200050;
		
		/// <summary>(2120,0070) VR=SQ Referenced Print Job Sequence 
		/// </summary>
		public const uint RefPrintJobSeqInQueue = 0x21200070;
		
		/// <summary>(2130,0010) VR=SQ Print Management Capabilities Sequence 
		/// </summary>
		public const uint PrintManagementCapabilitiesSeq = 0x21300010;
		
		/// <summary>(2130,0015) VR=SQ Printer Characteristics Sequence 
		/// </summary>
		public const uint PrinterCharacteristicsSeq = 0x21300015;
		
		/// <summary>(2130,0030) VR=SQ Film Box Content Sequence 
		/// </summary>
		public const uint FilmBoxContentSeq = 0x21300030;
		
		/// <summary>(2130,0040) VR=SQ Image Box Content Sequence 
		/// </summary>
		public const uint ImageBoxContentSeq = 0x21300040;
		
		/// <summary>(2130,0050) VR=SQ Annotation Content Sequence 
		/// </summary>
		public const uint AnnotationContentSeq = 0x21300050;
		
		/// <summary>(2130,0060) VR=SQ Image Overlay Box Content Sequence 
		/// </summary>
		public const uint ImageOverlayBoxContentSeq = 0x21300060;
		
		/// <summary>(2130,0080) VR=SQ Presentation LUT Content Sequence 
		/// </summary>
		public const uint PresentationLUTContentSeq = 0x21300080;
		
		/// <summary>(2130,00A0) VR=SQ Proposed Study Sequence 
		/// </summary>
		public const uint ProposedStudySeq = 0x213000A0;
		
		/// <summary>(2130,00C0) VR=SQ Original Image Sequence 
		/// </summary>
		public const uint OriginalImageSeq = 0x213000C0;
		
		/// <summary>(3002,0002) VR=SH RT Image Label 
		/// </summary>
		public const uint RTImageLabel = 0x30020002;
		
		/// <summary>(3002,0003) VR=LO RT Image Name 
		/// </summary>
		public const uint RTImageName = 0x30020003;
		
		/// <summary>(3002,0004) VR=ST RT Image Description 
		/// </summary>
		public const uint RTImageDescription = 0x30020004;
		
		/// <summary>(3002,000A) VR=CS Reported Values Origin 
		/// </summary>
		public const uint ReportedValuesOrigin = 0x3002000A;
		
		/// <summary>(3002,000C) VR=CS RT Image Plane 
		/// </summary>
		public const uint RTImagePlane = 0x3002000C;
		
		/// <summary>(3002,000D) VR=DS X-Ray Image Receptor Translation 
		/// </summary>
		public const uint XRayImageReceptorTranslation = 0x3002000D;
		
		/// <summary>(3002,000E) VR=DS X-Ray Image Receptor Angle 
		/// </summary>
		public const uint XRayImageReceptorAngle = 0x3002000E;
		
		/// <summary>(3002,0010) VR=DS RT Image Orientation 
		/// </summary>
		public const uint RTImageOrientation = 0x30020010;
		
		/// <summary>(3002,0011) VR=DS Image Plane Pixel Spacing 
		/// </summary>
		public const uint ImagePlanePixelSpacing = 0x30020011;
		
		/// <summary>(3002,0012) VR=DS RT Image Position 
		/// </summary>
		public const uint RTImagePosition = 0x30020012;
		
		/// <summary>(3002,0020) VR=SH Radiation Machine Name 
		/// </summary>
		public const uint RadiationMachineName = 0x30020020;
		
		/// <summary>(3002,0022) VR=DS Radiation Machine SAD 
		/// </summary>
		public const uint RadiationMachineSAD = 0x30020022;
		
		/// <summary>(3002,0024) VR=DS Radiation Machine SSD 
		/// </summary>
		public const uint RadiationMachineSSD = 0x30020024;
		
		/// <summary>(3002,0026) VR=DS RT Image SID 
		/// </summary>
		public const uint RTImageSID = 0x30020026;
		
		/// <summary>(3002,0028) VR=DS Source to Reference Object Distance 
		/// </summary>
		public const uint SourceToReferenceObjectDistance = 0x30020028;
		
		/// <summary>(3002,0029) VR=IS Fraction Number 
		/// </summary>
		public const uint FractionNumber = 0x30020029;
		
		/// <summary>(3002,0030) VR=SQ Exposure Sequence 
		/// </summary>
		public const uint ExposureSeq = 0x30020030;
		
		/// <summary>(3002,0032) VR=DS Meterset Exposure 
		/// </summary>
		public const uint MetersetExposure = 0x30020032;
		
		/// <summary>(3004,0001) VR=CS DVH Type 
		/// </summary>
		public const uint DVHType = 0x30040001;
		
		/// <summary>(3004,0002) VR=CS Dose Units 
		/// </summary>
		public const uint DoseUnits = 0x30040002;
		
		/// <summary>(3004,0004) VR=CS Dose Type 
		/// </summary>
		public const uint DoseType = 0x30040004;
		
		/// <summary>(3004,0006) VR=LO Dose Comment 
		/// </summary>
		public const uint DoseComment = 0x30040006;
		
		/// <summary>(3004,0008) VR=DS Normalization Point 
		/// </summary>
		public const uint NormalizationPoint = 0x30040008;
		
		/// <summary>(3004,000A) VR=CS Dose Summation Type 
		/// </summary>
		public const uint DoseSummationType = 0x3004000A;
		
		/// <summary>(3004,000C) VR=DS Grid Frame Offset Vector 
		/// </summary>
		public const uint GridFrameOffsetVector = 0x3004000C;
		
		/// <summary>(3004,000E) VR=DS Dose Grid Scaling 
		/// </summary>
		public const uint DoseGridScaling = 0x3004000E;
		
		/// <summary>(3004,0010) VR=SQ RT Dose ROI Sequence 
		/// </summary>
		public const uint RTDoseROISeq = 0x30040010;
		
		/// <summary>(3004,0012) VR=DS Dose Value 
		/// </summary>
		public const uint DoseValue = 0x30040012;
		
		/// <summary>(3004,0040) VR=DS DVH Normalization Point 
		/// </summary>
		public const uint DVHNormalizationPoint = 0x30040040;
		
		/// <summary>(3004,0042) VR=DS DVH Normalization Dose Value 
		/// </summary>
		public const uint DVHNormalizationDoseValue = 0x30040042;
		
		/// <summary>(3004,0050) VR=SQ DVH Sequence 
		/// </summary>
		public const uint DVHSeq = 0x30040050;
		
		/// <summary>(3004,0052) VR=DS DVH Dose Scaling 
		/// </summary>
		public const uint DVHDoseScaling = 0x30040052;
		
		/// <summary>(3004,0054) VR=CS DVH Volume Units 
		/// </summary>
		public const uint DVHVolumeUnits = 0x30040054;
		
		/// <summary>(3004,0056) VR=IS DVH Number of Bins 
		/// </summary>
		public const uint DVHNumberOfBins = 0x30040056;
		
		/// <summary>(3004,0058) VR=DS DVH Data 
		/// </summary>
		public const uint DVHData = 0x30040058;
		
		/// <summary>(3004,0060) VR=SQ DVH Referenced ROI Sequence 
		/// </summary>
		public const uint DVHRefROISeq = 0x30040060;
		
		/// <summary>(3004,0062) VR=CS DVH ROI Contribution Type 
		/// </summary>
		public const uint DVHROIContributionType = 0x30040062;
		
		/// <summary>(3004,0070) VR=DS DVH Minimum Dose 
		/// </summary>
		public const uint DVHMinimumDose = 0x30040070;
		
		/// <summary>(3004,0072) VR=DS DVH Maximum Dose 
		/// </summary>
		public const uint DVHMaximumDose = 0x30040072;
		
		/// <summary>(3004,0074) VR=DS DVH Mean Dose 
		/// </summary>
		public const uint DVHMeanDose = 0x30040074;
		
		/// <summary>(3006,0002) VR=SH Structure Set Label 
		/// </summary>
		public const uint StructureSetLabel = 0x30060002;
		
		/// <summary>(3006,0004) VR=LO Structure Set Name 
		/// </summary>
		public const uint StructureSetName = 0x30060004;
		
		/// <summary>(3006,0006) VR=ST Structure Set Description 
		/// </summary>
		public const uint StructureSetDescription = 0x30060006;
		
		/// <summary>(3006,0008) VR=DA Structure Set Date 
		/// </summary>
		public const uint StructureSetDate = 0x30060008;
		
		/// <summary>(3006,0009) VR=TM Structure Set Time 
		/// </summary>
		public const uint StructureSetTime = 0x30060009;
		
		/// <summary>(3006,0010) VR=SQ Referenced Frame of Reference Sequence 
		/// </summary>
		public const uint RefFrameOfReferenceSeq = 0x30060010;
		
		/// <summary>(3006,0012) VR=SQ RT Referenced Study Sequence 
		/// </summary>
		public const uint RTRefStudySeq = 0x30060012;
		
		/// <summary>(3006,0014) VR=SQ RT Referenced Series Sequence 
		/// </summary>
		public const uint RTRefSeriesSeq = 0x30060014;
		
		/// <summary>(3006,0016) VR=SQ Contour Image Sequence 
		/// </summary>
		public const uint ContourImageSeq = 0x30060016;
		
		/// <summary>(3006,0020) VR=SQ Structure Set ROI Sequence 
		/// </summary>
		public const uint StructureSetROISeq = 0x30060020;
		
		/// <summary>(3006,0022) VR=IS ROI Number 
		/// </summary>
		public const uint ROINumber = 0x30060022;
		
		/// <summary>(3006,0024) VR=UI Referenced Frame of Reference UID 
		/// </summary>
		public const uint RefFrameOfReferenceUID = 0x30060024;
		
		/// <summary>(3006,0026) VR=LO ROI Name 
		/// </summary>
		public const uint ROIName = 0x30060026;
		
		/// <summary>(3006,0028) VR=ST ROI Description 
		/// </summary>
		public const uint ROIDescription = 0x30060028;
		
		/// <summary>(3006,002A) VR=IS ROI Display Color 
		/// </summary>
		public const uint ROIDisplayColor = 0x3006002A;
		
		/// <summary>(3006,002C) VR=DS ROI Volume 
		/// </summary>
		public const uint ROIVolume = 0x3006002C;
		
		/// <summary>(3006,0030) VR=SQ RT Related ROI Sequence 
		/// </summary>
		public const uint RTRelatedROISeq = 0x30060030;
		
		/// <summary>(3006,0033) VR=CS RT ROI Relationship 
		/// </summary>
		public const uint RTROIRelationship = 0x30060033;
		
		/// <summary>(3006,0036) VR=CS ROI Generation Algorithm 
		/// </summary>
		public const uint ROIGenerationAlgorithm = 0x30060036;
		
		/// <summary>(3006,0038) VR=LO ROI Generation Description 
		/// </summary>
		public const uint ROIGenerationDescription = 0x30060038;
		
		/// <summary>(3006,0039) VR=SQ ROI Contour Sequence 
		/// </summary>
		public const uint ROIContourSeq = 0x30060039;
		
		/// <summary>(3006,0040) VR=SQ Contour Sequence 
		/// </summary>
		public const uint ContourSeq = 0x30060040;
		
		/// <summary>(3006,0042) VR=CS Contour Geometric Type 
		/// </summary>
		public const uint ContourGeometricType = 0x30060042;
		
		/// <summary>(3006,0044) VR=DS Contour Slab Thickness 
		/// </summary>
		public const uint ContourSlabThickness = 0x30060044;
		
		/// <summary>(3006,0045) VR=DS Contour Offset Vector 
		/// </summary>
		public const uint ContourOffsetVector = 0x30060045;
		
		/// <summary>(3006,0046) VR=IS Number of Contour Points 
		/// </summary>
		public const uint NumberOfContourPoints = 0x30060046;
		
		/// <summary>(3006,0048) VR=IS Contour Number 
		/// </summary>
		public const uint ContourNumber = 0x30060048;
		
		/// <summary>(3006,0049) VR=IS Attached Contours 
		/// </summary>
		public const uint AttachedContours = 0x30060049;
		
		/// <summary>(3006,0050) VR=DS Contour Data 
		/// </summary>
		public const uint ContourData = 0x30060050;
		
		/// <summary>(3006,0080) VR=SQ RT ROI Observations Sequence 
		/// </summary>
		public const uint RTROIObservationsSeq = 0x30060080;
		
		/// <summary>(3006,0082) VR=IS Observation Number 
		/// </summary>
		public const uint ObservationNumber = 0x30060082;
		
		/// <summary>(3006,0084) VR=IS Referenced ROI Number 
		/// </summary>
		public const uint RefROINumber = 0x30060084;
		
		/// <summary>(3006,0085) VR=SH ROI Observation Label 
		/// </summary>
		public const uint ROIObservationLabel = 0x30060085;
		
		/// <summary>(3006,0086) VR=SQ RT ROI Identification Code Sequence 
		/// </summary>
		public const uint RTROIIdentificationCodeSeq = 0x30060086;
		
		/// <summary>(3006,0088) VR=ST ROI Observation Description 
		/// </summary>
		public const uint ROIObservationDescription = 0x30060088;
		
		/// <summary>(3006,00A0) VR=SQ Related RT ROI Observations Sequence 
		/// </summary>
		public const uint RelatedRTROIObservationsSeq = 0x300600A0;
		
		/// <summary>(3006,00A4) VR=CS RT ROI Interpreted Type 
		/// </summary>
		public const uint RTROIInterpretedType = 0x300600A4;
		
		/// <summary>(3006,00A6) VR=PN ROI Interpreter 
		/// </summary>
		public const uint ROIInterpreter = 0x300600A6;
		
		/// <summary>(3006,00B0) VR=SQ ROI Physical Properties Sequence 
		/// </summary>
		public const uint ROIPhysicalPropertiesSeq = 0x300600B0;
		
		/// <summary>(3006,00B2) VR=CS ROI Physical Property 
		/// </summary>
		public const uint ROIPhysicalProperty = 0x300600B2;
		
		/// <summary>(3006,00B4) VR=DS ROI Physical Property Value 
		/// </summary>
		public const uint ROIPhysicalPropertyValue = 0x300600B4;
		
		/// <summary>(3006,00C0) VR=SQ Frame of Reference Relationship Sequence 
		/// </summary>
		public const uint FrameOfReferenceRelationshipSeq = 0x300600C0;
		
		/// <summary>(3006,00C2) VR=UI Related Frame of Reference UID 
		/// </summary>
		public const uint RelatedFrameOfReferenceUID = 0x300600C2;
		
		/// <summary>(3006,00C4) VR=CS Frame of Reference Transformation Type 
		/// </summary>
		public const uint FrameOfReferenceTransformationType = 0x300600C4;
		
		/// <summary>(3006,00C6) VR=DS Frame of Reference Transformation Matrix 
		/// </summary>
		public const uint FrameOfReferenceTransformationMatrix = 0x300600C6;
		
		/// <summary>(3006,00C8) VR=LO Frame of Reference Transformation Comment 
		/// </summary>
		public const uint FrameOfReferenceTransformationComment = 0x300600C8;
		
		/// <summary>(3008,0010) VR=SQ Measured Dose Reference Sequence 
		/// </summary>
		public const uint MeasuredDoseReferenceSeq = 0x30080010;
		
		/// <summary>(3008,0012) VR=ST Measured Dose Description 
		/// </summary>
		public const uint MeasuredDoseDescription = 0x30080012;
		
		/// <summary>(3008,0014) VR=CS Measured Dose Type 
		/// </summary>
		public const uint MeasuredDoseType = 0x30080014;
		
		/// <summary>(3008,0016) VR=DS Measured Dose Value 
		/// </summary>
		public const uint MeasuredDoseValue = 0x30080016;
		
		/// <summary>(3008,0020) VR=SQ Treatment Session Beam Sequence 
		/// </summary>
		public const uint TreatmentSessionBeamSeq = 0x30080020;
		
		/// <summary>(3008,0022) VR=IS Current Fraction Number 
		/// </summary>
		public const uint CurrentFractionNumber = 0x30080022;
		
		/// <summary>(3008,0024) VR=DA Treatment Control Point Date 
		/// </summary>
		public const uint TreatmentControlPointDate = 0x30080024;
		
		/// <summary>(3008,0025) VR=TM Treatment Control Point Time 
		/// </summary>
		public const uint TreatmentControlPointTime = 0x30080025;
		
		/// <summary>(3008,002A) VR=CS Treatment Termination Status 
		/// </summary>
		public const uint TreatmentTerminationStatus = 0x3008002A;
		
		/// <summary>(3008,002B) VR=SH Treatment Termination Code 
		/// </summary>
		public const uint TreatmentTerminationCode = 0x3008002B;
		
		/// <summary>(3008,002C) VR=CS Treatment Verification Status 
		/// </summary>
		public const uint TreatmentVerificationStatus = 0x3008002C;
		
		/// <summary>(3008,0030) VR=SQ Referenced Treatment Record Sequence 
		/// </summary>
		public const uint RefTreatmentRecordSeq = 0x30080030;
		
		/// <summary>(3008,0032) VR=DS Specified Primary Meterset 
		/// </summary>
		public const uint SpecifiedPrimaryMeterset = 0x30080032;
		
		/// <summary>(3008,0033) VR=DS Specified Secondary Meterset 
		/// </summary>
		public const uint SpecifiedSecondaryMeterset = 0x30080033;
		
		/// <summary>(3008,0036) VR=DS Delivered Primary Meterset 
		/// </summary>
		public const uint DeliveredPrimaryMeterset = 0x30080036;
		
		/// <summary>(3008,0037) VR=DS Delivered Secondary Meterset 
		/// </summary>
		public const uint DeliveredSecondaryMeterset = 0x30080037;
		
		/// <summary>(3008,003A) VR=DS Specified Treatment Time 
		/// </summary>
		public const uint SpecifiedTreatmentTime = 0x3008003A;
		
		/// <summary>(3008,003B) VR=DS Delivered Treatment Time 
		/// </summary>
		public const uint DeliveredTreatmentTime = 0x3008003B;
		
		/// <summary>(3008,0040) VR=SQ Control Point Delivery Sequence 
		/// </summary>
		public const uint ControlPointDeliverySeq = 0x30080040;
		
		/// <summary>(3008,0042) VR=DS Specified Meterset 
		/// </summary>
		public const uint SpecifiedMeterset = 0x30080042;
		
		/// <summary>(3008,0044) VR=DS Delivered Meterset 
		/// </summary>
		public const uint DeliveredMeterset = 0x30080044;
		
		/// <summary>(3008,0048) VR=DS Dose Rate Delivered 
		/// </summary>
		public const uint DoseRateDelivered = 0x30080048;
		
		/// <summary>(3008,0050) VR=SQ Treatment Summary Calculated Dose Reference Sequence 
		/// </summary>
		public const uint TreatmentSummaryCalculatedDoseReferenceSeq = 0x30080050;
		
		/// <summary>(3008,0052) VR=DS Cumulative Dose to Dose Reference 
		/// </summary>
		public const uint CumulativeDoseToDoseReference = 0x30080052;
		
		/// <summary>(3008,0054) VR=DA First Treatment Date 
		/// </summary>
		public const uint FirstTreatmentDate = 0x30080054;
		
		/// <summary>(3008,0056) VR=DA Most Recent Treatment Date 
		/// </summary>
		public const uint MostRecentTreatmentDate = 0x30080056;
		
		/// <summary>(3008,005A) VR=IS Number of Fractions Delivered 
		/// </summary>
		public const uint NumberOfFractionsDelivered = 0x3008005A;
		
		/// <summary>(3008,0060) VR=SQ Override Sequence 
		/// </summary>
		public const uint OverrideSeq = 0x30080060;
		
		/// <summary>(3008,0062) VR=AT Override Parameter Pointer 
		/// </summary>
		public const uint OverrideParameterPointer = 0x30080062;
		
		/// <summary>(3008,0064) VR=IS Measured Dose Reference Number 
		/// </summary>
		public const uint MeasuredDoseReferenceNumber = 0x30080064;
		
		/// <summary>(3008,0066) VR=ST Override Reason 
		/// </summary>
		public const uint OverrideReason = 0x30080066;
		
		/// <summary>(3008,0070) VR=SQ Calculated Dose Reference Sequence 
		/// </summary>
		public const uint CalculatedDoseReferenceSeq = 0x30080070;
		
		/// <summary>(3008,0072) VR=IS Calculated Dose Reference Number 
		/// </summary>
		public const uint CalculatedDoseReferenceNumber = 0x30080072;
		
		/// <summary>(3008,0074) VR=ST Calculated Dose Reference Description 
		/// </summary>
		public const uint CalculatedDoseReferenceDescription = 0x30080074;
		
		/// <summary>(3008,0076) VR=DS Calculated Dose Reference Dose Value 
		/// </summary>
		public const uint CalculatedDoseReferenceDoseValue = 0x30080076;
		
		/// <summary>(3008,0078) VR=DS Start Meterset 
		/// </summary>
		public const uint StartMeterset = 0x30080078;
		
		/// <summary>(3008,007A) VR=DS End Meterset 
		/// </summary>
		public const uint EndMeterset = 0x3008007A;
		
		/// <summary>(3008,0080) VR=SQ Referenced Measured Dose Reference Sequence 
		/// </summary>
		public const uint RefMeasuredDoseReferenceSeq = 0x30080080;
		
		/// <summary>(3008,0082) VR=IS Referenced Measured Dose Reference Number 
		/// </summary>
		public const uint RefMeasuredDoseReferenceNumber = 0x30080082;
		
		/// <summary>(3008,0090) VR=SQ Referenced Calculated Dose Reference Sequence 
		/// </summary>
		public const uint RefCalculatedDoseReferenceSeq = 0x30080090;
		
		/// <summary>(3008,0092) VR=IS Referenced Calculated Dose Reference Number 
		/// </summary>
		public const uint RefCalculatedDoseReferenceNumber = 0x30080092;
		
		/// <summary>(3008,00A0) VR=SQ Beam Limiting Device Leaf Pairs Sequence 
		/// </summary>
		public const uint BeamLimitingDeviceLeafPairsSeq = 0x300800A0;
		
		/// <summary>(3008,00B0) VR=SQ Recorded Wedge Sequence 
		/// </summary>
		public const uint RecordedWedgeSeq = 0x300800B0;
		
		/// <summary>(3008,00C0) VR=SQ Recorded Compensator Sequence 
		/// </summary>
		public const uint RecordedCompensatorSeq = 0x300800C0;
		
		/// <summary>(3008,00D0) VR=SQ Recorded Block Sequence 
		/// </summary>
		public const uint RecordedBlockSeq = 0x300800D0;
		
		/// <summary>(3008,00E0) VR=SQ Treatment Summary Measured Dose Reference Sequence 
		/// </summary>
		public const uint TreatmentSummaryMeasuredDoseReferenceSeq = 0x300800E0;
		
		/// <summary>(3008,0100) VR=SQ Recorded Source Sequence 
		/// </summary>
		public const uint RecordedSourceSeq = 0x30080100;
		
		/// <summary>(3008,0105) VR=LO Source Serial Number 
		/// </summary>
		public const uint SourceSerialNumber = 0x30080105;
		
		/// <summary>(3008,0110) VR=SQ Treatment Session Application Setup Sequence 
		/// </summary>
		public const uint TreatmentSessionApplicationSetupSeq = 0x30080110;
		
		/// <summary>(3008,0116) VR=CS Application Setup Check 
		/// </summary>
		public const uint ApplicationSetupCheck = 0x30080116;
		
		/// <summary>(3008,0120) VR=SQ Recorded Brachy Accessory Device Sequence 
		/// </summary>
		public const uint RecordedBrachyAccessoryDeviceSeq = 0x30080120;
		
		/// <summary>(3008,0122) VR=IS Referenced Brachy Accessory Device Number 
		/// </summary>
		public const uint RefBrachyAccessoryDeviceNumber = 0x30080122;
		
		/// <summary>(3008,0130) VR=SQ Recorded Channel Sequence 
		/// </summary>
		public const uint RecordedChannelSeq = 0x30080130;
		
		/// <summary>(3008,0132) VR=DS Specified Channel Total Time 
		/// </summary>
		public const uint SpecifiedChannelTotalTime = 0x30080132;
		
		/// <summary>(3008,0134) VR=DS Delivered Channel Total Time 
		/// </summary>
		public const uint DeliveredChannelTotalTime = 0x30080134;
		
		/// <summary>(3008,0136) VR=IS Specified Number of Pulses 
		/// </summary>
		public const uint SpecifiedNumberOfPulses = 0x30080136;
		
		/// <summary>(3008,0138) VR=IS Delivered Number of Pulses 
		/// </summary>
		public const uint DeliveredNumberOfPulses = 0x30080138;
		
		/// <summary>(3008,013A) VR=DS Specified Pulse Repetition Interval 
		/// </summary>
		public const uint SpecifiedPulseRepetitionInterval = 0x3008013A;
		
		/// <summary>(3008,013C) VR=DS Delivered Pulse Repetition Interval 
		/// </summary>
		public const uint DeliveredPulseRepetitionInterval = 0x3008013C;
		
		/// <summary>(3008,0140) VR=SQ Recorded Source Applicator Sequence 
		/// </summary>
		public const uint RecordedSourceApplicatorSeq = 0x30080140;
		
		/// <summary>(3008,0142) VR=IS Referenced Source Applicator Number 
		/// </summary>
		public const uint RefSourceApplicatorNumber = 0x30080142;
		
		/// <summary>(3008,0150) VR=SQ Recorded Channel Shield Sequence 
		/// </summary>
		public const uint RecordedChannelShieldSeq = 0x30080150;
		
		/// <summary>(3008,0152) VR=IS Referenced Channel Shield Number 
		/// </summary>
		public const uint RefChannelShieldNumber = 0x30080152;
		
		/// <summary>(3008,0160) VR=SQ Brachy Control Point Delivered Sequence 
		/// </summary>
		public const uint BrachyControlPointDeliveredSeq = 0x30080160;
		
		/// <summary>(3008,0162) VR=DA Safe Position Exit Date 
		/// </summary>
		public const uint SafePositionExitDate = 0x30080162;
		
		/// <summary>(3008,0164) VR=TM Safe Position Exit Time 
		/// </summary>
		public const uint SafePositionExitTime = 0x30080164;
		
		/// <summary>(3008,0166) VR=DA Safe Position Return Date 
		/// </summary>
		public const uint SafePositionReturnDate = 0x30080166;
		
		/// <summary>(3008,0168) VR=TM Safe Position Return Time 
		/// </summary>
		public const uint SafePositionReturnTime = 0x30080168;
		
		/// <summary>(3008,0200) VR=CS Current Treatment Status 
		/// </summary>
		public const uint CurrentTreatmentStatus = 0x30080200;
		
		/// <summary>(3008,0202) VR=ST Treatment Status Comment 
		/// </summary>
		public const uint TreatmentStatusComment = 0x30080202;
		
		/// <summary>(3008,0220) VR=SQ Fraction Group Summary Sequence 
		/// </summary>
		public const uint FractionGroupSummarySeq = 0x30080220;
		
		/// <summary>(3008,0223) VR=IS Referenced Fraction Number 
		/// </summary>
		public const uint RefFractionNumber = 0x30080223;
		
		/// <summary>(3008,0224) VR=CS Fraction Group Type 
		/// </summary>
		public const uint FractionGroupType = 0x30080224;
		
		/// <summary>(3008,0230) VR=CS Beam Stopper Position 
		/// </summary>
		public const uint BeamStopperPosition = 0x30080230;
		
		/// <summary>(3008,0240) VR=SQ Fraction Status Summary Sequence 
		/// </summary>
		public const uint FractionStatusSummarySeq = 0x30080240;
		
		/// <summary>(3008,0250) VR=DA Treatment Date 
		/// </summary>
		public const uint TreatmentDate = 0x30080250;
		
		/// <summary>(3008,0251) VR=TM Treatment Time 
		/// </summary>
		public const uint TreatmentTime = 0x30080251;
		
		/// <summary>(300A,0002) VR=SH RT Plan Label 
		/// </summary>
		public const uint RTPlanLabel = 0x300A0002;
		
		/// <summary>(300A,0003) VR=LO RT Plan Name 
		/// </summary>
		public const uint RTPlanName = 0x300A0003;
		
		/// <summary>(300A,0004) VR=ST RT Plan Description 
		/// </summary>
		public const uint RTPlanDescription = 0x300A0004;
		
		/// <summary>(300A,0006) VR=DA RT Plan Date 
		/// </summary>
		public const uint RTPlanDate = 0x300A0006;
		
		/// <summary>(300A,0007) VR=TM RT Plan Time 
		/// </summary>
		public const uint RTPlanTime = 0x300A0007;
		
		/// <summary>(300A,0009) VR=LO Treatment Protocols 
		/// </summary>
		public const uint TreatmentProtocols = 0x300A0009;
		
		/// <summary>(300A,000A) VR=CS Treatment Intent 
		/// </summary>
		public const uint TreatmentIntent = 0x300A000A;
		
		/// <summary>(300A,000B) VR=LO Treatment Sites 
		/// </summary>
		public const uint TreatmentSites = 0x300A000B;
		
		/// <summary>(300A,000C) VR=CS RT Plan Geometry 
		/// </summary>
		public const uint RTPlanGeometry = 0x300A000C;
		
		/// <summary>(300A,000E) VR=ST Prescription Description 
		/// </summary>
		public const uint PrescriptionDescription = 0x300A000E;
		
		/// <summary>(300A,0010) VR=SQ Dose Reference Sequence 
		/// </summary>
		public const uint DoseReferenceSeq = 0x300A0010;
		
		/// <summary>(300A,0012) VR=IS Dose Reference Number 
		/// </summary>
		public const uint DoseReferenceNumber = 0x300A0012;
		
		/// <summary>(300A,0014) VR=CS Dose Reference Structure Type 
		/// </summary>
		public const uint DoseReferenceStructureType = 0x300A0014;
		
		/// <summary>(300A,0015) VR=CS Nominal Beam Energy Unit 
		/// </summary>
		public const uint NominalBeamEnergyUnit = 0x300A0015;
		
		/// <summary>(300A,0016) VR=LO Dose Reference Description 
		/// </summary>
		public const uint DoseReferenceDescription = 0x300A0016;
		
		/// <summary>(300A,0018) VR=DS Dose Reference Point Coordinates 
		/// </summary>
		public const uint DoseReferencePointCoordinates = 0x300A0018;
		
		/// <summary>(300A,001A) VR=DS Nominal Prior Dose 
		/// </summary>
		public const uint NominalPriorDose = 0x300A001A;
		
		/// <summary>(300A,0020) VR=CS Dose Reference Type 
		/// </summary>
		public const uint DoseReferenceType = 0x300A0020;
		
		/// <summary>(300A,0021) VR=DS Constraint Weight 
		/// </summary>
		public const uint ConstraintWeight = 0x300A0021;
		
		/// <summary>(300A,0022) VR=DS Delivery Warning Dose 
		/// </summary>
		public const uint DeliveryWarningDose = 0x300A0022;
		
		/// <summary>(300A,0023) VR=DS Delivery Maximum Dose 
		/// </summary>
		public const uint DeliveryMaximumDose = 0x300A0023;
		
		/// <summary>(300A,0025) VR=DS Target Minimum Dose 
		/// </summary>
		public const uint TargetMinimumDose = 0x300A0025;
		
		/// <summary>(300A,0026) VR=DS Target Prescription Dose 
		/// </summary>
		public const uint TargetPrescriptionDose = 0x300A0026;
		
		/// <summary>(300A,0027) VR=DS Target Maximum Dose 
		/// </summary>
		public const uint TargetMaximumDose = 0x300A0027;
		
		/// <summary>(300A,0028) VR=DS Target Underdose Volume Fraction 
		/// </summary>
		public const uint TargetUnderdoseVolumeFraction = 0x300A0028;
		
		/// <summary>(300A,002A) VR=DS Organ at Risk Full-volume Dose 
		/// </summary>
		public const uint OrganAtRiskFullVolumeDose = 0x300A002A;
		
		/// <summary>(300A,002B) VR=DS Organ at Risk Limit Dose 
		/// </summary>
		public const uint OrganAtRiskLimitDose = 0x300A002B;
		
		/// <summary>(300A,002C) VR=DS Organ at Risk Maximum Dose 
		/// </summary>
		public const uint OrganAtRiskMaximumDose = 0x300A002C;
		
		/// <summary>(300A,002D) VR=DS Organ at Risk Overdose Volume Fraction 
		/// </summary>
		public const uint OrganAtRiskOverdoseVolumeFraction = 0x300A002D;
		
		/// <summary>(300A,0040) VR=SQ Tolerance Table Sequence 
		/// </summary>
		public const uint ToleranceTableSeq = 0x300A0040;
		
		/// <summary>(300A,0042) VR=IS Tolerance Table Number 
		/// </summary>
		public const uint ToleranceTableNumber = 0x300A0042;
		
		/// <summary>(300A,0043) VR=SH Tolerance Table Label 
		/// </summary>
		public const uint ToleranceTableLabel = 0x300A0043;
		
		/// <summary>(300A,0044) VR=DS Gantry Angle Tolerance 
		/// </summary>
		public const uint GantryAngleTolerance = 0x300A0044;
		
		/// <summary>(300A,0046) VR=DS Beam Limiting Device Angle Tolerance 
		/// </summary>
		public const uint BeamLimitingDeviceAngleTolerance = 0x300A0046;
		
		/// <summary>(300A,0048) VR=SQ Beam Limiting Device Tolerance Sequence 
		/// </summary>
		public const uint BeamLimitingDeviceToleranceSeq = 0x300A0048;
		
		/// <summary>(300A,004A) VR=DS Beam Limiting Device Position Tolerance 
		/// </summary>
		public const uint BeamLimitingDevicePositionTolerance = 0x300A004A;
		
		/// <summary>(300A,004C) VR=DS Patient Support Angle Tolerance 
		/// </summary>
		public const uint PatientSupportAngleTolerance = 0x300A004C;
		
		/// <summary>(300A,004E) VR=DS Table Top Eccentric Angle Tolerance 
		/// </summary>
		public const uint TableTopEccentricAngleTolerance = 0x300A004E;
		
		/// <summary>(300A,0051) VR=DS Table Top Vertical Position Tolerance 
		/// </summary>
		public const uint TableTopVerticalPositionTolerance = 0x300A0051;
		
		/// <summary>(300A,0052) VR=DS Table Top Longitudinal Position Tolerance 
		/// </summary>
		public const uint TableTopLongitudinalPositionTolerance = 0x300A0052;
		
		/// <summary>(300A,0053) VR=DS Table Top Lateral Position Tolerance 
		/// </summary>
		public const uint TableTopLateralPositionTolerance = 0x300A0053;
		
		/// <summary>(300A,0055) VR=CS RT Plan Relationship 
		/// </summary>
		public const uint RTPlanRelationship = 0x300A0055;
		
		/// <summary>(300A,0070) VR=SQ Fraction Group Sequence 
		/// </summary>
		public const uint FractionGroupSeq = 0x300A0070;
		
		/// <summary>(300A,0071) VR=IS Fraction Group Number 
		/// </summary>
		public const uint FractionGroupNumber = 0x300A0071;
		
		/// <summary>(300A,0078) VR=IS Number of Fractions Planned 
		/// </summary>
		public const uint NumberOfFractionsPlanned = 0x300A0078;
		
		/// <summary>(300A,0079) VR=IS Number of Fractions Per Day 
		/// </summary>
		public const uint NumberOfFractionsPerDay = 0x300A0079;
		
		/// <summary>(300A,007A) VR=IS Repeat Fraction Cycle Length 
		/// </summary>
		public const uint RepeatFractionCycleLength = 0x300A007A;
		
		/// <summary>(300A,007B) VR=LT Fraction Pattern 
		/// </summary>
		public const uint FractionPattern = 0x300A007B;
		
		/// <summary>(300A,0080) VR=IS Number of Beams 
		/// </summary>
		public const uint NumberOfBeams = 0x300A0080;
		
		/// <summary>(300A,0082) VR=DS Beam Dose Specification Point 
		/// </summary>
		public const uint BeamDoseSpecificationPoint = 0x300A0082;
		
		/// <summary>(300A,0084) VR=DS Beam Dose 
		/// </summary>
		public const uint BeamDose = 0x300A0084;
		
		/// <summary>(300A,0086) VR=DS Beam Meterset 
		/// </summary>
		public const uint BeamMeterset = 0x300A0086;
		
		/// <summary>(300A,00A0) VR=IS Number of Brachy Application Setups 
		/// </summary>
		public const uint NumberOfBrachyApplicationSetups = 0x300A00A0;
		
		/// <summary>(300A,00A2) VR=DS Brachy Application Setup Dose Specification Point 
		/// </summary>
		public const uint BrachyApplicationSetupDoseSpecificationPoint = 0x300A00A2;
		
		/// <summary>(300A,00A4) VR=DS Brachy Application Setup Dose 
		/// </summary>
		public const uint BrachyApplicationSetupDose = 0x300A00A4;
		
		/// <summary>(300A,00B0) VR=SQ Beam Sequence 
		/// </summary>
		public const uint BeamSeq = 0x300A00B0;
		
		/// <summary>(300A,00B2) VR=SH Treatment Machine Name 
		/// </summary>
		public const uint TreatmentMachineName = 0x300A00B2;
		
		/// <summary>(300A,00B3) VR=CS Primary Dosimeter Unit 
		/// </summary>
		public const uint PrimaryDosimeterUnit = 0x300A00B3;
		
		/// <summary>(300A,00B4) VR=DS Source-Axis Distance 
		/// </summary>
		public const uint SourceAxisDistance = 0x300A00B4;
		
		/// <summary>(300A,00B6) VR=SQ Beam Limiting Device Sequence 
		/// </summary>
		public const uint BeamLimitingDeviceSeq = 0x300A00B6;
		
		/// <summary>(300A,00B8) VR=CS RT Beam Limiting Device Type 
		/// </summary>
		public const uint RTBeamLimitingDeviceType = 0x300A00B8;
		
		/// <summary>(300A,00BA) VR=DS Source to Beam Limiting Device Distance 
		/// </summary>
		public const uint SourceToBeamLimitingDeviceDistance = 0x300A00BA;
		
		/// <summary>(300A,00BC) VR=IS Number of Leaf/Jaw Pairs 
		/// </summary>
		public const uint NumberOfLeafJawPairs = 0x300A00BC;
		
		/// <summary>(300A,00BE) VR=DS Leaf Position Boundaries 
		/// </summary>
		public const uint LeafPositionBoundaries = 0x300A00BE;
		
		/// <summary>(300A,00C0) VR=IS Beam Number 
		/// </summary>
		public const uint BeamNumber = 0x300A00C0;
		
		/// <summary>(300A,00C2) VR=LO Beam Name 
		/// </summary>
		public const uint BeamName = 0x300A00C2;
		
		/// <summary>(300A,00C3) VR=ST Beam Description 
		/// </summary>
		public const uint BeamDescription = 0x300A00C3;
		
		/// <summary>(300A,00C4) VR=CS Beam Type 
		/// </summary>
		public const uint BeamType = 0x300A00C4;
		
		/// <summary>(300A,00C6) VR=CS Radiation Type 
		/// </summary>
		public const uint RadiationType = 0x300A00C6;
		
		/// <summary>(300A,00C8) VR=IS Reference Image Number 
		/// </summary>
		public const uint ReferenceImageNumber = 0x300A00C8;
		
		/// <summary>(300A,00CA) VR=SQ Planned Verification Image Sequence 
		/// </summary>
		public const uint PlannedVerificationImageSeq = 0x300A00CA;
		
		/// <summary>(300A,00CC) VR=LO Imaging Device-Specific Acquisition Parameters 
		/// </summary>
		public const uint ImagingDeviceSpecificAcquisitionParameters = 0x300A00CC;
		
		/// <summary>(300A,00CE) VR=CS Treatment Delivery Type 
		/// </summary>
		public const uint TreatmentDeliveryType = 0x300A00CE;
		
		/// <summary>(300A,00D0) VR=IS Number of Wedges 
		/// </summary>
		public const uint NumberOfWedges = 0x300A00D0;
		
		/// <summary>(300A,00D1) VR=SQ Wedge Sequence 
		/// </summary>
		public const uint WedgeSeq = 0x300A00D1;
		
		/// <summary>(300A,00D2) VR=IS Wedge Number 
		/// </summary>
		public const uint WedgeNumber = 0x300A00D2;
		
		/// <summary>(300A,00D3) VR=CS Wedge Type 
		/// </summary>
		public const uint WedgeType = 0x300A00D3;
		
		/// <summary>(300A,00D4) VR=SH Wedge ID 
		/// </summary>
		public const uint WedgeID = 0x300A00D4;
		
		/// <summary>(300A,00D5) VR=IS Wedge Angle 
		/// </summary>
		public const uint WedgeAngle = 0x300A00D5;
		
		/// <summary>(300A,00D6) VR=DS Wedge Factor 
		/// </summary>
		public const uint WedgeFactor = 0x300A00D6;
		
		/// <summary>(300A,00D8) VR=DS Wedge Orientation 
		/// </summary>
		public const uint WedgeOrientation = 0x300A00D8;
		
		/// <summary>(300A,00DA) VR=DS Source to Wedge Tray Distance 
		/// </summary>
		public const uint SourceToWedgeTrayDistance = 0x300A00DA;
		
		/// <summary>(300A,00E0) VR=IS Number of Compensators 
		/// </summary>
		public const uint NumberOfCompensators = 0x300A00E0;
		
		/// <summary>(300A,00E1) VR=SH Material ID 
		/// </summary>
		public const uint MaterialID = 0x300A00E1;
		
		/// <summary>(300A,00E2) VR=DS Total Compensator Tray Factor 
		/// </summary>
		public const uint TotalCompensatorTrayFactor = 0x300A00E2;
		
		/// <summary>(300A,00E3) VR=SQ Compensator Sequence 
		/// </summary>
		public const uint CompensatorSeq = 0x300A00E3;
		
		/// <summary>(300A,00E4) VR=IS Compensator Number 
		/// </summary>
		public const uint CompensatorNumber = 0x300A00E4;
		
		/// <summary>(300A,00E5) VR=SH Compensator ID 
		/// </summary>
		public const uint CompensatorID = 0x300A00E5;
		
		/// <summary>(300A,00E6) VR=DS Source to Compensator Tray Distance 
		/// </summary>
		public const uint SourceToCompensatorTrayDistance = 0x300A00E6;
		
		/// <summary>(300A,00E7) VR=IS Compensator Rows 
		/// </summary>
		public const uint CompensatorRows = 0x300A00E7;
		
		/// <summary>(300A,00E8) VR=IS Compensator Columns 
		/// </summary>
		public const uint CompensatorColumns = 0x300A00E8;
		
		/// <summary>(300A,00E9) VR=DS Compensator Pixel Spacing 
		/// </summary>
		public const uint CompensatorPixelSpacing = 0x300A00E9;
		
		/// <summary>(300A,00EA) VR=DS Compensator Position 
		/// </summary>
		public const uint CompensatorPosition = 0x300A00EA;
		
		/// <summary>(300A,00EB) VR=DS Compensator Transmission Data 
		/// </summary>
		public const uint CompensatorTransmissionData = 0x300A00EB;
		
		/// <summary>(300A,00EC) VR=DS Compensator Thickness Data 
		/// </summary>
		public const uint CompensatorThicknessData = 0x300A00EC;
		
		/// <summary>(300A,00ED) VR=IS Number of Boli 
		/// </summary>
		public const uint NumberOfBoli = 0x300A00ED;
		
		/// <summary>(300A,00EE) VR=CS Compensator Type 
		/// </summary>
		public const uint CompensatorType = 0x300A00EE;
		
		/// <summary>(300A,00F0) VR=IS Number of Blocks 
		/// </summary>
		public const uint NumberOfBlocks = 0x300A00F0;
		
		/// <summary>(300A,00F2) VR=DS Total Block Tray Factor 
		/// </summary>
		public const uint TotalBlockTrayFactor = 0x300A00F2;
		
		/// <summary>(300A,00F4) VR=SQ Block Sequence 
		/// </summary>
		public const uint BlockSeq = 0x300A00F4;
		
		/// <summary>(300A,00F5) VR=SH Block Tray ID 
		/// </summary>
		public const uint BlockTrayID = 0x300A00F5;
		
		/// <summary>(300A,00F6) VR=DS Source to Block Tray Distance 
		/// </summary>
		public const uint SourceToBlockTrayDistance = 0x300A00F6;
		
		/// <summary>(300A,00F8) VR=CS Block Type 
		/// </summary>
		public const uint BlockType = 0x300A00F8;
		
		/// <summary>(300A,00FA) VR=CS Block Divergence 
		/// </summary>
		public const uint BlockDivergence = 0x300A00FA;
		
		/// <summary>(300A,00FC) VR=IS Block Number 
		/// </summary>
		public const uint BlockNumber = 0x300A00FC;
		
		/// <summary>(300A,00FE) VR=LO Block Name 
		/// </summary>
		public const uint BlockName = 0x300A00FE;
		
		/// <summary>(300A,0100) VR=DS Block Thickness 
		/// </summary>
		public const uint BlockThickness = 0x300A0100;
		
		/// <summary>(300A,0102) VR=DS Block Transmission 
		/// </summary>
		public const uint BlockTransmission = 0x300A0102;
		
		/// <summary>(300A,0104) VR=IS Block Number of Points 
		/// </summary>
		public const uint BlockNumberOfPoints = 0x300A0104;
		
		/// <summary>(300A,0106) VR=DS Block Data 
		/// </summary>
		public const uint BlockData = 0x300A0106;
		
		/// <summary>(300A,0107) VR=SQ Applicator Sequence 
		/// </summary>
		public const uint ApplicatorSeq = 0x300A0107;
		
		/// <summary>(300A,0108) VR=SH Applicator ID 
		/// </summary>
		public const uint ApplicatorID = 0x300A0108;
		
		/// <summary>(300A,0109) VR=CS Applicator Type 
		/// </summary>
		public const uint ApplicatorType = 0x300A0109;
		
		/// <summary>(300A,010A) VR=LO Applicator Description 
		/// </summary>
		public const uint ApplicatorDescription = 0x300A010A;
		
		/// <summary>(300A,010C) VR=DS Cumulative Dose Reference Coefficient 
		/// </summary>
		public const uint CumulativeDoseReferenceCoefficient = 0x300A010C;
		
		/// <summary>(300A,010E) VR=DS Final Cumulative Meterset Weight 
		/// </summary>
		public const uint FinalCumulativeMetersetWeight = 0x300A010E;
		
		/// <summary>(300A,0110) VR=IS Number of Control Points 
		/// </summary>
		public const uint NumberOfControlPoints = 0x300A0110;
		
		/// <summary>(300A,0111) VR=SQ Control Point Sequence 
		/// </summary>
		public const uint ControlPointSeq = 0x300A0111;
		
		/// <summary>(300A,0112) VR=IS Control Point Index 
		/// </summary>
		public const uint ControlPointIndex = 0x300A0112;
		
		/// <summary>(300A,0114) VR=DS Nominal Beam Energy 
		/// </summary>
		public const uint NominalBeamEnergy = 0x300A0114;
		
		/// <summary>(300A,0115) VR=DS Dose Rate Set 
		/// </summary>
		public const uint DoseRateSet = 0x300A0115;
		
		/// <summary>(300A,0116) VR=SQ Wedge Position Sequence 
		/// </summary>
		public const uint WedgePositionSeq = 0x300A0116;
		
		/// <summary>(300A,0118) VR=CS Wedge Position 
		/// </summary>
		public const uint WedgePosition = 0x300A0118;
		
		/// <summary>(300A,011A) VR=SQ Beam Limiting Device Position Sequence 
		/// </summary>
		public const uint BeamLimitingDevicePositionSeq = 0x300A011A;
		
		/// <summary>(300A,011C) VR=DS Leaf/Jaw Positions 
		/// </summary>
		public const uint LeafJawPositions = 0x300A011C;
		
		/// <summary>(300A,011E) VR=DS Gantry Angle 
		/// </summary>
		public const uint GantryAngle = 0x300A011E;
		
		/// <summary>(300A,011F) VR=CS Gantry Rotation Direction 
		/// </summary>
		public const uint GantryRotationDirection = 0x300A011F;
		
		/// <summary>(300A,0120) VR=DS Beam Limiting Device Angle 
		/// </summary>
		public const uint BeamLimitingDeviceAngle = 0x300A0120;
		
		/// <summary>(300A,0121) VR=CS Beam Limiting Device Rotation Direction 
		/// </summary>
		public const uint BeamLimitingDeviceRotationDirection = 0x300A0121;
		
		/// <summary>(300A,0122) VR=DS Patient Support Angle 
		/// </summary>
		public const uint PatientSupportAngle = 0x300A0122;
		
		/// <summary>(300A,0123) VR=CS Patient Support Rotation Direction 
		/// </summary>
		public const uint PatientSupportRotationDirection = 0x300A0123;
		
		/// <summary>(300A,0124) VR=DS Table Top Eccentric Axis Distance 
		/// </summary>
		public const uint TableTopEccentricAxisDistance = 0x300A0124;
		
		/// <summary>(300A,0125) VR=DS Table Top Eccentric Angle 
		/// </summary>
		public const uint TableTopEccentricAngle = 0x300A0125;
		
		/// <summary>(300A,0126) VR=CS Table Top Eccentric Rotation Direction 
		/// </summary>
		public const uint TableTopEccentricRotationDirection = 0x300A0126;
		
		/// <summary>(300A,0128) VR=DS Table Top Vertical Position 
		/// </summary>
		public const uint TableTopVerticalPosition = 0x300A0128;
		
		/// <summary>(300A,0129) VR=DS Table Top Longitudinal Position 
		/// </summary>
		public const uint TableTopLongitudinalPosition = 0x300A0129;
		
		/// <summary>(300A,012A) VR=DS Table Top Lateral Position 
		/// </summary>
		public const uint TableTopLateralPosition = 0x300A012A;
		
		/// <summary>(300A,012C) VR=DS Isocenter Position 
		/// </summary>
		public const uint IsocenterPosition = 0x300A012C;
		
		/// <summary>(300A,012E) VR=DS Surface Entry Point 
		/// </summary>
		public const uint SurfaceEntryPoint = 0x300A012E;
		
		/// <summary>(300A,0130) VR=DS Source to Surface Distance 
		/// </summary>
		public const uint SourceToSurfaceDistance = 0x300A0130;
		
		/// <summary>(300A,0134) VR=DS Cumulative Meterset Weight 
		/// </summary>
		public const uint CumulativeMetersetWeight = 0x300A0134;
		
		/// <summary>(300A,0180) VR=SQ Patient Setup Sequence 
		/// </summary>
		public const uint PatientSetupSeq = 0x300A0180;
		
		/// <summary>(300A,0182) VR=IS Patient Setup Number 
		/// </summary>
		public const uint PatientSetupNumber = 0x300A0182;
		
		/// <summary>(300A,0184) VR=LO Patient Additional Position 
		/// </summary>
		public const uint PatientAdditionalPosition = 0x300A0184;
		
		/// <summary>(300A,0190) VR=SQ Fixation Device Sequence 
		/// </summary>
		public const uint FixationDeviceSeq = 0x300A0190;
		
		/// <summary>(300A,0192) VR=CS Fixation Device Type 
		/// </summary>
		public const uint FixationDeviceType = 0x300A0192;
		
		/// <summary>(300A,0194) VR=SH Fixation Device Label 
		/// </summary>
		public const uint FixationDeviceLabel = 0x300A0194;
		
		/// <summary>(300A,0196) VR=ST Fixation Device Description 
		/// </summary>
		public const uint FixationDeviceDescription = 0x300A0196;
		
		/// <summary>(300A,0198) VR=SH Fixation Device Position 
		/// </summary>
		public const uint FixationDevicePosition = 0x300A0198;
		
		/// <summary>(300A,01A0) VR=SQ Shielding Device Sequence 
		/// </summary>
		public const uint ShieldingDeviceSeq = 0x300A01A0;
		
		/// <summary>(300A,01A2) VR=CS Shielding Device Type 
		/// </summary>
		public const uint ShieldingDeviceType = 0x300A01A2;
		
		/// <summary>(300A,01A4) VR=SH Shielding Device Label 
		/// </summary>
		public const uint ShieldingDeviceLabel = 0x300A01A4;
		
		/// <summary>(300A,01A6) VR=ST Shielding Device Description 
		/// </summary>
		public const uint ShieldingDeviceDescription = 0x300A01A6;
		
		/// <summary>(300A,01A8) VR=SH Shielding Device Position 
		/// </summary>
		public const uint ShieldingDevicePosition = 0x300A01A8;
		
		/// <summary>(300A,01B0) VR=CS Setup Technique 
		/// </summary>
		public const uint SetupTechnique = 0x300A01B0;
		
		/// <summary>(300A,01B2) VR=ST Setup Technique Description 
		/// </summary>
		public const uint SetupTechniqueDescription = 0x300A01B2;
		
		/// <summary>(300A,01B4) VR=SQ Setup Device Sequence 
		/// </summary>
		public const uint SetupDeviceSeq = 0x300A01B4;
		
		/// <summary>(300A,01B6) VR=CS Setup Device Type 
		/// </summary>
		public const uint SetupDeviceType = 0x300A01B6;
		
		/// <summary>(300A,01B8) VR=SH Setup Device Label 
		/// </summary>
		public const uint SetupDeviceLabel = 0x300A01B8;
		
		/// <summary>(300A,01BA) VR=ST Setup Device Description 
		/// </summary>
		public const uint SetupDeviceDescription = 0x300A01BA;
		
		/// <summary>(300A,01BC) VR=DS Setup Device Parameter 
		/// </summary>
		public const uint SetupDeviceParameter = 0x300A01BC;
		
		/// <summary>(300A,01D0) VR=ST Setup Reference Description 
		/// </summary>
		public const uint SetupReferenceDescription = 0x300A01D0;
		
		/// <summary>(300A,01D2) VR=DS Table Top Vertical Setup Displacement 
		/// </summary>
		public const uint TableTopVerticalSetupDisplacement = 0x300A01D2;
		
		/// <summary>(300A,01D4) VR=DS Table Top Longitudinal Setup Displacement 
		/// </summary>
		public const uint TableTopLongitudinalSetupDisplacement = 0x300A01D4;
		
		/// <summary>(300A,01D6) VR=DS Table Top Lateral Setup Displacement 
		/// </summary>
		public const uint TableTopLateralSetupDisplacement = 0x300A01D6;
		
		/// <summary>(300A,0200) VR=CS Brachy Treatment Technique 
		/// </summary>
		public const uint BrachyTreatmentTechnique = 0x300A0200;
		
		/// <summary>(300A,0202) VR=CS Brachy Treatment Type 
		/// </summary>
		public const uint BrachyTreatmentType = 0x300A0202;
		
		/// <summary>(300A,0206) VR=SQ Treatment Machine Sequence 
		/// </summary>
		public const uint TreatmentMachineSeq = 0x300A0206;
		
		/// <summary>(300A,0210) VR=SQ Source Sequence 
		/// </summary>
		public const uint SourceSeq = 0x300A0210;
		
		/// <summary>(300A,0212) VR=IS Source Number 
		/// </summary>
		public const uint SourceNumber = 0x300A0212;
		
		/// <summary>(300A,0214) VR=CS Source Type 
		/// </summary>
		public const uint SourceType = 0x300A0214;
		
		/// <summary>(300A,0216) VR=LO Source Manufacturer 
		/// </summary>
		public const uint SourceManufacturer = 0x300A0216;
		
		/// <summary>(300A,0218) VR=DS Active Source Diameter 
		/// </summary>
		public const uint ActiveSourceDiameter = 0x300A0218;
		
		/// <summary>(300A,021A) VR=DS Active Source Length 
		/// </summary>
		public const uint ActiveSourceLength = 0x300A021A;
		
		/// <summary>(300A,0222) VR=DS Source Encapsulation Nominal Thickness 
		/// </summary>
		public const uint SourceEncapsulationNominalThickness = 0x300A0222;
		
		/// <summary>(300A,0224) VR=DS Source Encapsulation Nominal Transmission 
		/// </summary>
		public const uint SourceEncapsulationNominalTransmission = 0x300A0224;
		
		/// <summary>(300A,0226) VR=LO Source Isotope Name 
		/// </summary>
		public const uint SourceIsotopeName = 0x300A0226;
		
		/// <summary>(300A,0228) VR=DS Source Isotope Half Life 
		/// </summary>
		public const uint SourceIsotopeHalfLife = 0x300A0228;
		
		/// <summary>(300A,022A) VR=DS Reference Air Kerma Rate 
		/// </summary>
		public const uint ReferenceAirKermaRate = 0x300A022A;
		
		/// <summary>(300A,022C) VR=DA Air Kerma Rate Reference Date 
		/// </summary>
		public const uint AirKermaRateReferenceDate = 0x300A022C;
		
		/// <summary>(300A,022E) VR=TM Air Kerma Rate Reference Time 
		/// </summary>
		public const uint AirKermaRateReferenceTime = 0x300A022E;
		
		/// <summary>(300A,0230) VR=SQ Application Setup Sequence 
		/// </summary>
		public const uint ApplicationSetupSeq = 0x300A0230;
		
		/// <summary>(300A,0232) VR=CS Application Setup Type 
		/// </summary>
		public const uint ApplicationSetupType = 0x300A0232;
		
		/// <summary>(300A,0234) VR=IS Application Setup Number 
		/// </summary>
		public const uint ApplicationSetupNumber = 0x300A0234;
		
		/// <summary>(300A,0236) VR=LO Application Setup Name 
		/// </summary>
		public const uint ApplicationSetupName = 0x300A0236;
		
		/// <summary>(300A,0238) VR=LO Application Setup Manufacturer 
		/// </summary>
		public const uint ApplicationSetupManufacturer = 0x300A0238;
		
		/// <summary>(300A,0240) VR=IS Template Number 
		/// </summary>
		public const uint TemplateNumber = 0x300A0240;
		
		/// <summary>(300A,0242) VR=SH Template Type 
		/// </summary>
		public const uint TemplateType = 0x300A0242;
		
		/// <summary>(300A,0244) VR=LO Template Name 
		/// </summary>
		public const uint TemplateName = 0x300A0244;
		
		/// <summary>(300A,0250) VR=DS Total Reference Air Kerma 
		/// </summary>
		public const uint TotalReferenceAirKerma = 0x300A0250;
		
		/// <summary>(300A,0260) VR=SQ Brachy Accessory Device Sequence 
		/// </summary>
		public const uint BrachyAccessoryDeviceSeq = 0x300A0260;
		
		/// <summary>(300A,0262) VR=IS Brachy Accessory Device Number 
		/// </summary>
		public const uint BrachyAccessoryDeviceNumber = 0x300A0262;
		
		/// <summary>(300A,0263) VR=SH Brachy Accessory Device ID 
		/// </summary>
		public const uint BrachyAccessoryDeviceID = 0x300A0263;
		
		/// <summary>(300A,0264) VR=CS Brachy Accessory Device Type 
		/// </summary>
		public const uint BrachyAccessoryDeviceType = 0x300A0264;
		
		/// <summary>(300A,0266) VR=LO Brachy Accessory Device Name 
		/// </summary>
		public const uint BrachyAccessoryDeviceName = 0x300A0266;
		
		/// <summary>(300A,026A) VR=DS Brachy Accessory Device Nominal Thickness 
		/// </summary>
		public const uint BrachyAccessoryDeviceNominalThickness = 0x300A026A;
		
		/// <summary>(300A,026C) VR=DS Brachy Accessory Device Nominal Transmission 
		/// </summary>
		public const uint BrachyAccessoryDeviceNominalTransmission = 0x300A026C;
		
		/// <summary>(300A,0280) VR=SQ Channel Sequence 
		/// </summary>
		public const uint ChannelSeq = 0x300A0280;
		
		/// <summary>(300A,0282) VR=IS Channel Number 
		/// </summary>
		public const uint ChannelNumber = 0x300A0282;
		
		/// <summary>(300A,0284) VR=DS Channel Length 
		/// </summary>
		public const uint ChannelLength = 0x300A0284;
		
		/// <summary>(300A,0286) VR=DS Channel Total Time 
		/// </summary>
		public const uint ChannelTotalTime = 0x300A0286;
		
		/// <summary>(300A,0288) VR=CS Source Movement Type 
		/// </summary>
		public const uint SourceMovementType = 0x300A0288;
		
		/// <summary>(300A,028A) VR=IS Number of Pulses 
		/// </summary>
		public const uint NumberOfPulses = 0x300A028A;
		
		/// <summary>(300A,028C) VR=DS Pulse Repetition Interval 
		/// </summary>
		public const uint PulseRepetitionInterval = 0x300A028C;
		
		/// <summary>(300A,0290) VR=IS Source Applicator Number 
		/// </summary>
		public const uint SourceApplicatorNumber = 0x300A0290;
		
		/// <summary>(300A,0291) VR=SH Source Applicator ID 
		/// </summary>
		public const uint SourceApplicatorID = 0x300A0291;
		
		/// <summary>(300A,0292) VR=CS Source Applicator Type 
		/// </summary>
		public const uint SourceApplicatorType = 0x300A0292;
		
		/// <summary>(300A,0294) VR=LO Source Applicator Name 
		/// </summary>
		public const uint SourceApplicatorName = 0x300A0294;
		
		/// <summary>(300A,0296) VR=DS Source Applicator Length 
		/// </summary>
		public const uint SourceApplicatorLength = 0x300A0296;
		
		/// <summary>(300A,0298) VR=LO Source Applicator Manufacturer 
		/// </summary>
		public const uint SourceApplicatorManufacturer = 0x300A0298;
		
		/// <summary>(300A,029C) VR=DS Source Applicator Wall Nominal Thickness 
		/// </summary>
		public const uint SourceApplicatorWallNominalThickness = 0x300A029C;
		
		/// <summary>(300A,029E) VR=DS Source Applicator Wall Nominal Transmission 
		/// </summary>
		public const uint SourceApplicatorWallNominalTransmission = 0x300A029E;
		
		/// <summary>(300A,02A0) VR=DS Source Applicator Step Size 
		/// </summary>
		public const uint SourceApplicatorStepSize = 0x300A02A0;
		
		/// <summary>(300A,02A2) VR=IS Transfer Tube Number 
		/// </summary>
		public const uint TransferTubeNumber = 0x300A02A2;
		
		/// <summary>(300A,02A4) VR=DS Transfer Tube Length 
		/// </summary>
		public const uint TransferTubeLength = 0x300A02A4;
		
		/// <summary>(300A,02B0) VR=SQ Channel Shield Sequence 
		/// </summary>
		public const uint ChannelShieldSeq = 0x300A02B0;
		
		/// <summary>(300A,02B2) VR=IS Channel Shield Number 
		/// </summary>
		public const uint ChannelShieldNumber = 0x300A02B2;
		
		/// <summary>(300A,02B3) VR=SH Channel Shield ID 
		/// </summary>
		public const uint ChannelShieldID = 0x300A02B3;
		
		/// <summary>(300A,02B4) VR=LO Channel Shield Name 
		/// </summary>
		public const uint ChannelShieldName = 0x300A02B4;
		
		/// <summary>(300A,02B8) VR=DS Channel Shield Nominal Thickness 
		/// </summary>
		public const uint ChannelShieldNominalThickness = 0x300A02B8;
		
		/// <summary>(300A,02BA) VR=DS Channel Shield Nominal Transmission 
		/// </summary>
		public const uint ChannelShieldNominalTransmission = 0x300A02BA;
		
		/// <summary>(300A,02C8) VR=DS Final Cumulative Time Weight 
		/// </summary>
		public const uint FinalCumulativeTimeWeight = 0x300A02C8;
		
		/// <summary>(300A,02D0) VR=SQ Brachy Control Point Sequence 
		/// </summary>
		public const uint BrachyControlPointSeq = 0x300A02D0;
		
		/// <summary>(300A,02D2) VR=DS Control Point Relative Position 
		/// </summary>
		public const uint ControlPointRelativePosition = 0x300A02D2;
		
		/// <summary>(300A,02D4) VR=DS Control Point 3D Position 
		/// </summary>
		public const uint ControlPoint3DPosition = 0x300A02D4;
		
		/// <summary>(300A,02D6) VR=DS Cumulative Time Weight 
		/// </summary>
		public const uint CumulativeTimeWeight = 0x300A02D6;
		
		/// <summary>(300C,0002) VR=SQ Referenced RT Plan Sequence 
		/// </summary>
		public const uint RefRTPlanSeq = 0x300C0002;
		
		/// <summary>(300C,0004) VR=SQ Referenced Beam Sequence 
		/// </summary>
		public const uint RefBeamSeq = 0x300C0004;
		
		/// <summary>(300C,0006) VR=IS Referenced Beam Number 
		/// </summary>
		public const uint RefBeamNumber = 0x300C0006;
		
		/// <summary>(300C,0007) VR=IS Referenced Reference Image Number 
		/// </summary>
		public const uint RefReferenceImageNumber = 0x300C0007;
		
		/// <summary>(300C,0008) VR=DS Start Cumulative Meterset Weight 
		/// </summary>
		public const uint StartCumulativeMetersetWeight = 0x300C0008;
		
		/// <summary>(300C,0009) VR=DS End Cumulative Meterset Weight 
		/// </summary>
		public const uint EndCumulativeMetersetWeight = 0x300C0009;
		
		/// <summary>(300C,000A) VR=SQ Referenced Brachy Application Setup Sequence 
		/// </summary>
		public const uint RefBrachyApplicationSetupSeq = 0x300C000A;
		
		/// <summary>(300C,000C) VR=IS Referenced Brachy Application Setup Number 
		/// </summary>
		public const uint RefBrachyApplicationSetupNumber = 0x300C000C;
		
		/// <summary>(300C,000E) VR=IS Referenced Source Number 
		/// </summary>
		public const uint RefSourceNumber = 0x300C000E;
		
		/// <summary>(300C,0020) VR=SQ Referenced Fraction Group Sequence 
		/// </summary>
		public const uint RefFractionGroupSeq = 0x300C0020;
		
		/// <summary>(300C,0022) VR=IS Referenced Fraction Group Number 
		/// </summary>
		public const uint RefFractionGroupNumber = 0x300C0022;
		
		/// <summary>(300C,0040) VR=SQ Referenced Verification Image Sequence 
		/// </summary>
		public const uint RefVerificationImageSeq = 0x300C0040;
		
		/// <summary>(300C,0042) VR=SQ Referenced Reference Image Sequence 
		/// </summary>
		public const uint RefReferenceImageSeq = 0x300C0042;
		
		/// <summary>(300C,0050) VR=SQ Referenced Dose Reference Sequence 
		/// </summary>
		public const uint RefDoseReferenceSeq = 0x300C0050;
		
		/// <summary>(300C,0051) VR=IS Referenced Dose Reference Number 
		/// </summary>
		public const uint RefDoseReferenceNumber = 0x300C0051;
		
		/// <summary>(300C,0055) VR=SQ Brachy Referenced Dose Reference Sequence 
		/// </summary>
		public const uint BrachyRefDoseReferenceSeq = 0x300C0055;
		
		/// <summary>(300C,0060) VR=SQ Referenced Structure Set Sequence 
		/// </summary>
		public const uint RefStructureSetSeq = 0x300C0060;
		
		/// <summary>(300C,006A) VR=IS Referenced Patient Setup Number 
		/// </summary>
		public const uint RefPatientSetupNumber = 0x300C006A;
		
		/// <summary>(300C,0080) VR=SQ Referenced Dose Sequence 
		/// </summary>
		public const uint RefDoseSeq = 0x300C0080;
		
		/// <summary>(300C,00A0) VR=IS Referenced Tolerance Table Number 
		/// </summary>
		public const uint RefToleranceTableNumber = 0x300C00A0;
		
		/// <summary>(300C,00B0) VR=SQ Referenced Bolus Sequence 
		/// </summary>
		public const uint RefBolusSeq = 0x300C00B0;
		
		/// <summary>(300C,00C0) VR=IS Referenced Wedge Number 
		/// </summary>
		public const uint RefWedgeNumber = 0x300C00C0;
		
		/// <summary>(300C,00D0) VR=IS Referenced Compensator Number 
		/// </summary>
		public const uint RefCompensatorNumber = 0x300C00D0;
		
		/// <summary>(300C,00E0) VR=IS Referenced Block Number 
		/// </summary>
		public const uint RefBlockNumber = 0x300C00E0;
		
		/// <summary>(300C,00F0) VR=IS Referenced Control Point Index 
		/// </summary>
		public const uint RefControlPointIndex = 0x300C00F0;
		
		/// <summary>(300E,0002) VR=CS Approval Status 
		/// </summary>
		public const uint ApprovalStatus = 0x300E0002;
		
		/// <summary>(300E,0004) VR=DA Review Date 
		/// </summary>
		public const uint ReviewDate = 0x300E0004;
		
		/// <summary>(300E,0005) VR=TM Review Time 
		/// </summary>
		public const uint ReviewTime = 0x300E0005;
		
		/// <summary>(300E,0008) VR=PN Reviewer Name 
		/// </summary>
		public const uint ReviewerName = 0x300E0008;
		
		/// <summary>(4000,0010) VR=LT Arbitrary (Retired) 
		/// </summary>
		public const uint ArbitraryRetired = 0x40000010;
		
		/// <summary>(4000,4000) VR=LT (Arbitrary) Comments (Retired) 
		/// </summary>
		public const uint ArbitraryCommentsRetired = 0x40004000;
		
		/// <summary>(4008,0040) VR=SH Results ID 
		/// </summary>
		public const uint ResultsID = 0x40080040;
		
		/// <summary>(4008,0042) VR=LO Results ID Issuer 
		/// </summary>
		public const uint ResultsIDIssuer = 0x40080042;
		
		/// <summary>(4008,0050) VR=SQ Referenced Interpretation Sequence 
		/// </summary>
		public const uint RefInterpretationSeq = 0x40080050;
		
		/// <summary>(4008,0100) VR=DA Interpretation Recorded Date 
		/// </summary>
		public const uint InterpretationRecordedDate = 0x40080100;
		
		/// <summary>(4008,0101) VR=TM Interpretation Recorded Time 
		/// </summary>
		public const uint InterpretationRecordedTime = 0x40080101;
		
		/// <summary>(4008,0102) VR=PN Interpretation Recorder 
		/// </summary>
		public const uint InterpretationRecorder = 0x40080102;
		
		/// <summary>(4008,0103) VR=LO Reference to Recorded Sound 
		/// </summary>
		public const uint ReferenceToRecordedSound = 0x40080103;
		
		/// <summary>(4008,0108) VR=DA Interpretation Transcription Date 
		/// </summary>
		public const uint InterpretationTranscriptionDate = 0x40080108;
		
		/// <summary>(4008,0109) VR=TM Interpretation Transcription Time 
		/// </summary>
		public const uint InterpretationTranscriptionTime = 0x40080109;
		
		/// <summary>(4008,010A) VR=PN Interpretation Transcriber 
		/// </summary>
		public const uint InterpretationTranscriber = 0x4008010A;
		
		/// <summary>(4008,010B) VR=ST Interpretation Text 
		/// </summary>
		public const uint InterpretationText = 0x4008010B;
		
		/// <summary>(4008,010C) VR=PN Interpretation Author 
		/// </summary>
		public const uint InterpretationAuthor = 0x4008010C;
		
		/// <summary>(4008,0111) VR=SQ Interpretation Approver Sequence 
		/// </summary>
		public const uint InterpretationApproverSeq = 0x40080111;
		
		/// <summary>(4008,0112) VR=DA Interpretation Approval Date 
		/// </summary>
		public const uint InterpretationApprovalDate = 0x40080112;
		
		/// <summary>(4008,0113) VR=TM Interpretation Approval Time 
		/// </summary>
		public const uint InterpretationApprovalTime = 0x40080113;
		
		/// <summary>(4008,0114) VR=PN Physician Approving Interpretation 
		/// </summary>
		public const uint PhysicianApprovingInterpretation = 0x40080114;
		
		/// <summary>(4008,0115) VR=LT Interpretation Diagnosis Description 
		/// </summary>
		public const uint InterpretationDiagnosisDescription = 0x40080115;
		
		/// <summary>(4008,0117) VR=SQ Interpretation Diagnosis Code Sequence 
		/// </summary>
		public const uint InterpretationDiagnosisCodeSeq = 0x40080117;
		
		/// <summary>(4008,0118) VR=SQ Results Distribution List Sequence 
		/// </summary>
		public const uint ResultsDistributionListSeq = 0x40080118;
		
		/// <summary>(4008,0119) VR=PN Distribution Name 
		/// </summary>
		public const uint DistributionName = 0x40080119;
		
		/// <summary>(4008,011A) VR=LO Distribution Address 
		/// </summary>
		public const uint DistributionAddress = 0x4008011A;
		
		/// <summary>(4008,0200) VR=SH Interpretation ID 
		/// </summary>
		public const uint InterpretationID = 0x40080200;
		
		/// <summary>(4008,0202) VR=LO Interpretation ID Issuer 
		/// </summary>
		public const uint InterpretationIDIssuer = 0x40080202;
		
		/// <summary>(4008,0210) VR=CS Interpretation Type ID 
		/// </summary>
		public const uint InterpretationTypeID = 0x40080210;
		
		/// <summary>(4008,0212) VR=CS Interpretation Status ID 
		/// </summary>
		public const uint InterpretationStatusID = 0x40080212;
		
		/// <summary>(4008,0300) VR=ST Impressions 
		/// </summary>
		public const uint Impressions = 0x40080300;
		
		/// <summary>(4008,4000) VR=ST Results Comments 
		/// </summary>
		public const uint ResultsComments = 0x40084000;

        /// <summary>(4FFE,0001) MAC Parameters Sequence	SQ 
        /// </summary>
        public const uint MACParametersSeq = 0x4FFE0001;

		/// <summary>(50xx,0005) VR=US Curve Dimensions 
		/// </summary>
		public const uint CurveDimensions = 0x50000005;
		
		/// <summary>(50xx,0010) VR=US Number of Points 
		/// </summary>
		public const uint NumberOfPoints = 0x50000010;
		
		/// <summary>(50xx,0020) VR=CS Type of Data 
		/// </summary>
		public const uint TypeOfData = 0x50000020;
		
		/// <summary>(50xx,0022) VR=LO Curve Description 
		/// </summary>
		public const uint CurveDescription = 0x50000022;
		
		/// <summary>(50xx,0030) VR=SH Axis Units 
		/// </summary>
		public const uint AxisUnits = 0x50000030;
		
		/// <summary>(50xx,0040) VR=SH Axis Labels 
		/// </summary>
		public const uint AxisLabels = 0x50000040;
		
		/// <summary>(50xx,0103) VR=US Data Value Representation 
		/// </summary>
		public const uint DataValueRepresentation = 0x50000103;
		
		/// <summary>(50xx,0104) VR=US Minimum Coordinate Value 
		/// </summary>
		public const uint MinimumCoordinateValue = 0x50000104;
		
		/// <summary>(50xx,0105) VR=US Maximum Coordinate Value 
		/// </summary>
		public const uint MaximumCoordinateValue = 0x50000105;
		
		/// <summary>(50xx,0106) VR=SH Curve Range 
		/// </summary>
		public const uint CurveRange = 0x50000106;
		
		/// <summary>(50xx,0110) VR=US Curve Data Descriptor 
		/// </summary>
		public const uint CurveDataDescriptor = 0x50000110;
		
		/// <summary>(50xx,0112) VR=US Coordinate Start Value 
		/// </summary>
		public const uint CoordinateStartValue = 0x50000112;
		
		/// <summary>(50xx,0114) VR=US Coordinate Step Value 
		/// </summary>
		public const uint CoordinateStepValue = 0x50000114;
		
		/// <summary>(50xx,1001) VR=CS Curve Activation Layer 
		/// </summary>
		public const uint CurveActivationLayer = 0x50001001;
		
		/// <summary>(50xx,2000) VR=US Audio Type 
		/// </summary>
		public const uint AudioType = 0x50002000;
		
		/// <summary>(50xx,2002) VR=US Audio Sample Format 
		/// </summary>
		public const uint AudioSampleFormat = 0x50002002;
		
		/// <summary>(50xx,2004) VR=US Number of Channels 
		/// </summary>
		public const uint NumberOfChannels = 0x50002004;
		
		/// <summary>(50xx,2006) VR=UL Number of Samples 
		/// </summary>
		public const uint NumberOfSamples = 0x50002006;
		
		/// <summary>(50xx,2008) VR=UL Sample Rate 
		/// </summary>
		public const uint SampleRate = 0x50002008;
		
		/// <summary>(50xx,200A) VR=UL Total Time 
		/// </summary>
		public const uint TotalTime = 0x5000200A;
		
		/// <summary>(50xx,200C) VR=OW,OB Audio Sample Data 
		/// </summary>
		public const uint AudioSampleData = 0x5000200C;
		
		/// <summary>(50xx,200E) VR=LT Audio Comments 
		/// </summary>
		public const uint AudioComments = 0x5000200E;
		
		/// <summary>(50xx,2500) VR=LO Curve Label 
		/// </summary>
		public const uint CurveLabel = 0x50002500;
		
		/// <summary>(50xx,2600) VR=SQ Referenced Overlay Sequence 
		/// </summary>
		public const uint RefOverlaySeqCurve = 0x50002600;
		
		/// <summary>(50xx,2610) VR=US Referenced Overlay Group 
		/// </summary>
		public const uint RefOverlayGroup = 0x50002610;
		
		/// <summary>(50xx,3000) VR=OB,OW Curve Data 
		/// </summary>
		public const uint CurveData = 0x50003000;
		
		/// <summary>(5200,9229) VR=SQ Shared Functional Groups Sequence 
		/// </summary>
		public const uint SharedFunctionalGroupsSeq = 0x52009229;
		
		/// <summary>(5200,9230) VR=SQ Per-frame Functional Groups Sequence 
		/// </summary>
		public const uint PerFrameFunctionalGroupsSeq = 0x52009230;
		
		/// <summary>(5400,0100) VR=SQ Waveform Sequence 
		/// </summary>
		public const uint WaveformSeq = 0x54000100;
		
		/// <summary>(5400,0110) VR=OW,OB Channel Minimum Value 
		/// </summary>
		public const uint ChannelMinimumValue = 0x54000110;
		
		/// <summary>(5400,0112) VR=OW,OB Channel Maximum Value 
		/// </summary>
		public const uint ChannelMaximumValue = 0x54000112;
		
		/// <summary>(5400,1004) VR=US Waveform Bits Allocated 
		/// </summary>
		public const uint WaveformBitsAllocated = 0x54001004;
		
		/// <summary>(5400,1006) VR=CS Waveform Sample Interpretation 
		/// </summary>
		public const uint WaveformSampleInterpretation = 0x54001006;
		
		/// <summary>(5400,100A) VR=OW,OB Waveform Padding Value 
		/// </summary>
		public const uint WaveformPaddingValue = 0x5400100A;
		
		/// <summary>(5400,1010) VR=OW,OB Waveform Data 
		/// </summary>
		public const uint WaveformData = 0x54001010;
				
		/// <summary>(60xx,0010) VR=US Overlay Rows 
		/// </summary>
		public const uint OverlayRows = 0x60000010;
		
		/// <summary>(60xx,0011) VR=US Overlay Columns 
		/// </summary>
		public const uint OverlayColumns = 0x60000011;
		
		/// <summary>(60xx,0012) VR=US Overlay Planes 
		/// </summary>
		public const uint OverlayPlanes = 0x60000012;
		
		/// <summary>(60xx,0015) VR=IS Number of Frames in Overlay 
		/// </summary>
		public const uint NumberOfFramesInOverlay = 0x60000015;
		
		/// <summary>(60xx,0022) VR=LO Overlay Description 
		/// </summary>
		public const uint OverlayDescription = 0x60000022;
		
		/// <summary>(60xx,0040) VR=CS Overlay Type 
		/// </summary>
		public const uint OverlayType = 0x60000040;
		
		/// <summary>(60xx,0045) VR=LO Overlay Subtype 
		/// </summary>
		public const uint OverlaySubtype = 0x60000045;
		
		/// <summary>(60xx,0050) VR=SS Overlay Origin 
		/// </summary>
		public const uint OverlayOrigin = 0x60000050;
		
		/// <summary>(60xx,0051) VR=US Image Frame Origin 
		/// </summary>
		public const uint ImageFrameOrigin = 0x60000051;
		
		/// <summary>(60xx,0052) VR=US Overlay Plane Origin 
		/// </summary>
		public const uint OverlayPlaneOrigin = 0x60000052;
		
		/// <summary>(60xx,0060) VR=CS (Overlay) Compression Code (Retired) 
		/// </summary>
		public const uint OverlayCompressionCodeRetired = 0x60000060;
		
		/// <summary>(60xx,0100) VR=US Overlay Bits Allocated 
		/// </summary>
		public const uint OverlayBitsAllocated = 0x60000100;
		
		/// <summary>(60xx,0102) VR=US Overlay Bit Position 
		/// </summary>
		public const uint OverlayBitPosition = 0x60000102;
		
		/// <summary>(60xx,0110) VR=CS Overlay Format (Retired) 
		/// </summary>
		public const uint OverlayFormatRetired = 0x60000110;
		
		/// <summary>(60xx,0200) VR=UN Overlay Location 
		/// </summary>
		public const uint OverlayLocationRetired = 0x60000200;
		
		/// <summary>(60xx,1001) VR=CS Overlay Activation Layer 
		/// </summary>
		public const uint OverlayActivationLayer = 0x60001001;
		
		/// <summary>(60xx,1100) VR=US Overlay Descriptor - Gray (Retired) 
		/// </summary>
		public const uint OverlayDescriptorGrayRetired = 0x60001100;
		
		/// <summary>(60xx,1101) VR=US Overlay Descriptor - Red (Retired) 
		/// </summary>
		public const uint OverlayDescriptorRedRetired = 0x60001101;
		
		/// <summary>(60xx,1102) VR=US Overlay Descriptor - Green (Retired) 
		/// </summary>
		public const uint OverlayDescriptorGreenRetired = 0x60001102;
		
		/// <summary>(60xx,1103) VR=US Overlay Descriptor - Blue (Retired) 
		/// </summary>
		public const uint OverlayDescriptorBlueRetired = 0x60001103;
		
		/// <summary>(60xx,1200) VR=US Overlays - Gray (Retired) 
		/// </summary>
		public const uint OverlaysGrayRetired = 0x60001200;
		
		/// <summary>(60xx,1201) VR=US Overlays - Red (Retired) 
		/// </summary>
		public const uint OverlaysRedRetired = 0x60001201;
		
		/// <summary>(60xx,1202) VR=US Overlays - Green (Retired) 
		/// </summary>
		public const uint OverlaysGreenRetired = 0x60001202;
		
		/// <summary>(60xx,1203) VR=US Overlays - Blue (Retired) 
		/// </summary>
		public const uint OverlaysBlueRetired = 0x60001203;
		
		/// <summary>(60xx,1301) VR=IS ROI Area 
		/// </summary>
		public const uint ROIArea = 0x60001301;
		
		/// <summary>(60xx,1302) VR=DS ROI Mean 
		/// </summary>
		public const uint ROIMean = 0x60001302;
		
		/// <summary>(60xx,1303) VR=DS ROI Standard Deviation 
		/// </summary>
		public const uint ROIStandardDeviation = 0x60001303;
		
		/// <summary>(60xx,1500) VR=LO Overlay Label 
		/// </summary>
		public const uint OverlayLabel = 0x60001500;
		
		/// <summary>(60xx,3000) VR=OW,OB Overlay Data 
		/// </summary>
		public const uint OverlayData = 0x60003000;
		
		/// <summary>(60xx,4000) VR=LT (Overlay) Comments (Retired) 
		/// </summary>
		public const uint OverlayCommentsRetired = 0x60004000;
		
		/// <summary>(7FE0,0010) VR=OW,OB Pixel Data 
		/// </summary>
		public const uint PixelData = 0x7FE00010;
		
		/// <summary>(FFFC,FFFC) VR=OB Data Set Trailing Padding 
		/// </summary>
		public const uint DataSetTrailingPadding = 0xFFFCFFFC;
		
		/// <summary>(FFFE,E000) VR=NONE Item 
		/// </summary>
		public const uint Item = 0xFFFEE000;
		
		/// <summary>(FFFE,E00D) VR=NONE Item Delimitation Item 
		/// </summary>
		public const uint ItemDelimitationItem = 0xFFFEE00D;
		
		/// <summary>(FFFE,E0DD) VR=NONE Seq Delimitation Item 
		/// </summary>
		public const uint SeqDelimitationItem = 0xFFFEE0DD;

		public static void Main()
		{
			try
			{
				Console.WriteLine( "CommandRecognitionCodeRetired=" + Tags.ToHexString(Tags.GetTag( "CommandRecognitionCodeRetired" ) ));
				Console.WriteLine( "0x00000010=" + Tags.GetName( 0x00000010 ) );
			}
			catch( Exception e )
			{
				Console.WriteLine( e );
				Console.WriteLine( "Error!!!!!!!!!!!!!" );
			}
		}
	}
}