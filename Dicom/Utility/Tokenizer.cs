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

using System;
using System.Collections;

namespace Dicom.Utility {
    public class Tokenizer {
        private readonly ArrayList elements;
        private readonly string source;
        private string delimiters = ",;\\ \t\n\r";

        public Tokenizer(string source) {
            elements = new ArrayList();
            this.source = source;
            ReTokenize();
        }

        public Tokenizer(string source, string delimiters) {
            elements = new ArrayList();
            this.delimiters = delimiters;
            this.source = source;
            ReTokenize();
        }

        public int Count {
            get { return (elements.Count); }
        }

        public bool HasMoreTokens() {
            return (elements.Count > 0);
        }

        public string NextToken() {
            string result;
            if ((source == "")
                || (elements.Count == 0)) {
                throw new Exception();
            }
            else {
                result = (string) elements[0];
                elements.RemoveAt(0);
                return result;
            }
        }

        public string NextToken(string delimiters) {
            this.delimiters = delimiters;
            return NextToken();
        }

        public void ReTokenize() {
            int prev_index = 0;

            for (int index = 0; index < source.Length; index++) {
                if (delimiters.IndexOf(source[index]) >= 0) {
                    elements.Add(source.Substring(prev_index, index - prev_index));
                    elements.Add(new string(source[index], 1));

                    prev_index = index + 1;
                }
            }

            if (prev_index != source.Length) {
                elements.Add(source.Substring(prev_index, source.Length - prev_index));
            }

            RemoveEmptyStrings();
        }

        private void RemoveEmptyStrings() {
            for (int index = 0; index < elements.Count; index++) {
                if ((string) elements[index] == "") {
                    elements.RemoveAt(index);
                    index--;
                }
            }
        }
    }
}