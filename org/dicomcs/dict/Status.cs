#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002 Fang Yang. All rights reserved.
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

	public class Status
	{
		private static Hashtable s_dict = new Hashtable( 50 );

		static Status()
		{	
			FieldInfo[] fields = typeof(Status).GetFields( BindingFlags.Public | BindingFlags.Static ); 
			foreach ( FieldInfo field in fields )
			{
				String name = field.Name; 
				int status = (int)field.GetValue( null );
				s_dict.Add( status, name );
			}
		}

		/// <summary>Private constructor 
		/// </summary>
		private Status()
		{
		}
		
		/// <summary>
		/// Get the description name of this status
		/// </summary>
		/// <param name="status">the status</param>
		/// <returns></returns>
		public static String GetName( int status)
		{
			if( s_dict.Contains( status) )
				return (String) s_dict[status];
			else
				throw new ArgumentException("Unkown status: " + Status.ToHexString(status));
		}

		/// <summary>
		/// Get the status value for this description name
		/// </summary>
		/// <param name="name">the description name</param>
		/// <returns></returns>
		public static int GetStatus(String name)
		{
			try
			{
				return (int) typeof(Status).GetField(name, BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(null);
			}
			catch ( Exception e)
			{
				throw new ArgumentException("Unkown Status Name: " + name);
			}
		}
		
		public static String ToHexString(int status)
		{
			return GetName(status) + " (" + String.Format("0x{0:x4}", status ) + ")";
		}
		
		/// <summary>Success: Success 
		/// </summary>
		public const int Success = 0x0000;
		
		/// <summary>Cancel: Cancel 
		/// </summary>
		public const int Cancel = 0xFE00;
		
		/// <summary>Pending: Pending 
		/// </summary>
		public const int Pending = 0xFF00;
		
		/// <summary>Warning: Attribute list error 
		/// </summary>
		public const int AttributeListError = 0x0107;
		
		/// <summary>Warning: Attribute Value Out of Range 
		/// </summary>
		public const int AttributeValueOutOfRange = 0x0116;
		
		/// <summary>Failure: Refused: SOP class not supported 
		/// </summary>
		public const int SOPClassNotSupported = 0x0122;
		
		/// <summary>Failure: Class-instance conflict 
		/// </summary>
		public const int ClassInstanceConflict = 0x0119;
		
		/// <summary>Failure: Duplicate SOP instance 
		/// </summary>
		public const int DuplicateSOPInstance = 0x0111;
		
		/// <summary>Failure: Duplicate invocation 
		/// </summary>
		public const int DuplicateInvocation = 0x0210;
		
		/// <summary>Failure: Invalid argument value 
		/// </summary>
		public const int InvalidArgumentValue = 0x0115;
		
		/// <summary>Failure: Invalid attribute value 
		/// </summary>
		public const int InvalidAttributeValue = 0x0106;
		
		/// <summary>Failure: Invalid object instance 
		/// </summary>
		public const int InvalidObjectInstance = 0x0117;
		
		/// <summary>Failure: Missing attribute 
		/// </summary>
		public const int MissingAttribute = 0x0120;
		
		/// <summary>Failure: Missing attribute value 
		/// </summary>
		public const int MissingAttributeValue = 0x0121;
		
		/// <summary>Failure: Mistyped argument 
		/// </summary>
		public const int MistypedArgument = 0x0212;
		
		/// <summary>Failure: No such argument 
		/// </summary>
		public const int NoSuchArgument = 0x0114;
		
		/// <summary>Failure: No such event type 
		/// </summary>
		public const int NoSuchEventType = 0x0113;
		
		/// <summary>Failure: No Such object instance 
		/// </summary>
		public const int NoSuchObjectInstance = 0x0112;
		
		/// <summary>Failure: No Such SOP class 
		/// </summary>
		public const int NoSuchSOPClass = 0x0118;
		
		/// <summary>Failure: Processing failure 
		/// </summary>
		public const int ProcessingFailure = 0x0110;
		
		/// <summary>Failure: Resource limitation 
		/// </summary>
		public const int ResourceLimitation = 0x0213;
		
		/// <summary>Failure: Unrecognized operation 
		/// </summary>
		public const int UnrecognizedOperation = 0x0211;
		
		/// <summary>Failure: No such action type 
		/// </summary>
		public const int NoSuchActionType = 0x0123;
		
		/// <summary>
		/// Storage Failure: Out of Resources 
		/// DUPLICATED STATUS CODE!!!!!
		/// </summary>
		//public const int StorageOutOfResources = 0xA700;
		
		/// <summary>
		/// Storage Failure: Data Set does not match SOP Class (Error) 
		/// DUPLICATED!!!
		/// </summary>
		//public const int DataSetDoesNotMatchSOPClassError = 0xA900;
		
		/// <summary>
		/// Storage Failure: Cannot understand 
		/// DUPLICATED!!!!
		/// </summary>
		//public const int CannotUnderstand = 0xC000;
		
		/// <summary>
		/// Storage Warning: Coercion of Data Elements 
		/// DUPLICATED!!!!
		/// </summary>
		//public const int CoercionOfDataElements = 0xB000;
		
		/// <summary>Storage Warning: Data Set does not match SOP Class (Warning) 
		/// </summary>
		public const int DataSetDoesNotMatchSOPClassWarning = 0xB007;
		
		/// <summary>Storage Warning: Elements Discarded 
		/// </summary>
		public const int ElementsDiscarded = 0xB006;
		
		/// <summary>QueryRetrieve Failure: Out of Resources 
		/// </summary>
		public const int OutOfResources = 0xA700;
		
		/// <summary>QueryRetrieve Failure: Unable to calculate number of matches 
		/// </summary>
		public const int UnableToCalculateNumberOfMatches = 0xA701;
		
		/// <summary>QueryRetrieve Failure: Unable to perform suboperations 
		/// </summary>
		public const int UnableToPerformSuboperations = 0xA702;
		
		/// <summary>QueryRetrieve Failure: Move Destination unknown 
		/// </summary>
		public const int MoveDestinationUnknown = 0xA801;
		
		/// <summary>QueryRetrieve Failure: Identifier does not match SOP Class 
		/// </summary>
		public const int IdentifierDoesNotMatchSOPClass = 0xA900;
		
		/// <summary>QueryRetrieve Failure: Unable to process 
		/// </summary>
		public const int UnableToProcess = 0xC000;
		
		/// <summary>QueryRetrieve Pending: Optional Keys Not Supported 
		/// </summary>
		public const int OptionalKeysNotSupported = 0xFF01;
		
		/// <summary>QueryRetrieve Warning: Sub-operations Complete - One or more Failures 
		/// </summary>
		public const int SubOpsOneOrMoreFailures = 0xB000;

		public static void Main()
		{
			try
			{
				Console.WriteLine( "OptionalKeysNotSupported=" + Status.ToHexString(Status.GetStatus( "OptionalKeysNotSupported" ) ));
				Console.WriteLine( "0xFF01=" + Status.GetName( 0xFF01 ) );
			}
			catch( Exception e )
			{
				Console.WriteLine( e );
				Console.WriteLine( "Error!!!!!!!!!!!!!" );
			}
		}
	}
}