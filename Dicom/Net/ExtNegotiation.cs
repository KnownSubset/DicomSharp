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
using Dicom.Utility;

namespace Dicom.Net {
    /// <summary>
    /// </summary>
    public class ExtNegotiation {
        private readonly String asuid;
        private readonly byte[] m_info;

        /// <summary>
        /// Creates a new instance of ExtNegotiation 
        /// </summary>
        internal ExtNegotiation(String asuid, byte[] info) {
            this.asuid = asuid;
            m_info = new byte[info.Length];
            info.CopyTo(m_info, 0);
        }

        /*
		internal ExtNegotiation(BinaryReader din, int len)
		{
			int uidLen = din.ReadUInt16();
			this.asuid = AAssociateRQAC.ReadASCII(din, uidLen);
			this.m_info = new byte[len - uidLen - 2];
			din.BaseStream.Read( m_info, 0, m_info.Length);
		}
		*/

        internal ExtNegotiation(ByteBuffer bb, int len) {
            int uidLen = bb.ReadInt16();
            asuid = bb.ReadString(uidLen);
            m_info = new byte[len - uidLen - 2];
            bb.Read(m_info, 0, m_info.Length);
        }

        public virtual String SOPClassUID {
            get { return asuid; }
        }

        public byte[] info() {
            var tmp = new byte[m_info.Length];
            Array.Copy(m_info, 0, tmp, 0, m_info.Length);
            return tmp;
        }

        internal int length() {
            return 2 + asuid.Length + m_info.Length;
        }

        internal void WriteTo(ByteBuffer bb) {
            bb.Write((Byte) 0x56);
            bb.Write((Byte) 0);
            bb.Write((Int16) length());
            bb.Write((Int16) asuid.Length);
            bb.Write(asuid);
            bb.Write(m_info);
        }
    }
}