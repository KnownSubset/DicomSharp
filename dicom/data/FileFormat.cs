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

namespace org.dicomcs.data
{
	using System;
	
	public class FileFormat
	{
		
		public bool hasPreamble;

		public bool hasFileMetaInfo;
		
		public DcmDecodeParam decodeParam;
		
		public FileFormat(bool hasPreamble, bool hasFileMetaInfo, DcmDecodeParam decodeParam)
		{
			if (hasPreamble && !hasFileMetaInfo)
			{
				throw new System.ArgumentException("Preamble without FMI");
			}
			this.hasPreamble = hasPreamble;
			this.hasFileMetaInfo = hasFileMetaInfo;
			this.decodeParam = decodeParam;
		}
		
		public override System.String ToString()
		{
			return "FileFormat[" + (hasFileMetaInfo?(hasPreamble?"Part 10,":"FMI without preamble,"):"Stream, ") + decodeParam.ToString() + "]";
		}
		
		public static FileFormat DICOM_FILE = new FileFormat(true, true, DcmDecodeParam.EVR_LE);
		public static FileFormat DICOM_FILE_WO_PREAMBLE = new FileFormat(false, true, DcmDecodeParam.EVR_LE);
		public static FileFormat EVR_LE_STREAM = new FileFormat(false, false, DcmDecodeParam.EVR_LE);
		public static FileFormat EVR_BE_FILE = new FileFormat(true, true, DcmDecodeParam.EVR_BE);
		public static FileFormat EVR_BE_FILE_WO_PREAMBLE = new FileFormat(false, true, DcmDecodeParam.EVR_BE);
		public static FileFormat EVR_BE_STREAM = new FileFormat(false, false, DcmDecodeParam.EVR_BE);
		public static FileFormat IVR_BE_FILE = new FileFormat(true, true, DcmDecodeParam.IVR_BE);
		public static FileFormat IVR_BE_FILE_WO_PREAMBLE = new FileFormat(false, true, DcmDecodeParam.IVR_BE);
		public static FileFormat IVR_BE_STREAM = new FileFormat(false, false, DcmDecodeParam.IVR_BE);
		public static FileFormat IVR_LE_FILE = new FileFormat(true, true, DcmDecodeParam.IVR_LE);
		public static FileFormat IVR_LE_FILE_WO_PREAMBLE = new FileFormat(false, true, DcmDecodeParam.IVR_LE);
		public static FileFormat ACRNEMA_STREAM = new FileFormat(false, false, DcmDecodeParam.IVR_LE);
	}
}