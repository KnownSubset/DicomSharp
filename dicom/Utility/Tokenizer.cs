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

	public class Tokenizer
	{
		private System.Collections.ArrayList elements;
		private string source;
		private string delimiters = ",;\\ \t\n\r";		

		public Tokenizer(string source)
		{			
			this.elements = new System.Collections.ArrayList();
			this.source = source;
			this.ReTokenize();
		}

		public Tokenizer(string source, string delimiters)
		{
			this.elements = new System.Collections.ArrayList();
			this.delimiters = delimiters;
			this.source = source;
			this.ReTokenize();
		}

		public int Count
		{
			get
			{
				return (this.elements.Count);
			}
		}

		public bool HasMoreTokens()
		{
			return (this.elements.Count > 0);			
		}

		public string NextToken()
		{			
			string result;
			if ((source == "")
            ||  (this.elements.Count == 0)) throw new System.Exception();
			else
			{
				result = (string) this.elements[0];
				this.elements.RemoveAt(0);				
				return result;					
			}			
		}

		public string NextToken(string delimiters)
		{
			this.delimiters = delimiters;
			return NextToken();
		}

		public void ReTokenize()
		{
			int prev_index = 0;

			for (int index=0;index < this.source.Length;index++)
			{
				if (this.delimiters.IndexOf(this.source[index]) >= 0)
				{
					this.elements.Add(this.source.Substring(prev_index, index - prev_index));
					this.elements.Add(new string(this.source[index], 1));

					prev_index = index + 1;
				}
			}

			if (prev_index != this.source.Length)
			{
				this.elements.Add(this.source.Substring(prev_index, this.source.Length - prev_index));
			}
			
			this.RemoveEmptyStrings();
		}

		private void RemoveEmptyStrings()
		{
			for (int index=0; index < this.elements.Count; index++)
				if ((string)this.elements[index]== "")
				{
					this.elements.RemoveAt(index);
					index--;
				}
		}
	}
}
