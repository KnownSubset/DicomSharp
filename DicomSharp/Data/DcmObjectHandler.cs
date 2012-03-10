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
using DicomSharp.Dictionary;
using DicomSharp.Utility;

namespace DicomSharp.Data {
    /// <summary>
    /// DcmAssociationHandler, parsing DICOM data into Object memory block
    /// </summary>
    internal class DcmObjectHandler : IDcmHandler {
        private readonly DcmObject result;
        private readonly Stack seqStack = new Stack();
        private ByteOrder byteOrder = ByteOrder.LITTLE_ENDIAN;
        private DcmObject curDcmObject;
        private long pos;
        private uint tag;
        private int vr;

        /// <summary>
        /// Creates a new instance of DcmHandlerImpl 
        /// </summary>
        public DcmObjectHandler(DcmObject result) {
            if (result == null) {
                throw new NullReferenceException();
            }

            this.result = result;
        }

        #region IDcmHandler Members

        public virtual DcmDecodeParam DcmDecodeParam {
            set { byteOrder = value.byteOrder; }
        }

        public virtual void StartCommand() {
            curDcmObject = result;
            seqStack.Clear();
        }

        public virtual void EndCommand() {
            curDcmObject = null;
        }

        public virtual void StartDcmFile() {
            // noop
        }

        public virtual void EndDcmFile() {
            // noop
        }

        public virtual void StartFileMetaInfo(byte[] preamble) {
            if (result is Dataset) {
                curDcmObject = ((Dataset) result).GetFileMetaInfo();
                if (curDcmObject == null) {
                    ((Dataset) result).SetFileMetaInfo((FileMetaInfo) (curDcmObject = new FileMetaInfo()));
                }
            }
            else {
                curDcmObject = result;
            }
            seqStack.Clear();
            if (preamble != null) {
                if (preamble.Length == 128) {
                    Array.Copy(preamble, 0, ((FileMetaInfo) curDcmObject).Preamble, 0, 128);
                }
                else {
                    // log.warn
                }
            }
        }

        public virtual void EndFileMetaInfo() {
            if (result is Dataset) {
                curDcmObject = result;
            }
            else {
                curDcmObject = null;
            }
        }

        public virtual void StartDataset() {
            curDcmObject = result;
            seqStack.Clear();
        }

        public virtual void EndDataset() {
            curDcmObject = null;
        }


        public virtual void StartElement(uint tag, int vr, long pos) {
            this.tag = tag;
            this.vr = vr;
            this.pos = pos;
        }

        public virtual void EndElement() {}

        public virtual void StartSequence(int length) {
            seqStack.Push(vr == VRs.SQ ? curDcmObject.PutSQ(tag) : curDcmObject.PutXXsq(tag, vr));
        }

        public virtual void EndSequence(int length) {
            seqStack.Pop();
        }

        public virtual void Value(ByteBuffer bb) {
            DcmElement elm = curDcmObject.PutXX(tag, vr, bb);
            elm.StreamPosition = pos;
        }

        public virtual void Value(byte[] data, int Start, int length) {
            ByteBuffer buf = ByteBuffer.Wrap(data, Start, length, byteOrder);
            DcmElement elm = curDcmObject.PutXX(tag, vr, buf);
            elm.StreamPosition = pos;
        }

        public virtual void Fragment(int id, long pos, byte[] data, int Start, int length) {
            ((DcmElement) seqStack.Peek()).AddDataFragment(ByteBuffer.Wrap(data, Start, length, byteOrder));
        }

        public virtual void StartItem(int id, long pos, int length) {
            curDcmObject = ((DcmElement) seqStack.Peek()).AddNewItem().SetItemOffset(pos);
        }

        public virtual void EndItem(int len) {
            curDcmObject = ((Dataset) curDcmObject).Parent;
        }

        #endregion
    }
}