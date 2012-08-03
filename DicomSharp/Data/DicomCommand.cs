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
using System.Linq;
using System.Text;
using DicomSharp.Dictionary;
using DicomSharp.Utility;

namespace DicomSharp.Data {
    /// <summary>
    /// Defines behavior of <code>Command</code> container objects.
    /// DICOM Part 7: Message Exchange, 6.3.1 Command Set Structure
    /// </summary>
    public class DicomCommand : DcmObject, IDicomCommand {
        private const string SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE = "The SOP Instance Unique Id is not specified";
        private const string SOP_CLASS_UNIQUE_ID_NOT_SPECIFIED_ERROR_MESSAGE = "SOP Class Unique Id not specified";
        private string sopClassUniqueId;
        private string sopInstanceUniqueId;
        private int cmdField = -1;
        private int dataSetType = -1;
        private int msgId = -1;
        private int status = -1;

        #region IDicomCommand Members

        public virtual int CommandField {
            get { return cmdField; }
        }

        public virtual int MessageID {
            get { return msgId; }
        }

        public virtual int MessageIDToBeingRespondedTo {
            get { return msgId; }
        }

        public virtual string AffectedSOPClassUniqueId {
            get { return sopClassUniqueId; }
        }

        public virtual string RequestedSOPClassUniqueId {
            get { return sopClassUniqueId; }
        }

        public virtual string AffectedSOPInstanceUniqueId {
            get { return sopInstanceUniqueId; }
        }

        public virtual string RequestedSOPInstanceUniqueId {
            get { return sopInstanceUniqueId; }
        }

        public virtual int Status {
            get { return status; }
        }

        public bool IsPending() {
            switch (status) {
                case 0xff00:
                case 0xff01:
                    return true;
            }
            return false;
        }

        public bool IsRequest() {
            switch (cmdField) {
                case (int) DicomCommandMessage.C_STORE_RQ:
                case (int) DicomCommandMessage.C_GET_RQ:
                case (int) DicomCommandMessage.C_FIND_RQ:
                case (int) DicomCommandMessage.C_MOVE_RQ:
                case (int) DicomCommandMessage.C_ECHO_RQ:
                case (int) DicomCommandMessage.N_EVENT_REPORT_RQ:
                case (int) DicomCommandMessage.N_GET_RQ:
                case (int) DicomCommandMessage.N_SET_RQ:
                case (int) DicomCommandMessage.N_ACTION_RQ:
                case (int) DicomCommandMessage.N_CREATE_RQ:
                case (int) DicomCommandMessage.N_DELETE_RQ:
                case (int) DicomCommandMessage.C_CANCEL_RQ:
                    return true;
            }
            return false;
        }

        public bool IsResponse() {
            switch (cmdField) {
                case (int) DicomCommandMessage.C_STORE_RSP:
                case (int) DicomCommandMessage.C_GET_RSP:
                case (int) DicomCommandMessage.C_FIND_RSP:
                case (int) DicomCommandMessage.C_MOVE_RSP:
                case (int) DicomCommandMessage.C_ECHO_RSP:
                case (int) DicomCommandMessage.N_EVENT_REPORT_RSP:
                case (int) DicomCommandMessage.N_GET_RSP:
                case (int) DicomCommandMessage.N_SET_RSP:
                case (int) DicomCommandMessage.N_ACTION_RSP:
                case (int) DicomCommandMessage.N_CREATE_RSP:
                case (int) DicomCommandMessage.N_DELETE_RSP:
                    return true;
            }
            return false;
        }

        public bool HasDataset() {
            if (dataSetType == -1) {
                throw new SystemException();
            }

            return dataSetType != (int) DicomCommandMessage.NO_DATASET;
        }

