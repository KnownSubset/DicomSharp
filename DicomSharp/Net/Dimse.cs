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
        private readonly IDicomCommand dicomCommand;
        private readonly int presentationContextId;
        private readonly IDataSource dataSource;
        private DataSet dataSet;
        private Stream stream;
        private String transferSyntaxUniqueId;

        public Dimse(int presentationContextId, string transferSyntaxUniqueId, IDicomCommand dicomCommand, Stream ins) {
            this.presentationContextId = presentationContextId;
            this.dicomCommand = dicomCommand;
            dataSet = null;
            dataSource = null;
            stream = ins;
            this.transferSyntaxUniqueId = transferSyntaxUniqueId;
        }

        public Dimse(int presentationContextId, IDicomCommand dicomCommand, DataSet dataSet, IDataSource dataSource) {
            this.presentationContextId = presentationContextId;
            this.dicomCommand = dicomCommand;
            this.dataSet = dataSet;
            this.dataSource = dataSource;
            stream = null;
            transferSyntaxUniqueId = null;
            this.dicomCommand.PutUS(Tags.DataSetType, dataSet == null && dataSource == null ? (int)DicomCommandMessage.NO_DATASET : 0);
        }

        public virtual IDicomCommand DicomCommand {
            get { return dicomCommand; }
        }

        public virtual String TransferSyntaxUniqueId {
            get { return transferSyntaxUniqueId; }
            set { transferSyntaxUniqueId = value; }
        }

        public virtual DataSet DataSet {
            get {
                if (dataSet != null) {
                    return dataSet;
                }
                if (stream == null) {
                    return null;
                }
                if (transferSyntaxUniqueId == null) {
                    throw new SystemException();
                }
                dataSet = new DataSet();
                dataSet.ReadDataset(stream, DcmDecodeParam.ValueOf(transferSyntaxUniqueId), 0);
                stream.Close();
                stream = null;
                return dataSet;
            }
        }

        public void ReadDataset()
        {
            DataSet dataSet = DataSet;
            dataSet = null;
        }

        public virtual Stream DataAsStream {
            get { return stream; }
        }

        #region IDataSource Members

        public virtual void WriteTo(Stream outs, String transferSyntaxUniqueId) {
            if (dataSource != null) {
                dataSource.WriteTo(outs, transferSyntaxUniqueId);
                return;
            }
            if (dataSet == null) {
                throw new SystemException("Missing DataSet");
            }
            dataSet.WriteDataSet(outs, DcmDecodeParam.ValueOf(transferSyntaxUniqueId));
        }

        #endregion

        public int PresentationContextId() {
            return presentationContextId;
        }


        public override String ToString() {
            return "[pc-" + presentationContextId + "] " + dicomCommand;
        }
    }
}