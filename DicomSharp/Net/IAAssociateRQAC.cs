using System;
using System.Collections;

namespace DicomSharp.Net {
    public interface IAAssociateRQAC {
        int ProtocolVersion { get; set; }
        String CalledAET { get; set; }
        String CallingAET { get; set; }
        String ApplicationContext { get; set; }
        String ClassUID { get; set; }
        String VersionName { get; set; }
        int MaxPduLength { get; set; }
        AsyncOpsWindow AsyncOpsWindow { get; set; }
        int NextPCID();
        void AddPresContext(PresentationContext presentationCtx);
        void RemovePresContext(int pcid);
        PresentationContext GetPresContext(int pcid);
        ICollection ListPresContext();
        void ClearPresContext();
        void RemoveRoleSelection(String uid);
        RoleSelection GetRoleSelection(String uid);
        ICollection ListRoleSelections();
        void ClearRoleSelections();
        void RemoveExtNegotiation(String uid);
        ExtNegotiation GetExtNegotiation(String uid);
        ICollection ListExtNegotiations();
        void ClearExtNegotiations();
        void AddRoleSelection(RoleSelection roleSel);
        void AddExtNegotiation(ExtNegotiation extNeg);
        String ToString();
    }
}