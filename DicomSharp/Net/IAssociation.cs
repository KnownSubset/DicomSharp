using System;
using DicomSharp.Utility;

namespace DicomSharp.Net {
    public interface IAssociation {
        AssociationState State { get; }
        int MaxOpsInvoked { get; }
        ActiveAssociation ActiveAssociation { get; set; }
        void AddAssociationListener(IAssociationListener l);
        void SetThreadPool(LF_ThreadPool pool);
        IPdu Connect(AAssociateRQ rq, int timeout);
        Dimse Read(int timeout);
        String GetAcceptedTransferSyntaxUID(int pcid);
        PresentationContext GetAcceptedPresContext(String asuid, String tsuid);
        int NextMsgID();
        int CurrentMessageId();
        void Write(IDimse dimse);
        void WriteReleaseRQ();
    }
}