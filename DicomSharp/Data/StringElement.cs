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
using System.Globalization;
using System.Reflection;
using System.Text;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Data {
    /// <summary>
    /// String element
    /// TODO: Date time related element
    /// </summary>
    public abstract class StringElement : ValueElement {
        /// <summary>
        /// MultiStringElement -> LO, PN
        /// </summary>
        private const byte DELIM = (0x5c);

        /// <summary>
        /// DataString -> DA
        /// </summary>
        private const byte HYPHEN = (0x2d);

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Trim NO_TRIM = NoTrim;
        private static readonly Trim TRAIL_TRIM = TrailTrim;
        private static readonly Trim TOT_TRIM = TotTrim;
        private static readonly Check NO_CHECK = NoCheck;

        private readonly bool IsText;
        private readonly int maxLen;
        private Trim trim;

        /// <summary>
        /// Constructor for StringElement
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <param name="trim"></param>
        internal StringElement(uint tag, String value, int maxLen, bool IsText, Trim trim, Check chk, Encoding encoding)
            : base(tag, null) {
            this.trim = trim;
            this.maxLen = maxLen;
            this.IsText = IsText;
            m_data = ToByteBuffer(value, trim, chk == null ? DoCheck : chk, encoding);
        }

        internal StringElement(uint tag, String[] values, int maxLen, bool IsText, Trim trim, Check chk,
                               Encoding encoding) : base(tag, null) {
            this.trim = trim;
            this.maxLen = maxLen;
            this.IsText = IsText;
            m_data = ToByteBuffer(values, trim, chk == null ? DoCheck : chk, encoding);
        }

        internal StringElement(uint tag, ByteBuffer data, Trim trim) : base(tag, data) {
            this.trim = trim;
        }

        protected virtual String DoCheck(String s) {
            char[] a = s.ToCharArray();
            if (a.Length > maxLen) {
                log.Warn("Value: " + s + " exeeds VR Length limit: " + maxLen);
            }
            for (int i = 0; i < a.Length; ++i) {
                if (!DoCheck(a[i])) {
                    log.Warn("Illegal character '" + a[i] + "' in value: " + s);
                }
            }
            return s;
        }

        protected virtual bool DoCheck(char c) {
            return (Char.GetUnicodeCategory(c) == UnicodeCategory.Control)
                       ? (IsText && StringUtils.IsDICOMControl(c))
                       : (IsText || c != '\\');
        }

        public override String GetString(int index, Encoding encoding) {
            if (index >= VM()) {
                return null;
            }
            try {
                return GetByteBuffer(index).ReadString();
            }
            catch (Exception ex) {
                throw new DcmValueException(ex.Message, ex);
            }
        }

        public override String[] GetStrings(Encoding encoding) {
            var a = new String[VM()];
            for (int i = 0; i < a.Length; ++i) {
                a[i] = GetString(i, encoding);
            }
            return a;
        }

        public virtual ByteBuffer GetByteBuffer(int index) {
            if (index >= VM()) {
                return null;
            }
            return m_data.Rewind();
        }

        private static String NoTrim(String s) {
            return s;
        }

        private static String TrailTrim(String s) {
            char ch;
            for (int r = s.Length; r > 0; --r) {
                if ((ch = s[r - 1]) != '\x0000' && ch != ' ') {
                    return s.Substring(0, (r) - (0));
                }
            }

            return "";
        }

        private static String TotTrim(String s) {
            for (int r = s.Length; r > 0; --r) {
                if (s[r - 1] != ' ') {
                    for (int l = 0; l < r; ++l) {
                        if (s[l] != ' ') {
                            return s.Substring(l, (r) - (l));
                        }
                    }
                }
            }
            return "";
        }

        public static String NoCheck(String s) {
            return s;
        }

        private static ByteBuffer ToByteBuffer(String value, Trim trim, Check check, Encoding encoding) {
            if (string.IsNullOrEmpty(value)) {
                return EMPTY_VALUE;
            }
            try {
                return ByteBuffer.Wrap((encoding ?? Encoding.ASCII).GetBytes(check(trim(value))));
            }
            catch (Exception) {
                throw new ArgumentException(value);
            }
        }

        private static ByteBuffer ToByteBuffer(ByteBuffer[] bbs, int totLen) {
            ByteBuffer bb = ByteBuffer.Wrap(new byte[totLen]);
            bb.Write(bbs[0]);
            for (int i = 1; i < bbs.Length; ++i) {
                bb.Write(DELIM);
                bb.Write(bbs[i]);
            }
            return bb;
        }

        private static ByteBuffer ToByteBuffer(String[] values, Trim trim, Check Check, Encoding encoding) {
            if (values.Length == 0) {
                return EMPTY_VALUE;
            }

            if (values.Length == 1) {
                return ToByteBuffer(values[0], trim, Check, encoding);
            }

            var bbs = new ByteBuffer[values.Length];
            int totLen = - 1;
            for (int i = 0; i < values.Length; ++i) {
                bbs[i] = ToByteBuffer(values[i], trim, Check, encoding);
                totLen += (int)(bbs[i].Length + 1);
            }
            return ToByteBuffer(bbs, totLen);
        }

        internal static DcmElement CreateLT(uint tag, ByteBuffer data) {
            return new LT(tag, data);
        }

        internal static DcmElement CreateLT(uint tag) {
            return new LT(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateLT(uint tag, String value, Encoding encoding) {
            return new LT(tag, value, encoding);
        }

        internal static DcmElement CreateLT(uint tag, String[] values, Encoding encoding) {
            return new LT(tag, values, encoding);
        }

        internal static DcmElement CreateST(uint tag, ByteBuffer data) {
            return new ST(tag, data);
        }

        internal static DcmElement CreateST(uint tag) {
            return new ST(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateST(uint tag, String value, Encoding encoding) {
            return new ST(tag, value, encoding);
        }

        internal static DcmElement CreateST(uint tag, String[] values, Encoding encoding) {
            return new ST(tag, values, encoding);
        }

        internal static DcmElement CreateUT(uint tag, ByteBuffer data) {
            return new UT(tag, data);
        }

        internal static DcmElement CreateUT(uint tag) {
            return new UT(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateUT(uint tag, String value, Encoding encoding) {
            return new UT(tag, value, encoding);
        }

        internal static DcmElement CreateUT(uint tag, String[] values, Encoding encoding) {
            return new UT(tag, values, encoding);
        }

        internal static DcmElement CreateLO(uint tag, ByteBuffer data) {
            return new LO(tag, data);
        }

        internal static DcmElement CreateLO(uint tag) {
            return new LO(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateLO(uint tag, String value, Encoding encoding) {
            return new LO(tag, value, encoding);
        }

        internal static DcmElement CreateLO(uint tag, String[] values, Encoding encoding) {
            return new LO(tag, values, encoding);
        }

        internal static DcmElement CreatePN(uint tag, ByteBuffer data) {
            return new PN(tag, data);
        }

        internal static DcmElement CreatePN(uint tag) {
            return new PN(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreatePN(uint tag, PersonName value, Encoding encoding) {
            return new PN(tag, value.ToString(), encoding);
        }

        internal static DcmElement CreatePN(uint tag, PersonName[] values, Encoding encoding) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = values[i].ToString();
            }
            return new PN(tag, tmp, encoding);
        }

        internal static DcmElement CreateSH(uint tag, ByteBuffer data) {
            return new SH(tag, data);
        }

        internal static DcmElement CreateSH(uint tag) {
            return new SH(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateSH(uint tag, String value, Encoding encoding) {
            return new SH(tag, value, encoding);
        }

        internal static DcmElement CreateSH(uint tag, String[] values, Encoding encoding) {
            return new SH(tag, values, encoding);
        }

        //
        // AsciiMultiStringElements ----------------------------------------------
        //

        internal static DcmElement CreateAE(uint tag, ByteBuffer data) {
            return new AE(tag, data);
        }

        internal static DcmElement CreateAE(uint tag) {
            return new AE(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateAE(uint tag, String value) {
            return new AE(tag, value, null);
        }

        internal static DcmElement CreateAE(uint tag, String[] values) {
            return new AE(tag, values, null);
        }

        internal static DcmElement CreateAS(uint tag, ByteBuffer data) {
            return new AS(tag, data);
        }

        internal static DcmElement CreateAS(uint tag) {
            return new AS(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateAS(uint tag, String value) {
            return new AS(tag, value);
        }

        internal static DcmElement CreateAS(uint tag, String[] values) {
            return new AS(tag, values);
        }

        internal static DcmElement CreateCS(uint tag, ByteBuffer data) {
            return new CS(tag, data);
        }

        internal static DcmElement CreateCS(uint tag) {
            return new CS(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateCS(uint tag, String value) {
            return new CS(tag, value);
        }

        internal static DcmElement CreateCS(uint tag, String[] values) {
            return new CS(tag, values);
        }

        internal static DcmElement CreateDS(uint tag, ByteBuffer data) {
            return new DS(tag, data);
        }

        internal static DcmElement CreateDS(uint tag) {
            return new DS(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateDS(uint tag, float value) {
            return new DS(tag, value.ToString(CultureInfo.InvariantCulture.NumberFormat), NO_TRIM, NO_CHECK);
        }

        internal static DcmElement CreateDS(uint tag, float[] values) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = values[i].ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
            return new DS(tag, tmp, NO_TRIM, NO_CHECK);
        }

        internal static DcmElement CreateDS(uint tag, String value) {
            return new DS(tag, value, TOT_TRIM, null);
        }

        internal static DcmElement CreateDS(uint tag, String[] a) {
            return new DS(tag, a, TOT_TRIM, null);
        }

        internal static DcmElement CreateIS(uint tag, ByteBuffer data) {
            return new IS(tag, data);
        }

        internal static DcmElement CreateIS(uint tag) {
            return new IS(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateIS(uint tag, int value) {
            return new IS(tag, value.ToString(), NO_TRIM, NO_CHECK);
        }

        internal static DcmElement CreateIS(uint tag, int[] values) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = values[i].ToString();
            }
            return new IS(tag, tmp, NO_TRIM, NO_CHECK);
        }

        internal static DcmElement CreateIS(uint tag, String value) {
            return new IS(tag, value, TOT_TRIM, null);
        }

        internal static DcmElement CreateIS(uint tag, String[] value) {
            return new IS(tag, value, TOT_TRIM, null);
        }

        internal static DcmElement CreateUI(uint tag, ByteBuffer data) {
            return new UI(tag, data);
        }

        internal static DcmElement CreateUI(uint tag) {
            return new UI(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateUI(uint tag, String value) {
            return new UI(tag, value);
        }

        internal static DcmElement CreateUI(uint tag, String[] values) {
            return new UI(tag, values);
        }

        internal static DcmElement CreateDA(uint tag, ByteBuffer data) {
            return new DA(tag, data);
        }

        internal static DcmElement CreateDA(uint tag) {
            return new DA(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateDA(uint tag, DateTime value) {
            return new DA(tag, DA.ToString(value));
        }

        internal static DcmElement CreateDA(uint tag, DateTime[] values) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = DA.ToString(values[i]);
            }
            return new DA(tag, tmp);
        }

        internal static DcmElement CreateDA(uint tag, DateTime from, DateTime to) {
            return new DA(tag, DA.ToString(from) + "-" + DA.ToString(to));
        }

        internal static DcmElement CreateDA(uint tag, String value) {
            return new DA(tag, value);
        }

        internal static DcmElement CreateDA(uint tag, String[] values) {
            return new DA(tag, values);
        }

        internal static DcmElement CreateDT(uint tag, ByteBuffer data) {
            return new DT(tag, data);
        }

        internal static DcmElement CreateDT(uint tag) {
            return new DT(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateDT(uint tag, DateTime value) {
            return new DT(tag, DT.ToString(value));
        }

        internal static DcmElement CreateDT(uint tag, DateTime[] values) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = DT.ToString(values[i]);
            }
            return new DT(tag, tmp);
        }

        internal static DcmElement CreateDT(uint tag, DateTime from, DateTime to) {
            return new DT(tag, from.ToShortTimeString() + "-" + to.ToShortTimeString());
        }

        internal static DcmElement CreateDT(uint tag, String value) {
            return new DT(tag, value);
        }

        internal static DcmElement CreateDT(uint tag, String[] values) {
            return new DT(tag, values);
        }

        internal static DcmElement CreateTM(uint tag, ByteBuffer data) {
            return new TM(tag, data);
        }

        internal static DcmElement CreateTM(uint tag) {
            return new TM(tag, EMPTY_VALUE);
        }

        internal static DcmElement CreateTM(uint tag, DateTime value) {
            return new TM(tag, TM.ToString(value));
        }

        internal static DcmElement CreateTM(uint tag, DateTime[] values) {
            var tmp = new String[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                tmp[i] = TM.ToString(values[i]);
            }
            return new TM(tag, tmp);
        }

        internal static DcmElement CreateTM(uint tag, DateTime from, DateTime to) {
            return new TM(tag, TM.ToString(from) + "-" + TM.ToString(to));
        }

        internal static DcmElement CreateTM(uint tag, String value) {
            return new TM(tag, value);
        }

        internal static DcmElement CreateTM(uint tag, String[] values) {
            return new TM(tag, values);
        }

        #region Nested type: AE

        /// <summary>
        /// AE
        /// </summary>
        private sealed class AE : AsciiMultiStringElement {
            internal AE(uint tag, String value, Encoding encoding)
                : base(tag, value, 16, true, TOT_TRIM, null, encoding) {}

            internal AE(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 16, true, TOT_TRIM, null, encoding) {}

            internal AE(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override int ValueRepresentation() {
                return 0x4145;
            }
        }

        #endregion

        #region Nested type: AS

        /// <summary>
        /// AS
        /// </summary>
        private sealed class AS : AsciiMultiStringElement {
            internal AS(uint tag, String value)
                : base(tag, value, 0, true, NO_TRIM, null, null) {}

            internal AS(uint tag, String[] values)
                : base(tag, values, 0, true, NO_TRIM, null, null) {}

            internal AS(uint tag, ByteBuffer data) : base(tag, data, NO_TRIM) {}

            public override int ValueRepresentation() {
                return 0x4153;
            }

            protected override String DoCheck(String s) {
                if (s.Length == 4 && Char.IsDigit(s[0]) && Char.IsDigit(s[1]) && Char.IsDigit(s[2])) {
                    switch (s[3]) {
                        case 'D':
                        case 'W':
                        case 'M':
                        case 'Y':
                            return s;
                    }
                }
                log.Warn("Illegal Age String: " + s);
                return s;
            }
        }

        #endregion

        #region Nested type: AsciiMultiStringElement

        private abstract class AsciiMultiStringElement : MultiStringElement {
            internal AsciiMultiStringElement(uint tag, ByteBuffer data, Trim trim) : base(tag, data, trim) {}

            internal AsciiMultiStringElement(uint tag, String value, int maxLen, bool IsText, Trim trim, Check chk,
                                             Encoding encoding)
                : base(tag, value, 10240, true, TRAIL_TRIM, chk, encoding) {}

            internal AsciiMultiStringElement(uint tag, String[] values, int maxLen, bool IsText, Trim trim, Check chk,
                                             Encoding encoding)
                : base(tag, values, 10240, true, TRAIL_TRIM, chk, encoding) {}

            public override String GetString(int index, Encoding encoding) {
                return base.GetString(index, null);
            }
        }

        #endregion

        #region Nested type: CS

        /// <summary>
        /// CS
        /// </summary>
        private sealed class CS : AsciiMultiStringElement {
            internal CS(uint tag, String value)
                : base(tag, value, 16, false, TOT_TRIM, null, null) {}

            internal CS(uint tag, String[] values)
                : base(tag, values, 16, false, TOT_TRIM, null, null) {}

            internal CS(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override int ValueRepresentation() {
                return 0x4353;
            }

            protected override bool DoCheck(char c) {
                return ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '_');
            }
        }

        #endregion

        #region Nested type: Check

        /// <summary>
        /// Check/validate the string
        /// </summary>
        internal delegate String Check(String s);

        #endregion

        #region Nested type: DA

        /// <summary>
        /// DA
        /// </summary>
        private sealed class DA : DateString {
            private static readonly string[] _Formats = {"yyyyMMdd", "yyyyMM", "yyyy"};

            internal DA(uint tag, String value)
                : base(tag, value, NO_TRIM, NO_CHECK) {}

            internal DA(uint tag, String[] values)
                : base(tag, values, NO_TRIM, NO_CHECK) {}

            internal DA(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override DateTime[] Dates {
                get {
                    var a = new DateTime[VM()];
                    for (int i = 0; i < a.Length; ++i) {
                        // TODO: handle all format
                        a[i] = Parse(GetString(i, null));
                    }
                    return a;
                }
            }

            public static string ToString(DateTime dt) {
                return dt.ToString(_Formats[0], CultureInfo.InvariantCulture.DateTimeFormat);
            }

            public static DateTime Parse(string val) {
                try {
                    return DateTime.ParseExact(val, _Formats, CultureInfo.InvariantCulture.DateTimeFormat,
                                               DateTimeStyles.None);
                }
                catch {}

                return DateTime.MinValue;
            }

            public override int ValueRepresentation() {
                return 0x4441;
            }

            public override DateTime GetDate(int index) {
                return Parse(GetString(index, null));
            }

            public override DateTime[] GetDateRange(int index) {
                return StringUtils.ParseDateTimeRange(GetString(index, null), Parse);
            }
        }

        #endregion

        #region Nested type: DS

        /// <summary>
        /// DS
        /// </summary>
        private sealed class DS : AsciiMultiStringElement {
            internal DS(uint tag, String value, Trim trim, Check chk)
                : base(tag, value, 0, false, trim, chk, null) {}

            internal DS(uint tag, String[] values, Trim trim, Check chk)
                : base(tag, values, 0, false, trim, chk, null) {}

            internal DS(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override float[] Floats {
                get {
                    var retval = new float[VM()];
                    for (int i = 0; i < retval.Length; ++i) {
                        retval[i] = GetFloat(i);
                    }
                    return retval;
                }
            }

            public override int ValueRepresentation() {
                return 0x4453;
            }

            public override float GetFloat(int index) {
                return Single.Parse(GetString(index, null));
            }
        }

        #endregion

        #region Nested type: DT

        /// <summary>
        /// DT
        /// </summary>
        private sealed class DT : DateString {
            private static readonly string[] _Formats = {
                                                            "yyyyMMddHHmmss.fff", "yyyyMMddHHmmss", "yyyyMMddHHmm",
                                                            "yyyyMMdd", "yyyyMM", "yyyy"
                                                        };

            internal DT(uint tag, String value)
                : base(tag, value, NO_TRIM, NO_CHECK) {}

            internal DT(uint tag, String[] values)
                : base(tag, values, NO_TRIM, NO_CHECK) {}

            internal DT(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override DateTime[] Dates {
                get {
                    var a = new DateTime[VM()];
                    for (int i = 0; i < a.Length; ++i) {
                        // TODO: more formats
                        a[i] = Parse(GetString(i, null));
                    }
                    return a;
                }
            }

            public static string ToString(DateTime dt) {
                return (dt.Millisecond == 0)
                           ? dt.ToString(_Formats[1], CultureInfo.InvariantCulture.DateTimeFormat)
                           : dt.ToString(_Formats[0], CultureInfo.InvariantCulture.DateTimeFormat);
            }

            public static DateTime Parse(string val) {
                try {
                    return DateTime.ParseExact(val, _Formats, CultureInfo.InvariantCulture.DateTimeFormat,
                                               DateTimeStyles.None);
                }
                catch {}

                return DateTime.MinValue;
            }

            public override int ValueRepresentation() {
                return 0x4454;
            }

            public override DateTime GetDate(int index) {
                return Parse(GetString(index, null));
            }

            public override DateTime[] GetDateRange(int index) {
                return StringUtils.ParseDateTimeRange(GetString(index, null), Parse);
            }
        }

        #endregion

        #region Nested type: DateString

        private abstract class DateString : AsciiMultiStringElement {
            internal DateString(uint tag, String value, Trim trim, Check chk)
                : base(tag, value, 0, false, trim, chk, null) {}

            internal DateString(uint tag, String[] values, Trim trim, Check chk)
                : base(tag, values, 0, false, trim, chk, null) {}

            internal DateString(uint tag, ByteBuffer data, Trim trim)
                : base(tag, data, trim) {}

            public bool IsDataRange() {
                for (int i = 0, n = (int) m_data.Length; i < n; ++i) {
                    if (m_data.ReadByte(i) == HYPHEN) {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Nested type: IS

        /// <summary>
        /// IS
        /// </summary>
        private sealed class IS : AsciiMultiStringElement {
            internal IS(uint tag, String value, Trim trim, Check chk)
                : base(tag, value, 0, false, trim, chk, null) {}

            internal IS(uint tag, String[] values, Trim trim, Check chk)
                : base(tag, values, 0, false, trim, chk, null) {}

            internal IS(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override int[] Ints {
                get {
                    var retval = new int[VM()];
                    for (int i = 0; i < retval.Length; ++i) {
                        retval[i] = GetInt(i);
                    }
                    return retval;
                }
            }

            public override int ValueRepresentation() {
                return 0x4953;
            }

            public override int GetInt(int index) {
                String s = GetString(index, null);
                try {
                    return Int32.Parse(s);
                }
                catch (FormatException ex) {
                    throw new DcmValueException(s, ex);
                }
            }

            protected override String DoCheck(String s) {
                try {
                    Int32.Parse(s);
                    if (s.Length > 12) {
                        log.Warn("IS Value: " + s + " exeeds IS Length limit: 12");
                    }
                }
                catch (FormatException exception) {
                    log.Warn(String.Format("Illegal IS Value: {0}, {1}", s, exception.Message));
                }
                return s;
            }
        }

        #endregion

        #region Nested type: LO

        /// <summary>
        /// LO
        /// </summary>
        private sealed class LO : MultiStringElement {
            internal LO(uint tag, String value, Encoding encoding)
                : base(tag, value, 64, false, TOT_TRIM, null, encoding) {}

            internal LO(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 64, false, TOT_TRIM, null, encoding) {}

            internal LO(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override int ValueRepresentation() {
                return 0x4C4F;
            }
        }

        #endregion

        #region Nested type: LT

        /// <summary>
        /// LT
        /// </summary>
        private sealed class LT : StringElement {
            internal LT(uint tag, String value, Encoding encoding)
                : base(tag, value, 10240, true, TRAIL_TRIM, null, encoding) {}

            internal LT(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 10240, true, TRAIL_TRIM, null, encoding) {}

            internal LT(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override int ValueRepresentation() {
                return 0x4C54;
            }
        }

        #endregion

        #region Nested type: MultiStringElement

        private abstract class MultiStringElement : StringElement {
            private int[] delimPos;

            internal MultiStringElement(uint tag, ByteBuffer data, Trim trim) : base(tag, data, trim) {}

            internal MultiStringElement(uint tag, String value, int maxLen, bool IsText, Trim trim, Check chk,
                                        Encoding encoding)
                : base(tag, value, maxLen, IsText, TRAIL_TRIM, chk, encoding) {}

            internal MultiStringElement(uint tag, String[] values, int maxLen, bool IsText, Trim trim, Check chk,
                                        Encoding encoding)
                : base(tag, values, maxLen, IsText, TRAIL_TRIM, chk, encoding) {}

            public override int VM() {
                if (delimPos != null) {
                    return delimPos.Length - 1;
                }
                byte[] a = m_data.ToArray();
                if (a.Length == 0) {
                    return 0;
                }
                int vm = 1;
                for (int i = 0; i < a.Length; ++i) {
                    if (a[i] == DELIM) {
                        ++vm;
                    }
                }
                delimPos = new int[vm + 1];
                delimPos[0] = - 1;
                delimPos[vm] = a.Length;
                for (int i = 0, j = 0; i < a.Length; ++i) {
                    if (a[i] == DELIM) {
                        delimPos[++j] = i;
                    }
                }
                return vm;
            }

            public override ByteBuffer GetByteBuffer(int index) {
                if (index >= VM()) {
                    return null;
                }
                return VM() == 1
                           ? m_data.Rewind()
                           : ByteBuffer.Wrap(m_data.ToArray(), delimPos[index] + 1,
                                             delimPos[index + 1] - delimPos[index] - 1);
            }
        }

        #endregion

        #region Nested type: PN

        /// <summary>
        /// PN
        /// </summary>
        private sealed class PN : MultiStringElement {
            internal PN(uint tag, String value, Encoding encoding)
                : base(tag, value, 0, false, NO_TRIM, NO_CHECK, encoding) {}

            internal PN(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 0, false, NO_TRIM, NO_CHECK, encoding) {}

            internal PN(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override int ValueRepresentation() {
                return 0x504E;
            }

            public PersonName GetPersonName(int index, Encoding encoding) {
                return new PersonName(GetString(index, encoding));
            }
        }

        #endregion

        #region Nested type: SH

        /// <summary>
        /// SH
        /// </summary>
        private sealed class SH : MultiStringElement {
            internal SH(uint tag, String value, Encoding encoding)
                : base(tag, value, 16, false, TOT_TRIM, null, encoding) {}

            internal SH(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 16, false, TOT_TRIM, null, encoding) {}

            internal SH(uint tag, ByteBuffer data) : base(tag, data, TOT_TRIM) {}

            public override int ValueRepresentation() {
                return 0x5348;
            }
        }

        #endregion

        #region Nested type: ST

        /// <summary>
        /// ST
        /// </summary>
        private sealed class ST : StringElement {
            internal ST(uint tag, String value, Encoding encoding)
                : base(tag, value, 1024, true, TRAIL_TRIM, null, encoding) {}

            internal ST(uint tag, String[] values, Encoding encoding)
                : base(tag, values, 1024, true, TRAIL_TRIM, null, encoding) {}

            internal ST(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override int ValueRepresentation() {
                return 0x5354;
            }
        }

        #endregion

        #region Nested type: TM

        /// <summary>
        /// TM
        /// </summary>
        private sealed class TM : DateString {
            private static readonly string[] _Formats = {"HHmmss.fff", "HHmmss", "HHmm"};

            internal TM(uint tag, String value)
                : base(tag, value, NO_TRIM, NO_CHECK) {}

            internal TM(uint tag, String[] values)
                : base(tag, values, NO_TRIM, NO_CHECK) {}

            internal TM(uint tag, ByteBuffer data) : base(tag, data, TRAIL_TRIM) {}

            public override DateTime[] Dates {
                get {
                    var a = new DateTime[VM()];
                    for (int i = 0; i < a.Length; ++i) {
                        a[i] = Parse(GetString(i, null));
                    }
                    return a;
                }
            }

            public static string ToString(DateTime dt) {
                return (dt.Millisecond == 0)
                           ? dt.ToString(_Formats[1], CultureInfo.InvariantCulture.DateTimeFormat)
                           : dt.ToString(_Formats[0], CultureInfo.InvariantCulture.DateTimeFormat);
            }

            public static DateTime Parse(string val) {
                try {
                    return DateTime.ParseExact(val, _Formats, CultureInfo.InvariantCulture.DateTimeFormat,
                                               DateTimeStyles.None);
                }
                catch {}

                return DateTime.MinValue;
            }

            public override int ValueRepresentation() {
                return 0x544D;
            }

            public override DateTime GetDate(int index) {
                return Parse(GetString(index, null));
            }

            public override DateTime[] GetDateRange(int index) {
                return StringUtils.ParseDateTimeRange(GetString(index, null), Parse);
            }
        }

        #endregion

        #region Nested type: Trim

        /// <summary>
        /// Define how to trim string
        /// </summary>
        internal delegate String Trim(String s);

        #endregion

        #region Nested type: UI

        /// <summary>
        /// UI
        /// </summary>
        private sealed class UI : AsciiMultiStringElement {
            internal UI(uint tag, String value)
                : base(tag, value, 64, false, NO_TRIM, null, null) {}

            internal UI(uint tag, String[] values)
                : base(tag, values, 64, false, NO_TRIM, null, null) {}

            internal UI(uint tag, ByteBuffer data) : base(tag, data, NO_TRIM) {}

            public override int ValueRepresentation() {
                return 0x5549;
            }

            protected override String DoCheck(String s) {
                char[] a = s.ToCharArray();
                if (a.Length > maxLen) {
                    log.Warn("Value: " + s + " exeeds VR Length limit: " + maxLen);
                }
                int state = StringUtils.UID_DIGIT1;
                for (int i = 0; i < a.Length; ++i) {
                    if ((state = StringUtils.NextState(state, a[i])) == StringUtils.UID_ERROR) {
                        log.Warn("Illegal UID value: " + s);
                        return s;
                    }
                }
                if (state == StringUtils.UID_DIGIT1) {
                    log.Warn("Illegal UID value: " + s);
                }
                return s;
            }
        }

        #endregion

        #region Nested type: UT

        /// <summary>
        /// UT
        /// </summary>
        private sealed class UT : StringElement {
            internal UT(uint tag, String value, Encoding encoding)
                : base(tag, value, Int32.MaxValue, true, TRAIL_TRIM, null, encoding) {}

            internal UT(uint tag, String[] values, Encoding encoding)
                : base(tag, values, Int32.MaxValue, true, TRAIL_TRIM, null, encoding) {}

            internal UT(uint tag, ByteBuffer data) : base(tag, data, NO_TRIM) {}

            public override int ValueRepresentation() {
                return 0x5554;
            }
        }

        #endregion
    }
}