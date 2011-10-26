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

namespace org.dicomcs.data
{
	using System;
	using System.IO;
	using System.Text;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	/// <summary>
	/// Defines behavior of <code>Command</code> container objects.
	/// </summary>
	/// <seealso cref=" "DICOM Part 7: Message Exchange, 6.3.1 Command Set Structure" />
	public class Command : DcmObject
	{
		public const int C_STORE_RQ = 0x0001;
		public const int C_STORE_RSP = 0x8001;
		public const int C_GET_RQ = 0x0010;
		public const int C_GET_RSP = 0x8010;
		public const int C_FIND_RQ = 0x0020;
		public const int C_FIND_RSP = 0x8020;
		public const int C_MOVE_RQ = 0x0021;
		public const int C_MOVE_RSP = 0x8021;
		public const int C_ECHO_RQ = 0x0030;
		public const int C_ECHO_RSP = 0x8030;
		public const int N_EVENT_REPORT_RQ = 0x0100;
		public const int N_EVENT_REPORT_RSP = 0x8100;
		public const int N_GET_RQ = 0x0110;
		public const int N_GET_RSP = 0x8110;
		public const int N_SET_RQ = 0x0120;
		public const int N_SET_RSP = 0x8120;
		public const int N_ACTION_RQ = 0x0130;
		public const int N_ACTION_RSP = 0x8130;
		public const int N_CREATE_RQ = 0x0140;
		public const int N_CREATE_RSP = 0x8140;
		public const int N_DELETE_RQ = 0x0150;
		public const int N_DELETE_RSP = 0x8150;
		public const int C_CANCEL_RQ = 0x0FFF;
		public const int MEDIUM = 0x0000;
		public const int HIGH = 0x0001;
		public const int LOW = 0x0002;
		public const int NO_DATASET = 0x0101;

		public virtual int CommandField
		{
			get { return cmdField; }			
		}
		public virtual int MessageID
		{
			get { return msgID; }			
		}
		public virtual int MessageIDToBeingRespondedTo
		{
			get { return msgID; }
			
		}
		public virtual String AffectedSOPClassUID
		{
			get { return sopClassUID; }			
		}
		public virtual String RequestedSOPClassUID
		{
			get { return sopClassUID; }			
		}
		public virtual String AffectedSOPInstanceUID
		{
			get { return sopInstUID; }			
		}
		public virtual String RequestedSOPInstanceUID
		{
			get { return sopInstUID; }
		}
		public virtual int Status
		{
			get { return status; }
		}
		
		private int cmdField = - 1;
		private int dataSetType = - 1;
		private int status = - 1;
		private int msgID = - 1;
		private String sopClassUID = null;
		private String sopInstUID = null;
		
		
		public bool IsPending()
		{
			switch (status)
			{
				case 0xff00: case 0xff01: 
					return true;
			}
			return false;
		}
		
		public bool IsRequest()
		{
			switch (cmdField)
			{
				case C_STORE_RQ: 
				case C_GET_RQ: 
				case C_FIND_RQ: 
				case C_MOVE_RQ: 
				case C_ECHO_RQ: 
				case N_EVENT_REPORT_RQ: 
				case N_GET_RQ: 
				case N_SET_RQ: 
				case N_ACTION_RQ: 
				case N_CREATE_RQ: 
				case N_DELETE_RQ: 
				case C_CANCEL_RQ: 
					return true;
				
			}
			return false;
		}
		
		public bool IsResponse()
		{
			switch (cmdField)
			{
				case C_STORE_RSP: 
				case C_GET_RSP: 
				case C_FIND_RSP: 
				case C_MOVE_RSP: 
				case C_ECHO_RSP: 
				case N_EVENT_REPORT_RSP: 
				case N_GET_RSP: 
				case N_SET_RSP: 
				case N_ACTION_RSP: 
				case N_CREATE_RSP: 
				case N_DELETE_RSP: 
					return true;
				
			}
			return false;
		}
		
