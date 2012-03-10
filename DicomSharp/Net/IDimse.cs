using System;
using System.IO;
using DicomCS.Data;

namespace DicomCS.Net {
    public interface IDimse : IDataSource {
        IDicomCommand DicomCommand { get; }
        String TransferSyntaxUID { get; set; }
        Dataset Dataset { get; }
        Stream DataAsStream { get; }
        int pcid();
    }
}