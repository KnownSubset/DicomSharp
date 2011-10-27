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

namespace org.dicomcs.util
{
	using System;
	using System.Reflection;
	using System.Text;
	using org.dicomcs.dict;
	
	/// <summary>
	/// String utility class
	/// </summary>
	public class StringUtils
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Prevent instances of Utility class 
		/// </summary>
		public StringUtils()
		{
		}
		
		public static float[] ParseFloats(String[] values)
		{
			float[] retval = new float[values.Length];
			for (int i = 0; i < retval.Length; ++i)
			{
				retval[i] = Single.Parse(values[i]);
			}
			return retval;
		}
		
		public static Double[] ParseDoubles(String[] values)
		{
			double[] retval = new Double[values.Length];
			for (int i = 0; i < retval.Length; ++i)
			{
				retval[i] = Double.Parse(values[i]);
			}
			return retval;
		}

		public static int[] ParseInts(String[] values, long min, long max)
		{
			int[] retval = new int[values.Length];
			for (int i = 0; i < retval.Length; ++i)
			{
				retval[i] = ParseInt(values[i], min, max);
			}
			return retval;
		}

		public static int ParseInt(String value, long min, long max)
		{
			long retval = Int64.Parse(value);
			if (retval < min || retval > max)
			{
				throw new System.FormatException("value: " + value+ ", min:" + min + ", max:" + max);
			}
			return (int) retval;
		}

		public static StringBuilder PromptBytes( StringBuilder sb, byte[] data, int start, int length, int maxlen)
		{
			if (length == 0)
				return sb;
			sb.Append( String.Format( "{0:X2}", data[start] ) );
			int i = start + 1;
			int remain = Math.Min(length, (maxlen - 2) / 3);
			for (; --remain > 0; ++i)
			{
				sb.Append('\\');
				sb.Append( String.Format( "{0:X2}", data[i]));
			}

			// if limited by maxlen
			if (sb.Length < 3 * length - 1)
			{
				sb.Append( ".." );
			}

			return sb;
		}
		
		public static String PromptBytes(byte[] data, int start, int length, int maxlen)
		{
			if (length == 0)
				return "";
			StringBuilder sb = PromptBytes(new StringBuilder(Math.Min(maxlen, length * 3 - 1)), data, start, length, maxlen);
			String str = sb.ToString();
			return str;
		}
		
		public static String PromptBytes(byte[] data, int start, int length)
		{
			return PromptBytes(data, start, length, Int32.MaxValue);
		}
		
		public static String Truncate(String val, int maxlen)
		{
			return val.Length > maxlen?(val.Substring(0, (maxlen - 2) - (0)) + ".."):val;
		}
		
		public static String PromptValue(int vr, ByteBuffer bb)
		{
			return PromptValue(vr, bb, Int32.MaxValue);
		}
		
		public static String PromptValue(int vr, ByteBuffer bb, int maxlen)
		{
			if (bb.Length == 0)
				return "";
			
			if (VRs.IsStringValue(vr))
			{
				String val = bb.ReadString();
				val = Truncate( val, maxlen);
				return val;
			}
			
			switch (vr)
			{
				case VRs.AT: 
					return PromptAT(bb, maxlen);
				
				case VRs.FD: 
					return PromptFD(bb, maxlen);
				
				case VRs.FL: 
					return PromptFL(bb, maxlen);
				
				case VRs.OB: 
				case VRs.UN: 
					return PromptOB(bb, maxlen);
				
				case VRs.OW: 
					return PromptOW(bb, maxlen);
				
				case VRs.SL: 
					return PromptSL(bb, maxlen);
				
				case VRs.SS: 
					return PromptSS(bb, maxlen);
				
				case VRs.UL: 
					return PromptUL(bb, maxlen);
				
				case VRs.US: 
					return PromptUS(bb, maxlen);
				
			}
			throw new ArgumentException("VR:" + VRs.ToString(vr));
		}
		
