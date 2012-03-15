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
using DicomSharp.Data;
using DicomSharp.Dictionary;

namespace DicomSharp.Net {
    /// <summary>
    /// </summary>
    public class Dimse : IDimse {
        private readonly IDicomCommand _dicomCommand;
        private readonly int _presentationContextId;
        private readonly IDataSource _dataSource;
        private DataSet _dataSet;
        private Stream m_ins;
        private String _transferSyntaxUniqueId;

        public Dimse(int presentationContextId, string transferSyntaxUniqueId, IDicomCommand dicomCommand, Stream ins) {
            _presentationContextId = presentationContextId;
            _dicomCommand = dicomCommand;
            _dataSet = null;
            _dataSource = null;
            m_ins = ins;
            _transferSyntaxUniqueId = transferSyntaxUniqueId;
        }

        public Dimse(int presentationContextId, IDicomCommand dicomCommand, DataSet dataSet, IDataSource dataSource) {
            _presentationContextId = presentationContextId;
            _dicomCommand = dicomCommand;
            _dataSet = dataSet;
            _dataSource = dataSource;
            m_ins = null;
            _transferSyntaxUniqueId = null;
            _dicomCommand.PutUS(Tags.DataSetType, dataSet == null && dataSource == null ? (int)DicomCommandMessage.NO_DATASET : 0);
        }

        public virtual IDicomCommand DicomCommand {
            get { return _dicomCommand; }
        }

        public virtual String TransferSyntaxUniqueId {
            get { return _transferSyntaxUniqueId; }
            set { _transferSyntaxUniqueId = value; }
        }

        public virtual DataSet DataSet {
            get {
                if (_dataSet != null) {
                    return _dataSet;
                }
                if (m_ins == null) {
                    return null;
                }
                if (_transferSyntaxUniqueId == null) {
                    throw new SystemException();
                }
                _dataSet = new DataSet();
                _dataSet.ReadDataset(m_ins, DcmDecodeParam.ValueOf(_transferSyntaxUniqueId), 0);
                m_ins.Close();
                m_ins = null;
                return _dataSet;
            }
        }

        public void ReadDataset()
        {
            DataSet dataSet = DataSet;
            dataSet = null;
        }

        public virtual Stream DataAsStream {
            get { return m_ins; }
        }

        #region DataSourceI Members

        public virtual void WriteTo(Stream outs, String transferSyntaxUniqueId) {
            if (_dataSource != null) {
                _dataSource.WriteTo(outs, transferSyntaxUniqueId);
                return;
            }
            if (_dataSet == null) {
                throw new SystemException("Missing DataSet");
            }
            _dataSet.WriteDataSet(outs, DcmDecodeParam.ValueOf(transferSyntaxUniqueId));
        }

        #endregion

        public int PresentationContextId() {
            return _presentationContextId;
        }


        public override String ToString() {
            return "[pc-" + _presentationContextId + "] " + _dicomCommand;
        }
    }
}