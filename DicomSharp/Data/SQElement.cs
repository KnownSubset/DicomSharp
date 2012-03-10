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
using System.Collections;
using System.Text;
using DicomSharp.Dictionary;

namespace DicomSharp.Data {
    /// <summary>
    /// </summary>
    public class SQElement : DcmElement {
        private readonly ArrayList m_list = new ArrayList();
        private readonly DataSet parent;
        private int totlen = - 1;

        /// <summary>
        /// Creates a new instance of ElementImpl 
        /// </summary>
        public SQElement(uint tag, DataSet parent) : base(tag) {
            this.parent = parent;
        }

        public override int VR() {
            return VRs.SQ;
        }

        public override int VM() {
            return m_list.Count;
        }

        public override bool HasItems() {
            return true;
        }

        public override DataSet GetItem(int index) {
            if (index >= VM()) {
                return null;
            }
            return (DataSet) m_list[index];
        }

        public override void AddItem(DataSet item) {
            m_list.Add(item);
        }

        public override DataSet AddNewItem() {
            var item = new DataSet(parent);
            m_list.Add(item);
            return item;
        }

        public virtual int CalcLength(DcmEncodeParam param) {
            totlen = param.undefSeqLen ? 8 : 0;
            for (int i = 0, n = VM(); i < n; ++i) {
                totlen += GetItem(i).CalcLength(param) + (param.undefItemLen ? 16 : 8);
            }
            return totlen;
        }

        public override int Length() {
            return totlen;
        }

        public override String ToString() {
            var sb = new StringBuilder(Dictionary.Tags.ToHexString(tag()));
            sb.Append(",SQ");
            if (!IsEmpty()) {
                for (int i = 0, n = VM(); i < n; ++i) {
                    sb.Append("\n\tItem-").Append(i + 1).Append(GetItem(i));
                }
            }
            return sb.ToString();
        }
    }
}