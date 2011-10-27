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

namespace org.dicomcs.util
{
	using System;
	using org.dicomcs;
	
	public class UIDGenerator
	{
		public static UIDGenerator Instance
		{
			get
			{
				return new UIDGenerator();
			}			
		}

		static UIDGenerator()
		{
			{
				System.String tmp;
				try
				{
					tmp = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList[0].ToString();
				}
				catch (System.Exception e)
				{
					tmp = "127.0.0.1";
				}
				IP = tmp;
			}
		}
		
		private static System.String IP;
		
		
		/// <summary>
		/// Creates a new instance of UIDGenerator
		/// </summary>
		private UIDGenerator()
		{
		}
		
		public virtual String createUID()
		{
			return createUID(Implementation.ClassUID);
		}
		
		public virtual String createUID(System.String root)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(64).Append(root).Append('.');
			sb.Append( IP.Replace( ".", "" ) );
			String str = DateTime.Now.ToString( "yyyyMMddHHmmssffffff" );
			sb.Append( str );
			return sb.ToString();
		}

		public static void Main()
		{
			String uid = UIDGenerator.Instance.createUID();
			Console.Write( uid );
		}
	}
}