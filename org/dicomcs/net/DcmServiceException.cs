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

namespace org.dicomcs.net
{
	using System;
	using org.dicomcs.dict;
	using org.dicomcs.net;
	using org.dicomcs.data;
	
	/// <summary> 
	/// </summary>
	public class DcmServiceException : Exception
	{
		public virtual int Status
		{
			get { return status; }
		}
		public virtual int ErrorID
		{
			get { return errorID; }
			set { this.errorID = value; }
		}
		public virtual int EventTypeID
		{
			get { return eventTypeID; }
			set { this.eventTypeID = value; }
		}
		public virtual int ActionTypeID
		{
			get { return actionTypeID; }
			set { this.actionTypeID = value; }
		}
		
		private readonly int status;
		private int errorID = - 1;
		private int actionTypeID = - 1;
		private int eventTypeID = - 1;
		
		public DcmServiceException(int status)
		{
			this.status = status;
		}
		
		public DcmServiceException(int status, String msg):base(msg)
		{
			this.status = status;
		}
		
		public DcmServiceException(int status, String msg, Exception cause):base(msg, cause)
		{
			this.status = status;
		}
		
		public DcmServiceException(int status, Exception cause):base("", cause)
		{
			this.status = status;
		}
		
		public virtual void  WriteTo(Command cmd)
		{
			cmd.PutUS(Tags.Status, status);
			String msg = Message;
			if (msg != null && msg.Length > 0)
			{
				cmd.PutLO(Tags.ErrorComment, msg.Length > 64?msg.Substring(0, (64) - (0)):msg);
			}
			if (errorID >= 0)
			{
				cmd.PutUS(Tags.ErrorID, errorID);
			}
			if (actionTypeID >= 0)
			{
				cmd.PutUS(Tags.ActionTypeID, actionTypeID);
			}
			if (eventTypeID >= 0)
			{
				cmd.PutUS(Tags.EventTypeID, eventTypeID);
			}
		}
	}
}