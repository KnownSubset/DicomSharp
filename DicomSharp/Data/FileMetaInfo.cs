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
using System.IO;
using DicomSharp.Dictionary;

namespace DicomSharp.Data {
    public class FileMetaInfo : DcmObject {
        internal static byte[] DICM_PREFIX = new[] {(byte) 'D', (byte) 'I', (byte) 'C', (byte) 'M'};
        internal static byte[] VERSION = new byte[] {0, 1};
        private readonly byte[] _preamble = new byte[128];
        private String _implementationClassUniqueId;
        private String _implementationVersionName;
        private String _sopClassUniqueId;
        private String _sopInstanceUniqueId;
        private String _tsUniqueId;

        public virtual byte[] Preamble {
            get { return _preamble; }
        }

        public virtual String MediaStorageSOPClassUniqueId {
            get { return _sopClassUniqueId; }
        }

        public virtual String MediaStorageSOPInstanceUniqueId {
            get { return _sopInstanceUniqueId; }
        }

        public virtual String TransferSyntaxUniqueId {
            get { return _tsUniqueId; }
        }

        public virtual String ImplementationClassUniqueId {
            get { return _implementationClassUniqueId; }
        }

        public virtual String ImplementationVersionName {
            get { return _implementationVersionName; }
        }


        public override String ToString() {
            return "FileMetaInfo[uid=" + _sopInstanceUniqueId + "\n\tclass=" + UIDs.GetName(_sopClassUniqueId) + "\n\tts=" +
                   UIDs.GetName(_tsUniqueId) + "\n\timpl=" + _implementationClassUniqueId + "-" + _implementationVersionName + "]";
        }


        internal FileMetaInfo Init(String sopClassUniqueId, String sopInstanceUniqueId, String transferSyntaxUniqueId, String implementationClassUniqueId, String implementationVersionName) {
            var generatedVar = new byte[VERSION.Length];
            VERSION.CopyTo(generatedVar, 0);
            PutOB(Tags.FileMetaInformationVersion, generatedVar);
            PutUI(Tags.MediaStorageSOPClassUniqueId, sopClassUniqueId);
            PutUI(Tags.MediaStorageSOPInstanceUID, sopInstanceUniqueId);
            PutUI(Tags.TransferSyntaxUniqueId, transferSyntaxUniqueId);
            PutUI(Tags.ImplementationClassUID, implementationClassUniqueId);
            if (implementationVersionName != null) {
                PutSH(Tags.ImplementationVersionName, implementationVersionName);
            }
            return this;
        }

        public override DcmElement Put(DcmElement newElem) {
            uint tag = newElem.tag();
            if ((tag & 0xFFFF0000) != 0x00020000) {
                throw new ArgumentException(newElem.ToString());
            }

            try {
                switch (tag) {
                    case Tags.MediaStorageSOPClassUniqueId:
                        _sopClassUniqueId = newElem.GetString(null);
                        break;

                    case Tags.MediaStorageSOPInstanceUID:
                        _sopInstanceUniqueId = newElem.GetString(null);
                        break;

                    case Tags.TransferSyntaxUniqueId:
                        _tsUniqueId = newElem.GetString(null);
                        break;

                    case Tags.ImplementationClassUID:
                        _implementationClassUniqueId = newElem.GetString(null);
                        break;

                    case Tags.ImplementationVersionName:
                        _implementationVersionName = newElem.GetString(null);
                        break;
                }
            }
            catch (DcmValueException) {
                throw new ArgumentException(newElem.ToString());
            }
            return base.Put(newElem);
        }

        public virtual int Length() {
            return grLen() + 12;
        }

        private int grLen() {
            int length = 0;
            for (int i = 0, n = Size; i < n; ++i) {
                var dcmElement = _dcmElements[i];
                length += dcmElement.Length() + (VRs.IsLengthField16Bit(dcmElement.VR()) ? 8 : 12);
            }
            return length;
        }

        public void Write(IDcmHandler handler) {
            handler.StartFileMetaInfo(_preamble);
            handler.DcmDecodeParam = DcmDecodeParam.EVR_LE;
            Write(0x00020000, grLen(), handler);
            handler.EndFileMetaInfo();
        }

        public void Write(Stream os) {
            Write(new DcmStreamHandler(os));
        }
    }
}