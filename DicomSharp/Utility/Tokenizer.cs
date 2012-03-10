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
using System.Collections.Generic;

namespace DicomSharp.Utility
{
    public class Tokenizer
    {
        private readonly IList<string> _elements = new List<string>();
        private readonly string _source;
        private string _delimiters = ",;\\ \t\n\r";

        public Tokenizer(string source)
        {
            _source = source;
            ReTokenize();
        }

        public Tokenizer(string source, string delimiters)
        {
            _delimiters = delimiters;
            _source = source;
            ReTokenize();
        }

        public int Count
        {
            get { return (_elements.Count); }
        }

        public bool HasMoreTokens()
        {
            return (_elements.Count > 0);
        }

        public string NextToken()
        {
            if ((_source == string.Empty) || (_elements.Count == 0))
            {
                throw new Exception();
            }
            string result = _elements[0];
            _elements.RemoveAt(0);
            return result;
        }

        public string NextToken(string delimiters)
        {
            _delimiters = delimiters;
            return NextToken();
        }

        public void ReTokenize()
        {
            int prevIndex = 0;

            for (int index = 0; index < _source.Length; index++)
            {
                if (_delimiters.IndexOf(_source[index]) >= 0)
                {
                    _elements.Add(_source.Substring(prevIndex, index - prevIndex));
                    _elements.Add(new string(_source[index], 1));

                    prevIndex = index + 1;
                }
            }

            if (prevIndex != _source.Length)
            {
                _elements.Add(_source.Substring(prevIndex, _source.Length - prevIndex));
            }

            RemoveEmptyStrings();
        }

        private void RemoveEmptyStrings()
        {
            for (int index = 0; index < _elements.Count; index++)
            {
                if (_elements[index] == "")
                {
                    _elements.RemoveAt(index);
                    index--;
                }
            }
        }
    }
}