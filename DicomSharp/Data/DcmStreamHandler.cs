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

using System.IO;
using DicomSharp.Dictionary;
using DicomSharp.Utility;

namespace DicomSharp.Data {
    /// <summary>
    /// Stream format of DICOM data
    /// </summary>
    internal class DcmStreamHandler : IDcmHandler {
        private const uint ITEM_TAG = 0xFFFEE000;
        private const uint ITEM_DELIMITATION_ITEM_TAG = 0xFFFEE00D;
        private const uint SEQ_DELIMITATION_ITEM_TAG = 0xFFFEE0DD;

        private readonly byte[] b12 = new byte[12];
        private readonly ByteBuffer bb12;
        private readonly BinaryWriter os;
        private bool explicitVR;
        private uint tag;
        private int vr;

        /// <summary>
        /// Creates a new instance of DcmStreamHandlerImpl 
        /// </summary>
        public DcmStreamHandler(Stream os) {
            bb12 = ByteBuffer.Wrap(b12, ByteOrder.LittleEndian);
            this.os = new BinaryWriter(os);
        }

        #region IDcmHandler Members

        public virtual DcmDecodeParam DcmDecodeParam {
            set {
                bb12.SetOrder(value.byteOrder);
                explicitVR = value.explicitVR;
            }
        }

        public virtual void StartCommand() {
            // noop
        }

        public virtual void EndCommand() {
            // noop
        }

        public virtual void StartDcmFile() {
            // noop
        }

        public virtual void EndDcmFile() {
            // noop
        }

        public virtual void StartFileMetaInfo(byte[] preamble) {
            if (preamble != null) {
                os.Write(preamble, 0, 128);
                os.Write(FileMetaInfo.DICM_PREFIX, 0, 4);
            }
        }

        public virtual void EndFileMetaInfo() {
            // noop
        }

        public virtual void StartDataSet() {
            // noop
        }

        public virtual void EndDataSet() {
            // noop
        }

        /*    
		public final void setByteOrder(ByteOrder byteOrder) {
		bb12.order(byteOrder);
		}
		
		public final void setExplicitVR(boolean explicitVR) {
		this.explicitVR = explicitVR;
		}*/

        public virtual void StartElement(uint tag, int vr, long pos) {
            this.tag = tag;
            this.vr = vr;
        }

        public virtual void EndElement() {}

        public virtual void StartSequence(int len) {
            WriteHeader(tag, vr, len);
        }

        public virtual void EndSequence(int len) {
            if (len == - 1) {
                WriteHeader(SEQ_DELIMITATION_ITEM_TAG, VRs.NONE, 0);
            }
        }

        public virtual void StartItem(int id, long pos, int len) {
            WriteHeader(ITEM_TAG, VRs.NONE, len);
        }

        public virtual void EndItem(int len) {
            if (len == - 1) {
                WriteHeader(ITEM_DELIMITATION_ITEM_TAG, VRs.NONE, 0);
            }
        }

        public void Value(ByteBuffer bb) {
            int length = (int) bb.Length;

            WriteHeader(tag, vr, (length + 1) & (~1));

            long tempPos = bb.Position;
            bb.WriteTo(os.BaseStream);
            bb.Position = tempPos;

            if ((length & 1) != 0) {
                os.Write((byte) VRs.GetPadding(vr));
            }
        }

        public virtual void Value(byte[] data, int Start, int length) {
            WriteHeader(tag, vr, (length + 1) & (~ 1));
            os.Write(data, Start, length);
            if ((length & 1) != 0) {
                os.Write((byte) VRs.GetPadding(vr));
            }
        }

        public virtual void Fragment(int id, long pos, byte[] data, int Start, int length) {
            WriteHeader(ITEM_TAG, VRs.NONE, (length + 1) & (~ 1));
            os.Write(data, Start, length);
            if ((length & 1) != 0) {
                os.Write((byte) 0);
            }
        }

        #endregion

        public virtual int WriteHeader(uint tag, int vr, int len) {
            bb12.Clear();
            bb12.Write((short) (tag >> 16));
            bb12.Write((short) tag);
            if (!explicitVR || vr == VRs.NONE) {
                bb12.Write(len);
                os.Write(b12, 0, 8);
                return 8;
            }
            bb12.Write((byte) ((vr) >> 8));
            bb12.Write((byte) vr);
            if (VRs.IsLengthField16Bit(vr)) {
                bb12.Write((short) len);
                os.Write(b12, 0, 8);
                return 8;
            }
            bb12.Write((byte) 0);
            bb12.Write((byte) 0);
            bb12.Write(len);
            os.Write(b12, 0, 12);
            return 12;
        }
    }
}