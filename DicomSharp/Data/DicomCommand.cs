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
using System.Text;
using DicomSharp.Dictionary;
using DicomSharp.Utility;

namespace DicomSharp.Data {
    /// <summary>
    /// Defines behavior of <code>Command</code> container objects.
    /// </summary>
    /// <seealso cref=" "DICOM Part 7: Message Exchange, 6.3.1 Command Set Structure" />

    public class DicomCommand : DcmObject, IDicomCommand
    {
        private int cmdField = -1;
        private int dataSetType = -1;
        private int msgId = -1;
        private String _sopClassUniqueId;
        private String _sopInstUniqueId;
        private int status = -1;

        #region IDicomCommand Members

        public virtual int CommandField
        {
            get { return cmdField; }
        }

        public virtual int MessageID
        {
            get { return msgId; }
        }

        public virtual int MessageIDToBeingRespondedTo
        {
            get { return msgId; }
        }

        public virtual String AffectedSOPClassUniqueId
        {
            get { return _sopClassUniqueId; }
        }

        public virtual String RequestedSOPClassUniqueId
        {
            get { return _sopClassUniqueId; }
        }

        public virtual String AffectedSOPInstanceUniqueId
        {
            get { return _sopInstUniqueId; }
        }

        public virtual String RequestedSOPInstanceUniqueId
        {
            get { return _sopInstUniqueId; }
        }

        public virtual int Status
        {
            get { return status; }
        }

        public bool IsPending()
        {
            switch (status)
            {
                case 0xff00:
                case 0xff01:
                    return true;
            }
            return false;
        }

        public bool IsRequest()
        {
            switch (cmdField)
            {
                case (int)DicomCommandMessage.C_STORE_RQ:
                case (int)DicomCommandMessage.C_GET_RQ:
                case (int)DicomCommandMessage.C_FIND_RQ:
                case (int)DicomCommandMessage.C_MOVE_RQ:
                case (int)DicomCommandMessage.C_ECHO_RQ:
                case (int)DicomCommandMessage.N_EVENT_REPORT_RQ:
                case (int)DicomCommandMessage.N_GET_RQ:
                case (int)DicomCommandMessage.N_SET_RQ:
                case (int)DicomCommandMessage.N_ACTION_RQ:
                case (int)DicomCommandMessage.N_CREATE_RQ:
                case (int)DicomCommandMessage.N_DELETE_RQ:
                case (int)DicomCommandMessage.C_CANCEL_RQ:
                    return true;
            }
            return false;
        }

        public bool IsResponse()
        {
            switch (cmdField)
            {
                case (int)DicomCommandMessage.C_STORE_RSP:
                case (int)DicomCommandMessage.C_GET_RSP:
                case (int)DicomCommandMessage.C_FIND_RSP:
                case (int)DicomCommandMessage.C_MOVE_RSP:
                case (int)DicomCommandMessage.C_ECHO_RSP:
                case (int)DicomCommandMessage.N_EVENT_REPORT_RSP:
                case (int)DicomCommandMessage.N_GET_RSP:
                case (int)DicomCommandMessage.N_SET_RSP:
                case (int)DicomCommandMessage.N_ACTION_RSP:
                case (int)DicomCommandMessage.N_CREATE_RSP:
                case (int)DicomCommandMessage.N_DELETE_RSP:
                    return true;
            }
            return false;
        }

        public bool HasDataset()
        {
            if (dataSetType == -1)
            {
                throw new SystemException();
            }

            return dataSetType != (int)DicomCommandMessage.NO_DATASET;
        }

        public DicomCommand InitCStoreRQ(int messageId, String sopClassUID, String sopInstUID, int priority)
        {
            if (sopInstUID.Length == 0)
            {
                throw new ArgumentException();
            }
            InitCxxxxRQ(DicomCommandMessage.C_STORE_RQ, messageId, sopClassUID, priority);
            PutUI(Tags.AffectedSOPInstanceUID, sopInstUID);
            return this;
        }

        public DicomCommand InitCStoreRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.C_STORE_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public IDicomCommand InitCFindRQ(int messageId, string sopClassUID, int priority)
        {
            return InitCxxxxRQ(DicomCommandMessage.C_FIND_RQ, messageId, sopClassUID, priority);
        }

