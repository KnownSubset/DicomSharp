using DicomSharp.Net;

namespace DicomCS.Net {
    public interface IAAssociateAC : IPdu {
        int countAcceptedPresContext();
    }
}