		public bool HasDataset()
		{
			if (dataSetType == - 1)
				throw new System.SystemException();
			
			return dataSetType != NO_DATASET;
		}
		
		
		private Command InitCxxxxRQ(int cmd, int msgID, String sopClassUID, int priority)
		{
			if (priority != MEDIUM && priority != HIGH && priority != LOW)
			{
				throw new System.ArgumentException("priority=" + priority);
			}
			if (sopClassUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			PutUS(Tags.CommandField, cmd);
			PutUS(Tags.MessageID, msgID);
			PutUS(Tags.Priority, priority);
			return this;
		}
		
		private Command InitCxxxxRSP(int cmd, int msgID, String sopClassUID, int status)
		{
			if (sopClassUID != null)
			{
				PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			}
			PutUS(Tags.CommandField, cmd);
			PutUS(Tags.MessageIDToBeingRespondedTo, msgID);
			PutUS(Tags.Status, status);
			return this;
		}
		
		public Command InitCStoreRQ(int msgID, String sopClassUID, String sopInstUID, int priority)
		{
			if (sopInstUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			InitCxxxxRQ(C_STORE_RQ, msgID, sopClassUID, priority);
			PutUI(Tags.AffectedSOPInstanceUID, sopInstUID);
			return this;
		}
		
		public Command setMoveOriginator(String aet, int msgID)
		{
			if (aet.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutAE(Tags.MoveOriginatorAET, aet);
			PutUS(Tags.MoveOriginatorMessageID, msgID);
			return this;
		}
		
		public Command InitCStoreRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(C_STORE_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitCFindRQ(int msgID, String sopClassUID, int priority)
		{
			return InitCxxxxRQ(C_FIND_RQ, msgID, sopClassUID, priority);
		}
		
		public Command InitCFindRSP(int msgID, String sopClassUID, int status)
		{
			return InitCxxxxRSP(C_FIND_RSP, msgID, sopClassUID, status);
		}
		
		public Command InitCCancelRQ(int msgID)
		{
			PutUS(Tags.CommandField, C_CANCEL_RQ);
			PutUS(Tags.MessageIDToBeingRespondedTo, msgID);
			return this;
		}
		
		public Command InitCGetRQ(int msgID, String sopClassUID, int priority)
		{
			return InitCxxxxRQ(C_GET_RQ, msgID, sopClassUID, priority);
		}
		
		public Command InitCGetRSP(int msgID, String sopClassUID, int status)
		{
			return InitCxxxxRSP(C_GET_RSP, msgID, sopClassUID, status);
		}
		
		public Command InitCMoveRQ(int msgID, String sopClassUID, int priority, String moveDest)
		{
			if (moveDest.Length == 0)
			{
				throw new System.ArgumentException();
			}
			InitCxxxxRQ(C_MOVE_RQ, msgID, sopClassUID, priority);
			PutAE(Tags.MoveDestination, moveDest);
			return this;
		}
		
		public Command InitCMoveRSP(int msgID, String sopClassUID, int status)
		{
			return InitCxxxxRSP(C_MOVE_RSP, msgID, sopClassUID, status);
		}
		
		public Command InitCEchoRQ(int msgID, String sopClassUID)
		{
			if (sopClassUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			PutUS(Tags.CommandField, C_ECHO_RQ);
			PutUS(Tags.MessageID, msgID);
			return this;
		}
		
		public Command InitCEchoRQ(int msgID)
		{
			return InitCEchoRQ(msgID, UIDs.Verification);
		}
		
		public Command InitCEchoRSP(int msgID, String sopClassUID, int status)
		{
			return InitCxxxxRSP(C_ECHO_RSP, msgID, sopClassUID, status);
		}
		
		public Command InitCEchoRSP(int msgID)
		{
			return InitCxxxxRSP(C_ECHO_RSP, msgID, UIDs.Verification, 0);
		}
		
		private Command InitNxxxxRQ(int cmd, int msgID, String sopClassUID, String sopInstanceUID)
		{
			if (sopClassUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			if (sopInstanceUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutUI(Tags.RequestedSOPClassUID, sopClassUID);
			PutUS(Tags.CommandField, cmd);
			PutUS(Tags.MessageID, msgID);
			PutUI(Tags.RequestedSOPInstanceUID, sopInstanceUID);
			return this;
		}
		
		private Command InitNxxxxRSP(int cmd, int msgID, String sopClassUID, String sopInstanceUID, int status)
		{
			if (sopClassUID != null)
			{
				PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			}
			PutUS(Tags.CommandField, cmd);
			PutUS(Tags.MessageIDToBeingRespondedTo, msgID);
			PutUS(Tags.Status, status);
			if (sopInstanceUID != null)
			{
				PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
			}
			return this;
		}
		
		public Command InitNEventReportRQ(int msgID, String sopClassUID, String sopInstanceUID, int eventTypeID)
		{
			if (sopClassUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			if (sopInstanceUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			PutUS(Tags.CommandField, N_EVENT_REPORT_RQ);
			PutUS(Tags.MessageID, msgID);
			PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
			PutUS(Tags.EventTypeID, eventTypeID);
			return this;
		}
		
		public Command InitNEventReportRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_EVENT_REPORT_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitNGetRQ(int msgID, String sopClassUID, String sopInstUID, int[] attrIDs)
		{
			InitNxxxxRQ(N_GET_RQ, msgID, sopClassUID, sopInstUID);
			if (attrIDs != null)
			{
				PutAT(Tags.AttributeIdentifierList, attrIDs);
			}
			return this;
		}
		
		public Command InitNGetRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_GET_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitNSetRQ(int msgID, String sopClassUID, String sopInstUID)
		{
			return InitNxxxxRQ(N_SET_RQ, msgID, sopClassUID, sopInstUID);
		}
		
		public Command InitNSetRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_SET_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitNActionRQ(int msgID, String sopClassUID, String sopInstUID, int actionTypeID)
		{
			InitNxxxxRQ(N_ACTION_RQ, msgID, sopClassUID, sopInstUID);
			PutUS(Tags.ActionTypeID, actionTypeID);
			return this;
		}
		
		public Command InitNActionRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_ACTION_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitNCreateRQ(int msgID, String sopClassUID, String sopInstanceUID)
		{
			if (sopClassUID.Length == 0)
			{
				throw new System.ArgumentException();
			}
			PutUI(Tags.AffectedSOPClassUID, sopClassUID);
			PutUS(Tags.CommandField, N_CREATE_RQ);
			PutUS(Tags.MessageID, msgID);
			if (sopInstanceUID != null)
			{
				PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
			}
			return this;
		}
		
		public Command InitNCreateRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_CREATE_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public Command InitNDeleteRQ(int msgID, String sopClassUID, String sopInstUID)
		{
			return InitNxxxxRQ(N_DELETE_RQ, msgID, sopClassUID, sopInstUID);
		}
		
		public Command InitNDeleteRSP(int msgID, String sopClassUID, String sopInstUID, int status)
		{
			return InitNxxxxRSP(N_DELETE_RSP, msgID, sopClassUID, sopInstUID, status);
		}
		
		public override DcmElement Put(DcmElement newElem)
		{
			uint tag = newElem.tag();
			if ((tag & 0xFFFF0000) != 0x00000000)
			{
				throw new System.ArgumentException(newElem.ToString());
			}
			
			if (newElem.GetByteBuffer().GetOrder() != ByteOrder.LITTLE_ENDIAN)
				throw new ArgumentException( "The byte order must be LITTLE_ENDIAN: " + newElem.GetByteBuffer().ToString());

			try
			{
				switch (tag)
				{
					case Tags.AffectedSOPClassUID: 
					case Tags.RequestedSOPClassUID: 
						sopClassUID = newElem.GetString(null);
						break;
					
					case Tags.CommandField: 
						cmdField = newElem.Int;
						break;
					
					case Tags.MessageID: case 
					Tags.MessageIDToBeingRespondedTo: 
						msgID = newElem.Int;
						break;
					
					case Tags.DataSetType: 
						dataSetType = newElem.Int;
						break;
					
					case Tags.Status: 
						status = newElem.Int;
						break;
					
					case Tags.AffectedSOPInstanceUID: 
					case Tags.RequestedSOPInstanceUID: 
						sopInstUID = newElem.GetString(null);
						break;
					
				}
			}
			catch (DcmValueException ex)
			{
				throw new System.ArgumentException(newElem.ToString(), ex);
			}
			return base.Put(newElem);
		}
		
		public virtual int length()
		{
			return grLen() + 12;
		}
		
		private int grLen()
		{
			int len = 0;
			 for (int i = 0, n = m_list.Count; i < n; ++i)
				len += ((DcmElement) m_list[i]).length() + 8;
			
			return len;
		}
		
		public void  Write(DcmHandlerI handler)
		{
			handler.DcmDecodeParam = DcmDecodeParam.IVR_LE;
			Write(0x00000000, grLen(), handler);
		}
		
		public void  Write(Stream os)
		{
			Write(new DcmStreamHandler(os));
		}
		
		public void  Read(Stream ins)
		{
			DcmParser Parser = new DcmParser(ins);
			Parser.DcmHandler = DcmHandler;
			Parser.ParseCommand();
		}
		
		public override String ToString()
		{
			return toStringBuffer(new StringBuilder()).ToString();
		}
		
		private StringBuilder toStringBuffer(StringBuilder sb)
		{
			sb.Append(msgID).Append(':').Append(cmdFieldAsString());
			if (dataSetType != NO_DATASET)
				sb.Append(" with Dataset");
			if (sopClassUID != null)
				sb.Append("\n\tclass:\t").Append(UIDs.ToString(sopClassUID));
			if (sopInstUID != null)
				sb.Append("\n\tinstance:\t").Append(sopInstUID);
			if (status != - 1)
				sb.Append("\n\tstatus:\t").Append(Convert.ToString(status, 16));
			return sb;
		}
		
		private String cmdFieldAsString()
		{
			switch (cmdField)
			{
				case C_STORE_RQ: 
					return "C_STORE_RQ";
				
				case C_GET_RQ: 
					return "C_GET_RQ";
				
				case C_FIND_RQ: 
					return "C_FIND_RQ";
				
				case C_MOVE_RQ: 
					return "C_MOVE_RQ";
				
				case C_ECHO_RQ: 
					return "C_ECHO_RQ";
				
				case N_EVENT_REPORT_RQ: 
					return "N_EVENT_REPORT_RQ";
				
				case N_GET_RQ: 
					return "N_GET_RQ";
				
				case N_SET_RQ: 
					return "N_SET_RQ";
				
				case N_ACTION_RQ: 
					return "N_ACTION_RQ";
				
				case N_CREATE_RQ: 
					return "N_CREATE_RQ";
				
				case N_DELETE_RQ: 
					return "N_DELETE_RQ";
				
				case C_CANCEL_RQ: 
					return "C_CANCEL_RQ";
				
				case C_STORE_RSP: 
					return "C_STORE_RSP";
				
				case C_GET_RSP: 
					return "C_GET_RSP";
				
				case C_FIND_RSP: 
					return "C_FIND_RSP";
				
				case C_MOVE_RSP: 
					return "C_MOVE_RSP";
				
				case C_ECHO_RSP: 
					return "C_ECHO_RSP";
				
				case N_EVENT_REPORT_RSP: 
					return "N_EVENT_REPORT_RSP";
				
				case N_GET_RSP: 
					return "N_GET_RSP";
				
				case N_SET_RSP: 
					return "N_SET_RSP";
				
				case N_ACTION_RSP: 
					return "N_ACTION_RSP";
				
				case N_CREATE_RSP: 
					return "N_CREATE_RSP";
				
				case N_DELETE_RSP: 
					return "N_DELETE_RSP";
				
			}
			return "cmd:" + Convert.ToString(cmdField, 16);
		}
	}
}