		public static String PromptAT(ByteBuffer bb, int maxlen)
		{
			int l = (int)bb.Length / 4 * 9 - 1;
			if (l < 0)
				return "";
			
			StringBuilder sb = new StringBuilder(l);
			bb.Rewind();
			sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
			sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
			while (bb.Remaining >= 4 && sb.Length < maxlen)
			{
				sb.Append('\\');
				sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
				sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
			}
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptFD(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 8)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadDouble());
			while (bb.Remaining >= 8 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadDouble());
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptFL(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 4)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadSingle());
			while (bb.Remaining >= 4 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadSingle());
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptOB(ByteBuffer bb, int maxlen)
		{
			return PromptBytes(bb.ToArray(), (int)bb.Position, (int)bb.Length, maxlen);
		}
		
		public static String PromptOW(ByteBuffer bb, int maxlen)
		{
			int l = (int)bb.Length / 2 * 5 - 1;
			if (l < 0)
				return "";
			
			StringBuilder sb = new StringBuilder(l);
			bb.Rewind();
			sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
			while (bb.Remaining >= 2 && sb.Length < maxlen)
			{
				sb.Append('\\');
				sb.Append( String.Format( "{0:X4}", bb.ReadInt16() ) );
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptSL(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 4)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadInt32());
			while (bb.Remaining >= 4 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadInt32());
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptSS(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 2)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadInt16());
			while (bb.Remaining >= 2 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadInt16());
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptUL(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 4)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadInt32() & (int) (- (0x100000000 - 0xffffffffL)));
			while (bb.Remaining >= 4 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadInt32() & (int) (- (0x100000000 - 0xffffffffL)));
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		public static String PromptUS(ByteBuffer bb, int maxlen)
		{
			if (bb.Length < 2)
				return "";
			
			StringBuilder sb = new StringBuilder((int)bb.Length);
			bb.Rewind();
			sb.Append(bb.ReadInt16() & 0xffff);
			while (bb.Remaining >= 2 && sb.Length < maxlen)
			{
				sb.Append('\\').Append(bb.ReadInt16() & 0xffff);
			}
			
			return Truncate(sb.ToString(), maxlen);
		}
		
		
		public const int UID_DIGIT1 = 0;
		public const int UID_DIGIT = 1;
		public const int UID_DOT = 2;
		public const int UID_ERROR = -1;

		public static int NextState( int state, char c )
		{
			switch( state )
			{
				case UID_DIGIT1:
					if( c > '0' && c <= '9' )
						return UID_DIGIT;
					if( c == '0' )
						return UID_DOT;
					return UID_ERROR;

				case UID_DIGIT:
					if( c >= '0' && c <= '9' )
						return UID_DIGIT;
					goto case UID_DOT;
					
				case UID_DOT:
					if( c == '.' )
						return UID_DIGIT1;
					break;
			}
			return UID_ERROR;
		}
		
		public static String CheckUID(String s)
		{
			char[] a = s.ToCharArray();
			if (a.Length == 0 || a.Length > 64)
				throw new ArgumentException(s);
			
			int state = UID_DIGIT1;
			 for (int i = 0; i < a.Length; ++i)
			{
				if ((state = NextState(state, a[i])) == UID_ERROR)
					throw new ArgumentException(s);
			}
			if (state == UID_DIGIT1)
				throw new ArgumentException(s);
			
			return s;
		}
		
		public static String[] CheckUIDs(String[] a)
		{
			 for (int i = 0; i < a.Length; ++i)
			{
				CheckUID(a[i]);
			}
			return a;
		}
		
		public static String CheckAET(String s)
		{
			char[] a = s.ToCharArray();
			if (a.Length == 0 || a.Length > 16)
				throw new ArgumentException(s);
			
			 for (int i = 0; i < a.Length; ++i)
			{
				if (a[i] < '\u0020' || a[i] >= '\u007f')
					throw new ArgumentException(s);
			}
			return s;
		}
		
		public static String[] CheckAETs(String[] a)
		{
			 for (int i = 0; i < a.Length; ++i)
			{
				CheckAET(a[i]);
			}
			return a;
		}

		public delegate DateTime ParseDelegate(String val);

		public static DateTime[] ParseDateTimeRange(String date, ParseDelegate pd)
		{
			if (date == null || date.Length == 0)
				return null;
			
			int hypPos = date.IndexOf((Char) '-');
			DateTime[] range = new DateTime[2];
			range[1] = pd(date.Substring(hypPos + 1));
			if (hypPos == - 1)
				range[0] = range[1];
			else if (hypPos != 0)
				range[0] = pd(date.Substring(0, (hypPos) - (0)));
			
			return range;
		}

		public static bool IsDICOMControl(char c)
		{
			switch (c)
			{
				case '\n': case '\f': case '\r': case '\x001B': 
					return true;
				
			}
			return false;
		}

		public static void  dumpBytes( String name, byte[] req, int offset, int len)
		{
			return;

			StringBuilder buf = new StringBuilder("------------" + name + " - Bytes: " + len + " ------------");
			int all = offset + len;
			for (int i = 0; i < all; i++)
			{
				if (i % 16 == 0)
					buf.Append("\n");
				
				if (i < offset)
					buf.Append("  ");
				else
				{
					String s = System.Convert.ToString(req[i] & 0xFF, 16);
					if (s.Length == 1)
						buf.Append("0");
					buf.Append(s);
				}
				buf.Append(" ");
			}
			log.Debug( "\n"+buf.ToString()+"\n");
		}
	}
}