        public DicomCommand InitCFindRSP(int messageId, String sopClassUID, int status)
        {
            return InitCxxxxRSP(DicomCommandMessage.C_FIND_RSP, messageId, sopClassUID, status);
        }

        public IDicomCommand InitCCancelRQ(int messageId)
        {
            PutUS(Tags.CommandField, (int)DicomCommandMessage.C_CANCEL_RQ);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            return this;
        }

        public DicomCommand InitCGetRQ(int messageId, String sopClassUID, int priority)
        {
            return InitCxxxxRQ(DicomCommandMessage.C_GET_RQ, messageId, sopClassUID, priority);
        }

        public DicomCommand InitCGetRSP(int messageId, String sopClassUID, int status)
        {
            return InitCxxxxRSP(DicomCommandMessage.C_GET_RSP, messageId, sopClassUID, status);
        }

        public IDicomCommand InitCMoveRQ(int messageId, string sopClassUID, int priority, string moveDestintation)
        {
            if (string.Empty.Equals(moveDestintation))
            {
                throw new ArgumentException();
            }
            InitCxxxxRQ(DicomCommandMessage.C_MOVE_RQ, messageId, sopClassUID, priority);
            PutAE(Tags.MoveDestination, moveDestintation);
            return this;
        }

        public DicomCommand InitCMoveRSP(int messageId, String sopClassUID, int status)
        {
            return InitCxxxxRSP(DicomCommandMessage.C_MOVE_RSP, messageId, sopClassUID, status);
        }

