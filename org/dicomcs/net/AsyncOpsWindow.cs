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
	using org.dicomcs.util;
	
	/// <summary>
	/// 
	/// </summary>
	public class AsyncOpsWindow
	{
		public virtual int MaxOpsInvoked
		{
			get { return maxOpsInvoked; }
		}
		public virtual int MaxOpsPerformed
		{
			get { return maxOpsPerformed; }
		}
		
		private int maxOpsInvoked;
		private int maxOpsPerformed;
		internal static AsyncOpsWindow DEFAULT = new AsyncOpsWindow(1, 1);
		
		/// <summary>
		/// Creates a new instance of AsyncOpsWindow 
		/// </summary>
		internal AsyncOpsWindow(int maxOpsInvoked, int maxOpsPerformed)
		{
			this.maxOpsInvoked = maxOpsInvoked;
			this.maxOpsPerformed = maxOpsPerformed;
		}
		
		internal AsyncOpsWindow(ByteBuffer bb, int len)
		{
			if (len != 4)
			{
				throw new PduException("Illegal length of AsyncOpsWindow sub-item: " + len, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			this.maxOpsInvoked = bb.ReadInt16();
			this.maxOpsPerformed = bb.ReadInt16();
		}
		
		
		
		internal void  WriteTo(ByteBuffer bb)
		{
			bb.Write((System.Byte) 0x53);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) 4);
			bb.Write((System.Int16) maxOpsInvoked);
			bb.Write((System.Int16) maxOpsPerformed);
		}
		
		public override System.String ToString()
		{
			return "AsyncOpsWindow[maxOpsInvoked=" + maxOpsInvoked + ",maxOpsPerformed=" + maxOpsPerformed + "]";
		}
	}
}