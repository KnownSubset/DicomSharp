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
	using System.IO;
	using System.Collections;
	using org.dicomcs.net;
	
	/// <summary> 
	/// 
	/// </summary>
	public class FutureRSP : DimseListenerI, AssociationListenerI
	{
		private long setAfterCloseTO = 500;
		
		private bool closed = false;
		private bool ready = false;
		private Dimse rsp = null;
		private ArrayList pending = new ArrayList();
		private System.IO.IOException exception = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public FutureRSP()
		{
		}

		public virtual System.IO.IOException Exception
		{
			get
			{
				lock(this)
				{
					return exception;
				}
			}
			
			set
			{
				lock(this)
				{
					exception = value;
					ready = true;
					System.Threading.Monitor.PulseAll(this);
				}
			}
		}
		
		public virtual void  Set(Dimse rsp)
		{
			lock(this)
			{
				this.rsp = rsp;
				ready = true;
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		public virtual Dimse Get()
		{
			lock(this)
			{
				while (!ready && !closed)
				{
					System.Threading.Monitor.Wait(this);
				}
				
				if (!ready)
					System.Threading.Monitor.Wait(this, TimeSpan.FromMilliseconds(setAfterCloseTO));
				
				return DoGet();
			}
		}
		
		public virtual ArrayList ListPending()
		{
			lock(this)
			{
				return pending;
			}
		}
		
		public virtual bool IsReady()
		{
			lock(this)
			{
				return ready;
			}
		}
		
		public virtual Dimse Peek()
		{
			lock(this)
			{
				return rsp;
			}
		}
		
		public virtual void  DimseReceived(Association assoc, Dimse dimse)
		{
			if (dimse.Command.IsPending())
			{
				pending.Add(dimse);
			}
			else
			{
				Set(dimse);
			}
		}
		
		public virtual void  Write(Association src, PduI Pdu)
		{
		}
		
		public virtual void  Received(Association src, Dimse dimse)
		{
		}
		
		public virtual void  Error(Association src, System.IO.IOException ioe)
		{
			Exception = ioe;
		}
		
		public virtual void  Close(Association src)
		{
			lock(this)
			{
				closed = true;
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		public virtual void  Write(Association src, Dimse dimse)
		{
		}
		
		public virtual void  Received(Association src, PduI Pdu)
		{
		}
		
		private Dimse DoGet()
		{
			if (exception != null)
				throw exception;
			else
				return rsp;
		}
	}
}