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
using System.Reflection;
using System.Text;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Data {
    public abstract class ValueElement : DcmElement {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected ByteBuffer m_data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        internal ValueElement(uint tag, ByteBuffer data) : base(tag) {
            m_data = data;
        }

        public override ByteBuffer GetByteBuffer() {
            return m_data;
        }

        public virtual void SetByteBuffer(ByteBuffer data) {
            m_data = data;
        }

        /// <summary>
        /// Create a element for Short value
        /// </summary>
        private static ByteBuffer SetShort(int value) {
            return ByteBuffer.Wrap(new byte[2], ByteOrder.LITTLE_ENDIAN).Write((short) value);
        }

        private static ByteBuffer SetShorts(int[] value) {
            if (value.Length == 0) {
                return EMPTY_VALUE;
            }

            if (value.Length == 1) {
                return SetShort(value[0]);
            }

            ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 1], ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < value.Length; ++i) {
                bb.Write((short) value[i]);
            }
            return bb;
        }

        /// <summary>
        /// Create a element for Int value
        /// </summary>
        private static ByteBuffer SetInt(int value) {
            return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write(value);
        }

        private static ByteBuffer SetInts(int[] value) {
            if (value.Length == 0) {
                return EMPTY_VALUE;
            }

            if (value.Length == 1) {
                return SetInt(value[0]);
            }

            ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < value.Length; ++i) {
                bb.Write(value[i]);
            }
            return bb;
        }

        /// <summary>
        /// Create a element for Tag value
        /// </summary>
        private static ByteBuffer SetTag(int value) {
            return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write((short) (value >> 8)).Write((short) value);
        }

        private static ByteBuffer SetTags(int[] value) {
            if (value.Length == 0) {
                return EMPTY_VALUE;
            }

            if (value.Length == 1) {
                return SetTag(value[0]);
            }

            ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < value.Length; ++i) {
                bb.Write((short) (value[i] >> 16)).Write((short) value[i]);
            }
            return bb;
        }

        /// <summary>
        /// Create a element for Float value
        /// </summary>
        private static ByteBuffer SetFloat(float value) {
            return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write(value);
        }

        private static ByteBuffer SetFloats(float[] value) {
            if (value.Length == 0) {
                return EMPTY_VALUE;
            }

            if (value.Length == 1) {
                return SetFloat(value[0]);
            }

            ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < value.Length; ++i) {
                bb.Write(value[i]);
            }
            return bb;
        }

        /// <summary>
        /// Create a element for Double value
        /// </summary>
        private static ByteBuffer SetDouble(Double value) {
            return ByteBuffer.Wrap(new byte[8], ByteOrder.LITTLE_ENDIAN).Write(value);
        }

        private static ByteBuffer SetDoubles(Double[] value) {
            if (value.Length == 0) {
                return EMPTY_VALUE;
            }

            if (value.Length == 1) {
                return SetDouble(value[0]);
            }

            ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 3], ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < value.Length; ++i) {
                bb.Write(value[i]);
            }
            return bb;
        }

        public override int length() {
            return ((m_data.length() + 1) & (~ 1));
        }


        public override ByteBuffer GetByteBuffer(ByteOrder byteOrder) {
            if (m_data.GetOrder() != byteOrder) {
                SwapOrder();
            }
            return m_data.Rewind();
        }

        public override int vm() {
            return m_data.length() <= 0 ? 0 : m_data.length();
        }

        public override String GetString(int index, Encoding encoding) {
            if (index >= vm()) {
                return null;
            }
            return GetInt(index).ToString();
        }

        public override String[] GetStrings(Encoding encoding) {
            var a = new String[vm()];
            for (int i = 0; i < a.Length; ++i) {
                a[i] = GetInt(i).ToString();
            }
            return a;
        }

        public virtual void SwapOrder() {
            m_data.SetOrder(Swap(m_data.GetOrder()));
        }

        internal static DcmElement CreateSS(uint tag, ByteBuffer data) {
            if ((data.length() & 1) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " SS #" + data.length());
                return new SS(tag, EMPTY_VALUE);
            }
            return new SS(tag, data);
        }

        internal static DcmElement CreateSS(uint tag) {
            return new SS(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateSS(uint tag, int v) {
            return new SS(tag, SetShort(v));
        }

        internal static DcmElement CreateSS(uint tag, int[] a) {
            return new SS(tag, SetShorts(a));
        }

        internal static DcmElement CreateUS(uint tag, ByteBuffer data) {
            if ((data.length() & 1) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " US #" + data.length());
                return new US(tag, EMPTY_VALUE);
            }
            return new US(tag, data);
        }

        internal static DcmElement CreateUS(uint tag) {
            return new US(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateUS(uint tag, int s) {
            return new US(tag, SetShort(s));
        }

        internal static DcmElement CreateUS(uint tag, int[] s) {
            return new US(tag, SetShorts(s));
        }

        internal static DcmElement CreateSL(uint tag, ByteBuffer data) {
            if ((data.length() & 3) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " SL #" + data.length());
                return new SL(tag, EMPTY_VALUE);
            }
            return new SL(tag, data);
        }

        internal static DcmElement CreateSL(uint tag) {
            return new SL(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateSL(uint tag, int v) {
            return new SL(tag, SetInt(v));
        }

        internal static DcmElement CreateSL(uint tag, int[] a) {
            return new SL(tag, SetInts(a));
        }

        internal static DcmElement CreateUL(uint tag, ByteBuffer data) {
            if ((data.length() & 3) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " UL #" + data.length());
                return new UL(tag, EMPTY_VALUE);
            }
            return new UL(tag, data);
        }

        internal static DcmElement CreateUL(uint tag) {
            return new UL(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateUL(uint tag, int v) {
            return new UL(tag, SetInt(v));
        }

        internal static DcmElement CreateUL(uint tag, int[] a) {
            return new UL(tag, SetInts(a));
        }

        internal static DcmElement CreateAT(uint tag, ByteBuffer data) {
            if ((data.length() & 3) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " AT #" + data.length());
                return new AT(tag, EMPTY_VALUE);
            }
            return new AT(tag, data);
        }

        internal static DcmElement CreateAT(uint tag) {
            return new AT(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateAT(uint tag, int v) {
            return new AT(tag, SetTag(v));
        }

        internal static DcmElement CreateAT(uint tag, int[] a) {
            return new AT(tag, SetTags(a));
        }

        internal static DcmElement CreateFL(uint tag, ByteBuffer data) {
            if ((data.length() & 3) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " FL #" + data.length());
                return new FL(tag, EMPTY_VALUE);
            }

            return new FL(tag, data);
        }

        internal static DcmElement CreateFL(uint tag) {
            return new FL(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateFL(uint tag, float v) {
            return new FL(tag, SetFloat(v));
        }

        internal static DcmElement CreateFL(uint tag, float[] a) {
            return new FL(tag, SetFloats(a));
        }

        internal static DcmElement CreateFD(uint tag, ByteBuffer data) {
            if ((data.length() & 7) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " FD #" + data.length());
                return new FD(tag, EMPTY_VALUE);
            }
            return new FD(tag, data);
        }

        internal static DcmElement CreateFD(uint tag) {
            return new FD(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateFD(uint tag, Double v) {
            return new FD(tag, SetDouble(v));
        }

        internal static DcmElement CreateFD(uint tag, Double[] a) {
            return new FD(tag, SetDoubles(a));
        }

        internal static DcmElement CreateOW(uint tag) {
            return new OW(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateOW(uint tag, short[] v) {
            ByteBuffer buf = ByteBuffer.Wrap(new byte[v.Length << 1]);
            buf.SetOrder(ByteOrder.LITTLE_ENDIAN);
            for (int i = 0; i < v.Length; ++i) {
                buf.Write(v[i]);
            }

            return new OW(tag, buf);
        }

        internal static DcmElement CreateOW(uint tag, short[][] v) {
            int len2 = v[0].Length;

            ByteBuffer buf = ByteBuffer.Wrap(new byte[(v.Length*len2) << 1]);
            buf.SetOrder(ByteOrder.LITTLE_ENDIAN);

            for (int j = 0; j < len2; j++) {
                for (int i = 0; i < v.Length; ++i) {
                    buf.Write(v[i][j]);
                }
            }

            return new OW(tag, buf);
        }

        internal static DcmElement CreateOW(uint tag, ByteBuffer data) {
            if ((data.length() & 1) != 0) {
                log.Warn("Ignore illegal value of " + Dictionary.Tags.ToHexString(tag) + " OW #" + data.length());
                return new OW(tag, EMPTY_VALUE);
            }
            return new OW(tag, data);
        }

        internal static DcmElement CreateOB(uint tag) {
            return new OB(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateOB(uint tag, ByteBuffer v) {
            return new OB(tag, v);
        }

        internal static DcmElement CreateOB(uint tag, byte[] v) {
            return new OB(tag, ByteBuffer.Wrap(v, ByteOrder.LITTLE_ENDIAN));
        }

        internal static DcmElement CreateUN(uint tag) {
            return new UN(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateUN(uint tag, ByteBuffer v) {
            return new UN(tag, v);
        }

        internal static DcmElement CreateUN(uint tag, byte[] v) {
            return new UN(tag, ByteBuffer.Wrap(v, ByteOrder.LITTLE_ENDIAN));
        }

        #region Nested type: AT

        /// <summary>
        /// AT - Attribute tag, 4 bytes fixed
        /// </summary>
        internal sealed class AT : ValueElement {
            internal AT(uint tag, ByteBuffer data) : base(tag, data) {}

            public override uint[] Tags {
                get {
                    var a = new uint[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetTag(i);
                    }
                    return a;
                }
            }

            public override int vr() {
                return 0x4154;
            }

            public override int vm() {
                return m_data.length() >> 2;
            }

            public override uint GetTag(int index) {
                if (index >= vm()) {
                    return 0;
                }
                int pos = index << 2;
                return (uint) (((m_data.ReadInt16(pos) << 16) | (m_data.ReadInt16(pos + 2) & 0xffff)));
            }


            public override String GetString(int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return Dictionary.Tags.ToHexString(GetTag(index));
            }

            public override String[] GetStrings(Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetString(i, null);
                }
                return a;
            }

            public override void SwapOrder() {
                SwapWords(m_data);
            }
        }

        #endregion

        #region Nested type: FD

        /// <summary>
        /// FD - Floating point Double, 8 bytes fixed
        /// </summary>
        internal sealed class FD : ValueElement {
            internal FD(uint tag, ByteBuffer data) : base(tag, data) {}

            public override Double[] Doubles {
                get {
                    var a = new Double[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetDouble(i);
                    }
                    return a;
                }
            }

            public override int vm() {
                return m_data.length() >> 3;
            }

            public override int vr() {
                return 0x4644;
            }

            public override Double GetDouble(int index) {
                if (index >= vm()) {
                    return 0.0;
                }
                return m_data.ReadDouble(index << 3);
            }


            public override String GetString(int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return GetDouble(index).ToString();
            }

            public override String[] GetStrings(Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetDouble(i).ToString();
                }
                return a;
            }

            public override void SwapOrder() {
                SwapLongs(m_data);
            }
        }

        #endregion

        #region Nested type: FL

        /// <summary>
        /// FL - Floating point single, 4 bytes fixed
        /// </summary>
        internal sealed class FL : ValueElement {
            internal FL(uint tag, ByteBuffer data) : base(tag, data) {}

            public override float[] Floats {
                get {
                    var a = new float[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetFloat(i);
                    }
                    return a;
                }
            }

            public override int vm() {
                return m_data.length() >> 2;
            }

            public override int vr() {
                return 0x464C;
            }

            public override float GetFloat(int index) {
                if (index >= vm()) {
                    return 0.0f;
                }
                return m_data.ReadSingle(index << 2);
            }


            public override String GetString(int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return GetFloat(index).ToString();
            }

            public override String[] GetStrings(Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetFloat(i).ToString();
                }
                return a;
            }

            public override void SwapOrder() {
                SwapInts(m_data);
            }
        }

        #endregion

        #region Nested type: IntBase

        /// <summary>
        /// Int - SL, UL
        /// </summary>
        internal abstract class IntBase : ValueElement {
            internal IntBase(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int[] Ints {
                get {
                    var a = new int[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetInt(i);
                    }
                    return a;
                }
            }

            public override int GetInt(int index) {
                if (index >= vm()) {
                    return 0;
                }
                return m_data.ReadInt32(index << 2);
            }


            public override int vm() {
                return m_data.length() >> 2;
            }

            public override void SwapOrder() {
                SwapInts(m_data);
            }
        }

        #endregion

        #region Nested type: OB

        /// <summary>
        /// OB - Other byte string, depending on transfer syntax
        /// </summary>
        internal sealed class OB : ValueElement {
            internal OB(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int vr() {
                return 0x4F42;
            }

            public override String GetString(int index, Encoding encoding) {
                return GetBoundedString(Int32.MaxValue, index, encoding);
            }

            public override String GetBoundedString(int maxLen, int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return StringUtils.PromptOB(m_data, maxLen);
            }

            public override String[] GetStrings(Encoding encoding) {
                return GetBoundedStrings(Int32.MaxValue, encoding);
            }

            public override String[] GetBoundedStrings(int maxLen, Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetBoundedString(maxLen, i, null);
                }
                return a;
            }
        }

        #endregion

        #region Nested type: OW

        /// <summary>
        /// OW - Other word (2 bytes) string, depending on Transfer Syntax
        /// </summary>
        internal sealed class OW : ValueElement {
            internal OW(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int[] Ints {
                get {
                    var a = new int[m_data.length() >> 1];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetInt(i);
                    }
                    return a;
                }
            }

            public override int vr() {
                return 0x4F57;
            }

            public override int GetInt(int index) {
                if (index >= vm()) {
                    return 0;
                }
                return m_data.ReadInt16(index << 1) & 0xffff;
            }


            public override String GetString(int index, Encoding encoding) {
                return GetBoundedString(Int32.MaxValue, index, encoding);
            }

            public override String GetBoundedString(int maxLen, int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return StringUtils.PromptOW(m_data, maxLen);
            }

            public override String[] GetStrings(Encoding encoding) {
                return GetBoundedStrings(Int32.MaxValue, encoding);
            }

            public override String[] GetBoundedStrings(int maxLen, Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetBoundedString(maxLen, i, null);
                }
                return a;
            }

            public override void SwapOrder() {
                SwapWords(m_data);
            }
        }

        #endregion

        #region Nested type: SL

        /// <summary>
        /// SL
        /// </summary>
        internal class SL : IntBase {
            internal SL(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int vr() {
                return 0x534C;
            }
        }

        #endregion

        #region Nested type: SS

        /// <summary>
        /// SS - Signed short, 2 bytes fixed
        /// </summary>
        private sealed class SS : ValueElement {
            internal SS(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int[] Ints {
                get {
                    var a = new int[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetInt(i);
                    }
                    return a;
                }
            }

            public override int vr() {
                return 0x5353;
            }

            public override int vm() {
                return m_data.length() >> 1;
            }

            public override int GetInt(int index) {
                if (index >= vm()) {
                    return 0;
                }
                return m_data.ReadInt16(index << 1);
            }


            public override void SwapOrder() {
                SwapWords(m_data);
            }
        }

        #endregion

        #region Nested type: UL

        /// <summary>
        /// UL - Unsigned long, 4 bytes fixed
        /// </summary>
        internal class UL : IntBase {
            internal UL(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int vr() {
                return 0x554C;
            }

            public override String GetString(int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return GetInt(index).ToString();
            }

            public override String[] GetStrings(Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetInt(i).ToString();
                }
                return a;
            }
        }

        #endregion

        #region Nested type: UN

        /// <summary>
        /// UN - Unkown, a string of bytes, any length
        /// </summary>
        internal sealed class UN : ValueElement {
            internal UN(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int vr() {
                return 0x554E;
            }

            public override String GetString(int index, Encoding encoding) {
                return GetBoundedString(Int32.MaxValue, index, encoding);
            }

            public override String GetBoundedString(int maxLen, int index, Encoding encoding) {
                if (index >= vm()) {
                    return null;
                }
                return StringUtils.PromptOB(m_data, maxLen);
            }

            public override String[] GetStrings(Encoding encoding) {
                return GetBoundedStrings(Int32.MaxValue, encoding);
            }

            public override String[] GetBoundedStrings(int maxLen, Encoding encoding) {
                var a = new String[vm()];
                for (int i = 0; i < a.Length; ++i) {
                    a[i] = GetBoundedString(maxLen, i, null);
                }
                return a;
            }
        }

        #endregion

        #region Nested type: US

        /// <summary>
        /// US - Unsigned short, 2 bytes fixed
        /// </summary>
        private sealed class US : ValueElement {
            internal US(uint tag, ByteBuffer data) : base(tag, data) {}

            public override int[] Ints {
                get {
                    var a = new int[vm()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = GetInt(i);
                    }
                    return a;
                }
            }

            public override int vr() {
                return 0x5553;
            }

            public override int vm() {
                return m_data.length() >> 1;
            }

            public override int GetInt(int index) {
                if (index >= vm()) {
                    return 0;
                }
                return m_data.ReadInt16(index << 1) & 0xffff;
            }


            public override void SwapOrder() {
                SwapWords(m_data);
            }
        }

        #endregion
    }
}