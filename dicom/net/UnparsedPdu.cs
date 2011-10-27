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
	using System.Reflection;
	using System.Text;
	using org.dicomcs.util;

	/// <summary>
	/// </summary>
	public class UnparsedPdu{		
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		internal const long MAX_LENGTH = 1048576L; // 1 MB
		private byte[] buf;
		private int m_type;
		private int len;
		
		/// <summary>
		/// Creates a new instance of RawPdu 
		/// </summary>
		public UnparsedPdu(Stream ins, byte[] buf)
		{
			if (buf == null || buf.Length < 6)
			{
				buf = new byte[10];
			}
			ReadFully(ins, buf, 0, 6);
			this.m_type = buf[0] & 0xFF;
			this.len = ((buf[2] & 0xff) << 24) | ((buf[3] & 0xff) << 16) | ((buf[4] & 0xff) << 8) | ((buf[5] & 0xff) << 0);
			if ((len & 0xFFFFFFFF) > MAX_LENGTH)
			{
				SkipFully(ins, len & 0xFFFFFFFFL);
				this.buf = null;
				return ;
			}
			if (buf.Length < 6 + len)
			{
				this.buf = new byte[6 + len];
				Array.Copy(buf, 0, this.buf, 0, 6);
			}
			else
			{
				this.buf = buf;
			}

			//ins.Read( this.buf, 6, len);
			ReadFully( ins, this.buf, 6, len );
		}
		
		new public int GetType()
		{
			return m_type;
		}
		
		public int length()
		{
			return len;
		}
		
		public byte[] buffer()
		{
			return buf;
		}
		
		public override String ToString()
		{
			return "Pdu[type=" + m_type + ", length=" + (len & 0xFFFFFFFFL) + "]";
		}
		
		internal static void SkipFully(Stream ins, long len)
		{
			long n = 0;
			while (n < len)
			{
				Int64 pos = ins.Position;
				pos = ins.Seek(len - n, SeekOrigin.Current) - pos;
				long count = pos;
				if (count < 0)
					throw new EndOfStreamException();
				n += count;
			}
		}
		
		internal static void ReadFully(Stream ins, byte[] b, int off, int len)
		{
			int n = 0;
			
			while (n < len)
			{
				int count = ins.Read( b, off + n, len - n);
				if (count < 0)
					throw new EndOfStreamException();
				n += count;
			}

			StringUtils.dumpBytes( "UnparsedPdu", b, off, n);
		}				
	}
}