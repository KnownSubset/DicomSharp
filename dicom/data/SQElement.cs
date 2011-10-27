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
	using System.Text;
	using System.Collections;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	
	/// <summary>
	/// </summary>
	public class SQElement : DcmElement
	{		
		private ArrayList m_list = new ArrayList();
		private Dataset parent;
		private int totlen = - 1;
		
		/// <summary>
		/// Creates a new instance of ElementImpl 
		/// </summary>
		public SQElement(uint tag, Dataset parent):base(tag)
		{
			this.parent = parent;
		}
		
		public override int vr()
		{
			return VRs.SQ;
		}
		
		public override int vm()
		{
			return m_list.Count;
		}
		
		public override bool HasItems()
		{
			return true;
		}
		
		public override Dataset GetItem(int index)
		{
			if (index >= vm())
			{
				return null;
			}
			return (Dataset) m_list[index];
		}
		
		public override void  AddItem(Dataset item)
		{
			m_list.Add(item);
		}
		
		public override Dataset AddNewItem()
		{
			Dataset item = new Dataset(parent);
			m_list.Add(item);
			return item;
		}
		
		public virtual int CalcLength(DcmEncodeParam param)
		{
			totlen = param.undefSeqLen?8:0;
			 for (int i = 0, n = vm(); i < n; ++i)
				totlen += GetItem(i).CalcLength(param) + (param.undefItemLen?16:8);
			return totlen;
		}
		
		public override int length()
		{
			return totlen;
		}
		
		public override System.String ToString()
		{
			StringBuilder sb = new StringBuilder( org.dicomcs.dict.Tags.ToHexString(tag()));
			sb.Append(",SQ");
			if (!IsEmpty())
			{
				for (int i = 0, n = vm(); i < n; ++i)
				{
					sb.Append("\n\tItem-").Append(i + 1).Append(GetItem(i));
				}
			}
			return sb.ToString();
		}
	}
}