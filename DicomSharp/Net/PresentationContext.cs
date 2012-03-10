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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DicomSharp.Dictionary;
using DicomSharp.Utility;

namespace DicomSharp.Net {
    /// <summary>
    /// </summary>
    public class PresentationContext {
        public const int ACCEPTANCE = 0;
        public const int USER_REJECTION = 1;
        public const int NO_REASON_GIVEN = 2;
        public const int ABSTRACT_SYNTAX_NOT_SUPPORTED = 3;
        public const int TRANSFER_SYNTAXES_NOT_SUPPORTED = 4;
        private readonly String m_asuid;

        private readonly int m_pcid;
        private readonly int m_result;
        private readonly IList<string> _transferSyntaxUniqueIds = new List<string>();
        private readonly int m_type;

        public PresentationContext(int type, int pcid, int result, String asuid, String[] tsuids) {
            if ((m_pcid | 1) == 0 || (m_pcid & ~0xff) != 0) {
                throw new ArgumentException("pcid=" + pcid);
            }
            if (tsuids.Length == 0) {
                throw new ArgumentException("Missing TransferSyntax");
            }
            m_type = type;
            m_pcid = pcid;
            m_result = result;
            m_asuid = asuid;
            _transferSyntaxUniqueIds = new List<string>(tsuids);
        }

        /// <summary>
        /// Constructor 2
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bb"></param>
        /// <param name="len"></param>
        public PresentationContext(int type, ByteBuffer bb, int len) {
            m_type = type;
            m_pcid = bb.ReadByte();
            bb.Skip();
            m_result = bb.ReadByte();
            bb.Skip();
            int remain = len - 4;
            _transferSyntaxUniqueIds = new List<string>();
            while (remain > 0) {
                int uidtype = bb.ReadByte();
                bb.Skip();
                int uidlen = bb.ReadInt16();
                switch (uidtype) {
                    case 0x30:
                        if (type == 0x21 || m_asuid != null) {
                            throw new PduException("Unexpected Abstract Syntax sub-item in" + " Presentation Context",
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
                        }
                        m_asuid = bb.ReadString(uidlen);
                        break;

                    case 0x40:
                        if (type == 0x21 && _transferSyntaxUniqueIds.Count > 0) {
                            throw new PduException("Unexpected Transfer Syntax sub-item in" + " Presentation Context",
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
                        }
                        String tsuid = bb.ReadString(uidlen);
                        _transferSyntaxUniqueIds.Add(tsuid);
                        break;

                    default:
                        throw new PduException("unrecognized item type " + Convert.ToString(uidtype, 16) + 'H',
                                               new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU_PARAMETER));
                }
                remain -= 4 + uidlen;
            }
            if (remain < 0) {
                throw new PduException("Presentation item Length: " + len + " mismatch Length of sub-items",
                                       new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
            }
        }


        public virtual String AbstractSyntaxUID {
            get { return m_asuid; }
        }

        public virtual IList<string> TransferSyntaxUIDs {
            get { return _transferSyntaxUniqueIds; }
        }

        public virtual String TransferSyntaxUID {
            get { return _transferSyntaxUniqueIds[0]; }
        }

        internal void WriteTo(ByteBuffer byteBuffer) {
            byteBuffer.Write((Byte) m_type);
            byteBuffer.Write((Byte) 0);
            byteBuffer.Write((Int16) length());
            byteBuffer.Write((Byte) m_pcid);
            byteBuffer.Write((Byte) 0);
            byteBuffer.Write((Byte) m_result);
            byteBuffer.Write((Byte) 0);
            if (m_asuid != null) {
                byteBuffer.Write((Byte) 0x30);
                byteBuffer.Write((Byte) 0);
                byteBuffer.Write((Int16) m_asuid.Length);
                byteBuffer.Write(m_asuid);
            }

            IEnumerator enu = _transferSyntaxUniqueIds.GetEnumerator();
            while (enu.MoveNext()) {
                var tsuid = (String) enu.Current;
                byteBuffer.Write((Byte) 0x40);
                byteBuffer.Write((Byte) 0);
                byteBuffer.Write((Int16) tsuid.Length);
                byteBuffer.Write(tsuid);
            }
        }

        public int length() {
            int retval = 4;
            if (m_asuid != null) {
                retval += 4 + m_asuid.Length;
            }
            IEnumerator enu = _transferSyntaxUniqueIds.GetEnumerator();
            while (enu.MoveNext()) {
                retval += 4 + ((String) enu.Current).Length;
            }
            return retval;
        }

        public int type() {
            return m_type;
        }

        public int pcid() {
            return m_pcid;
        }

        public int result() {
            return m_result;
        }

        public override String ToString() {
            var sb = new StringBuilder();
            sb.Append("PresentationContext[m_pcid=").Append(m_pcid);
            if (m_type == 0x20) {
                sb.Append(", as=").Append(UIDs.GetName(m_asuid));
            }
            else {
                sb.Append(", m_result=").Append(ResultAsString());
            }
            IEnumerator enu = _transferSyntaxUniqueIds.GetEnumerator();
            enu.MoveNext();
            sb.Append(", ts=").Append(UIDs.GetName((String) enu.Current));
            while (enu.MoveNext()) {
                sb.Append(", ").Append(UIDs.GetName((String) enu.Current));
            }
            sb.Append("]");
            return sb.ToString();
        }

        public String ResultAsString() {
            switch (m_result) {
                case ACCEPTANCE:
                    return "0 - acceptance";

                case USER_REJECTION:
                    return "1 - user-rejection";

                case NO_REASON_GIVEN:
                    return "2 - no-reason-given";

                case ABSTRACT_SYNTAX_NOT_SUPPORTED:
                    return "3 - abstract-syntax-not-supported";

                case TRANSFER_SYNTAXES_NOT_SUPPORTED:
                    return "4 - transfer-syntaxes-not-supported";

                default:
                    return m_result.ToString();
            }
        }
    }
}