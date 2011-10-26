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
	using System.Text;
	using System.Collections;
	using org.dicomcs.dict;
	using org.dicomcs.util;

	/// <summary>
	/// </summary>
	public class PresContext
	{
		public const int ACCEPTANCE = 0;
		public const int USER_REJECTION = 1;
		public const int NO_REASON_GIVEN = 2;
		public const int ABSTRACT_SYNTAX_NOT_SUPPORTED = 3;
		public const int TRANSFER_SYNTAXES_NOT_SUPPORTED = 4;

		private int m_type;
		private int m_pcid;
		private int m_result;
		private String m_asuid;
		private ArrayList m_tsuids;
		

		public virtual String AbstractSyntaxUID
		{
			get
			{
				return m_asuid;
			}			
		}
		public virtual ArrayList TransferSyntaxUIDs
		{
			get
			{
				return m_tsuids;
			}			
		}
		public virtual String TransferSyntaxUID
		{
			get
			{
				return (String) m_tsuids[0];
			}			
		}

		public PresContext(int type, int pcid, int result, String asuid, String[] tsuids)
		{
			if ((m_pcid | 1) == 0 || (m_pcid & ~0xff) != 0)
			{
				throw new ArgumentException("pcid=" + pcid);
			}
			if (tsuids.Length == 0)
			{
				throw new ArgumentException("Missing TransferSyntax");
			}
			m_type = type;
			m_pcid = pcid;
			m_result = result;
			m_asuid = asuid;
			m_tsuids = new ArrayList(tsuids);
		}
		
		/// <summary>
		/// Constructor 1
		/// </summary>
		/// <param name="type"></param>
		/// <param name="din"></param>
		/// <param name="len"></param>
		/*public PresContext(int type, BinaryReader din, int len)
		{
			m_type = type;
			m_pcid = din.ReadByte();
			din.ReadByte();
			m_result = din.ReadByte();
			din.ReadByte();
			int remain = len - 4;
			String m_asuid = null;
			m_tsuids = new ArrayList();
			while (remain > 0)
			{
				int uidtype = din.ReadByte();
				din.ReadByte();
				int uidlen = din.ReadUInt16();
				switch (uidtype)
				{
					case 0x30: 
						if (type == 0x21 || m_asuid != null)
						{
							throw new PduException("Unexpected Abstract Syntax sub-item in" + " Presentation Context", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
						}
						m_asuid = AAssociateRQAC.ReadASCII(din, uidlen);
						break;
					
					case 0x40: 
						if (type == 0x21 && m_tsuids.Count > 0)
						{
							throw new PduException("Unexpected Transfer Syntax sub-item in" + " Presentation Context", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
						}
						m_tsuids.Add(AAssociateRQAC.ReadASCII(din, uidlen));
						break;
					
					default: 
						throw new PduException("unrecognized item type " + Convert.ToString(uidtype, 16) + 'H', new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU_PARAMETER));
					
				}
				remain -= 4 + uidlen;
			}
			m_asuid = m_asuid;
			if (remain < 0)
			{
				throw new PduException("Presentation item length: " + len + " mismatch length of sub-items", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
		}*/
		
		/// <summary>
		/// Constructor 2
		/// </summary>
		/// <param name="type"></param>
		/// <param name="bb"></param>
		/// <param name="len"></param>
		public PresContext(int type, ByteBuffer bb, int len)
		{
			m_type = type;
			m_pcid = bb.ReadByte();
			bb.Skip();
			m_result = bb.ReadByte();
			bb.Skip();
			int remain = len - 4;
			m_tsuids = new ArrayList();
			while (remain > 0)
			{
				int uidtype = bb.ReadByte();
				bb.Skip();
				int uidlen = bb.ReadInt16();
				switch (uidtype)
				{
					case 0x30: 
						if (type == 0x21 || m_asuid != null)
						{
							throw new PduException("Unexpected Abstract Syntax sub-item in" + " Presentation Context", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
						}
						m_asuid = bb.ReadString(uidlen);
						break;
					
					case 0x40: 
						if (type == 0x21 && m_tsuids.Count > 0)
						{
							throw new PduException("Unexpected Transfer Syntax sub-item in" + " Presentation Context", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
						}
						String tsuid = bb.ReadString(uidlen);
						m_tsuids.Add( tsuid );
						break;
					
					default: 
						throw new PduException("unrecognized item type " + Convert.ToString(uidtype, 16) + 'H', new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU_PARAMETER));
					
				}
				remain -= 4 + uidlen;
			}
			if (remain < 0)
			{
				throw new PduException("Presentation item length: " + len + " mismatch length of sub-items", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
		}
		
		internal void  WriteTo( ByteBuffer buf)
		{
			buf.Write((Byte) m_type);
			buf.Write((Byte) 0);
			buf.Write((Int16) length());
			buf.Write((Byte) m_pcid);
			buf.Write((Byte) 0);
			buf.Write((Byte) m_result);
			buf.Write((Byte) 0);
			if (m_asuid != null)
			{
				buf.Write((Byte) 0x30);
				buf.Write((Byte) 0);
				buf.Write((Int16) m_asuid.Length);
				buf.Write(m_asuid);
			}

			IEnumerator enu = m_tsuids.GetEnumerator();
			while( enu.MoveNext() )
			{
				String tsuid = (String) enu.Current;
				buf.Write((Byte) 0x40);
				buf.Write((Byte) 0);
				buf.Write((Int16) tsuid.Length);
				buf.Write(tsuid);
			}
		}
		
		public int length()
		{
			int retval = 4;
			if (m_asuid != null)
			{
				retval += 4 + m_asuid.Length;
			}
			IEnumerator enu = m_tsuids.GetEnumerator();
			while( enu.MoveNext() )
			{
				retval += 4 + ((String)enu.Current).Length;
			}
			return retval;
		}
		
		public int type()
		{
			return m_type;
		}
		
		public int pcid()
		{
			return m_pcid;
		}
		
		public int result()
		{
			return m_result;
		}
								
		public override String ToString()
		{
			StringBuilder sb = new StringBuilder();			
			sb.Append("PresContext[m_pcid=").Append(m_pcid);
			if (m_type == 0x20)
			{
				sb.Append(", as=").Append(UIDs.GetName(m_asuid));
			}
			else
			{
				sb.Append(", m_result=").Append(ResultAsString());
			}
			IEnumerator enu = m_tsuids.GetEnumerator();
			enu.MoveNext();
			sb.Append(", ts=").Append(UIDs.GetName((String)enu.Current));
			while( enu.MoveNext())
			{
				sb.Append(", ").Append(UIDs.GetName((String) enu.Current));
			}
			sb.Append("]");
			return sb.ToString();
		}
		
		public String ResultAsString()
		{
			switch (m_result)
			{
				case ACCEPTANCE: 
					return "0 - acceptance";
				
				case USER_REJECTION: 
					return "1 - user-rejection";
				
				case NO_REASON_GIVEN: 
					return "2 - no-reason-given";
				
				case ABSTRACT_SYNTAX_NOT_SUPPORTED: 
					return "3 - abstract-syntax-not-supported";
				
				case TRANSFER_SYNTAXES_NOT_SUPPORTED: 
					return "4 - transfer-syntaxes-not-supported";
				
				default: 
					return m_result.ToString();
				
			}
		}
	}
}