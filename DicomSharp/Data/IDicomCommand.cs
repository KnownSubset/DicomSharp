using System;
using System.IO;
using DicomSharp.Data;

namespace DicomCS.Data {
    public interface IDicomCommand : IDcmObject {
        int CommandField { get; }
        int MessageID { get; }
        int MessageIDToBeingRespondedTo { get; }
        bool IsPending();
        bool IsRequest();
        bool IsResponse();
        bool HasDataset();
        String AffectedSOPClassUID { get; }
        String RequestedSOPClassUID { get; }
        String AffectedSOPInstanceUID { get; }
        String RequestedSOPInstanceUID { get; }
        int Status { get; }
        DicomCommand InitCStoreRQ(int messageId, String sopClassUID, String sopInstUID, int priority);
        DicomCommand InitCStoreRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        IDicomCommand InitCFindRQ(int messageId, String sopClassUID, int priority);
        DicomCommand InitCFindRSP(int messageId, String sopClassUID, int status);
        IDicomCommand InitCCancelRQ(int messageId);
        DicomCommand InitCGetRQ(int messageId, String sopClassUID, int priority);
        DicomCommand InitCGetRSP(int messageId, String sopClassUID, int status);
        IDicomCommand InitCMoveRQ(int messageId, String sopClassUID, int priority, String moveDestintation);
        DicomCommand InitCMoveRSP(int messageId, String sopClassUID, int status);
        DicomCommand InitCEchoRQ(int messageId, String sopClassUID);
        IDicomCommand InitCEchoRQ(int messageId);
        DicomCommand InitCEchoRSP(int messageId, String sopClassUID, int status);
        DicomCommand InitCEchoRSP(int messageId);
        DicomCommand InitNEventReportRQ(int messageId, String sopClassUID, String sopInstanceUID, int eventTypeID);
        DicomCommand InitNEventReportRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        DicomCommand InitNGetRQ(int messageId, String sopClassUID, String sopInstUID, int[] attrIDs);
        DicomCommand InitNGetRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        DicomCommand InitNSetRQ(int messageId, String sopClassUID, String sopInstUID);
        DicomCommand InitNSetRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        DicomCommand InitNActionRQ(int messageId, String sopClassUID, String sopInstUID, int actionTypeID);
        DicomCommand InitNActionRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        DicomCommand InitNCreateRQ(int messageId, String sopClassUID, String sopInstanceUID);
        DicomCommand InitNCreateRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        DicomCommand InitNDeleteRQ(int messageId, String sopClassUID, String sopInstUID);
        DicomCommand InitNDeleteRSP(int messageId, String sopClassUID, String sopInstUID, int status);
        void Write(IDcmHandler handler);
        void Write(Stream os);
        void Read(Stream ins);
    }
}