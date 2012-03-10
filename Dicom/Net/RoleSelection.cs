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
    public class RoleSelection {
        private readonly String m_asuid;
        private readonly bool m_isScp;
        private readonly bool m_isScu;

        /// <summary>Creates a new instance of RoleSelection 
        /// </summary>
        internal RoleSelection(String asuid, bool scu, bool scp) {
            m_asuid = asuid;
            m_isScu = scu;
            m_isScp = scp;
        }

        /*
		internal RoleSelection(BinaryReader din, int len)
		{
			int uidLen = din.ReadUInt16();
			if (uidLen + 4 != len)
			{
				throw new PduException("SCP/SCU role selection sub-item length: " + len + " mismatch UID-length:" + uidLen, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}
			this.m_asuid = AAssociateRQAC.ReadASCII(din, uidLen);
			this.m_isScu = din.ReadBoolean();
			this.m_isScp = din.ReadBoolean();
		}*/

        internal RoleSelection(ByteBuffer bb, int len) {
            int uidLen = bb.ReadInt16();
            if (uidLen + 4 != len) {
                throw new PduException(
                    "SCP/SCU role selection sub-item length: " + len + " mismatch UID-length:" + uidLen,
                    new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
            }
            m_asuid = bb.ReadString(uidLen);
            m_isScu = bb.ReadBoolean();
            m_isScp = bb.ReadBoolean();
        }

        public virtual String SOPClassUID {
            get { return m_asuid; }
        }

        public bool scu() {
            return m_isScu;
        }

        public bool scp() {
            return m_isScp;
        }

        internal int length() {
            return 4 + m_asuid.Length;
        }

        internal void WriteTo(ByteBuffer bb) {
            bb.Write((Byte) 0x54);
            bb.Write((Byte) 0);
            bb.Write((Int16) length());
            bb.Write((Int16) m_asuid.Length);
            bb.Write(m_asuid);
            bb.Write(m_isScu);
            bb.Write(m_isScp);
        }
    }
}