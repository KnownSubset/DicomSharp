using System;
using System.IO;
using DicomSharp.Data;

namespace DicomSharp.Net {
    public interface IDimse : IDataSource {
        IDicomCommand DicomCommand { get; }
        String TransferSyntaxUniqueId { get; set; }
        DataSet DataSet { get; }
        Stream DataAsStream { get; }
        int PresentationContextId();
        void ReadDataset();
    }
}