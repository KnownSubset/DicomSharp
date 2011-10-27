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
	/// </summary>
	public class AAbort : PduI
	{
		public const int SERVICE_USER = 0;
		public const int SERVICE_PROVIDER = 2;
		public const int REASON_NOT_SPECIFIED = 0;
		public const int UNRECOGNIZED_PDU = 1;
		public const int UNEXPECTED_PDU = 2;
		public const int UNRECOGNIZED_PDU_PARAMETER = 4;
		public const int UNEXPECTED_PDU_PARAMETER = 5;
		public const int INVALID_PDU_PARAMETER_VALUE = 6;

		private readonly byte[] buf;
		
		internal static AAbort Parse(UnparsedPdu raw)
		{
			if (raw.length() != 4)
			{
				throw new PduException("Illegal A-ABORT " + raw, new AAbort(SERVICE_PROVIDER, INVALID_PDU_PARAMETER_VALUE));
			}
			return new AAbort(raw.buffer());
		}
		
		public AAbort(byte[] buf)
		{
			this.buf = buf;
		}
		
		internal AAbort(int source, int reason)
		{
			this.buf = new byte[]{7, 0, 0, 0, 0, 4, 0, 0, (byte) source, (byte) reason};
		}
		
		/// <summary>
		/// Returns Source field value.
		/// </summary>
		/// <returns>
		/// Source field value. 
		/// </returns>
		public int source()
		{
			return buf[8] & 0xff;
		}
		
		/// <summary>
		/// Returns Reason field value.
		/// </summary>
		/// <returns>
		/// Reason field value. 
		/// </returns>
		public int reason()
		{
			return buf[9] & 0xff;
		}
		
		public void  WriteTo(Stream outs)
		{
			outs.Write(buf, 0, buf.Length);
			StringUtils.dumpBytes( "AAbort", buf, 0, buf.Length );
		}
		
		public String ToString(bool verbose)
		{
			return ToString();
		}
		
		public override String ToString()
		{
			return toStringBuffer(new System.Text.StringBuilder()).ToString();
		}
		
		internal System.Text.StringBuilder toStringBuffer(System.Text.StringBuilder sb)
		{
			return sb.Append("A-ABORT\n\tsource=").Append(sourceAsString()).Append("\n\treason=").Append(reasonAsString());
		}
		
		private String sourceAsString()
		{
			switch (source())
			{
				case SERVICE_USER: 
					return "0 - service-user";
				
				case SERVICE_PROVIDER: 
					return "2 - service-provider";
				
				default: 
					return source().ToString();
				
			}
		}
		
		private String reasonAsString()
		{
			switch (reason())
			{
				case REASON_NOT_SPECIFIED: 
					return "0 - reason-not-specified";
				
				case UNRECOGNIZED_PDU: 
					return "1 - unrecognized-Pdu";
				
				case UNEXPECTED_PDU: 
					return "2 - unexpected-Pdu";
				
				case UNRECOGNIZED_PDU_PARAMETER: 
					return "4 - unrecognized-Pdu parameter";
				
				case UNEXPECTED_PDU_PARAMETER: 
					return "5 - unexpected-Pdu parameter";
				
				case INVALID_PDU_PARAMETER_VALUE: 
					return "6 - invalid-Pdu-parameter value";
				
				default: 
					return reason().ToString();
				
			}
		}
	}
}