#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2008 Fang Yang. All rights reserved.
// 
// This file is part of dicomcs, see http://www.sourceforge.net/projects/dicom-cs
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
// Fang Yang (yangfang@email.com)
//
#endregion

namespace org.dicomcs.data
{
	using System;
	using System.Text;
	using System.Reflection;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	public abstract class ValueElement : DcmElement
	{		
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected ByteBuffer m_data;

		public override ByteBuffer GetByteBuffer()
		{
			return m_data;
		}

		public virtual void SetByteBuffer( ByteBuffer data )
		{
			m_data = data;
		}

		/// <summary>
		/// Create a element for Short value
		/// </summary>
		private static ByteBuffer SetShort( int value )
		{
			return ByteBuffer.Wrap(new byte[2], ByteOrder.LITTLE_ENDIAN).Write( (short)value);
		}

		private static ByteBuffer SetShorts( int[] value )
		{
			if (value.Length == 0)
				return EMPTY_VALUE;
			
			if (value.Length == 1)
				return SetShort( value[0] );
			
			ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 1], ByteOrder.LITTLE_ENDIAN);
			for (int i = 0; i < value.Length; ++i)
				bb.Write((short) value[i]);
			return bb;
		}

		/// <summary>
		/// Create a element for Int value
		/// </summary>
		private static ByteBuffer SetInt( int value )
		{
			return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write(value);
		}

		private static ByteBuffer SetInts( int[] value )
		{
			if (value.Length == 0)
				return EMPTY_VALUE;
			
			if (value.Length == 1)
				return SetInt(value[0]);
			
			ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
			for (int i = 0; i < value.Length; ++i)
				bb.Write(value[i]);
			return bb;
		}

		/// <summary>
		/// Create a element for Tag value
		/// </summary>
		private static ByteBuffer SetTag( int value )
		{
			return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write((short) (value >> 8)).Write((short) value);
		}
		private static ByteBuffer SetTags( int[] value )
		{
			if (value.Length == 0)
				return EMPTY_VALUE;
			
			if (value.Length == 1)
				return SetTag(value[0]);
			
			ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
			for (int i = 0; i < value.Length; ++i)
				bb.Write((short) (value[i] >> 16)).Write((short) value[i]);
			return bb;
		}

		/// <summary>
		/// Create a element for Float value
		/// </summary>
		private static ByteBuffer SetFloat( float value )
		{
			return ByteBuffer.Wrap(new byte[4], ByteOrder.LITTLE_ENDIAN).Write(value);
		}
		private static ByteBuffer SetFloats( float[] value )
		{
			if (value.Length == 0)
				return EMPTY_VALUE;
			
			if (value.Length == 1)
				return SetFloat(value[0]);
			
			ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 2], ByteOrder.LITTLE_ENDIAN);
				for (int i = 0; i < value.Length; ++i)
				bb.Write(value[i]);
			return bb;
		}
		
		/// <summary>
		/// Create a element for Double value
		/// </summary>
		private static ByteBuffer SetDouble( Double value )
		{
			return ByteBuffer.Wrap(new byte[8], ByteOrder.LITTLE_ENDIAN).Write(value);
		}
		private static ByteBuffer SetDoubles( Double[] value )
		{
			if (value.Length == 0)
				return EMPTY_VALUE;
			
			if (value.Length == 1)
				return SetDouble(value[0]);
			
			ByteBuffer bb = ByteBuffer.Wrap(new byte[value.Length << 3], ByteOrder.LITTLE_ENDIAN);
				for (int i = 0; i < value.Length; ++i)
				bb.Write(value[i]);
			return bb;
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="data"></param>
		internal ValueElement(uint tag, ByteBuffer data):base(tag)
		{
			this.m_data = data;
		}
		
		public override int length()
		{
			return (int)((m_data.length() + 1) & (~ 1));
		}
		
		
		public override ByteBuffer GetByteBuffer(ByteOrder byteOrder)
		{
			if (m_data.GetOrder() != byteOrder)
				SwapOrder();
			return (ByteBuffer) m_data.Rewind();
		}
		
		public override int vm()
		{
			return m_data.length() <= 0 ? 0 : m_data.length();
		}
		
		public override String GetString(int index, Encoding encoding)
		{
			if (index >= vm())
			{
				return null;
			}
			return GetInt(index).ToString();
		}
		
		public override String[] GetStrings(Encoding encoding)
		{
			String[] a = new String[vm()];
			 for (int i = 0; i < a.Length; ++i)
				a[i] = GetInt(i).ToString();
			return a;
		}
		
		public virtual void  SwapOrder()
		{
			m_data.SetOrder(Swap(m_data.GetOrder()));
		}
		
		/// <summary>
		/// SS - Signed short, 2 bytes fixed
		/// </summary>
		private sealed class SS : ValueElement
		{
			public override int[] Ints
			{
				get
				{
					int[] a = new int[vm()];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetInt(i);
					return a;
				}				
			}
			internal SS(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x5353;
			}
			
			public override int vm()
			{
				return (int)m_data.length() >> 1;
			}
			
			public override int GetInt(int index)
			{
				if (index >= vm())
				{
					return 0;
				}
				return m_data.ReadInt16(index << 1);
			}
			
			
			public override void  SwapOrder()
			{
				SwapWords(m_data);
			}
		}
		
		internal static DcmElement CreateSS(uint tag, ByteBuffer data)
		{
			if ((data.length() & 1) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " SS #" + data.length());
				return new SS(tag, EMPTY_VALUE);
			}
			return new SS(tag, data);
		}
		
		internal static DcmElement CreateSS(uint tag)
		{
			return new SS(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateSS(uint tag, int v)
		{
			return new SS(tag, SetShort(v));
		}
		
		internal static DcmElement CreateSS(uint tag, int[] a)
		{
			return new SS(tag, SetShorts(a) );
		}
		
		/// <summary>
		/// US - Unsigned short, 2 bytes fixed
		/// </summary>
		private sealed class US : ValueElement
		{
			public override int[] Ints
			{
				get
				{
					int[] a = new int[vm()];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetInt(i);
					return a;
				}				
			}
			internal US(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x5553;
			}
			
			public override int vm()
			{
				return m_data.length() >> 1;
			}
			
			public override int GetInt(int index)
			{
				if (index >= vm())
				{
					return 0;
				}
				return m_data.ReadInt16(index << 1) & 0xffff;
			}
			
			
			public override void SwapOrder()
			{
				SwapWords(m_data);
			}
		}
		
		internal static DcmElement CreateUS(uint tag, ByteBuffer data)
		{
			if ((data.length() & 1) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " US #" + data.length());
				return new US(tag, EMPTY_VALUE);
			}
			return new US(tag, data);
		}
		
		internal static DcmElement CreateUS(uint tag)
		{
			return new US(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateUS(uint tag, int s)
		{
			return new US(tag, SetShort(s));
		}
		
		internal static DcmElement CreateUS(uint tag, int[] s)
		{
			return new US(tag, SetShorts(s) );
		}
		
		/// <summary>
		/// Int - SL, UL
		/// </summary>
		internal abstract class IntBase : ValueElement
		{
			public override int[] Ints
			{
				get
				{
					int[] a = new int[vm()];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetInt(i);
					return a;
				}				
			}
			internal IntBase(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int GetInt(int index)
			{
				if (index >= vm())
				{
					return 0;
				}
				return m_data.ReadInt32( index << 2 );
			}
			
			
			public override int vm()
			{
				return (int)m_data.length() >> 2;
			}
			
			public override void  SwapOrder()
			{
				SwapInts(m_data);
			}
		}
		
		/// <summary>
		/// SL
		/// </summary>
		internal class SL : IntBase
		{
			internal SL(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x534C;
			}
		}
		
		internal static DcmElement CreateSL(uint tag, ByteBuffer data)
		{
			if ((data.length() & 3) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " SL #" + data.length());
				return new SL(tag, EMPTY_VALUE);
			}
			return new SL(tag, data);
		}
		
		internal static DcmElement CreateSL(uint tag)
		{
			return new SL(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateSL(uint tag, int v)
		{
			return new SL(tag, SetInt(v) );
		}
		
		internal static DcmElement CreateSL(uint tag, int[] a)
		{
			return new SL(tag, SetInts(a) );
		}
		
		/// <summary>
		/// UL - Unsigned long, 4 bytes fixed
		/// </summary>
		internal class UL : IntBase
		{
			internal UL(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x554C;
			}
			
			public override String GetString(int index, Encoding encoding)
			{
				if (index >= vm())
				{
					return null;
				}
				return GetInt(index).ToString();
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetInt(i).ToString();
				return a;
			}
		}
		
		internal static DcmElement CreateUL(uint tag, ByteBuffer data)
		{
			if ((data.length() & 3) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " UL #" + data.length());
				return new UL(tag, EMPTY_VALUE);
			}
			return new UL(tag, data);
		}
		
		internal static DcmElement CreateUL(uint tag)
		{
			return new UL(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateUL(uint tag, int v)
		{
			return new UL(tag, SetInt(v));
		}
		
		internal static DcmElement CreateUL(uint tag, int[] a)
		{
			return new UL(tag, SetInts(a) );
		}
		
		/// <summary>
		/// AT - Attribute tag, 4 bytes fixed
		/// </summary>
		internal sealed class AT : ValueElement
		{
			public override uint[] Tags
			{
				get
				{
					uint[] a = new uint[vm()];
					for (int i = 0; i < a.Length; ++i)
						a[i] = GetTag(i);
					return a;
				}				
			}
			internal AT(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x4154;
			}
			
			public override int vm()
			{
				return (int)m_data.length() >> 2;
			}
			
			public override uint GetTag(int index)
			{
				if (index >= vm())
				{
					return 0;
				}
				int pos = index << 2;
				return (uint) (((m_data.ReadInt16(pos) << 16) | (m_data.ReadInt16(pos + 2) & 0xffff)));
			}
			
			
			public override String GetString(int index, Encoding encoding)
			{
				if (index >= vm())
					return null;
				return org.dicomcs.dict.Tags.ToHexString(GetTag(index));
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetString(i, null);
				return a;
			}
			
			public override void  SwapOrder()
			{
				SwapWords(m_data);
			}
		}
		
		internal static DcmElement CreateAT(uint tag, ByteBuffer data)
		{
			if ((data.length() & 3) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " AT #" + data.length());
				return new AT(tag, EMPTY_VALUE);
			}
			return new AT(tag, data);
		}
		
		internal static DcmElement CreateAT(uint tag)
		{
			return new AT(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateAT(uint tag, int v)
		{
			return new AT(tag, SetTag(v));
		}
		
		internal static DcmElement CreateAT(uint tag, int[] a)
		{
			return new AT(tag, SetTags(a));
		}
		
		/// <summary>
		/// FL - Floating point single, 4 bytes fixed
		/// </summary>
		
		internal sealed class FL : ValueElement
		{
			public override float[] Floats
			{
				get
				{
					float[] a = new float[vm()];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetFloat(i);
					return a;
				}				
			}
			internal FL(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vm()
			{
				return (int)m_data.length() >> 2;
			}
			
			public override int vr()
			{
				return 0x464C;
			}
			
			public override float GetFloat(int index)
			{
				if (index >= vm())
				{
					return 0.0f;
				}
				return m_data.ReadSingle(index << 2);
			}
			
			
			public override String GetString(int index, Encoding encoding)
			{
				if (index >= vm())
				{
					return null;
				}
				return GetFloat(index).ToString();
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetFloat(i).ToString();
				return a;
			}
			
			public override void  SwapOrder()
			{
				SwapInts(m_data);
			}
		}
		
		internal static DcmElement CreateFL(uint tag, ByteBuffer data)
		{
			if ((data.length() & 3) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " FL #" + data.length());
				return new FL(tag, EMPTY_VALUE);
			}
			
			return new FL(tag, data);
		}
		
		internal static DcmElement CreateFL(uint tag)
		{
			return new FL(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateFL(uint tag, float v)
		{
			return new FL(tag, SetFloat(v));
		}
		
		internal static DcmElement CreateFL(uint tag, float[] a)
		{
			return new FL(tag, SetFloats(a));
		}
		
		/// <summary>
		/// FD - Floating point Double, 8 bytes fixed
		/// </summary>
		internal sealed class FD : ValueElement
		{
			public override Double[] Doubles
			{
				get
				{
					double[] a = new Double[vm()];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetDouble(i);
					return a;
				}				
			}
			internal FD(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vm()
			{
				return m_data.length() >> 3;
			}
			
			public override int vr()
			{
				return 0x4644;
			}
			
			public override Double GetDouble(int index)
			{
				if (index >= vm())
				{
					return 0.0;
				}
				return m_data.ReadDouble(index << 3);
			}
			
			
			public override String GetString(int index, Encoding encoding)
			{
				if (index >= vm())
					return null;
				return GetDouble(index).ToString();
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetDouble(i).ToString();
				return a;
			}
			
			public override void  SwapOrder()
			{
				SwapLongs(m_data);
			}
		}
		
		internal static DcmElement CreateFD(uint tag, ByteBuffer data)
		{
			if ((data.length() & 7) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " FD #" + data.length());
				return new FD(tag, EMPTY_VALUE);
			}
			return new FD(tag, data);
		}
		
		internal static DcmElement CreateFD(uint tag)
		{
			return new FD(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateFD(uint tag, Double v)
		{
			return new FD(tag, SetDouble(v));
		}
		
		internal static DcmElement CreateFD(uint tag, Double[] a)
		{
			return new FD(tag, SetDoubles(a));
		}
		
		/// <summary>
		/// OW - Other word (2 bytes) string, depending on Transfer Syntax
		/// </summary>
		internal sealed class OW : ValueElement
		{
			public override int[] Ints
			{
				get
				{
					int[] a = new int[m_data.length() >> 1];
					 for (int i = 0; i < a.Length; ++i)
						a[i] = GetInt(i);
					return a;
				}				
			}
			internal OW(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x4F57;
			}
			
			public override int GetInt(int index)
			{
				if (index >= vm())
				{
					return 0;
				}
				return m_data.ReadInt16(index << 1) & 0xffff;
			}
			
			
			public override String GetString(int index, Encoding encoding)
			{
				return GetBoundedString(Int32.MaxValue, index, encoding);
			}
			
			public override String GetBoundedString(int maxLen, int index, Encoding encoding)
			{
				if (index >= vm())
				{
					return null;
				}
				return StringUtils.PromptOW(m_data, maxLen);
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				return GetBoundedStrings(Int32.MaxValue, encoding);
			}
			
			public override String[] GetBoundedStrings(int maxLen, Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetBoundedString(maxLen, i, null);
				return a;
			}
			
			public override void  SwapOrder()
			{
				SwapWords(m_data);
			}
		}
		
		internal static DcmElement CreateOW(uint tag)
		{
			return new OW(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateOW(uint tag, short[] v)
		{
			ByteBuffer buf = ByteBuffer.Wrap( new byte[v.Length << 1]);
			buf.SetOrder( ByteOrder.LITTLE_ENDIAN );
			 for (int i = 0; i < v.Length; ++i)
				buf.Write(v[i]);

			return new OW(tag, buf);
		}

		internal static DcmElement CreateOW(uint tag, short[][] v)
		{
			int len2 = v[0].Length;

			ByteBuffer buf = ByteBuffer.Wrap( new byte[(v.Length * len2) << 1]);
			buf.SetOrder( ByteOrder.LITTLE_ENDIAN );

			for (int j = 0; j < len2;j++)
				for (int i = 0; i < v.Length; ++i)
					buf.Write(v[i][j]);

			return new OW(tag, buf);
		}
		
		internal static DcmElement CreateOW(uint tag, ByteBuffer data)
		{
			if ((data.length() & 1) != 0)
			{
				log.Warn("Ignore illegal value of " + org.dicomcs.dict.Tags.ToHexString(tag) + " OW #" + data.length());
				return new OW(tag, EMPTY_VALUE);
			}
			return new OW(tag, data);
		}
		
		/// <summary>
		/// OB - Other byte string, depending on transfer syntax
		/// </summary>
		internal sealed class OB:ValueElement
		{
			internal OB(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x4F42;
			}
			
			public override String GetString(int index, Encoding encoding)
			{
				return GetBoundedString(Int32.MaxValue, index, encoding);
			}
			
			public override String GetBoundedString(int maxLen, int index, Encoding encoding)
			{
				if (index >= vm())
				{
					return null;
				}
				return StringUtils.PromptOB(m_data, maxLen);
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				return GetBoundedStrings(Int32.MaxValue, encoding);
			}
			
			public override String[] GetBoundedStrings(int maxLen, Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetBoundedString(maxLen, i, null);
				return a;
			}
		}
		
		internal static DcmElement CreateOB(uint tag)
		{
			return new OB(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateOB(uint tag, ByteBuffer v)
		{
			return new OB(tag, v);
		}
		
		internal static DcmElement CreateOB(uint tag, byte[] v)
		{
			return new OB(tag, ByteBuffer.Wrap(v, ByteOrder.LITTLE_ENDIAN));
		}
		
		/// <summary>
		/// UN - Unkown, a string of bytes, any length
		/// </summary>
		internal sealed class UN : ValueElement
		{
			internal UN(uint tag, ByteBuffer data):base(tag, data)
			{
			}
			
			public override int vr()
			{
				return 0x554E;
			}
			
			public override String GetString(int index, Encoding encoding)
			{
				return GetBoundedString(Int32.MaxValue, index, encoding);
			}
			
			public override String GetBoundedString(int maxLen, int index, Encoding encoding)
			{
				if (index >= vm())
				{
					return null;
				}
				return StringUtils.PromptOB(m_data, maxLen);
			}
			
			public override String[] GetStrings(Encoding encoding)
			{
				return GetBoundedStrings(Int32.MaxValue, encoding);
			}
			
			public override String[] GetBoundedStrings(int maxLen, Encoding encoding)
			{
				String[] a = new String[vm()];
				 for (int i = 0; i < a.Length; ++i)
					a[i] = GetBoundedString(maxLen, i, null);
				return a;
			}
		}
		
		internal static DcmElement CreateUN(uint tag)
		{
			return new UN(tag, EMPTY_VALUE);
		}
		
		internal static DcmElement CreateUN(uint tag, ByteBuffer v)
		{
			return new UN(tag, v);
		}
		
		internal static DcmElement CreateUN(uint tag, byte[] v)
		{
			return new UN(tag, ByteBuffer.Wrap(v, ByteOrder.LITTLE_ENDIAN));
		}
	}
}