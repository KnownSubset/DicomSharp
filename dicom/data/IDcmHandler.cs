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
#endregion

namespace org.dicomcs.data
{
	using System;
	
	public interface DcmHandlerI
	{
		DcmDecodeParam DcmDecodeParam
		{
			set;			
		}

		void  StartCommand();
		void  EndCommand();
		void  StartDcmFile();
		void  EndDcmFile();
		void  StartFileMetaInfo(byte[] preamble);
		void  EndFileMetaInfo();
		void  StartDataset();
		void  EndDataset();
		void  StartElement(uint tag, int vr, long pos);
		void  EndElement();
		void  StartSequence(int length);
		void  EndSequence(int length);
		void  StartItem(int id, long pos, int length);
		void  EndItem(int len);
		void  Value(dicomcs.util.ByteBuffer bb);
		void  Value(byte[] data, int Start, int length);
		void  Fragment(int id, long pos, byte[] data, int Start, int length);
	}
}