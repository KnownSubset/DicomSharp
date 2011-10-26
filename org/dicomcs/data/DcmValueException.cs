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
	
	/// <summary>
	/// </summary>
	public class DcmValueException:System.IO.IOException
	{
		
		/// <summary>
		/// Creates a new instance of <code>DcmValueException</code> without detail
		/// message.
		/// </summary>
		public DcmValueException()
		{
		}
		
		/// <summary>
		/// Constructs an instance of <code>DcmValueException</code> with the
		/// specified detail message.
		/// </summary>
		/// <param name="msg">the detail message.</param>
		public DcmValueException(System.String msg):base(msg)
		{
		}
		
		/// <summary>
		/// Constructs a new throwable with the specified detail message and
		/// cause.
		/// </summary>
		/// <param name="msg">the detail message.
		/// </param>
		/// <param name="cause">the cause.
		/// </param>
		public DcmValueException(System.String msg, System.Exception cause):base(msg, cause)
		{
		}
	}
}