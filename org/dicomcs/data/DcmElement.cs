#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002 Fang Yang. All rights reserved.
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
	using System.IO;
	using System.Text;
	using System.Reflection;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.util;

	public class DcmElement : IComparable
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType);

		public static ByteBuffer EMPTY_VALUE = new ByteBuffer();
		public static byte[] EMPTY_BYTE_ARRAY = new byte[]{};

		private uint tagValue;
		private long streamPos = - 1L;

		public virtual long StreamPosition
		{
			get { return streamPos; }			
			set { this.streamPos = value; }			
		}

		public virtual ByteBuffer GetByteBuffer()
		{
			throw new NotSupportedException(this.ToString());
		}

		/// <summary>
		/// Get element as Int value
		/// </summary>
		public virtual int Int
		{
			get { return GetInt(0); }			
		}
		public virtual int[] Ints
		{
			get { throw new NotSupportedException(this.ToString()); }			
		}

		/// <summary>
		/// Get element as Tag value
		/// </summary>
		public virtual uint Tag
		{
			get { return GetTag(0); }			
		}
		public virtual uint[] Tags
		{
			get { throw new NotSupportedException(this.ToString()); }			
		}

		/// <summary>
		/// Get element as Float value
		/// </summary>
		public virtual float Float
		{
			get { return GetFloat(0); }	
		}
		public virtual float[] Floats
		{
			get { throw new NotSupportedException(this.ToString()); }
		}

		/// <summary>
		/// Get element as Double value
		/// </summary>
		public virtual Double Double
		{
			get { return GetDouble(0); }			
		}
		public virtual Double[] Doubles
		{
			get { throw new NotSupportedException(this.ToString()); }			
		}

		/// <summary>
		/// Get element as Datatime value
		/// </summary>
		public virtual DateTime Date
		{
			get { return GetDate(0); }			
		}
		public virtual DateTime[] Dates
		{
			get { throw new NotSupportedException(this.ToString()); }			
		}
		public virtual DateTime[] DateRange
		{
			get { return GetDateRange(0); }			
		}

		/// <summary>
		/// Get element as Dataset value
		/// </summary>
		public virtual Dataset Item
		{
			get { return GetItem(0); }			
		}		

		/// <summary>
		/// Creates a new instance of Element
		/// </summary>
		public DcmElement(uint tag)
		{
			this.tagValue = tag;
		}
		
		public uint tag()
		{
			return tagValue;
		}

		public void SetTag( uint tag )
		{
			tagValue = tag;
		}
		
		public virtual int vr()
		{
			return VRs.NONE;
		}
		
		public virtual int vm()
		{
			return 0;
		}
		
		public virtual bool IsEmpty()
		{
			return vm() == 0;
		}
		
		public virtual int length()
		{
			return - 1;
		}
				
		public virtual int CompareTo(Object o)
		{ 
			if( o == null ) return 1;	// null sorts before current

			if( !( o is DcmElement ))		
				throw new ArgumentException( "The current object is not type of DcmElement" );
   
			return (int) ((tagValue & 0xffffffffL) - (((DcmElement)o).tagValue & 0xffffffffL));
		}
		
		public override String ToString()
		{
			int vr1 = vr();
			ByteBuffer bb = GetByteBuffer();
			String val = StringUtils.PromptValue( vr1, bb, 64 );
			String tmp = org.dicomcs.dict.Tags.ToHexString(tag()) + "," + VRs.ToString(vr()) + ",*" + vm() + ",#" + length() + ",[" + val + "]";
			return tmp;
		}
						
		public virtual ByteBuffer GetByteBuffer(ByteOrder byteOrder)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual bool HasDataFragments()
		{
			return false;
		}
		
		public virtual ByteBuffer GetDataFragment(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual ByteBuffer GetDataFragment(int index, ByteOrder byteOrder)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual int GetDataFragmentLength(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public String GetString(Encoding encoding)
		{
			return GetString(0, encoding);
		}
		
		public virtual String GetString(int index, Encoding encoding)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual String[] GetStrings(Encoding encoding)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public String GetBoundedString(int maxLen, Encoding encoding)
		{
			return GetBoundedString(maxLen, 0, encoding);
		}
		
		public virtual String GetBoundedString(int maxLen, int index, Encoding encoding)
		{
			return GetString(index, encoding);
		}
		
		public virtual String[] GetBoundedStrings(int maxLen, Encoding encoding)
		{
			return GetStrings(encoding);
		}
				
		public virtual int GetInt(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual uint GetTag(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual float GetFloat(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual Double GetDouble(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual DateTime GetDate(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual DateTime[] GetDateRange(int index)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual void  AddDataFragment(ByteBuffer byteBuffer)
		{
			throw new NotSupportedException(this.ToString());
		}
		
		public virtual bool HasItems()
		{
			return false;
		}		
		public virtual Dataset AddNewItem()
		{
			throw new NotSupportedException(this.ToString());
		}		
		public virtual void  AddItem(Dataset item)
		{
			throw new NotSupportedException(this.ToString());
		}		
		public virtual Dataset GetItem(int index)
		{
			throw new NotSupportedException(this.ToString());
		}

		internal static ByteOrder Swap(ByteOrder from)
		{
			return from == ByteOrder.LITTLE_ENDIAN ? 
				ByteOrder.BIG_ENDIAN : ByteOrder.LITTLE_ENDIAN;
		}
		
		internal static void SwapWords(ByteBuffer bb)
		{
			if ((bb.Length & 1) != 0)
				throw new ArgumentException("illegal value length: " + bb);
			
			ByteOrder from = bb.GetOrder();
			ByteOrder to = Swap( from );
			short tmp;
			for (int i = 0, n = (int)bb.Length; i < n; i += 2)
			{
				tmp = bb.ReadInt16(i);
				bb.SetOrder(to).Write(i, tmp).SetOrder(from);
			}
			bb.SetOrder(to);
		}
		
		internal static void SwapInts(ByteBuffer bb)
		{
			if ((bb.Length & 3) != 0)
				throw new ArgumentException("illegal value length " + bb);
			
			ByteOrder from = bb.GetOrder();
			ByteOrder to = Swap(from);
			int tmp;
			for (int i = 0, n = (int)bb.Length; i < n; i += 4)
			{
				tmp = bb.ReadInt32( i);
				bb.SetOrder(to).Write(i, tmp).SetOrder(from);
			}
			bb.SetOrder(to);
		}
		
		internal static void  SwapLongs(ByteBuffer bb)
		{
			if ((bb.Length & 7) != 0)
				throw new ArgumentException("illegal value length " + bb);
			
			ByteOrder from = bb.GetOrder();
			ByteOrder to = Swap(from);
			long tmp;
			for (int i = 0, n = (int)bb.Length; i < n; i += 8)
			{
				tmp = bb.ReadInt64(i);
				bb.SetOrder(to).Write(i, tmp).SetOrder(from);
			}
			bb.SetOrder(to);
		}
	}
}