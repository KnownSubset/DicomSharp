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

namespace Dicom.Net
{
	using System;
	using System.Collections;
	using Dicom.Dictionary;
	
	/// <summary>
	/// 
	/// </summary>
	public class AAssociateAC : AAssociateRQAC
	{
		
		internal static AAssociateAC Parse(UnparsedPdu raw)
		{
			return (AAssociateAC) new AAssociateAC().Init(raw);
		}
		
		internal AAssociateAC()
		{
		}
		
		public int countAcceptedPresContext()
		{
			int accepted = 0;
			for (IEnumerator enu = presCtxs.Values.GetEnumerator(); enu.MoveNext(); )
			{
				if (((PresContext) enu.Current).result() == 0)
					++accepted;
			}
			return accepted;
		}
		
		
		protected override int type()
		{
			return 2;
		}
		
		protected override int pctype()
		{
			return 0x21;
		}
		
		protected override String TypeAsString()
		{
			return "AAssociateAC";
		}
		
		protected override void  Append(PresContext pc, System.Text.StringBuilder sb)
		{
			sb.Append("\n\tpc-").Append(pc.pcid()).Append(":\t").Append(pc.ResultAsString()).Append("\n\t\tts=").Append(UIDs.GetName(pc.TransferSyntaxUID));
		}
		
		protected override void  AppendPresCtxSummary(System.Text.StringBuilder sb)
		{
			int accepted = countAcceptedPresContext();
			sb.Append("\n\tpresCtx:\taccepted=").Append(accepted).Append(", rejected=").Append(presCtxs.Count- accepted);
		}
	}
}