        public DicomCommand InitCStoreRQ(int messageId, string sopClassUniqueId, string sopInstUniqueId, Priority priority) {
            if (string.IsNullOrEmpty(sopInstUniqueId)) {
                throw new ArgumentException(SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            InitateCxxxxRequest(DicomCommandMessage.C_STORE_RQ, messageId, sopClassUniqueId, priority);
            PutUI(Tags.AffectedSOPInstanceUID, sopInstUniqueId);
            return this;
        }

        public DicomCommand InitCStoreRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.C_STORE_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitCFindRSP(int messageId, string sopClassUniqueId, int status) {
            return InitateCxxxxResponse(DicomCommandMessage.C_FIND_RSP, messageId, sopClassUniqueId, status);
        }

        public IDicomCommand InitCCancelRQ(int messageId) {
            PutUS(Tags.CommandField, (int) DicomCommandMessage.C_CANCEL_RQ);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            return this;
        }

        public DicomCommand InitCGetRSP(int messageId, string sopClassUniqueId, int status) {
            return InitateCxxxxResponse(DicomCommandMessage.C_GET_RSP, messageId, sopClassUniqueId, status);
        }

        public DicomCommand InitCMoveRSP(int messageId, string sopClassUniqueId, int status) {
            return InitateCxxxxResponse(DicomCommandMessage.C_MOVE_RSP, messageId, sopClassUniqueId, status);
        }

        public DicomCommand InitCEchoRQ(int messageId, string sopClassUniqueId) {
            if (string.IsNullOrEmpty(sopClassUniqueId)) {
                throw new ArgumentException(SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            PutUS(Tags.CommandField, (int) DicomCommandMessage.C_ECHO_RQ);
            PutUS(Tags.MessageId, messageId);
            return this;
        }

        public IDicomCommand InitCEchoRQ(int messageId) {
            return InitCEchoRQ(messageId, UIDs.Verification);
        }

        public DicomCommand InitCEchoRSP(int messageId, string sopClassUniqueId, int status) {
            return InitateCxxxxResponse(DicomCommandMessage.C_ECHO_RSP, messageId, sopClassUniqueId, status);
        }

        public DicomCommand InitCEchoRSP(int messageId) {
            return InitateCxxxxResponse(DicomCommandMessage.C_ECHO_RSP, messageId, UIDs.Verification, 0);
        }

        public DicomCommand InitNEventReportRQ(int messageId, string sopClassUniqueId, string sopInstanceUniqueId, int eventTypeID) {
            if (string.IsNullOrEmpty(sopClassUniqueId)) {
                throw new ArgumentException(SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            if (string.IsNullOrEmpty(sopInstanceUniqueId)) {
                throw new ArgumentException(SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            PutUS(Tags.CommandField, (int) DicomCommandMessage.N_EVENT_REPORT_RQ);
            PutUS(Tags.MessageId, messageId);
            PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUniqueId);
            PutUS(Tags.EventTypeID, eventTypeID);
            return this;
        }

        public DicomCommand InitNEventReportRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_EVENT_REPORT_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitNGetRQ(int messageId, string sopClassUniqueId, string sopInstUniqueId, int[] attrIDs) {
            InitateNxxxxRequest(DicomCommandMessage.N_GET_RQ, messageId, sopClassUniqueId, sopInstUniqueId);
            if (attrIDs != null) {
                PutAT(Tags.AttributeIdentifierList, attrIDs);
            }
            return this;
        }

        public DicomCommand InitNGetRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_GET_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitNSetRQ(int messageId, string sopClassUniqueId, string sopInstUniqueId) {
            return InitateNxxxxRequest(DicomCommandMessage.N_SET_RQ, messageId, sopClassUniqueId, sopInstUniqueId);
        }

        public DicomCommand InitNSetRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_SET_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitNActionRQ(int messageId, string sopClassUniqueId, string sopInstUniqueId, int actionTypeID) {
            InitateNxxxxRequest(DicomCommandMessage.N_ACTION_RQ, messageId, sopClassUniqueId, sopInstUniqueId);
            PutUS(Tags.ActionTypeID, actionTypeID);
            return this;
        }

        public DicomCommand InitNActionRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_ACTION_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitNCreateRQ(int messageId, string sopClassUniqueId, string sopInstanceUniqueId) {
            if (string.IsNullOrEmpty(sopClassUniqueId)) {
                throw new ArgumentException(SOP_INSTANCE_UNIQUE_ID_IS_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            PutUS(Tags.CommandField, (int) DicomCommandMessage.N_CREATE_RQ);
            PutUS(Tags.MessageId, messageId);
            if (sopInstanceUniqueId != null) {
                PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUniqueId);
            }
            return this;
        }

        public DicomCommand InitNCreateRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_CREATE_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public DicomCommand InitNDeleteRQ(int messageId, string sopClassUniqueId, string sopInstUniqueId) {
            return InitateNxxxxRequest(DicomCommandMessage.N_DELETE_RQ, messageId, sopClassUniqueId, sopInstUniqueId);
        }

        public DicomCommand InitNDeleteRSP(int messageId, string sopClassUniqueId, string sopInstUniqueId, int status) {
            return InitateNxxxxResponse(DicomCommandMessage.N_DELETE_RSP, messageId, sopClassUniqueId, sopInstUniqueId, status);
        }

        public override DcmElement Put(DcmElement newElem) {
            uint tag = newElem.tag();
            if ((tag & 0xFFFF0000) != 0x00000000) {
                throw new ArgumentException(newElem.ToString());
            }

            if (newElem.GetByteBuffer().GetOrder() != ByteOrder.LittleEndian) {
                throw new ArgumentException("The byte order must be LITTLE_ENDIAN: " + newElem.GetByteBuffer());
            }

            try {
                switch (tag) {
                    case Tags.AffectedSOPClassUID:
                    case Tags.RequestedSOPClassUID:
                        sopClassUniqueId = newElem.GetString(null);
                        break;

                    case Tags.CommandField:
                        cmdField = newElem.Int;
                        break;

                    case Tags.MessageId:
                    case Tags.MessageIdBeingRespondedTo:
                        msgId = newElem.Int;
                        break;

                    case Tags.DataSetType:
                        dataSetType = newElem.Int;
                        break;

                    case Tags.Status:
                        status = newElem.Int;
                        break;

                    case Tags.AffectedSOPInstanceUID:
                    case Tags.RequestedSOPInstanceUID:
                        sopInstanceUniqueId = newElem.GetString(null);
                        break;
                }
            } catch (DcmValueException ex) {
                throw new ArgumentException(newElem.ToString(), ex);
            }
            return base.Put(newElem);
        }

        public void Write(IDcmHandler handler) {
            handler.DcmDecodeParam = DcmDecodeParam.IVR_LE;
            Write(0x00000000, GrLen(), handler);
        }

        public void Write(Stream os) {
            Write(new DcmStreamHandler(os));
        }

        public void Read(Stream ins) {
            var parser = new DcmParser(ins);
            parser.DcmHandler = DcmHandler;
            parser.ParseCommand();
        }

        public IDicomCommand InitCFindRQ(int messageId, string sopClassUniqueId, Priority priority) {
            return InitateCxxxxRequest(DicomCommandMessage.C_FIND_RQ, messageId, sopClassUniqueId, priority);
        }

        public DicomCommand InitCGetRQ(int messageId, string sopClassUniqueId, Priority priority) {
            return InitateCxxxxRequest(DicomCommandMessage.C_GET_RQ, messageId, sopClassUniqueId, priority);
        }

        public IDicomCommand InitCMoveRQ(int messageId, string sopClassUniqueId, Priority priority, string moveDestintation) {
            if (string.Empty.Equals(moveDestintation)) {
                throw new ArgumentException();
            }
            InitateCxxxxRequest(DicomCommandMessage.C_MOVE_RQ, messageId, sopClassUniqueId, priority);
            PutAE(Tags.MoveDestination, moveDestintation);
            return this;
        }

        #endregion

        private DicomCommand InitateCxxxxRequest(DicomCommandMessage cmd, int messageId, string sopClassUniqueId, Priority priority) {
            if (string.IsNullOrEmpty(sopClassUniqueId)) {
                throw new ArgumentException(SOP_CLASS_UNIQUE_ID_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            PutUS(Tags.CommandField, (int) cmd);
            PutUS(Tags.MessageId, messageId);
            PutUS(Tags.Priority, (int) priority);
            return this;
        }

        private DicomCommand InitateCxxxxResponse(DicomCommandMessage cmd, int messageId, string sopClassUniqueId, int status) {
            if (sopClassUniqueId != null) {
                PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            }
            PutUS(Tags.CommandField, (int) cmd);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            PutUS(Tags.Status, status);
            return this;
        }

        public DicomCommand SetMoveOriginator(string aet, int messageId) {
            if (string.IsNullOrEmpty(aet)) {
                throw new ArgumentException("AET not specified");
            }
            PutAE(Tags.MoveOriginatorAET, aet);
            PutUS(Tags.MoveOriginatorMessageID, messageId);
            return this;
        }

        private DicomCommand InitateNxxxxRequest(DicomCommandMessage cmd, int messageId, string sopClassUniqueId, string sopInstanceUniqueId) {
            if (string.IsNullOrEmpty(sopClassUniqueId)) {
                throw new ArgumentException(SOP_CLASS_UNIQUE_ID_NOT_SPECIFIED_ERROR_MESSAGE);
            }
            if (string.IsNullOrEmpty(sopInstanceUniqueId)) {
                throw new ArgumentException("SOP Instance Unique Id not specified");
            }
            PutUI(Tags.RequestedSOPClassUID, sopClassUniqueId);
            PutUS(Tags.CommandField, (int) cmd);
            PutUS(Tags.MessageId, messageId);
            PutUI(Tags.RequestedSOPInstanceUID, sopInstanceUniqueId);
            return this;
        }

        private DicomCommand InitateNxxxxResponse(DicomCommandMessage cmd, int messageId, string sopClassUniqueId, string sopInstanceUniqueId, int status) {
            if (sopClassUniqueId != null) {
                PutUI(Tags.AffectedSOPClassUID, sopClassUniqueId);
            }
            PutUS(Tags.CommandField, (int) cmd);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            PutUS(Tags.Status, status);
            if (sopInstanceUniqueId != null) {
                PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUniqueId);
            }
            return this;
        }

        public virtual int Length() {
            return GrLen() + 12;
        }

        private int GrLen() {
            return _dcmElements.Sum(dcmElement => dcmElement.Length() + 8);
        }

        public override string ToString() {
            return ToStringBuffer(new StringBuilder()).ToString();
        }

        private StringBuilder ToStringBuffer(StringBuilder sb) {
            sb.Append(msgId).Append(':').Append(CommandFieldAsString());
            if (dataSetType != (int) DicomCommandMessage.NO_DATASET) {
                sb.Append(" with DataSet");
            }
            if (sopClassUniqueId != null) {
                sb.Append("\n\tclass:\t").Append(UIDs.ToString(sopClassUniqueId));
            }
            if (sopInstanceUniqueId != null) {
                sb.Append("\n\tinstance:\t").Append(sopInstanceUniqueId);
            }
            if (status != -1) {
                sb.Append("\n\tstatus:\t").Append(Convert.ToString(status, 16));
            }
            return sb;
        }

        private string CommandFieldAsString() {
            switch (cmdField) {
                case (int) DicomCommandMessage.C_STORE_RQ:
                    return "C_STORE_RQ";
                case (int) DicomCommandMessage.C_GET_RQ:
                    return "C_GET_RQ";
                case (int) DicomCommandMessage.C_FIND_RQ:
                    return "C_FIND_RQ";
                case (int) DicomCommandMessage.C_MOVE_RQ:
                    return "C_MOVE_RQ";
                case (int) DicomCommandMessage.C_ECHO_RQ:
                    return "C_ECHO_RQ";
                case (int) DicomCommandMessage.N_EVENT_REPORT_RQ:
                    return "N_EVENT_REPORT_RQ";
                case (int) DicomCommandMessage.N_GET_RQ:
                    return "N_GET_RQ";
                case (int) DicomCommandMessage.N_SET_RQ:
                    return "N_SET_RQ";
                case (int) DicomCommandMessage.N_ACTION_RQ:
                    return "N_ACTION_RQ";
                case (int) DicomCommandMessage.N_CREATE_RQ:
                    return "N_CREATE_RQ";
                case (int) DicomCommandMessage.N_DELETE_RQ:
                    return "N_DELETE_RQ";
                case (int) DicomCommandMessage.C_CANCEL_RQ:
                    return "C_CANCEL_RQ";
                case (int) DicomCommandMessage.C_STORE_RSP:
                    return "C_STORE_RSP";
                case (int) DicomCommandMessage.C_GET_RSP:
                    return "C_GET_RSP";
                case (int) DicomCommandMessage.C_FIND_RSP:
                    return "C_FIND_RSP";
                case (int) DicomCommandMessage.C_MOVE_RSP:
                    return "C_MOVE_RSP";
                case (int) DicomCommandMessage.C_ECHO_RSP:
                    return "C_ECHO_RSP";
                case (int) DicomCommandMessage.N_EVENT_REPORT_RSP:
                    return "N_EVENT_REPORT_RSP";
                case (int) DicomCommandMessage.N_GET_RSP:
                    return "N_GET_RSP";
                case (int) DicomCommandMessage.N_SET_RSP:
                    return "N_SET_RSP";
                case (int) DicomCommandMessage.N_ACTION_RSP:
                    return "N_ACTION_RSP";
                case (int) DicomCommandMessage.N_CREATE_RSP:
                    return "N_CREATE_RSP";
                case (int) DicomCommandMessage.N_DELETE_RSP:
                    return "N_DELETE_RSP";
            }
            return "cmd:" + Convert.ToString(cmdField, 16);
        }
    }
}