/*$Id: FilterSQElement.cs,v 1.1 2003/02/02 22:06:44 fyang Exp $*/
/// <summary>**************************************************************************
/// *
/// Copyright (c) 2002 by TIANI MEDGRAPH AG <gunter.zeilinger@tiani.com>     *
/// *
/// This file is part of dicomcs.                                            *
/// *
/// This library is free software; you can redistribute it and/or modify it  *
/// under the terms of the GNU Lesser General Public License as published    *
/// by the Free Software Foundation; either version 2 of the License, or     *
/// (at your option) any later version.                                      *
/// *
/// This library is distributed in the hope that it will be useful, but      *
/// WITHOUT ANY WARRANTY; without even the implied warranty of               *
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU        *
/// Lesser General Public License for more details.                          *
/// *
/// You should have received a copy of the GNU Lesser General Public         *
/// License along with this library; if not, write to the Free Software      *
/// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA  *
/// *
/// ***************************************************************************
/// </summary>
namespace org.dicomcs.data
{
	using System;
	using DcmEncodeParam = org.dicomcs.data.DcmEncodeParam;
	using VRs = org.dicomcs.dict.VRs;
	
	/// <summary>
	/// </summary>
	public class FilterSQElement : DcmElement
	{
		private SQElement sqElem;
		private Dataset filter;
		private int totlen = - 1;
		
		/// <summary>
		/// Creates a new instance of ElementImpl 
		/// </summary>
		public FilterSQElement(SQElement sqElem, Dataset filter):base(sqElem.tag())
		{
			this.sqElem = sqElem;
			this.filter = filter;
		}
		
		public override int vr()
		{
			return VRs.SQ;
		}
		
		public override int vm()
		{
			return sqElem.vm();
		}
		
		public override Dataset getItem(int index)
		{
			return new FilterDataset.Selection(sqElem.getItem(index), filter);
		}
		
		public virtual int calcLength(DcmEncodeParam param)
		{
			totlen = param.undefSeqLen?8:0;
			 for (int i = 0, n = vm(); i < n; ++i)
				totlen += getItem(i).calcLength(param) + (param.undefItemLen?16:8);
			return totlen;
		}
		
		public override int length()
		{
			return totlen;
		}
	}
}