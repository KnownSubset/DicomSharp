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
	using System.Reflection;
	using System.Text;
	using System.IO;
	using System.Collections;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	/// <summary>
	/// Contains a list of tag element
	/// </summary>
	public abstract class DcmObject
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType);

		protected ArrayList m_list = new ArrayList();
		private const int MIN_TRUNCATE_STRING_LEN = 16;
	
		public virtual DcmHandlerI DcmHandler
		{
			get { return new DcmObjectHandler(this); }			
		}

		public virtual String PrivateCreatorID
		{
			get { return null; }			
			set { throw new NotSupportedException(); }			
		}
		public virtual Encoding GetEncoding()
		{
			return null;
		}
		
		public virtual int Size
		{
			get { return m_list.Count; }
		}
		
		public virtual bool IsEmpty()
		{
			return m_list.Count == 0;
		}
		
		public virtual void Clear()
		{
			m_list.Clear();
		}
		
		public virtual bool Contains(uint tag)
		{
			if (Tags.IsPrivate(tag))
			{
				try
				{
					tag = AdjustPrivateTag(tag, false);
				}
				catch (DcmValueException e)
				{
					log.Warn("Could not access Creator ID", e);
					return false;
				}
				if (tag == 0)
				{
					return false;
				}
			}
			return m_list.BinarySearch(new DcmElement(tag)) >= 0;
		}

		public virtual IEnumerator GetEnumerator()
		{
			return m_list.GetEnumerator();
		}		
		
		public virtual int vm(uint tag)
		{
			if (Tags.IsPrivate(tag))
			{
				try
				{
					tag = AdjustPrivateTag(tag, false);
				}
				catch (DcmValueException e)
				{
					log.Warn("Could not access Creator ID", e);
					return - 1;
				}
				if (tag == 0)
				{
					return - 1;
				}
			}
			int index = m_list.BinarySearch(new DcmElement(tag));
			return index >= 0?((DcmElement) m_list[index]).vm():-1;
		}
		
		private uint AdjustPrivateTag(uint tag, bool create)
		{
			String creatorID = PrivateCreatorID;
			// no adjustments, if creatorID not set
			if (creatorID == null)
			{
				return tag;
			}
			uint gr = tag & 0xffff0000;
			uint el = 0x10;
			int index = m_list.BinarySearch(new DcmElement(gr | el));
			if (index >= 0)
			{
				DcmElement elm = (DcmElement) m_list[index];
				while (++index < m_list.Count)
				{
					if (creatorID.Equals( elm.GetString(GetEncoding())))
					{
						return gr | (el << 8) | (tag & 0xff);
					}
					elm = (DcmElement) m_list[index];
					if (elm.tag() != (gr | ++el))
					{
						break;
					}
				}
			}
			if (!create)
			{
				return 0;
			}
			DoPut(StringElement.CreateLO(gr | el, creatorID, GetEncoding()));
			return gr | (el << 8) | (tag & 0xff);
		}
		
		public virtual DcmElement Get(uint tag)
		{
			if (Tags.IsPrivate(tag))
			{
				try
				{
					tag = AdjustPrivateTag(tag, false);
				}
				catch (DcmValueException e)
				{
					log.Warn("Could not access Creator ID", e);
					return null;
				}
				if (tag == 0)
				{
					return null;
				}
			}
			int index = m_list.BinarySearch(new DcmElement(tag));
			return index >= 0?(DcmElement) m_list[index]:null;
		}
		
		public virtual DcmElement Remove(uint tag)
		{
			int index = m_list.BinarySearch(new DcmElement(tag));
			if( index >= 0 )
			{
				DcmElement elm = (DcmElement)m_list[index];
				m_list.RemoveAt(index);
				return elm;
			}
			return null;
		}
		
		public virtual ByteBuffer GetByteBuffer(uint tag)
		{
			DcmElement e = Get(tag);
			return e != null?e.GetByteBuffer():null;
		}
		
		public virtual String GetString(uint tag, String defVal)
		{
			return GetString(tag, 0, defVal);
		}
		
		public virtual String GetString(uint tag)
		{
			return GetString(tag, 0, null);
		}
		
		public virtual String GetString(uint tag, int index)
		{
			return GetString(tag, index, null);
		}
		
		public virtual String GetString(uint tag, int index, String defVal)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return defVal;
			
			return e.GetString(index, GetEncoding());
		}
		
		public virtual String[] GetStrings(uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.GetStrings(GetEncoding());
		}
		
		public virtual String GetBoundedString(int maxLen, uint tag, String defVal)
		{
			return GetBoundedString(maxLen, tag, 0, defVal);
		}
		
		public virtual String GetBoundedString(int maxLen, uint tag)
		{
			return GetBoundedString(maxLen, tag, 0, null);
		}
		
		public virtual String GetBoundedString(int maxLen, uint tag, int index)
		{
			return GetBoundedString(maxLen, tag, index, null);
		}
		
		public virtual String GetBoundedString(int maxLen, uint tag, int index, String defVal)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return defVal;
			
			return e.GetBoundedString(maxLen, index, GetEncoding());
		}
		
		public virtual String[] GetBoundedStrings(int maxLen, uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.GetBoundedStrings(maxLen, GetEncoding());
		}
		
		public virtual Int32 GetInteger(uint tag)
		{
			return GetInteger(tag, 0);
		}
		
		public virtual Int32 GetInteger(uint tag, int index)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return Int32.MinValue;
			
			return e.GetInt(index);
		}
		
		public virtual int GetInt(uint tag, int defVal)
		{
			return GetInt(tag, 0, defVal);
		}
		
		public virtual int GetInt(uint tag, int index, int defVal)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return defVal;
			
			return e.GetInt(index);
		}
		
		public virtual int[] GetInts(uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.Ints;
		}
		
		public virtual float GetFloat(uint tag, float defVal)
		{
			return GetFloat(tag, 0, defVal);
		}
		
		public virtual float GetFloat(uint tag, int index, float defVal)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return defVal;
			
			return e.GetFloat(index);
		}
		
		public virtual float[] GetFloats(uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.Floats;
		}
		
		public virtual Double GetDouble(uint tag, Double defVal)
		{
			return GetDouble(tag, 0, defVal);
		}
		
		public virtual Double GetDouble(uint tag, int index, Double defVal)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return defVal;
			
			return e.GetDouble(index);
		}
		
		public virtual Double[] GetDoubles(uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.Doubles;
		}
		
		public virtual DateTime GetDate(uint tag)
		{
			return GetDate(tag, 0);
		}
		
		public virtual DateTime GetDate(uint tag, int index)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return DateTime.MinValue;
			
			return e.GetDate(index);
		}
		
		public virtual DateTime[] GetDateRange(uint tag)
		{
			return GetDateRange(tag, 0);
		}
		
		public virtual DateTime[] GetDateRange(uint tag, int index)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return null;
			
			return e.GetDateRange(index);
		}
		
		public virtual DateTime[] GetDates(uint tag)
		{
			DcmElement e = Get(tag);
			if (e == null)
				return null;
			
			return e.Dates;
		}
		
		public virtual DateTime GetDateTime(uint dateTag, uint timeTag)
		{
			DcmElement date = Get(dateTag);
			if (date == null || date.IsEmpty())
				return DateTime.MinValue;
			
			DcmElement time = Get(timeTag);
			if (time == null || time.IsEmpty())
				return date.Date;
			
			DateTime
				dtDate = date.GetDate(0),
				dtTime = time.GetDate(0);

			return new DateTime(dtDate.Year, dtDate.Month, dtDate.Day, dtTime.Hour, dtTime.Minute, dtTime.Second, dtTime.Millisecond);
		}
		
		public virtual DateTime[] GetDateTimeRange(uint dateTag, uint timeTag)
		{
			DcmElement date = Get(dateTag);
			if (date == null || date.IsEmpty())
				return null;
			
			DateTime[] dateRange = date.DateRange;
			DcmElement time = Get(timeTag);
			if (time == null || time.IsEmpty())
				return dateRange;
			
			DateTime[] timeRange = time.DateRange;
			return new DateTime[]{new DateTime((dateRange[0].Ticks - 621355968000000000) / 10000 + (timeRange[0].Ticks - 621355968000000000) / 10000 * 10000 + 621355968000000000), new DateTime((dateRange[1].Ticks - 621355968000000000) / 10000 + (timeRange[1].Ticks - 621355968000000000) / 10000 * 10000 + 621355968000000000)};
		}
		
		public virtual Dataset GetItem(uint tag)
		{
			return GetItem(tag, 0);
		}
		
		public virtual Dataset GetItem(uint tag, int index)
		{
			DcmElement e = Get(tag);
			if (e == null || e.vm() <= index)
				return null;
			
			return e.GetItem(index);
		}
		
		public virtual DcmElement Put(DcmElement newElem)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("Put " + newElem);
			}
			if ((newElem.tag() & 0xffff) == 0)
				return newElem;
			
			if (Tags.IsPrivate(newElem.tag()))
			{
				try
				{
					((DcmElement) newElem).SetTag( AdjustPrivateTag(newElem.tag(), true) );
				}
				catch (DcmValueException e)
				{
					log.Warn("Could not access creator ID - ignore " + newElem, e);
					return newElem;
				}
			}
			return DoPut(newElem);
		}
		
		private DcmElement DoPut(DcmElement newElem)
		{
			int size = m_list.Count;
			if (size == 0 || newElem.CompareTo(m_list[size-1]) > 0)
			{
				m_list.Add(newElem);
			}
			else
			{
				int index = m_list.BinarySearch(newElem);
				if (index >= 0)
				{
					m_list[index] = newElem;
				}
				else
				{
					m_list.Insert(-(index + 1), newElem);
				}
			}
			return newElem;
		}
		
		public virtual DcmElement PutAE(uint tag)
		{
			return Put(StringElement.CreateAE(tag));
		}
		
		public virtual DcmElement PutAE(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateAE(tag, value):StringElement.CreateAE(tag));
		}
		
		public virtual DcmElement PutAE(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateAE(tag, values):StringElement.CreateAE(tag));
		}
		
		public virtual DcmElement PutAS(uint tag)
		{
			return Put(StringElement.CreateAS(tag));
		}
		
		public virtual DcmElement PutAS(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateAS(tag, value):StringElement.CreateAS(tag));
		}
		
		public virtual DcmElement PutAS(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateAS(tag, values):StringElement.CreateAS(tag));
		}
		
		public virtual DcmElement PutAT(uint tag)
		{
			return Put(ValueElement.CreateAT(tag));
		}
		
		public virtual DcmElement PutAT(uint tag, int value)
		{
			return Put(ValueElement.CreateAT(tag, value));
		}
		
		public virtual DcmElement PutAT(uint tag, int[] values)
		{
			return Put(values != null?ValueElement.CreateAT(tag, values):StringElement.CreateAT(tag));
		}
		
		public virtual DcmElement PutAT(uint tag, String value)
		{
			return value != null?PutAT(tag, Convert.ToInt32(value, 16)):PutAT(tag);
		}
		
		public virtual DcmElement PutAT(uint tag, String[] values)
		{
			if (values == null)
			{
				return PutAT(tag);
			}
			int[] a = new int[values.Length];
			 for (int i = 0; i < values.Length; ++i)
			{
				a[i] = Convert.ToInt32(values[i], 16);
			}
			return PutAT(tag, a);
		}
		
		public virtual DcmElement PutCS(uint tag)
		{
			return Put(StringElement.CreateCS(tag));
		}
		
		public virtual DcmElement PutCS(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateCS(tag, value):StringElement.CreateCS(tag));
		}
		
		public virtual DcmElement PutCS(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateCS(tag, values):StringElement.CreateCS(tag));
		}
		
		public virtual DcmElement PutDA(uint tag)
		{
			return Put(StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDA(uint tag, DateTime value)
		{
			return Put( (value != DateTime.MinValue) ? StringElement.CreateDA(tag, value):StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDA(uint tag, DateTime[] values)
		{
			return Put(values != null?StringElement.CreateDA(tag, values):StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDA(uint tag, DateTime from, DateTime to)
		{
			return Put(from != DateTime.MinValue || to != DateTime.MinValue?StringElement.CreateDA(tag, from, to):StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDA(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateDA(tag, value):StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDA(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateDA(tag, values):StringElement.CreateDA(tag));
		}
		
		public virtual DcmElement PutDS(uint tag)
		{
			return Put(StringElement.CreateDS(tag));
		}
		
		public virtual DcmElement PutDS(uint tag, float value)
		{
			return Put(StringElement.CreateDS(tag, value));
		}
		
		public virtual DcmElement PutDS(uint tag, float[] values)
		{
			return Put(values != null?StringElement.CreateDS(tag, values):StringElement.CreateDS(tag));
		}
		
		public virtual DcmElement PutDS(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateDS(tag, value):StringElement.CreateDS(tag));
		}
		
		public virtual DcmElement PutDS(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateDS(tag, values):StringElement.CreateDS(tag));
		}
		
		public virtual DcmElement PutDT(uint tag)
		{
			return Put(StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutDT(uint tag, DateTime value)
		{
			return Put(value != DateTime.MinValue?StringElement.CreateDT(tag, value):StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutDT(uint tag, DateTime[] values)
		{
			return Put(values != null?StringElement.CreateDT(tag, values):StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutDT(uint tag, DateTime from, DateTime to)
		{
			return Put(from != DateTime.MinValue || to != DateTime.MinValue?StringElement.CreateDT(tag, from, to):StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutDT(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateDT(tag, value):StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutDT(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateDT(tag, values):StringElement.CreateDT(tag));
		}
		
		public virtual DcmElement PutFL(uint tag)
		{
			return Put(ValueElement.CreateFL(tag));
		}
		
		public virtual DcmElement PutFL(uint tag, float value)
		{
			return Put(ValueElement.CreateFL(tag, value));
		}
		
		public virtual DcmElement PutFL(uint tag, float[] values)
		{
			return Put(values != null?ValueElement.CreateFL(tag, values):ValueElement.CreateFL(tag));
		}
		
		public virtual DcmElement PutFL(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateFL(tag, Single.Parse(value)):ValueElement.CreateFL(tag));
		}
		
		public virtual DcmElement PutFL(uint tag, String[] values)
		{
			return Put(values != null?ValueElement.CreateFL(tag, StringUtils.ParseFloats(values)):ValueElement.CreateFL(tag));
		}
		
		public virtual DcmElement PutFD(uint tag)
		{
			return Put(ValueElement.CreateFD(tag));
		}
		
		public virtual DcmElement PutFD(uint tag, Double value)
		{
			return Put(ValueElement.CreateFD(tag, value));
		}
		
		public virtual DcmElement PutFD(uint tag, Double[] values)
		{
			return Put(values != null?ValueElement.CreateFD(tag, values):ValueElement.CreateFD(tag));
		}
		
		public virtual DcmElement PutFD(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateFD(tag, Double.Parse(value)):ValueElement.CreateFD(tag));
		}
		
		public virtual DcmElement PutFD(uint tag, String[] values)
		{
			return Put(values != null?ValueElement.CreateFD(tag, StringUtils.ParseDoubles(values)):ValueElement.CreateFD(tag));
		}
		
		public virtual DcmElement PutIS(uint tag)
		{
			return Put(StringElement.CreateIS(tag));
		}
		
		public virtual DcmElement PutIS(uint tag, int value)
		{
			return Put(StringElement.CreateIS(tag, value));
		}
		
		public virtual DcmElement PutIS(uint tag, int[] values)
		{
			return Put(values != null?StringElement.CreateIS(tag, values):StringElement.CreateIS(tag));
		}
		
		public virtual DcmElement PutIS(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateIS(tag, value):StringElement.CreateIS(tag));
		}
		
		public virtual DcmElement PutIS(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateIS(tag, values):StringElement.CreateIS(tag));
		}
		
		public virtual DcmElement PutLO(uint tag)
		{
			return Put(StringElement.CreateLO(tag));
		}
		
		public virtual DcmElement PutLO(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateLO(tag, value, GetEncoding()):StringElement.CreateLO(tag));
		}
		
		public virtual DcmElement PutLO(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateLO(tag, values, GetEncoding()):StringElement.CreateLO(tag));
		}
		
		public virtual DcmElement PutLT(uint tag)
		{
			return Put(StringElement.CreateLT(tag));
		}
		
		public virtual DcmElement PutLT(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateLT(tag, value, GetEncoding()):StringElement.CreateLT(tag));
		}
		
		public virtual DcmElement PutLT(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateLT(tag, values, GetEncoding()):StringElement.CreateLT(tag));
		}
		
		public virtual DcmElement PutOB(uint tag)
		{
			return Put(ValueElement.CreateOB(tag));
		}
		
		public virtual DcmElement PutOB(uint tag, byte[] value)
		{
			return Put(value != null?ValueElement.CreateOB(tag, value):ValueElement.CreateOB(tag));
		}
		
		public virtual DcmElement PutOB(uint tag, ByteBuffer value)
		{
			return Put(value != null?ValueElement.CreateOB(tag, value):ValueElement.CreateOB(tag));
		}
		
		public virtual DcmElement PutOBsq(uint tag)
		{
			return Put(FragmentElement.CreateOB(tag));
		}
		
		public virtual DcmElement PutOW(uint tag)
		{
			return Put(ValueElement.CreateOW(tag));
		}
		
		public virtual DcmElement PutOW(uint tag, short[] value)
		{
			return Put(value != null?ValueElement.CreateOW(tag, value):ValueElement.CreateOW(tag));
		}

		public virtual DcmElement PutOW(uint tag, short[][] value)
		{
			return Put(value != null?ValueElement.CreateOW(tag, value):ValueElement.CreateOW(tag));
		}
		
		public virtual DcmElement PutOW(uint tag, ByteBuffer value)
		{
			return Put(value != null?ValueElement.CreateOW(tag, value):ValueElement.CreateOW(tag));
		}
		
		public virtual DcmElement PutOWsq(uint tag)
		{
			return Put(FragmentElement.CreateOW(tag));
		}
		
		public virtual DcmElement PutPN(uint tag)
		{
			return Put(StringElement.CreateSH(tag));
		}
		
		public virtual DcmElement PutPN(uint tag, PersonName value)
		{
			return Put(value != null?StringElement.CreatePN(tag, value, GetEncoding()):StringElement.CreatePN(tag));
		}
		
		public virtual DcmElement PutPN(uint tag, PersonName[] values)
		{
			return Put(values != null?StringElement.CreatePN(tag, values, GetEncoding()):StringElement.CreatePN(tag));
		}
		
		public virtual DcmElement PutPN(uint tag, String value)
		{
			return Put(value != null?StringElement.CreatePN(tag, new PersonName(value), GetEncoding()):StringElement.CreatePN(tag));
		}
		
		public virtual DcmElement PutPN(uint tag, String[] values)
		{
			if (values == null)
			{
				return StringElement.CreatePN(tag);
			}
			PersonName[] a = new PersonName[values.Length];
			 for (int i = 0; i < a.Length; ++i)
			{
				a[i] = new PersonName(values[i]);
			}
			return Put(StringElement.CreatePN(tag, a, GetEncoding()));
		}
		
		public virtual DcmElement PutSH(uint tag)
		{
			return Put(StringElement.CreateSH(tag));
		}
		
		public virtual DcmElement PutSH(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateSH(tag, value, GetEncoding()):StringElement.CreateSH(tag));
		}
		
		public virtual DcmElement PutSH(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateSH(tag, values, GetEncoding()):StringElement.CreateSH(tag));
		}
		
		public virtual DcmElement PutSL(uint tag)
		{
			return Put(ValueElement.CreateSL(tag));
		}
		
		public virtual DcmElement PutSL(uint tag, int value)
		{
			return Put(ValueElement.CreateSL(tag, value));
		}
		
		public virtual DcmElement PutSL(uint tag, int[] values)
		{
			return Put(values != null?ValueElement.CreateSL(tag, values):ValueElement.CreateSL(tag));
		}
		
		public virtual DcmElement PutSL(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateSL(tag, StringUtils.ParseInt(value, Int32.MinValue, Int32.MaxValue)):ValueElement.CreateSL(tag));
		}
		
		public virtual DcmElement PutSL(uint tag, String[] values)
		{
			return Put(ValueElement.CreateSL(tag, StringUtils.ParseInts(values, Int32.MinValue, Int32.MaxValue)));
		}
		
		public virtual DcmElement PutSQ(uint tag)
		{
			throw new NotSupportedException();
		}
		
		public virtual DcmElement PutSS(uint tag)
		{
			return Put(ValueElement.CreateSS(tag));
		}
		
		public virtual DcmElement PutSS(uint tag, int value)
		{
			return Put(ValueElement.CreateSS(tag, value));
		}
		
		public virtual DcmElement PutSS(uint tag, int[] values)
		{
			return Put(values != null?ValueElement.CreateSS(tag, values):ValueElement.CreateSS(tag));
		}
		
		public virtual DcmElement PutSS(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateSS(tag, StringUtils.ParseInt(value, Int16.MinValue, Int16.MaxValue)):ValueElement.CreateSS(tag));
		}
		
		public virtual DcmElement PutSS(uint tag, String[] values)
		{
			return Put(values != null?ValueElement.CreateSS(tag, StringUtils.ParseInts(values, Int16.MinValue, Int16.MaxValue)):ValueElement.CreateSS(tag));
		}
		
		public virtual DcmElement PutST(uint tag)
		{
			return Put(StringElement.CreateST(tag));
		}
		
		public virtual DcmElement PutST(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateST(tag, value, GetEncoding()):StringElement.CreateST(tag));
		}
		
		public virtual DcmElement PutST(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateST(tag, values, GetEncoding()):StringElement.CreateST(tag));
		}
		
		public virtual DcmElement PutTM(uint tag)
		{
			return Put(StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutTM(uint tag, DateTime value)
		{
			return Put(value != DateTime.MinValue?StringElement.CreateTM(tag, value):StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutTM(uint tag, DateTime[] values)
		{
			return Put(values != null?StringElement.CreateTM(tag, values):StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutTM(uint tag, DateTime from, DateTime to)
		{
			return Put((from != DateTime.MinValue) || (to != DateTime.MinValue)?StringElement.CreateTM(tag, from, to):StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutTM(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateTM(tag, value):StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutTM(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateTM(tag, values):StringElement.CreateTM(tag));
		}
		
		public virtual DcmElement PutUI(uint tag)
		{
			return Put(StringElement.CreateUI(tag));
		}
		
		public virtual DcmElement PutUI(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateUI(tag, value):StringElement.CreateUI(tag));
		}
		
		public virtual DcmElement PutUI(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateUI(tag, values):StringElement.CreateUI(tag));
		}
		
		public virtual DcmElement PutUL(uint tag)
		{
			return Put(ValueElement.CreateUL(tag));
		}
		
		public virtual DcmElement PutUL(uint tag, int value)
		{
			return Put(ValueElement.CreateUL(tag, value));
		}
		
		public virtual DcmElement PutUL(uint tag, int[] values)
		{
			return Put(values != null?ValueElement.CreateUL(tag, values):StringElement.CreateUI(tag));
		}
		
		public virtual DcmElement PutUL(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateUL(tag, StringUtils.ParseInt(value, 0L, (int) (- (0x100000000 - 0xFFFFFFFFL)))):ValueElement.CreateUL(tag));
		}
		
		public virtual DcmElement PutUL(uint tag, String[] values)
		{
			return Put(values != null?ValueElement.CreateUL(tag, StringUtils.ParseInts(values, 0L, (int) (- (0x100000000 - 0xFFFFFFFFL)))):ValueElement.CreateUL(tag));
		}
		
		public virtual DcmElement PutUN(uint tag)
		{
			return Put(ValueElement.CreateUN(tag));
		}
		
		public virtual DcmElement PutUN(uint tag, byte[] value)
		{
			return Put(value != null?ValueElement.CreateUN(tag, value):ValueElement.CreateUN(tag));
		}
		
		public virtual DcmElement PutUN(uint tag, ByteBuffer value)
		{
			return Put(value != null?ValueElement.CreateUN(tag, value):ValueElement.CreateUN(tag));
		}
		
		public virtual DcmElement PutUNsq(uint tag)
		{
			return Put(FragmentElement.CreateUN(tag));
		}
		
		public virtual DcmElement PutUS(uint tag)
		{
			return Put(ValueElement.CreateUS(tag));
		}
		
		public virtual DcmElement PutUS(uint tag, int value)
		{
			return Put(ValueElement.CreateUS(tag, value));
		}
		
		public virtual DcmElement PutUS(uint tag, int[] values)
		{
			return Put(values != null?ValueElement.CreateUS(tag, values):ValueElement.CreateUS(tag));
		}
		
		public virtual DcmElement PutUS(uint tag, String value)
		{
			return Put(value != null?ValueElement.CreateUS(tag, StringUtils.ParseInt(value, 0L, 0xFFFFL)):ValueElement.CreateUS(tag));
		}
		
		public virtual DcmElement PutUS(uint tag, String[] values)
		{
			return Put(values != null?ValueElement.CreateUS(tag, StringUtils.ParseInts(values, 0L, 0xFFFFL)):ValueElement.CreateUS(tag));
		}
		
		public virtual DcmElement PutUT(uint tag)
		{
			return Put(StringElement.CreateUT(tag));
		}
		
		public virtual DcmElement PutUT(uint tag, String value)
		{
			return Put(value != null?StringElement.CreateUT(tag, value, GetEncoding()):StringElement.CreateUT(tag));
		}
		
		public virtual DcmElement PutUT(uint tag, String[] values)
		{
			return Put(values != null?StringElement.CreateUT(tag, values, GetEncoding()):StringElement.CreateUT(tag));
		}
		
		public virtual DcmElement PutXX(uint tag)
		{
			return PutXX(tag, VRs.GetVR(tag));
		}
		
		public virtual DcmElement PutXX(uint tag, ByteBuffer bytes)
		{
			return PutXX(tag, VRs.GetVR(tag), bytes);
		}
		
		public virtual DcmElement PutXX(uint tag, String value)
		{
			return PutXX(tag, VRs.GetVR(tag), value);
		}
		
		public virtual DcmElement PutXX(uint tag, String[] values)
		{
			return PutXX(tag, VRs.GetVR(tag), values);
		}
		
		public virtual DcmElement PutXXsq(uint tag)
		{
			return PutXXsq(tag, VRs.GetVR(tag));
		}
		
		public virtual DcmElement PutXXsq(uint tag, int vr)
		{
			switch (vr)
			{
				case VRs.OB: 
					return PutOBsq(tag);
				
				case VRs.OW: 
					return PutOWsq(tag);
				
				case VRs.UN: 
					return PutUNsq(tag);
				
				default: 
					throw new ArgumentException(Tags.ToHexString(tag) + " " + VRs.ToString(vr));
				
			}
		}
		
		public virtual DcmElement PutXX(uint tag, int vr)
		{
			switch (vr)
			{
				case VRs.AE: 
					return PutAE(tag);
				
				case VRs.AS: 
					return PutAS(tag);
				
				case VRs.AT: 
					return PutAT(tag);
				
				case VRs.CS: 
					return PutCS(tag);
				
				case VRs.DA: 
					return PutDA(tag);
				
				case VRs.DS: 
					return PutDS(tag);
				
				case VRs.DT: 
					return PutDT(tag);
				
				case VRs.FL: 
					return PutFL(tag);
				
				case VRs.FD: 
					return PutFD(tag);
				
				case VRs.IS: 
					return PutIS(tag);
				
				case VRs.LO: 
					return PutLO(tag);
				
				case VRs.LT: 
					return PutLT(tag);
				
				case VRs.OB: 
					return PutOB(tag);
				
				case VRs.OW: 
					return PutOW(tag);
				
				case VRs.PN: 
					return PutPN(tag);
				
				case VRs.SH: 
					return PutSH(tag);
				
				case VRs.SL: 
					return PutSL(tag);
				
				case VRs.SQ: 
					return ((Dataset) this).PutSQ(tag);
				
				case VRs.SS: 
					return PutSS(tag);
				
				case VRs.ST: 
					return PutST(tag);
				
				case VRs.TM: 
					return PutTM(tag);
				
				case VRs.UI: 
					return PutUI(tag);
				
				case VRs.UN: 
					return PutUN(tag);
				
				case VRs.UL: 
					return PutUL(tag);
				
				case VRs.US: 
					return PutUS(tag);
				
				case VRs.UT: 
					return PutUT(tag);
				
				default: 
					throw new ArgumentException(Tags.ToHexString(tag) + " " + VRs.ToString(vr));
				
			}
		}
		
		public virtual DcmElement PutXX(uint tag, int vr, ByteBuffer value)
		{
			if (value == null)
			{
				return PutXX(tag, vr);
			}
			switch (vr)
			{
				case VRs.AE: 
					return Put(StringElement.CreateAE(tag, value));
				
				case VRs.AS: 
					return Put(StringElement.CreateAS(tag, value));
				
				case VRs.AT: 
					return Put(ValueElement.CreateAT(tag, value));
				
				case VRs.CS: 
					return Put(StringElement.CreateCS(tag, value));
				
				case VRs.DA: 
					return Put(StringElement.CreateDA(tag, value));
				
				case VRs.DS: 
					return Put(StringElement.CreateDS(tag, value));
				
				case VRs.DT: 
					return Put(StringElement.CreateDT(tag, value));
				
				case VRs.FL: 
					return Put(ValueElement.CreateFL(tag, value));
				
				case VRs.FD: 
					return Put(ValueElement.CreateFD(tag, value));
				
				case VRs.IS: 
					return Put(StringElement.CreateIS(tag, value));
				
				case VRs.LO: 
					return Put(StringElement.CreateLO(tag, value));
				
				case VRs.LT: 
					return Put(StringElement.CreateLT(tag, value));
				
				case VRs.OB: 
					return Put(ValueElement.CreateOB(tag, value));
				
				case VRs.OW: 
					return Put(ValueElement.CreateOW(tag, value));
				
				case VRs.PN: 
					return Put(StringElement.CreatePN(tag, value));
				
				case VRs.SH: 
					return Put(StringElement.CreateSH(tag, value));
				
				case VRs.SL: 
					return Put(ValueElement.CreateSL(tag, value));
				
				case VRs.SS: 
					return Put(ValueElement.CreateSS(tag, value));
				
				case VRs.ST: 
					return Put(StringElement.CreateST(tag, value));
				
				case VRs.TM: 
					return Put(StringElement.CreateTM(tag, value));
				
				case VRs.UI: 
					return Put(StringElement.CreateUI(tag, value));
				
				case VRs.UN: 
					return Put(ValueElement.CreateUN(tag, value));
				
				case VRs.UL: 
					return Put( ValueElement.CreateUL(tag, value) );
				
				case VRs.US: 
					return Put(ValueElement.CreateUS(tag, value));
				
				case VRs.UT: 
					return Put(StringElement.CreateUT(tag, value));
				
				default: 
					throw new ArgumentException(Tags.ToHexString(tag) + " " + VRs.ToString(vr));
				
			}
		}
		
		public virtual DcmElement PutXX(uint tag, int vr, String value)
		{
			if (value == null)
			{
				return PutXX(tag, vr);
			}
			switch (vr)
			{
				case VRs.AE: 
					return PutAE(tag, value);
				
				case VRs.AS: 
					return PutAS(tag, value);
				
				case VRs.AT: 
					return PutAT(tag, value);
				
				case VRs.CS: 
					return PutCS(tag, value);
				
				case VRs.DA: 
					return PutDA(tag, value);
				
				case VRs.DS: 
					return PutDS(tag, value);
				
				case VRs.DT: 
					return PutDT(tag, value);
				
				case VRs.FL: 
					return PutFL(tag, value);
				
				case VRs.FD: 
					return PutFD(tag, value);
				
				case VRs.IS: 
					return PutIS(tag, value);
				
				case VRs.LO: 
					return PutLO(tag, value);
				
				case VRs.LT: 
					return PutLT(tag, value);
					//            case VRs.OB:
					//                return PutOB(tag, value);
					//            case VRs.OW:
					//                return PutOW(tag, value);
				
				case VRs.PN: 
					return PutPN(tag, value);
				
				case VRs.SH: 
					return PutSH(tag, value);
				
				case VRs.SL: 
					return PutSL(tag, value);
				
				case VRs.SS: 
					return PutSS(tag, value);
				
				case VRs.ST: 
					return PutST(tag, value);
				
				case VRs.TM: 
					return PutTM(tag, value);
				
				case VRs.UI: 
					return PutUI(tag, value);
					//            case VRs.UN:
					//                return PutUN(tag, value);
				
				case VRs.UL: 
					return PutUL(tag, value);
				
				case VRs.US: 
					return PutUS(tag, value);
				
				case VRs.UT: 
					return PutUT(tag, value);
				
				default: 
					throw new ArgumentException(Tags.ToHexString(tag) + " " + VRs.ToString(vr));
				
			}
		}
		
		public virtual DcmElement PutXX(uint tag, int vr, String[] values)
		{
			if (values == null)
			{
				return PutXX(tag, vr);
			}
			switch (vr)
			{
				case VRs.AE: 
					return PutAE(tag, values);
				
				case VRs.AS: 
					return PutAS(tag, values);
				
				case VRs.AT: 
					return PutAT(tag, values);
				
				case VRs.CS: 
					return PutCS(tag, values);
				
				case VRs.DA: 
					return PutDA(tag, values);
				
				case VRs.DS: 
					return PutDS(tag, values);
				
				case VRs.DT: 
					return PutDT(tag, values);
				
				case VRs.FL: 
					return PutFL(tag, values);
				
				case VRs.FD: 
					return PutFD(tag, values);
				
				case VRs.IS: 
					return PutIS(tag, values);
				
				case VRs.LO: 
					return PutLO(tag, values);
				
				case VRs.LT: 
					return PutLT(tag, values);
					//            case VRs.OB:
					//                return PutOB(tag, values);
					//            case VRs.OW:
					//                return PutOW(tag, values);
				
				case VRs.PN: 
					return PutPN(tag, values);
				
				case VRs.SH: 
					return PutSH(tag, values);
				
				case VRs.SL: 
					return PutSL(tag, values);
				
				case VRs.SS: 
					return PutSS(tag, values);
				
				case VRs.ST: 
					return PutST(tag, values);
				
				case VRs.TM: 
					return PutTM(tag, values);
				
				case VRs.UI: 
					return PutUI(tag, values);
					//            case VRs.UN:
					//                return PutUN(tag, values);
				
				case VRs.UL: 
					return PutUL(tag, values);
				
				case VRs.US: 
					return PutUS(tag, values);
				
				case VRs.UT: 
					return PutUT(tag, values);
				
				default: 
					throw new ArgumentException(Tags.ToHexString(tag) + " " + VRs.ToString(vr));
				
			}
		}
		
		public virtual void  PutAll(DcmObject dcmObj)
		{
			IEnumerator enu = dcmObj.GetEnumerator();
			while( enu.MoveNext() )
			{
				DcmElement el = (DcmElement) enu.Current;
				if (el.IsEmpty())
				{
					PutXX(el.tag(), el.vr());
				}
				else
				{
					DcmElement sq;
					switch (el.vr())
					{
						case VRs.SQ: 
							sq = PutSQ(el.tag());
							 for (int i = 0, n = el.vm(); i < n; ++i)
							{
								sq.AddItem(el.GetItem(i));
							}
							break;
						
						case VRs.OB: 
						case VRs.OW: 
						case VRs.UN: 
							if (el.HasDataFragments())
							{
								sq = PutXXsq(el.tag(), el.vr());
								 for (int i = 0, n = el.vm(); i < n; ++i)
								{
									sq.AddDataFragment(el.GetDataFragment(i));
								}
								break;
							}
							goto default;
						
						default: 
							PutXX(el.tag(), el.vr(), el.GetByteBuffer());
							break;
						
					}
				}
			}
		}
		
		protected virtual void  Write(uint grTag, int grLen, DcmHandlerI handler)
		{
			byte[] b4 = new byte[]{(byte) grLen, (byte)(grLen>>8), (byte)(grLen>>16), (byte)((grLen>>24))};
			long el1Pos = ((DcmElement) m_list[0]).StreamPosition;
			handler.StartElement(grTag, VRs.UL, el1Pos == - 1L?- 1L:el1Pos - 12);
			handler.Value(b4, 0, 4);
			handler.EndElement();
			for (int i = 0, n = m_list.Count; i < n; ++i)
			{
				DcmElement el = (DcmElement) m_list[i];
				int len = el.length();
				handler.StartElement(el.tag(), el.vr(), el.StreamPosition);
				ByteBuffer bb = el.GetByteBuffer(ByteOrder.LITTLE_ENDIAN);
				handler.Value(bb.ToArray(), (int)bb.Position, bb.length());
				handler.EndElement();
			}
		}
				
		public virtual void  WriteHeader( Stream os, DcmEncodeParam encParam, uint tag, int vr, int len)
		{
			if (encParam.byteOrder == ByteOrder.LITTLE_ENDIAN)
			{
				os.WriteByte((Byte) (tag >> 16));
				os.WriteByte((Byte) (tag >> 24));
				os.WriteByte((Byte) (tag >> 0));
				os.WriteByte((Byte) (tag >> 8));
			}
			else
			{
				os.WriteByte((Byte) (tag >> 24));
				os.WriteByte((Byte) (tag >> 16));
				os.WriteByte((Byte) (tag >> 8));
				os.WriteByte((Byte) (tag >> 0));
			}
			
			if (encParam.explicitVR)
			{
				os.WriteByte((Byte) (vr >> 8));
				os.WriteByte((Byte) (vr >> 0));
				if (VRs.IsLengthField16Bit(vr))
				{
					if (encParam.byteOrder == ByteOrder.LITTLE_ENDIAN)
					{
						os.WriteByte((Byte) (len >> 0));
						os.WriteByte((Byte) (len >> 8));
					}
					else
					{
						os.WriteByte((Byte) (len >> 8));
						os.WriteByte((Byte) (len >> 0));
					}
					return ;
				}
				else
				{
					os.WriteByte((Byte) 0);
					os.WriteByte((Byte) 0);
				}
			}
			if (encParam.byteOrder == ByteOrder.LITTLE_ENDIAN)
			{
				os.WriteByte((Byte) (len >> 0));
				os.WriteByte((Byte) (len >> 8));
				os.WriteByte((Byte) (len >> 16));
				os.WriteByte((Byte) (len >> 24));
			}
			else
			{
				// order == ByteOrder.BIG_ENDIAN
				os.WriteByte((Byte) (len >> 24));
				os.WriteByte((Byte) (len >> 16));
				os.WriteByte((Byte) (len >> 8));
				os.WriteByte((Byte) (len >> 0));
			}
		}
	}
}