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
using System.Text;

namespace DicomSharp.Utility {
    /// <summary>
    /// Summary description for ByteBuffer.
    /// </summary>
    public class ByteBuffer : MemoryStream {
        private ByteOrder _order = ByteOrder.LittleEndian;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public ByteBuffer(byte[] buf, ByteOrder order) : base(buf) {
            SetOrder(order);
        }

        public ByteBuffer(byte[] buf, int offset, int size, ByteOrder order) : base(buf, offset, size) {
            SetOrder(order);
        }

        public ByteBuffer(int size, ByteOrder order) : base(size) {
            SetOrder(order);
        }

        public ByteBuffer() {}

        public int Remaining {
            get { return (int) (Length - Position); }
        }

        public ByteOrder GetOrder() {
            return _order;
        }

        public ByteBuffer SetOrder(ByteOrder order) {
            _order = order;

            // Both reader and writer work on the same back store: MemoryStream
            if (order == ByteOrder.LittleEndian) {
                _reader = new BinaryReader(this);
                _writer = new BinaryWriter(this);
            }
            else {
                _reader = new BEBinaryReader(this);
                _writer = new BEBinaryWriter(this);
            }
            return this;
        }

        public ByteBuffer Rewind() {
            Position = 0;
            return this;
        }

        public ByteBuffer Clear() {
            Position = 0;
            SetLength(0);
            return this;
        }

        /// <summary>
        /// Skip bytes
        /// </summary>
        /// <param name="count">How many bytes to skip</param>
        /// <returns>Actual bytes skipped</returns>
        public int Skip(int count) {
            var old = (int) Position;
            Position += count;
            if (Position > Length) {
                return (int) Length - old;
            }
            return count;
        }

        /// <summary>
        /// Skip one byte
        /// </summary>
        /// <returns>Actual bytes skipped</returns>
        public int Skip() {
            return Skip(1);
        }


        /// <summary>
        /// ByteBuffer
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(ByteBuffer data) {
            _writer.Write(data.ToArray());
            return this;
        }

        public virtual ByteBuffer ReadBuffer(int len) {
            _reader.ReadBytes(len);
            return this;
        }

        public virtual ByteBuffer ReadBuffer(int offset, int len) {
            Position = offset;
            _reader.ReadBytes(len);
            return this;
        }

        /// <summary>
        /// Byte
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(byte value) {
            _writer.Write(value);
            return this;
        }

        public new virtual byte ReadByte() {
            return _reader.ReadByte();
        }

        public virtual ByteBuffer Write(byte value, int off) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual byte ReadByte(int off) {
            Position = off;
            return _reader.ReadByte();
        }

        /// <summary>
        /// Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(byte[] value) {
            _writer.Write(value);
            return this;
        }

        /// <summary>
        /// Short
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(short value) {
            _writer.Write(value);
            return this;
        }

        public virtual short ReadInt16() {
            return _reader.ReadInt16();
        }

        public virtual ByteBuffer Write(int off, short value) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual short ReadInt16(int off) {
            Position = off;
            return _reader.ReadInt16();
        }

        /// <summary>
        /// Int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(int value) {
            _writer.Write(value);
            return this;
        }

        public virtual int ReadInt32() {
            return _reader.ReadInt32();
        }

        public virtual ByteBuffer Write(int off, int value) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual int ReadInt32(int off) {
            Position = off;
            return _reader.ReadInt32();
        }

        /// <summary>
        /// Long
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(long value) {
            _writer.Write(value);
            return this;
        }

        public virtual long ReadInt64() {
            return _reader.ReadInt64();
        }

        public virtual ByteBuffer Write(int off, long value) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual long ReadInt64(int off) {
            Position = off;
            return _reader.ReadInt64();
        }

        /// <summary>
        /// Float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(float value) {
            _writer.Write(value);
            return this;
        }

        public virtual float ReadSingle() {
            return _reader.ReadSingle();
        }

        public virtual ByteBuffer Write(int off, float value) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual float ReadSingle(int off) {
            Position = off;
            return _reader.ReadSingle();
        }

        /// <summary>
        /// Double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(Double value) {
            _writer.Write(value);
            return this;
        }

        public virtual Double ReadDouble() {
            return _reader.ReadDouble();
        }

        public virtual ByteBuffer Write(int off, Double value) {
            Position = off;
            _writer.Write(value);
            return this;
        }

        public virtual Double ReadDouble(int off) {
            Position = off;
            return _reader.ReadDouble();
        }

        /// <summary>
        /// String
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(String value) {
            _writer.Write(Encoding.ASCII.GetBytes(value));
            return this;
        }

        public virtual String ReadString() {
            Rewind();
            return ReadString((int) Length);
        }

        public virtual String ReadString(int length) {
            var b = new byte[length];
            _reader.Read(b, 0, length);
            while (length > 0 && b[length - 1] == 0) {
                --length;
            }
            return Encoding.ASCII.GetString(b, 0, length).Trim();
        }

        /// <summary>
        /// Boolean
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ByteBuffer Write(bool value) {
            _writer.Write(value);
            return this;
        }

        public virtual bool ReadBoolean() {
            return _reader.ReadBoolean();
        }

        public override String ToString() {
            var buf = new StringBuilder();

            byte[] arr = ToArray();
            foreach (byte b in arr) {
                buf.Append(String.Format("{0:X2} ", b));
            }

            return buf.ToString();
        }

        ///////////////////////////////////////////////////////////////////////
        /// Public Class Methods
        ///////////////////////////////////////////////////////////////////////
        public static ByteBuffer Wrap(byte[] buf) {
            return Wrap(buf, ByteOrder.LittleEndian);
        }

        public static ByteBuffer Wrap(byte[] buf, ByteOrder order) {
            return new ByteBuffer(buf, order);
        }

        public static ByteBuffer Wrap(byte[] buf, int offset, int len) {
            return Wrap(buf, offset, len, ByteOrder.LittleEndian);
        }

        public static ByteBuffer Wrap(byte[] buf, int offset, int len, ByteOrder order) {
            return new ByteBuffer(buf, offset, len, order);
        }

        ///////////////////////////////////////////////////////////////////////
        /// Restricted Methods: Protected, Internal, Private
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        /// Main() For Testing
        ///////////////////////////////////////////////////////////////////////
        public static void Main() {
            ByteBuffer buf = Wrap(new Byte[40], ByteOrder.LittleEndian);
            buf.Write(0x01020304);
            buf.Write(16, (short) 10);
            //buf.PutInt16( 11 );
            buf.Write((float) 123.345);

            Console.WriteLine(buf.ToString());

            buf.Rewind();
            int n = buf.ReadInt32();
            Console.WriteLine("int = 0x{0:X8}", n);

            float f = buf.ReadSingle();
            Console.WriteLine("float = {0}", f);

            //buf.PutInt16( 9 );
            //Console.WriteLine( buf.ToString() );
        }
    }

    /// <summary>
    /// Encoding byte order
    /// </summary>
    public class ByteOrder {
        public static readonly ByteOrder BigEndian = new ByteOrder("BIG_ENDIAN");
        public static readonly ByteOrder LittleEndian = new ByteOrder("LITTLE_ENDIAN");
        private readonly String _name;

        private ByteOrder(String name) {
            _name = name;
        }

        public override String ToString() {
            return _name;
        }
    }
}