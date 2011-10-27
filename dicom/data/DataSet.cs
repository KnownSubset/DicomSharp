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
	using System.IO;
	using System.Reflection;
	using System.Text;
	using org.dicomcs.data;
	using Tags = org.dicomcs.dict.Tags;
	
	/// <summary>
	/// Implementation of <code>Dataset</code> container objects.
	/// </summary>
	public class Dataset : BaseDataset
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public override String PrivateCreatorID
		{
			get
			{
				return privateCreatorID != null?privateCreatorID:parent != null?parent.PrivateCreatorID:null;
			}			
			set
			{
				this.privateCreatorID = value;
			}			
		}
		public virtual Encoding Encoding
		{
			get
			{
				return encoding != null?encoding:parent != null?parent.Encoding:null;
			}			
		}
		public virtual Dataset Parent
		{
			get
			{
				return parent;
			}			
		}				
		
		private readonly Dataset parent;
		private Encoding encoding = null;
		private String privateCreatorID = null;
		private long itemOffset = - 1L;
		
		public Dataset(): this(null)
		{
		}
		
		public Dataset(Dataset parent)
		{
			this.parent = parent;
		}

		public override long GetItemOffset()
		{
			if (itemOffset != - 1L || m_list.Count == 0)
				return itemOffset;
			
			long elm1pos = ((DcmElement) m_list[0]).StreamPosition;
			return elm1pos == - 1L?- 1L:elm1pos - 8L;
		}
		public override Dataset SetItemOffset( long itemOffset )
		{
			this.itemOffset = itemOffset;
			return this;
		}
	
		public override DcmElement PutSQ(uint tag)
		{
			return Put(new SQElement(tag, this));
		}
		
		public override DcmElement Put(DcmElement newElem)
		{
			if ((newElem.tag())>>16 < 4)
			{
				throw new System.ArgumentException(newElem.ToString());
			}
			
			if (newElem.tag() == Tags.SpecificCharacterSet)
			{
				try
				{
					//TODO: decide the encoding
					//this.encoding = Encodings.lookup(newElem.GetStrings(null));
				}
				catch (System.Exception ex)
				{
					log.Warn("Failed to consider specified Encoding!", ex);
					this.encoding = null;
				}
			}
			
			return base.Put(newElem);
		}
		
		public override DcmElement Remove(uint tag)
		{
			if (tag == Tags.SpecificCharacterSet)
				encoding = null;
			return base.Remove(tag);
		}
		
		public override void  Clear()
		{
			base.Clear();
			encoding = null;
			totLen = 0;
		}
		
		public virtual void  ReadFile(Stream ins, FileFormat format, uint stopTag)
		{
			DcmParser Parser = new DcmParser(ins);
			Parser.DcmHandler = DcmHandler;
			Parser.ParseDcmFile(format, stopTag);
		}

		public virtual void  ReadDataset(Stream ins, DcmDecodeParam param, uint stopTag)
		{
			DcmParser Parser = new DcmParser(ins);
			Parser.DcmHandler = DcmHandler;
			Parser.ParseDataset(param, stopTag);
		}		

	}
}