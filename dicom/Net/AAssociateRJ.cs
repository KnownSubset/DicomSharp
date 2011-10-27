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
	using System.Text;
	using org.dicomcs.util;
	
	/// <summary>
	/// </summary>
	public class AAssociateRJ : PduI
	{
		public const int REJECTED_PERMANENT = 1;
		public const int REJECTED_TRANSIENT = 2;
		public const int SERVICE_USER = 1;
		public const int SERVICE_PROVIDER_ACSE = 2;
		public const int SERVICE_PROVIDER_PRES = 3;
		public const int NO_REASON_GIVEN = 1;
		public const int APPLICATION_CONTEXT_NAME_NOT_SUPPORTED = 2;
		public const int CALLING_AE_TITLE_NOT_RECOGNIZED = 3;
		public const int CALLED_AE_TITLE_NOT_RECOGNIZED = 7;
		public const int PROTOCOL_VERSION_NOT_SUPPORTED = 2;
		public const int TEMPORARY_CONGESTION = 1;
		public const int LOCAL_LIMIT_EXCEEDED = 2;

		private readonly byte[] buf;
		
		internal static AAssociateRJ Parse(UnparsedPdu raw)
		{
			if (raw.length() != 4)
			{
				throw new PduException("Illegal A-ASSOCIATE-RJ " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			return new AAssociateRJ(raw.buffer());
		}
		
		public AAssociateRJ(byte[] buf)
		{
			this.buf = buf;
		}
		
		internal AAssociateRJ(int result, int source, int reason)
		{
			this.buf = new byte[]{3, 0, 0, 0, 0, 4, 0, (byte) result, (byte) source, (byte) reason};
		}
		
		/// <summary>
		/// Returns Result field value.
		/// </summary>
		/// <returns>
		/// Result field value. 
		/// </returns>
		public int result()
		{
			return buf[7] & 0xff;
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
		
		public void  WriteTo(System.IO.Stream outs)
		{
			outs.Write( buf, 0, buf.Length);
			StringUtils.dumpBytes( "AAssociateRJ", buf, 0, (int)buf.Length);
		}
		
		public String ToString(bool verbose)
		{
			return ToString();
		}
		
		public override String ToString()
		{
			return toStringBuffer(new StringBuilder()).ToString();
		}
		
		internal StringBuilder toStringBuffer(StringBuilder sb)
		{
			return sb.Append("A-ASSOCIATE-RJ\n\tresult=").Append(resultAsString()).Append("\n\tsource=").Append(sourceAsString()).Append("\n\treason=").Append(ReasonAsString());
		}
		
		private String resultAsString()
		{
			switch (result())
			{
				case REJECTED_PERMANENT: 
					return "1 - rejected-permanent";
				
				case REJECTED_TRANSIENT: 
					return "2 - rejected-transient";
				
				default: 
					return result().ToString();
				
			}
		}
		
		private String sourceAsString()
		{
			switch (source())
			{
				case SERVICE_USER: 
					return "1 - service-user";
				
				case SERVICE_PROVIDER_ACSE: 
					return "2 - service-provider (ACSE)";
				
				case SERVICE_PROVIDER_PRES: 
					return "3 - service-provider (Presentation)";
				
				default: 
					return source().ToString();
				
			}
		}
		
		private String ReasonAsString()
		{
			switch (source())
			{
				case SERVICE_USER: 
					switch (reason())
					{
						case NO_REASON_GIVEN: 
							return "1 - no-reason-given";
						
						case APPLICATION_CONTEXT_NAME_NOT_SUPPORTED: 
							return "2 - application-context-name-not-supported";
						
						case CALLING_AE_TITLE_NOT_RECOGNIZED: 
							return "3 - calling-AE-title-not-recognized";
						
						case CALLED_AE_TITLE_NOT_RECOGNIZED: 
							return "7 - called-AE-title-not-recognizedr";
						
					}
					goto case SERVICE_PROVIDER_ACSE;
				
				case SERVICE_PROVIDER_ACSE: 
					switch (reason())
					{
						case NO_REASON_GIVEN: 
							return "1 - no-reason-given";
						
						case PROTOCOL_VERSION_NOT_SUPPORTED: 
							return "2 - protocol-version-not-supported";
						
					}
					goto case SERVICE_PROVIDER_PRES;
				
				case SERVICE_PROVIDER_PRES: 
					switch (reason())
					{
						case TEMPORARY_CONGESTION: 
							return "1 - temporary-congestion";
						
						case LOCAL_LIMIT_EXCEEDED: 
							return "2 - local-limit-exceeded";
						
					}
					break;
				
			}
			return reason().ToString();
		}
	}
}