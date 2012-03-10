#region Copyright

// 
// This library is based on Dicom# see http://sourceforge.net/projects/dicom-cs/
// Copyright (C) 2002 Fang Yang. All rights reserved.
// That library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2012 Nathan Dauber. All rights reserved.
// 
// This file is part of dicomSharp, see https://github.com/KnownSubset/DicomSharp
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
// Nathan Dauber (nathan.dauber@gmail.com)
//

#endregion

using System;
using DicomSharp;
using DicomSharp.Dictionary;

namespace DicomSharp.Data {
    /// <summary>
    /// Dicom data object factory
    /// </summary>
    public class DcmObjectFactory {
        private static readonly DcmObjectFactory s_instance = new DcmObjectFactory();

        /// <summary>
        /// Creates a new instance of DcmParserFactory
        /// </summary>
        private DcmObjectFactory() {}

        public static DcmObjectFactory Instance {
            get { return s_instance; }
        }

        public virtual DicomCommand NewCommand() {
            return new DicomCommand();
        }

        public virtual DataSet NewDataset() {
            return new DataSet();
        }

        public virtual FileMetaInfo NewFileMetaInfo(String sopClassUID, String sopInstanceUID, String transferSyntaxUID,
                                                    String ClassUID, String VersName) {
            return new FileMetaInfo().Init(sopClassUID, sopInstanceUID, transferSyntaxUID, ClassUID, VersName);
        }

        public virtual FileMetaInfo NewFileMetaInfo(String sopClassUID, String sopInstanceUID, String transferSyntaxUID) {
            return new FileMetaInfo().Init(sopClassUID, sopInstanceUID, transferSyntaxUID, Implementation.ClassUID,
                                           Implementation.VersionName);
        }

        public virtual PersonName NewPersonName(String s) {
            return new PersonName(s);
        }

        public virtual FileMetaInfo NewFileMetaInfo(DataSet ds, String transferSyntaxUID) {
            try {
                return new FileMetaInfo().Init(ds.GetString(Tags.SOPClassUniqueId, null),
                                               ds.GetString(Tags.SOPInstanceUniqueId, null), transferSyntaxUID,
                                               Implementation.ClassUID, Implementation.VersionName);
            }
            catch (DcmValueException ex) {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}