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
	using org.dicomcs.data;
	using Tags = org.dicomcs.dict.Tags;
	
	/// <summary>
	/// Dicom data object factory
	/// </summary>
	public class DcmObjectFactory
	{
		private static DcmObjectFactory s_instance = new DcmObjectFactory();

		public static DcmObjectFactory Instance
		{
			get{ return s_instance; }			
		}

		/// <summary>
		/// Creates a new instance of DcmParserFactory
		/// </summary>
		private DcmObjectFactory()
		{
		}
		
		public virtual Command NewCommand()
		{
			return new Command();
		}
		
		public virtual Dataset NewDataset()
		{
			return new Dataset();
		}
		
		public virtual FileMetaInfo NewFileMetaInfo(System.String sopClassUID, System.String sopInstanceUID, System.String transferSyntaxUID, System.String ClassUID, System.String VersName)
		{
			return new FileMetaInfo().Init(sopClassUID, sopInstanceUID, transferSyntaxUID, ClassUID, VersName);
		}
		
		public virtual FileMetaInfo NewFileMetaInfo(System.String sopClassUID, System.String sopInstanceUID, System.String transferSyntaxUID)
		{
			return new FileMetaInfo().Init(sopClassUID, sopInstanceUID, transferSyntaxUID, Implementation.ClassUID, Implementation.VersionName);
		}
		
		public virtual PersonName NewPersonName(System.String s)
		{
			return new PersonName(s);
		}
		
		public virtual FileMetaInfo NewFileMetaInfo(Dataset ds, System.String transferSyntaxUID)
		{
			try
			{
				return new FileMetaInfo().Init(ds.GetString(Tags.SOPClassUID, null), ds.GetString(Tags.SOPInstanceUID, null), transferSyntaxUID, Implementation.ClassUID, Implementation.VersionName);
			}
			catch (DcmValueException ex)
			{
				throw new System.ArgumentException(ex.Message);
			}
		}
	}
}