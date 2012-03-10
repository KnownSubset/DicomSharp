using System;
using System.IO;
using DicomSharp.Data;

namespace DicomSharp.Net {
    public interface IDimse : IDataSource {
        IDicomCommand DicomCommand { get; }
        String TransferSyntaxUniqueId { get; set; }
        Dataset Dataset { get; }
        Stream DataAsStream { get; }
        int pcid();
        void ReadDataset();
    }
}