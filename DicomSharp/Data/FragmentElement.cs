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
// 7/23/08: Solved bug by Marcel de Wijs. virtual changed into override (changed lines 128, 206).

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using DicomSharp.Dictionary;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Data {
    /// <summary>
    /// </summary>
    public abstract class FragmentElement : DcmElement {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IList<ByteBuffer> _byteBuffers = new List<ByteBuffer>();

        /// <summary>
        /// Creates a new instance of ElementImpl 
        /// </summary>
        public FragmentElement(uint tag) : base(tag) {}

        public override int VM() {
            return _byteBuffers.Count;
        }

        public override bool HasDataFragments() {
            return true;
        }

        public override ByteBuffer GetDataFragment(int index) {
            if (index >= VM()) {
                return null;
            }

            int offsetSize = Marshal.SizeOf(typeof (uint)),
                end = _byteBuffers.Count - 1;

            var data = _byteBuffers[index];

            if ((0 == index)
                && (tag() == Dictionary.Tags.PixelData)
                && (data.Length == (end*offsetSize))) {
                uint nOffsetCorrection = 0;
                var mybuffy = new ByteBuffer((int) data.Length, data.GetOrder());

                for (int i = 1; i < end; i++) {
                    var sizeofElement = (uint) _byteBuffers[i].Length;

                    nOffsetCorrection += (uint) (sizeofElement + (((sizeofElement & 0x01) == 0x01) ? 9 : 8));

                    mybuffy.Write((i*offsetSize), (int) nOffsetCorrection);
                }

                // set the data to return.
                data = mybuffy;
                data.Position = 0;
            }

            return data;
        }

        public override ByteBuffer GetDataFragment(int index, ByteOrder byteOrder) {
            if (index >= VM()) {
                return null;
            }

            int offsetSize = Marshal.SizeOf(typeof (uint)),
                end = _byteBuffers.Count - 1;

            var data = _byteBuffers[index];

            if ((0 == index)
                && (tag() == Dictionary.Tags.PixelData)
                && (data.Length == (end*offsetSize))) {
                uint nOffsetCorrection = 0;
                var mybuffy = new ByteBuffer((int) data.Length, data.GetOrder());

                for (int i = 1; i < end; i++) {
                    var sizeofElement = (uint)_byteBuffers[i].Length;

                    nOffsetCorrection += (uint) (sizeofElement + (((sizeofElement & 0x01) == 0x01) ? 9 : 8));

                    mybuffy.Write((i*offsetSize), (int) nOffsetCorrection);
                }

                // set the data to return.
                data = mybuffy;
                data.Position = 0;
            }

            if (data.GetOrder() != byteOrder) {
                SwapOrder(data);
            }
            return data;
        }

        public override int GetDataFragmentLength(int index) {
            if (index >= VM()) {
                return 0;
            }
            var data = _byteBuffers[index];
            return (int) ((data.Length + 1) & (~1));
        }

        public override String GetString(int index, Encoding encoding) {
            return GetBoundedString(Int32.MaxValue, index, encoding);
        }

        public override String GetBoundedString(int maxLen, int index, Encoding encoding) {
            if (index >= VM()) {
                return null;
            }
            return StringUtils.PromptValue(VR(), GetDataFragment(index), maxLen);
        }

        public virtual String[] GetStrings(Encoding encoding) {
            return GetBoundedStrings(Int32.MaxValue, encoding);
        }

        public override String[] GetBoundedStrings(int maxLen, Encoding encoding) {
            var a = new String[VM()];
            for (int i = 0; i < a.Length; ++i) {
                a[i] = StringUtils.PromptValue(VR(), GetDataFragment(i), maxLen);
            }
            return a;
        }

        public virtual int CalcLength() {
            int len = 8;
            for (int i = 0, n = VM(); i < n; ++i) {
                len += GetDataFragmentLength(i) + 8;
            }
            return len;
        }

        public override void AddDataFragment(ByteBuffer data) {
            _byteBuffers.Add(data ?? EMPTY_VALUE);
        }

        protected internal virtual void SwapOrder(ByteBuffer data) {
            data.SetOrder(Swap(data.GetOrder()));
        }

        public static DcmElement CreateOB(uint tag) {
            return new OB(tag);
        }

        /*
		private sealed class OF:FragmentElement
		{
			internal OF(uint tag):base(tag)
			{
			}
			
			public int VR()
			{
				return VRs.OF;
			}
			
			public void  AddDataFragment(ByteBuffer data)
			{
				if ((data.Length() & 3) != 0)
				{
					log.warn("Ignore odd Length fragment of " + DicomSharp.Dictionary.Tags.toString(tag) + " OF #" + data.Length());
					data = null;
				}
				base.AddDataFragment(data);
			}
			
			protected internal void  SwapOrder(ByteBuffer data)
			{
				SwapInts(data);
			}
		}
		
		public static DcmElement CreateOF(uint tag)
		{
			return new FragmentElement.OF(tag);
		}
		*/

        public static DcmElement CreateOW(uint tag) {
            return new OW(tag);
        }

        public static DcmElement CreateUN(uint tag) {
            return new UN(tag);
        }

        public override String ToString() {
            var sb = new StringBuilder(Dictionary.Tags.ToHexString(tag()));
            sb.Append(",").Append(VRs.ToString(VR()));
            if (!IsEmpty()) {
                for (int i = 0, n = VM(); i < n; ++i) {
                    sb.Append("\n\tFrag-").Append(i + 1).Append(",#").Append(GetDataFragmentLength(i)).Append("[").
                        Append(StringUtils.PromptValue(VR(), GetDataFragment(i), 64)).Append("]");
                }
            }
            return sb.ToString();
        }

        #region Nested type: OB

        /// <summary>
        /// OB
        /// </summary>
        private sealed class OB : FragmentElement {
            internal OB(uint tag) : base(tag) {}

            public override int VR() {
                return VRs.OB;
            }
        }

        #endregion

        #region Nested type: OW

        /// <summary>
        /// OW
        /// </summary>
        private sealed class OW : FragmentElement {
            internal OW(uint tag) : base(tag) {}

            public override int VR() {
                return VRs.OW;
            }

            public override void AddDataFragment(ByteBuffer data) {
                if ((data.Length & 1) != 0) {
                    log.Warn("Ignore odd Length fragment of " + Dictionary.Tags.ToHexString(tag()) + " OW #" +data.Length);
                    data = null;
                }
                base.AddDataFragment(data);
            }

            protected internal void SwapOrder(ByteBuffer data) {
                SwapWords(data);
            }
        }

        #endregion

        #region Nested type: UN

        /// <summary>
        /// UN
        /// </summary>
        private sealed class UN : FragmentElement {
            internal UN(uint tag) : base(tag) {}

            public override int VR() {
                return VRs.UN;
            }
        }

        #endregion
    }
}