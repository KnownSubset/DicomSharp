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
	public sealed class AReleaseRQ : PduI
	{
		private readonly static AReleaseRQ s_instance = new AReleaseRQ();

		public static AReleaseRQ Instance
		{
			get { return s_instance; }			
		}
		
		public AReleaseRQ()
		{
		}
				
		public static AReleaseRQ Parse(UnparsedPdu raw)
		{
			if (raw.length() != 4)
			{
				throw new PduException("Illegal A-RELEASE-RP " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			return s_instance;
		}
		
		private static byte[] BYTES = new byte[]{5, 0, 0, 0, 0, 4, 0, 0, 0, 0};
		
		public void  WriteTo(Stream outs)
		{
			outs.Write(BYTES, 0, BYTES.Length);
			StringUtils.dumpBytes( "AReleaseRQ", BYTES, 0, BYTES.Length);
		}
		
		public String ToString(bool verbose)
		{
			return ToString();
		}
		
		public override String ToString()
		{
			return "A-RELEASE-RQ";
		}
	}
}