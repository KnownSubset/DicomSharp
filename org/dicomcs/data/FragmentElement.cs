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
// 7/23/08: Solved bug by Marcel de Wijs. virtual changed into override (changed lines 128, 206).
#endregion

namespace org.dicomcs.data
{
	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Collections;
	using System.Text;
	using org.dicomcs.dict;
	using org.dicomcs.util;
		
	/// <summary>
	/// </summary>
	public abstract class FragmentElement : DcmElement
	{		
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private ArrayList m_list = new ArrayList();
		
		/// <summary>
		/// Creates a new instance of ElementImpl 
		/// </summary>
		public FragmentElement(uint tag):base(tag)
		{
		}
		
		public override int vm()
		{
			return m_list.Count;
		}
		
		public override bool HasDataFragments()
		{
			return true;
		}
		
		public override ByteBuffer GetDataFragment(int index)
		{
			if (index >= vm())
			{
				return null;
			}

			int offsetSize = Marshal.SizeOf(typeof(uint)),
				end = m_list.Count-1;

			ByteBuffer data = (ByteBuffer) m_list[index];

			if ((0 == index)
			&&  (this.tag() == org.dicomcs.dict.Tags.PixelData)
			&&  (data.length() == (end * offsetSize)))
			{
				uint nOffsetCorrection = 0;
				ByteBuffer mybuffy = new ByteBuffer((int)data.Length, data.GetOrder());

				for (int i = 1; i < end; i++ )
				{
					uint sizeofElement = (uint)((ByteBuffer)m_list[i]).length();

					nOffsetCorrection += (uint) (sizeofElement + (((sizeofElement & 0x01) == 0x01) ? 9 : 8));

					mybuffy.Write((i * offsetSize), (int)nOffsetCorrection);
				}

				// set the data to return.
				data = mybuffy;
				data.Position = 0;
			}

			return data;
		}
		
		public override ByteBuffer GetDataFragment(int index, ByteOrder byteOrder)
		{
			if (index >= vm())
			{
				return null;
			}

			int offsetSize = Marshal.SizeOf(typeof(uint)),
				end = m_list.Count-1;

			ByteBuffer data = (ByteBuffer) m_list[index];

			if ((0 == index)
				&&  (this.tag() == org.dicomcs.dict.Tags.PixelData)
				&&  (data.length() == (end * offsetSize)))
			{
				uint nOffsetCorrection = 0;
				ByteBuffer mybuffy = new ByteBuffer((int)data.Length, data.GetOrder());

				for (int i = 1; i < end; i++ )
				{
					uint sizeofElement = (uint)((ByteBuffer)m_list[i]).length();

					nOffsetCorrection += (uint) (sizeofElement + (((sizeofElement & 0x01) == 0x01) ? 9 : 8));

					mybuffy.Write((i * offsetSize), (int)nOffsetCorrection);
				}

				// set the data to return.
				data = mybuffy;
				data.Position = 0;
			}

			if (data.GetOrder() != byteOrder)
			{
				SwapOrder(data);
			}
			return data;
		}
		
		public override int GetDataFragmentLength(int index)
		{
			if (index >= vm())
			{
				return 0;
			}
			ByteBuffer data = (ByteBuffer) m_list[index];
			return (data.length() + 1) & (~ 1);
		}
		
		public override String GetString(int index, Encoding encoding)
		{
			return GetBoundedString(Int32.MaxValue, index, encoding);
		}
		
		public override System.String GetBoundedString(int maxLen, int index, Encoding encoding)
		{
			if (index >= vm())
			{
				return null;
			}
			return StringUtils.PromptValue(vr(), GetDataFragment(index), maxLen);
		}
		
		public virtual System.String[] GetStrings(Encoding encoding)
		{
			return GetBoundedStrings(System.Int32.MaxValue, encoding);
		}
		
		public override System.String[] GetBoundedStrings(int maxLen, Encoding encoding)
		{
			System.String[] a = new System.String[vm()];
			for (int i = 0; i < a.Length; ++i)
				a[i] = StringUtils.PromptValue(vr(), GetDataFragment(i), maxLen);
			return a;
		}
		
		public virtual int CalcLength()
		{
			int len = 8;
			for (int i = 0, n = vm(); i < n; ++i)
				len += GetDataFragmentLength(i) + 8;
			return len;
		}
		
		public override void  AddDataFragment(ByteBuffer data)
		{
			m_list.Add(data != null?data:EMPTY_VALUE);
		}
		
		protected internal virtual void  SwapOrder(ByteBuffer data)
		{
			data.SetOrder(Swap(data.GetOrder()));
		}
		
		/// <summary>
		/// OB
		/// </summary>
		private sealed class OB : FragmentElement
		{
			internal OB(uint tag):base(tag)
			{
			}
			
			public override int vr()
			{
				return VRs.OB;
			}
		}
		
		public static DcmElement CreateOB(uint tag)
		{
			return new FragmentElement.OB(tag);
		}
		
		/*
		private sealed class OF:FragmentElement
		{
			internal OF(uint tag):base(tag)
			{
			}
			
			public int vr()
			{
				return VRs.OF;
			}
			
			public void  AddDataFragment(ByteBuffer data)
			{
				if ((data.length() & 3) != 0)
				{
					log.warn("Ignore odd length fragment of " + org.dicomcs.dict.Tags.toString(tag) + " OF #" + data.length());
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
		
		/// <summary>
		/// OW
		/// </summary>
		private sealed class OW:FragmentElement
		{
			internal OW(uint tag):base(tag)
			{
			}
			
			public override int vr()
			{
				return VRs.OW;
			}
			
			public override void  AddDataFragment(ByteBuffer data)
			{
				if ((data.length() & 1) != 0)
				{
					log.Warn("Ignore odd length fragment of " + org.dicomcs.dict.Tags.ToHexString(tag()) + " OW #" + data.length());
					data = null;
				}
				base.AddDataFragment(data);
			}
			
			protected internal void  SwapOrder(ByteBuffer data)
			{
				SwapWords(data);
			}
		}
		
		public static DcmElement CreateOW(uint tag)
		{
			return new FragmentElement.OW(tag);
		}
		
		/// <summary>
		/// UN
		/// </summary>
		private sealed class UN : FragmentElement
		{
			internal UN(uint tag):base(tag)
			{
			}
			
			public override int vr()
			{
				return VRs.UN;
			}
		}
		
		public static DcmElement CreateUN(uint tag)
		{
			return new FragmentElement.UN(tag);
		}
		
		public override String ToString()
		{
			System.Text.StringBuilder sb = new StringBuilder(org.dicomcs.dict.Tags.ToHexString(tag()));
			sb.Append(",").Append(VRs.ToString(vr()));
			if (!IsEmpty())
			{
				 for (int i = 0, n = vm(); i < n; ++i)
				{
					sb.Append("\n\tFrag-").Append(i + 1).Append(",#").Append(GetDataFragmentLength(i)).Append("[").Append(StringUtils.PromptValue(vr(), GetDataFragment(i), 64)).Append("]");
				}
			}
			return sb.ToString();
		}
	}
}