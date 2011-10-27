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
	using org.dicomcs.util;
	
	/// <summary>
	/// 
	/// </summary>
	public class PersonName
	{
		public const int FAMILY = 0;
		public const int GIVEN = 1;
		public const int MIDDLE = 2;
		public const int PREFIX = 3;
		public const int SUFFIX = 4;

		private String[] components = new String[5];
		private PersonName ideographic;
		private PersonName phonetic;

		public virtual PersonName Ideographic
		{
			get { return ideographic; }
			set { this.ideographic = (PersonName) value; }			
		}
		public virtual PersonName Phonetic
		{
			get { return phonetic; }
			set { this.phonetic = (PersonName) value; }			
		}
		
		public PersonName(String s)
		{
            if (s == null)
                s = string.Empty;

			int grLen = s.IndexOf((System.Char) '=');
			if ((grLen == - 1?s.Length:grLen) > 64)
			{
				throw new System.ArgumentException(s);
			}

			Tokenizer stk = new Tokenizer(s, "=^");
			int field = FAMILY;
			String tk;
			while (stk.HasMoreTokens())
			{
				tk = stk.NextToken();
				switch (tk[0])
				{
					case '^': 
						if (++field > SUFFIX)
							throw new System.ArgumentException(s);
						break;
					
					case '=': 
						goto WHILE_brk;
					
					default: 
						components[field] = tk;
						break;
					
				}
			}
WHILE_brk: ;
			
			if (!stk.HasMoreTokens())
				return ;
			
			tk = stk.NextToken("=");
			if (tk[0] != '=')
			{
				ideographic = new PersonName(tk);
				if (stk.HasMoreTokens())
					tk = stk.NextToken("=");
			}
			if (!stk.HasMoreTokens())
				return ;
			
			tk = stk.NextToken();
			if (tk[0] == '=' || stk.HasMoreTokens())
				throw new System.ArgumentException(s);
			
			phonetic = new PersonName(tk);
		}
		
		public virtual String Get(int field)
		{
			return components[field];
		}
		
		public virtual void  Set(int field, String value_Renamed)
		{
			components[field] = value_Renamed;
		}
		
		private System.Text.StringBuilder AppendComponents(System.Text.StringBuilder sb)
		{
			int lastField = FAMILY;
			for (int field = FAMILY; field <= SUFFIX; ++field)
			{
				if (components[field] != null)
				{
					sb.Append(components[field]);
					lastField = field;
				}
				sb.Append('^');
			}
			int l = sb.Length + lastField - SUFFIX - 1;
			sb.Remove(l , sb.Length - l);
			return sb;
		}
		
		public override String ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			AppendComponents(sb);
			if (ideographic != null || phonetic != null)
			{
				sb.Append('=');
				if (ideographic != null)
					ideographic.AppendComponents(sb);
				if (phonetic != null)
					phonetic.AppendComponents(sb.Append('='));
			}
			return sb.ToString();
		}
	}
}