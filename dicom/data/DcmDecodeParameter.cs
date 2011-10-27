#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2008 Fang Yang. All rights reserved.
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
// 7/31/08: Edited by Maarten JB van Ettinger. Changes strings to use UIDs from UIDs class (added line 32, changed lines 73, 75, 77, 79).
#endregion

namespace org.dicomcs.data
{
	using System;
	using org.dicomcs.util;
	using org.dicomcs.dict;
	

	public class DcmDecodeParam
	{
		public readonly ByteOrder byteOrder;
		
		public readonly bool explicitVR;
		
		public readonly bool deflated;
		
		public readonly bool encapsulated;
		
		public DcmDecodeParam(ByteOrder byteOrder, bool explicitVR, bool deflated, bool encapsulated)
		{
			if (byteOrder == null)
				throw new NullReferenceException();
			this.byteOrder = byteOrder;
			this.explicitVR = explicitVR;
			this.deflated = deflated;
			this.encapsulated = encapsulated;
		}
		
		public override String ToString()
		{
			return (explicitVR?"explVR-":"implVR-") + byteOrder.ToString() + (deflated?" deflated":"") + (encapsulated?" encapsulated":"");
		}
		
		public readonly static DcmEncodeParam IVR_LE = new DcmEncodeParam(ByteOrder.LITTLE_ENDIAN, false, false, false, false, false, false);
		
		public readonly static DcmEncodeParam IVR_BE = new DcmEncodeParam(ByteOrder.BIG_ENDIAN, false, false, false, true, true, true);
		
		public readonly static DcmEncodeParam EVR_LE = new DcmEncodeParam(ByteOrder.LITTLE_ENDIAN, true, false, false, true, true, true);
		
		public readonly static DcmEncodeParam EVR_BE = new DcmEncodeParam(ByteOrder.BIG_ENDIAN, true, false, false, true, true, true);
		
		public readonly static DcmEncodeParam DEFL_EVR_LE = new DcmEncodeParam(ByteOrder.LITTLE_ENDIAN, true, true, false, true, true, true);
		
		public readonly static DcmEncodeParam ENCAPS_EVR_LE = new DcmEncodeParam(ByteOrder.LITTLE_ENDIAN, true, false, true, true, true, true);
		
		public static DcmEncodeParam ValueOf(String tsuid)
		{
			if (UIDs.ImplicitVRLittleEndian.Equals(tsuid))
				return IVR_LE;
			if (UIDs.ExplicitVRLittleEndian.Equals(tsuid))
				return EVR_LE;
			if (UIDs.DeflatedExplicitVRLittleEndian.Equals(tsuid))
				return DEFL_EVR_LE;
			if (UIDs.ExplicitVRBigEndian.Equals(tsuid))
				return EVR_BE;
			
			return ENCAPS_EVR_LE;
		}
	}
}