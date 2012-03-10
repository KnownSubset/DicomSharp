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
using System.Collections;
using System.Net.Sockets;
using System.Reflection;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {

    public enum AssociationState
    {
        IDLE = 1,
        AWAITING_READ_ASS_RQ = 2,
        AWAITING_WRITE_ASS_RP = 3,
        AWAITING_WRITE_ASS_RQ = 4,
        AWAITING_READ_ASS_RP = 5,
        ASSOCIATION_ESTABLISHED = 6,
        AWAITING_READ_REL_RP = 7,
        AWAITING_WRITE_REL_RP = 8,
        RCRS_AWAITING_WRITE_REL_RP = 9,
        RCAS_AWAITING_READ_REL_RP = 10,
        RCRS_AWAITING_READ_REL_RP = 11,
        RCAS_AWAITING_WRITE_REL_RP = 12,
        ASSOCIATION_TERMINATING = 13
    }

    /// <summary>
    /// DICOM Association
    /// </summary>
    public class Association {
        public const int IDLE = 1;
        public const int AWAITING_READ_ASS_RQ = 2;
        public const int AWAITING_WRITE_ASS_RP = 3;
        public const int AWAITING_WRITE_ASS_RQ = 4;
        public const int AWAITING_READ_ASS_RP = 5;
        public const int ASSOCIATION_ESTABLISHED = 6;
        public const int AWAITING_READ_REL_RP = 7;
        public const int AWAITING_WRITE_REL_RP = 8;
        public const int RCRS_AWAITING_WRITE_REL_RP = 9;
        public const int RCAS_AWAITING_READ_REL_RP = 10;
        public const int RCRS_AWAITING_READ_REL_RP = 11;
        public const int RCAS_AWAITING_WRITE_REL_RP = 12;
        public const int ASSOCIATION_TERMINATING = 13;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static int assocCount;
        private readonly byte[] b10 = new byte[10];
        private readonly Fsm fsm;
        private readonly DimseReader reader;
        private readonly DimseWriter writer;
        private ActiveAssociation activeAssociation;
        private int msgID;
        private String name;
        private Hashtable properties;

        /// <summary>
        /// Creates a new instance of Association 
        /// </summary>
        public Association(TcpClient s, bool requestor) {
            name = "Assoc-" + ++assocCount;
            NDC.Push(name);
            try {
                fsm = new Fsm(this, s, requestor);
                reader = new DimseReader(fsm);
                writer = new DimseWriter(fsm);
            }
            finally {
                NDC.Pop();
            }
        }

        public virtual String Name {
            get { return name; }
            set { name = value; }
        }

        public virtual int State {
            get { return fsm.GetState(); }
        }

        public virtual String StateAsString {
            get { return fsm.StateAsString; }
        }

        public virtual int MaxOpsInvoked {
            get { return fsm.MaxOpsInvoked; }
        }

        public virtual int MaxOpsPerformed {
            get { return fsm.MaxOpsPerformed; }
        }

        public virtual AAssociateRQ AAssociateRQ {
            get { return fsm.AAssociateRQ; }
        }

        public virtual AAssociateAC AAssociateAC {
            get { return fsm.AAssociateAC; }
        }

        public virtual AAssociateRJ AAssociateRJ {
            get { return fsm.AAssociateRJ; }
        }

        public virtual AAbort AAbort {
            get { return fsm.AAbort; }
        }

        public virtual String CallingAET {
            get { return fsm.CallingAET; }
        }

        public virtual String CalledAET {
            get { return fsm.CalledAET; }
        }

        public virtual int TCPCloseTimeout {
            get { return fsm.TCPCloseTimeout; }
            set { fsm.TCPCloseTimeout = value; }
        }

        public virtual ActiveAssociation ActiveAssociation {
            get { return activeAssociation; }
            set {
                activeAssociation = value;
                reader.ActiveAssociation = value;
            }
        }

        public override String ToString() {
            return name + "[" + StateAsString + "]";
        }

        public void AddAssociationListener(IAssociationListener l) {
            fsm.AddAssociationListener(l);
        }

        public void RemoveAssociationListener(IAssociationListener l) {
            fsm.RemoveAssociationListener(l);
        }

        public int NextMsgID() {
            lock (this) {
                return ++msgID;
            }
        }

        public void SetThreadPool(LF_ThreadPool pool) {
            fsm.ReaderThreadPool = pool;
            reader.ReaderThreadPool = pool;
        }

        public IPdu Connect(AAssociateRQ rq, int timeout) {
            NDC.Push(name);
            try {
                fsm.Write(rq);
                return fsm.Read(timeout, b10);
            }
            finally {
                NDC.Pop();
            }
        }

        public IPdu Accept(AcceptorPolicy policy, int timeout) {
            NDC.Push(name);
            try {
                IPdu rq = fsm.Read(timeout, b10);
                if (!(rq is AAssociateRQ)) {
                    return rq;
                }

                IPdu rp = policy.Negotiate((AAssociateRQ) rq);
                if (rp is AAssociateAC) {
                    fsm.Write((AAssociateAC) rp);
                }
                else {
                    fsm.Write((AAssociateRJ) rp);
                }
                return rp;
            }
            finally {
                NDC.Pop();
            }
        }

        public Dimse Read(int timeout) {
            NDC.Push(name);
            try {
                Dimse dimse = reader.Read(timeout);
                if (dimse != null) {
                    msgID = Math.Max(dimse.DicomCommand.MessageID, msgID);
                }
                return dimse;
            }
            finally {
                NDC.Pop();
            }
        }

        public void Write(Dimse dimse) {
            NDC.Push(name);
            try {
                msgID = Math.Max(dimse.DicomCommand.MessageID, msgID);
                writer.Write(dimse);
            }
            finally {
                NDC.Pop();
            }
        }

        public IPdu release(int timeout) {
            NDC.Push(name);
            try {
                fsm.Write(AReleaseRQ.Instance);
                return fsm.Read(timeout, b10);
            }
            finally {
                NDC.Pop();
            }
        }

        internal void WriteReleaseRQ() {
            NDC.Push(name);
            try {
                fsm.Write(AReleaseRQ.Instance);
            }
            finally {
                NDC.Pop();
            }
        }

        public void Abort(AAbort aa) {
            NDC.Push(name);
            try {
                fsm.Write(aa);
            }
            finally {
                NDC.Pop();
            }
        }

        public String GetAcceptedTransferSyntaxUID(int pcid) {
            return fsm.GetAcceptedTransferSyntaxUID(pcid);
        }

        public PresContext GetAcceptedPresContext(String asuid, String tsuid) {
            return fsm.GetAcceptedPresContext(asuid, tsuid);
        }

        public ArrayList ListAcceptedPresContext(String asuid) {
            return fsm.ListAcceptedPresContext(asuid);
        }

        public int CountAcceptedPresContext() {
            return fsm.CountAcceptedPresContext();
        }

        public Object GetProperty(Object key) {
            return properties != null ? properties[key] : null;
        }

        public void PutProperty(Object key, Object v) {
            if (properties == null) {
                properties = new Hashtable(2);
            }
            if (v != null) {
                properties.Add(key, v);
            }
            else {
                properties.Remove(key);
            }
        }
    }
}