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
using System.Reflection;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {
    /// <summary>
    /// </summary>
    public class UnparsedPdu {
        internal const long MAX_LENGTH = 1048576L; // 1 MB
        private readonly byte[] _buffer;
        private readonly int _length;
        private readonly int _type;

        /// <summary>
        /// Creates a new instance of RawPdu 
        /// </summary>
        public UnparsedPdu(Stream ins, byte[] buffer) {
            if (buffer == null || buffer.Length < 6) {
                buffer = new byte[10];
            }
            ReadFully(ins, buffer, 0, 6);
            _type = buffer[0] & 0xFF;
            _length = ((buffer[2] & 0xff) << 24) | ((buffer[3] & 0xff) << 16) | ((buffer[4] & 0xff) << 8) | ((buffer[5] & 0xff) << 0);
            if ((_length & 0xFFFFFFFF) > MAX_LENGTH) {
                SkipFully(ins, _length & 0xFFFFFFFFL);
                _buffer = null;
                return;
            }
            if (buffer.Length < 6 + _length) {
                _buffer = new byte[6 + _length];
                Array.Copy(buffer, 0, _buffer, 0, 6);
            }
            else {
                _buffer = buffer;
            }

            //ins.Read( this._buffer, 6, _length);
            ReadFully(ins, _buffer, 6, _length);
        }

        public new int GetType() {
            return _type;
        }

        public int Length() {
            return _length;
        }

        public byte[] Buffer() {
            return _buffer;
        }

        public override String ToString() {
            return "Pdu[type=" + _type + ", Length=" + (_length & 0xFFFFFFFFL) + "]";
        }

        internal static void SkipFully(Stream ins, long len) {
            long n = 0;
            while (n < len) {
                Int64 pos = ins.Position;
                pos = ins.Seek(len - n, SeekOrigin.Current) - pos;
                long count = pos;
                if (count < 0) {
                    throw new EndOfStreamException();
                }
                n += count;
            }
        }

        internal static void ReadFully(Stream ins, byte[] b, int off, int len) {
            int n = 0;

            while (n < len) {
                int count = ins.Read(b, off + n, len - n);
                if (count < 0) {
                    throw new EndOfStreamException();
                }
                n += count;
            }

            StringUtils.dumpBytes("UnparsedPdu", b, off, n);
        }
    }
}