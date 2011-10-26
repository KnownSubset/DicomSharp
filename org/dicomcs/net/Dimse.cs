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
	using org.dicomcs.data;
	using org.dicomcs.dict;
	
	/// <summary>
	/// </summary>
	public class Dimse : DataSourceI
	{
		public virtual Command Command
		{
			get { return cmd; }
		}
		public virtual String TransferSyntaxUID
		{
			get { return tsUID; }			
			set { this.tsUID = value; }			
		}
		public virtual Dataset Dataset
		{
			get
			{
				if (ds != null)
				{
					return ds;
				}
				if (m_ins == null)
				{
					return null;
				}
				if (tsUID == null)
				{
					throw new System.SystemException();
				}
				ds = new Dataset();
				ds.ReadDataset(m_ins, DcmDecodeParam.ValueOf(tsUID), 0);
				m_ins.Close();
				m_ins = null;
				return ds;
			}
			
		}
		public virtual Stream DataAsStream
		{
			get { return m_ins; }
		}
		
		private int m_pcid;
		private Command cmd;
		private Dataset ds;
		private Stream m_ins;
		private DataSourceI src;
		private String tsUID;
		
		public Dimse(int pcid, String tsUID, Command cmd, Stream ins)
		{
			this.m_pcid = pcid;
			this.cmd = cmd;
			this.ds = null;
			this.src = null;
			this.m_ins = ins;
			this.tsUID = tsUID;
		}
		
		public Dimse(int pcid, Command cmd, Dataset ds, DataSourceI src)
		{
			this.m_pcid = pcid;
			this.cmd = cmd;
			this.ds = ds;
			this.src = src;
			this.m_ins = null;
			this.tsUID = null;
			this.cmd.PutUS(Tags.DataSetType, ds == null && src == null?Command.NO_DATASET:0);
		}
		
		public int pcid()
		{
			return m_pcid;
		}
			
		public virtual void  WriteTo(Stream outs, String tsUID)
		{
			if (src != null)
			{
				src.WriteTo(outs, tsUID);
				return ;
			}
			if (ds == null)
			{
				throw new System.SystemException("Missing Dataset");
			}
			ds.WriteDataset(outs, DcmDecodeParam.ValueOf(tsUID));
		}
		
		
		public override String ToString()
		{
			return "[pc-" + m_pcid + "] " + cmd.ToString();
		}
	}
}