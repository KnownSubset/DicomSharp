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
	using org.dicomcs.util;	
	
	/// <summary>
	/// </summary>
	public sealed class PDataTF : PduI
	{
		public const int DEF_MAX_PDU_LENGTH = 16352;
		
		private const int DEF_MAX_LENGTH = 0xFFFF;
		private const int MIN_MAX_LENGTH = 128;
		private byte[] buf;
		private int Pdulen;
		private int wpos;
		private ArrayList pdvs = new ArrayList();
		private IEnumerator enu;
		private PDV curPDV = null;
		

		public static PDataTF Parse(UnparsedPdu raw)
		{
			if (raw.buffer() == null)
			{
				throw new PduException("Pdu length exceeds supported maximum " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.REASON_NOT_SPECIFIED));
			}
			return new PDataTF(raw.length(), raw.buffer());
		}
		
		public PDataTF(int Pdulen, byte[] buf)
		{
			this.Pdulen = Pdulen;
			this.wpos = Pdulen + 12;
			this.buf = buf;
			int off = 6;
			while (off <= Pdulen)
			{
				PDV pdv = new PDV(this, off);
				pdvs.Add(pdv);
				off += 4 + pdv.length();
			}
			if (off != Pdulen + 6)
			{
				throw new PduException("Illegal " + ToString(), new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			this.enu = pdvs.GetEnumerator();
		}
		
		internal PDataTF(int maxLength)
		{
			if (maxLength == 0)
			{
				maxLength = DEF_MAX_LENGTH;
			}
			if (maxLength < MIN_MAX_LENGTH || maxLength > UnparsedPdu.MAX_LENGTH)
			{
				throw new System.ArgumentException("maxLength:" + maxLength);
			}
			this.Pdulen = 0;
			this.wpos = 12;
			this.buf = new byte[6 + maxLength];
			this.enu = null;
		}
		
		public void  Clear()
		{
			if (enu != null)
			{
				throw new SystemException("P-DATA-TF Read only");
			}
			Pdulen = 0;
			wpos = 12;
			pdvs.Clear();
		}
		
		public PDV ReadPDV()
		{
			if (enu == null)
			{
				throw new SystemException("P-DATA-TF Write only");
			}
			return enu.MoveNext() ? (PDV) enu.Current : null;
		}
		
		public String ToString( bool verbose )
		{
			return ToString();
		}

		public override String ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("P-DATA-TF[Pdulen=").Append(Pdulen).Append("]");
			IEnumerator enu = pdvs.GetEnumerator();
			while (enu.MoveNext())
			{
				sb.Append("\n\t").Append(enu.Current);
			}
			return sb.ToString();
		}
		
		public int Free()
		{
			return buf.Length - wpos;
		}
		
		public void  OpenPDV(int pcid, bool cmd)
		{
			if (enu != null)
			{
				throw new System.SystemException("P-DATA-TF Read only");
			}
			if ((pcid & 1) == 0)
			{
				throw new System.ArgumentException("pcid=" + pcid);
			}
			if (curPDV != null)
			{
				throw new System.SystemException("Open PDV " + curPDV);
			}
			if (Free() < 0)
			{
				throw new System.SystemException("Maximal length of Pdu reached");
			}
			curPDV = new PDV(this, 6 + Pdulen);
			curPDV.pcid(pcid);
			curPDV.cmd(cmd);
			Pdulen += 6;
		}
		
		internal bool IsOpenPDV()
		{
			return curPDV != null;
		}
		
		internal bool IsEmpty()
		{
			return pdvs.Count == 0;
		}
		
		public void  ClosePDV(bool last)
		{
			if (curPDV == null)
			{
				throw new System.SystemException("No Open PDV");
			}
			curPDV.Last(last);
			curPDV.Close();
			pdvs.Add(curPDV);
			curPDV = null;
			wpos += 6;
		}
		
		public bool Write(int b)
		{
			if (curPDV == null)
			{
				throw new System.SystemException("No Open PDV");
			}
			if (wpos >= buf.Length)
				return false;
			buf[wpos++] = (byte) b;
			++Pdulen;
			return true;
		}
		
		public int Write(byte[] b, int off, int len)
		{
			if (curPDV == null)
			{
				throw new System.SystemException("No Open PDV");
			}
			int wlen = System.Math.Min(len, buf.Length - wpos);
			Array.Copy(b, off, buf, wpos, wlen);
			wpos += wlen;
			Pdulen += wlen;
			return wlen;
		}
		
		public void  WriteTo(Stream outs)
		{
			if (curPDV != null)
			{
				throw new System.SystemException("Open PDV " + curPDV);
			}
			buf[0] = (byte) 4;
			buf[1] = (byte) 0;
			buf[2] = (byte) (Pdulen >> 24);
			buf[3] = (byte) (Pdulen >> 16);
			buf[4] = (byte) (Pdulen >> 8);
			buf[5] = (byte) (Pdulen >> 0);
			outs.Write(buf, 0, Pdulen + 6);

			StringUtils.dumpBytes( "PDataTF", buf, 0, Pdulen+6 );
		}
		
		/// <summary>
		/// PDV
		/// </summary>
		public class PDV
		{
			private PDataTF m_pDataTF;
			internal int m_off;

			public PDV(PDataTF pDataTF, int off)
			{
				m_pDataTF = pDataTF;
				m_off = off;
			}

			public virtual Stream InputStream
			{
				get
				{
					return new MemoryStream(m_pDataTF.buf, m_off + 6, length() - 2);
				}				
			}			
			
			internal void  pcid(int pcid)
			{
				m_pDataTF.buf[m_off + 4] = (byte) pcid;
			}
			
			internal void  length(int pdvLen)
			{
				m_pDataTF.buf[m_off] = (byte) (pdvLen >> 24);
				m_pDataTF.buf[m_off + 1] = (byte) (pdvLen >> 16);
				m_pDataTF.buf[m_off + 2] = (byte) (pdvLen >> 8);
				m_pDataTF.buf[m_off + 3] = (byte) (pdvLen >> 0);
			}
			
			internal void  cmd(bool cmd)
			{
				if (cmd)
				{
					m_pDataTF.buf[m_off + 5] |= 1;
				}
				else
				{
					m_pDataTF.buf[m_off + 5] &= 0xFE;
				}
			}
			
			internal void  Last(bool last)
			{
				if (last)
				{
					m_pDataTF.buf[m_off + 5] |= 2;
				}
				else
				{
					m_pDataTF.buf[m_off + 5] &= 0xFD;
				}
			}
			
			internal void  Close()
			{
				length(m_pDataTF.wpos - m_off - 4);
			}
			
			public int length()
			{
				return ((m_pDataTF.buf[m_off] & 0xff) << 24) 
						| ((m_pDataTF.buf[m_off + 1] & 0xff) << 16) 
						| ((m_pDataTF.buf[m_off + 2] & 0xff) << 8) 
						| ((m_pDataTF.buf[m_off + 3] & 0xff) << 0);
			}
			
			public int pcid()
			{
				return m_pDataTF.buf[m_off + 4] & 0xFF;
			}
			
			public bool cmd()
			{
				return (m_pDataTF.buf[m_off + 5] & 1) != 0;
			}
			
			public bool last()
			{
				return (m_pDataTF.buf[m_off + 5] & 2) != 0;
			}
			
			
			public override String ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("PDV[pc-").Append(pcid()).Append(cmd()?",cmd":",data").Append(last()?"(last),m_off=":",m_off=").Append(m_off).Append(",pdvlen=").Append(length()).Append("]");
				return sb.ToString();
			}			
		}
	}
}