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
	using org.dicomcs.data;
	using org.dicomcs.dict;
	
	public class FileMetaInfo : DcmObject
	{
		internal static byte[] DICM_PREFIX = new byte[]{(byte) 'D', (byte) 'I', (byte) 'C', (byte) 'M'};
		
		internal static byte[] VERSION = new byte[]{0, 1};
		
		private readonly byte[] preamble = new byte[128];
		private String sopClassUID = null;
		private String sopInstanceUID = null;
		private String tsUID = null;
		private String implClassUID = null;
		private String implVersionName = null;
		
		public FileMetaInfo()
		{
		}

		public virtual byte[] Preamble
		{
			get { return preamble; }			
		}
		public virtual String MediaStorageSOPClassUID
		{
			get { return sopClassUID; }
			
		}
		public virtual String MediaStorageSOPInstanceUID
		{
			get { return sopInstanceUID; }			
		}
		public virtual String TransferSyntaxUID
		{
			get { return tsUID; }			
		}
		public virtual String ImplementationClassUID
		{
			get { return implClassUID; }			
		}
		public virtual String ImplementationVersionName
		{
			get { return implVersionName; }			
		}
		
		
		public override String ToString()
		{
			return "FileMetaInfo[uid=" + sopInstanceUID + "\n\tclass=" + UIDs.GetName(sopClassUID) + "\n\tts=" + UIDs.GetName(tsUID) + "\n\timpl=" + implClassUID + "-" + implVersionName + "]";
		}
		
		
		internal FileMetaInfo Init(String sopClassUID, String sopInstUID, String tsUID, String implClassUID, String implVersName)
		{
			byte[] generated_var = new byte[VERSION.Length];
			VERSION.CopyTo(generated_var, 0);
			PutOB(Tags.FileMetaInformationVersion, generated_var);
			PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
			PutUI(Tags.MediaStorageSOPInstanceUID, sopInstUID);
			PutUI(Tags.TransferSyntaxUID, tsUID);
			PutUI(Tags.ImplementationClassUID, implClassUID);
			if (implVersName != null)
			{
				PutSH(Tags.ImplementationVersionName, implVersName);
			}
			return this;
		}
		
		public override DcmElement Put(DcmElement newElem)
		{
			uint tag = newElem.tag();
			if ((tag & 0xFFFF0000) != 0x00020000)
			{
				throw new System.ArgumentException(newElem.ToString());
			}
			
			try
			{
				switch (tag)
				{
					case Tags.MediaStorageSOPClassUID: 
						sopClassUID = newElem.GetString(null);
						break;
					
					case Tags.MediaStorageSOPInstanceUID: 
						sopInstanceUID = newElem.GetString(null);
						break;
					
					case Tags.TransferSyntaxUID: 
						tsUID = newElem.GetString(null);
						break;
					
					case Tags.ImplementationClassUID: 
						implClassUID = newElem.GetString(null);
						break;
					
					case Tags.ImplementationVersionName: 
						implVersionName = newElem.GetString(null);
						break;
					
				}
			}
			catch (DcmValueException ex)
			{
				throw new System.ArgumentException(newElem.ToString());
			}
			return base.Put(newElem);
		}
		
		public virtual int length()
		{
			return grLen() + 12;
		}
		
		private int grLen()
		{
			int len = 0;
			 for (int i = 0, n = Size; i < n; ++i)
			{
				DcmElement e = (DcmElement) m_list[i];
				len += e.length() + (VRs.IsLengthField16Bit(e.vr())?8:12);
			}
			return len;
		}
		
		public void  Write(DcmHandlerI handler)
		{
			handler.StartFileMetaInfo(preamble);
			handler.DcmDecodeParam = DcmDecodeParam.EVR_LE;
			Write(0x00020000, grLen(), handler);
			handler.EndFileMetaInfo();
		}
		
		public void  Write(System.IO.Stream os)
		{
			Write(new DcmStreamHandler(os));
		}
	}
}