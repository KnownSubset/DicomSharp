#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2010 Fang Yang. All rights reserved.
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
	/// </summary>
	public class RoleSelection
	{
		public virtual String SOPClassUID
		{
			get { return m_asuid; }			
		}
		
		private String	m_asuid;
		private bool			m_isScu;
		private bool			m_isScp;
		
		/// <summary>Creates a new instance of RoleSelection 
		/// </summary>
		internal RoleSelection(String asuid, bool scu, bool scp)
		{
			this.m_asuid = asuid;
			this.m_isScu = scu;
			this.m_isScp = scp;
		}
		
		/*
		internal RoleSelection(BinaryReader din, int len)
		{
			int uidLen = din.ReadUInt16();
			if (uidLen + 4 != len)
			{
				throw new PduException("SCP/SCU role selection sub-item length: " + len + " mismatch UID-length:" + uidLen, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			this.m_asuid = AAssociateRQAC.ReadASCII(din, uidLen);
			this.m_isScu = din.ReadBoolean();
			this.m_isScp = din.ReadBoolean();
		}*/

		internal RoleSelection(ByteBuffer bb, int len)
		{
			int uidLen = bb.ReadInt16();
			if (uidLen + 4 != len)
			{
				throw new PduException("SCP/SCU role selection sub-item length: " + len + " mismatch UID-length:" + uidLen, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			this.m_asuid = bb.ReadString(uidLen);
			this.m_isScu = bb.ReadBoolean();
			this.m_isScp = bb.ReadBoolean();
		}
		
		public bool scu()
		{
			return m_isScu;
		}
		
		public bool scp()
		{
			return m_isScp;
		}
		
		internal int length()
		{
			return 4 + m_asuid.Length;
		}
		
		internal void  WriteTo(ByteBuffer bb)
		{
			bb.Write((System.Byte) 0x54);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) length());
			bb.Write((System.Int16) m_asuid.Length);
			bb.Write(m_asuid);
			bb.Write(m_isScu);
			bb.Write(m_isScp);
		}
	}
}