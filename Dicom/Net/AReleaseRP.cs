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
using Dicom.Utility;

namespace Dicom.Net {
    /// <summary>
    /// 
    /// </summary>
    public sealed class AReleaseRP : PduI {
        private static readonly AReleaseRP instance = new AReleaseRP();

        private static readonly byte[] BYTES = new byte[] {6, 0, 0, 0, 0, 4, 0, 0, 0, 0};

        public static AReleaseRP Instance {
            get { return instance; }
        }

        #region PduI Members

        public void WriteTo(Stream outs) {
            outs.Write(BYTES, 0, BYTES.Length);
            StringUtils.dumpBytes("AReleaseRP", BYTES, 0, BYTES.Length);
        }

        public String ToString(bool verbose) {
            return ToString();
        }

        #endregion

        public static AReleaseRP Parse(UnparsedPdu raw) {
            if (raw.length() != 4) {
                throw new PduException("Illegal A-RELEASE-RQ " + raw,
                                       new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
            }
            return instance;
        }

        public override String ToString() {
            return "A-RELEASE-RP";
        }
    }
}