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
	using System.Collections;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	/// <summary>
	/// DcmHandler, parsing DICOM data into Object memory block
	/// </summary>
	class DcmObjectHandler : DcmHandlerI
	{
		private DcmObject result;
		private ByteOrder byteOrder = ByteOrder.LITTLE_ENDIAN;
		private DcmObject curDcmObject;
		private uint tag;
		private int vr;
		private long pos;
		private Stack seqStack = new Stack();

		public virtual DcmDecodeParam DcmDecodeParam
		{
			set { this.byteOrder = value.byteOrder; }			
		}
	
		/// <summary>
		/// Creates a new instance of DcmHandlerImpl 
		/// </summary>
		public DcmObjectHandler(DcmObject result)
		{
			if (result == null)
				throw new NullReferenceException();
			
			this.result = result;
		}
		
		public virtual void  StartCommand()
		{
			curDcmObject = (Command) result;
			seqStack.Clear();
		}
		
		public virtual void  EndCommand()
		{
			curDcmObject = null;
		}
		
		public virtual void  StartDcmFile()
		{
			// noop
		}
		
		public virtual void  EndDcmFile()
		{
			// noop
		}
		
		public virtual void  StartFileMetaInfo(byte[] preamble)
		{
			if (result is Dataset)
			{
				curDcmObject = ((Dataset) result).GetFileMetaInfo();
				if (curDcmObject == null)
					((Dataset) result).SetFileMetaInfo( (FileMetaInfo) (curDcmObject = new FileMetaInfo()) );
			}
			else
				curDcmObject = (FileMetaInfo) result;
			seqStack.Clear();
			if (preamble != null)
			{
				if (preamble.Length == 128)
				{
					Array.Copy(preamble, 0, ((FileMetaInfo) curDcmObject).Preamble, 0, 128);
				}
				else
				{
					// log.warn
				}
			}
		}
		
		public virtual void  EndFileMetaInfo()
		{
			if (result is Dataset)
			{
				curDcmObject = result;
			}
			else
				curDcmObject = null;
		}
		
		public virtual void  StartDataset()
		{
			curDcmObject = (Dataset) result;
			seqStack.Clear();
		}
		
		public virtual void  EndDataset()
		{
			curDcmObject = null;
		}
		
		
		public virtual void  StartElement(uint tag, int vr, long pos)
		{
			this.tag = tag;
			this.vr = vr;
			this.pos = pos;
		}
		
		public virtual void  EndElement()
		{
		}
		
		public virtual void  StartSequence(int length)
		{
			seqStack.Push(vr == VRs.SQ?curDcmObject.PutSQ(tag):curDcmObject.PutXXsq(tag, vr));
		}
		
		public virtual void  EndSequence(int length)
		{
			seqStack.Pop();
		}

		public virtual void  Value(dicomcs.util.ByteBuffer bb)
		{
			DcmElement elm = curDcmObject.PutXX(tag, vr, bb);
			elm.StreamPosition = pos;
		}
		
		public virtual void  Value(byte[] data, int Start, int length)
		{
			ByteBuffer buf = ByteBuffer.Wrap(data, Start, length, byteOrder);
			DcmElement elm = curDcmObject.PutXX(tag, vr, buf);
			elm.StreamPosition = pos;
		}
		
		public virtual void  Fragment(int id, long pos, byte[] data, int Start, int length)
		{
			((DcmElement) seqStack.Peek()).AddDataFragment(ByteBuffer.Wrap(data, Start, length, byteOrder));
		}
		
		public virtual void  StartItem(int id, long pos, int length)
		{
			curDcmObject = ((DcmElement) seqStack.Peek()).AddNewItem().SetItemOffset( pos );
		}
		
		public virtual void  EndItem(int len)
		{
			curDcmObject = ((Dataset) curDcmObject).Parent;
		}
	}
}