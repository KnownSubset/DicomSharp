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
	using System.Collections;
	using org.dicomcs.dict;
	
	/// <summary>
	/// *
	/// </summary>
	public class AAssociateRQ : AAssociateRQAC
	{
		
		public static AAssociateRQ Parse(UnparsedPdu raw)
		{
			return (AAssociateRQ) new AAssociateRQ().Init(raw);
		}
		
		public AAssociateRQ()
		{
		}
		
		protected override int type()
		{
			return 1;
		}
		
		protected override int pctype()
		{
			return 0x20;
		}
		
		protected override String TypeAsString()
		{
			return "AAssociateRQ";
		}
		
		protected override void  Append(PresContext pc, System.Text.StringBuilder sb)
		{
			sb.Append("\n\tpc-").Append(pc.pcid()).Append(":\tas=").Append(UIDs.GetName(pc.AbstractSyntaxUID));
			for( IEnumerator enu = pc.TransferSyntaxUIDs.GetEnumerator(); enu.MoveNext(); )
				sb.Append("\n\t\tts=").Append(UIDs.GetName((String) enu.Current));
		}
		
		protected override void  AppendPresCtxSummary(System.Text.StringBuilder sb)
		{
			sb.Append("\n\tpresCtx:\toffered=").Append(presCtxs.Count);
		}
	}
}