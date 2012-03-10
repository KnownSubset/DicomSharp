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
using System.IO;
using System.Text;
using DicomSharp.Dictionary;
using log4net;

namespace DicomSharp.Data {
    /// <summary>
    /// Implementation of <code>DataSet</code> container objects.
    /// </summary>
    public class DataSet : BaseDataSet {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataSet));

        private readonly DataSet _parentDataSet;
        private Encoding _encoding;
        private long _itemOffset = - 1L;
        private String _privateCreatorId;

        public DataSet() : this(null) {}

        public DataSet(DataSet parentDataSet) {
            this._parentDataSet = parentDataSet;
        }

        public override String PrivateCreatorID {
            get { return _privateCreatorId ?? (_parentDataSet != null ? _parentDataSet.PrivateCreatorID : null); }
            set { _privateCreatorId = value; }
        }

        public virtual Encoding Encoding {
            get { return _encoding ?? (_parentDataSet != null ? _parentDataSet.Encoding : null); }
        }

        public virtual DataSet ParentDataSet {
            get { return _parentDataSet; }
        }

        public override long GetItemOffset() {
            if (_itemOffset != - 1L || _dcmElements.Count == 0) {
                return _itemOffset;
            }

            long elm1pos = ((DcmElement) _dcmElements[0]).StreamPosition;
            return elm1pos == - 1L ? - 1L : elm1pos - 8L;
        }

        public override DataSet SetItemOffset(long itemOffset) {
            this._itemOffset = itemOffset;
            return this;
        }

        public override DcmElement PutSQ(uint tag) {
            return Put(new SQElement(tag, this));
        }

        public override DcmElement Put(DcmElement newElem) {
            if ((newElem.tag()) >> 16 < 4) {
                throw new ArgumentException(newElem.ToString());
            }

            if (newElem.tag() == Tags.SpecificCharacterSet) {
                try {
                    //TODO: decide the encoding
                    //this.encoding = Encodings.lookup(newElem.GetStrings(null));
                }
                catch (Exception ex) {
                    Logger.Warn("Failed to consider specified Encoding!", ex);
                    _encoding = null;
                }
            }

            return base.Put(newElem);
        }

        public override DcmElement Remove(uint tag) {
            if (tag == Tags.SpecificCharacterSet) {
                _encoding = null;
            }
            return base.Remove(tag);
        }

        public override void Clear() {
            base.Clear();
            _encoding = null;
            totLen = 0;
        }

        public virtual void ReadFile(Stream ins, FileFormat format, uint stopTag) {
            var Parser = new DcmParser(ins);
            Parser.DcmHandler = DcmHandler;
            Parser.ParseDcmFile(format, stopTag);
        }

        public virtual void ReadDataset(Stream ins, DcmDecodeParam param, uint stopTag) {
            var Parser = new DcmParser(ins);
            Parser.DcmHandler = DcmHandler;
            Parser.ParseDataset(param, stopTag);
        }
    }
}