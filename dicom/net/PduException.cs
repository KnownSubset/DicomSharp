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
	
	/// <summary>
	/// </summary>
	public class PduException : IOException
	{
		public virtual AAbort AAbort
		{
			get
			{
				return abort;
			}			
		}
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'abort '. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1003"'
		private AAbort abort;
		
		/// <summary> 
		/// Constructs an instance of <code>PduException</code> with the
		/// specified detail message and corresponding A-Abort Pdu.
		/// </summary>
		/// <param name="msg">
		/// the detail message.
		/// </param>
		/// <param name="abort">
		/// corresponding A-Abort Pdu.
		/// </param>
		public PduException(String msg, AAbort abort):base(msg)
		{
			this.abort = abort;
		}
		
		/// <summary> 
		/// Constructs a new throwable with the specified detail message and
		/// cause and corresponding A-Abort Pdu.
		/// </summary>
		/// <param name="msg">the detail message.
		/// </param>
		/// <param name="cause">the cause.
		/// </param>
		/// <param name="abort">corresponding A-Abort Pdu.
		/// 
		/// </param>
		//UPGRADE_NOTE: Exception 'java.lang.Throwable' was converted to ' ' which has different behavior. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1100"'
		public PduException(String msg, Exception cause, AAbort abort):base(msg, cause)
		{
			this.abort = abort;
		}
		
		/// <summary> 
		/// Returns corresponding A-Abort Pdu.
		/// </summary>
		/// <returns>corresponding A-Abort Pdu.
		/// </returns>
	}
}