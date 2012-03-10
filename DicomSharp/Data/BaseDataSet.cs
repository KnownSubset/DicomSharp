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
using System.IO;
using System.Reflection;
using DicomSharp.Dictionary;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Data {
    public abstract class BaseDataSet : DcmObject {
        private int grCount;
        private int[] grLens = new int[8];
        private uint[] grTags = new uint[8];
        protected internal int totLen;

        public FileMetaInfo FileMetaInfo { get; set; }

        public override String ToString() {
            return "[" + Size + " elements]";
        }

        // TODO: unify these two
        private int[] EnsureCapacity(int[] old, int n) {
            if (n <= old.Length) {
                return old;
            }
            var retval = new int[old.Length << 1];
            Array.Copy(old, 0, retval, 0, old.Length);
            return retval;
        }

        private uint[] EnsureCapacity(uint[] old, int n) {
            if (n <= old.Length) {
                return old;
            }
            var retval = new uint[old.Length << 1];
            Array.Copy(old, 0, retval, 0, old.Length);
            return retval;
        }

        public virtual int CalcLength(DcmEncodeParam param) {
            totLen = 0;
            grCount = 0;

            uint prevGrTag = 0;
            foreach (var dcmElement in _dcmElements)
            {
                uint curGrTag = dcmElement.tag() & 0xffff0000;
                if (curGrTag != prevGrTag)
                {
                    grCount++;
                    grTags = EnsureCapacity(grTags, grCount + 1);
                    grLens = EnsureCapacity(grLens, grCount + 1);
                    grTags[grCount - 1] = prevGrTag = curGrTag;
                    grLens[grCount - 1] = 0;
                }
                grLens[grCount - 1] += (param.explicitVR && !VRs.IsLengthField16Bit(dcmElement.VR())) ? 12 : 8;
                if (dcmElement is ValueElement)
                {
                    grLens[grCount - 1] += dcmElement.Length();
                }
                else if (dcmElement is FragmentElement)
                {
                    grLens[grCount - 1] += ((FragmentElement)dcmElement).CalcLength();
                }
                else
                {
                    grLens[grCount - 1] += ((SQElement)dcmElement).CalcLength(param);
                } 
            }
            grTags[grCount] = 0;
            if (!param.skipGroupLen) {
                totLen += grCount*12;
            }
            for (int i = 0; i < grCount; ++i) {
                totLen += grLens[i];
            }
            return totLen;
        }

        public virtual int Length() {
            return totLen;
        }

        public override void Clear() {
            base.Clear();
            totLen = 0;
        }

        public abstract long GetItemOffset();
        public abstract DataSet SetItemOffset(long itemOffset);

        public virtual void WriteDataSet(IDcmHandler handler, DcmEncodeParam param) {
            if (!(param.skipGroupLen && param.undefItemLen && param.undefSeqLen)) {
                CalcLength(param);
            }
            handler.StartDataSet();
            handler.DcmDecodeParam = param;
            DoWrite(handler, param);
            handler.EndDataSet();
        }

        private void DoWrite(IDcmHandler handler, DcmEncodeParam param) {
            int grIndex = 0;
            foreach (var dcmElement in _dcmElements){
                if (!param.skipGroupLen && grTags[grIndex] == (dcmElement.tag() & (int) (- (0x100000000 - 0xffff0000)))) {
                    var b4 = new byte[4];
                    ByteBuffer.Wrap(b4, param.byteOrder).Write(grLens[grIndex]);
                    handler.StartElement(grTags[grIndex], VRs.UL, dcmElement.StreamPosition);
                    handler.Value(b4, 0, 4);
                    handler.EndElement();
                    ++grIndex;
                }
                if (dcmElement is SQElement) {
                    int len = param.undefSeqLen ? - 1 : dcmElement.Length();
                    handler.StartElement(dcmElement.tag(), VRs.SQ, dcmElement.StreamPosition);
                    handler.StartSequence(len);
                    for (int j = 0, m = dcmElement.VM(); j < m;) {
                        BaseDataSet dataSet = dcmElement.GetItem(j);
                        int itemlen = param.undefItemLen ? - 1 : dataSet.Length();
                        handler.StartItem(++j, dataSet.GetItemOffset(), itemlen);
                        dataSet.DoWrite(handler, param);
                        handler.EndItem(itemlen);
                    }
                    handler.EndSequence(len);
                    handler.EndElement();
                }
                else if (dcmElement is FragmentElement) {
                    long offset = dcmElement.StreamPosition;
                    handler.StartElement(dcmElement.tag(), dcmElement.VR(), offset);
                    handler.StartSequence(- 1);
                    if (offset != - 1L) {
                        offset += 12;
                    }
                    for (int j = 0, m = dcmElement.VM(); j < m;) {
                        ByteBuffer bb = dcmElement.GetDataFragment(j, param.byteOrder);
                        handler.Fragment(++j, offset, bb.ToArray(), (int) bb.Position, (int) bb.Length);
                        if (offset != - 1L) {
                            offset += (bb.Length + 9) & (~ 1);
                        }
                    }
                    handler.EndSequence(- 1);
                    handler.EndElement();
                }
                else {
//					int len = el.Length();
                    handler.StartElement(dcmElement.tag(), dcmElement.VR(), dcmElement.StreamPosition);
                    ByteBuffer bb = dcmElement.GetByteBuffer(param.byteOrder);
                    handler.Value(bb);
                    handler.EndElement();
                }
            }
        }

        public virtual void WriteDataSet(Stream outs, DcmEncodeParam param) {
            if (param == null) {
                param = DcmDecodeParam.IVR_LE;
            }
            // TODO: Check deflated 
            WriteDataSet(new DcmStreamHandler(outs), param);
        }

        public virtual void WriteFile(Stream outs, DcmEncodeParam param) {
            FileMetaInfo fmi = FileMetaInfo;
            if (fmi != null) {
                fmi.Write(outs);
                if (param == null) {
                    param = DcmDecodeParam.ValueOf(fmi.TransferSyntaxUniqueId);
                }
            }
            WriteDataSet(outs, param);
        }


        /*
		/// <summary>
		/// Get the subset of current DataSet
		/// Used by DICOMDIR
		/// </summary>
		/// <param name="fromTag"></param>
		/// <param name="toTag"></param>
		/// <returns></returns>
		public virtual DataSet subSet(int fromTag, int toTag)
		{
			return new FilterDataset.Segment(thIs, fromTag, toTag);
		}
		
		/// <summary>
		/// Get the subset of current DataSet
		/// Used by DICOMDIR
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public virtual DataSet subSet(DataSet filter)
		{
			return new FilterDataset.Selection(thIs, filter);
		}	
		*/
    }
}