        public DicomCommand InitCEchoRQ(int messageId, String sopClassUID)
        {
            if (sopClassUID.Length == 0)
            {
                throw new ArgumentException();
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            PutUS(Tags.CommandField, (int)DicomCommandMessage.C_ECHO_RQ);
            PutUS(Tags.MessageId, messageId);
            return this;
        }

        public IDicomCommand InitCEchoRQ(int messageId)
        {
            return InitCEchoRQ(messageId, UIDs.Verification);
        }

        public DicomCommand InitCEchoRSP(int messageId, String sopClassUID, int status)
        {
            return InitCxxxxRSP(DicomCommandMessage.C_ECHO_RSP, messageId, sopClassUID, status);
        }

        public DicomCommand InitCEchoRSP(int messageId)
        {
            return InitCxxxxRSP(DicomCommandMessage.C_ECHO_RSP, messageId, UIDs.Verification, 0);
        }

        public DicomCommand InitNEventReportRQ(int messageId, String sopClassUID, String sopInstanceUID, int eventTypeID)
        {
            if (sopClassUID.Length == 0)
            {
                throw new ArgumentException();
            }
            if (sopInstanceUID.Length == 0)
            {
                throw new ArgumentException();
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            PutUS(Tags.CommandField, (int)DicomCommandMessage.N_EVENT_REPORT_RQ);
            PutUS(Tags.MessageId, messageId);
            PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
            PutUS(Tags.EventTypeID, eventTypeID);
            return this;
        }

        public DicomCommand InitNEventReportRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_EVENT_REPORT_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public DicomCommand InitNGetRQ(int messageId, String sopClassUID, String sopInstUID, int[] attrIDs)
        {
            InitNxxxxRQ(DicomCommandMessage.N_GET_RQ, messageId, sopClassUID, sopInstUID);
            if (attrIDs != null)
            {
                PutAT(Tags.AttributeIdentifierList, attrIDs);
            }
            return this;
        }

        public DicomCommand InitNGetRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_GET_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public DicomCommand InitNSetRQ(int messageId, String sopClassUID, String sopInstUID)
        {
            return InitNxxxxRQ(DicomCommandMessage.N_SET_RQ, messageId, sopClassUID, sopInstUID);
        }

        public DicomCommand InitNSetRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_SET_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public DicomCommand InitNActionRQ(int messageId, String sopClassUID, String sopInstUID, int actionTypeID)
        {
            InitNxxxxRQ(DicomCommandMessage.N_ACTION_RQ, messageId, sopClassUID, sopInstUID);
            PutUS(Tags.ActionTypeID, actionTypeID);
            return this;
        }

        public DicomCommand InitNActionRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_ACTION_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public DicomCommand InitNCreateRQ(int messageId, String sopClassUID, String sopInstanceUID)
        {
            if (sopClassUID.Length == 0)
            {
                throw new ArgumentException();
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            PutUS(Tags.CommandField, (int)DicomCommandMessage.N_CREATE_RQ);
            PutUS(Tags.MessageId, messageId);
            if (sopInstanceUID != null)
            {
                PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
            }
            return this;
        }

        public DicomCommand InitNCreateRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_CREATE_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public DicomCommand InitNDeleteRQ(int messageId, String sopClassUID, String sopInstUID)
        {
            return InitNxxxxRQ(DicomCommandMessage.N_DELETE_RQ, messageId, sopClassUID, sopInstUID);
        }

        public DicomCommand InitNDeleteRSP(int messageId, String sopClassUID, String sopInstUID, int status)
        {
            return InitNxxxxRSP(DicomCommandMessage.N_DELETE_RSP, messageId, sopClassUID, sopInstUID, status);
        }

        public override DcmElement Put(DcmElement newElem)
        {
            uint tag = newElem.tag();
            if ((tag & 0xFFFF0000) != 0x00000000)
            {
                throw new ArgumentException(newElem.ToString());
            }

            if (newElem.GetByteBuffer().GetOrder() != ByteOrder.LITTLE_ENDIAN)
            {
                throw new ArgumentException("The byte order must be LITTLE_ENDIAN: " + newElem.GetByteBuffer());
            }

            try
            {
                switch (tag)
                {
                    case Tags.AffectedSOPClassUID:
                    case Tags.RequestedSOPClassUID:
                        _sopClassUniqueId = newElem.GetString(null);
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
                        _sopInstUniqueId = newElem.GetString(null);
                        break;
                }
            }
            catch (DcmValueException ex)
            {
                throw new ArgumentException(newElem.ToString(), ex);
            }
            return base.Put(newElem);
        }

        public void Write(IDcmHandler handler)
        {
            handler.DcmDecodeParam = DcmDecodeParam.IVR_LE;
            Write(0x00000000, grLen(), handler);
        }

        public void Write(Stream os)
        {
            Write(new DcmStreamHandler(os));
        }

        public void Read(Stream ins)
        {
            var parser = new DcmParser(ins);
            parser.DcmHandler = DcmHandler;
            parser.ParseCommand();
        }

        #endregion

        private DicomCommand InitCxxxxRQ(DicomCommandMessage cmd, int messageId, String sopClassUID, int priority)
        {
            if (priority != (int)DicomCommandMessage.MEDIUM && priority != (int)DicomCommandMessage.HIGH && priority != (int)DicomCommandMessage.LOW)
            {
                throw new ArgumentException("priority=" + priority);
            }
            if (sopClassUID.Length == 0)
            {
                throw new ArgumentException();
            }
            PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            PutUS(Tags.CommandField, (int)cmd);
            PutUS(Tags.MessageId, messageId);
            PutUS(Tags.Priority, priority);
            return this;
        }

        private DicomCommand InitCxxxxRSP(DicomCommandMessage cmd, int messageId, String sopClassUID, int status)
        {
            if (sopClassUID != null)
            {
                PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            }
            PutUS(Tags.CommandField, (int)cmd);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            PutUS(Tags.Status, status);
            return this;
        }

        public DicomCommand SetMoveOriginator(String aet, int messageId)
        {
            if (aet.Length == 0)
            {
                throw new ArgumentException();
            }
            PutAE(Tags.MoveOriginatorAET, aet);
            PutUS(Tags.MoveOriginatorMessageID, messageId);
            return this;
        }

        private DicomCommand InitNxxxxRQ(DicomCommandMessage cmd, int messageId, String sopClassUID, String sopInstanceUID)
        {
            if (sopClassUID.Length == 0)
            {
                throw new ArgumentException();
            }
            if (sopInstanceUID.Length == 0)
            {
                throw new ArgumentException();
            }
            PutUI(Tags.RequestedSOPClassUID, sopClassUID);
            PutUS(Tags.CommandField, (int)cmd);
            PutUS(Tags.MessageId, messageId);
            PutUI(Tags.RequestedSOPInstanceUID, sopInstanceUID);
            return this;
        }

        private DicomCommand InitNxxxxRSP(DicomCommandMessage cmd, int messageId, String sopClassUID, String sopInstanceUID, int status)
        {
            if (sopClassUID != null)
            {
                PutUI(Tags.AffectedSOPClassUID, sopClassUID);
            }
            PutUS(Tags.CommandField, (int)cmd);
            PutUS(Tags.MessageIdBeingRespondedTo, messageId);
            PutUS(Tags.Status, status);
            if (sopInstanceUID != null)
            {
                PutUI(Tags.AffectedSOPInstanceUID, sopInstanceUID);
            }
            return this;
        }

        public virtual int length()
        {
            return grLen() + 12;
        }

        private int grLen()
        {
            int len = 0;
            for (int i = 0, n = _dcmElements.Count; i < n; ++i)
            {
                len += _dcmElements[i].Length() + 8;
            }

            return len;
        }

        public override String ToString()
        {
            return ToStringBuffer(new StringBuilder()).ToString();
        }

        private StringBuilder ToStringBuffer(StringBuilder sb)
        {
            sb.Append(msgId).Append(':').Append(CommandFieldAsString());
            if (dataSetType != (int)DicomCommandMessage.NO_DATASET)
            {
                sb.Append(" with DataSet");
            }
            if (_sopClassUniqueId != null)
            {
                sb.Append("\n\tclass:\t").Append(UIDs.ToString(_sopClassUniqueId));
            }
            if (_sopInstUniqueId != null)
            {
                sb.Append("\n\tinstance:\t").Append(_sopInstUniqueId);
            }
            if (status != -1)
            {
                sb.Append("\n\tstatus:\t").Append(Convert.ToString(status, 16));
            }
            return sb;
        }

        private String CommandFieldAsString()
        {
            switch (cmdField)
            {
                case (int)DicomCommandMessage.C_STORE_RQ:
                    return "C_STORE_RQ";
                case (int)DicomCommandMessage.C_GET_RQ:
                    return "C_GET_RQ";
                case (int)DicomCommandMessage.C_FIND_RQ:
                    return "C_FIND_RQ";
                case (int)DicomCommandMessage.C_MOVE_RQ:
                    return "C_MOVE_RQ";
                case (int)DicomCommandMessage.C_ECHO_RQ:
                    return "C_ECHO_RQ";
                case (int)DicomCommandMessage.N_EVENT_REPORT_RQ:
                    return "N_EVENT_REPORT_RQ";
                case (int)DicomCommandMessage.N_GET_RQ:
                    return "N_GET_RQ";
                case (int)DicomCommandMessage.N_SET_RQ:
                    return "N_SET_RQ";
                case (int)DicomCommandMessage.N_ACTION_RQ:
                    return "N_ACTION_RQ";
                case (int)DicomCommandMessage.N_CREATE_RQ:
                    return "N_CREATE_RQ";
                case (int)DicomCommandMessage.N_DELETE_RQ:
                    return "N_DELETE_RQ";
                case (int)DicomCommandMessage.C_CANCEL_RQ:
                    return "C_CANCEL_RQ";
                case (int)DicomCommandMessage.C_STORE_RSP:
                    return "C_STORE_RSP";
                case (int)DicomCommandMessage.C_GET_RSP:
                    return "C_GET_RSP";
                case (int)DicomCommandMessage.C_FIND_RSP:
                    return "C_FIND_RSP";
                case (int)DicomCommandMessage.C_MOVE_RSP:
                    return "C_MOVE_RSP";
                case (int)DicomCommandMessage.C_ECHO_RSP:
                    return "C_ECHO_RSP";
                case (int)DicomCommandMessage.N_EVENT_REPORT_RSP:
                    return "N_EVENT_REPORT_RSP";
                case (int)DicomCommandMessage.N_GET_RSP:
                    return "N_GET_RSP";
                case (int)DicomCommandMessage.N_SET_RSP:
                    return "N_SET_RSP";
                case (int)DicomCommandMessage.N_ACTION_RSP:
                    return "N_ACTION_RSP";
                case (int)DicomCommandMessage.N_CREATE_RSP:
                    return "N_CREATE_RSP";
                case (int)DicomCommandMessage.N_DELETE_RSP:
                    return "N_DELETE_RSP";
            }
            return "cmd:" + Convert.ToString(cmdField, 16);
        }
    }
}