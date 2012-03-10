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
using DicomSharp.Utility;

namespace DicomSharp.Net {
    /// <summary>
    /// 
    /// </summary>
    public class AsyncOpsWindow {
        internal static AsyncOpsWindow DEFAULT = new AsyncOpsWindow(1, 1);
        private readonly int maxOpsInvoked;
        private readonly int maxOpsPerformed;

        /// <summary>
        /// Creates a new instance of AsyncOpsWindow 
        /// </summary>
        internal AsyncOpsWindow(int maxOpsInvoked, int maxOpsPerformed) {
            this.maxOpsInvoked = maxOpsInvoked;
            this.maxOpsPerformed = maxOpsPerformed;
        }

        internal AsyncOpsWindow(ByteBuffer bb, int len) {
            if (len != 4) {
                throw new PduException("Illegal length of AsyncOpsWindow sub-item: " + len,
                                       new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
            }
            maxOpsInvoked = bb.ReadInt16();
            maxOpsPerformed = bb.ReadInt16();
        }

        public virtual int MaxOpsInvoked {
            get { return maxOpsInvoked; }
        }

        public virtual int MaxOpsPerformed {
            get { return maxOpsPerformed; }
        }


        internal void WriteTo(ByteBuffer bb) {
            bb.Write((Byte) 0x53);
            bb.Write((Byte) 0);
            bb.Write((Int16) 4);
            bb.Write((Int16) maxOpsInvoked);
            bb.Write((Int16) maxOpsPerformed);
        }

        public override String ToString() {
            return "AsyncOpsWindow[maxOpsInvoked=" + maxOpsInvoked + ",maxOpsPerformed=" + maxOpsPerformed + "]";
        }
    }
}